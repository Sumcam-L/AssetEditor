using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Models;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

public class TreeViewModel : AdapterViewModel
{
	private AutoExpandMode m_autoExpand = AutoExpandMode.Default;

	private bool m_multiSelectEnabled = true;

	private bool m_showRoot = true;

	private bool m_synchronisingSelection;

	private static readonly PropertyChangedEventArgs s_synchronisingSelectionArgs = ObservableUtil.CreateArgs((TreeViewModel x) => x.SynchronisingSelection);

	private Path<Node> m_ensureVisiblePath;

	private static readonly PropertyChangedEventArgs s_ensureVisiblePathArgs = ObservableUtil.CreateArgs((TreeViewModel x) => x.EnsureVisiblePath);

	private ITreeView m_treeView;

	private IItemView m_itemView;

	private ILabelEditingContext m_labelEditingContext;

	private IValidationContext m_validationContext;

	private IObservableContext m_observableContext;

	private ISelectionContext m_selectionContext;

	private bool m_synchronizingSelection;

	private HashSet<Node> m_selectionChangedNodes;

	private HashSet<object> m_parentsWithRemovedChildren;

	private HashSet<object> m_parentsWithAddedChildren;

	private HashSet<object> m_itemsToReload;

	private Path<object>[] m_previousSelection;

	private readonly Multimap<object, Node> m_itemToNodesMap = new Multimap<object, Node>(null);

	public ITreeView TreeView
	{
		get
		{
			return m_treeView;
		}
		set
		{
			if (m_treeView != value)
			{
				base.Adaptee = value;
				if (m_treeView != null)
				{
					m_itemView = null;
					if (m_observableContext != null)
					{
						m_observableContext.ItemInserted -= tree_ItemInserted;
						m_observableContext.ItemRemoved -= tree_ItemRemoved;
						m_observableContext.ItemChanged -= tree_ItemChanged;
						m_observableContext.Reloaded -= tree_Reloaded;
						m_observableContext = null;
					}
					if (m_validationContext != null)
					{
						m_validationContext.Beginning -= validationContext_Beginning;
						m_validationContext.Ended -= validationContext_Ended;
						m_validationContext.Cancelled -= validationContext_Cancelled;
						m_validationContext = null;
					}
					if (m_selectionContext != null)
					{
						m_selectionContext.SelectionChanging -= selection_Changing;
						m_selectionContext.SelectionChanged -= selection_Changed;
						m_selectionContext = null;
					}
					if (m_labelEditingContext != null)
					{
						m_labelEditingContext.BeginLabelEdit -= labelEditingContext_BeginLabelEdit;
					}
				}
				m_treeView = value;
				if (m_treeView != null)
				{
					m_itemView = m_treeView.As<IItemView>();
					m_observableContext = m_treeView.As<IObservableContext>();
					if (m_observableContext != null)
					{
						m_observableContext.ItemInserted += tree_ItemInserted;
						m_observableContext.ItemRemoved += tree_ItemRemoved;
						m_observableContext.ItemChanged += tree_ItemChanged;
						m_observableContext.Reloaded += tree_Reloaded;
					}
					m_validationContext = m_treeView.As<IValidationContext>();
					if (m_validationContext != null)
					{
						m_validationContext.Beginning += validationContext_Beginning;
						m_validationContext.Ended += validationContext_Ended;
						m_validationContext.Cancelled += validationContext_Cancelled;
					}
					m_selectionContext = m_treeView.As<ISelectionContext>();
					if (m_selectionContext != null)
					{
						m_selectionContext.SelectionChanging += selection_Changing;
						m_selectionContext.SelectionChanged += selection_Changed;
					}
					m_labelEditingContext = m_treeView.As<ILabelEditingContext>();
					if (m_labelEditingContext != null)
					{
						m_labelEditingContext.BeginLabelEdit += labelEditingContext_BeginLabelEdit;
					}
				}
			}
			Load();
		}
	}

	public AutoExpandMode AutoExpand
	{
		get
		{
			return m_autoExpand;
		}
		set
		{
			m_autoExpand = value;
		}
	}

	public bool MultiSelectEnabled
	{
		get
		{
			return m_multiSelectEnabled;
		}
		set
		{
			m_multiSelectEnabled = value;
		}
	}

	public bool ShowRoot
	{
		get
		{
			return m_showRoot;
		}
		set
		{
			if (m_showRoot != value)
			{
				if (m_selectionContext != null)
				{
					m_selectionContext.Clear();
				}
				m_showRoot = value;
				OnPropertyChanged(new PropertyChangedEventArgs("Roots"));
			}
		}
	}

	public bool SynchronisingSelection
	{
		get
		{
			return m_synchronisingSelection;
		}
		set
		{
			if (m_synchronisingSelection != value)
			{
				m_synchronisingSelection = value;
				OnPropertyChanged(s_synchronisingSelectionArgs);
				this.SynchronisingSelectionChanged.Raise(this, EventArgs.Empty);
			}
		}
	}

	public IEnumerable<Node> Roots
	{
		get
		{
			if (Root == null)
			{
				return EmptyEnumerable<Node>.Instance;
			}
			if (ShowRoot)
			{
				return new Node[1] { Root };
			}
			return Root.Children;
		}
	}

	public Node Root { get; private set; }

	public Path<Node> EnsureVisiblePath
	{
		get
		{
			return m_ensureVisiblePath;
		}
		private set
		{
			if (m_ensureVisiblePath != value)
			{
				m_ensureVisiblePath = value;
				if (m_ensureVisiblePath != null && !m_showRoot && m_ensureVisiblePath.First == Root && m_ensureVisiblePath.Count > 1)
				{
					m_ensureVisiblePath = m_ensureVisiblePath.Suffix(m_ensureVisiblePath.Count - 1);
				}
				OnPropertyChanged(s_ensureVisiblePathArgs);
			}
		}
	}

	public bool InSelectionTransaction
	{
		get
		{
			return m_selectionChangedNodes != null;
		}
		set
		{
			if (!value && m_selectionChangedNodes != null)
			{
				RefreshSelection();
				m_selectionChangedNodes = null;
			}
			else if (value && m_selectionChangedNodes == null)
			{
				m_selectionChangedNodes = new HashSet<Node>();
			}
		}
	}

	protected ISelectionContext SelectionContext => m_selectionContext;

	internal event EventHandler SynchronisingSelectionChanged;

	public TreeViewModel()
	{
	}

	public TreeViewModel(object adaptee)
		: base(adaptee)
	{
		TreeView = adaptee as ITreeView;
	}

	public void Refresh(object item)
	{
		foreach (Node item2 in m_itemToNodesMap[item])
		{
			RefreshNode(item2);
		}
	}

	public void RefreshParents(object item)
	{
		foreach (Node item2 in m_itemToNodesMap[item])
		{
			if (item2.Parent != null)
			{
				RefreshNode(item2.Parent);
			}
		}
	}

	public Node ExpandToFirstLeaf()
	{
		Node node = Root;
		if (node == null)
		{
			return null;
		}
		Node node2;
		do
		{
			node2 = node;
			node2.Expanded = true;
			using IEnumerator<Node> enumerator = node.Children.GetEnumerator();
			if (enumerator.MoveNext())
			{
				Node current = enumerator.Current;
				node = current;
			}
		}
		while (node2 != node);
		return node;
	}

	public Node Show(Path<object> path, bool select)
	{
		Node node = null;
		Path<Node> path2 = ExpandPath(path);
		if (path2 != null)
		{
			node = path2.Last;
			if (select)
			{
				node.IsSelected = true;
			}
			EnsureVisiblePath = path2;
		}
		return node;
	}

	public Node Show(object item, bool select)
	{
		Node node = GetNode(item);
		if (node != null)
		{
			Path<object> path = MakePath(node);
			Show(path, select);
		}
		return node;
	}

	public void ExpandAll()
	{
		foreach (Node item in GetAllNodesInTree())
		{
			item.Expanded = true;
		}
	}

	public void CollapseAll()
	{
		try
		{
			m_synchronizingSelection = true;
			if (Root == null)
			{
				return;
			}
			foreach (Node item in GetAllNodesInTree())
			{
				item.Expanded = false;
			}
		}
		finally
		{
			m_synchronizingSelection = false;
		}
	}

	public IEnumerable<object> GetExpandedItems()
	{
		if (m_treeView == null)
		{
			yield break;
		}
		Stack<object> items = new Stack<object>();
		items.Push(m_treeView.Root);
		while (items.Count > 0)
		{
			object item = items.Pop();
			foreach (Node node in m_itemToNodesMap[item])
			{
				if (!node.Expanded)
				{
					continue;
				}
				yield return item;
				foreach (object child in m_treeView.GetChildren(item))
				{
					items.Push(child);
				}
			}
		}
	}

	public void Expand(object item)
	{
		foreach (Node item2 in m_itemToNodesMap[item])
		{
			item2.Expanded = true;
		}
	}

	public void Expand(IEnumerable<object> items)
	{
		foreach (object item in items)
		{
			Expand(item);
		}
	}

	protected override void OnAdapteeChanged(object oldAdaptee)
	{
		base.OnAdapteeChanged(oldAdaptee);
		TreeView = base.Adaptee as ITreeView;
	}

	private void RefreshNode(Node node)
	{
		if (node.ChildrenInternal != null)
		{
			List<object> path = new List<object>();
			HashSet<Path<object>> paths = new HashSet<Path<object>>();
			foreach (Node item in node.ChildrenInternal)
			{
				AddPaths(item, path, paths);
			}
			RemoveChildren(node);
			foreach (object child in TreeView.GetChildren(node.Adaptee))
			{
				InsertObject(child, node.Adaptee, -1);
			}
			foreach (Node item2 in node.ChildrenInternal)
			{
				ExpandPaths(item2, path, paths);
			}
		}
		UpdateNodeSubTree(node);
	}

	private void AddPaths(Node node, List<object> path, HashSet<Path<object>> paths)
	{
		if (!node.Expanded)
		{
			return;
		}
		path.Add(node);
		paths.Add(new AdaptablePath<object>(path));
		foreach (Node child in node.Children)
		{
			AddPaths(child, path, paths);
		}
		path.RemoveAt(path.Count - 1);
	}

	private void ExpandPaths(Node node, List<object> path, HashSet<Path<object>> paths)
	{
		path.Add(node);
		if (paths.Contains(new AdaptablePath<object>(path)))
		{
			node.Expanded = true;
			foreach (Node item in node.ChildrenInternal)
			{
				ExpandPaths(item, path, paths);
			}
		}
		path.RemoveAt(path.Count - 1);
	}

	internal ObservableCollection<Node> CreateChildren(Node treeNodeViewModel)
	{
		ObservableCollection<Node> observableCollection = new ObservableCollection<Node>();
		foreach (object child in TreeView.GetChildren(treeNodeViewModel.Adaptee))
		{
			Node node = CreateNode(child, treeNodeViewModel);
			if (node != null)
			{
				observableCollection.Add(node);
			}
		}
		return observableCollection;
	}

	private Node CreateNode(object adaptee, Node parent)
	{
		Node node = m_itemToNodesMap[adaptee].FirstOrDefault((Node n) => n.Parent == parent);
		if (node != null)
		{
			return node;
		}
		node = new Node(adaptee, this, parent);
		node.IsSelectedChanged += node_IsSelectedChanged;
		m_itemToNodesMap.Add(adaptee, node);
		UpdateNode(node);
		return node;
	}

	private void Load()
	{
		Unload();
		if (m_treeView != null)
		{
			object root = m_treeView.Root;
			if (root != null)
			{
				Root = CreateNode(root, null);
				Root.Expanded = true;
				UpdateNode(Root);
			}
			else
			{
				Root = null;
			}
			OnPropertyChanged(new PropertyChangedEventArgs("Root"));
			OnPropertyChanged(new PropertyChangedEventArgs("Roots"));
		}
	}

	private void Unload()
	{
		EnsureVisiblePath = null;
		Root = null;
		m_itemToNodesMap.Clear();
		m_previousSelection = new Path<object>[0];
		OnPropertyChanged(new PropertyChangedEventArgs("Root"));
		OnPropertyChanged(new PropertyChangedEventArgs("Roots"));
		OnPropertyChanged(new PropertyChangedEventArgs("EnsureVisiblePath"));
	}

	private void InsertObject(object child, object parent, int index)
	{
		foreach (Node item in m_itemToNodesMap[parent])
		{
			if (item.ChildrenInternal != null)
			{
				Node node = CreateNode(child, item);
				if (node != null)
				{
					if (index >= 0)
					{
						item.ChildrenInternal.Insert(index, node);
					}
					else
					{
						item.ChildrenInternal.Add(node);
					}
				}
			}
			if (!item.Expanded)
			{
				if ((int)(m_autoExpand & AutoExpandMode.ExpandInsertedIfParentSelected) > 0 && item.IsSelected)
				{
					item.Expanded = true;
				}
				else if ((int)(m_autoExpand & AutoExpandMode.ExpandInserted) > 0)
				{
					item.Expanded = true;
				}
			}
		}
	}

	private void RemoveObject(object tree)
	{
		foreach (Node item in m_itemToNodesMap[tree])
		{
			HashSet<Path<object>> hashSet = new HashSet<Path<object>>();
			foreach (Node item2 in GetSubtree(item))
			{
				m_itemToNodesMap.Remove(item2);
				hashSet.Add(MakePath(item2));
			}
			item.Parent.ChildrenInternal.Remove(item);
			try
			{
				m_synchronizingSelection = true;
				if (m_selectionContext != null)
				{
					m_selectionContext.RemoveRange(hashSet);
				}
			}
			finally
			{
				m_synchronizingSelection = false;
			}
		}
	}

	private void RemoveChildren(Node node)
	{
		HashSet<Path<object>> hashSet = new HashSet<Path<object>>();
		foreach (Node item in node.ChildrenInternal)
		{
			foreach (Node item2 in GetSubtree(item))
			{
				m_itemToNodesMap.Remove(item2);
				hashSet.Add(MakePath(item));
			}
		}
		node.ChildrenInternal.Clear();
		try
		{
			m_synchronizingSelection = true;
			if (m_selectionContext != null)
			{
				m_selectionContext.RemoveRange(hashSet);
			}
		}
		finally
		{
			m_synchronizingSelection = false;
		}
	}

	private void UpdateNode(Node node)
	{
		if (m_itemView != null)
		{
			m_itemView.GetInfo(node.Adaptee, node.ItemInfo);
			OnNodeInfoUpdated(node);
			node.ItemInfoChanged();
		}
		if (m_selectionContext != null && !m_synchronizingSelection)
		{
			try
			{
				m_synchronizingSelection = true;
				node.IsSelected = m_selectionContext.SelectionContains(MakePath(node));
			}
			finally
			{
				m_synchronizingSelection = false;
			}
		}
	}

	private void UpdateNodeSubTree(Node node)
	{
		UpdateNode(node);
		if (!node.Expanded)
		{
			return;
		}
		foreach (Node child in node.Children)
		{
			UpdateNodeSubTree(child);
		}
	}

	protected virtual void OnNodeInfoUpdated(Node node)
	{
	}

	private void UpdateChangedParents()
	{
		if (m_parentsWithRemovedChildren == null)
		{
			return;
		}
		foreach (object parentsWithRemovedChild in m_parentsWithRemovedChildren)
		{
			foreach (Node item in m_itemToNodesMap[parentsWithRemovedChild])
			{
				RefreshNode(item);
			}
		}
		foreach (object parentsWithAddedChild in m_parentsWithAddedChildren)
		{
			foreach (Node item2 in m_itemToNodesMap[parentsWithAddedChild])
			{
				if (!item2.Expanded)
				{
					if ((int)(m_autoExpand & AutoExpandMode.ExpandInsertedIfParentSelected) > 0 && item2.IsSelected)
					{
						item2.Expanded = true;
					}
					else if ((int)(m_autoExpand & AutoExpandMode.ExpandInserted) > 0)
					{
						item2.Expanded = true;
					}
				}
				RefreshNode(item2);
			}
		}
		foreach (object item3 in m_itemsToReload)
		{
			foreach (Node item4 in m_itemToNodesMap[item3])
			{
				RefreshNode(item4);
			}
		}
		m_parentsWithRemovedChildren = null;
		m_parentsWithAddedChildren = null;
		m_itemsToReload = null;
	}

	private IEnumerable<Node> GetSubtree(Node node)
	{
		if (node.ChildrenInternal == null || node.ChildrenInternal.Count == 0)
		{
			yield return node;
			yield break;
		}
		Queue<Node> nodes = new Queue<Node>();
		nodes.Enqueue(node);
		while (nodes.Count > 0)
		{
			Node item = nodes.Dequeue();
			yield return item;
			if (item.ChildrenInternal == null)
			{
				continue;
			}
			foreach (Node child in item.ChildrenInternal)
			{
				nodes.Enqueue(child);
			}
		}
	}

	private Path<Node> ExpandPath(Path<object> path)
	{
		Path<Node> result = null;
		if (path[0].Equals(Root.Adaptee))
		{
			Node node = Root;
			result = new Path<Node>(node);
			if (path.Count > 0)
			{
				node.Expanded = true;
			}
			for (int i = 1; i < path.Count; i++)
			{
				Node node2 = null;
				foreach (Node child in node.Children)
				{
					if (path[i].Equals(child.Adaptee))
					{
						node2 = child;
						if (i != path.Count - 1)
						{
							child.Expanded = true;
						}
						break;
					}
				}
				if (node2 == null)
				{
					return null;
				}
				node = node2;
				result += node;
			}
		}
		return result;
	}

	private IEnumerable<Node> GetNodes(object item)
	{
		if (!m_itemToNodesMap[item].Any())
		{
			GetAllNodesInTree();
		}
		return m_itemToNodesMap[item];
	}

	private Node GetNode(object item)
	{
		return GetNodes(item).FirstOrDefault();
	}

	private IEnumerable<Node> GetAllNodesInTree()
	{
		if (Root == null)
		{
			yield break;
		}
		Stack<Node> nodes = new Stack<Node>();
		nodes.Push(Root);
		while (nodes.Count > 0)
		{
			Node node = nodes.Pop();
			yield return node;
			foreach (Node child in node.Children)
			{
				nodes.Push(child);
			}
		}
	}

	private static Path<object> MakePath(Node node)
	{
		return new AdaptablePath<object>(from x in GetLineage(node).Reverse()
			select x.Adaptee);
	}

	private static IEnumerable<Node> GetLineage(Node node)
	{
		while (node != null)
		{
			yield return node;
			node = node.Parent;
		}
	}

	private static void Swap(ref Node node1, ref Node node2)
	{
		Node node3 = node1;
		node1 = node2;
		node2 = node3;
	}

	private static Node GetRoot(Node node)
	{
		while (node.Parent != null)
		{
			node = node.Parent;
		}
		return node;
	}

	private void tree_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		if (e.Parent == null)
		{
			return;
		}
		if (m_parentsWithAddedChildren != null)
		{
			foreach (Node item in m_itemToNodesMap[e.Parent])
			{
				if (!item.Expanded)
				{
					if ((int)(m_autoExpand & AutoExpandMode.ExpandInsertedIfParentSelected) > 0 && item.IsSelected)
					{
						item.Expanded = true;
					}
					else if ((int)(m_autoExpand & AutoExpandMode.ExpandInserted) > 0)
					{
						item.Expanded = true;
					}
				}
				m_parentsWithAddedChildren.Add(e.Parent);
			}
			return;
		}
		InsertObject(e.Item, e.Parent, e.Index);
	}

	private void tree_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		if (m_parentsWithRemovedChildren != null && e.Parent != null)
		{
			m_parentsWithRemovedChildren.Add(e.Parent);
		}
		else
		{
			RemoveObject(e.Item);
		}
	}

	private void tree_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		if (e.Reloaded)
		{
			if (m_itemsToReload != null)
			{
				m_itemsToReload.Add(e.Item);
				return;
			}
			{
				foreach (Node item in m_itemToNodesMap[e.Item])
				{
					RefreshNode(item);
				}
				return;
			}
		}
		foreach (Node item2 in m_itemToNodesMap[e.Item])
		{
			UpdateNode(item2);
		}
	}

	private void tree_Reloaded(object sender, EventArgs e)
	{
		Load();
	}

	private void validationContext_Beginning(object sender, EventArgs e)
	{
		m_parentsWithRemovedChildren = new HashSet<object>();
		m_parentsWithAddedChildren = new HashSet<object>();
		m_itemsToReload = new HashSet<object>();
	}

	private void validationContext_Ended(object sender, EventArgs e)
	{
		UpdateChangedParents();
	}

	private void validationContext_Cancelled(object sender, EventArgs e)
	{
		UpdateChangedParents();
	}

	private void selection_Changing(object sender, EventArgs e)
	{
		m_previousSelection = m_selectionContext.GetSelection<Path<object>>().ToArray();
	}

	private void selection_Changed(object sender, EventArgs e)
	{
		if (!m_synchronizingSelection)
		{
			try
			{
				m_synchronizingSelection = true;
				DeselectPreviousSelection();
				SelectCurrentSelection();
			}
			finally
			{
				m_synchronizingSelection = false;
			}
		}
	}

	private void labelEditingContext_BeginLabelEdit(object sender, BeginLabelEditEventArgs e)
	{
		object obj = e.Item;
		Path<object> path = obj as Path<object>;
		if (path != null)
		{
			obj = path.Last;
		}
		foreach (Node item in m_itemToNodesMap[obj])
		{
			item.IsInLabelEditMode = true;
		}
	}

	private void node_IsSelectedChanged(object sender, EventArgs e)
	{
		if (m_selectionContext == null || m_synchronizingSelection)
		{
			return;
		}
		if (m_selectionChangedNodes != null)
		{
			m_selectionChangedNodes.Add((Node)sender);
			return;
		}
		try
		{
			m_synchronizingSelection = true;
			Node node = (Node)sender;
			Path<object> item = MakePath(node);
			if (node.IsSelected)
			{
				if (!m_multiSelectEnabled)
				{
					DeselectAll();
				}
				m_selectionContext.Add(item);
			}
			else
			{
				m_selectionContext.Remove(item);
			}
		}
		finally
		{
			m_synchronizingSelection = false;
		}
	}

	private void RefreshSelection()
	{
		if (m_selectionContext == null || m_selectionChangedNodes == null || m_selectionChangedNodes.Count <= 0)
		{
			return;
		}
		try
		{
			m_synchronizingSelection = true;
			if (!m_multiSelectEnabled)
			{
				Node node = m_selectionChangedNodes.LastOrDefault((Node x) => x.IsSelected);
				foreach (Node selectionChangedNode in m_selectionChangedNodes)
				{
					if (selectionChangedNode != node)
					{
						Path<object> item = MakePath(selectionChangedNode);
						m_selectionContext.Remove(item);
					}
				}
				Path<object> item2 = MakePath(node);
				m_selectionContext.Add(item2);
				return;
			}
			List<Path<object>> list = new List<Path<object>>();
			List<Path<object>> list2 = new List<Path<object>>();
			foreach (Node selectionChangedNode2 in m_selectionChangedNodes)
			{
				if (selectionChangedNode2.IsSelected)
				{
					list.Add(MakePath(selectionChangedNode2));
				}
				else
				{
					list2.Add(MakePath(selectionChangedNode2));
				}
			}
			m_selectionContext.RemoveRange(list2);
			m_selectionContext.AddRange(list);
		}
		finally
		{
			m_synchronizingSelection = false;
		}
	}

	private void DeselectAll()
	{
		if (m_selectionContext == null)
		{
			return;
		}
		List<Path<object>> list = new List<Path<object>>();
		Path<object>[] array = m_selectionContext.GetSelection<Path<object>>().ToArray();
		foreach (Path<object> path in array)
		{
			foreach (Node item in m_itemToNodesMap[path.Last])
			{
				list.Add(path);
				item.IsSelected = false;
			}
		}
		m_selectionContext.RemoveRange(list);
	}

	private void DeselectPreviousSelection()
	{
		Path<object>[] previousSelection = m_previousSelection;
		foreach (Path<object> path in previousSelection)
		{
			foreach (Node item in m_itemToNodesMap[path.Last])
			{
				item.IsSelected = false;
			}
		}
	}

	private void SelectCurrentSelection()
	{
		if (m_selectionContext == null || m_selectionContext.SelectionCount == 0)
		{
			return;
		}
		Path<Node> path = null;
		Path<object>[] array = m_selectionContext.GetSelection<Path<object>>().ToArray();
		if (array.Length == 0)
		{
			HashSet<Node> hashSet = new HashSet<Node>();
			object[] array2 = m_selectionContext.GetSelection<object>().ToArray();
			object[] array3 = array2;
			foreach (object obj in array3)
			{
				foreach (Node item in m_itemToNodesMap[obj])
				{
					hashSet.Add(item);
					m_selectionContext.Remove(obj);
				}
			}
			if (hashSet.Count <= 0)
			{
				return;
			}
			try
			{
				InSelectionTransaction = true;
				foreach (Node item2 in hashSet)
				{
					item2.IsSelected = true;
					m_selectionChangedNodes.Add(item2);
				}
				return;
			}
			finally
			{
				InSelectionTransaction = false;
			}
		}
		Path<object>[] array4 = array;
		foreach (Path<object> path2 in array4)
		{
			List<Node> list = new List<Node>();
			if ((int)(m_autoExpand & AutoExpandMode.ExpandSelected) > 0)
			{
				path = ExpandPath(path2);
				if (path != null)
				{
					list.Add(path.Last);
				}
			}
			else
			{
				list.AddRange(m_itemToNodesMap[path2.Last]);
			}
			foreach (Node item3 in list)
			{
				item3.IsSelected = true;
			}
			if (!m_multiSelectEnabled)
			{
				break;
			}
		}
		if (path != null && (int)(m_autoExpand & AutoExpandMode.ExpandSelected) > 0)
		{
			EnsureVisiblePath = path;
		}
	}
}

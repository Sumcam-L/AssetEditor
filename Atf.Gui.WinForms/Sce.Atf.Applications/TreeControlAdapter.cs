using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications;

public class TreeControlAdapter
{
	private HashSet<object> m_changedParents;

	private readonly TreeControl m_treeControl;

	private ITreeView m_treeView;

	private IItemView m_itemView;

	private IValidationContext m_validationContext;

	private IObservableContext m_observableContext;

	private ISelectionContext m_selectionContext;

	private readonly Multimap<object, TreeControl.Node> m_itemToNodeMap;

	private object m_lastHit;

	private bool m_autoExpand = true;

	private bool m_synchronizingSelection;

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
						m_selectionContext.SelectionChanged -= selection_Changed;
						m_selectionContext = null;
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
						m_selectionContext.SelectionChanged += selection_Changed;
					}
				}
			}
			Load();
		}
	}

	public TreeControl TreeControl => m_treeControl;

	public object LastHit => m_lastHit;

	public bool AutoExpand
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

	public event EventHandler LastHitChanged;

	public TreeControlAdapter()
		: this(new TreeControl(), null)
	{
	}

	public TreeControlAdapter(TreeControl treeControl)
		: this(treeControl, null)
	{
	}

	public TreeControlAdapter(TreeControl treeControl, IEqualityComparer<object> comparer)
	{
		m_treeControl = treeControl;
		m_itemToNodeMap = new Multimap<object, TreeControl.Node>(comparer);
		m_treeControl.MouseDown += treeControl_MouseDown;
		m_treeControl.MouseUp += treeControl_MouseUp;
		m_treeControl.DragOver += treeControl_DragOver;
		m_treeControl.DragDrop += treeControl_DragDrop;
		m_treeControl.NodeExpandedChanged += treeControl_NodeExpandedChanged;
		m_treeControl.NodeSelectedChanged += treeControl_NodeSelectedChanged;
		m_treeControl.SelectionChanging += treeControl_SelectionChanging;
		m_treeControl.SelectionChanged += treeControl_SelectionChanged;
	}

	protected virtual void OnLastHitChanged(EventArgs e)
	{
		this.LastHitChanged.Raise(this, e);
	}

	public void Expand(object item)
	{
		IEnumerable<TreeControl.Node> enumerable = m_itemToNodeMap[item];
		foreach (TreeControl.Node item2 in enumerable)
		{
			item2.Expanded = true;
		}
	}

	public void Collapse(object item)
	{
		IEnumerable<TreeControl.Node> enumerable = m_itemToNodeMap[item];
		foreach (TreeControl.Node item2 in enumerable)
		{
			item2.Expanded = false;
		}
	}

	public IEnumerable<Path<object>> GetPaths(object item)
	{
		foreach (TreeControl.Node node in m_itemToNodeMap[item])
		{
			yield return MakePath(node);
		}
	}

	public TreeControl.Node ExpandPath(Path<object> path)
	{
		return ExpandPath(path, suppressAutoExpand: false);
	}

	public object GetItemAt(Point clientPoint)
	{
		return m_treeControl.GetNodeAt(clientPoint)?.Tag;
	}

	public Path<object> GetPathAt(Point clientPoint)
	{
		TreeControl.Node node = m_treeControl.GetNodeAt(clientPoint);
		if (node != null)
		{
			List<object> list = new List<object>();
			while (node != null)
			{
				list.Add(node.Tag);
				node = node.Parent;
			}
			list.Reverse();
			return new AdaptablePath<object>(list);
		}
		return null;
	}

	public object[] GetSelectedItems()
	{
		List<object> list = new List<object>();
		foreach (TreeControl.Node selectedNode in m_treeControl.SelectedNodes)
		{
			list.Add(selectedNode.Tag);
		}
		return list.ToArray();
	}

	public Path<object>[] GetSelectedPaths()
	{
		List<Path<object>> list = new List<Path<object>>();
		foreach (TreeControl.Node selectedNode in m_treeControl.SelectedNodes)
		{
			list.Add(MakePath(selectedNode));
		}
		return list.ToArray();
	}

	public void Refresh(object item)
	{
		IEnumerable<TreeControl.Node> source = m_itemToNodeMap[item];
		TreeControl.Node[] array = source.ToArray();
		foreach (TreeControl.Node node in array)
		{
			RefreshNode(node);
		}
	}

	public void RefreshParents(object item)
	{
		IEnumerable<TreeControl.Node> enumerable = m_itemToNodeMap[item];
		foreach (TreeControl.Node item2 in enumerable)
		{
			if (item2.Parent != null)
			{
				RefreshNode(item2.Parent);
			}
		}
	}

	private void RefreshNode(TreeControl.Node node)
	{
		UpdateNode(node);
		if (!node.Expanded)
		{
			return;
		}
		List<object> path = new List<object>();
		HashSet<Path<object>> paths = new HashSet<Path<object>>();
		foreach (TreeControl.Node child in node.Children)
		{
			AddPaths(child, path, paths);
		}
		node.Expanded = false;
		node.Expanded = true;
		foreach (TreeControl.Node child2 in node.Children)
		{
			ExpandPaths(child2, path, paths);
		}
	}

	private void AddPaths(TreeControl.Node node, List<object> path, HashSet<Path<object>> paths)
	{
		if (!node.Expanded)
		{
			return;
		}
		path.Add(node.Tag);
		paths.Add(new AdaptablePath<object>(path));
		foreach (TreeControl.Node child in node.Children)
		{
			AddPaths(child, path, paths);
		}
		path.RemoveAt(path.Count - 1);
	}

	private void ExpandPaths(TreeControl.Node node, List<object> path, HashSet<Path<object>> paths)
	{
		if (node.Tag == null)
		{
			return;
		}
		path.Add(node.Tag);
		if (paths.Contains(new AdaptablePath<object>(path)))
		{
			node.Expanded = true;
			foreach (TreeControl.Node child in node.Children)
			{
				ExpandPaths(child, path, paths);
			}
		}
		path.RemoveAt(path.Count - 1);
	}

	public TreeControl.Node Show(Path<object> path, bool select)
	{
		TreeControl.Node node = ExpandPath(path, suppressAutoExpand: false);
		if (node != null)
		{
			if (select)
			{
				node.Selected = true;
			}
			m_treeControl.EnsureVisible(node);
		}
		return node;
	}

	public void BeginLabelEdit(Path<object> path)
	{
		TreeControl.Node node = Show(path, select: true);
		m_treeControl.BeginLabelEdit(node);
	}

	public bool IsVisible(object item)
	{
		IEnumerable<TreeControl.Node> enumerable = m_itemToNodeMap[item];
		using (IEnumerator<TreeControl.Node> enumerator = enumerable.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				TreeControl.Node current = enumerator.Current;
				return true;
			}
		}
		return false;
	}

	public bool IsVisible(Path<object> path)
	{
		return ExpandPath(path, suppressAutoExpand: true) != null;
	}

	public bool IsExpanded(object item)
	{
		IEnumerable<TreeControl.Node> enumerable = m_itemToNodeMap[item];
		using (IEnumerator<TreeControl.Node> enumerator = enumerable.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				TreeControl.Node current = enumerator.Current;
				return current.Expanded;
			}
		}
		return false;
	}

	public bool IsExpanded(Path<object> path)
	{
		return ExpandPath(path, suppressAutoExpand: true)?.Expanded ?? false;
	}

	public Tree<object> GetExpansion()
	{
		if (m_treeView != null)
		{
			Tree<object> tree = new Tree<object>(m_treeView.Root);
			GetExpansion(tree, m_treeControl.Root.Children);
			return tree;
		}
		return new Tree<object>();
	}

	private void GetExpansion(Tree<object> tree, IEnumerable<TreeControl.Node> nodes)
	{
		foreach (TreeControl.Node node in nodes)
		{
			Tree<object> tree2 = new Tree<object>(node.Tag);
			GetExpansion(tree2, node.Children);
			tree2.Parent = tree;
		}
	}

	private void treeControl_MouseDown(object sender, MouseEventArgs e)
	{
		Point lastHit = new Point(e.X, e.Y);
		SetLastHit(lastHit);
	}

	private void treeControl_MouseUp(object sender, MouseEventArgs e)
	{
		Point lastHit = new Point(e.X, e.Y);
		SetLastHit(lastHit);
	}

	private void treeControl_DragOver(object sender, DragEventArgs e)
	{
		Point lastHit = m_treeControl.PointToClient(new Point(e.X, e.Y));
		SetLastHit(lastHit);
	}

	private void treeControl_DragDrop(object sender, DragEventArgs e)
	{
		Point lastHit = m_treeControl.PointToClient(new Point(e.X, e.Y));
		SetLastHit(lastHit);
	}

	private void treeControl_NodeExpandedChanged(object sender, TreeControl.NodeEventArgs e)
	{
		UpdateNode(e.Node);
		SetChildren(e.Node);
	}

	private void SetChildren(TreeControl.Node parentNode)
	{
		if (m_treeView == null)
		{
			return;
		}
		if (parentNode.Expanded)
		{
			object tag = parentNode.Tag;
			if (tag == null)
			{
				return;
			}
			TreeControl.Node node = null;
			{
				foreach (object child in m_treeView.GetChildren(tag))
				{
					node = parentNode.Add(child);
					m_itemToNodeMap.Add(child, node);
					UpdateNode(node);
				}
				return;
			}
		}
		foreach (TreeControl.Node child2 in parentNode.Children)
		{
			Unbind(child2);
		}
		parentNode.Clear();
	}

	private void treeControl_NodeSelectedChanged(object sender, TreeControl.NodeEventArgs e)
	{
		if (m_selectionContext == null || m_synchronizingSelection)
		{
			return;
		}
		try
		{
			m_synchronizingSelection = true;
			Path<object> items = MakePath(e.Node);
			if (e.Node.Selected)
			{
				m_selectionContext.AddRange(items);
			}
			else
			{
				m_selectionContext.RemoveRange(items);
			}
		}
		finally
		{
			m_synchronizingSelection = false;
		}
	}

	private void treeControl_SelectionChanging(object sender, EventArgs e)
	{
		m_synchronizingSelection = true;
	}

	private void treeControl_SelectionChanged(object sender, EventArgs e)
	{
		if (m_selectionContext != null)
		{
			List<object> list = new List<object>();
			foreach (TreeControl.Node selectedNode in m_treeControl.SelectedNodes)
			{
				list.Add(MakePath(selectedNode));
			}
			m_selectionContext.SetRange(list);
		}
		m_synchronizingSelection = false;
	}

	private void tree_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		if (m_changedParents != null)
		{
			IEnumerable<TreeControl.Node> enumerable = m_itemToNodeMap[e.Parent];
			foreach (TreeControl.Node item in enumerable)
			{
				item.Expanded = true;
			}
			m_changedParents.Add(e.Parent);
		}
		else
		{
			InsertObject(e.Item, e.Parent, e.Index);
		}
	}

	private void tree_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		if (m_changedParents != null)
		{
			m_changedParents.Add(e.Parent);
		}
		else
		{
			RemoveObject(e.Item);
		}
	}

	private void tree_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		object item = e.Item;
		IEnumerable<TreeControl.Node> enumerable = m_itemToNodeMap[item];
		foreach (TreeControl.Node item2 in enumerable)
		{
			if (e.Reloaded)
			{
				RefreshNode(item2);
			}
			else
			{
				UpdateNode(item2);
			}
		}
	}

	private void tree_Reloaded(object sender, EventArgs e)
	{
		Load();
	}

	private void validationContext_Beginning(object sender, EventArgs e)
	{
		m_changedParents = new HashSet<object>();
	}

	private void validationContext_Ended(object sender, EventArgs e)
	{
		UpdateChangedParents();
	}

	private void validationContext_Cancelled(object sender, EventArgs e)
	{
		UpdateChangedParents();
	}

	private void UpdateChangedParents()
	{
		if (m_changedParents != null)
		{
			foreach (object changedParent in m_changedParents)
			{
				Refresh(changedParent);
			}
			m_changedParents = null;
		}
		if (m_lastHit != null && !m_itemToNodeMap.ContainsKey(m_lastHit))
		{
			SetLastHit(null);
		}
	}

	private void selection_Changed(object sender, EventArgs e)
	{
		if (m_synchronizingSelection)
		{
			return;
		}
		try
		{
			m_synchronizingSelection = true;
			m_treeControl.ClearSelection();
			TreeControl.Node node = null;
			foreach (Path<object> item in m_selectionContext.GetSelection<Path<object>>())
			{
				TreeControl.Node node2 = ExpandPath(item, !m_autoExpand);
				if (node2 != null)
				{
					node = node2;
					node2.Selected = true;
				}
			}
			if (node != null)
			{
				m_treeControl.EnsureVisible(node);
			}
		}
		finally
		{
			m_synchronizingSelection = false;
		}
	}

	private void Load()
	{
		Unload();
		if (m_treeView != null)
		{
			object root = m_treeView.Root;
			m_treeControl.Root.Tag = root;
			m_itemToNodeMap.Add(root, m_treeControl.Root);
			UpdateNode(m_treeControl.Root);
			SetChildren(m_treeControl.Root);
		}
	}

	private void Unload()
	{
		m_treeControl.Root.Clear();
		m_treeControl.Root.Tag = null;
		TreeControl.Node root = m_treeControl.Root;
		root.Label = null;
		root.ImageIndex = -1;
		root.StateImageIndex = -1;
		root.IsLeaf = true;
		root.HasCheck = false;
		root.AllowSelect = false;
		root.AllowLabelEdit = false;
		m_itemToNodeMap.Clear();
	}

	private void InsertObject(object childObj, object parentObj, int index)
	{
		List<TreeControl.Node> list = new List<TreeControl.Node>(m_itemToNodeMap[parentObj]);
		foreach (TreeControl.Node item in list)
		{
			if (!item.IsLeaf)
			{
				if (item.Expanded)
				{
					TreeControl.Node node = ((index < 0) ? item.Add(childObj) : item.Insert(index, childObj));
					m_itemToNodeMap.Add(childObj, node);
					UpdateNode(node);
				}
				else if (m_autoExpand)
				{
					item.Expanded = true;
				}
				else
				{
					RefreshNode(item);
				}
			}
		}
	}

	private void RemoveObject(object tree)
	{
		List<TreeControl.Node> list = new List<TreeControl.Node>(m_itemToNodeMap[tree]);
		foreach (TreeControl.Node item in list)
		{
			Unbind(item);
			item.Parent.Remove(item.Tag);
		}
	}

	private void UpdateNode(TreeControl.Node node)
	{
		ItemInfo itemInfo = new WinFormsItemInfo(m_treeControl.ImageList, m_treeControl.StateImageList);
		itemInfo.IsExpandedInView = node.Expanded;
		if (m_itemView != null && node.Tag != null)
		{
			m_itemView.GetInfo(node.Tag, itemInfo);
		}
		node.Label = itemInfo.Label;
		node.FontStyle = itemInfo.FontStyle;
		node.ImageIndex = itemInfo.ImageIndex;
		node.StateImageIndex = itemInfo.StateImageIndex;
		node.IsLeaf = itemInfo.IsLeaf;
		node.HasCheck = itemInfo.HasCheck;
		node.CheckState = itemInfo.GetCheckState();
		node.AllowSelect = itemInfo.AllowSelect;
		node.AllowLabelEdit = itemInfo.AllowLabelEdit;
		node.HoverText = itemInfo.HoverText;
		if (m_selectionContext != null && !m_synchronizingSelection)
		{
			try
			{
				m_synchronizingSelection = true;
				node.Selected = m_selectionContext.SelectionContains(MakePath(node));
			}
			finally
			{
				m_synchronizingSelection = false;
			}
		}
	}

	private void Unbind(TreeControl.Node node)
	{
		if (node.Tag != null)
		{
			m_itemToNodeMap.Remove(node.Tag, node);
		}
		foreach (TreeControl.Node child in node.Children)
		{
			Unbind(child);
		}
	}

	private Path<object> MakePath(TreeControl.Node node)
	{
		List<object> list = new List<object>();
		while (node != null)
		{
			list.Add(node.Tag);
			node = node.Parent;
		}
		list.Reverse();
		return new AdaptablePath<object>(list);
	}

	private TreeControl.Node ExpandPath(Path<object> path, bool suppressAutoExpand)
	{
		TreeControl.Node node = null;
		if (path[0].Equals(m_treeControl.Root.Tag))
		{
			node = m_treeControl.Root;
			for (int i = 1; i < path.Count; i++)
			{
				TreeControl.Node node2 = null;
				foreach (TreeControl.Node child in node.Children)
				{
					if (path[i].Equals(child.Tag))
					{
						node2 = child;
						if (i != path.Count - 1 && !suppressAutoExpand && !child.Expanded)
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
			}
		}
		return node;
	}

	private void SetLastHit(Point clientPoint)
	{
		object obj = GetItemAt(clientPoint);
		if (obj == null)
		{
			obj = TreeView;
		}
		SetLastHit(obj);
	}

	private void SetLastHit(object lastHit)
	{
		if (!object.Equals(lastHit, m_lastHit))
		{
			m_lastHit = lastHit;
			OnLastHitChanged(EventArgs.Empty);
		}
	}
}

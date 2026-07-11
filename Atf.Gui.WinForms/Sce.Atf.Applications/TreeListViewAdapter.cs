using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications;

public class TreeListViewAdapter
{
	public class NodeAdapterEventArgs : EventArgs
	{
		public object Item { get; private set; }

		public TreeListView.Node Node { get; private set; }

		public NodeAdapterEventArgs(object item, TreeListView.Node node)
		{
			Item = item;
			Node = node;
		}
	}

	public class RetrieveVirtualNodeAdapter : EventArgs
	{
		private readonly int m_index;

		public object Item { get; set; }

		public int ItemIndex => m_index;

		public RetrieveVirtualNodeAdapter(int index)
		{
			m_index = index;
		}
	}

	public class ItemLazyLoadEventArgs : EventArgs
	{
		public object Item { get; private set; }

		public ItemLazyLoadEventArgs(object item)
		{
			Item = item;
		}
	}

	public class CanItemLabelEditEventArgs : EventArgs
	{
		public object Item { get; private set; }

		public bool CanEdit { get; set; }

		public CanItemLabelEditEventArgs(object item)
		{
			Item = item;
			CanEdit = true;
		}
	}

	public class ItemLabelEditEventArgs : EventArgs
	{
		public object Item { get; private set; }

		public string Label { get; private set; }

		public bool CancelEdit { get; set; }

		public ItemLabelEditEventArgs(object item, string label)
		{
			Item = item;
			Label = label;
		}
	}

	public class CanItemPropertyChangeEventArgs : EventArgs
	{
		public object Item { get; private set; }

		public int PropertyIndex { get; private set; }

		public bool CanChange { get; set; }

		public CanItemPropertyChangeEventArgs(object item, int propertyIndex)
		{
			Item = item;
			PropertyIndex = propertyIndex;
			CanChange = true;
		}
	}

	public class ItemPropertyChangedEventArgs : EventArgs
	{
		public object Item { get; private set; }

		public int PropertyIndex { get; private set; }

		public object Value { get; private set; }

		public bool CancelChange { get; set; }

		public ItemPropertyChangedEventArgs(object item, int propertyIndex, object value)
		{
			Item = item;
			PropertyIndex = propertyIndex;
			Value = value;
		}
	}

	private IItemView m_itemView;

	private ITreeListView m_view;

	private ISelectionContext m_selectionContext;

	private IObservableContext m_observableContext;

	private IValidationContext m_validationContext;

	private int m_inTransaction = 0;

	private readonly TreeListView m_treeListView;

	private readonly List<ItemInsertedEventArgs<object>> m_queueInserts = new List<ItemInsertedEventArgs<object>>();

	private readonly List<ItemChangedEventArgs<object>> m_queueUpdates = new List<ItemChangedEventArgs<object>>();

	private readonly List<ItemRemovedEventArgs<object>> m_queueRemoves = new List<ItemRemovedEventArgs<object>>();

	private readonly Dictionary<object, TreeListView.Node> m_dictNodes = new Dictionary<object, TreeListView.Node>();

	public ITreeListView View
	{
		get
		{
			return m_view;
		}
		set
		{
			if (m_view != value)
			{
				if (m_view != null)
				{
					m_itemView = null;
					if (m_selectionContext != null)
					{
						m_treeListView.NodeSelected -= TreeListViewNodeSelected;
						m_selectionContext = null;
					}
					if (m_observableContext != null)
					{
						m_observableContext.ItemInserted -= ObservableContextItemInserted;
						m_observableContext.ItemChanged -= ObservableContextItemChanged;
						m_observableContext.ItemRemoved -= ObservableContextItemRemoved;
						m_observableContext.Reloaded -= ObservableContextReloaded;
						m_observableContext = null;
					}
					if (m_validationContext != null)
					{
						m_validationContext.Beginning -= ValidationContextBeginning;
						m_validationContext.Ending -= ValidationContextEnding;
						m_validationContext.Ended -= ValidationContextEnded;
						m_validationContext.Cancelled -= ValidationContextCancelled;
						m_validationContext = null;
					}
				}
				m_view = value;
				if (m_view != null)
				{
					m_itemView = m_view.As<IItemView>();
					m_selectionContext = m_view.As<ISelectionContext>();
					if (m_selectionContext != null)
					{
						m_treeListView.NodeSelected += TreeListViewNodeSelected;
					}
					m_observableContext = m_view.As<IObservableContext>();
					if (m_observableContext != null)
					{
						m_observableContext.ItemInserted += ObservableContextItemInserted;
						m_observableContext.ItemChanged += ObservableContextItemChanged;
						m_observableContext.ItemRemoved += ObservableContextItemRemoved;
						m_observableContext.Reloaded += ObservableContextReloaded;
					}
					m_validationContext = m_view.As<IValidationContext>();
					if (m_validationContext != null)
					{
						m_validationContext.Beginning += ValidationContextBeginning;
						m_validationContext.Ending += ValidationContextEnding;
						m_validationContext.Ended += ValidationContextEnded;
						m_validationContext.Cancelled += ValidationContextCancelled;
					}
				}
			}
			Load();
		}
	}

	public TreeListView TreeListView => m_treeListView;

	public string PersistedSettings
	{
		get
		{
			return m_treeListView.PersistedSettings;
		}
		set
		{
			m_treeListView.PersistedSettings = value;
		}
	}

	public object LastHit
	{
		get
		{
			object lastHit = m_treeListView.LastHit;
			return lastHit.Is<TreeListView.Node>() ? lastHit.As<TreeListView.Node>().Tag : this;
		}
	}

	public IEnumerable<object> Selection
	{
		get
		{
			return m_treeListView.SelectedNodes.Select((TreeListView.Node node) => node.Tag);
		}
		set
		{
			List<TreeListView.Node> list = new List<TreeListView.Node>();
			foreach (object item in value)
			{
				if (m_dictNodes.TryGetValue(item, out var value2))
				{
					list.Add(value2);
				}
			}
			if (list.Count > 0)
			{
				m_treeListView.SelectedNodes = list;
			}
		}
	}

	public int VirtualListSize
	{
		get
		{
			if (m_treeListView.TheStyle != TreeListView.Style.VirtualList)
			{
				throw new InvalidOperationException("property only valid on virtual lists");
			}
			return m_treeListView.VirtualListSize;
		}
		set
		{
			if (m_treeListView.TheStyle != TreeListView.Style.VirtualList)
			{
				throw new InvalidOperationException("property only valid on virtual lists");
			}
			m_treeListView.VirtualListSize = value;
		}
	}

	public object TopItem
	{
		get
		{
			return m_treeListView.TopItem?.Tag;
		}
		set
		{
			if (m_dictNodes.TryGetValue(value, out var value2))
			{
				m_treeListView.TopItem = value2;
			}
		}
	}

	public event EventHandler<KeyEventArgs> KeyDown;

	public event EventHandler<KeyPressEventArgs> KeyPress;

	public event EventHandler<KeyEventArgs> KeyUp;

	public event EventHandler<MouseEventArgs> MouseClick;

	public event EventHandler<MouseEventArgs> MouseDoubleClick;

	public event EventHandler<MouseEventArgs> MouseDown;

	public event EventHandler<MouseEventArgs> MouseUp;

	public event EventHandler<NodeAdapterEventArgs> ItemSelected;

	public event EventHandler<NodeAdapterEventArgs> ItemChecked;

	public event EventHandler<ItemLazyLoadEventArgs> ItemLazyLoad;

	public event EventHandler<RetrieveVirtualNodeAdapter> RetrieveVirtualItem;

	public event EventHandler<NodeAdapterEventArgs> ItemExpandedChanged;

	public event EventHandler<CanItemLabelEditEventArgs> CanItemLabelEdit;

	public event EventHandler<ItemLabelEditEventArgs> AfterItemLabelEdit;

	public event EventHandler<CanItemPropertyChangeEventArgs> CanItemPropertyChange;

	public event EventHandler<ItemPropertyChangedEventArgs> ItemPropertyChanged;

	public TreeListViewAdapter()
		: this(new TreeListView())
	{
	}

	public TreeListViewAdapter(TreeListView treeListView)
	{
		m_treeListView = treeListView;
		if (treeListView.TheStyle == TreeListView.Style.VirtualList)
		{
			m_treeListView.RetrieveVirtualNode += TreeListViewRetrieveVirtualNode;
		}
		m_treeListView.NodeChecked += TreeListViewNodeChecked;
		m_treeListView.NodeLazyLoad += TreeListViewNodeLazyLoad;
		m_treeListView.NodeExpandedChanged += TreeListViewNodeExpandedChanged;
		m_treeListView.CanLabelEdit += TreeListViewCanLabelEdit;
		m_treeListView.AfterNodeLabelEdit += TreeListViewAfterNodeLabelEdit;
		m_treeListView.CanPropertyChange += TreeListViewCanPropertyChange;
		m_treeListView.PropertyChanged += TreeListViewPropertyChanged;
		m_treeListView.Control.KeyDown += ControlKeyDown;
		m_treeListView.Control.KeyPress += ControlKeyPress;
		m_treeListView.Control.KeyUp += ControlKeyUp;
		m_treeListView.Control.MouseClick += ControlMouseClick;
		m_treeListView.Control.MouseDoubleClick += ControlMouseDoubleClick;
		m_treeListView.Control.MouseDown += ControlMouseDown;
		m_treeListView.Control.MouseUp += ControlMouseUp;
	}

	public object GetItemAt(Point clientPoint)
	{
		return TreeListView.GetNodeAt(clientPoint)?.Tag;
	}

	public int GetItemColumnIndexAt(Point clientPoint)
	{
		return m_treeListView.GetNodeColumnIndexAt(clientPoint);
	}

	public void BeginLabelEdit(object item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (m_dictNodes.TryGetValue(item, out var value))
		{
			m_treeListView.BeginLabelEdit(value);
		}
	}

	public TreeListView.Node GetNode(object item)
	{
		m_dictNodes.TryGetValue(item, out var value);
		return value;
	}

	protected virtual void OnKeyDown(KeyEventArgs e)
	{
		this.KeyDown.Raise(this, e);
	}

	protected virtual void OnKeyPress(KeyPressEventArgs e)
	{
		this.KeyPress.Raise(this, e);
	}

	protected virtual void OnKeyUp(KeyEventArgs e)
	{
		this.KeyUp.Raise(this, e);
	}

	protected virtual void OnMouseClick(MouseEventArgs e)
	{
		this.MouseClick.Raise(this, e);
	}

	protected virtual void OnMouseDoubleClick(MouseEventArgs e)
	{
		this.MouseDoubleClick.Raise(this, e);
	}

	protected virtual void OnMouseDown(MouseEventArgs e)
	{
		this.MouseDown.Raise(this, e);
	}

	protected virtual void OnMouseUp(MouseEventArgs e)
	{
		this.MouseUp.Raise(this, e);
	}

	private void ObservableContextItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		if (m_treeListView.TheStyle != TreeListView.Style.VirtualList)
		{
			if (m_inTransaction > 0)
			{
				m_queueInserts.Add(e);
			}
			else
			{
				AddItem(e.Item, e.Parent);
			}
		}
		else if (e.Item is object[])
		{
			VirtualListSize += ((object[])e.Item).Length;
		}
		else
		{
			VirtualListSize++;
		}
	}

	private void ObservableContextItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		if (m_treeListView.TheStyle != TreeListView.Style.VirtualList)
		{
			if (m_inTransaction > 0)
			{
				m_queueUpdates.Add(e);
			}
			else
			{
				UpdateItem(e.Item);
			}
		}
		else
		{
			UpdateItem(e.Item);
		}
	}

	private void ObservableContextItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		if (m_treeListView.TheStyle != TreeListView.Style.VirtualList)
		{
			if (m_inTransaction > 0)
			{
				m_queueRemoves.Add(e);
			}
			else
			{
				RemoveItem(e.Item);
			}
		}
		else if (e.Item is object[])
		{
			object[] array = (object[])e.Item;
			VirtualListSize -= array.Length;
			object[] array2 = array;
			foreach (object key in array2)
			{
				if (m_dictNodes.ContainsKey(key))
				{
					m_dictNodes.Remove(key);
				}
			}
		}
		else
		{
			VirtualListSize--;
			if (m_dictNodes.ContainsKey(e.Item))
			{
				m_dictNodes.Remove(e.Item);
			}
		}
	}

	private void ObservableContextReloaded(object sender, EventArgs e)
	{
		Load();
	}

	private void ValidationContextBeginning(object sender, EventArgs e)
	{
		m_inTransaction++;
		m_treeListView.UseInsertQueue = true;
	}

	private void ValidationContextEnding(object sender, EventArgs e)
	{
	}

	private void ValidationContextEnded(object sender, EventArgs e)
	{
		if (m_inTransaction == 0)
		{
			return;
		}
		try
		{
			m_treeListView.BeginUpdate();
			try
			{
				m_treeListView.UseInsertQueue = true;
				foreach (ItemInsertedEventArgs<object> queueInsert in m_queueInserts)
				{
					AddItem(queueInsert.Item, queueInsert.Parent);
				}
			}
			finally
			{
				m_treeListView.UseInsertQueue = false;
				m_treeListView.FlushInsertQueue();
			}
			foreach (ItemChangedEventArgs<object> queueUpdate in m_queueUpdates)
			{
				UpdateItem(queueUpdate.Item);
			}
			foreach (ItemRemovedEventArgs<object> queueRemove in m_queueRemoves)
			{
				RemoveItem(queueRemove.Item);
			}
		}
		finally
		{
			m_inTransaction--;
			m_queueInserts.Clear();
			m_queueUpdates.Clear();
			m_queueRemoves.Clear();
			m_treeListView.EndUpdate();
		}
	}

	private void ValidationContextCancelled(object sender, EventArgs e)
	{
		m_queueInserts.Clear();
		m_queueUpdates.Clear();
		m_queueRemoves.Clear();
		m_inTransaction--;
	}

	private void TreeListViewRetrieveVirtualNode(object sender, TreeListView.RetrieveVirtualNodeEventArgs e)
	{
		RetrieveVirtualNodeAdapter retrieveVirtualNodeAdapter = new RetrieveVirtualNodeAdapter(e.NodeIndex);
		this.RetrieveVirtualItem.Raise(this, retrieveVirtualNodeAdapter);
		if (retrieveVirtualNodeAdapter.Item != null)
		{
			TreeListView.Node node = CreateNodeForObject(retrieveVirtualNodeAdapter.Item, m_itemView, m_treeListView.ImageList, m_treeListView.StateImageList, m_dictNodes);
			m_dictNodes[retrieveVirtualNodeAdapter.Item] = node;
			e.Node = node;
		}
	}

	private void TreeListViewNodeSelected(object sender, ItemSelectedEventArgs<TreeListView.Node> e)
	{
		if (m_selectionContext == null)
		{
			return;
		}
		object tag = e.Item.Tag;
		if (tag != null)
		{
			if (e.Selected)
			{
				m_selectionContext.Add(tag);
			}
			else
			{
				m_selectionContext.Remove(tag);
			}
			this.ItemSelected.Raise(this, new NodeAdapterEventArgs(tag, e.Item));
		}
	}

	private void TreeListViewNodeChecked(object sender, TreeListView.NodeCheckedEventArgs e)
	{
		object tag = e.Node.Tag;
		if (tag != null)
		{
			this.ItemChecked.Raise(this, new NodeAdapterEventArgs(tag, e.Node));
		}
	}

	private void TreeListViewNodeLazyLoad(object sender, TreeListView.NodeEventArgs e)
	{
		object tag = e.Node.Tag;
		if (tag != null)
		{
			this.ItemLazyLoad.Raise(this, new ItemLazyLoadEventArgs(tag));
		}
	}

	private void TreeListViewNodeExpandedChanged(object sender, TreeListView.NodeEventArgs e)
	{
		object tag = e.Node.Tag;
		if (tag != null)
		{
			this.ItemExpandedChanged.Raise(this, new NodeAdapterEventArgs(tag, e.Node));
		}
	}

	private void TreeListViewCanLabelEdit(object sender, TreeListView.CanLabelEditEventArgs e)
	{
		CanItemLabelEditEventArgs e2 = new CanItemLabelEditEventArgs(e.Node.Tag)
		{
			CanEdit = e.CanEdit
		};
		this.CanItemLabelEdit.Raise(this, e2);
		e.CanEdit = e2.CanEdit;
	}

	private void TreeListViewAfterNodeLabelEdit(object sender, TreeListView.NodeLabelEditEventArgs e)
	{
		ItemLabelEditEventArgs e2 = new ItemLabelEditEventArgs(e.Node.Tag, e.Label);
		this.AfterItemLabelEdit.Raise(this, e2);
		e.CancelEdit = e2.CancelEdit;
	}

	private void TreeListViewCanPropertyChange(object sender, TreeListView.CanPropertyChangeEventArgs e)
	{
		CanItemPropertyChangeEventArgs e2 = new CanItemPropertyChangeEventArgs(e.Node.Tag, e.PropertyIndex)
		{
			CanChange = e.CanChange
		};
		this.CanItemPropertyChange.Raise(this, e2);
		e.CanChange = e2.CanChange;
	}

	private void TreeListViewPropertyChanged(object sender, TreeListView.PropertyChangedEventArgs e)
	{
		ItemPropertyChangedEventArgs e2 = new ItemPropertyChangedEventArgs(e.Node.Tag, e.PropertyIndex, e.Value);
		this.ItemPropertyChanged.Raise(this, e2);
		e.CancelChange = e2.CancelChange;
	}

	private void ControlKeyDown(object sender, KeyEventArgs e)
	{
		OnKeyDown(e);
	}

	private void ControlKeyPress(object sender, KeyPressEventArgs e)
	{
		OnKeyPress(e);
	}

	private void ControlKeyUp(object sender, KeyEventArgs e)
	{
		OnKeyUp(e);
	}

	private void ControlMouseClick(object sender, MouseEventArgs e)
	{
		OnMouseClick(e);
	}

	private void ControlMouseDoubleClick(object sender, MouseEventArgs e)
	{
		OnMouseDoubleClick(e);
	}

	private void ControlMouseDown(object sender, MouseEventArgs e)
	{
		OnMouseDown(e);
	}

	private void ControlMouseUp(object sender, MouseEventArgs e)
	{
		OnMouseUp(e);
	}

	private void Load()
	{
		Unload();
		if (m_view == null)
		{
			return;
		}
		if (m_treeListView.TheStyle == TreeListView.Style.VirtualList)
		{
			m_treeListView.VirtualListSize = 0;
		}
		string[] columnNames = m_view.ColumnNames;
		foreach (string label in columnNames)
		{
			m_treeListView.Columns.Add(new TreeListView.Column(label));
		}
		try
		{
			m_treeListView.BeginUpdate();
			if (m_treeListView.TheStyle == TreeListView.Style.VirtualList)
			{
				return;
			}
			foreach (object root in m_view.Roots)
			{
				if (m_inTransaction > 0)
				{
					m_queueInserts.Add(new ItemInsertedEventArgs<object>(-1, root, null));
				}
				else
				{
					AddItem(root, null);
				}
			}
		}
		finally
		{
			m_treeListView.EndUpdate();
		}
	}

	private void Unload()
	{
		if (m_treeListView == null)
		{
			return;
		}
		try
		{
			m_treeListView.ClearAll();
		}
		finally
		{
			m_dictNodes.Clear();
			m_queueInserts.Clear();
			m_queueUpdates.Clear();
			m_queueRemoves.Clear();
		}
	}

	private void AddItem(object item, object parent)
	{
		if (m_dictNodes.ContainsKey(item))
		{
			return;
		}
		TreeListView.Node value = null;
		if (parent != null)
		{
			m_dictNodes.TryGetValue(parent, out value);
		}
		TreeListView.Node node = CreateNodeForObject(item, m_itemView, m_treeListView.ImageList, m_treeListView.StateImageList, m_dictNodes);
		m_dictNodes.Add(item, node);
		AddChildrenToItemRecursively(item, node, m_view, m_itemView, m_treeListView.ImageList, m_treeListView.StateImageList, m_dictNodes);
		TreeListView.NodeCollection nodeCollection = ((value == null) ? m_treeListView.Nodes : value.Nodes);
		if (nodeCollection.IsReadOnly && nodeCollection.Owner != null)
		{
			nodeCollection.Owner.IsLeaf = false;
		}
		if (!nodeCollection.IsReadOnly)
		{
			nodeCollection.Add(node);
			if (value != null)
			{
				value.Expanded = true;
			}
		}
	}

	private void UpdateItem(object item)
	{
		if (m_dictNodes.TryGetValue(item, out var value))
		{
			ItemInfo itemInfo = GetItemInfo(item, m_itemView, m_treeListView.ImageList, m_treeListView.StateImageList);
			UpdateNodeFromItemInfo(value, item, itemInfo);
			m_treeListView.Invalidate(value);
		}
	}

	private void RemoveItem(object item)
	{
		if (!m_dictNodes.TryGetValue(item, out var value))
		{
			return;
		}
		m_dictNodes.Remove(item);
		if (value.HasChildren)
		{
			foreach (TreeListView.Node item2 in GatherNodes(value.Nodes))
			{
				if (m_dictNodes.ContainsKey(item2.Tag))
				{
					m_dictNodes.Remove(item2.Tag);
				}
			}
		}
		TreeListView.NodeCollection nodeCollection = ((value.Parent != null) ? value.Parent.Nodes : m_treeListView.Nodes);
		if (!nodeCollection.IsReadOnly)
		{
			nodeCollection.Remove(value);
		}
	}

	private static void AddChildrenToItemRecursively(object item, TreeListView.Node node, ITreeListView view, IItemView itemView, ImageList imageList, ImageList stateImageList, IDictionary<object, TreeListView.Node> dictNodes)
	{
		if (view == null)
		{
			return;
		}
		IEnumerable<object> children = view.GetChildren(item);
		foreach (object item2 in children)
		{
			TreeListView.Node node2 = CreateNodeForObject(item2, itemView, imageList, stateImageList, dictNodes);
			dictNodes?.Add(item2, node2);
			node.Nodes.Add(node2);
			AddChildrenToItemRecursively(item2, node2, view, itemView, imageList, stateImageList, dictNodes);
		}
	}

	private static TreeListView.Node CreateNodeForObject(object item, IItemView itemView, ImageList imageList, ImageList stateImageList, IDictionary<object, TreeListView.Node> dictNodes)
	{
		if (dictNodes.TryGetValue(item, out var value))
		{
			return value;
		}
		value = new TreeListView.Node();
		ItemInfo itemInfo = GetItemInfo(item, itemView, imageList, stateImageList);
		UpdateNodeFromItemInfo(value, item, itemInfo);
		return value;
	}

	private static ItemInfo GetItemInfo(object item, IItemView itemView, ImageList imageList, ImageList stateImageList)
	{
		WinFormsItemInfo winFormsItemInfo = new WinFormsItemInfo(imageList, stateImageList);
		if (itemView == null)
		{
			winFormsItemInfo.Label = GetObjectString(item);
			winFormsItemInfo.Properties = new object[0];
			winFormsItemInfo.ImageIndex = -1;
			winFormsItemInfo.StateImageIndex = -1;
			winFormsItemInfo.CheckState = CheckState.Unchecked;
			winFormsItemInfo.FontStyle = FontStyle.Regular;
			winFormsItemInfo.IsLeaf = true;
			winFormsItemInfo.IsExpandedInView = false;
			winFormsItemInfo.HoverText = string.Empty;
		}
		else
		{
			itemView.GetInfo(item, winFormsItemInfo);
		}
		return winFormsItemInfo;
	}

	private static void UpdateNodeFromItemInfo(TreeListView.Node node, object item, ItemInfo info)
	{
		node.Label = info.Label;
		node.Properties = info.Properties;
		node.ImageIndex = info.ImageIndex;
		node.StateImageIndex = info.StateImageIndex;
		node.CheckState = info.GetCheckState();
		node.Tag = item;
		node.IsLeaf = info.IsLeaf;
		node.Expanded = info.IsExpandedInView;
		node.FontStyle = info.FontStyle;
		node.HoverText = info.HoverText;
	}

	private static string GetObjectString(object value)
	{
		return (value is IFormattable formattable) ? formattable.ToString(null, null) : value.ToString();
	}

	private static IEnumerable<TreeListView.Node> GatherNodes(IEnumerable<TreeListView.Node> collection)
	{
		foreach (TreeListView.Node node in collection)
		{
			yield return node;
			if (!node.HasChildren)
			{
				continue;
			}
			foreach (TreeListView.Node item in GatherNodes(node.Nodes))
			{
				yield return item;
			}
		}
	}
}

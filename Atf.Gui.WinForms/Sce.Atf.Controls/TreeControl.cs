using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class TreeControl : Control
{
	public enum Style
	{
		Tree,
		SimpleTree,
		CategorizedPalette
	}

	[Flags]
	public enum KeyboardShortcuts
	{
		UpDownNav = 1,
		LeftRightExpand = 2,
		HomeEnd = 4,
		PageUpDown = 8,
		StarExpandSubTree = 0x10,
		FirstLetterMatching = 0x20,
		Default = 1,
		WindowsExplorer = 0x3F
	}

	[Flags]
	public enum LabelEditModes
	{
		EditOnClick = 1,
		EditOnF2 = 2,
		Default = 1
	}

	private delegate void WndProcCallback(ref Message m);

	protected class NodeLayoutInfo
	{
		public Node Node;

		public int X;

		public int Y;

		public int Depth;

		public int StateImageLeft;

		public int ImageLeft;

		public int LabelLeft;
	}

	protected enum HitType
	{
		None,
		Expander,
		CheckBox,
		Item,
		Label
	}

	protected struct HitRecord
	{
		public readonly HitType Type;

		public Node Node;

		public HitRecord(HitType type, Node node)
		{
			Type = type;
			Node = node;
		}
	}

	public class NodeEventArgs : EventArgs
	{
		public readonly Node Node;

		public NodeEventArgs(Node node)
		{
			Node = node;
		}
	}

	public class CancelNodeEventArgs : CancelEventArgs
	{
		public readonly Node Node;

		public CancelNodeEventArgs(Node node)
		{
			Node = node;
		}
	}

	public class Node
	{
		[Flags]
		private enum Flags
		{
			None = 0,
			IsLeaf = 1,
			Expanded = 2,
			Selected = 4,
			NoSelect = 8,
			HasCheck = 0x10,
			AllowLabelEdit = 0x20,
			PartiallyExpanded = 0x40
		}

		private readonly TreeControl m_owner;

		private Node m_parent;

		private object m_tag;

		private string m_label;

		private FontStyle m_fontStyle = FontStyle.Regular;

		private string m_hoverText = string.Empty;

		private List<Node> m_children;

		private int m_imageIndex = -1;

		private int m_stateImageIndex = -1;

		private int m_labelWidth = -1;

		private int m_labelHeight = -1;

		private Flags m_flags;

		private CheckState m_checkState;

		private static readonly Node[] s_emptyArray = EmptyArray<Node>.Instance;

		public Node Parent => m_parent;

		public TreeControl TreeControl => m_owner;

		public bool Expanded
		{
			get
			{
				return GetFlag(Flags.Expanded);
			}
			set
			{
				bool flag = GetFlag(Flags.Expanded) ^ value;
				if (flag)
				{
					CancelNodeEventArgs e = new CancelNodeEventArgs(this);
					m_owner.OnNodeExpandedChanging(e);
					flag = !e.Cancel;
				}
				if (flag)
				{
					SetFlag(Flags.Expanded, value);
					m_owner.OnNodeExpandedChanged(new NodeEventArgs(this));
					m_owner.Invalidate();
				}
			}
		}

		public bool PartiallyExpanded
		{
			get
			{
				return GetFlag(Flags.PartiallyExpanded);
			}
			set
			{
				SetFlag(Flags.PartiallyExpanded, value);
			}
		}

		public bool AllowSelect
		{
			get
			{
				return !GetFlag(Flags.NoSelect);
			}
			set
			{
				SetFlag(Flags.NoSelect, !value);
			}
		}

		public bool Selected
		{
			get
			{
				return GetFlag(Flags.Selected);
			}
			set
			{
				SelectionMode selectionMode = m_owner.m_selectionMode;
				if (selectionMode == SelectionMode.None)
				{
					value = false;
				}
				bool flag = (m_flags & Flags.Selected) != 0;
				if (flag == value || (value && GetFlag(Flags.NoSelect)))
				{
					return;
				}
				if (selectionMode == SelectionMode.One && value)
				{
					m_owner.ClearSelection();
				}
				bool allowSelect = AllowSelect;
				try
				{
					m_owner.OnNodeSelectionFiltered(new NodeEventArgs(this));
				}
				finally
				{
					if (AllowSelect)
					{
						SetFlag(Flags.Selected, value);
						m_owner.OnNodeSelectedChanged(new NodeEventArgs(this));
						m_owner.Invalidate();
					}
					AllowSelect = allowSelect;
				}
			}
		}

		public IEnumerable<Node> Children => (m_children != null) ? m_children.ToArray() : s_emptyArray;

		internal List<Node> InnerList => m_children;

		public bool IsLeaf
		{
			get
			{
				return GetFlag(Flags.IsLeaf);
			}
			set
			{
				if (SetFlag(Flags.IsLeaf, value) != value)
				{
					m_owner.Invalidate();
				}
			}
		}

		public object Tag
		{
			get
			{
				return m_tag;
			}
			set
			{
				m_tag = value;
			}
		}

		public string Label
		{
			get
			{
				return m_label;
			}
			set
			{
				if (m_label != value)
				{
					m_label = value;
					InvalidateLabelSize();
					m_owner.Invalidate();
				}
			}
		}

		public FontStyle FontStyle
		{
			get
			{
				return m_fontStyle;
			}
			set
			{
				if (m_fontStyle != value)
				{
					m_fontStyle = value;
					m_owner.Invalidate();
				}
			}
		}

		public string HoverText
		{
			get
			{
				return m_hoverText;
			}
			set
			{
				if (string.Compare(value, m_hoverText) != 0)
				{
					m_hoverText = value ?? string.Empty;
				}
			}
		}

		public bool AllowLabelEdit
		{
			get
			{
				return GetFlag(Flags.AllowLabelEdit);
			}
			set
			{
				SetFlag(Flags.AllowLabelEdit, value);
			}
		}

		public bool HasCheck
		{
			get
			{
				return GetFlag(Flags.HasCheck);
			}
			set
			{
				if (SetFlag(Flags.HasCheck, value))
				{
					m_owner.Invalidate();
				}
			}
		}

		public CheckState CheckState
		{
			get
			{
				return m_checkState;
			}
			set
			{
				if (m_checkState != value)
				{
					m_checkState = value;
					m_owner.Invalidate();
				}
			}
		}

		public int ImageIndex
		{
			get
			{
				return m_imageIndex;
			}
			set
			{
				if (m_imageIndex != value)
				{
					m_imageIndex = value;
					m_owner.Invalidate();
				}
			}
		}

		public int StateImageIndex
		{
			get
			{
				return m_stateImageIndex;
			}
			set
			{
				if (m_stateImageIndex != value)
				{
					m_stateImageIndex = value;
					m_owner.Invalidate();
				}
			}
		}

		public int LabelWidth
		{
			get
			{
				if (m_labelWidth == -1)
				{
					throw new InvalidOperationException("Was not initialized by a call to UpdateNodeMeasurements");
				}
				return m_labelWidth;
			}
			set
			{
				m_labelWidth = value;
			}
		}

		public int LabelHeight
		{
			get
			{
				if (m_labelHeight == -1)
				{
					throw new InvalidOperationException("Was not initialized by a call to UpdateNodeMeasurements");
				}
				return m_labelHeight;
			}
			set
			{
				m_labelHeight = value;
			}
		}

		internal Node(TreeControl owner, Node parent, object tag)
		{
			m_owner = owner;
			m_parent = parent;
			m_tag = tag;
			if (parent == null)
			{
				SetFlag(Flags.Expanded, value: true);
			}
			m_owner.m_nodesToMeasure.Add(this);
		}

		public Node Add(object tag)
		{
			int index = 0;
			if (m_children != null)
			{
				index = m_children.Count;
			}
			return Insert(index, tag);
		}

		public Node Insert(int index, object tag)
		{
			if (m_children == null)
			{
				m_children = new List<Node>();
			}
			Node node = new Node(m_owner, this, tag);
			m_children.Insert(index, node);
			IsLeaf = false;
			m_owner.Invalidate();
			return node;
		}

		public bool Remove(object tag)
		{
			if (m_children != null)
			{
				for (int i = 0; i < m_children.Count; i++)
				{
					if (m_children[i].Tag.Equals(tag))
					{
						m_children[i].m_parent = null;
						m_children.RemoveAt(i);
						m_owner.UpdateNodeMeasurements();
						m_owner.CleanUpSpecialNodes();
						m_owner.Invalidate();
						if (m_children.Count == 0)
						{
							m_children = null;
						}
						return true;
					}
				}
			}
			return false;
		}

		public void Clear()
		{
			if (m_children == null)
			{
				return;
			}
			foreach (Node child in m_children)
			{
				child.m_parent = null;
			}
			m_children = null;
			m_owner.UpdateNodeMeasurements();
			m_owner.CleanUpSpecialNodes();
			m_owner.Invalidate();
		}

		private void InvalidateLabelSize()
		{
			m_labelWidth = -1;
			m_labelHeight = -1;
			if (!m_owner.m_nodesToMeasure.Contains(this))
			{
				m_owner.m_nodesToMeasure.Add(this);
			}
		}

		private bool GetFlag(Flags flag)
		{
			return (m_flags & flag) != 0;
		}

		private bool SetFlag(Flags flag, bool value)
		{
			Flags flags = m_flags;
			if (value)
			{
				m_flags |= flag;
			}
			else
			{
				m_flags &= ~flag;
			}
			return flags != m_flags;
		}
	}

	private Node m_firstSelectedNode;

	private readonly Style m_style;

	private TreeItemRenderer m_itemRenderer;

	private readonly Node m_root;

	private readonly HashSet<Node> m_nodesToMeasure = new HashSet<Node>();

	private Size m_clientSize;

	private int m_averageRowHeight;

	private readonly VScrollBar m_vScrollBar;

	private int m_vScroll;

	private readonly HScrollBar m_hScrollBar;

	private int m_hScroll;

	private readonly Timer m_autoScrollTimer;

	private int m_autoScrollSpeed = 50;

	private bool m_autoScrollOnExpand = true;

	private bool m_autoScrollUp;

	private int m_indent = 16;

	private ImageList m_imageList;

	private ImageList m_stateImageList;

	private Image m_filterImage;

	private Point m_lastHoverPosition = new Point(int.MinValue, int.MinValue);

	private bool m_resetHoverEvent;

	private SelectionMode m_selectionMode = SelectionMode.MultiExtended;

	private Node m_extendSelectionBaseNode;

	private Node m_currentKeyedNode;

	private Node m_leftClickedSelectedNode;

	private Node m_labelEditNode;

	private Point m_mouseDownPoint;

	private readonly TextBox m_textBox;

	private readonly Timer m_dragHoverExpandTimer;

	private Node m_dragHoverNode;

	private readonly Timer m_editLabelTimer;

	private bool m_showRoot = true;

	private bool m_dragHoverExpand;

	private bool m_selecting;

	private bool m_dragging;

	private bool m_lastMouseDownWasDoubleClick;

	private bool m_toggleOnDoubleClick = true;

	private bool m_expandOnSingleClick;

	private bool m_dragBetween;

	private bool m_showDragBetweenCue;

	protected bool m_handleKeyUp;

	private int m_contentVerticalOffset;

	private KeyboardShortcuts m_navigationKeyBehavior;

	private LabelEditModes m_labelEditMode = LabelEditModes.EditOnClick;

	public Node Root => m_root;

	public bool ShowRoot
	{
		get
		{
			return m_showRoot;
		}
		set
		{
			if (m_style != Style.CategorizedPalette && m_showRoot != value)
			{
				m_showRoot = value;
				Invalidate();
			}
		}
	}

	public TreeItemRenderer ItemRenderer
	{
		get
		{
			return m_itemRenderer;
		}
		set
		{
			m_itemRenderer = value;
			m_itemRenderer.Owner = this;
			Indent = Indent;
			Invalidate();
		}
	}

	public int Indent
	{
		get
		{
			return m_indent;
		}
		set
		{
			if (m_style != Style.CategorizedPalette)
			{
				value = Math.Max(m_itemRenderer.ExpanderSize.Width + base.Margin.Left, value);
			}
			if (m_indent != value)
			{
				m_indent = value;
				Invalidate();
			}
		}
	}

	public ImageList ImageList
	{
		get
		{
			return m_imageList;
		}
		set
		{
			m_imageList = value;
		}
	}

	public ImageList StateImageList
	{
		get
		{
			return m_stateImageList;
		}
		set
		{
			m_stateImageList = value;
		}
	}

	public SelectionMode SelectionMode
	{
		get
		{
			return m_selectionMode;
		}
		set
		{
			if (m_selectionMode != value)
			{
				m_selectionMode = value;
				ClearSelection();
			}
		}
	}

	internal Size ActualClientSize => m_clientSize;

	public bool DragHoverExpand
	{
		get
		{
			return m_dragHoverExpand;
		}
		set
		{
			m_dragHoverExpand = value;
		}
	}

	public int AutoExpandDelay
	{
		get
		{
			return m_dragHoverExpandTimer.Interval;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("delay must be > 0");
			}
			m_dragHoverExpandTimer.Interval = value;
		}
	}

	public int AutoScrollSpeed
	{
		get
		{
			return m_autoScrollSpeed;
		}
		set
		{
			m_autoScrollSpeed = value;
		}
	}

	public bool AutoScrollOnExpand
	{
		get
		{
			return m_autoScrollOnExpand;
		}
		set
		{
			m_autoScrollOnExpand = value;
		}
	}

	public bool ToggleOnDoubleClick
	{
		get
		{
			return m_toggleOnDoubleClick;
		}
		set
		{
			m_toggleOnDoubleClick = value;
		}
	}

	public bool ExpandOnSingleClick
	{
		get
		{
			return m_expandOnSingleClick;
		}
		set
		{
			m_expandOnSingleClick = value;
		}
	}

	public bool DragBetween => m_dragBetween;

	public bool ShowDragBetweenCue
	{
		get
		{
			return m_showDragBetweenCue;
		}
		set
		{
			m_showDragBetweenCue = value;
		}
	}

	public bool NavigationKeyChangingSelection { get; set; }

	public KeyboardShortcuts NavigationKeyBehavior
	{
		get
		{
			return m_navigationKeyBehavior;
		}
		set
		{
			m_navigationKeyBehavior = value;
		}
	}

	public LabelEditModes LabelEditMode
	{
		get
		{
			return m_labelEditMode;
		}
		set
		{
			m_labelEditMode = value;
		}
	}

	public IEnumerable<Node> Nodes
	{
		get
		{
			Stack<Node> nodes = new Stack<Node>();
			nodes.Push(m_root);
			while (nodes.Count > 0)
			{
				Node node = nodes.Pop();
				yield return node;
				if (node.InnerList != null)
				{
					for (int i = node.InnerList.Count - 1; i >= 0; i--)
					{
						nodes.Push(node.InnerList[i]);
					}
				}
			}
		}
	}

	public IEnumerable<Node> VisibleNodes
	{
		get
		{
			Stack<Node> nodes = new Stack<Node>();
			if (m_showRoot)
			{
				nodes.Push(m_root);
			}
			else if (m_root.InnerList != null)
			{
				for (int i = m_root.InnerList.Count - 1; i >= 0; i--)
				{
					nodes.Push(m_root.InnerList[i]);
				}
			}
			while (nodes.Count > 0)
			{
				Node node = nodes.Pop();
				yield return node;
				if (node.Expanded && node.InnerList != null)
				{
					for (int i2 = node.InnerList.Count - 1; i2 >= 0; i2--)
					{
						nodes.Push(node.InnerList[i2]);
					}
				}
			}
		}
	}

	public IEnumerable<Node> SelectedNodes
	{
		get
		{
			Stack<Node> nodes = new Stack<Node>();
			nodes.Push(m_root);
			while (nodes.Count > 0)
			{
				Node node = nodes.Pop();
				if (node.Selected)
				{
					yield return node;
				}
				if (node.InnerList != null)
				{
					for (int i = node.InnerList.Count - 1; i >= 0; i--)
					{
						nodes.Push(node.InnerList[i]);
					}
				}
			}
		}
	}

	public Node FirstSelectedNode => m_firstSelectedNode;

	public bool IsDragging => m_dragging;

	public int ContentVerticalOffset
	{
		get
		{
			return m_contentVerticalOffset;
		}
		set
		{
			m_contentVerticalOffset = value;
		}
	}

	protected IEnumerable<NodeLayoutInfo> NodeLayout
	{
		get
		{
			NodeLayoutInfo nodeInfo = new NodeLayoutInfo();
			int xPadding = Margin.Left;
			int yPadding = Margin.Top;
			int x = -m_hScroll + xPadding;
			int y = -m_vScroll + yPadding + ContentVerticalOffset;
			bool drawExpanders = DrawExpanders;
			Node lastNode = m_root;
			int depth = 0;
			foreach (Node node in VisibleNodes)
			{
				while (node.Parent != lastNode.Parent)
				{
					if (node.Parent == lastNode)
					{
						depth++;
						x += Indent;
						break;
					}
					depth--;
					x -= Indent;
					lastNode = lastNode.Parent;
				}
				nodeInfo.Node = node;
				nodeInfo.Depth = depth;
				nodeInfo.X = x;
				nodeInfo.Y = y;
				int left = x;
				if (!drawExpanders)
				{
					left -= Indent;
				}
				if (node.HasCheck)
				{
					left += m_itemRenderer.CheckBoxSize.Width + xPadding;
				}
				nodeInfo.StateImageLeft = left;
				if (m_stateImageList != null && node.StateImageIndex >= 0)
				{
					left += m_stateImageList.ImageSize.Width + 1;
				}
				nodeInfo.ImageLeft = left;
				if (m_imageList != null && node.ImageIndex >= 0)
				{
					left += m_imageList.ImageSize.Width + xPadding;
				}
				nodeInfo.LabelLeft = left;
				yield return nodeInfo;
				y += GetRowHeight(node);
				lastNode = node;
			}
		}
	}

	private bool DrawExpanders => m_style != Style.CategorizedPalette;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public event EventHandler<NodeEventArgs> NodeSelectedChanged;

	public event EventHandler<NodeEventArgs> NodeSelectionFiltered;

	public event EventHandler<CancelNodeEventArgs> NodeExpandedChanging;

	public event EventHandler<NodeEventArgs> NodeExpandedChanged;

	public event EventHandler<NodeEventArgs> NodeCheckStateEdited;

	public event EventHandler<NodeEventArgs> NodeLabelEdited;

	public TreeControl()
		: this(Style.Tree, null)
	{
	}

	public TreeControl(Style style)
		: this(style, null)
	{
	}

	public TreeControl(Style style, TreeItemRenderer itemRenderer)
	{
		m_style = style;
		if (m_style == Style.CategorizedPalette)
		{
			m_showRoot = false;
		}
		m_root = new Node(this, null, null);
		m_dragHoverExpandTimer = new Timer();
		m_dragHoverExpandTimer.Interval = 1000;
		m_dragHoverExpandTimer.Tick += DragHoverTimerTick;
		m_autoScrollTimer = new Timer();
		m_autoScrollTimer.Interval = 200;
		m_autoScrollTimer.Tick += AutoScrollTimerTick;
		m_editLabelTimer = new Timer();
		m_editLabelTimer.Tick += EditLabelTimerTick;
		m_averageRowHeight = base.FontHeight + base.Margin.Top;
		SuspendLayout();
		m_textBox = new TextBox();
		m_textBox.Visible = false;
		m_textBox.BorderStyle = BorderStyle.None;
		m_textBox.KeyDown += TextBoxKeyDown;
		m_textBox.KeyPress += TextBoxKeyPress;
		m_textBox.LostFocus += TextBoxLostFocus;
		m_vScrollBar = new VScrollBar();
		m_vScrollBar.Dock = DockStyle.Right;
		m_vScrollBar.SmallChange = m_averageRowHeight;
		m_vScrollBar.ValueChanged += VerticalScrollBarValueChanged;
		m_hScrollBar = new HScrollBar();
		m_hScrollBar.Dock = DockStyle.Bottom;
		m_vScrollBar.SmallChange = 8;
		m_hScrollBar.ValueChanged += HorizontalScrollBarValueChanged;
		base.Controls.Add(m_vScrollBar);
		base.Controls.Add(m_hScrollBar);
		base.Controls.Add(m_textBox);
		ResumeLayout();
		BackColor = SystemColors.Window;
		base.DoubleBuffered = true;
		SetStyle(ControlStyles.ResizeRedraw, value: true);
		if (itemRenderer == null)
		{
			itemRenderer = new TreeItemRenderer();
		}
		ItemRenderer = itemRenderer;
		m_filterImage = ResourceUtil.GetImage16(Resources.FilterImage) as Bitmap;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_dragHoverExpandTimer.Dispose();
			m_autoScrollTimer.Dispose();
			m_editLabelTimer.Dispose();
		}
		base.Dispose(disposing);
	}

	public Node GetNodeAt(Point clientPoint)
	{
		UpdateNodeMeasurements();
		int num = clientPoint.Y + m_vScroll - ContentVerticalOffset;
		foreach (Node visibleNode in VisibleNodes)
		{
			int rowHeight = GetRowHeight(visibleNode);
			if (rowHeight > 0 && num <= rowHeight)
			{
				return visibleNode;
			}
			num -= rowHeight;
		}
		return null;
	}

	public void ExpandAll()
	{
		foreach (Node node in Nodes)
		{
			if (!node.IsLeaf)
			{
				node.Expanded = true;
			}
		}
	}

	public void CollapseAll()
	{
		foreach (Node node in Nodes)
		{
			if (node != m_root)
			{
				node.Expanded = false;
			}
		}
	}

	public Node ExpandToFirstLeaf()
	{
		Node node = m_root;
		Node node2;
		do
		{
			node2 = node;
			using IEnumerator<Node> enumerator = node.Children.GetEnumerator();
			if (enumerator.MoveNext())
			{
				Node current = enumerator.Current;
				node = current;
			}
		}
		while (node2 != node);
		if (node != null)
		{
			EnsureVisible(node);
		}
		return node;
	}

	public void Show(Node node)
	{
		for (Node node2 = node.Parent; node2 != null; node2 = node2.Parent)
		{
			node2.Expanded = true;
		}
	}

	public void EnsureVisible(Node node)
	{
		Show(node);
		ScrollIntoView(node);
	}

	public void ScrollIntoView(Node node)
	{
		ScrollIntoView(node, scrollChildren: false);
	}

	public void ScrollChildrenIntoView(Node node)
	{
		ScrollIntoView(node, scrollChildren: true);
	}

	private void ScrollIntoView(Node node, bool scrollChildren)
	{
		UpdateNodeMeasurements();
		int num = -m_vScroll;
		foreach (Node visibleNode in VisibleNodes)
		{
			if (visibleNode == node)
			{
				break;
			}
			int rowHeight = GetRowHeight(visibleNode);
			num += rowHeight;
		}
		int num2 = m_vScroll;
		if (num < 0)
		{
			num2 += num;
		}
		else
		{
			int num3 = 0;
			if (scrollChildren)
			{
				num3 = GetChildrenHeight(node, m_clientSize.Height);
			}
			int num4 = GetRowHeight(node) + num3;
			if (num4 > m_clientSize.Height)
			{
				num4 = m_clientSize.Height;
			}
			int num5 = num + num4;
			if (num5 > m_clientSize.Height)
			{
				num2 += num5 - m_clientSize.Height;
			}
		}
		SetVerticalScroll(num2);
	}

	public void BeginLabelEdit(Node node)
	{
		EndLabelEdit();
		if (!node.AllowLabelEdit)
		{
			return;
		}
		m_labelEditNode = node;
		foreach (NodeLayoutInfo item in NodeLayout)
		{
			if (item.Node == m_labelEditNode)
			{
				m_textBox.Bounds = GetLabelEditBounds(item);
				m_textBox.Text = m_labelEditNode.Label;
				m_textBox.SelectAll();
				m_textBox.Show();
				m_textBox.Focus();
				Invalidate();
				break;
			}
		}
	}

	protected virtual Rectangle GetLabelEditBounds(NodeLayoutInfo info)
	{
		return new Rectangle(info.LabelLeft, info.Y, m_clientSize.Width - info.LabelLeft + 1, base.FontHeight + base.Margin.Top + 1);
	}

	public void SetSelection(Node selected)
	{
		foreach (Node selectedNode in SelectedNodes)
		{
			selectedNode.Selected = false;
		}
		if (selected != null)
		{
			selected.Selected = true;
		}
		m_extendSelectionBaseNode = selected;
		m_currentKeyedNode = selected;
	}

	public void ClearSelection()
	{
		SetSelection(null);
		m_firstSelectedNode = null;
	}

	protected virtual void OnSelectionChanging(EventArgs e)
	{
		this.SelectionChanging.Raise(this, e);
	}

	protected virtual void OnSelectionChanged(EventArgs e)
	{
		this.SelectionChanged.Raise(this, e);
	}

	protected virtual void OnNodeSelectedChanged(NodeEventArgs e)
	{
		this.NodeSelectedChanged.Raise(this, e);
	}

	protected virtual void OnNodeSelectionFiltered(NodeEventArgs e)
	{
		this.NodeSelectionFiltered.Raise(this, e);
	}

	protected virtual bool OnNodeExpandedChanging(CancelNodeEventArgs e)
	{
		return this.NodeExpandedChanging.RaiseCancellable(this, e);
	}

	protected virtual void OnNodeExpandedChanged(NodeEventArgs e)
	{
		this.NodeExpandedChanged.Raise(this, e);
	}

	protected virtual void OnNodeCheckStateEdited(NodeEventArgs e)
	{
		this.NodeCheckStateEdited.Raise(this, e);
	}

	protected virtual void OnNodeLabelEdited(NodeEventArgs e)
	{
		this.NodeLabelEdited.Raise(this, e);
	}

	protected virtual bool IsNodeMultiSelectable(Node node)
	{
		return true;
	}

	protected Node GetPreviousNode(Node node)
	{
		Node result = null;
		foreach (Node visibleNode in VisibleNodes)
		{
			if (visibleNode == node)
			{
				return result;
			}
			result = visibleNode;
		}
		return null;
	}

	protected Node GetNextNode(Node node)
	{
		Node node2 = null;
		foreach (Node visibleNode in VisibleNodes)
		{
			if (node2 != null)
			{
				return visibleNode;
			}
			if (visibleNode == node)
			{
				node2 = visibleNode;
			}
		}
		return null;
	}

	protected Node GetFirstNode()
	{
		using (IEnumerator<Node> enumerator = VisibleNodes.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return null;
	}

	protected override void OnResize(EventArgs e)
	{
		EndLabelEdit();
		base.OnResize(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		EndLabelEdit();
		Focus();
		m_mouseDownPoint = new Point(e.X, e.Y);
		HitRecord hitRecord = Pick(m_mouseDownPoint);
		Node node = hitRecord.Node;
		switch (hitRecord.Type)
		{
		case HitType.Expander:
			ToggleExpand(node);
			break;
		case HitType.CheckBox:
		{
			CheckState checkState = ((node.CheckState != CheckState.Checked) ? CheckState.Checked : CheckState.Unchecked);
			if (node.CheckState != checkState)
			{
				node.CheckState = checkState;
				OnNodeCheckStateEdited(new NodeEventArgs(node));
			}
			break;
		}
		case HitType.Item:
		case HitType.Label:
		{
			m_selecting = false;
			Keys keys = FilterModifiers();
			if ((keys & Keys.Alt) != Keys.None)
			{
				break;
			}
			if (e.Button == MouseButtons.Left)
			{
				m_selecting = true;
				OnSelectionChanging(EventArgs.Empty);
				if ((keys & Keys.Control) != Keys.None)
				{
					if (IsNodeMultiSelectable(node))
					{
						node.Selected = !node.Selected;
					}
					break;
				}
				if ((keys & Keys.Shift) != Keys.None)
				{
					ExtendSelection(node);
					break;
				}
				if (node.Selected)
				{
					m_leftClickedSelectedNode = node;
				}
				else
				{
					SetSelection(node);
					m_firstSelectedNode = node;
				}
				if (ExpandOnSingleClick && !node.IsLeaf && !node.Expanded)
				{
					node.Expanded = true;
				}
			}
			else if (e.Button == MouseButtons.Right && !node.Selected)
			{
				m_selecting = true;
				OnSelectionChanging(EventArgs.Empty);
				SetSelection(node);
				if (m_firstSelectedNode == null)
				{
					m_firstSelectedNode = node;
				}
			}
			break;
		}
		}
		m_lastMouseDownWasDoubleClick = e.Clicks == 2;
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && !m_dragging && AllowDrop)
		{
			if (m_selecting)
			{
				Size dragSize = SystemInformation.DragSize;
				if (Math.Abs(e.X - m_mouseDownPoint.X) > dragSize.Width || Math.Abs(e.Y - m_mouseDownPoint.Y) > dragSize.Height)
				{
					m_dragging = true;
				}
			}
			else
			{
				m_dragging = false;
			}
		}
		if (m_resetHoverEvent)
		{
			ResetMouseEventArgs();
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		HitRecord hitRecord = Pick(new Point(e.X, e.Y));
		if (m_selecting)
		{
			m_selecting = false;
			if (m_leftClickedSelectedNode != null && e.Button == MouseButtons.Left)
			{
				SetSelection(m_leftClickedSelectedNode);
				m_firstSelectedNode = m_leftClickedSelectedNode;
				if (LabelEditModeContains(LabelEditModes.EditOnClick) && !m_lastMouseDownWasDoubleClick && e.Button == MouseButtons.Left && hitRecord.Node == m_leftClickedSelectedNode && hitRecord.Type == HitType.Label && hitRecord.Node.AllowLabelEdit)
				{
					m_editLabelTimer.Interval = SystemInformation.DoubleClickTime;
					m_editLabelTimer.Tag = m_leftClickedSelectedNode;
					m_editLabelTimer.Enabled = true;
				}
				m_leftClickedSelectedNode = null;
			}
			OnSelectionChanged(EventArgs.Empty);
		}
		m_dragging = false;
		base.OnMouseUp(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		if (m_selecting)
		{
			m_selecting = false;
			m_leftClickedSelectedNode = null;
			OnSelectionChanged(EventArgs.Empty);
		}
		m_dragging = false;
		base.OnMouseLeave(e);
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		EndLabelEdit();
		HitRecord hitRecord = Pick(new Point(e.X, e.Y));
		Node node = hitRecord.Node;
		HitType type = hitRecord.Type;
		if ((type == HitType.Item || type == HitType.Label) && ToggleOnDoubleClick)
		{
			ToggleExpand(node);
		}
		base.OnMouseDoubleClick(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		int verticalScroll = m_vScrollBar.Value - e.Delta / 2;
		SetVerticalScroll(verticalScroll);
		base.OnMouseWheel(e);
	}

	protected override void OnMouseHover(EventArgs e)
	{
		Point point = PointToClient(Control.MousePosition);
		if (point != m_lastHoverPosition)
		{
			m_lastHoverPosition = point;
			Node node = Pick(point).Node;
			m_resetHoverEvent = true;
		}
		base.OnMouseHover(e);
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		Node node = null;
		bool flag = false;
		switch (keyData & Keys.KeyCode)
		{
		case Keys.Down:
			node = GetNextNode(m_currentKeyedNode);
			if (m_style == Style.CategorizedPalette && NavigationKeyBehaviorContains(KeyboardShortcuts.UpDownNav))
			{
				while (node != null && node.Parent == Root)
				{
					node = GetNextNode(node);
				}
			}
			flag = true;
			break;
		case Keys.Up:
			node = GetPreviousNode(m_currentKeyedNode);
			if (m_style == Style.CategorizedPalette && NavigationKeyBehaviorContains(KeyboardShortcuts.UpDownNav))
			{
				while (node != null && node.Parent == Root)
				{
					node = GetPreviousNode(node);
				}
			}
			flag = true;
			break;
		case Keys.Left:
			if (NavigationKeyBehaviorContains(KeyboardShortcuts.LeftRightExpand) && m_currentKeyedNode != null)
			{
				if (m_currentKeyedNode.Expanded)
				{
					ToggleExpand(m_currentKeyedNode);
				}
				else
				{
					node = m_currentKeyedNode.Parent;
				}
				flag = true;
			}
			break;
		case Keys.Right:
			if (!NavigationKeyBehaviorContains(KeyboardShortcuts.LeftRightExpand) || m_currentKeyedNode == null)
			{
				break;
			}
			if (m_currentKeyedNode.Expanded)
			{
				using IEnumerator<Node> enumerator = m_currentKeyedNode.Children.GetEnumerator();
				if (enumerator.MoveNext())
				{
					Node current = enumerator.Current;
					node = current;
				}
			}
			else if (!m_currentKeyedNode.IsLeaf)
			{
				m_currentKeyedNode.Expanded = true;
			}
			flag = true;
			break;
		case Keys.Home:
			if (!NavigationKeyBehaviorContains(KeyboardShortcuts.HomeEnd))
			{
				break;
			}
			if (ShowRoot)
			{
				node = Root;
			}
			else
			{
				using IEnumerator<Node> enumerator3 = Root.Children.GetEnumerator();
				if (enumerator3.MoveNext())
				{
					Node current3 = enumerator3.Current;
					node = current3;
				}
			}
			flag = true;
			break;
		case Keys.End:
			if (!NavigationKeyBehaviorContains(KeyboardShortcuts.HomeEnd))
			{
				break;
			}
			foreach (Node visibleNode in VisibleNodes)
			{
				node = visibleNode;
			}
			flag = true;
			break;
		case Keys.Next:
			if (NavigationKeyBehaviorContains(KeyboardShortcuts.PageUpDown))
			{
				int num2 = base.Height - base.Margin.Top - base.Margin.Bottom;
				SetVerticalScroll(m_vScroll + num2 - base.FontHeight);
				flag = true;
			}
			break;
		case Keys.Prior:
			if (NavigationKeyBehaviorContains(KeyboardShortcuts.PageUpDown))
			{
				int num = base.Height - base.Margin.Top - base.Margin.Bottom;
				SetVerticalScroll(m_vScroll - num - base.FontHeight);
				flag = true;
			}
			break;
		}
		if (node != null)
		{
			EndLabelEdit();
			ScrollIntoView(node);
			try
			{
				NavigationKeyChangingSelection = true;
				OnSelectionChanging(EventArgs.Empty);
				m_currentKeyedNode = node;
				if ((keyData & Keys.Shift) != Keys.None && m_selectionMode == SelectionMode.MultiExtended)
				{
					ExtendSelection(node);
				}
				else
				{
					SetSelection(node);
					m_firstSelectedNode = node;
				}
			}
			finally
			{
				OnSelectionChanged(EventArgs.Empty);
				NavigationKeyChangingSelection = false;
			}
		}
		if (flag)
		{
			return true;
		}
		return base.ProcessDialogKey(keyData);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.Handled || m_currentKeyedNode == null)
		{
			return;
		}
		if (e.KeyData == Keys.F2 && LabelEditModeContains(LabelEditModes.EditOnF2))
		{
			BeginLabelEdit(m_currentKeyedNode);
		}
		else if (e.KeyData == Keys.Multiply)
		{
			Stack<Node> stack = new Stack<Node>();
			if (!m_currentKeyedNode.IsLeaf)
			{
				stack.Push(m_currentKeyedNode);
			}
			while (stack.Count > 0)
			{
				Node node = stack.Pop();
				node.Expanded = true;
				if (node.InnerList == null)
				{
					continue;
				}
				for (int num = node.InnerList.Count - 1; num >= 0; num--)
				{
					if (!node.InnerList[num].IsLeaf)
					{
						stack.Push(node.InnerList[num]);
					}
				}
			}
		}
		else
		{
			if (e.Modifiers != Keys.None)
			{
				return;
			}
			char c = Convert.ToChar(e.KeyValue);
			if (!char.IsLetterOrDigit(c))
			{
				return;
			}
			string value = char.ToString(c);
			Node node2 = m_currentKeyedNode;
			do
			{
				node2 = GetNextNode(node2) ?? GetFirstNode();
				if (node2 == m_currentKeyedNode)
				{
					break;
				}
				if (node2.Label != null && node2.Label.StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
				{
					ScrollIntoView(node2);
					try
					{
						NavigationKeyChangingSelection = true;
						OnSelectionChanging(EventArgs.Empty);
						m_currentKeyedNode = node2;
						SetSelection(node2);
						m_firstSelectedNode = node2;
						break;
					}
					finally
					{
						OnSelectionChanged(EventArgs.Empty);
						NavigationKeyChangingSelection = false;
					}
				}
			}
			while (node2 != m_currentKeyedNode);
		}
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		if (m_handleKeyUp)
		{
			m_handleKeyUp = false;
			e.Handled = true;
		}
		base.OnKeyUp(e);
	}

	protected override void OnDragOver(DragEventArgs e)
	{
		Point clientPoint = PointToClient(new Point(e.X, e.Y));
		if (clientPoint.Y < m_averageRowHeight)
		{
			m_autoScrollUp = true;
			m_autoScrollTimer.Enabled = true;
		}
		else if (clientPoint.Y >= m_clientSize.Height - m_averageRowHeight)
		{
			m_autoScrollUp = false;
			m_autoScrollTimer.Enabled = true;
		}
		else
		{
			m_autoScrollTimer.Enabled = false;
			Node nodeAt = GetNodeAt(clientPoint);
			if (nodeAt != m_dragHoverNode)
			{
				m_dragHoverExpandTimer.Stop();
				m_dragHoverExpandTimer.Enabled = nodeAt != null;
				m_dragHoverNode = nodeAt;
			}
			int num = clientPoint.Y + m_vScroll;
			foreach (Node visibleNode in VisibleNodes)
			{
				int rowHeight = GetRowHeight(visibleNode);
				if (num <= rowHeight)
				{
					break;
				}
				num -= rowHeight;
			}
			m_dragBetween = num < 5;
		}
		base.OnDragOver(e);
	}

	protected override void OnDragDrop(DragEventArgs e)
	{
		StopDragTimers();
		base.OnDragDrop(e);
	}

	protected override void OnDragLeave(EventArgs e)
	{
		StopDragTimers();
		base.OnDragLeave(e);
	}

	private void StopDragTimers()
	{
		m_dragHoverExpandTimer.Enabled = false;
		m_dragHoverNode = null;
		m_autoScrollTimer.Enabled = false;
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 15)
		{
			if (base.InvokeRequired)
			{
				BeginInvoke(new WndProcCallback(WndProc), m);
				return;
			}
			using Graphics g = CreateGraphics();
			UpdateNodeMeasurements(g);
			int num = 0;
			int num2 = 0;
			foreach (NodeLayoutInfo item in NodeLayout)
			{
				Node node = item.Node;
				num = Math.Max(num, item.LabelLeft + node.LabelWidth);
				num2 = item.Y;
			}
			num += m_hScroll;
			num2 += m_vScroll + base.FontHeight + base.Margin.Top;
			m_hScrollBar.Value = m_hScroll;
			m_vScrollBar.Value = m_vScroll;
			int num3 = base.Width;
			int num4 = base.Height;
			WinFormsUtil.UpdateScrollbars(m_vScrollBar, m_hScrollBar, new Size(num3, num4), new Size(num, num2));
			if (m_vScrollBar.Visible)
			{
				num3 -= m_vScrollBar.Width;
			}
			if (m_hScrollBar.Visible)
			{
				num4 -= m_hScrollBar.Height;
			}
			m_clientSize = new Size(num3, num4);
		}
		base.WndProc(ref m);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		Invalidate();
		base.OnGotFocus(e);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		Invalidate();
		base.OnLostFocus(e);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		foreach (Node node in Nodes)
		{
			m_nodesToMeasure.Add(node);
		}
		base.OnFontChanged(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics graphics = e.Graphics;
		UpdateNodeMeasurements(graphics);
		int num = 0;
		int num2 = 0;
		int num3 = m_clientSize.Height;
		int top = base.Margin.Top;
		bool flag = false;
		bool drawExpanders = DrawExpanders;
		Size expanderSize = m_itemRenderer.ExpanderSize;
		int num4 = expanderSize.Width / 2;
		int num5 = expanderSize.Height / 2;
		int num6 = m_itemRenderer.CheckBoxSize.Height / 2;
		int num7 = m_clientSize.Width;
		Node node = null;
		if (ShowDragBetweenCue && DragBetween)
		{
			int num8 = PointToClient(Cursor.Position).Y + m_vScroll;
			foreach (Node visibleNode in VisibleNodes)
			{
				int rowHeight = GetRowHeight(visibleNode);
				if (num8 <= rowHeight)
				{
					node = visibleNode;
					break;
				}
				num8 -= rowHeight;
			}
		}
		List<int> list = new List<int>(16);
		int num9 = 0;
		foreach (NodeLayoutInfo item in NodeLayout)
		{
			Node node2 = item.Node;
			int rowHeight2 = GetRowHeight(node2);
			int num10 = item.Y + rowHeight2 / 2;
			if (list.Count == 0)
			{
				list.Add(rowHeight2 - top);
			}
			while (item.Depth != num9)
			{
				if (item.Depth > num9)
				{
					list.Add(item.Y + rowHeight2 - top);
					num9 = item.Depth;
					break;
				}
				list.RemoveAt(num9);
				num9--;
			}
			bool flag2 = rowHeight2 > 0 && item.Y + rowHeight2 > 0 && item.Y < num3;
			list[num9] = item.Y + rowHeight2 - top;
			if (num9 > 0)
			{
				int num11 = list[num9 - 1];
				int num12;
				int value;
				if (node2.IsLeaf)
				{
					num12 = num10;
					value = num12 + 1;
				}
				else
				{
					num12 = Math.Max(num10 - num5, num11);
					value = num10 + num5 + 1;
				}
				list[num9 - 1] = value;
				if (flag2)
				{
					m_itemRenderer.DrawBackground(node2, graphics, item.X, item.Y);
				}
				if (num12 >= 0 && num11 <= num3 && flag)
				{
					m_itemRenderer.DrawHierarchyLine(graphics, new Point(item.X - Indent + num4, num11), new Point(item.X - Indent + num4, num12));
				}
				if (flag2)
				{
					int num13 = item.X - Indent + num4;
					if (drawExpanders && !node2.IsLeaf)
					{
						num13 += num4 + 1;
					}
					if (flag)
					{
						m_itemRenderer.DrawHierarchyLine(graphics, new Point(num13, num10), new Point(item.X, num10));
					}
				}
			}
			if (!flag2)
			{
				continue;
			}
			num++;
			num2 += rowHeight2;
			if (node2.HasCheck)
			{
				m_itemRenderer.DrawCheckBox(node2, graphics, item.X, num10 - num6);
			}
			if (m_stateImageList != null && node2.StateImageIndex >= 0 && node2.StateImageIndex < m_stateImageList.Images.Count)
			{
				m_itemRenderer.DrawImage(m_stateImageList, graphics, item.StateImageLeft, num10 - m_stateImageList.ImageSize.Height / 2, node2.StateImageIndex);
			}
			int num14 = 0;
			if (node2.PartiallyExpanded)
			{
				num14 += m_filterImage.Width;
				graphics.DrawImage(m_filterImage, item.ImageLeft + m_filterImage.Width, num10 - (m_filterImage.Height + top) / 2);
			}
			if (node2 != m_labelEditNode)
			{
				m_itemRenderer.DrawLabel(node2, graphics, item.LabelLeft + num14, num10 - node2.LabelHeight / 2, m_clientSize.Width);
			}
			if (m_imageList != null && node2.ImageIndex >= 0 && node2.ImageIndex < m_imageList.Images.Count)
			{
				m_itemRenderer.DrawImage(m_imageList, graphics, item.ImageLeft, num10 - m_imageList.ImageSize.Height / 2, node2.ImageIndex);
			}
			if (drawExpanders)
			{
				if (!node2.IsLeaf)
				{
					m_itemRenderer.DrawExpander(node2, graphics, item.X - Indent, num10 - num5);
				}
			}
			else if (node2.Parent == m_root)
			{
				Rectangle r = new Rectangle(0, item.Y, num7, rowHeight2);
				m_itemRenderer.DrawCategory(node2, graphics, r);
			}
			m_itemRenderer.DrawData(node2, graphics, item.LabelLeft + num14, num10 - node2.LabelHeight / 2);
			if (node2 == node)
			{
				graphics.DrawLine(Pens.Red, new Point(item.X - Indent + num4, item.Y - 2), new Point(item.X - Indent + num4 + 100, item.Y - 2));
			}
		}
		if (num > 0)
		{
			int num15 = (int)Math.Ceiling((double)num2 / (double)num);
			if (num15 != m_averageRowHeight)
			{
				m_averageRowHeight = num15;
				m_vScrollBar.SmallChange = num15;
			}
		}
		else
		{
			graphics.DrawString(Text, Font, ItemRenderer.TextBrush, base.ClientRectangle);
		}
	}

	private void TextBoxLostFocus(object sender, EventArgs e)
	{
		EndLabelEdit();
	}

	private void TextBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			EndLabelEdit();
			e.Handled = true;
			m_handleKeyUp = true;
		}
		else if (e.KeyCode == Keys.Escape)
		{
			AbortLabelEdit();
			e.Handled = true;
			m_handleKeyUp = true;
		}
	}

	private void TextBoxKeyPress(object sender, KeyPressEventArgs e)
	{
		e.Handled = m_labelEditNode == null;
	}

	private void VerticalScrollBarValueChanged(object sender, EventArgs e)
	{
		m_vScroll = m_vScrollBar.Value;
		EndLabelEdit();
		Refresh();
	}

	private void HorizontalScrollBarValueChanged(object sender, EventArgs e)
	{
		m_hScroll = m_hScrollBar.Value;
		EndLabelEdit();
		Refresh();
	}

	private void DragHoverTimerTick(object sender, EventArgs e)
	{
		if (m_dragHoverNode == null)
		{
			return;
		}
		if (!m_dragHoverNode.Expanded && !m_dragHoverNode.IsLeaf)
		{
			m_dragHoverNode.Expanded = true;
			if (m_autoScrollOnExpand)
			{
				ScrollChildrenIntoView(m_dragHoverNode);
			}
		}
		m_dragHoverNode = null;
	}

	private void AutoScrollTimerTick(object sender, EventArgs e)
	{
		if (m_autoScrollUp)
		{
			SetVerticalScroll(m_vScroll - m_autoScrollSpeed);
		}
		else
		{
			SetVerticalScroll(m_vScroll + m_autoScrollSpeed);
		}
	}

	private void EditLabelTimerTick(object sender, EventArgs e)
	{
		BeginLabelEdit((Node)m_editLabelTimer.Tag);
	}

	private void ExtendSelection(Node clickedNode)
	{
		if (m_extendSelectionBaseNode == null)
		{
			return;
		}
		bool flag = false;
		foreach (Node visibleNode in VisibleNodes)
		{
			if (!IsNodeMultiSelectable(visibleNode))
			{
				continue;
			}
			visibleNode.Selected = flag;
			if (visibleNode == m_extendSelectionBaseNode || visibleNode == clickedNode)
			{
				if (m_extendSelectionBaseNode != clickedNode)
				{
					flag = !flag;
				}
				visibleNode.Selected = true;
			}
		}
	}

	private void ToggleExpand(Node node)
	{
		if (node != m_root)
		{
			bool flag = (node.Expanded = !node.IsLeaf && !node.Expanded);
			if (flag && m_autoScrollOnExpand)
			{
				ScrollChildrenIntoView(node);
			}
		}
	}

	private Keys FilterModifiers()
	{
		Keys result = Control.ModifierKeys;
		if (m_selectionMode != SelectionMode.MultiExtended)
		{
			result = Keys.None;
		}
		return result;
	}

	private void EndLabelEdit()
	{
		if (m_labelEditNode != null)
		{
			string text = m_textBox.Text;
			if (m_labelEditNode.Label != text)
			{
				m_labelEditNode.Label = text;
				OnNodeLabelEdited(new NodeEventArgs(m_labelEditNode));
			}
			m_labelEditNode = null;
		}
		m_textBox.Hide();
		m_editLabelTimer.Enabled = false;
		m_editLabelTimer.Tag = null;
	}

	private void AbortLabelEdit()
	{
		m_labelEditNode = null;
		m_textBox.Hide();
		m_editLabelTimer.Enabled = false;
		m_editLabelTimer.Tag = null;
	}

	private void SetVerticalScroll(int vScroll)
	{
		vScroll = Math.Max(m_vScrollBar.Minimum, Math.Min(m_vScrollBar.Maximum, vScroll));
		if (m_vScroll != vScroll)
		{
			m_vScroll = vScroll;
			Invalidate();
		}
	}

	private void UpdateNodeMeasurements()
	{
		if (m_nodesToMeasure.Count > 0)
		{
			using (Graphics g = CreateGraphics())
			{
				UpdateNodeMeasurements(g);
			}
		}
	}

	private void UpdateNodeMeasurements(Graphics g)
	{
		Size size = new Size(0, 0);
		foreach (Node item in m_nodesToMeasure)
		{
			Size size2 = m_itemRenderer.MeasureLabel(item, g);
			size.Width = Math.Max(size2.Width, size.Width);
			size.Height = Math.Max(size2.Height, size.Height);
		}
		foreach (Node item2 in m_nodesToMeasure)
		{
			item2.LabelWidth = size.Width;
			item2.LabelHeight = size.Height;
		}
		m_nodesToMeasure.Clear();
	}

	private bool IsNodeInTree(Node node)
	{
		while (node != null && node != m_root)
		{
			node = node.Parent;
		}
		return node == m_root;
	}

	private void CleanUpSpecialNodes()
	{
		if (!IsNodeInTree(m_currentKeyedNode))
		{
			m_currentKeyedNode = null;
		}
		if (!IsNodeInTree(m_extendSelectionBaseNode))
		{
			m_extendSelectionBaseNode = null;
		}
		if (!IsNodeInTree(m_leftClickedSelectedNode))
		{
			m_leftClickedSelectedNode = null;
		}
		if (!IsNodeInTree(m_labelEditNode))
		{
			m_labelEditNode = null;
		}
	}

	protected HitRecord Pick(Point p)
	{
		UpdateNodeMeasurements();
		int left = base.Margin.Left;
		bool flag = m_style != Style.CategorizedPalette;
		int num = m_clientSize.Width;
		foreach (NodeLayoutInfo item in NodeLayout)
		{
			Node node = item.Node;
			int rowHeight = GetRowHeight(node);
			if (rowHeight == 0 || p.Y >= item.Y + rowHeight)
			{
				continue;
			}
			if (node != m_root && !node.IsLeaf)
			{
				if (flag)
				{
					if (p.X >= item.X - Indent && p.X <= item.X - Indent + m_itemRenderer.ExpanderSize.Width)
					{
						return new HitRecord(HitType.Expander, node);
					}
				}
				else if (node.Parent == m_root || p.X > num - 9 - left)
				{
					return new HitRecord(HitType.Expander, node);
				}
			}
			if (node.HasCheck && p.X >= item.X && p.X <= item.X + m_itemRenderer.CheckBoxSize.Width)
			{
				return new HitRecord(HitType.CheckBox, node);
			}
			return (p.X > item.LabelLeft) ? new HitRecord(HitType.Label, node) : new HitRecord(HitType.Item, node);
		}
		return default(HitRecord);
	}

	public int GetRowHeight(Node node)
	{
		int num = node.LabelHeight;
		if (DrawExpanders && !node.IsLeaf)
		{
			num = Math.Max(num, m_itemRenderer.ExpanderSize.Height);
		}
		if (node.HasCheck)
		{
			num = Math.Max(num, m_itemRenderer.CheckBoxSize.Height);
		}
		if (m_stateImageList != null && node.StateImageIndex >= 0)
		{
			num = Math.Max(num, m_stateImageList.ImageSize.Height);
		}
		if (m_imageList != null && node.ImageIndex >= 0)
		{
			num = Math.Max(num, m_imageList.ImageSize.Height);
		}
		if (num == 0 && m_style == Style.CategorizedPalette && node.Parent == m_root)
		{
			return 0;
		}
		return num + base.Margin.Top;
	}

	private int GetChildrenHeight(Node node, int maxHeight)
	{
		UpdateNodeMeasurements();
		int num = 0;
		foreach (Node child in node.Children)
		{
			num += GetRowHeight(child);
			if (num > maxHeight)
			{
				return maxHeight;
			}
			if (child.Expanded && !child.IsLeaf)
			{
				num += GetChildrenHeight(child, maxHeight - num);
			}
		}
		return num;
	}

	private bool LabelEditModeContains(LabelEditModes flag)
	{
		return (m_labelEditMode & flag) != 0;
	}

	private bool NavigationKeyBehaviorContains(KeyboardShortcuts shortcut)
	{
		return (m_navigationKeyBehavior & shortcut) == shortcut;
	}
}

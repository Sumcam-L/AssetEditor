using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls;

public sealed class TreeListView : IDisposable, IAdaptable
{
	public class Column
	{
		public const int DefaultWidth = 60;

		private string m_label;

		private int m_width;

		private bool m_allowPropertyEdit;

		public string Label
		{
			get
			{
				return m_label;
			}
			set
			{
				if (string.Compare(m_label, value) != 0)
				{
					m_label = value;
					this.LabelChanged.Raise(this, EventArgs.Empty);
				}
			}
		}

		public int Width
		{
			get
			{
				return m_width;
			}
			set
			{
				if (m_width != value)
				{
					m_width = value;
					this.WidthChanged.Raise(this, EventArgs.Empty);
				}
			}
		}

		public int ActualWidth { get; set; }

		public bool AllowPropertyEdit
		{
			get
			{
				return m_allowPropertyEdit;
			}
			set
			{
				if (m_allowPropertyEdit != value)
				{
					m_allowPropertyEdit = value;
					this.AllowPropertyEditChanged.Raise(this, EventArgs.Empty);
				}
			}
		}

		public object Tag { get; set; }

		internal event EventHandler LabelChanged;

		internal event EventHandler WidthChanged;

		internal event EventHandler AllowPropertyEditChanged;

		public Column(string label)
		{
			Label = label;
			Width = 60;
			ActualWidth = 60;
		}

		public Column(string label, int width)
		{
			Label = label;
			m_width = width;
			ActualWidth = width;
		}
	}

	public class ColumnCollection : ICollection<Column>, IEnumerable<Column>, IEnumerable
	{
		private readonly List<Column> m_columns = new List<Column>();

		public int Count => m_columns.Count;

		public bool IsReadOnly => false;

		internal Column this[int index] => m_columns[index];

		internal event EventHandler<CancelColumnEventArgs> ColumnAdding;

		internal event EventHandler<ColumnEventArgs> ColumnAdded;

		internal event EventHandler<ColumnEventArgs> ColumnRemoving;

		internal event EventHandler ColumnClearAll;

		public IEnumerator<Column> GetEnumerator()
		{
			return m_columns.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_columns.GetEnumerator();
		}

		public void Add(Column item)
		{
			CancelColumnEventArgs e = new CancelColumnEventArgs(item);
			if (!this.ColumnAdding.RaiseCancellable(this, e))
			{
				m_columns.Add(item);
				this.ColumnAdded.Raise(this, new ColumnEventArgs(item));
			}
		}

		public void Clear()
		{
			this.ColumnClearAll.Raise(this, EventArgs.Empty);
			m_columns.Clear();
		}

		public bool Contains(Column item)
		{
			return m_columns.Contains(item);
		}

		public void CopyTo(Column[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(Column item)
		{
			this.ColumnRemoving.Raise(this, new ColumnEventArgs(item));
			return m_columns.Remove(item);
		}
	}

	internal class ColumnEventArgs : EventArgs
	{
		public Column Column { get; private set; }

		public ColumnEventArgs(Column column)
		{
			Column = column;
		}
	}

	internal class CancelColumnEventArgs : CancelEventArgs
	{
		public Column Column { get; private set; }

		public CancelColumnEventArgs(Column column)
			: base(cancel: false)
		{
			Column = column;
		}
	}

	public enum Style
	{
		List,
		CheckedList,
		VirtualList,
		TreeList,
		CheckedTreeList
	}

	private class DefaultSorter : IComparer<Node>
	{
		public SortOrder SortOrder { private get; set; }

		public int Compare(Node x, Node y)
		{
			int num = string.Compare(x.Label, y.Label);
			if (num == 0)
			{
				num = CompareProperties(x.Properties, y.Properties);
			}
			if (SortOrder == SortOrder.Descending)
			{
				num *= -1;
			}
			return num;
		}

		private static int CompareProperties(object[] props1, object[] props2)
		{
			if (props1 == null && props2 == null)
			{
				return 0;
			}
			if (props1 == null)
			{
				return 1;
			}
			if (props2 == null)
			{
				return -1;
			}
			if (props1.Length != props2.Length)
			{
				return (props1.Length >= props2.Length) ? 1 : (-1);
			}
			int num = 0;
			for (int i = 0; i < props1.Length; i++)
			{
				num = string.Compare(props1[i].ToString(), props2[i].ToString());
				if (num != 0)
				{
					break;
				}
			}
			return num;
		}
	}

	internal class TheTreeListView : ListView, IAdaptable
	{
		private bool m_painting;

		private bool m_gridLines;

		private NodeRenderer m_renderer;

		private Color m_textColor;

		private Color m_modifiableTextColor;

		private Color m_highlightTextColor;

		private Color m_modifiableHighlightTextColor;

		private Color m_disabledTextColor;

		private Color m_highlightBackColor;

		private Color m_disabledBackColor;

		private Color m_gridLinesColor;

		private ControlGradient m_expanderGradient;

		private Pen m_expanderPen;

		private Pen m_hierarchyLinePen;

		private readonly Style m_style;

		private readonly TreeListView m_owner;

		private readonly List<ListViewItem> m_lstWorkaround = new List<ListViewItem>();

		private const int ExtraneousItemWidth = 16;

		private const int SB_HORZ = 0;

		private const int SB_VERT = 1;

		public Color TextColor
		{
			get
			{
				return m_textColor;
			}
			set
			{
				m_textColor = value;
				Invalidate();
			}
		}

		public Color ModifiableTextColor
		{
			get
			{
				return m_modifiableTextColor;
			}
			set
			{
				m_modifiableTextColor = value;
				Invalidate();
			}
		}

		public Color HighlightTextColor
		{
			get
			{
				return m_highlightTextColor;
			}
			set
			{
				m_highlightTextColor = value;
				Invalidate();
			}
		}

		public Color ModifiableHighlightTextColor
		{
			get
			{
				return m_modifiableHighlightTextColor;
			}
			set
			{
				m_modifiableHighlightTextColor = value;
				Invalidate();
			}
		}

		public Color DisabledTextColor
		{
			get
			{
				return m_disabledTextColor;
			}
			set
			{
				m_disabledTextColor = value;
				Invalidate();
			}
		}

		public Color HighlightBackColor
		{
			get
			{
				return m_highlightBackColor;
			}
			set
			{
				m_highlightBackColor = value;
				Invalidate();
			}
		}

		public Color DisabledBackColor
		{
			get
			{
				return m_disabledBackColor;
			}
			set
			{
				m_disabledBackColor = value;
				Invalidate();
			}
		}

		public Color GridLinesColor
		{
			get
			{
				return m_gridLinesColor;
			}
			set
			{
				m_gridLinesColor = value;
				Invalidate();
			}
		}

		public new bool GridLines
		{
			get
			{
				return m_gridLines;
			}
			set
			{
				m_gridLines = value;
				Invalidate();
			}
		}

		public ControlGradient ExpanderGradient
		{
			get
			{
				return m_expanderGradient;
			}
			set
			{
				m_expanderGradient = value;
				Invalidate();
			}
		}

		public Pen ExpanderPen
		{
			get
			{
				return m_expanderPen;
			}
			set
			{
				m_expanderPen = value;
				Invalidate();
			}
		}

		public Pen HierarchyLinePen
		{
			get
			{
				return m_hierarchyLinePen;
			}
			set
			{
				m_hierarchyLinePen = value;
				Invalidate();
			}
		}

		internal Style TheStyle => m_style;

		internal NodeRenderer Renderer
		{
			get
			{
				return m_renderer ?? (m_renderer = new NodeRenderer(m_owner));
			}
			set
			{
				m_renderer = value ?? new NodeRenderer(m_owner);
			}
		}

		public event ScrollEventHandler Scroll;

		internal TheTreeListView(Style style, TreeListView owner)
		{
			m_style = style;
			m_owner = owner;
			base.OwnerDraw = true;
			base.DoubleBuffered = true;
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			base.MouseMove += ListTreeViewMouseMove;
			base.DrawColumnHeader += ListTreeViewDrawColumnHeader;
			base.DrawItem += ListTreeViewDrawItem;
			base.DrawSubItem += ListTreeViewDrawSubItem;
			base.BorderStyle = BorderStyle.Fixed3D;
			m_textColor = SystemColors.ControlText;
			m_modifiableTextColor = Color.Red;
			m_highlightTextColor = SystemColors.HighlightText;
			m_modifiableHighlightTextColor = SystemColors.ControlText;
			BackColor = SystemColors.ControlLightLight;
			m_disabledTextColor = SystemColors.GrayText;
			m_highlightBackColor = ((SolidBrush)SystemBrushes.Highlight).Color;
			m_disabledBackColor = ((SolidBrush)SystemBrushes.Control).Color;
			m_gridLinesColor = Control.DefaultBackColor;
			m_expanderGradient = new ControlGradient
			{
				StartColor = Color.White,
				EndColor = Color.LightGray,
				LinearGradientMode = LinearGradientMode.Vertical
			};
			m_expanderPen = Pens.Black;
			m_hierarchyLinePen = Pens.DarkGray;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			SkinService.ApplyActiveSkin(this);
		}

		public object GetAdapter(Type type)
		{
			return (type == typeof(TreeListView)) ? m_owner : null;
		}

		internal void ResetWorkaroundList()
		{
			m_lstWorkaround.Clear();
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
			case 15:
				try
				{
					m_painting = true;
					base.WndProc(ref m);
					if ((TheStyle == Style.VirtualList && base.VirtualListSize == 0) || (TheStyle != Style.VirtualList && base.Items.Count == 0))
					{
						using (Graphics gfx = CreateGraphics())
						{
							Renderer.DrawBackground(gfx, base.Bounds);
							break;
						}
					}
					break;
				}
				finally
				{
					m_painting = false;
				}
			case 277:
			case 522:
				base.WndProc(ref m);
				if (this.Scroll != null)
				{
					this.Scroll(this, new ScrollEventArgs(ScrollEventType.EndScroll, User32.GetScrollPos(base.Handle, 1)));
				}
				break;
			case 276:
				base.WndProc(ref m);
				if (this.Scroll != null)
				{
					this.Scroll(this, new ScrollEventArgs(ScrollEventType.EndScroll, User32.GetScrollPos(base.Handle, 0)));
				}
				break;
			case 256:
				base.WndProc(ref m);
				if (this.Scroll != null)
				{
					switch (m.WParam.ToInt32())
					{
					case 40:
						this.Scroll(this, new ScrollEventArgs(ScrollEventType.SmallIncrement, User32.GetScrollPos(base.Handle, 1)));
						break;
					case 38:
						this.Scroll(this, new ScrollEventArgs(ScrollEventType.SmallDecrement, User32.GetScrollPos(base.Handle, 1)));
						break;
					case 34:
						this.Scroll(this, new ScrollEventArgs(ScrollEventType.LargeIncrement, User32.GetScrollPos(base.Handle, 1)));
						break;
					case 33:
						this.Scroll(this, new ScrollEventArgs(ScrollEventType.LargeDecrement, User32.GetScrollPos(base.Handle, 1)));
						break;
					case 36:
						this.Scroll(this, new ScrollEventArgs(ScrollEventType.First, User32.GetScrollPos(base.Handle, 1)));
						break;
					case 35:
						this.Scroll(this, new ScrollEventArgs(ScrollEventType.Last, User32.GetScrollPos(base.Handle, 1)));
						break;
					case 37:
					case 39:
						break;
					}
				}
				break;
			default:
				base.WndProc(ref m);
				break;
			}
		}

		private void ListTreeViewMouseMove(object sender, MouseEventArgs e)
		{
			ListViewItem itemAt = GetItemAt(e.X, e.Y);
			if (itemAt != null && !m_lstWorkaround.Contains(itemAt))
			{
				m_lstWorkaround.Add(itemAt);
				Invalidate(itemAt.Bounds);
			}
		}

		private void ListTreeViewDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			e.DrawDefault = true;
		}

		private void ListTreeViewDrawItem(object sender, DrawListViewItemEventArgs e)
		{
		}

		private void ListTreeViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			if (!m_painting)
			{
				return;
			}
			int iOffset = 0;
			Node node = (Node)e.Item.Tag;
			bool flag = TheStyle != Style.VirtualList && e.ItemIndex == base.Items.Count - 1;
			bool flag2 = e.ColumnIndex == base.Columns.Count - 1;
			Renderer.DrawBackground(node, e.Graphics, e.Bounds);
			if (flag)
			{
				Rectangle bounds = new Rectangle(e.Bounds.X, e.Bounds.Y + e.Bounds.Height, e.Bounds.Width, base.Bounds.Bottom - e.Bounds.Bottom);
				Renderer.DrawBackground(node, e.Graphics, bounds);
			}
			if (GridLines)
			{
				DrawGridLines(e.Graphics, e.Bounds, GridLinesColor);
			}
			if (flag2)
			{
				Rectangle rectangle = new Rectangle(e.Bounds.Right, e.Bounds.Top, base.Bounds.Right - e.Bounds.Right, e.Bounds.Height);
				Renderer.DrawBackground(node, e.Graphics, rectangle);
				if (flag)
				{
					Rectangle bounds2 = new Rectangle(e.Bounds.Right, e.Bounds.Y + e.Bounds.Height, base.Bounds.Right - e.Bounds.Right, base.Bounds.Bottom - e.Bounds.Bottom);
					Renderer.DrawBackground(node, e.Graphics, bounds2);
				}
				if (GridLines)
				{
					Rectangle bounds3 = rectangle;
					bounds3.Inflate(1, 0);
					DrawGridLines(e.Graphics, bounds3, GridLinesColor);
				}
			}
			if (e.ColumnIndex == 0)
			{
				Rectangle rectangle2 = new Rectangle(e.Bounds.X, e.Bounds.Y, 16, e.Bounds.Height);
				switch (m_style)
				{
				case Style.CheckedList:
					Renderer.DrawCheckBox(node, e.Graphics, rectangle2);
					node.CheckBoxHitRect = rectangle2;
					iOffset += 16;
					break;
				case Style.CheckedTreeList:
					DrawExtraneousStuff(e, ref iOffset, ExpanderGradient, ExpanderPen, HierarchyLinePen);
					iOffset += 2;
					rectangle2.Offset(iOffset, 2);
					Renderer.DrawCheckBox(node, e.Graphics, rectangle2);
					node.CheckBoxHitRect = rectangle2;
					iOffset += 16;
					break;
				case Style.TreeList:
					DrawExtraneousStuff(e, ref iOffset, ExpanderGradient, ExpanderPen, HierarchyLinePen);
					break;
				}
				if (base.StateImageList != null && node.StateImageIndex != -1)
				{
					Rectangle bounds4 = new Rectangle(e.Bounds.X + iOffset, e.Bounds.Y, 16, e.Bounds.Height);
					Renderer.DrawStateImage(node, e.Graphics, bounds4);
					iOffset += 16;
				}
				if (node.ImageIndex != -1 && m_owner.ImageList != null && node.ImageIndex < m_owner.ImageList.Images.Count)
				{
					Rectangle bounds5 = new Rectangle(e.Bounds.X + iOffset, e.Bounds.Y, 16, e.Bounds.Height);
					Renderer.DrawImage(node, e.Graphics, bounds5);
					iOffset += 16;
				}
			}
			Rectangle rectangle3 = new Rectangle(e.Bounds.X + iOffset, e.Bounds.Y, e.Bounds.Width - iOffset, e.Bounds.Height);
			Renderer.DrawLabel(node, e.Graphics, rectangle3, e.ColumnIndex);
			if (e.ColumnIndex == 0)
			{
				node.LabelHitRect = rectangle3;
			}
		}

		private static void DrawExtraneousStuff(DrawListViewSubItemEventArgs e, ref int iOffset, ControlGradient expanderGradient, Pen expanderPen, Pen hierarchyLinePen)
		{
			if (e.Item.Tag == null || !e.Item.Tag.Is<Node>())
			{
				return;
			}
			Node node = e.Item.Tag.As<Node>();
			if (node.IsLeaf && node.Level == 1)
			{
				iOffset += 16;
				return;
			}
			FindSibling(node, out var sibling);
			bool bObjectBelow = sibling != null;
			FindChild(node, out var child);
			bool bItemBelow = child != null;
			for (int i = 0; i < node.Level; i++)
			{
				Point location = new Point(e.Item.Bounds.X + iOffset, e.Item.Bounds.Y);
				Size size = new Size(16, e.Item.Bounds.Height);
				Rectangle rectangle = new Rectangle(location, size);
				FindExpandedRelativeAboveAtLevel(node, i + 1, out var relative);
				if (relative != null && relative == node.Parent)
				{
					DrawElbow(e.Graphics, rectangle, hierarchyLinePen, bObjectBelow);
				}
				bool flag = false;
				FindExpandedRelativeAboveAtLevel(node, i + 2, out var relative2);
				if (relative2 != null)
				{
					FindSibling(relative2, out var sibling2);
					if (sibling2 != null)
					{
						flag = true;
					}
				}
				if (flag)
				{
					DrawVerticalLine(e.Graphics, rectangle, hierarchyLinePen);
				}
				bool flag2 = i == node.Level - 1 && (node.HasChildren || !node.IsLeaf);
				if (flag2)
				{
					if (node.Expanded)
					{
						DrawCollapser(e.Graphics, rectangle, expanderGradient, expanderPen, hierarchyLinePen, bItemBelow);
					}
					else
					{
						DrawExpander(e.Graphics, rectangle, expanderGradient, expanderPen, hierarchyLinePen);
					}
					node.HitRect = rectangle;
				}
				if (i == node.Level - 1 && i > 0 && !flag2)
				{
					DrawHorizontalLine(e.Graphics, rectangle, hierarchyLinePen);
				}
				iOffset += 16;
			}
		}

		private static void DrawExpanderButton(Graphics gfx, Rectangle bounds, ControlGradient expanderGradient, Pen hierarchyLinePen)
		{
			using Brush brush = new LinearGradientBrush(bounds, expanderGradient.StartColor, expanderGradient.EndColor, expanderGradient.LinearGradientMode);
			gfx.FillRectangle(brush, bounds);
			gfx.DrawRectangle(hierarchyLinePen, bounds);
		}

		private static void DrawExpander(Graphics gfx, Rectangle bounds, ControlGradient expanderGradient, Pen expanderPen, Pen hierarchyLinePen)
		{
			Point point = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
			DrawExpanderButton(gfx, new Rectangle(point.X - 5, point.Y - 5, 10, 10), expanderGradient, hierarchyLinePen);
			gfx.DrawLine(expanderPen, point.X - 3, point.Y, point.X + 3, point.Y);
			gfx.DrawLine(expanderPen, point.X, point.Y - 3, point.X, point.Y + 3);
		}

		private static void DrawCollapser(Graphics gfx, Rectangle bounds, ControlGradient expanderGradient, Pen expanderPen, Pen hierarchyLinePen, bool bItemBelow)
		{
			Point point = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
			DrawExpanderButton(gfx, new Rectangle(point.X - 5, point.Y - 5, 10, 10), expanderGradient, hierarchyLinePen);
			gfx.DrawLine(expanderPen, point.X - 3, point.Y, point.X + 3, point.Y);
			if (bItemBelow)
			{
				gfx.DrawLine(hierarchyLinePen, point.X, point.Y + 5, point.X, bounds.Bottom);
			}
		}

		private static void DrawVerticalLine(Graphics gfx, Rectangle bounds, Pen hierarchyLinePen)
		{
			Point point = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
			gfx.DrawLine(hierarchyLinePen, point.X, bounds.Top, point.X, bounds.Bottom);
		}

		private static void DrawElbow(Graphics gfx, Rectangle bounds, Pen hierarchyLinePen, bool bObjectBelow)
		{
			Point point = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
			gfx.DrawLine(hierarchyLinePen, point.X, bounds.Top, point.X, point.Y);
			gfx.DrawLine(hierarchyLinePen, point.X, point.Y, bounds.Right, point.Y);
			if (bObjectBelow)
			{
				gfx.DrawLine(hierarchyLinePen, point.X, point.Y, point.X, bounds.Bottom);
			}
		}

		private static void DrawHorizontalLine(Graphics gfx, Rectangle bounds, Pen hierarchyLinePen)
		{
			Point point = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
			gfx.DrawLine(hierarchyLinePen, bounds.Left, point.Y, bounds.Right, point.Y);
		}

		private static void DrawGridLines(Graphics gfx, Rectangle bounds, Color color)
		{
			using Pen pen = new Pen(color);
			gfx.DrawRectangle(pen, bounds);
		}

		private static void FindSibling(Node node, out Node sibling)
		{
			sibling = null;
			if (node == null)
			{
				return;
			}
			for (Node next = node.Next; next != null; next = next.Next)
			{
				if (next.Visible)
				{
					sibling = next;
					break;
				}
			}
		}

		private static void FindChild(Node node, out Node child)
		{
			child = null;
			if (node == null)
			{
				return;
			}
			foreach (Node node2 in node.Nodes)
			{
				if (!node2.Visible)
				{
					continue;
				}
				child = node2;
				break;
			}
		}

		private static void FindExpandedRelativeAboveAtLevel(Node node, int level, out Node relative)
		{
			relative = null;
			if (node == null || level <= 0)
			{
				return;
			}
			for (Node node2 = node.Parent; node2 != null; node2 = node2.Parent)
			{
				if (node2.Visible && node2.Level == level)
				{
					relative = node2;
					break;
				}
			}
		}
	}

	private class ListViewItemSorter : IComparer
	{
		public int Compare(object x, object y)
		{
			if (x == null && y == null)
			{
				return 0;
			}
			if (x == null)
			{
				return 1;
			}
			if (y == null)
			{
				return -1;
			}
			if (x == y)
			{
				return 0;
			}
			ListViewItem listViewItem = (ListViewItem)x;
			ListViewItem listViewItem2 = (ListViewItem)y;
			Node node = (Node)listViewItem.Tag;
			Node node2 = (Node)listViewItem2.Tag;
			return (node.VisualPosition >= node2.VisualPosition) ? 1 : (-1);
		}
	}

	public class RetrieveVirtualNodeEventArgs : EventArgs
	{
		private readonly int m_index;

		public Node Node { get; set; }

		public int NodeIndex => m_index;

		public RetrieveVirtualNodeEventArgs(int index)
		{
			m_index = index;
		}
	}

	public class NodeCheckedEventArgs : EventArgs
	{
		public Node Node { get; private set; }

		public NodeCheckedEventArgs(Node node)
		{
			Node = node;
		}
	}

	public class NodeDragEventArgs : EventArgs
	{
		public Node Node { get; private set; }

		public MouseButtons Button { get; private set; }

		public NodeDragEventArgs(Node node, MouseButtons button)
		{
			Node = node;
			Button = button;
		}
	}

	public class Node : IAdaptable
	{
		private object[] m_properties;

		private bool m_expanded;

		private bool m_selected;

		private bool m_expandedChanging;

		private int m_imageIndex = -1;

		private int m_stateImageIndex = -1;

		private string m_label = string.Empty;

		private string m_hoverText = string.Empty;

		private FontStyle m_fontStyle = FontStyle.Regular;

		private CheckState m_checkState = CheckState.Unchecked;

		private readonly NodeCollection m_children;

		public Node Parent { get; internal set; }

		public bool IsLeaf { get; set; }

		public bool HasChildren => Nodes.Count > 0;

		public NodeCollection Nodes => m_children;

		public object Tag { get; set; }

		public string Label
		{
			get
			{
				return m_label;
			}
			set
			{
				if (string.Compare(value, m_label) != 0)
				{
					m_label = value;
					this.LabelChanged.Raise(this, EventArgs.Empty);
				}
			}
		}

		public bool Expanded
		{
			get
			{
				return m_expanded;
			}
			set
			{
				if (value == m_expanded || m_expandedChanging)
				{
					return;
				}
				try
				{
					m_expandedChanging = true;
					m_expanded = value;
					this.ExpandedChanged.Raise(this, EventArgs.Empty);
				}
				finally
				{
					m_expandedChanging = false;
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
					this.CheckStateChanged.Raise(this, EventArgs.Empty);
				}
			}
		}

		public object[] Properties
		{
			get
			{
				return m_properties;
			}
			set
			{
				if (m_properties != value)
				{
					m_properties = value;
					this.PropertiesChanged.Raise(this, EventArgs.Empty);
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
					this.ImageIndexChanged.Raise(this, EventArgs.Empty);
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
					this.StateImageIndexChanged.Raise(this, EventArgs.Empty);
				}
			}
		}

		public bool Selected
		{
			get
			{
				return m_selected;
			}
			set
			{
				if (m_selected != value)
				{
					m_selected = value;
					this.SelectedChanged.Raise(this, EventArgs.Empty);
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
					this.FontStyleChanged.Raise(this, EventArgs.Empty);
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
					this.HoverTextChanged.Raise(this, EventArgs.Empty);
				}
			}
		}

		internal Node RootLevelAncestor
		{
			get
			{
				Node node = this;
				while (node.Parent != null)
				{
					node = node.Parent;
				}
				return node;
			}
		}

		internal Node Next { get; set; }

		internal Node Previous { get; set; }

		internal bool Visible { get; set; }

		internal int Level { get; set; }

		internal Rectangle HitRect { get; set; }

		internal Rectangle CheckBoxHitRect { get; set; }

		internal Rectangle LabelHitRect { get; set; }

		internal bool Expandable
		{
			get
			{
				foreach (Node node in Nodes)
				{
					if (node.Visible)
					{
						return true;
					}
				}
				return false;
			}
		}

		internal bool NeedsLazyLoad => !IsLeaf && !HasChildren;

		internal ulong VisualPosition { get; set; }

		public event EventHandler LabelChanged;

		public event EventHandler ExpandedChanged;

		public event EventHandler CheckStateChanged;

		public event EventHandler PropertiesChanged;

		public event EventHandler ImageIndexChanged;

		public event EventHandler StateImageIndexChanged;

		public event EventHandler SelectedChanged;

		public event EventHandler FontStyleChanged;

		public event EventHandler HoverTextChanged;

		public Node()
		{
			m_children = new NodeCollection(this);
			Visible = true;
			Level = 1;
			IsLeaf = false;
		}

		public object GetAdapter(Type type)
		{
			if (Tag == null)
			{
				return null;
			}
			Type type2 = Tag.GetType();
			if (type.Equals(type2))
			{
				return Tag;
			}
			return type.IsAssignableFrom(type2) ? Tag : null;
		}

		internal void SetProperty(int index, object value)
		{
			if (index >= m_properties.Length)
			{
				throw new InvalidOperationException("Property index is greater than the number of properties for this node.");
			}
			m_properties[index] = value;
			this.PropertiesChanged.Raise(this, EventArgs.Empty);
		}
	}

	public class NodeCollection : ICollection<Node>, IEnumerable<Node>, IEnumerable
	{
		private readonly Node m_owner;

		private readonly List<Node> m_nodes = new List<Node>();

		public Node Owner => m_owner;

		public int Count => m_nodes.Count;

		public bool IsReadOnly => m_owner != null && m_owner.IsLeaf;

		internal event EventHandler<CancelNodeEventArgs> NodeAdding;

		internal event EventHandler<NodeEventArgs> NodeAdded;

		internal event EventHandler<NodeEventArgs> NodeRemoving;

		internal event EventHandler<NodesRemovingEventArgs> NodesRemoving;

		internal NodeCollection(Node owner)
		{
			m_owner = owner;
		}

		internal void Sort(IComparer<Node> comparer)
		{
			m_nodes.Sort(comparer);
		}

		public IEnumerator<Node> GetEnumerator()
		{
			return m_nodes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_nodes.GetEnumerator();
		}

		public void Add(Node item)
		{
			if (IsReadOnly)
			{
				throw new InvalidOperationException("collection is read only");
			}
			if (!Contains(item))
			{
				if (m_owner != null)
				{
					item.Parent = m_owner;
				}
				CancelNodeEventArgs e = new CancelNodeEventArgs(item);
				if (!this.NodeAdding.RaiseCancellable(this, e))
				{
					m_nodes.Add(item);
					this.NodeAdded.Raise(this, new NodeEventArgs(item));
				}
			}
		}

		public void Clear()
		{
			if (IsReadOnly)
			{
				throw new InvalidOperationException("collection is read only");
			}
			List<Node> list = new List<Node>(m_nodes);
			foreach (Node item in list)
			{
				if (item.HasChildren)
				{
					item.Nodes.Clear();
				}
			}
			this.NodesRemoving.Raise(this, new NodesRemovingEventArgs(Owner, list));
			m_nodes.Clear();
		}

		public bool Contains(Node item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			return m_nodes.Contains(item);
		}

		public void CopyTo(Node[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			for (int i = 0; i < Count; i++)
			{
				array[i + arrayIndex] = m_nodes[i];
			}
		}

		public bool Remove(Node item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (IsReadOnly)
			{
				throw new InvalidOperationException("collection is read only");
			}
			if (item.HasChildren)
			{
				item.Nodes.Clear();
			}
			this.NodeRemoving.Raise(this, new NodeEventArgs(item));
			return m_nodes.Remove(item);
		}
	}

	public class NodeEventArgs : EventArgs
	{
		public Node Node { get; private set; }

		public NodeEventArgs(Node node)
		{
			Node = node;
		}
	}

	public class CanLabelEditEventArgs : NodeEventArgs
	{
		public bool CanEdit { get; set; }

		public CanLabelEditEventArgs(Node node)
			: base(node)
		{
			CanEdit = true;
		}
	}

	public class NodeLabelEditEventArgs : EventArgs
	{
		public Node Node { get; private set; }

		public string Label { get; private set; }

		public bool CancelEdit { get; set; }

		public NodeLabelEditEventArgs(Node node, string label)
		{
			Node = node;
			Label = label;
		}
	}

	public class CanPropertyChangeEventArgs : NodeEventArgs
	{
		public int PropertyIndex { get; private set; }

		public bool CanChange { get; set; }

		public CanPropertyChangeEventArgs(Node node, int propertyIndex)
			: base(node)
		{
			PropertyIndex = propertyIndex;
			CanChange = true;
		}
	}

	public class PropertyChangedEventArgs : EventArgs
	{
		public Node Node { get; private set; }

		public int PropertyIndex { get; private set; }

		public object Value { get; private set; }

		public bool CancelChange { get; set; }

		public PropertyChangedEventArgs(Node node, int propertyIndex, object value)
		{
			Node = node;
			PropertyIndex = propertyIndex;
			Value = value;
		}
	}

	[Flags]
	internal enum NodeChangeTypes
	{
		None = 0,
		Label = 1,
		Expanded = 2,
		CheckState = 4,
		Properties = 8,
		ImageIndex = 0x10,
		StateImageIndex = 0x20,
		Selected = 0x40,
		FontStyle = 0x80,
		HoverText = 0x100
	}

	internal class NodesRemovingEventArgs : EventArgs
	{
		public Node Owner { get; private set; }

		public IEnumerable<Node> Nodes { get; private set; }

		public NodesRemovingEventArgs(Node owner, IEnumerable<Node> nodes)
		{
			Owner = owner;
			Nodes = nodes;
		}
	}

	internal class CancelNodeEventArgs : CancelEventArgs
	{
		public Node Node { get; private set; }

		public CancelNodeEventArgs(Node node)
		{
			Node = node;
		}
	}

	public class NodeRenderer
	{
		public TreeListView Owner { get; private set; }

		public NodeRenderer(TreeListView owner)
		{
			Owner = owner;
		}

		public virtual void DrawBackground(Graphics gfx, Rectangle bounds)
		{
			if (Owner.Control.Enabled)
			{
				using (Brush brush = new SolidBrush(Owner.BackColor))
				{
					gfx.FillRectangle(brush, bounds);
					return;
				}
			}
			using Brush brush2 = new SolidBrush(Owner.DisabledBackColor);
			gfx.FillRectangle(brush2, bounds);
		}

		public virtual void DrawBackground(Node node, Graphics gfx, Rectangle bounds)
		{
			if (Owner.Control.Enabled)
			{
				using (Brush brush = new SolidBrush(Owner.BackColor))
				{
					gfx.FillRectangle(brush, bounds);
					return;
				}
			}
			using Brush brush2 = new SolidBrush(Owner.DisabledBackColor);
			gfx.FillRectangle(brush2, bounds);
		}

		public virtual void DrawLabel(Node node, Graphics gfx, Rectangle bounds, int column)
		{
			string text = ((column == 0) ? node.Label : ((node.Properties != null && node.Properties.Length >= column) ? GetObjectString(node.Properties[column - 1]) : null));
			if (string.IsNullOrEmpty(text))
			{
				text = string.Empty;
			}
			TextFormatFlags textFormatFlags = TextFormatFlags.VerticalCenter;
			if (TextRenderer.MeasureText(gfx, text, Owner.Control.Font).Width > bounds.Width)
			{
				textFormatFlags |= TextFormatFlags.EndEllipsis;
			}
			if (node.Selected && Owner.Control.Enabled)
			{
				using Brush brush = new SolidBrush(Owner.HighlightBackColor);
				gfx.FillRectangle(brush, bounds);
			}
			Color foreColor = (node.Selected ? Owner.HighlightTextColor : Owner.TextColor);
			if (!Owner.Control.Enabled)
			{
				foreColor = Owner.DisabledTextColor;
			}
			Font font = Owner.Control.Font;
			if (node.FontStyle != FontStyle.Regular)
			{
				using (font = new Font(font, node.FontStyle))
				{
					TextRenderer.DrawText(gfx, text, font, bounds, foreColor, textFormatFlags);
					return;
				}
			}
			TextRenderer.DrawText(gfx, text, font, bounds, foreColor, textFormatFlags);
		}

		public virtual void DrawCheckBox(Node node, Graphics gfx, Rectangle bounds)
		{
			bool enabled = Owner.Control.Enabled;
			CheckBoxState state = ((node.CheckState != CheckState.Checked) ? (enabled ? CheckBoxState.UncheckedNormal : CheckBoxState.UncheckedDisabled) : (enabled ? CheckBoxState.CheckedNormal : CheckBoxState.CheckedDisabled));
			CheckBoxRenderer.DrawCheckBox(gfx, bounds.Location, state);
		}

		public virtual void DrawImage(Node node, Graphics gfx, Rectangle bounds)
		{
			if (node.ImageIndex == -1 || Owner.ImageList == null || node.ImageIndex >= Owner.ImageList.Images.Count)
			{
				return;
			}
			using Image image = Owner.ImageList.Images[node.ImageIndex];
			if (Owner.Control.Enabled)
			{
				gfx.DrawImage(image, bounds);
			}
			else
			{
				ControlPaint.DrawImageDisabled(gfx, image, bounds.X, bounds.Y, Owner.DisabledBackColor);
			}
		}

		public virtual void DrawStateImage(Node node, Graphics gfx, Rectangle bounds)
		{
			if (node.StateImageIndex == -1 || Owner.StateImageList == null || node.StateImageIndex >= Owner.StateImageList.Images.Count)
			{
				return;
			}
			using Image image = Owner.StateImageList.Images[node.StateImageIndex];
			if (Owner.Control.Enabled)
			{
				gfx.DrawImage(image, bounds);
			}
			else
			{
				ControlPaint.DrawImageDisabled(gfx, image, bounds.X, bounds.Y, Owner.DisabledBackColor);
			}
		}
	}

	private bool m_autoSizeColumns = false;

	private bool m_disposed;

	private object m_lastHit;

	private int m_sortColumn;

	private int m_updateCount;

	private bool m_addingColumn;

	private bool m_sorting;

	private bool m_needSorting;

	private bool m_checkChanging;

	private bool m_expandingCollapsing;

	private bool m_selectionChanging;

	private SortOrder m_sortOrder;

	private IComparer<Node> m_sorter;

	private bool m_virtualListAvoidSelectionRecursion;

	private ListViewItem m_virtualListSelectionLastHit;

	private Node m_currentEditNode;

	private int m_currentEditColumnIndex;

	private bool m_recursiveCheckBoxes;

	private bool m_doingRecursiveCheckStateChange;

	private readonly Timer m_sortTimer;

	private readonly NodeCollection m_nodes;

	private readonly TheTreeListView m_control;

	private readonly ColumnCollection m_columns;

	private readonly ListViewItemSorter m_listViewItemSorter;

	private readonly TextBox m_editBox;

	private readonly List<ListViewItem> m_insertQueue = new List<ListViewItem>();

	private readonly DefaultSorter m_defaultSorter = new DefaultSorter();

	private readonly Dictionary<string, int> m_columnWidths = new Dictionary<string, int>();

	private readonly Dictionary<Node, ListViewItem> m_itemMap = new Dictionary<Node, ListViewItem>();

	private readonly List<int> m_virtualListOldSelectedIndices = new List<int>();

	private readonly Dictionary<int, ListViewItem> m_virtualItemMap = new Dictionary<int, ListViewItem>();

	private readonly Dictionary<Column, ColumnHeader> m_columnMap = new Dictionary<Column, ColumnHeader>();

	private readonly Dictionary<string, bool> m_dictColumnAllowEditCache = new Dictionary<string, bool>();

	private const string ExceptionTextOnlyAvailableOnVirtualList = "only available in virtual list mode";

	private const string ExceptionTextSortingNotAllowedInVirtualList = "sorting not allowed in virtual list mode";

	private const string ExceptionTextAddingNotAllowedInVirtualList = "adding not allowed in virtual list mode";

	private const string ExceptionTextRemovingNotAllowedInVirtualList = "removing not allowed in virtual list mode";

	public const int InvalidImageIndex = -1;

	public string Name
	{
		get
		{
			return m_control.Name;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new InvalidOperationException("empty or null name not permitted");
			}
			m_control.Name = value;
		}
	}

	public bool AutoSizeColumns
	{
		get
		{
			return m_autoSizeColumns;
		}
		set
		{
			if (m_autoSizeColumns != value)
			{
				m_autoSizeColumns = value;
				if (m_autoSizeColumns)
				{
					m_control.SizeChanged += TheTreeListView_SizeChanged;
				}
				else
				{
					m_control.SizeChanged -= TheTreeListView_SizeChanged;
				}
			}
		}
	}

	public Control Control => m_control;

	public ColumnCollection Columns => m_columns;

	public Style TheStyle => m_control.TheStyle;

	public ColumnHeaderStyle HeaderStyle
	{
		get
		{
			return m_control.HeaderStyle;
		}
		set
		{
			m_control.HeaderStyle = value;
		}
	}

	public bool GridLines
	{
		get
		{
			return m_control.GridLines;
		}
		set
		{
			m_control.GridLines = value;
		}
	}

	public NodeCollection Nodes => m_nodes;

	public IComparer<Node> NodeSorter
	{
		get
		{
			return m_sorter ?? m_defaultSorter;
		}
		set
		{
			m_sorter = value;
		}
	}

	public int SortColumn
	{
		get
		{
			if (m_control.TheStyle == Style.VirtualList)
			{
				throw new InvalidOperationException("sorting not allowed in virtual list mode");
			}
			return m_sortColumn;
		}
		set
		{
			if (m_control.TheStyle == Style.VirtualList)
			{
				throw new InvalidOperationException("sorting not allowed in virtual list mode");
			}
			if (value != m_sortColumn)
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				if (value > m_control.Columns.Count)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				m_sortColumn = value;
				Sort();
			}
		}
	}

	public SortOrder SortOrder
	{
		get
		{
			if (m_control.TheStyle == Style.VirtualList)
			{
				throw new InvalidOperationException("sorting not allowed in virtual list mode");
			}
			return m_sortOrder;
		}
		set
		{
			if (m_control.TheStyle == Style.VirtualList)
			{
				throw new InvalidOperationException("sorting not allowed in virtual list mode");
			}
			m_sortOrder = value;
			Sort();
		}
	}

	public bool AllowLabelEdit { get; set; }

	public bool AllowPropertyEdit { get; set; }

	public bool ShowNodeHoverText
	{
		get
		{
			return m_control.ShowItemToolTips;
		}
		set
		{
			m_control.ShowItemToolTips = value;
		}
	}

	public ImageList ImageList
	{
		get
		{
			return m_control.SmallImageList;
		}
		set
		{
			m_control.SmallImageList = value;
		}
	}

	public ImageList StateImageList
	{
		get
		{
			return m_control.StateImageList;
		}
		set
		{
			m_control.StateImageList = value;
		}
	}

	public bool RecursiveCheckBoxes
	{
		get
		{
			return m_recursiveCheckBoxes;
		}
		set
		{
			m_recursiveCheckBoxes = value;
		}
	}

	public IEnumerable<Node> SelectedNodes
	{
		get
		{
			if (m_control.TheStyle != Style.VirtualList)
			{
				foreach (ListViewItem lstItem in m_control.SelectedItems)
				{
					if (lstItem.Tag != null && lstItem.Tag is Node)
					{
						yield return (Node)lstItem.Tag;
					}
				}
				yield break;
			}
			foreach (int index in m_control.SelectedIndices)
			{
				ListViewItem lstItem2 = GetVirtualListItemAtIndexOrLookup(index);
				if (lstItem2.Tag != null && lstItem2.Tag is Node)
				{
					yield return (Node)lstItem2.Tag;
				}
			}
		}
		set
		{
			if (m_control.TheStyle != Style.VirtualList)
			{
				List<Node> list = SelectedNodes.ToList();
				foreach (Node item in list)
				{
					if (m_itemMap.TryGetValue(item, out var value2))
					{
						value2.Selected = false;
					}
				}
				if (value == null)
				{
					return;
				}
				{
					foreach (Node item2 in value)
					{
						if (m_itemMap.TryGetValue(item2, out var value3))
						{
							value3.Selected = true;
						}
					}
					return;
				}
			}
			throw new NotImplementedException("not added... yet");
		}
	}

	public int VirtualListSize
	{
		get
		{
			if (m_control.TheStyle != Style.VirtualList)
			{
				throw new InvalidOperationException("only available in virtual list mode");
			}
			return m_control.VirtualListSize;
		}
		set
		{
			if (m_control.TheStyle != Style.VirtualList)
			{
				throw new InvalidOperationException("only available in virtual list mode");
			}
			m_control.VirtualListSize = value;
		}
	}

	public object LastHit
	{
		get
		{
			return m_lastHit;
		}
		private set
		{
			if (value != m_lastHit)
			{
				m_lastHit = value;
				this.LastHitChanged.Raise(this, EventArgs.Empty);
			}
		}
	}

	public NodeRenderer Renderer
	{
		get
		{
			return m_control.Renderer;
		}
		set
		{
			m_control.Renderer = value;
		}
	}

	public Node TopItem
	{
		get
		{
			ListViewItem topItem = m_control.TopItem;
			return (topItem == null) ? null : (topItem.Tag as Node);
		}
		set
		{
			if (m_itemMap.TryGetValue(value, out var value2))
			{
				m_control.TopItem = value2;
				if (m_control.TopItem != value2)
				{
					m_control.TopItem = value2;
				}
			}
		}
	}

	public bool AllowDrop
	{
		get
		{
			return m_control.AllowDrop;
		}
		set
		{
			if (m_control.AllowDrop != value)
			{
				if (!value && m_control.AllowDrop)
				{
					m_control.DragEnter -= ControlDragEnter;
					m_control.DragOver -= ControlDragOver;
					m_control.DragLeave -= ControlDragLeave;
					m_control.DragDrop -= ControlDragDrop;
					m_control.ItemDrag -= ControlItemDrag;
				}
				m_control.AllowDrop = value;
				if (value)
				{
					m_control.DragEnter += ControlDragEnter;
					m_control.DragOver += ControlDragOver;
					m_control.DragLeave += ControlDragLeave;
					m_control.DragDrop += ControlDragDrop;
					m_control.ItemDrag += ControlItemDrag;
				}
			}
		}
	}

	internal bool UseInsertQueue { get; set; }

	public string PersistedSettings
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("Columns");
			xmlDocument.AppendChild(xmlElement);
			foreach (KeyValuePair<string, int> columnWidth in m_columnWidths)
			{
				XmlElement xmlElement2 = xmlDocument.CreateElement("Column");
				xmlElement.AppendChild(xmlElement2);
				xmlElement2.SetAttribute("Name", columnWidth.Key);
				xmlElement2.SetAttribute("Width", columnWidth.Value.ToString());
			}
			return xmlDocument.InnerXml;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(value);
			XmlElement documentElement = xmlDocument.DocumentElement;
			if (documentElement == null || documentElement.Name != "Columns")
			{
				throw new ArgumentException("invalid TreeListView settings");
			}
			XmlNodeList xmlNodeList = documentElement.SelectNodes("Column");
			if (xmlNodeList == null)
			{
				return;
			}
			foreach (XmlElement item in xmlNodeList)
			{
				string attribute = item.GetAttribute("Name");
				string attribute2 = item.GetAttribute("Width");
				if (!string.IsNullOrEmpty(attribute2) && int.TryParse(attribute2, out var result))
				{
					m_columnWidths[attribute] = result;
				}
			}
			try
			{
				m_control.SuspendLayout();
				foreach (Column column in Columns)
				{
					SetColumnWidth(column);
				}
			}
			finally
			{
				m_control.ResumeLayout();
			}
		}
	}

	public Color TextColor
	{
		get
		{
			return m_control.TextColor;
		}
		set
		{
			m_control.TextColor = value;
		}
	}

	public Color ModifiableTextColor
	{
		get
		{
			return m_control.ModifiableTextColor;
		}
		set
		{
			m_control.ModifiableTextColor = value;
		}
	}

	public Color HighlightTextColor
	{
		get
		{
			return m_control.HighlightTextColor;
		}
		set
		{
			m_control.HighlightTextColor = value;
		}
	}

	public Color ModifiableHighlightTextColor
	{
		get
		{
			return m_control.ModifiableHighlightTextColor;
		}
		set
		{
			m_control.ModifiableHighlightTextColor = value;
		}
	}

	public Color DisabledTextColor
	{
		get
		{
			return m_control.DisabledTextColor;
		}
		set
		{
			m_control.DisabledTextColor = value;
		}
	}

	public Color BackColor
	{
		get
		{
			return m_control.BackColor;
		}
		set
		{
			m_control.BackColor = value;
		}
	}

	public Color HighlightBackColor
	{
		get
		{
			return m_control.HighlightBackColor;
		}
		set
		{
			m_control.HighlightBackColor = value;
		}
	}

	public Color DisabledBackColor
	{
		get
		{
			return m_control.DisabledBackColor;
		}
		set
		{
			m_control.DisabledBackColor = value;
		}
	}

	public Color GridLinesColor
	{
		get
		{
			return m_control.GridLinesColor;
		}
		set
		{
			m_control.GridLinesColor = value;
		}
	}

	public ControlGradient ExpanderGradient
	{
		get
		{
			return m_control.ExpanderGradient;
		}
		set
		{
			m_control.ExpanderGradient = value;
		}
	}

	public Pen ExpanderPen
	{
		get
		{
			return m_control.ExpanderPen;
		}
		set
		{
			m_control.ExpanderPen = value;
		}
	}

	public Pen HierarchyLinePen
	{
		get
		{
			return m_control.HierarchyLinePen;
		}
		set
		{
			m_control.HierarchyLinePen = value;
		}
	}

	public event EventHandler<NodeEventArgs> NodeLazyLoad;

	public event EventHandler LastHitChanged;

	public event EventHandler<ItemSelectedEventArgs<Node>> NodeSelected;

	public event EventHandler<NodeCheckedEventArgs> NodeChecked;

	public event EventHandler<RetrieveVirtualNodeEventArgs> RetrieveVirtualNode;

	public event EventHandler<NodeEventArgs> NodeExpandedChanged;

	public event EventHandler<DragEventArgs> DragEnter;

	public event EventHandler<DragEventArgs> DragOver;

	public event EventHandler DragLeave;

	public event EventHandler<DragEventArgs> DragDrop;

	public event EventHandler<NodeDragEventArgs> NodeDrag;

	public event EventHandler<CanLabelEditEventArgs> CanLabelEdit;

	public event EventHandler<NodeLabelEditEventArgs> AfterNodeLabelEdit;

	public event EventHandler<CanPropertyChangeEventArgs> CanPropertyChange;

	public event EventHandler<PropertyChangedEventArgs> PropertyChanged;

	public TreeListView()
		: this(Style.TreeList)
	{
	}

	public TreeListView(Style style)
	{
		m_nodes = new NodeCollection(null);
		m_nodes.NodeAdding += NodeCollectionNodeAdding;
		m_nodes.NodeAdded += NodeCollectionNodeAdded;
		m_nodes.NodeRemoving += NodeCollectionNodeRemoving;
		m_nodes.NodesRemoving += NodeCollectionNodesRemoving;
		m_columns = new ColumnCollection();
		m_columns.ColumnAdding += ColumnsColumnAdding;
		m_columns.ColumnRemoving += ColumnsColumnRemoving;
		m_columns.ColumnClearAll += ColumnsColumnClearAll;
		m_control = new TheTreeListView(style, this)
		{
			View = View.Details,
			FullRowSelect = true,
			GridLines = true,
			AllowColumnReorder = false,
			LabelEdit = false,
			SmallImageList = ResourceUtil.GetImageList16(),
			StateImageList = ResourceUtil.GetImageList16(),
			CheckBoxes = false
		};
		m_control.MouseDown += ControlMouseDown;
		m_control.MouseUp += ControlMouseUp;
		m_control.ColumnWidthChanged += ControlColumnWidthChanged;
		m_control.MouseClick += ControlMouseClick;
		m_control.MouseDoubleClick += ControlMouseDoubleClick;
		m_control.Scroll += ControlScroll;
		if (style == Style.VirtualList)
		{
			m_control.VirtualMode = true;
			m_control.RetrieveVirtualItem += ControlRetrieveVirtualItem;
			m_control.SelectedIndexChanged += ControlSelectedIndexChanged;
		}
		else
		{
			m_control.ColumnClick += ControlColumnClick;
			m_control.ItemSelectionChanged += ControlItemSelectionChanged;
			m_sortTimer = new Timer
			{
				Interval = 200
			};
			m_sortTimer.Tick += SortTimerTick;
			m_sortTimer.Start();
		}
		m_editBox = new TextBox
		{
			Parent = m_control
		};
		m_editBox.LostFocus += EditBoxLostFocus;
		m_editBox.KeyPress += EditBoxKeyPress;
		m_editBox.Hide();
		m_listViewItemSorter = new ListViewItemSorter();
		m_control.ListViewItemSorter = m_listViewItemSorter;
	}

	public static implicit operator Control(TreeListView treeListView)
	{
		return treeListView.m_control;
	}

	private void TheTreeListView_SizeChanged(object sender, EventArgs e)
	{
		int width = m_control.ClientRectangle.Width;
		int num = 0;
		foreach (Column column in Columns)
		{
			column.Width = -2;
			num += column.Width;
		}
		if (num >= width)
		{
			return;
		}
		float num2 = (float)width / (float)num;
		foreach (Column column2 in Columns)
		{
			column2.Width = (int)((float)column2.Width * num2);
		}
	}

	public void Sort()
	{
		if (m_sorting)
		{
			return;
		}
		if (m_updateCount != 0)
		{
			m_needSorting = true;
			return;
		}
		try
		{
			m_sorting = true;
			if (m_control.TheStyle != Style.VirtualList)
			{
				ulong number = 0uL;
				SetupHierarchy(Nodes, ref number, NodeSorter);
				m_control.Sort();
			}
		}
		finally
		{
			m_sorting = false;
			m_needSorting = false;
		}
	}

	public void BeginLabelEdit(Node node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (AllowLabelEdit && m_itemMap.TryGetValue(node, out var _))
		{
			CanLabelEditEventArgs e = new CanLabelEditEventArgs(node);
			this.CanLabelEdit.Raise(this, e);
			if (e.CanEdit)
			{
				m_editBox.Bounds = node.LabelHitRect;
				m_currentEditNode = node;
				m_currentEditColumnIndex = 0;
				m_editBox.Text = node.Label;
				m_editBox.Show();
				m_editBox.Focus();
			}
		}
	}

	public Node GetNodeAt(Point clientPoint)
	{
		ListViewItem itemAt = m_control.GetItemAt(clientPoint.X, clientPoint.Y);
		return (itemAt == null) ? null : (itemAt.Tag as Node);
	}

	public int GetNodeColumnIndexAt(Point clientPoint)
	{
		ListViewItem itemAt = m_control.GetItemAt(clientPoint.X, clientPoint.Y);
		if (itemAt == null)
		{
			return -1;
		}
		ListViewItem.ListViewSubItem subItemAt = itemAt.GetSubItemAt(clientPoint.X, clientPoint.Y);
		if (subItemAt == null)
		{
			return -1;
		}
		int result = 0;
		if (itemAt.Bounds != subItemAt.Bounds)
		{
			result = itemAt.SubItems.IndexOf(subItemAt);
		}
		return result;
	}

	public int GetNodeIndex(Node node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		ListViewItem value;
		return m_itemMap.TryGetValue(node, out value) ? value.Index : (-1);
	}

	public Node GetNodeAtIndex(int index)
	{
		ListViewItem listViewItem;
		if (m_control.TheStyle != Style.VirtualList)
		{
			if (index < 0 || index >= m_control.Items.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			listViewItem = m_control.Items[index];
		}
		else
		{
			listViewItem = GetVirtualListItemAtIndexOrLookup(index);
		}
		return (Node)listViewItem.Tag;
	}

	public void ExpandAll()
	{
		try
		{
			m_expandingCollapsing = true;
			BeginUpdate();
			List<Node> list = new List<Node>(Nodes);
			foreach (Node item in list)
			{
				ExpandAll(item);
			}
		}
		finally
		{
			m_expandingCollapsing = false;
			EndUpdate();
		}
	}

	public void CollapseAll()
	{
		try
		{
			m_expandingCollapsing = true;
			BeginUpdate();
			List<Node> list = new List<Node>(Nodes);
			foreach (Node item in list)
			{
				CollapseAll(item);
			}
		}
		finally
		{
			m_expandingCollapsing = false;
			EndUpdate();
		}
	}

	public void ClearAll()
	{
		try
		{
			BeginUpdate();
			m_insertQueue.Clear();
			m_control.Columns.Clear();
			m_control.ResetWorkaroundList();
			m_control.Items.Clear();
			m_columnMap.Clear();
			m_itemMap.Clear();
			m_virtualItemMap.Clear();
			m_virtualListOldSelectedIndices.Clear();
			Columns.Clear();
			Nodes.Clear();
		}
		finally
		{
			EnsureEditingTerminated();
			EndUpdate();
		}
	}

	public void Show(Node node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		for (Node parent = node.Parent; parent != null; parent = parent.Parent)
		{
			parent.Expanded = true;
			ExpandNode(parent);
		}
	}

	public void EnsureVisible(Node node)
	{
		Show(node);
		if (m_itemMap.TryGetValue(node, out var value))
		{
			m_control.EnsureVisible(value.Index);
		}
	}

	public void ScrollIntoView(Node node)
	{
		EnsureVisible(node);
	}

	public void BeginUpdate()
	{
		if (m_updateCount == 0)
		{
			m_control.BeginUpdate();
		}
		m_updateCount++;
	}

	public void EndUpdate()
	{
		m_updateCount--;
		if (m_updateCount < 0)
		{
			m_updateCount = 0;
		}
		if (m_updateCount == 0)
		{
			Sort();
			m_control.EndUpdate();
		}
	}

	public void Invalidate(Node node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (m_itemMap.TryGetValue(node, out var value))
		{
			m_control.Invalidate(value.Bounds);
		}
	}

	public void DoDragDrop(object data, DragDropEffects allowedEffects)
	{
		if (!AllowDrop)
		{
			throw new InvalidOperationException("drag and drop not allowed");
		}
		m_control.DoDragDrop(data, allowedEffects);
	}

	internal void FlushInsertQueue()
	{
		if (m_insertQueue.Count <= 0)
		{
			return;
		}
		try
		{
			m_checkChanging = true;
			m_control.Items.AddRange(m_insertQueue.ToArray());
		}
		finally
		{
			m_checkChanging = false;
			m_insertQueue.Clear();
		}
	}

	public void Dispose()
	{
		if (m_disposed)
		{
			return;
		}
		try
		{
			m_control.Dispose();
			m_sortTimer.Stop();
			m_sortTimer.Dispose();
		}
		finally
		{
			m_disposed = true;
		}
	}

	public object GetAdapter(Type type)
	{
		return type.Equals(typeof(Control)) ? Control : null;
	}

	private void ControlMouseDown(object sender, MouseEventArgs e)
	{
		EnsureEditingTerminated();
		Point lastHit = new Point(e.X, e.Y);
		SetLastHit(lastHit);
	}

	private void ControlMouseUp(object sender, MouseEventArgs e)
	{
		Point lastHit = new Point(e.X, e.Y);
		SetLastHit(lastHit);
	}

	private void ControlColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
	{
		if (!m_addingColumn)
		{
			ColumnHeader columnHeader = m_control.Columns[e.ColumnIndex];
			m_columnWidths[columnHeader.Text] = columnHeader.Width;
			((Column)columnHeader.Tag).Width = columnHeader.Width;
		}
	}

	private void ControlRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
	{
		e.Item = GetVirtualListItemAtIndexOrLookup(e.ItemIndex);
	}

	private void ControlColumnClick(object sender, ColumnClickEventArgs e)
	{
		if (m_sortColumn == e.Column)
		{
			m_sortOrder = ((m_sortOrder != SortOrder.Ascending) ? SortOrder.Ascending : SortOrder.Descending);
		}
		m_sortColumn = e.Column;
		m_defaultSorter.SortOrder = m_sortOrder;
		Sort();
	}

	private void ControlItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
	{
		if (m_selectionChanging)
		{
			return;
		}
		try
		{
			m_selectionChanging = true;
			Node node = e.Item.Tag.As<Node>();
			if (node != null)
			{
				node.Selected = e.Item.Selected;
				this.NodeSelected.Raise(this, new ItemSelectedEventArgs<Node>(node, e.IsSelected));
			}
		}
		finally
		{
			m_selectionChanging = false;
		}
	}

	private void ControlSelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_virtualListAvoidSelectionRecursion)
		{
			return;
		}
		if (m_control.TheStyle != Style.VirtualList)
		{
			throw new InvalidOperationException("only available in virtual list mode");
		}
		try
		{
			m_virtualListAvoidSelectionRecursion = true;
			bool flag = true;
			Node node = LastHit.As<Node>();
			Keys modifierKeys = Control.ModifierKeys;
			if (modifierKeys == Keys.Control)
			{
				flag = false;
			}
			if (flag)
			{
				foreach (int virtualListOldSelectedIndex in m_virtualListOldSelectedIndices)
				{
					ListViewItem virtualListItemAtIndexOrLookup = GetVirtualListItemAtIndexOrLookup(virtualListOldSelectedIndex);
					virtualListItemAtIndexOrLookup.Selected = false;
					Node node2 = (Node)virtualListItemAtIndexOrLookup.Tag;
					node2.Selected = false;
					this.NodeSelected.Raise(this, new ItemSelectedEventArgs<Node>(node2, node2.Selected));
				}
			}
			foreach (int selectedIndex in m_control.SelectedIndices)
			{
				ListViewItem virtualListItemAtIndexOrLookup2 = GetVirtualListItemAtIndexOrLookup(selectedIndex);
				Node node3 = (Node)virtualListItemAtIndexOrLookup2.Tag;
				node3.Selected = true;
				this.NodeSelected.Raise(this, new ItemSelectedEventArgs<Node>(node3, node3.Selected));
			}
			if (modifierKeys == Keys.Shift)
			{
				if (m_virtualListSelectionLastHit != null && node != null)
				{
					int index2 = m_virtualListSelectionLastHit.Index;
					int nodeIndex = GetNodeIndex(node);
					int num = Math.Min(index2, nodeIndex);
					int num2 = Math.Max(index2, nodeIndex);
					for (int i = num; i <= num2; i++)
					{
						ListViewItem virtualListItemAtIndexOrLookup3 = GetVirtualListItemAtIndexOrLookup(i);
						virtualListItemAtIndexOrLookup3.Selected = true;
						Node node4 = (Node)virtualListItemAtIndexOrLookup3.Tag;
						node4.Selected = true;
						this.NodeSelected.Raise(this, new ItemSelectedEventArgs<Node>(node4, node4.Selected));
					}
				}
			}
			else if (node != null)
			{
				m_itemMap.TryGetValue(node, out m_virtualListSelectionLastHit);
			}
			m_virtualListOldSelectedIndices.Clear();
			m_virtualListOldSelectedIndices.AddRange(m_control.SelectedIndices.Cast<int>());
		}
		finally
		{
			m_virtualListAvoidSelectionRecursion = false;
		}
	}

	private void ControlDragEnter(object sender, DragEventArgs e)
	{
		this.DragEnter.Raise(this, e);
	}

	private void ControlDragOver(object sender, DragEventArgs e)
	{
		Point lastHit = m_control.PointToClient(new Point(e.X, e.Y));
		SetLastHit(lastHit);
		this.DragOver.Raise(this, e);
	}

	private void ControlDragLeave(object sender, EventArgs e)
	{
		this.DragLeave.Raise(this, e);
	}

	private void ControlDragDrop(object sender, DragEventArgs e)
	{
		Point lastHit = m_control.PointToClient(new Point(e.X, e.Y));
		SetLastHit(lastHit);
		this.DragDrop.Raise(this, e);
	}

	private void ControlItemDrag(object sender, ItemDragEventArgs e)
	{
		ListViewItem listViewItem = e.Item.As<ListViewItem>();
		if (listViewItem != null)
		{
			Node node = listViewItem.Tag.As<Node>();
			if (node != null)
			{
				this.NodeDrag.Raise(this, new NodeDragEventArgs(node, e.Button));
			}
		}
	}

	private void ControlMouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			HandleLeftMouseClick(e);
		}
	}

	private void ControlMouseDoubleClick(object sender, MouseEventArgs e)
	{
		Node nodeAt = GetNodeAt(e.Location);
		if (nodeAt == null)
		{
			return;
		}
		int nodeColumnIndexAt = GetNodeColumnIndexAt(e.Location);
		if (nodeColumnIndexAt < 0)
		{
			return;
		}
		ListViewHitTestInfo listViewHitTestInfo = m_control.HitTest(e.X, e.Y);
		if (nodeColumnIndexAt == 0)
		{
			if (AllowLabelEdit && nodeAt.LabelHitRect.Contains(e.Location))
			{
				BeginLabelEdit(nodeAt);
			}
		}
		else if (AllowPropertyEdit || m_columns.ElementAt(nodeColumnIndexAt).AllowPropertyEdit)
		{
			CanPropertyChangeEventArgs e2 = new CanPropertyChangeEventArgs(nodeAt, nodeColumnIndexAt - 1);
			this.CanPropertyChange.Raise(this, e2);
			if (e2.CanChange)
			{
				m_editBox.Bounds = listViewHitTestInfo.SubItem.Bounds;
				m_currentEditNode = nodeAt;
				m_currentEditColumnIndex = nodeColumnIndexAt;
				m_editBox.Text = listViewHitTestInfo.SubItem.Text;
				m_editBox.Show();
				m_editBox.Focus();
			}
		}
	}

	private void ControlScroll(object sender, ScrollEventArgs e)
	{
		EnsureEditingTerminated();
	}

	private void EditBoxLostFocus(object sender, EventArgs e)
	{
		EnsureEditingTerminated();
	}

	private void EditBoxKeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar != '\r')
		{
			return;
		}
		try
		{
			EnsureEditingTerminated();
		}
		finally
		{
			e.Handled = true;
		}
	}

	private void ColumnsColumnAdding(object sender, CancelColumnEventArgs e)
	{
		e.Cancel = !AddColumn(e.Column);
	}

	private void ColumnsColumnRemoving(object sender, ColumnEventArgs e)
	{
		RemoveColumn(e.Column);
	}

	private void ColumnsColumnClearAll(object sender, EventArgs e)
	{
		ClearColumns();
	}

	private bool AddColumn(Column column)
	{
		try
		{
			m_addingColumn = true;
			if (m_columnMap.ContainsKey(column))
			{
				return false;
			}
			ColumnHeader columnHeader = new ColumnHeader
			{
				Text = column.Label,
				Width = column.Width,
				Tag = column
			};
			m_control.Columns.Add(columnHeader);
			m_columnMap.Add(column, columnHeader);
			if (m_columnWidths.ContainsKey(column.Label))
			{
				column.Width = m_columnWidths[column.Label];
				columnHeader.Width = column.Width;
			}
			else
			{
				m_columnWidths[column.Label] = column.Width;
			}
			if (m_dictColumnAllowEditCache.ContainsKey(column.Label))
			{
				column.AllowPropertyEdit = m_dictColumnAllowEditCache[column.Label];
			}
			else
			{
				m_dictColumnAllowEditCache[column.Label] = column.AllowPropertyEdit;
			}
			column.LabelChanged += ColumnLabelChanged;
			column.WidthChanged += ColumnWidthChanged;
			column.AllowPropertyEditChanged += ColumnAllowPropertyEditChanged;
			return true;
		}
		finally
		{
			m_addingColumn = false;
		}
	}

	private void ColumnLabelChanged(object sender, EventArgs e)
	{
		Column column = (Column)sender;
		if (m_columnMap.TryGetValue(column, out var value))
		{
			value.Text = column.Label;
		}
	}

	private void ColumnWidthChanged(object sender, EventArgs e)
	{
		Column column = (Column)sender;
		if (m_columnMap.TryGetValue(column, out var value))
		{
			value.Width = column.Width;
		}
	}

	private void ColumnAllowPropertyEditChanged(object sender, EventArgs e)
	{
		Column column = (Column)sender;
		m_dictColumnAllowEditCache[column.Label] = column.AllowPropertyEdit;
	}

	private void RemoveColumn(Column column)
	{
		if (m_columnMap.TryGetValue(column, out var value))
		{
			column.LabelChanged -= ColumnLabelChanged;
			column.WidthChanged -= ColumnWidthChanged;
			m_control.Columns.Remove(value);
			m_columnMap.Remove(column);
		}
	}

	private void ClearColumns()
	{
		m_control.Columns.Clear();
		m_columnMap.Clear();
	}

	private void NodeCollectionNodeAdding(object sender, CancelNodeEventArgs e)
	{
		e.Cancel = !AddNode(e.Node);
	}

	private void NodeCollectionNodeAdded(object sender, NodeEventArgs e)
	{
		Sort();
	}

	private void NodeCollectionNodeRemoving(object sender, NodeEventArgs e)
	{
		RemoveNode(e.Node);
	}

	private void NodeCollectionNodesRemoving(object sender, NodesRemovingEventArgs e)
	{
		RemoveNodes(e.Owner, e.Nodes);
	}

	private bool AddNode(Node node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (m_control.TheStyle == Style.VirtualList)
		{
			throw new InvalidOperationException("adding not allowed in virtual list mode");
		}
		node.Visible = true;
		if (m_itemMap.ContainsKey(node))
		{
			return false;
		}
		if (!node.Visible)
		{
			return true;
		}
		ListViewItem listViewItem = CreateListViewItem(node, m_control.Columns.Count);
		if (UseInsertQueue)
		{
			m_insertQueue.Add(listViewItem);
		}
		else
		{
			m_control.Items.Add(listViewItem);
		}
		m_needSorting = true;
		m_itemMap.Add(node, listViewItem);
		node.LabelChanged += NodePropertyLabelChanged;
		node.ExpandedChanged += NodePropertyExpandedChanged;
		node.CheckStateChanged += NodePropertyCheckStateChanged;
		node.PropertiesChanged += NodePropertyPropertiesChanged;
		node.ImageIndexChanged += NodePropertyImageIndexChanged;
		node.StateImageIndexChanged += NodePropertyStateImageIndexChanged;
		node.SelectedChanged += NodePropertySelectedChanged;
		node.FontStyleChanged += NodePropertyFontStyleChanged;
		node.HoverTextChanged += NodePropertyHoverTextChanged;
		node.Nodes.NodeAdding += NodeCollectionNodeAdding;
		node.Nodes.NodeAdded += NodeCollectionNodeAdded;
		node.Nodes.NodeRemoving += NodeCollectionNodeRemoving;
		node.Nodes.NodesRemoving += NodeCollectionNodesRemoving;
		if (node.Expanded)
		{
			foreach (Node node2 in node.Nodes)
			{
				AddNode(node2);
			}
		}
		return true;
	}

	private void UpdateNode(Node node, NodeChangeTypes changeTypes)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (!m_itemMap.TryGetValue(node, out var value))
		{
			return;
		}
		bool flag = !m_selectionChanging;
		try
		{
			if (flag)
			{
				BeginUpdate();
			}
			if (IsSet(NodeChangeTypes.Label, changeTypes))
			{
				value.Text = node.Label;
			}
			if (IsSet(NodeChangeTypes.Expanded, changeTypes))
			{
				ExpandOrCollapse(node);
			}
			if (!m_checkChanging && IsSet(NodeChangeTypes.CheckState, changeTypes))
			{
				value.Checked = node.CheckState == CheckState.Checked;
			}
			if (IsSet(NodeChangeTypes.Properties, changeTypes))
			{
				UpdateProperties(node, value, Columns.Count);
			}
			if (IsSet(NodeChangeTypes.ImageIndex, changeTypes))
			{
				value.ImageIndex = node.ImageIndex;
			}
			if (!m_selectionChanging && IsSet(NodeChangeTypes.Selected, changeTypes))
			{
				value.Selected = node.Selected;
			}
			if (IsSet(NodeChangeTypes.FontStyle, changeTypes))
			{
				Invalidate(node);
			}
			if (IsSet(NodeChangeTypes.HoverText, changeTypes))
			{
				value.ToolTipText = node.HoverText;
				Invalidate(node);
			}
		}
		finally
		{
			if (flag)
			{
				EndUpdate();
			}
		}
	}

	private void RemoveNode(Node node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (m_control.TheStyle == Style.VirtualList)
		{
			throw new InvalidOperationException("removing not allowed in virtual list mode");
		}
		if (m_itemMap.TryGetValue(node, out var value))
		{
			node.Nodes.NodeAdding -= NodeCollectionNodeAdding;
			node.Nodes.NodeAdded -= NodeCollectionNodeAdded;
			node.Nodes.NodeRemoving -= NodeCollectionNodeRemoving;
			node.Nodes.NodesRemoving -= NodeCollectionNodesRemoving;
			node.LabelChanged -= NodePropertyLabelChanged;
			node.ExpandedChanged -= NodePropertyExpandedChanged;
			node.CheckStateChanged -= NodePropertyCheckStateChanged;
			node.PropertiesChanged -= NodePropertyPropertiesChanged;
			node.ImageIndexChanged -= NodePropertyImageIndexChanged;
			node.StateImageIndexChanged -= NodePropertyStateImageIndexChanged;
			node.SelectedChanged -= NodePropertySelectedChanged;
			node.FontStyleChanged -= NodePropertyFontStyleChanged;
			node.HoverTextChanged -= NodePropertyHoverTextChanged;
			try
			{
				m_selectionChanging = true;
				node.Selected = false;
				value.Selected = false;
			}
			finally
			{
				m_selectionChanging = false;
			}
			m_control.Items.Remove(value);
			m_itemMap.Remove(node);
			m_needSorting = true;
		}
	}

	private void RemoveNodes(Node node, IEnumerable<Node> nodes)
	{
		bool expandingCollapsing = m_expandingCollapsing;
		try
		{
			m_needSorting = true;
			m_expandingCollapsing = true;
			try
			{
				BeginUpdate();
				foreach (Node node2 in nodes)
				{
					RemoveNode(node2);
				}
				if (node != null && node.Expanded)
				{
					node.Expanded = false;
					InvalidateNode(node);
				}
			}
			finally
			{
				EndUpdate();
			}
		}
		finally
		{
			m_expandingCollapsing = expandingCollapsing;
		}
	}

	private void NodePropertyLabelChanged(object sender, EventArgs e)
	{
		UpdateNode((Node)sender, NodeChangeTypes.Label);
	}

	private void NodePropertyExpandedChanged(object sender, EventArgs e)
	{
		Node node = (Node)sender;
		this.NodeExpandedChanged.Raise(this, new NodeEventArgs(node));
		if (!m_expandingCollapsing && !LazyLoadNode(node) && (node.Expandable || !node.IsLeaf))
		{
			ExpandOrCollapse(node);
		}
	}

	private void NodePropertyCheckStateChanged(object sender, EventArgs e)
	{
		Node node = (Node)sender;
		UpdateNode(node, NodeChangeTypes.CheckState);
		this.NodeChecked.Raise(this, new NodeCheckedEventArgs(node));
		if (m_recursiveCheckBoxes && TheStyle == Style.CheckedTreeList && !m_doingRecursiveCheckStateChange)
		{
			m_doingRecursiveCheckStateChange = true;
			SetCheckStateRecursive(node.Nodes, node.CheckState);
			m_doingRecursiveCheckStateChange = false;
		}
	}

	private void SetCheckStateRecursive(IEnumerable<Node> nodes, CheckState checkState)
	{
		foreach (Node item in new List<Node>(nodes))
		{
			item.CheckState = checkState;
			this.NodeChecked.Raise(this, new NodeCheckedEventArgs(item));
			SetCheckStateRecursive(item.Nodes, checkState);
		}
	}

	private void NodePropertyPropertiesChanged(object sender, EventArgs e)
	{
		UpdateNode((Node)sender, NodeChangeTypes.Properties);
	}

	private void NodePropertyImageIndexChanged(object sender, EventArgs e)
	{
		UpdateNode((Node)sender, NodeChangeTypes.ImageIndex);
	}

	private void NodePropertyStateImageIndexChanged(object sender, EventArgs e)
	{
		UpdateNode((Node)sender, NodeChangeTypes.StateImageIndex);
	}

	private void NodePropertySelectedChanged(object sender, EventArgs e)
	{
		UpdateNode((Node)sender, NodeChangeTypes.Selected);
	}

	private void NodePropertyFontStyleChanged(object sender, EventArgs e)
	{
		UpdateNode((Node)sender, NodeChangeTypes.FontStyle);
	}

	private void NodePropertyHoverTextChanged(object sender, EventArgs e)
	{
		UpdateNode((Node)sender, NodeChangeTypes.HoverText);
	}

	private static void SetupHierarchy(NodeCollection nodes, ref ulong number, IComparer<Node> comparer)
	{
		nodes.Sort(comparer);
		foreach (Node node in nodes)
		{
			node.VisualPosition = ++number;
			SetupNodeHierarchy(node, ref number, comparer);
		}
	}

	private static void SetupNodeHierarchy(Node node, ref ulong number, IComparer<Node> comparer)
	{
		if (node == null)
		{
			return;
		}
		if (node.Parent != null)
		{
			node.Level = node.Parent.Level + 1;
			if (!node.Parent.Visible)
			{
				node.Visible = false;
			}
		}
		else
		{
			node.Level = 1;
		}
		if (!node.HasChildren)
		{
			return;
		}
		node.Nodes.Sort(comparer);
		Node node2 = null;
		foreach (Node node3 in node.Nodes)
		{
			node3.Previous = node2;
			node3.Next = null;
			if (node2 != null)
			{
				node2.Next = node3;
			}
			node2 = node3;
			node3.VisualPosition = ++number;
			SetupNodeHierarchy(node3, ref number, comparer);
		}
	}

	private void SetLastHit(Point clientPoint)
	{
		bool flag = false;
		try
		{
			object itemAt = m_control.GetItemAt(clientPoint.X, clientPoint.Y);
			if (itemAt != null && itemAt is ListViewItem)
			{
				flag = true;
				SetLastHit((ListViewItem)itemAt);
			}
		}
		finally
		{
			if (!flag)
			{
				LastHit = this;
			}
		}
	}

	private void SetLastHit(ListViewItem item)
	{
		LastHit = item.Tag;
	}

	private void HandleLeftMouseClick(MouseEventArgs e)
	{
		object lastHit = LastHit;
		if (lastHit != null && lastHit != this && lastHit is Node node && m_itemMap.TryGetValue(node, out var _))
		{
			if ((m_control.TheStyle == Style.TreeList || m_control.TheStyle == Style.CheckedTreeList) && node.HitRect.Contains(e.Location))
			{
				node.Expanded = !node.Expanded;
			}
			else if ((m_control.TheStyle == Style.CheckedList || m_control.TheStyle == Style.CheckedTreeList) && node.CheckBoxHitRect.Contains(e.Location))
			{
				node.CheckState = ((node.CheckState != CheckState.Checked) ? CheckState.Checked : CheckState.Unchecked);
			}
		}
	}

	private void InvalidateNode(Node node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (m_itemMap.TryGetValue(node, out var value))
		{
			m_control.Invalidate(value.GetBounds(ItemBoundsPortion.Entire));
		}
	}

	private void ExpandOrCollapse(Node node)
	{
		if (m_expandingCollapsing)
		{
			return;
		}
		try
		{
			m_expandingCollapsing = true;
			try
			{
				BeginUpdate();
				if (node.Expanded)
				{
					ExpandNode(node);
					node.Expanded = true;
					Sort();
				}
				else
				{
					CollapseNode(node);
					node.Expanded = false;
					InvalidateNode(node);
				}
			}
			finally
			{
				EndUpdate();
			}
		}
		finally
		{
			m_expandingCollapsing = false;
		}
	}

	private void ExpandNode(Node node)
	{
		foreach (Node node2 in node.Nodes)
		{
			AddNode(node2);
			if (node2.HasChildren && node2.Expanded)
			{
				ExpandNode(node2);
			}
		}
	}

	private void CollapseNode(Node node)
	{
		foreach (Node node2 in node.Nodes)
		{
			if (node2.Visible)
			{
				RemoveNode(node2);
				if (node2.HasChildren && node2.Expanded)
				{
					CollapseNode(node2);
				}
			}
		}
	}

	private bool LazyLoadNode(Node node)
	{
		if (node == null)
		{
			return false;
		}
		if (!node.NeedsLazyLoad)
		{
			return false;
		}
		this.NodeLazyLoad.Raise(this, new NodeEventArgs(node));
		bool expandingCollapsing = m_expandingCollapsing;
		try
		{
			m_expandingCollapsing = true;
			node.Expanded = true;
		}
		finally
		{
			m_expandingCollapsing = expandingCollapsing;
		}
		if (!node.HasChildren)
		{
			node.IsLeaf = true;
			InvalidateNode(node);
		}
		return true;
	}

	private void ExpandAll(Node node)
	{
		if ((node.Expandable || !node.IsLeaf || node.HasChildren) && !node.Expanded)
		{
			node.Expanded = true;
			if (!LazyLoadNode(node))
			{
				ExpandNode(node);
			}
		}
		List<Node> list = new List<Node>(node.Nodes);
		foreach (Node item in list)
		{
			ExpandAll(item);
		}
	}

	private void CollapseAll(Node node)
	{
		node.Expanded = false;
		CollapseNode(node);
		List<Node> list = new List<Node>(node.Nodes);
		foreach (Node item in list)
		{
			CollapseAll(item);
		}
	}

	private void SetColumnWidth(Column column)
	{
		if (column != null && m_columnMap.TryGetValue(column, out var value))
		{
			if (m_columnWidths.TryGetValue(column.Label, out var value2))
			{
				column.Width = value2;
				value.Width = value2;
			}
			else
			{
				m_columnWidths[value.Text] = column.Width;
			}
		}
	}

	private static ListViewItem CreateListViewItem(Node node, int columnCount)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		ListViewItem listViewItem = new ListViewItem(node.Label)
		{
			Tag = node,
			ImageIndex = node.ImageIndex,
			Checked = (node.CheckState == CheckState.Checked),
			ToolTipText = node.HoverText
		};
		UpdateProperties(node, listViewItem, columnCount);
		return listViewItem;
	}

	private static void UpdateProperties(Node node, ListViewItem lstItem, int columns)
	{
		if (lstItem == null || node.Properties == null)
		{
			return;
		}
		int num = Math.Min(columns, node.Properties.Length);
		for (int i = 0; i < num; i++)
		{
			bool flag = lstItem.SubItems.Count > i + 1;
			object value = node.Properties[i];
			if (flag)
			{
				ListViewItem.ListViewSubItem listViewSubItem = lstItem.SubItems[i + 1];
				listViewSubItem.Tag = node;
				listViewSubItem.Text = GetObjectString(value);
			}
			else
			{
				ListViewItem.ListViewSubItem item = new ListViewItem.ListViewSubItem(lstItem, GetObjectString(value))
				{
					Tag = node
				};
				lstItem.SubItems.Add(item);
			}
		}
	}

	private static string GetObjectString(object value)
	{
		return (value is IFormattable formattable) ? formattable.ToString(null, null) : value.ToString();
	}

	private static bool IsSet(NodeChangeTypes type, NodeChangeTypes changes)
	{
		return (changes & type) == type;
	}

	private void SortTimerTick(object sender, EventArgs e)
	{
		if (m_needSorting)
		{
			Sort();
		}
	}

	private ListViewItem GetVirtualListItemAtIndexOrLookup(int index)
	{
		if (m_control.TheStyle != Style.VirtualList)
		{
			throw new InvalidOperationException("only available in virtual list mode");
		}
		if (index < 0 || index >= VirtualListSize)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (m_virtualItemMap.TryGetValue(index, out var value))
		{
			return value;
		}
		RetrieveVirtualNodeEventArgs e = new RetrieveVirtualNodeEventArgs(index);
		this.RetrieveVirtualNode.Raise(this, e);
		if (e.Node == null)
		{
			throw new NullReferenceException("client code returned null");
		}
		value = CreateListViewItem(e.Node, m_columns.Count);
		m_itemMap.Add(e.Node, value);
		m_virtualItemMap.Add(index, value);
		return value;
	}

	private void EnsureEditingTerminated()
	{
		try
		{
			if (m_currentEditNode == null)
			{
				return;
			}
			try
			{
				string text = m_editBox.Text;
				if (m_currentEditColumnIndex == 0)
				{
					NodeLabelEditEventArgs e = new NodeLabelEditEventArgs(m_currentEditNode, text);
					this.AfterNodeLabelEdit.Raise(this, e);
					if (m_itemMap.TryGetValue(m_currentEditNode, out var value) && !e.CancelEdit)
					{
						value.Text = text;
						m_currentEditNode.Label = text;
					}
				}
				else
				{
					TrySetNodeProperty(m_currentEditNode, m_currentEditColumnIndex - 1, text);
				}
			}
			finally
			{
				m_currentEditNode = null;
				m_editBox.Hide();
			}
		}
		finally
		{
			m_currentEditNode = null;
			m_currentEditColumnIndex = -1;
		}
	}

	private void TrySetNodeProperty(Node node, int propertyIndex, string value)
	{
		object value2 = node.Properties[propertyIndex];
		node.SetProperty(propertyIndex, value);
		if (this.PropertyChanged != null)
		{
			PropertyChangedEventArgs e = new PropertyChangedEventArgs(node, propertyIndex, value);
			this.PropertyChanged(this, e);
			if (e.CancelChange)
			{
				node.SetProperty(propertyIndex, value2);
			}
		}
	}
}

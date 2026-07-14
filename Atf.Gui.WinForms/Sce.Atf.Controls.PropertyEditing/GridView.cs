using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class GridView : PropertyView, IPropertyEditingControlOwner
{
	private enum HitType
	{
		None,
		CategoryExpander,
		ColumnHeader,
		ColumnHeaderRightEdge,
		Cell,
		Row
	}

	private class HitRecord
	{
		public HitType Type = HitType.None;

		public Category Category;

		public Property Property;

		public int Row = -1;

		public Point Offset;
	}

	private class PropertyComparer : IComparer, IComparer<object>
	{
		private readonly PropertyDescriptor m_descriptor;

		private readonly bool m_ascending;

		public PropertyComparer(PropertyDescriptor descriptor, bool ascending)
		{
			m_descriptor = descriptor;
			m_ascending = ascending;
		}

		public int Compare(object x, object y)
		{
			IComparable comparable = m_descriptor.GetValue(x) as IComparable;
			IComparable comparable2 = m_descriptor.GetValue(y) as IComparable;
			if (comparable != null)
			{
				if (comparable2 != null)
				{
					int num = comparable.CompareTo(comparable2);
					if (!m_ascending)
					{
						num *= -1;
					}
					return num;
				}
				return -1;
			}
			return 0;
		}

		int IComparer<object>.Compare(object x, object y)
		{
			return Compare(x, y);
		}
	}

	public class ColumnHeaders : Control
	{
		private GridView m_gridView;

		private int m_dropColumnX;

		private int m_dropColumnIndex;

		private static Point s_mouseDown;

		private static Point s_mouseMove;

		private static Property s_sizingProperty;

		private static int s_sizingOriginalWidth;

		private static bool s_sizing;

		private static Property s_columnHeaderMouseDownProperty;

		private static int s_columnHeaderMouseDownPropertyIndex;

		private static bool s_columnHeaderMouseDown;

		private static bool s_draggingColumnHeader;

		public const int MinimumColumnWidth = 24;

		public GridView GridView
		{
			get
			{
				return m_gridView;
			}
			set
			{
				m_gridView = value;
			}
		}

		public bool Dragging => s_draggingColumnHeader;

		public Brush ColumnHeaderAlphaBrush { get; set; } = new SolidBrush(Color.FromArgb(128, 255, 255, 255));

		public Brush ColumnHeaderBrush { get; set; } = SystemBrushes.ControlLightLight;

		public Pen DropColumnIndicatorPen { get; set; } = new Pen(Color.Black, 3f);

		public ColumnHeaders()
		{
			base.DoubleBuffered = true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DropColumnIndicatorPen?.Dispose();
				ColumnHeaderAlphaBrush?.Dispose();
			}
			base.Dispose(disposing);
		}

		public ColumnHeaders(GridView gridView)
		{
			m_gridView = gridView;
			base.DoubleBuffered = true;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Graphics graphics = e.Graphics;
			int num = -m_gridView.HScroll;
			int rowHeight = m_gridView.RowHeight;
			int left = base.Margin.Left;
			int top = base.Margin.Top;
			int num2 = 0;
			bool flag = false;
			int num3 = num;
			int num4 = 0;
			foreach (Property property in m_gridView.Properties)
			{
				if (property.Equals(s_columnHeaderMouseDownProperty))
				{
					num2 = s_mouseDown.X - num3;
					s_columnHeaderMouseDownPropertyIndex = num4;
				}
				if (m_gridView.GetVisible(property))
				{
					DrawColumnHeader(graphics, property, GridView.CategorySettings, num3, 0, rowHeight, left, top, ColumnHeaderBrush);
					num3 += m_gridView.GetColumnInfo(property).Width;
					flag = true;
				}
				else if (property.FirstInCategory)
				{
					ControlPaint.DrawBorder3D(graphics, num3, 0, rowHeight, rowHeight, Border3DStyle.Etched, Border3DSide.Right | Border3DSide.Bottom);
					GdiUtil.DrawExpander(num3 + left, top, expanded: false, graphics);
					num3 += rowHeight;
					flag = true;
				}
				num4++;
			}
			if (s_draggingColumnHeader && s_columnHeaderMouseDownProperty != null)
			{
				DrawDropColumnIndicator(graphics, m_dropColumnX, 0, rowHeight);
				DrawColumnHeader(graphics, s_columnHeaderMouseDownProperty, GridView.CategorySettings, s_mouseMove.X - num2, 0, rowHeight, left, top, ColumnHeaderAlphaBrush);
			}
			if (flag)
			{
				graphics.FillRectangle(ColumnHeaderBrush, 0, 0, m_gridView.GripWidth, rowHeight);
				ControlPaint.DrawBorder3D(graphics, 0, 0, m_gridView.GripWidth, rowHeight, Border3DStyle.Etched, Border3DSide.Right | Border3DSide.Bottom);
			}
		}

		private void DrawDropColumnIndicator(Graphics g, int x, int y, int height)
		{
			g.DrawLine(DropColumnIndicatorPen, x - 1, y, x - 1, y + height);
		}

		private void DrawColumnHeader(Graphics g, Property p, PropertyCategorySettings categorySettings, int x, int y, int rowHeight, int xPadding, int yPadding, Brush menuBar)
		{
			using Brush brush = new SolidBrush(ForeColor);
			ColumnInfo columnInfo = m_gridView.GetColumnInfo(p);
			int num = columnInfo.Width;
			Rectangle rectangle = new Rectangle(x, y, num, rowHeight);
			g.FillRectangle(menuBar, rectangle);
			ControlPaint.DrawBorder3D(g, x, y, num, rowHeight, Border3DStyle.Etched, Border3DSide.Right | Border3DSide.Bottom);
			if (p.FirstInCategory && categorySettings != PropertyCategorySettings.Disabled)
			{
				GdiUtil.DrawExpander(x + xPadding, y + yPadding, p.Category.Expanded, g);
				int num2 = xPadding + 8 + xPadding;
				rectangle.X += num2;
				rectangle.Width -= num2;
				if (categorySettings != PropertyCategorySettings.HideText)
				{
					string s = p.Category.Name + ": ";
					if (!p.HideDisplayName)
					{
						g.DrawString(s, m_gridView.BoldFont, brush, rectangle, PropertyView.LeftStringFormat);
					}
					num2 = (int)g.MeasureString(s, m_gridView.BoldFont).Width;
					rectangle.X += num2;
					rectangle.Width -= num2;
				}
			}
			if (CanSort(p))
			{
				rectangle.Width -= 16 + xPadding;
				GdiUtil.DrawSortDirectionIndicator(rectangle.Right + xPadding, rectangle.Top + rowHeight / 2 - 1 - 3, columnInfo.NextSortDirection == ListSortDirection.Descending, g);
			}
			if (!p.HideDisplayName)
			{
				g.DrawString(p.Descriptor.DisplayName, m_gridView.Font, brush, rectangle, PropertyView.LeftStringFormat);
			}
		}

		public virtual SizeF MeasureProperty(PropertyCategorySettings categorySettings, bool firstInCategory, string categoryName, string displayName, bool canSort)
		{
			using Graphics graphics = CreateGraphics();
			SizeF result = new SizeF(0f, 0f);
			if (firstInCategory && categorySettings != PropertyCategorySettings.HideText)
			{
				result = graphics.MeasureString(categoryName + ": ", m_gridView.BoldFont);
			}
			SizeF sizeF = graphics.MeasureString(displayName, m_gridView.Font);
			result.Width += sizeF.Width;
			result.Height = Math.Max(result.Height, sizeF.Height);
			if (canSort)
			{
				result.Width += 16 + base.Margin.Left;
			}
			return result;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			s_mouseDown = new Point(e.X, e.Y);
			m_gridView.Focus();
			HitRecord hitRecord = m_gridView.Pick(s_mouseDown);
			switch (hitRecord.Type)
			{
			case HitType.ColumnHeader:
				m_gridView.SelectedProperty = hitRecord.Property;
				s_columnHeaderMouseDown = true;
				s_columnHeaderMouseDownProperty = hitRecord.Property;
				break;
			case HitType.ColumnHeaderRightEdge:
				m_gridView.SelectedProperty = hitRecord.Property;
				m_gridView.Select();
				if (!hitRecord.Property.DisableResize)
				{
					s_sizing = true;
					s_sizingProperty = hitRecord.Property;
					s_sizingOriginalWidth = m_gridView.GetColumnInfo(s_sizingProperty).Width;
					Cursor = Cursors.VSplit;
				}
				break;
			case HitType.CategoryExpander:
				hitRecord.Category.Expanded = !hitRecord.Category.Expanded;
				m_gridView.Invalidate();
				break;
			case HitType.None:
				if (m_gridView.SelectedCount == 0)
				{
					m_gridView.SelectAll();
				}
				else
				{
					m_gridView.ClearSelection();
				}
				break;
			}
			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			s_mouseMove = e.Location;
			if (s_sizing && (e.Button & MouseButtons.Left) != MouseButtons.None)
			{
				int num = e.X - s_mouseDown.X;
				int val = s_sizingOriginalWidth + num;
				val = Math.Max(val, 24);
				m_gridView.SetPropertyColumnWidth(s_sizingProperty, val);
				m_gridView.Invalidate();
			}
			else if (e.Button == MouseButtons.None)
			{
				HitRecord hitRecord = m_gridView.Pick(new Point(e.X, e.Y));
				if (hitRecord.Type == HitType.ColumnHeaderRightEdge && !hitRecord.Property.DisableResize)
				{
					Cursor = Cursors.VSplit;
				}
				else
				{
					Cursor = Cursors.Arrow;
				}
			}
			if (m_gridView.DragDropColumnsEnabed && s_columnHeaderMouseDownProperty != null && !s_columnHeaderMouseDownProperty.DisableDragging)
			{
				int num2 = (e.Location.X - s_mouseDown.X) * (e.Location.X - s_mouseDown.X) + (e.Location.Y - s_mouseDown.Y) * (e.Location.Y - s_mouseDown.Y);
				if (s_columnHeaderMouseDown && e.Button == MouseButtons.Left && num2 > 2)
				{
					s_draggingColumnHeader = true;
				}
			}
			if (s_draggingColumnHeader)
			{
				int num3 = -m_gridView.HScroll;
				int num4 = (m_dropColumnIndex = 0);
				m_dropColumnX = 0;
				foreach (Property property in m_gridView.Properties)
				{
					if (m_gridView.GetVisible(property))
					{
						if (property.DisableDragging && num4 == m_dropColumnIndex)
						{
							m_dropColumnX += m_gridView.GetColumnInfo(property).Width;
							m_dropColumnIndex++;
						}
						if (s_mouseMove.X >= num3 + (int)((double)m_gridView.GetColumnInfo(property).Width * 0.5))
						{
							m_dropColumnX = num3 + m_gridView.GetColumnInfo(property).Width;
							m_dropColumnIndex = num4 + 1;
						}
						num3 += m_gridView.GetColumnInfo(property).Width;
					}
					else if (property.FirstInCategory)
					{
						num3 += m_gridView.RowHeight;
					}
					num4++;
				}
				if (m_dropColumnIndex == 0)
				{
					m_dropColumnX += 2;
				}
				if (m_dropColumnIndex == num4)
				{
					m_dropColumnX--;
				}
				Invalidate();
			}
			base.OnMouseMove(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			Cursor = Cursors.Arrow;
			s_sizing = false;
			if (s_draggingColumnHeader)
			{
				if (s_columnHeaderMouseDownPropertyIndex != m_dropColumnIndex)
				{
					List<string> list = new List<string>();
					foreach (Property property in m_gridView.Properties)
					{
						list.Add(property.Descriptor.Name);
					}
					if (list.Remove(s_columnHeaderMouseDownProperty.Descriptor.Name))
					{
						if (s_columnHeaderMouseDownPropertyIndex < m_dropColumnIndex)
						{
							m_dropColumnIndex--;
						}
						list.Insert(m_dropColumnIndex, s_columnHeaderMouseDownProperty.Descriptor.Name);
					}
					m_gridView.SetCustomPropertySortOrder(list);
					m_gridView.Invalidate();
				}
				Invalidate();
			}
			else if (s_columnHeaderMouseDown)
			{
				HitRecord hitRecord = m_gridView.Pick(e.Location);
				if (hitRecord.Type == HitType.ColumnHeader && CanSort(hitRecord.Property) && s_columnHeaderMouseDownProperty.Equals(hitRecord.Property))
				{
					ColumnInfo columnInfo = m_gridView.GetColumnInfo(hitRecord.Property);
					m_gridView.Sort(hitRecord.Property, columnInfo.NextSortDirection == ListSortDirection.Ascending);
					columnInfo.NextSortDirection ^= ListSortDirection.Descending;
					if (m_gridView.m_editingRowVisible)
					{
						m_gridView.LoseFocusOnEditingControl();
						m_gridView.TurnOffEditingRow();
					}
					m_gridView.Invalidate();
				}
			}
			s_columnHeaderMouseDownProperty = null;
			s_columnHeaderMouseDown = false;
			s_draggingColumnHeader = false;
			base.OnMouseUp(e);
		}

		public void CancelDrag()
		{
			s_draggingColumnHeader = false;
			s_columnHeaderMouseDownProperty = null;
			s_columnHeaderMouseDown = false;
			Invalidate();
		}
	}

	private class ColumnInfo
	{
		public int Width;

		public ListSortDirection NextSortDirection;

		public bool UserHidden;
	}

	private const int kDefaultGripWidth = 18;

	private const int BorderWidth = 1;

	private const int DefaultColumnWidth = 100;

	private const int SortDirectionIndicatorWidth = 16;

	private SolidBrush m_cachedHighlightBrush;
	private SolidBrush m_cachedHighlightTextBrush;
	private SolidBrush m_cachedEvenRowBrush;
	private SolidBrush m_cachedOddRowBrush;
	private SolidBrush m_cachedCellTextBrush;
	private SolidBrush m_cachedCategoryCellBrush;
	private SolidBrush m_cachedCellReadOnlyBrush;
	private Pen m_cachedGridLinePen;

	private SolidBrush HighlightBrush => m_cachedHighlightBrush ??= new SolidBrush(HighlightColor);
	private SolidBrush HighlightTextBrush => m_cachedHighlightTextBrush ??= new SolidBrush(HighlightTextColor);
	private SolidBrush EvenRowBrush => m_cachedEvenRowBrush ??= new SolidBrush(EvenRowColor);
	private SolidBrush OddRowBrush => m_cachedOddRowBrush ??= new SolidBrush(OddRowColor);
	private SolidBrush CellTextBrush => m_cachedCellTextBrush ??= new SolidBrush(CellTextColor);
	private SolidBrush CategoryCellBrush => m_cachedCategoryCellBrush ??= new SolidBrush(CategoryCellBackColor);
	private SolidBrush CellReadOnlyBrush => m_cachedCellReadOnlyBrush ??= new SolidBrush(CellReadOnlyTextColor);
	private Pen GridLinePen => m_cachedGridLinePen ??= new Pen(GridLineColor);

	private void InvalidateCachedGdiObjects()
	{
		m_cachedHighlightBrush?.Dispose(); m_cachedHighlightBrush = null;
		m_cachedHighlightTextBrush?.Dispose(); m_cachedHighlightTextBrush = null;
		m_cachedEvenRowBrush?.Dispose(); m_cachedEvenRowBrush = null;
		m_cachedOddRowBrush?.Dispose(); m_cachedOddRowBrush = null;
		m_cachedCellTextBrush?.Dispose(); m_cachedCellTextBrush = null;
		m_cachedCategoryCellBrush?.Dispose(); m_cachedCategoryCellBrush = null;
		m_cachedCellReadOnlyBrush?.Dispose(); m_cachedCellReadOnlyBrush = null;
		m_cachedGridLinePen?.Dispose(); m_cachedGridLinePen = null;
	}

	private Property m_lastSortProperty;

	private readonly Dictionary<PropertyDescriptor, ColumnInfo> m_columnInfo = new Dictionary<PropertyDescriptor, ColumnInfo>();

	private readonly Dictionary<string, int> m_savedColumnWidths = new Dictionary<string, int>();

	private bool m_editingRowVisible;

	private readonly Selection<int> m_selectedRows;

	private readonly ColumnHeaders m_columnHeaders;

	private readonly VScrollBar m_vScrollBar;

	private int m_vScroll;

	private readonly HScrollBar m_hScrollBar;

	private int m_hScroll;

	private readonly ToolTip m_toolTip;

	private Property m_hoveredProperty = null;

	private int m_hoveredRow = -1;

	private int m_maxControlHeight;

	private List<string> m_userPropertySortOrder = new List<string>();

	private Dictionary<string, int> m_columnWidths = new Dictionary<string, int>();

	private string m_sortByPropertyName;

	private ListSortDirection m_sortByPropertyDirection;

	private Dictionary<string, bool> m_columnUserHiddenStates = new Dictionary<string, bool>();

	private bool m_clickedOnSelectedRow;

	private PropertyCategorySettings m_categorySettings;

	private static Point s_mouseDown;

	private static readonly StringFormat s_verticalFormat;

	public int GripWidth { get; set; } = 18;

	private int HScroll
	{
		get
		{
			return m_hScroll - GripWidth;
		}
		set
		{
			m_hScroll = value;
		}
	}

	public Color HighlightColor { get; set; }

	public Color HighlightTextColor { get; set; }

	public Color EvenRowColor { get; set; }

	public Color OddRowColor { get; set; }

	public Color GridLineColor { get; set; }

	public Color SelectedCellColor { get; set; }

	public Color CategoryCellBackColor { get; set; }

	public Color CellTextColor { get; set; }

	public Color CellReadOnlyTextColor { get; set; }

	public IEnumerable<int> SelectedIndices => m_selectedRows.GetSnapshot().AsEnumerable();

	public PropertyCategorySettings CategorySettings => m_categorySettings;

	public bool AutoScaleColumns { get; set; }

	public int SelectedCount => m_selectedRows.Count;

	public bool EditingRowVisible => m_editingRowVisible;

	public bool DragDropColumnsEnabed { get; set; }

	public bool SelectionEnabed { get; set; }

	public bool MultiSelectionEnabled { get; set; }

	public bool UpDownKeySelectionWrapEnabled { get; set; }

	public bool MouseUpSelectionEnabled { get; set; }

	object[] IPropertyEditingControlOwner.SelectedObjects
	{
		get
		{
			object[] selectedObjects = base.SelectedObjects;
			if (m_selectedRows.Count == 0 || selectedObjects.Length == 0)
			{
				return EmptyArray<object>.Instance;
			}
			int num = selectedObjects.Length;
			List<object> list = new List<object>(m_selectedRows.Count);
			foreach (int selectedRow in m_selectedRows)
			{
				if (selectedRow < num)
				{
					list.Add(selectedObjects[selectedRow]);
				}
			}
			return list.ToArray();
		}
	}

	private int EditingRowHeight => m_maxControlHeight + 2;

	private int HeaderHeight => RowHeight;

	public event EventHandler SelectedRowsChanged;

	public event EventHandler<RowChangedEventArgs> RowValueChanged;

	public GridView()
		: this(new ColumnHeaders(), PropertyCategorySettings.ShowAll)
	{
	}

	public GridView(PropertyCategorySettings catSet)
		: this(new ColumnHeaders(), catSet)
	{
	}

	public GridView(ColumnHeaders columnHeaders, PropertyCategorySettings catSet)
	{
		HighlightColor = SystemColors.Highlight;
		HighlightTextColor = SystemColors.HighlightText;
		EvenRowColor = Color.LightSteelBlue;
		OddRowColor = ColorUtil.GetShade(Color.LightSteelBlue, 1.2f);
		GridLineColor = Color.FromArgb(255, BackColor.R * 200 / 255, BackColor.G * 200 / 255, BackColor.B * 200 / 255);
		SelectedCellColor = Color.LightGray;
		CategoryCellBackColor = SystemColors.Control;
		CellTextColor = SystemColors.ControlText;
		CellReadOnlyTextColor = SystemColors.GrayText;
		m_categorySettings = catSet;
		m_selectedRows = new Selection<int>();
		m_selectedRows.Changed += selectedRows_Changed;
		m_columnHeaders = columnHeaders;
		m_columnHeaders.GridView = this;
		m_columnHeaders.Height = HeaderHeight;
		m_columnHeaders.Dock = DockStyle.Top;
		m_vScrollBar = new VScrollBar();
		m_vScrollBar.Dock = DockStyle.Right;
		m_vScrollBar.ValueChanged += vScrollBar_ValueChanged;
		m_hScrollBar = new HScrollBar();
		m_hScrollBar.Dock = DockStyle.Bottom;
		m_hScrollBar.ValueChanged += hScrollBar_ValueChanged;
		m_toolTip = new ToolTip();
		m_toolTip.AutoPopDelay = 2500;
		m_toolTip.InitialDelay = 500;
		m_toolTip.ReshowDelay = 500;
		base.SizeChanged += GridView_SizeChanged;
		base.Controls.Add(m_columnHeaders);
		base.Controls.Add(m_vScrollBar);
		base.Controls.Add(m_hScrollBar);
		base.DoubleBuffered = true;
		DragDropColumnsEnabed = true;
		SelectionEnabed = true;
		MultiSelectionEnabled = true;
		UpDownKeySelectionWrapEnabled = true;
		AutoScaleColumns = false;
	}

	private void GridView_SizeChanged(object sender, EventArgs e)
	{
		if (AutoScaleColumns)
		{
			ApplyColumnBestFitAllColumns();
		}
	}

	protected virtual void OnSelectedRowsChanged(EventArgs e)
	{
		this.SelectedRowsChanged?.Invoke(this, e);
	}

	protected virtual void OnRowValueChanged(RowChangedEventArgs e)
	{
		if (this.RowValueChanged != null)
		{
			this.RowValueChanged(this, e);
		}
	}

	protected override void ObservableContext_OnItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		int num = base.SelectedObjects.Count() - 1;
		if (num <= e.Index)
		{
			m_selectedRows.Remove(e.Index);
		}
		base.ObservableContext_OnItemRemoved(sender, e);
	}

	public void SetSelection(IEnumerable<int> selectedRows)
	{
		int num = base.EditingContext.Items.Count();
		List<int> list = new List<int>();
		foreach (int selectedRow in selectedRows)
		{
			if (selectedRow < 0)
			{
				throw new ArgumentException("row index out of allowable range");
			}
			list.Add(selectedRow);
		}
		bool num2 = list.Count != 0 && list.Count == num;
		int lastSelected = m_selectedRows.LastSelected;
		if (num2 && lastSelected != -1)
		{
			list.Remove(lastSelected);
			list.Add(lastSelected);
		}
		m_selectedRows.SetRange(list);
		TryMakeSelectionVisible();
	}

	public void ClearSelection()
	{
		m_selectedRows.SetRange(EmptyArray<int>.Instance);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_toolTip?.Dispose();
			InvalidateCachedGdiObjects();
		}
		base.Dispose(disposing);
	}

	public PropertyDescriptor GetDescriptorAt(Point clientPoint)
	{
		Point offset;
		return GetDescriptorAt(clientPoint, out offset);
	}

	public PropertyDescriptor GetDescriptorAt(Point clientPoint, out Point offset)
	{
		HitRecord hitRecord = Pick(clientPoint);
		offset = hitRecord.Offset;
		return hitRecord.Property?.Descriptor;
	}

	public int GetRowIndexAt(Point clientPoint)
	{
		HitRecord hitRecord = Pick(clientPoint);
		return hitRecord.Row;
	}

	public bool SortByProperty(string propertyName, ListSortDirection direction)
	{
		m_sortByPropertyName = propertyName;
		m_sortByPropertyDirection = direction;
		return ApplySortByProperty();
	}

	public override void SetCustomPropertySortOrder(List<string> customSortOrder)
	{
		m_userPropertySortOrder = customSortOrder;
		base.SetCustomPropertySortOrder(customSortOrder);
	}

	public string GetLastSortPropertyName()
	{
		string result = string.Empty;
		if (m_lastSortProperty != null)
		{
			result = m_lastSortProperty.Descriptor.Name;
		}
		return result;
	}

	public ListSortDirection GetPropertySortOrder(string propertyName)
	{
		ListSortDirection result = ListSortDirection.Ascending;
		foreach (Property property in base.Properties)
		{
			if (property.Descriptor.Name.Equals(propertyName))
			{
				ColumnInfo columnInfo = GetColumnInfo(property);
				if (columnInfo != null)
				{
					result = ((columnInfo.NextSortDirection != ListSortDirection.Descending) ? ListSortDirection.Descending : ListSortDirection.Ascending);
				}
				break;
			}
		}
		return result;
	}

	protected int ApplyColumnBestFit(Property p)
	{
		if (p.DisableResize)
		{
			return GetColumnInfo(p)?.Width ?? 0;
		}
		int num = 4;
		using Graphics g = CreateGraphics();
		int val = (int)m_columnHeaders.MeasureProperty(m_categorySettings, p.FirstInCategory, p.Descriptor.Category, p.Descriptor.DisplayName, CanSort(p)).Width + num;
		if (GetVisible(p))
		{
			int rowAtY = GetRowAtY(HeaderHeight);
			int num2 = GetRowAtY(base.Height);
			if (num2 >= base.SelectedObjects.Length)
			{
				num2 = base.SelectedObjects.Length - 1;
			}
			for (int i = rowAtY; i <= num2; i++)
			{
				bool selected = m_selectedRows.Contains(i);
				int val2 = (int)MeasureProperty(g, p, i, selected).Width + num;
				val = Math.Max(val, val2);
				int val3 = (int)MeasurePropertyEditor(g, p, i, selected).Width + num;
				val = Math.Max(val, val3);
			}
		}
		val = Math.Max(val, 24);
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		dictionary.Add(p.Descriptor.Name, val);
		SetColumnWidths(dictionary);
		Invalidate();
		return val;
	}

	public void ApplyColumnBestFit(Point location)
	{
		HitRecord hitRecord = Pick(location);
		if (hitRecord.Type != HitType.None)
		{
			ApplyColumnBestFit(hitRecord.Property);
		}
	}

	public void ApplyColumnBestFitAllColumns()
	{
		Property[] array = base.Properties.ToArray();
		foreach (Property p in array)
		{
			ApplyColumnBestFit(p);
		}
	}

	public Dictionary<string, int> GetColumnWidths()
	{
		foreach (Property property in base.Properties)
		{
			ColumnInfo columnInfo = GetColumnInfo(property);
			if (columnInfo != null)
			{
				if (m_columnWidths.ContainsKey(property.Descriptor.Name))
				{
					m_columnWidths[property.Descriptor.Name] = columnInfo.Width;
				}
				else
				{
					m_columnWidths.Add(property.Descriptor.Name, columnInfo.Width);
				}
			}
		}
		return m_columnWidths;
	}

	public bool SetColumnWidths(Dictionary<string, int> columnWidths)
	{
		foreach (KeyValuePair<string, int> columnWidth in columnWidths)
		{
			m_columnWidths[columnWidth.Key] = columnWidth.Value;
		}
		return ApplyColumnWidths();
	}

	public void EnterEditMode(string propertyName)
	{
		foreach (Property property in base.Properties)
		{
			if (property.Descriptor.Name.Equals(propertyName))
			{
				EditProperty(property);
				break;
			}
		}
	}

	protected bool GetVisible(Property p)
	{
		return !GetColumnInfo(p).UserHidden && p.Visible;
	}

	public Dictionary<string, bool> GetColumnUserHiddenStates()
	{
		if (m_columnUserHiddenStates == null)
		{
			m_columnUserHiddenStates = new Dictionary<string, bool>();
		}
		if (base.Properties.Any())
		{
			m_columnUserHiddenStates.Clear();
		}
		foreach (Property property in base.Properties)
		{
			ColumnInfo columnInfo = GetColumnInfo(property);
			if (columnInfo != null)
			{
				if (m_columnUserHiddenStates.ContainsKey(property.Descriptor.Name))
				{
					m_columnUserHiddenStates[property.Descriptor.Name] = columnInfo.UserHidden;
				}
				else
				{
					m_columnUserHiddenStates.Add(property.Descriptor.Name, columnInfo.UserHidden);
				}
			}
		}
		return m_columnUserHiddenStates;
	}

	public bool SetColumnUserHiddenStates(Dictionary<string, bool> columnUserHiddenStates)
	{
		m_columnUserHiddenStates = columnUserHiddenStates;
		return ApplyColumnUserHiddenStates();
	}

	public List<string> GetCustomPropertySortOrder()
	{
		return m_userPropertySortOrder;
	}

	private IEnumerable<int> GetRangeOfInts(int start, int end)
	{
		for (int i = start; i <= end; i++)
		{
			yield return i;
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == (Keys.A | Keys.Control) && !m_editingRowVisible && MultiSelectionEnabled)
		{
			SelectAll();
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		if (e.KeyChar == 'a' && !m_editingRowVisible && MultiSelectionEnabled)
		{
			m_selectedRows.SetRange(GetRangeOfInts(0, base.SelectedObjects.Length - 1));
			e.Handled = true;
		}
		base.OnKeyPress(e);
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (base.SelectedObjects == null)
		{
			return base.ProcessDialogKey(keyData);
		}
		if (m_columnHeaders.Dragging && keyData == Keys.Escape)
		{
			m_columnHeaders.CancelDrag();
			return true;
		}
		int num = base.SelectedObjects.Length;
		if (m_editingRowVisible && num > 0)
		{
			int lastSelected = m_selectedRows.LastSelected;
			int num2 = lastSelected;
			switch (keyData)
			{
			case Keys.Escape:
				CancelEdit();
				return true;
			case Keys.Return:
				CommitEdit();
				return true;
			case Keys.Down:
				if (UpDownKeySelectionWrapEnabled || num2 != num - 1)
				{
					Property selectedProperty2 = base.SelectedProperty;
					LoseFocusOnEditingControl();
					num2++;
					if (num2 >= num)
					{
						num2 = 0;
					}
					base.SelectedProperty = selectedProperty2;
				}
				break;
			case Keys.Up:
				if (UpDownKeySelectionWrapEnabled || num2 != 0)
				{
					Property selectedProperty = base.SelectedProperty;
					LoseFocusOnEditingControl();
					num2--;
					if (num2 < 0)
					{
						num2 = num - 1;
					}
					base.SelectedProperty = selectedProperty;
				}
				break;
			}
			if (num2 != lastSelected)
			{
				Select(num2, keyData);
				Invalidate();
				return true;
			}
			switch (keyData)
			{
			case Keys.Left:
			{
				Property previousEditableProperty = GetPreviousEditableProperty(base.SelectedProperty);
				if (previousEditableProperty != null && previousEditableProperty.Control != null)
				{
					previousEditableProperty.Control.Select();
					if (previousEditableProperty.Control.CanFocus)
					{
						previousEditableProperty.Control.Focus();
					}
					base.SelectedProperty = previousEditableProperty;
					TryMakeSelectionVisible();
					return true;
				}
				break;
			}
			case Keys.Right:
			{
				Property nextEditableProperty = GetNextEditableProperty(base.SelectedProperty);
				if (nextEditableProperty != null && nextEditableProperty.Control != null)
				{
					nextEditableProperty.Control.Select();
					if (nextEditableProperty.Control.CanFocus)
					{
						nextEditableProperty.Control.Focus();
					}
					base.SelectedProperty = nextEditableProperty;
					TryMakeSelectionVisible();
					return true;
				}
				break;
			}
			}
		}
		else if (SelectedCount != 0)
		{
			if (keyData == Keys.Return && base.SelectedProperty != null && !base.SelectedProperty.DisableEditing)
			{
				EditProperty(base.SelectedProperty);
				return true;
			}
			int lastSelected2 = m_selectedRows.LastSelected;
			int num3 = lastSelected2;
			Property property = base.SelectedProperty;
			if (keyData == Keys.Down || keyData == (Keys.Down | Keys.Shift))
			{
				if (UpDownKeySelectionWrapEnabled || num3 != num - 1)
				{
					num3++;
					if (num3 >= num)
					{
						num3 = 0;
					}
				}
			}
			else if (keyData == Keys.Up || keyData == (Keys.Up | Keys.Shift))
			{
				if (UpDownKeySelectionWrapEnabled || num3 != 0)
				{
					num3--;
					if (num3 < 0)
					{
						num3 = num - 1;
					}
				}
			}
			else if (keyData == Keys.Left || keyData == (Keys.Left | Keys.Shift))
			{
				property = GetPreviousProperty(base.SelectedProperty);
			}
			else if (keyData == Keys.Right || keyData == (Keys.Right | Keys.Shift))
			{
				property = GetNextProperty(base.SelectedProperty);
			}
			if (property != null && !property.Equals(base.SelectedProperty))
			{
				base.SelectedProperty = property;
				TryMakeSelectionVisible();
				Invalidate();
				return true;
			}
			if (num3 != lastSelected2)
			{
				Select(num3, keyData);
				Invalidate();
				return true;
			}
		}
		return base.ProcessDialogKey(keyData);
	}

	private Rectangle GetLastSelectionBounds()
	{
		int val = int.MaxValue;
		int val2 = int.MinValue;
		int num;
		int num2;
		if (base.SelectedProperty == null)
		{
			num = 0;
			num2 = 0;
		}
		else
		{
			num = GetColumnLeft(base.SelectedProperty);
			num2 = num + GetColumnInfo(base.SelectedProperty).Width;
		}
		int num3 = HeaderHeight + m_selectedRows.LastSelected * RowHeight;
		int val3 = num3 + RowHeight;
		val = Math.Min(val, num3);
		val2 = Math.Max(val2, val3);
		return new Rectangle(num, val, num2 - num, val2 - val);
	}

	private void TryMakeSelectionVisible()
	{
		if (SelectedCount != 0)
		{
			int value = m_vScrollBar.Value;
			int value2 = m_hScrollBar.Value;
			Rectangle lastSelectionBounds = GetLastSelectionBounds();
			int num = base.Height - HeaderHeight;
			if (lastSelectionBounds.Bottom > num + m_vScroll)
			{
				SetVerticalScroll(lastSelectionBounds.Bottom - num);
			}
			if (lastSelectionBounds.Top < m_vScroll + HeaderHeight)
			{
				SetVerticalScroll(lastSelectionBounds.Top - HeaderHeight);
			}
			if (lastSelectionBounds.Right > base.Width + HScroll)
			{
				SetHorizontalScroll(lastSelectionBounds.Right - base.Width);
			}
			if (lastSelectionBounds.Left < HScroll)
			{
				SetHorizontalScroll(lastSelectionBounds.Left);
			}
			if (value != m_vScrollBar.Value || value2 != m_hScrollBar.Value)
			{
				Invalidate();
			}
		}
	}

	private void Select(int row, Keys modifiers)
	{
		bool flag = true;
		if (MultiSelectionEnabled && (modifiers & Keys.Control) == Keys.Control)
		{
			if (m_selectedRows.Contains(row))
			{
				flag = false;
			}
			m_selectedRows.Toggle(row);
		}
		else if (MultiSelectionEnabled && (modifiers & Keys.Shift) == Keys.Shift)
		{
			m_selectedRows.Add(row);
		}
		else
		{
			m_selectedRows.Set(row);
		}
		if (flag)
		{
			TryMakeSelectionVisible();
		}
	}

	private void SelectAll()
	{
		m_selectedRows.SetRange(GetRangeOfInts(0, base.SelectedObjects.Length - 1));
	}

	private void SelectRange(IEnumerable<int> items, Keys modifiers)
	{
		if ((modifiers & Keys.Control) == Keys.Control)
		{
			m_selectedRows.ToggleRange(items);
			return;
		}
		if ((modifiers & Keys.Shift) == Keys.Shift)
		{
			m_selectedRows.AddRange(items);
			return;
		}
		m_selectedRows.SetRange(items);
		TryMakeSelectionVisible();
	}

	private void LoseFocusOnEditingControl()
	{
		Focus();
	}

	private bool ApplyColumnWidths()
	{
		bool result = false;
		float num = 1f;
		if (AutoScaleColumns)
		{
			int num2 = base.ClientSize.Width - GripWidth;
			if (m_vScrollBar.Visible)
			{
				num2 -= m_vScrollBar.Width;
			}
			int totalWidth = 0;
			m_columnWidths.ForEach(delegate(KeyValuePair<string, int> kvPair)
			{
				totalWidth += kvPair.Value;
			});
			if (totalWidth == 0)
			{
				totalWidth = 1;
			}
			num = Math.Max(1f, (float)num2 / (float)totalWidth);
		}
		foreach (Property property in base.Properties)
		{
			if (m_columnWidths.ContainsKey(property.Descriptor.Name))
			{
				ColumnInfo columnInfo = GetColumnInfo(property);
				if (columnInfo != null)
				{
					columnInfo.Width = (int)((float)m_columnWidths[property.Descriptor.Name] * num);
					result = true;
				}
			}
		}
		return result;
	}

	private bool ApplyColumnUserHiddenStates()
	{
		bool result = false;
		foreach (Property property in base.Properties)
		{
			if (m_columnUserHiddenStates.ContainsKey(property.Descriptor.Name))
			{
				ColumnInfo columnInfo = GetColumnInfo(property);
				if (columnInfo != null)
				{
					columnInfo.UserHidden = m_columnUserHiddenStates[property.Descriptor.Name];
					result = true;
				}
			}
		}
		return result;
	}

	private bool ApplySortByProperty()
	{
		bool result = false;
		foreach (Property property in base.Properties)
		{
			if (!property.Descriptor.Name.Equals(m_sortByPropertyName))
			{
				continue;
			}
			if (CanSort(property))
			{
				bool flag = m_sortByPropertyDirection == ListSortDirection.Ascending;
				Sort(property, flag);
				ColumnInfo columnInfo = GetColumnInfo(property);
				if (columnInfo != null)
				{
					columnInfo.NextSortDirection = (flag ? ListSortDirection.Descending : ListSortDirection.Ascending);
				}
				Invalidate();
				result = true;
			}
			break;
		}
		return result;
	}

	private void ApplyAllLayoutState()
	{
		if (m_userPropertySortOrder != null && m_userPropertySortOrder.Count > 0)
		{
			SetCustomPropertySortOrder(m_userPropertySortOrder);
		}
		if (m_sortByPropertyName != null)
		{
			ApplySortByProperty();
		}
		if (m_columnWidths.Count != base.Properties.Count())
		{
			ApplyColumnBestFitAllColumns();
		}
		else
		{
			ApplyColumnWidths();
		}
		ApplyColumnUserHiddenStates();
	}

	private void MouseSelection(MouseEventArgs e)
	{
		HitRecord hitRecord = Pick(e.Location);
		base.SelectedProperty = hitRecord.Property;
		if (hitRecord.Type != HitType.None)
		{
			Keys modifierKeys = Control.ModifierKeys;
			if (m_selectedRows.Count > 0 && (modifierKeys & Keys.Shift) != Keys.None && e.Button == MouseButtons.Left)
			{
				int num = m_selectedRows.LastSelected;
				int num2 = hitRecord.Row;
				if (num > num2)
				{
					num2 = num;
					num = hitRecord.Row;
				}
				m_editingRowVisible = false;
				SelectRange(Enumerable.Range(num, num2 - num + 1), modifierKeys);
				Select(hitRecord.Row, Keys.Shift);
			}
			else if ((modifierKeys & Keys.Control) != Keys.None && e.Button == MouseButtons.Left)
			{
				m_editingRowVisible = false;
				Select(hitRecord.Row, modifierKeys);
			}
			else
			{
				if (hitRecord.Row < 0)
				{
					return;
				}
				if (!m_editingRowVisible && m_selectedRows.Contains(hitRecord.Row))
				{
					if (e.Button == MouseButtons.Left)
					{
						Select(hitRecord.Row, Keys.Shift);
					}
				}
				else
				{
					m_editingRowVisible = false;
					Select(hitRecord.Row, modifierKeys);
				}
			}
		}
		else
		{
			m_selectedRows.Clear();
		}
	}

	protected ColumnHeaders GetColumnHeaders()
	{
		return m_columnHeaders;
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		m_columnHeaders.Height = HeaderHeight;
	}

	protected override void OnInvalidated(InvalidateEventArgs e)
	{
		m_columnHeaders.Invalidate();
		base.OnInvalidated(e);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		Invalidate();
		base.OnGotFocus(e);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		ClearToolTips();
		Invalidate();
		base.OnLostFocus(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		bool bWidthChanged = UpdateScrollbars();
		UpdateColumnWidths(bWidthChanged);
		PositionControls();
		base.OnPaint(e);
		PaintRowBackgrounds(e.Graphics);
		PaintRowValues(e.Graphics);
		PaintRowFocusIndicators(e.Graphics);
	}

	private void UpdateColumnWidths(bool bWidthChanged)
	{
		if (bWidthChanged && AutoScaleColumns)
		{
			ApplyColumnWidths();
		}
	}

	private bool UpdateScrollbars()
	{
		int columnLeft = GetColumnLeft(null);
		int num = EditingRowHeight;
		if (base.EditingContext != null)
		{
			num += RowHeight * base.SelectedObjects.Length;
		}
		bool result = WinFormsUtil.UpdateScrollbars(m_vScrollBar, m_hScrollBar, new Size(base.Width, base.Height - HeaderHeight), new Size(columnLeft, num));
		VScrollBar vScrollBar = m_vScrollBar;
		int smallChange = (m_hScrollBar.SmallChange = RowHeight);
		vScrollBar.SmallChange = smallChange;
		return result;
	}

	private void PaintRowBackgrounds(Graphics graphics)
	{
		if (base.EditingContext == null)
		{
			return;
		}
		int num = RowHeight * base.SelectedObjects.Length;
		if (m_editingRowVisible)
		{
			num += EditingRowHeight - RowHeight;
		}
		int columnLeft = GetColumnLeft(null);
		int num2 = -HScroll;
		int num3 = -m_vScroll;
		Rectangle rowRect = new Rectangle(0, num3 + HeaderHeight, columnLeft, RowHeight);
		for (int i = 0; i < base.SelectedObjects.Length; i++)
		{
			int num4 = RowHeight;
			Brush brush4 = (((i & 1) == 0) ? EvenRowBrush : OddRowBrush);
			bool flag = m_selectedRows.Contains(i);
			if (flag)
			{
				brush4 = HighlightBrush;
				if (m_editingRowVisible && m_selectedRows.LastSelected == i)
				{
					num4 = EditingRowHeight;
				}
			}
			FillRowBackground(graphics, brush4, rowRect, i, flag);
			graphics.DrawLine(GridLinePen, rowRect.Left, rowRect.Bottom - 1, rowRect.Right - 1, rowRect.Bottom - 1);
			DrawGrip(graphics, rowRect.Left, rowRect.Top, GripWidth - 1, num4 - 1, brush4, CategoryCellBrush);
			rowRect.Y += num4;
		}
	}

	private void PaintRowValues(Graphics graphics)
	{
		if (base.EditingContext == null)
		{
			return;
		}
		int num = RowHeight * base.SelectedObjects.Length;
		if (m_editingRowVisible)
		{
			num += EditingRowHeight - RowHeight;
		}
		int rowAtY = GetRowAtY(HeaderHeight);
		int num2 = GetRowAtY(base.Height);
		if (num2 >= base.SelectedObjects.Length)
		{
			num2 = base.SelectedObjects.Length - 1;
		}
		int num3 = -HScroll;
		int num4 = -m_vScroll;
		int num5 = num3;
		foreach (Property property in base.Properties)
		{
			if (GetVisible(property))
			{
				int num6 = GetColumnInfo(property).Width;
				int rowY = GetRowY(rowAtY);
				Rectangle valueRect = new Rectangle(num5, rowY, num6, RowHeight);
				for (int i = rowAtY; i <= num2; i++)
				{
					bool flag = m_selectedRows.Contains(i);
					Brush defaultBrush = CellTextBrush;
					if (property.Descriptor.IsReadOnly)
					{
						defaultBrush = CellReadOnlyBrush;
					}
					if (flag)
					{
						defaultBrush = HighlightTextBrush;
					}
					if (!m_editingRowVisible && property.Equals(base.SelectedProperty) && m_selectedRows.LastSelected == i && SelectedCount != 0)
					{
						DrawSelectedPropertyIndicator(graphics, valueRect);
					}
					int num7 = valueRect.Y;
					int rowHeight = RowHeight;
					if (flag && m_editingRowVisible && m_selectedRows.LastSelected == i && !property.DisableEditing)
					{
						valueRect.Y += EditingRowHeight;
						rowHeight = EditingRowHeight;
					}
					else
					{
						DrawValue(graphics, defaultBrush, property, valueRect, i, flag);
						valueRect.Y += RowHeight;
					}
				}
				num5 += num6;
			}
			else if (property.FirstInCategory)
			{
				Rectangle rectangle = new Rectangle(num5, RowHeight, RowHeight, num);
				graphics.FillRectangle(CategoryCellBrush, rectangle);
				graphics.DrawString(property.Category.Name, BoldFont, CellTextBrush, rectangle, s_verticalFormat);
				num5 += RowHeight;
			}
		}
	}

	private void PaintRowFocusIndicators(Graphics graphics)
	{
		if (base.EditingContext == null)
		{
			return;
		}
		int num = RowHeight * base.SelectedObjects.Length;
		if (m_editingRowVisible)
		{
			num += EditingRowHeight - RowHeight;
		}
		int columnLeft = GetColumnLeft(null);
		int num2 = -HScroll;
		int num3 = -m_vScroll;
		Rectangle rectangle = new Rectangle(0, num3 + HeaderHeight, columnLeft, RowHeight);
		for (int i = 0; i < base.SelectedObjects.Length; i++)
		{
			int num4 = RowHeight;
			Brush back = (((i & 1) == 0) ? EvenRowBrush : OddRowBrush);
			if (m_selectedRows.Contains(i))
			{
				back = HighlightBrush;
				if (m_editingRowVisible && m_selectedRows.LastSelected == i)
				{
					num4 = EditingRowHeight;
				}
			}
			DrawGrip(graphics, rectangle.Left, rectangle.Top, GripWidth - 1, num4 - 1, back, CategoryCellBrush);
			graphics.DrawLine(GridLinePen, GripWidth - 1, num3 + HeaderHeight, GripWidth - 1, num3 + HeaderHeight + num - 1);
			int num5 = num2;
			foreach (Property property in base.Properties)
			{
				if (GetVisible(property))
				{
					num5 += GetColumnInfo(property).Width;
				}
				else if (property.FirstInCategory)
				{
					num5 += RowHeight;
				}
				if (num5 > GripWidth)
				{
					graphics.DrawLine(GridLinePen, num5 - 1, num3 + HeaderHeight, num5 - 1, num3 + HeaderHeight + num - 1);
				}
			}
			graphics.DrawLine(GridLinePen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			rectangle.Y += num4;
		}
	}

	protected void OnPaintOld(PaintEventArgs e)
	{
		using Brush brush = new SolidBrush(HighlightColor);
		using Brush brush2 = new SolidBrush(HighlightTextColor);
		using Brush brush3 = new SolidBrush(EvenRowColor);
		using Brush brush4 = new SolidBrush(OddRowColor);
		using Brush brush5 = new SolidBrush(CellTextColor);
		using Brush brush6 = new SolidBrush(CategoryCellBackColor);
		using Brush brush7 = new SolidBrush(CellReadOnlyTextColor);
		using Pen pen = new Pen(GridLineColor);
		int columnLeft = GetColumnLeft(null);
		int num = EditingRowHeight;
		if (base.EditingContext != null)
		{
			num += RowHeight * base.SelectedObjects.Length;
		}
		if (WinFormsUtil.UpdateScrollbars(m_vScrollBar, m_hScrollBar, new Size(base.Width, base.Height - HeaderHeight), new Size(columnLeft, num)) && AutoScaleColumns)
		{
			ApplyColumnWidths();
		}
		VScrollBar vScrollBar = m_vScrollBar;
		int smallChange = (m_hScrollBar.SmallChange = RowHeight);
		vScrollBar.SmallChange = smallChange;
		PositionControls();
		base.OnPaint(e);
		if (base.EditingContext == null)
		{
			return;
		}
		Graphics graphics = e.Graphics;
		int num2 = RowHeight * base.SelectedObjects.Length;
		if (m_editingRowVisible)
		{
			num2 += EditingRowHeight - RowHeight;
		}
		int columnLeft2 = GetColumnLeft(null);
		int num3 = -HScroll;
		int num4 = -m_vScroll;
		Rectangle rowRect = new Rectangle(0, num4 + HeaderHeight, columnLeft2, RowHeight);
		for (int i = 0; i < base.SelectedObjects.Length; i++)
		{
			int num5 = RowHeight;
			Brush brush8 = (((i & 1) == 0) ? brush3 : brush4);
			bool flag = m_selectedRows.Contains(i);
			if (flag)
			{
				brush8 = brush;
				if (m_editingRowVisible && m_selectedRows.LastSelected == i)
				{
					num5 = EditingRowHeight;
				}
			}
			FillRowBackground(graphics, brush8, rowRect, i, flag);
			graphics.DrawLine(pen, rowRect.Left, rowRect.Bottom - 1, rowRect.Right - 1, rowRect.Bottom - 1);
			DrawGrip(graphics, rowRect.Left, rowRect.Top, GripWidth - 1, num5 - 1, brush8, brush6);
			rowRect.Y += num5;
		}
		graphics.DrawLine(pen, GripWidth - 1, HeaderHeight, GripWidth - 1, base.Height - HeaderHeight + 1);
		int rowAtY = GetRowAtY(HeaderHeight);
		int num6 = GetRowAtY(base.Height);
		if (num6 >= base.SelectedObjects.Length)
		{
			num6 = base.SelectedObjects.Length - 1;
		}
		int num7 = num3;
		foreach (Property property in base.Properties)
		{
			if (GetVisible(property))
			{
				int num8 = GetColumnInfo(property).Width;
				int rowY = GetRowY(rowAtY);
				Rectangle valueRect = new Rectangle(num7, rowY, num8, RowHeight);
				for (int j = rowAtY; j <= num6; j++)
				{
					bool flag2 = m_selectedRows.Contains(j);
					Brush defaultBrush = brush5;
					if (property.Descriptor.IsReadOnly)
					{
						defaultBrush = brush7;
					}
					if (flag2)
					{
						defaultBrush = brush2;
					}
					if (!m_editingRowVisible && property.Equals(base.SelectedProperty) && m_selectedRows.LastSelected == j && SelectedCount != 0)
					{
						DrawSelectedPropertyIndicator(graphics, valueRect);
					}
					int num9 = valueRect.Y;
					int rowHeight2 = RowHeight;
					if (flag2 && m_editingRowVisible && m_selectedRows.LastSelected == j && !property.DisableEditing)
					{
						valueRect.Y += EditingRowHeight;
						rowHeight2 = EditingRowHeight;
					}
					else
					{
						DrawValue(graphics, defaultBrush, property, valueRect, j, flag2);
						valueRect.Y += RowHeight;
					}
				}
				num7 += num8;
			}
			else if (property.FirstInCategory)
			{
				Rectangle rectangle = new Rectangle(num7, RowHeight, RowHeight, num2);
				graphics.FillRectangle(brush6, rectangle);
				graphics.DrawString(property.Category.Name, BoldFont, brush5, rectangle, s_verticalFormat);
				num7 += RowHeight;
			}
			graphics.DrawLine(pen, num7 - 1, num4 + HeaderHeight, num7 - 1, num4 + HeaderHeight + num2 - 1);
		}
	}

	protected virtual void FillRowBackground(Graphics g, Brush defaultBrush, Rectangle rowRect, int row, bool selected)
	{
		g.FillRectangle(defaultBrush, rowRect);
	}

	protected virtual void DrawValue(Graphics g, Brush defaultBrush, Property p, Rectangle valueRect, int row, bool selected)
	{
		object obj = base.SelectedObjects[row];
		Font font = ((p.Descriptor.CanResetValue(obj) || p.Descriptor.IsReadOnly) ? BoldFont : Font);
		ICustomDrawProperty customDrawProperty = p.Descriptor.As<ICustomDrawProperty>();
		if (customDrawProperty != null)
		{
			customDrawProperty.DrawValue(g, font, defaultBrush, valueRect, p.Descriptor, obj, selected);
			return;
		}
		string propertyText = PropertyUtils.GetPropertyText(obj, p.Descriptor);
		g.DrawString(propertyText, font, defaultBrush, valueRect, PropertyView.LeftStringFormat);
	}

	protected virtual SizeF MeasureProperty(Graphics g, Property p, int row, bool selected)
	{
		object obj = base.SelectedObjects[row];
		Font font = (p.Descriptor.CanResetValue(obj) ? BoldFont : Font);
		string propertyText = PropertyUtils.GetPropertyText(obj, p.Descriptor);
		return g.MeasureString(propertyText, font);
	}

	protected virtual SizeF MeasurePropertyEditor(Graphics g, Property p, int row, bool selected)
	{
		if (p.Descriptor.GetEditor(typeof(UITypeEditor)) is IPropertyEditor propertyEditor)
		{
			object component = base.SelectedObjects[row];
			Font f = (p.Descriptor.CanResetValue(component) ? BoldFont : Font);
			return propertyEditor.GetDesiredSize(g, f);
		}
		return new SizeF(0f, 0f);
	}

	protected virtual void DrawGrip(Graphics graphics, int x, int y, int width, int height, Brush back, Brush fill)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		if (back == null)
		{
			throw new ArgumentNullException("back");
		}
		if (fill == null)
		{
			throw new ArgumentNullException("fill");
		}
		graphics.FillRectangle(back, x, y, width, height);
		int num = (int)(0.5f * (float)height);
		int num2 = (int)((double)(height - num) / 2.0);
		int num3 = (int)(0.666666f * (float)width);
		int num4 = (int)((double)(width - num3) / 2.0);
		int num5 = Math.Max(2, (int)((double)num3 * 0.075));
		num3 -= 2 * num5;
		num3 = (int)((double)num3 / 3.0);
		Rectangle rect = new Rectangle(x + num4, y + num2, num3, num);
		graphics.FillRectangle(fill, rect);
		rect.Offset(num3 + num5, 0);
		graphics.FillRectangle(fill, rect);
		rect.Offset(num3 + num5, 0);
		graphics.FillRectangle(fill, rect);
	}

	protected virtual void DrawSelectedPropertyIndicator(Graphics g, Rectangle valueRect)
	{
		using Pen pen = new Pen(SelectedCellColor);
		Rectangle rect = valueRect;
		rect.Inflate(-1, -1);
		rect.Offset(-1, -1);
		g.DrawRectangle(pen, rect);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		Focus();
		if (!SelectionEnabed)
		{
			return;
		}
		s_mouseDown = new Point(e.X, e.Y);
		HitRecord hitRecord = Pick(s_mouseDown);
		m_clickedOnSelectedRow = hitRecord.Property != null && m_selectedRows.Contains(hitRecord.Row);
		if (!MouseUpSelectionEnabled)
		{
			MouseSelection(e);
		}
		if (m_editingRowVisible && base.SelectedProperty != null && base.SelectedProperty.Control != null)
		{
			base.SelectedProperty.Control.Visible = true;
			base.SelectedProperty.Control.Select();
			if (base.SelectedProperty.Control.CanFocus)
			{
				base.SelectedProperty.Control.Focus();
			}
		}
		Invalidate();
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (MouseUpSelectionEnabled)
		{
			MouseSelection(e);
		}
		HitRecord hitRecord = Pick(e.Location);
		if (hitRecord.Property == base.SelectedProperty && base.SelectedProperty != null && m_selectedRows.Contains(hitRecord.Row) && !base.SelectedProperty.DisableEditing)
		{
			EditProperty(base.SelectedProperty);
		}
		base.OnMouseUp(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		HitRecord hitRecord = Pick(e.Location);
		if (hitRecord.Type == HitType.Cell)
		{
			if (m_hoveredProperty != hitRecord.Property || m_hoveredRow != hitRecord.Row)
			{
				m_hoveredProperty = hitRecord.Property;
				m_hoveredRow = hitRecord.Row;
				m_toolTip.Active = false;
				m_toolTip.SetToolTip(this, m_hoveredProperty?.Descriptor?.Description ?? string.Empty);
				m_toolTip.Active = true;
			}
		}
		else
		{
			ClearToolTips();
		}
		if (hitRecord.Type == HitType.Row)
		{
			Cursor.Current = ResourceUtil.GetCursor(Resources.RowSelectorCursor);
		}
		else
		{
			Cursor.Current = Cursors.Arrow;
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		ClearToolTips();
		base.OnMouseLeave(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		int verticalScroll = m_vScrollBar.Value - e.Delta / 2;
		SetVerticalScroll(verticalScroll);
		base.OnMouseWheel(e);
	}

	private void ClearToolTips()
	{
		m_hoveredProperty = null;
		m_hoveredRow = -1;
		m_toolTip.RemoveAll();
	}

	private void SetVerticalScroll(int vScroll)
	{
		m_vScrollBar.Value = Math.Max(m_vScrollBar.Minimum, Math.Min(m_vScrollBar.Maximum, vScroll));
	}

	private void SetHorizontalScroll(int hScroll)
	{
		m_hScrollBar.Value = Math.Max(m_hScrollBar.Minimum, Math.Min(m_hScrollBar.Maximum, hScroll));
	}

	protected override void OnEditingContextChanged()
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		ClearToolTips();
		m_selectedRows.Clear();
		if (m_lastSortProperty != null)
		{
			ColumnInfo columnInfo = GetColumnInfo(m_lastSortProperty);
			if (columnInfo != null)
			{
				bool flag = columnInfo.NextSortDirection != ListSortDirection.Ascending;
				Sort(m_lastSortProperty, flag);
				Invalidate();
			}
		}
		m_maxControlHeight = RowHeight;
		long skinMax = 0;
		string skinMaxName = "";
		int skinCount = 0;
		foreach (Property property in base.Properties)
		{
			if (property.Control != null)
			{
				var swSkin = System.Diagnostics.Stopwatch.StartNew();
				SkinService.ApplyActiveSkin(property.Control);
				swSkin.Stop();
				if (swSkin.ElapsedMilliseconds > skinMax)
				{
					skinMax = swSkin.ElapsedMilliseconds;
					skinMaxName = property.Descriptor?.Name ?? "?";
				}
				skinCount++;
				property.Control.Height = RowHeight;
				if (property.Control is PropertyEditingControl propertyEditingControl)
				{
					propertyEditingControl.Bind(property.Context);
					propertyEditingControl.PropertyEdited -= propertyEditingControl_PropertyEdited;
					propertyEditingControl.PropertyEdited += propertyEditingControl_PropertyEdited;
				}
				AddLostFocusHandlerRecursively(property.Control);
			}
			if (property.Control != null)
			{
				m_maxControlHeight = Math.Max(m_maxControlHeight, property.Control.Height);
			}
		}
		ApplyAllLayoutState();
		sw.Stop();
		if (sw.ElapsedMilliseconds > 10)
			System.Diagnostics.Trace.WriteLine(string.Format("[GridView.OnEditingContextChanged] Total: {0} ms, Skins: {1}, Slowest skin: '{2}' took {3} ms", sw.ElapsedMilliseconds, skinCount, skinMaxName, skinMax));
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		bool applyInitialLayout = base.Visible && !EditingContextRendered;
		base.OnVisibleChanged(e);
		if (applyInitialLayout)
		{
			ApplyAllLayoutState();
		}
		else if (!base.Visible)
		{
			ClearToolTips();
		}
	}

	protected override Control GetEditingControl(Property property)
	{
		if (property.DisableEditing)
		{
			return null;
		}
		Control control = base.GetEditingControl(property);
		if (control == null)
		{
			PropertyEditingControl propertyEditingControl = new PropertyEditingControl();
			propertyEditingControl.Bind(property.Context);
			propertyEditingControl.Height = RowHeight;
			control = propertyEditingControl;
		}
		else
		{
			control.Height = RowHeight;
		}
		return control;
	}

	protected void SetPropertyColumnWidth(Property property, int width)
	{
		if (m_columnWidths.ContainsKey(property.Descriptor.Name))
		{
			m_columnWidths[property.Descriptor.Name] = width;
		}
		else
		{
			m_columnWidths.Add(property.Descriptor.Name, width);
		}
		m_columnInfo[property.Descriptor].Width = width;
	}

	private void vScrollBar_ValueChanged(object sender, EventArgs e)
	{
		m_vScroll = m_vScrollBar.Value;
		Invalidate();
	}

	private void hScrollBar_ValueChanged(object sender, EventArgs e)
	{
		m_hScroll = m_hScrollBar.Value;
		Invalidate();
	}

	private void selectedRows_Changed(object sender, EventArgs e)
	{
		if (m_selectedRows.Count == 0)
		{
			m_editingRowVisible = false;
		}
		Invalidate();
		if (m_editingRowVisible)
		{
			foreach (Property property in base.Properties)
			{
				if (property.Control != null && property.Control.Visible)
				{
					property.Control.Refresh();
				}
			}
			if (base.SelectedProperty != null)
			{
				base.SelectedProperty.Control.Select();
			}
		}
		OnSelectedRowsChanged(e);
	}

	private void propertyEditingControl_PropertyEdited(object sender, PropertyEditedEventArgs e)
	{
		int num = 0;
		foreach (object item in base.EditingContext.Items)
		{
			if (item.Equals(e.Owner))
			{
				OnRowValueChanged(new RowChangedEventArgs(num));
				break;
			}
			num++;
		}
	}

	private void AddLostFocusHandlerRecursively(Control control)
	{
		control.LostFocus -= propertyEditingControl_LostFocus;
		control.LostFocus += propertyEditingControl_LostFocus;
		foreach (Control control2 in control.Controls)
		{
			AddLostFocusHandlerRecursively(control2);
		}
		if (!(control is IFormsOwner formsOwner))
		{
			return;
		}
		foreach (Form form in formsOwner.Forms)
		{
			AddLostFocusHandlerRecursively(form);
		}
	}

	private void LeaveEditModeWhenGrinDoesNotContainFocus()
	{
		bool flag = base.ContainsFocus;
		if (!flag)
		{
			foreach (Property property in base.Properties)
			{
				if (property.Control == null)
				{
					continue;
				}
				flag |= property.Control.ContainsFocus;
				if (property.Control is IFormsOwner formsOwner)
				{
					foreach (Form form in formsOwner.Forms)
					{
						flag |= form.ContainsFocus;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (!flag)
		{
			CancelEdit();
		}
	}

	private void propertyEditingControl_LostFocus(object sender, EventArgs e)
	{
		LeaveEditModeWhenGrinDoesNotContainFocus();
	}

	private int GetColumnLeft(Property property)
	{
		int num = GripWidth;
		foreach (Property property2 in base.Properties)
		{
			if (property2.FirstInCategory && !property2.Category.Expanded)
			{
				num += RowHeight;
			}
			if (property2 == property)
			{
				return num;
			}
			if (GetVisible(property2))
			{
				int num2 = GetColumnInfo(property2).Width;
				num += num2;
			}
		}
		return num;
	}

	protected override void RefreshEditingControls()
	{
		if (m_editingRowVisible)
		{
			base.RefreshEditingControls();
		}
	}

	private void TurnOnEditingRow()
	{
		if (m_editingRowVisible)
		{
			return;
		}
		m_editingRowVisible = true;
		foreach (Property property in base.Properties)
		{
			if (property.Control != null && GetVisible(property))
			{
				property.Control.Visible = true;
				m_toolTip.SetToolTip(property.Control, property.Descriptor.Description);
				property.Control.Refresh();
			}
		}
	}

	private void TurnOffEditingRow()
	{
		if (!m_editingRowVisible)
		{
			return;
		}
		m_editingRowVisible = false;
		foreach (Property property in base.Properties)
		{
			if (property.Control != null)
			{
				property.Control.Visible = false;
			}
		}
		ClearToolTips();
	}

	private void EditProperty(Property property)
	{
		TurnOnEditingRow();
		if (property.Control != null)
		{
			SkinService.ApplyActiveSkin(property.Control);
			property.Control.Select();
			if (property.Control.CanFocus)
			{
				property.Control.Focus();
			}
		}
		foreach (Property property2 in base.Properties)
		{
			if (property2.Control != null && property2.Control.Visible)
			{
				property2.Control.Refresh();
			}
		}
	}

	private void CommitEdit()
	{
		LoseFocusOnEditingControl();
		TurnOffEditingRow();
		if (base.CanFocus)
		{
			Focus();
		}
	}

	private void CancelEdit()
	{
		TurnOffEditingRow();
		if (base.CanFocus)
		{
			Focus();
		}
	}

	private HitRecord Pick(Point pt)
	{
		HitRecord hitRecord = new HitRecord();
		if (pt.Y <= HeaderHeight && pt.X < GripWidth)
		{
			hitRecord.Offset = new Point(pt.X, pt.Y);
			return hitRecord;
		}
		int rowAtY = GetRowAtY(pt.Y);
		if (pt.Y > HeaderHeight && pt.X < GripWidth && rowAtY < base.SelectedObjects.Length)
		{
			hitRecord.Offset = new Point(pt.X, pt.Y % RowHeight);
			hitRecord.Type = HitType.Row;
			hitRecord.Row = rowAtY;
			return hitRecord;
		}
		int num = -HScroll;
		int left = base.Margin.Left;
		foreach (Property property in base.Properties)
		{
			if (property.FirstInCategory)
			{
				if (pt.Y < HeaderHeight && pt.X >= num && pt.X <= num + 8 + 2 * left && m_categorySettings != PropertyCategorySettings.Disabled)
				{
					hitRecord.Type = HitType.CategoryExpander;
					hitRecord.Property = property;
					hitRecord.Category = property.Category;
					hitRecord.Row = -1;
					hitRecord.Offset = new Point(pt.X - num, pt.Y);
					return hitRecord;
				}
				if (!GetVisible(property))
				{
					num += RowHeight;
				}
			}
			if (!GetVisible(property))
			{
				continue;
			}
			int num2 = GetColumnInfo(property).Width;
			if (pt.X >= num && pt.X <= num + num2)
			{
				hitRecord.Property = property;
				if (pt.Y >= 0 && pt.Y < HeaderHeight)
				{
					if (Math.Abs(num + num2 - pt.X) < PropertyView.SystemDragSize.Width)
					{
						hitRecord.Type = HitType.ColumnHeaderRightEdge;
						hitRecord.Offset = new Point(pt.X - num, pt.Y);
						return hitRecord;
					}
					hitRecord.Type = HitType.ColumnHeader;
					hitRecord.Offset = new Point(pt.X - num, pt.Y);
					return hitRecord;
				}
				if (rowAtY < base.SelectedObjects.Length)
				{
					hitRecord.Offset = new Point(pt.X - num, pt.Y % RowHeight);
					hitRecord.Type = HitType.Cell;
					hitRecord.Row = rowAtY;
				}
				return hitRecord;
			}
			num += num2;
		}
		return hitRecord;
	}

	private ColumnInfo GetColumnInfo(Property p)
	{
		if (!m_columnInfo.TryGetValue(p.Descriptor, out var value))
		{
			value = new ColumnInfo();
			value.Width = 100;
			int value2;
			if (p.Control != null && p.Control.Width > 0)
			{
				value.Width = p.Control.Width;
			}
			else if (m_savedColumnWidths.TryGetValue(p.Descriptor.Name + p.Descriptor.PropertyType, out value2))
			{
				value.Width = value2;
			}
			else if (p.DefaultWidth != 0)
			{
				value.Width = p.DefaultWidth;
			}
			m_columnInfo.Add(p.Descriptor, value);
		}
		return value;
	}

	private void PositionControls()
	{
		int lastSelected = m_selectedRows.LastSelected;
		int top = -m_vScroll + HeaderHeight + lastSelected * RowHeight + 1;
		int num = -HScroll;
		int num2 = 0;
		foreach (Property property in base.Properties)
		{
			if (property.Control != null)
			{
				property.Control.Visible = m_editingRowVisible && num >= 0 && GetVisible(property);
			}
			int num3 = 0;
			if (GetVisible(property))
			{
				num3 = GetColumnInfo(property).Width;
				if (property.Control != null)
				{
					property.Control.Top = top;
					if (num >= GripWidth)
					{
						property.Control.Left = num;
						property.Control.Width = num3 - 1;
					}
					else
					{
						property.Control.Left = GripWidth;
						property.Control.Width = num3 - (GripWidth - num) - 1;
					}
					property.Control.TabIndex = num2++;
					m_toolTip.SetToolTip(property.Control, property.Descriptor.Description);
				}
			}
			else if (property.FirstInCategory)
			{
				num3 = RowHeight;
			}
			num += num3;
		}
	}

	private int GetRowAtY(int y)
	{
		int num = -m_vScroll + HeaderHeight;
		if (m_editingRowVisible)
		{
			int lastSelected = m_selectedRows.LastSelected;
			int num2 = num + lastSelected * RowHeight;
			if (y > num2)
			{
				int num3 = num2 + EditingRowHeight;
				if (y < num3)
				{
					return lastSelected;
				}
				return lastSelected + (y - num3) / RowHeight + 1;
			}
		}
		return (y - num) / RowHeight;
	}

	private int GetRowY(int row)
	{
		int num = -m_vScroll + HeaderHeight + row * RowHeight;
		if (m_editingRowVisible && m_selectedRows.LastSelected < row)
		{
			num += EditingRowHeight - RowHeight;
		}
		return num;
	}

	protected override void ReadSettings(XmlElement root)
	{
		XmlNodeList xmlNodeList = root.SelectNodes("PropertyDescriptors");
		if (xmlNodeList == null || xmlNodeList.Count == 0)
		{
			return;
		}
		if (xmlNodeList.Count > 1)
		{
			throw new Exception("Duplicated PropertyDescriptors settings");
		}
		XmlElement xmlElement = (XmlElement)xmlNodeList[0];
		foreach (XmlElement item in xmlElement)
		{
			string attribute = item.GetAttribute("Name");
			string attribute2 = item.GetAttribute("PropertyType");
			string attribute3 = item.GetAttribute("Width");
			if (attribute3 != null && int.TryParse(attribute3, out var result))
			{
				m_savedColumnWidths[attribute + attribute2] = result;
			}
		}
	}

	protected override void WriteSettings(XmlElement root)
	{
		XmlDocument ownerDocument = root.OwnerDocument;
		XmlElement xmlElement = ownerDocument.CreateElement("PropertyDescriptors");
		root.AppendChild(xmlElement);
		foreach (KeyValuePair<PropertyDescriptor, ColumnInfo> item in m_columnInfo)
		{
			XmlElement xmlElement2 = ownerDocument.CreateElement("Descriptor");
			xmlElement2.SetAttribute("Name", item.Key.Name);
			xmlElement2.SetAttribute("PropertyType", item.Key.PropertyType.ToString());
			xmlElement2.SetAttribute("Width", item.Value.Width.ToString());
			xmlElement.AppendChild(xmlElement2);
		}
	}

	private static bool CanSort(Property property)
	{
		return !property.DisableSort && typeof(IComparable).IsAssignableFrom(property.Descriptor.PropertyType);
	}

	private IComparer GetComparer(PropertyDescriptor descriptor, bool ascending)
	{
		IComparer comparer = null;
		if (descriptor is IPropertyCustomSorter propertyCustomSorter)
		{
			comparer = propertyCustomSorter.GetComparer(ascending);
		}
		if (comparer == null)
		{
			comparer = new PropertyComparer(descriptor, ascending);
		}
		return comparer;
	}

	private void Sort(Property property, bool ascending)
	{
		List<object> list = new List<object>();
		foreach (int selectedRow in m_selectedRows)
		{
			if (selectedRow >= base.SelectedObjects.Length)
			{
				Outputs.WriteLine(OutputMessageType.Error, "Row has been selected that is beyond the range of the selected objects array.");
			}
			else
			{
				list.Add(base.SelectedObjects[selectedRow]);
			}
		}
		IComparer comparer = GetComparer(property.Descriptor, ascending);
		if (comparer is IComparer<object>)
		{
			object[] array = base.SelectedObjects.OrderBy((object x) => x, comparer as IComparer<object>).ToArray();
			for (int num = 0; num < array.Length; num++)
			{
				base.SelectedObjects[num] = array[num];
			}
		}
		else
		{
			Array.Sort(base.SelectedObjects, comparer);
		}
		m_lastSortProperty = property;
		m_sortByPropertyName = m_lastSortProperty.Descriptor.Name;
		m_sortByPropertyDirection = ((!ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending);
		List<int> list2 = new List<int>();
		foreach (object item in list)
		{
			list2.Add(Array.IndexOf(base.SelectedObjects, item));
		}
		SelectRange(list2, Keys.None);
	}

	static GridView()
	{
		s_verticalFormat = new StringFormat(StringFormatFlags.DirectionVertical | StringFormatFlags.NoWrap);
		s_verticalFormat.Alignment = StringAlignment.Near;
		s_verticalFormat.Trimming = StringTrimming.EllipsisCharacter;
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.PropertyEditing;

public class PropertyGridView : PropertyView
{
	private Color m_oddRowColor = Color.SteelBlue;

	private Color m_evenRowColor = Color.LightSteelBlue;

	private Brush m_categoryBackgroundBrush;

	private Brush m_categoryNameBrush;

	private Pen m_categoryLinePen;

	private Brush m_categoryExpanderBrush;

	private Brush m_propertyBackgroundBrush;

	private Brush m_propertyBackgroundHighlightBrush;

	private Brush m_propertyTextBrush;

	private Brush m_propertyReadOnlyTextBrush;

	private Brush m_propertyTextHighlightBrush;

	private Brush m_propertyExpanderBrush;

	private Pen m_propertyLinePen;

	private readonly Stack<IPropertyEditingContext> m_history = new Stack<IPropertyEditingContext>();

	private float m_splitterAmount = 0.5f;

	private Property m_hoverProperty;

	private Point m_mouseDown;

	private readonly ToolTip m_toolTip;

	private readonly PropertyEditingControl m_editingControl;

	private readonly OverlayButton m_resetButton;

	private readonly VScrollBar m_scrollBar;

	private int m_scroll;

	private bool m_dragging;

	private bool m_openingComposite;

	private bool m_allowTooltips;

	private bool m_allowEditingComposites;

	private bool m_showScrollbar = true;

	private bool m_autoSizeColumns = true;

	public bool ShowLabels { get; set; }

	public bool ShowRowStriping { get; set; }

	public bool ShowResetButton { get; set; }

	public bool ShowCopyButton { get; set; }

	public bool ShowScrollbar
	{
		get
		{
			return m_showScrollbar;
		}
		set
		{
			m_showScrollbar = value;
		}
	}

	public bool ShowCategories => (base.PropertySorting & (PropertySorting.Categorized | PropertySorting.CategoryAlphabetical)) != 0;

	public Color OddRowColor
	{
		get
		{
			return m_oddRowColor;
		}
		set
		{
			if (!(m_oddRowColor == value))
			{
				m_oddRowColor = value;
				OddRowBrush?.Dispose();
				OddRowBrush = new SolidBrush(m_oddRowColor);
			}
		}
	}

	public Color EvenRowColor
	{
		get
		{
			return m_evenRowColor;
		}
		set
		{
			if (!(m_evenRowColor == value))
			{
				m_evenRowColor = value;
				EvenRowBrush?.Dispose();
				EvenRowBrush = new SolidBrush(m_evenRowColor);
			}
		}
	}

	private Brush EvenRowBrush { get; set; }

	private Brush OddRowBrush { get; set; }

	public bool AutoSizeColumns
	{
		get
		{
			return m_autoSizeColumns;
		}
		set
		{
			m_autoSizeColumns = value;
		}
	}

	public bool AllowTooltips
	{
		get
		{
			return m_allowTooltips;
		}
		set
		{
			m_allowTooltips = value;
		}
	}

	public bool AllowEditingComposites
	{
		get
		{
			return m_allowEditingComposites;
		}
		set
		{
			m_allowEditingComposites = value;
			if (!m_allowEditingComposites)
			{
				m_history.Clear();
			}
		}
	}

	public bool CanNavigateBack => m_history.Count > 0;

	public override bool AllowDrop
	{
		set
		{
			base.AllowDrop = value;
			m_editingControl.AllowDrop = value;
		}
	}

	private IEnumerable<Pair<object, int>> ItemPositions
	{
		get
		{
			int top = 0;
			foreach (object obj in VisibleItems)
			{
				if (obj is Category)
				{
					yield return new Pair<object, int>(obj, top);
					top += RowHeight;
				}
				else
				{
					Property property = (Property)obj;
					yield return new Pair<object, int>(obj, top);
					top += GetRowHeight(property);
				}
			}
		}
	}

	public Brush CategoryBackgroundBrush
	{
		get
		{
			return m_categoryBackgroundBrush;
		}
		set
		{
			if (m_categoryBackgroundBrush != null)
			{
				m_categoryBackgroundBrush.Dispose();
			}
			m_categoryBackgroundBrush = value;
		}
	}

	public Brush CategoryNameBrush
	{
		get
		{
			return m_categoryNameBrush;
		}
		set
		{
			if (m_categoryNameBrush != null)
			{
				m_categoryNameBrush.Dispose();
			}
			m_categoryNameBrush = value;
		}
	}

	public Pen CategoryLinePen
	{
		get
		{
			return m_categoryLinePen;
		}
		set
		{
			if (m_categoryLinePen != null)
			{
				m_categoryLinePen.Dispose();
			}
			m_categoryLinePen = value;
		}
	}

	public Brush CategoryExpanderBrush
	{
		get
		{
			return m_categoryExpanderBrush;
		}
		set
		{
			if (m_categoryExpanderBrush != null)
			{
				m_categoryExpanderBrush.Dispose();
			}
			m_categoryExpanderBrush = value;
		}
	}

	public Brush PropertyBackgroundBrush
	{
		get
		{
			return m_propertyBackgroundBrush;
		}
		set
		{
			if (m_propertyBackgroundBrush != null)
			{
				m_propertyBackgroundBrush.Dispose();
			}
			m_propertyBackgroundBrush = value;
		}
	}

	public Brush PropertyBackgroundHighlightBrush
	{
		get
		{
			return m_propertyBackgroundHighlightBrush;
		}
		set
		{
			if (m_propertyBackgroundHighlightBrush != null)
			{
				m_propertyBackgroundHighlightBrush.Dispose();
			}
			m_propertyBackgroundHighlightBrush = value;
		}
	}

	public Brush PropertyTextBrush
	{
		get
		{
			return m_propertyTextBrush;
		}
		set
		{
			if (m_propertyTextBrush != null)
			{
				m_propertyTextBrush.Dispose();
			}
			m_propertyTextBrush = value;
		}
	}

	public Brush PropertyReadOnlyTextBrush
	{
		get
		{
			return m_propertyReadOnlyTextBrush;
		}
		set
		{
			if (m_propertyReadOnlyTextBrush != null)
			{
				m_propertyReadOnlyTextBrush.Dispose();
			}
			m_propertyReadOnlyTextBrush = value;
		}
	}

	public Brush PropertyTextHighlightBrush
	{
		get
		{
			return m_propertyTextHighlightBrush;
		}
		set
		{
			if (m_propertyTextHighlightBrush != null)
			{
				m_propertyTextHighlightBrush.Dispose();
			}
			m_propertyTextHighlightBrush = value;
		}
	}

	public Brush PropertyExpanderBrush
	{
		get
		{
			return m_propertyExpanderBrush;
		}
		set
		{
			if (m_propertyExpanderBrush != null)
			{
				m_propertyExpanderBrush.Dispose();
			}
			m_propertyExpanderBrush = value;
		}
	}

	public Pen PropertyLinePen
	{
		get
		{
			return m_propertyLinePen;
		}
		set
		{
			if (m_propertyLinePen != null)
			{
				m_propertyLinePen.Dispose();
			}
			m_propertyLinePen = value;
		}
	}

	public Action<System.ComponentModel.PropertyDescriptor> DescriptionSetter { get; set; }

	public PropertyGridView()
	{
		SuspendLayout();
		m_resetButton = new OverlayButton(this);
		m_resetButton.ToolTipText = "Resets the property to its default value".Localize();
		m_resetButton.BackgroundImage = ResourceUtil.GetImage16(Resources.ResetImage);
		m_scrollBar = new VScrollBar();
		m_scrollBar.Dock = DockStyle.Right;
		m_scrollBar.ValueChanged += scrollBar_ValueChanged;
		m_toolTip = new ToolTip();
		m_toolTip.AutoPopDelay = 1750;
		m_toolTip.InitialDelay = 750;
		m_toolTip.ReshowDelay = 500;
		m_toolTip.ShowAlways = true;
		m_editingControl = new PropertyEditingControl();
		m_editingControl.TabStop = false;
		m_editingControl.DragOver += editingControl_DragOver;
		m_editingControl.DragDrop += editingControl_DragDrop;
		m_editingControl.MouseHover += editingControl_MouseHover;
		m_editingControl.MouseLeave += editingControl_MouseLeave;
		CategoryBackgroundBrush = new SolidBrush(SystemColors.ControlLight);
		CategoryNameBrush = new SolidBrush(SystemColors.ControlText);
		CategoryLinePen = new Pen(SystemColors.Control);
		CategoryExpanderBrush = new SolidBrush(SystemColors.ControlDarkDark);
		PropertyBackgroundBrush = new SolidBrush(SystemColors.Window);
		PropertyBackgroundHighlightBrush = new SolidBrush(SystemColors.Highlight);
		PropertyTextBrush = new SolidBrush(SystemColors.ControlText);
		PropertyReadOnlyTextBrush = new SolidBrush(SystemColors.GrayText);
		PropertyTextHighlightBrush = new SolidBrush(SystemColors.HighlightText);
		PropertyExpanderBrush = new SolidBrush(SystemColors.ControlDarkDark);
		PropertyLinePen = new Pen(SystemColors.Control);
		EvenRowBrush = new SolidBrush(EvenRowColor);
		OddRowBrush = new SolidBrush(ColorUtil.GetShade(OddRowColor, 1.2f));
		base.EditingContext = null;
		Font = new Font("Segoe UI", 9f);
		ShowLabels = true;
		ShowResetButton = true;
		ShowCopyButton = true;
		base.Controls.AddRange(new Control[2] { m_editingControl, m_scrollBar });
		ResumeLayout();
		m_resetButton.Click += delegate
		{
			base.SelectedProperty.Context.ResetValue();
			if (!base.EditingContext.Is<IObservableContext>())
			{
				RefreshEditingControls();
			}
		};
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			EvenRowBrush?.Dispose();
			OddRowBrush?.Dispose();
			CategoryBackgroundBrush?.Dispose();
			CategoryNameBrush?.Dispose();
			CategoryLinePen?.Dispose();
			CategoryExpanderBrush?.Dispose();
			PropertyBackgroundBrush?.Dispose();
			PropertyBackgroundHighlightBrush?.Dispose();
			PropertyTextBrush?.Dispose();
			PropertyReadOnlyTextBrush?.Dispose();
			PropertyTextHighlightBrush?.Dispose();
			PropertyExpanderBrush?.Dispose();
			PropertyLinePen?.Dispose();
			m_resetButton?.Dispose();
			m_toolTip?.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void ReadSettings(XmlElement root)
	{
		if (!AutoSizeColumns)
		{
			string attribute = root.GetAttribute("SplitterAmount");
			if (float.TryParse(attribute, out var result))
			{
				m_splitterAmount = result;
			}
		}
	}

	protected override void WriteSettings(XmlElement root)
	{
		if (!AutoSizeColumns)
		{
			root.SetAttribute("SplitterAmount", m_splitterAmount.ToString());
		}
	}

	public System.ComponentModel.PropertyDescriptor GetDescriptorAt(Point clientPoint)
	{
		int bottom;
		return GetDescriptorAt(clientPoint, out bottom);
	}

	public System.ComponentModel.PropertyDescriptor GetDescriptorAt(Point clientPoint, out IPropertyEditingContext editingContext)
	{
		int bottom;
		object obj = Pick(clientPoint, out bottom, out editingContext);
		if (!(obj is Property { Descriptor: var descriptor }))
		{
			return null;
		}
		return descriptor;
	}

	public System.ComponentModel.PropertyDescriptor GetDescriptorAt(Point clientPoint, out int bottom)
	{
		IPropertyEditingContext editingContext;
		object obj = Pick(clientPoint, out bottom, out editingContext);
		if (!(obj is Property { Descriptor: var descriptor }))
		{
			return null;
		}
		return descriptor;
	}

	public object Pick(Point clientPnt, out int bottom, out IPropertyEditingContext editingContext)
	{
		editingContext = base.EditingContext;
		int num = (bottom = -m_scroll);
		int middleX = GetMiddleX();
		foreach (object visibleItem in base.VisibleItems)
		{
			if (visibleItem is Category)
			{
				num += RowHeight;
				if (clientPnt.Y < num)
				{
					return visibleItem;
				}
				continue;
			}
			Property property = (Property)visibleItem;
			int rowHeight = GetRowHeight(property);
			num += rowHeight;
			if (clientPnt.Y >= num)
			{
				continue;
			}
			bottom = num;
			int num2 = ((property.HorizontalEditorOffset >= 0) ? property.HorizontalEditorOffset : middleX);
			if (property.Control != null && clientPnt.X > num2 && (!property.NameHasWholeRow || clientPnt.Y >= num - rowHeight + RowHeight))
			{
				foreach (PropertyGridView item in FindChildControls<PropertyGridView>(property.Control))
				{
					if (item != null)
					{
						Point p = PointToScreen(clientPnt);
						Point clientPnt2 = item.PointToClient(p);
						int bottom2;
						IPropertyEditingContext editingContext2;
						object obj = item.Pick(clientPnt2, out bottom2, out editingContext2);
						if (obj != null)
						{
							Point p2 = new Point(0, bottom2);
							Point p3 = item.PointToScreen(p2);
							bottom = PointToClient(p3).Y;
							editingContext = editingContext2;
							return obj;
						}
					}
				}
			}
			return visibleItem;
		}
		return null;
	}

	public void NavigateBack()
	{
		if (m_history.Count == 0)
		{
			throw new InvalidOperationException("No history");
		}
		IPropertyEditingContext editingContext = m_history.Pop();
		try
		{
			m_openingComposite = true;
			base.EditingContext = editingContext;
		}
		finally
		{
			m_openingComposite = false;
		}
	}

	public int GetMiddleX()
	{
		int num = base.Width;
		if (m_scrollBar.Visible)
		{
			num -= m_scrollBar.Width;
		}
		return (int)(m_splitterAmount * (float)num);
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (keyData == Keys.Down || keyData == Keys.Tab)
		{
			Property nextProperty = GetNextProperty(base.SelectedProperty);
			if (nextProperty != null)
			{
				StartPropertyEdit(nextProperty);
				TryMakeSelectionVisible();
				return true;
			}
		}
		if (keyData == Keys.Up || keyData == (Keys.Tab | Keys.Shift))
		{
			Property previousProperty = GetPreviousProperty(base.SelectedProperty);
			if (previousProperty != null)
			{
				StartPropertyEdit(previousProperty, fromEnd: true);
				TryMakeSelectionVisible();
				return true;
			}
		}
		return base.ProcessDialogKey(keyData);
	}

	protected override void OnSelectedPropertyChanged(EventArgs e)
	{
		base.OnSelectedPropertyChanged(e);
		if (base.SelectedProperty == null)
		{
			m_editingControl.Hide();
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		m_mouseDown = new Point(e.X, e.Y);
		if (m_resetButton.MouseDown(e))
		{
			Invalidate();
			return;
		}
		int middleX = GetMiddleX();
		bool flag = Math.Abs(e.X - middleX) <= PropertyView.SystemDragSize.Width;
		int bottom;
		IPropertyEditingContext editingContext;
		object obj = Pick(e.Location, out bottom, out editingContext);
		if (obj is Category)
		{
			if (!flag)
			{
				if (e.Button == MouseButtons.Left)
				{
					Category category = obj as Category;
					category.Expanded = !category.Expanded;
				}
				Select();
				m_editingControl.Hide();
				Refresh();
			}
		}
		else if (obj is Property)
		{
			Property property = obj as Property;
			if (e.X < middleX && property.ChildProperties != null && property.ChildProperties.Count > 0)
			{
				if (!flag)
				{
					if (e.Button == MouseButtons.Left)
					{
						property.ChildrenExpanded = !property.ChildrenExpanded;
					}
					Select();
					m_editingControl.Hide();
					if (DescriptionSetter != null)
					{
						DescriptionSetter(property.Descriptor);
					}
					Refresh();
				}
			}
			else if (!flag || property.HorizontalEditorOffset >= 0)
			{
				Select();
				m_editingControl.Hide();
				if (DescriptionSetter != null)
				{
					DescriptionSetter(property.Descriptor);
				}
				StartPropertyEdit(property);
			}
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (!m_dragging && m_resetButton.MouseMove(e))
		{
			Cursor = Cursors.Arrow;
			return;
		}
		base.OnMouseMove(e);
		int middleX = GetMiddleX();
		int bottom;
		IPropertyEditingContext editingContext;
		Property property = Pick(e.Location, out bottom, out editingContext) as Property;
		bool flag = property == null || property.HorizontalEditorOffset < 0;
		if (!m_dragging && e.Button == MouseButtons.Left)
		{
			m_dragging = flag && Math.Abs(m_mouseDown.X - middleX) <= PropertyView.SystemDragSize.Width;
		}
		if (property != m_hoverProperty)
		{
			m_hoverProperty = property;
			if (m_allowTooltips)
			{
				m_toolTip.RemoveAll();
				if (m_hoverProperty != null && !m_dragging)
				{
					m_toolTip.Active = false;
					m_toolTip.SetToolTip(this, property?.Descriptor?.Description ?? string.Empty);
					m_toolTip.Active = true;
				}
			}
		}
		if (base.EditingContext == null || !ShowLabels)
		{
			return;
		}
		if (!m_dragging)
		{
			Cursor cursor = Cursors.Arrow;
			if (flag && Math.Abs(e.X - middleX) < PropertyView.SystemDragSize.Width)
			{
				cursor = Cursors.VSplit;
			}
			if (cursor != Cursor)
			{
				Cursor = cursor;
			}
			return;
		}
		int num = base.Width;
		if (m_scrollBar.Visible)
		{
			num -= m_scrollBar.Width;
		}
		float val = (float)e.X / (float)num;
		val = Math.Max(0.0625f, val);
		val = Math.Min(0.875f, val);
		if (m_splitterAmount != val)
		{
			m_splitterAmount = val;
			Invalidate();
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		m_resetButton.MouseUp(e);
		m_dragging = false;
		Invalidate();
		base.OnMouseUp(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		Cursor = Cursors.Arrow;
		ClearToolTips();
		base.OnMouseLeave(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		int verticalScroll = m_scrollBar.Value - e.Delta / 2;
		SetVerticalScroll(verticalScroll);
		base.OnMouseWheel(e);
	}

	private void ClearToolTips()
	{
		m_hoverProperty = null;
		m_toolTip?.RemoveAll();
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		Invalidate();
		base.OnLayout(levent);
	}

	private int ComputeCategoryIndent(Category cat, int indent)
	{
		int num = 0;
		for (Category category = cat?.Parent; category != null; category = category.Parent)
		{
			num += indent;
		}
		return num;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		m_resetButton.Visible = false;
		SuspendLayout();
		Graphics graphics = e.Graphics;
		UpdateScrollbar();
		m_scrollBar.Refresh();
		int num = -m_scroll;
		int num2 = base.Width;
		if (m_scrollBar.Visible)
		{
			num2 -= m_scrollBar.Width;
		}
		int middleX = GetMiddleX();
		int num3 = (ShowLabels ? (middleX + 1) : 0);
		int num4 = 0;
		bool flag = true;
		foreach (object item in base.Items)
		{
			flag = !flag;
			if (item is Category category)
			{
				if (category.Visible)
				{
					int num5 = ComputeCategoryIndent(category, 13);
					DrawCategoryRow(category, category.Expanded, num5, num, num2 - num5, graphics);
					num += RowHeight;
				}
				continue;
			}
			Property property = (Property)item;
			int num6 = middleX + 1;
			if (property.HorizontalEditorOffset >= 0)
			{
				num6 = Math.Min(property.HorizontalEditorOffset, num6);
			}
			if (property.Control != null)
			{
				bool visible = property.Visible;
				if (visible)
				{
					int num7 = num2 - Math.Max(num6, 1);
					property.Control.Top = num + (property.NameHasWholeRow ? RowHeight : 0);
					property.Control.Left = num6;
					property.Control.Width = num7;
					property.Control.TabIndex = num4++;
				}
				property.Control.Visible = visible;
			}
			if (property.Visible)
			{
				int num8 = ComputeCategoryIndent(property.Category, 13);
				int middle = num6 - 1;
				DrawPropertyRow(flag, property, num8, num, num2 - num8, middle, graphics);
				num += GetRowHeight(property);
			}
		}
		SetEditingControlTop();
		m_editingControl.Left = num3;
		m_editingControl.Width = num2 - num3;
		m_editingControl.TabIndex = num4++;
		m_editingControl.Font = Font;
		ResumeLayout();
	}

	public override void Refresh()
	{
		FlushEditingControl();
		base.Refresh();
	}

	protected override void OnEditingContextChanging()
	{
		if (!m_openingComposite)
		{
			m_history.Clear();
		}
		FlushEditingControl();
		m_editingControl.Hide();
	}

	protected override void OnEditingContextChanged()
	{
		foreach (Property property in base.Properties)
		{
			Control control = property.Control;
			if (control != null)
			{
				control.SizeChanged -= control_SizeChanged;
				control.SizeChanged += control_SizeChanged;
				control.Enter -= control_Enter;
				control.Enter += control_Enter;
				control.MouseUp -= control_MouseUp;
				control.MouseUp += control_MouseUp;
			}
		}
		if (AutoSizeColumns)
		{
			AutoAdjustSplitter();
		}
		Invalidate();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		if (AutoSizeColumns)
		{
			AutoAdjustSplitter();
		}
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		if (AutoSizeColumns)
		{
			AutoAdjustSplitter();
		}
	}

	private void AutoAdjustSplitter()
	{
		if (!base.Properties.Any())
		{
			return;
		}
		int num = 16;
		foreach (Property property in base.Properties)
		{
			int num2 = 8;
			num2 = ((property.Descriptor is ICustomPropertyDisplayName customPropertyDisplayName) ? (num2 + TextRenderer.MeasureText(customPropertyDisplayName.GetDisplayName(base.LastSelectedObject), Font).Width) : (num2 + TextRenderer.MeasureText(property.Descriptor.DisplayName, Font).Width));
			if (num2 > num)
			{
				num = num2;
			}
		}
		m_splitterAmount = Math.Max(0.0625f, Math.Min(0.65f, (float)num / (float)base.Width));
	}

	private void scrollBar_ValueChanged(object sender, EventArgs e)
	{
		m_scroll = m_scrollBar.Value;
		Invalidate();
	}

	private void control_SizeChanged(object sender, EventArgs e)
	{
		Invalidate();
	}

	private void control_Enter(object sender, EventArgs e)
	{
		Control control = (Control)sender;
		foreach (Property property in base.Properties)
		{
			if (property.Control == control && property != base.SelectedProperty)
			{
				StartPropertyEdit(property);
				break;
			}
		}
	}

	private void control_MouseUp(object sender, MouseEventArgs e)
	{
		Point p = ((Control)sender).PointToScreen(e.Location);
		Point point = PointToClient(p);
		MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta);
		OnMouseUp(e2);
	}

	private void editingControl_DragOver(object sender, DragEventArgs e)
	{
		OnDragOver(e);
	}

	private void editingControl_DragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void editingControl_MouseHover(object sender, EventArgs e)
	{
		OnMouseHover(e);
	}

	private void editingControl_MouseLeave(object sender, EventArgs e)
	{
		OnMouseLeave(e);
	}

	private void StartPropertyEdit(Property property, bool fromEnd = false)
	{
		Select();
		base.SelectedProperty = property;
		if (property.Control != null)
		{
			property.Control.Select();
			if (property.Control.Controls.Count > 0)
			{
				if (fromEnd)
				{
					for (int num = property.Control.Controls.Count - 1; num >= 0; num--)
					{
						Control control = property.Control.Controls[num];
						if (control.CanFocus && !(control is ToolStrip))
						{
							control.Focus();
							break;
						}
					}
				}
				else
				{
					foreach (Control control2 in property.Control.Controls)
					{
						if (control2.CanFocus && !(control2 is ToolStrip))
						{
							control2.Focus();
							break;
						}
					}
				}
			}
			else if (property.Control.CanFocus)
			{
				property.Control.Focus();
			}
			m_editingControl.Hide();
		}
		else if (!property.Descriptor.IsReadOnly)
		{
			m_editingControl.Bind(property.Context);
			SetEditingControlTop();
			m_editingControl.Show();
			m_editingControl.Focus();
		}
		Invalidate();
	}

	private void FlushEditingControl()
	{
		if (m_editingControl.Visible)
		{
			m_editingControl.Flush();
		}
	}

	protected override void RefreshEditingControls()
	{
		if (m_editingControl != null)
		{
			m_editingControl.Refresh();
		}
		base.RefreshEditingControls();
	}

	private void SetEditingControlTop()
	{
		if (base.SelectedProperty != null)
		{
			int top = -m_scroll + GetRowY(base.SelectedProperty);
			m_editingControl.Top = top;
		}
	}

	private IEnumerable<C> FindChildControls<C>(Control control) where C : Control
	{
		foreach (Control child in control.Controls)
		{
			if (child is C)
			{
				yield return (C)child;
			}
			foreach (C result in FindChildControls<C>(child))
			{
				if (result != null)
				{
					yield return result;
				}
			}
		}
	}

	private int GetRowY(Property property)
	{
		object item = null;
		int num = 0;
		foreach (Pair<object, int> itemPosition in ItemPositions)
		{
			item = itemPosition.First;
			num = itemPosition.Second;
			if (itemPosition.First is Property property2 && property2 == property)
			{
				return num;
			}
		}
		return num + GetRowHeight(item);
	}

	protected int GetRowHeight(object item)
	{
		if (item == null)
		{
			return 0;
		}
		if (item is Category)
		{
			return RowHeight;
		}
		return GetRowHeight((Property)item);
	}

	protected int GetRowHeight(Property property)
	{
		return Math.Max(val2: ((property.Control == null) ? m_editingControl : property.Control).Height + 1 + (property.NameHasWholeRow ? RowHeight : 0), val1: RowHeight);
	}

	protected int GetDepth(Property property)
	{
		int num = 0;
		while (property.Parent != null)
		{
			num++;
			property = property.Parent;
		}
		return num;
	}

	public int GetPreferredHeight()
	{
		int num = 0;
		foreach (Property property in base.Properties)
		{
			if (ShowCategories && property.FirstInCategory)
			{
				num += 8 + base.Margin.Top + base.Margin.Bottom;
			}
			if ((property.Category == null || property.Category.Expanded) && (property.Parent == null || property.Parent.ChildrenExpanded))
			{
				num += GetRowHeight(property);
			}
		}
		return num;
	}

	private void TryMakeSelectionVisible()
	{
		if (base.SelectedProperty == null)
		{
			return;
		}
		foreach (Pair<object, int> itemPosition in ItemPositions)
		{
			if (itemPosition.First is Property property && property == base.SelectedProperty)
			{
				int second = itemPosition.Second;
				if (second < m_scroll)
				{
					SetVerticalScroll(second);
				}
				else if (second + RowHeight > m_scroll + base.Height - RowHeight * 2)
				{
					SetVerticalScroll(second + RowHeight - (base.Height - RowHeight * 2));
				}
				break;
			}
		}
	}

	private void SetVerticalScroll(int scroll)
	{
		m_scrollBar.Value = Math.Max(m_scrollBar.Minimum, Math.Min(m_scrollBar.Maximum, scroll));
	}

	private void UpdateScrollbar()
	{
		int rowY = GetRowY(null);
		WinFormsUtil.UpdateScrollbars(m_scrollBar, null, new Size(0, base.Height), new Size(0, rowY));
		m_scrollBar.SmallChange = RowHeight;
		m_editingControl.Height = RowHeight;
	}

	public bool SelectProperty(System.ComponentModel.PropertyDescriptor descriptor)
	{
		Refresh();
		foreach (Pair<object, int> itemPosition in ItemPositions)
		{
			if (itemPosition.First is Property property && property.Descriptor.Equals(descriptor))
			{
				SetVerticalScroll(itemPosition.Second - 2 * RowHeight);
				StartPropertyEdit(property);
				return true;
			}
		}
		return false;
	}

	protected virtual void DrawCategoryRow(Category category, bool expanded, int x, int y, int width, Graphics g)
	{
		int left = base.Margin.Left;
		int num = (RowHeight - base.FontHeight) / 2;
		g.FillRectangle(CategoryBackgroundBrush, x, y, width, RowHeight - 1);
		GdiUtil.DrawExpander(x + left, y + (RowHeight - 8) / 2, expanded, g, CategoryExpanderBrush);
		int num2 = 8 + 2 * left;
		g.DrawString(layoutRectangle: new RectangleF(x + num2, y + num, width - num2, RowHeight), s: category.DisplayName, font: BoldFont, brush: CategoryNameBrush, format: PropertyView.LeftStringFormat);
		g.DrawLine(CategoryLinePen, x, y + RowHeight, x + width, y + RowHeight);
	}

	protected virtual void DrawPropertyRow(bool evenRow, Property property, int x, int y, int width, int middle, Graphics g)
	{
		Brush brush = ((!ShowRowStriping) ? PropertyBackgroundBrush : (evenRow ? EvenRowBrush : OddRowBrush));
		int rowHeight = GetRowHeight(property);
		g.FillRectangle(brush, x, y, width, rowHeight - 1);
		Brush brush2 = PropertyTextBrush;
		if (property == base.SelectedProperty && ShowLabels)
		{
			int num = (property.NameHasWholeRow ? width : (middle - x));
			g.FillRectangle(PropertyBackgroundHighlightBrush, x, y, num, rowHeight);
			brush2 = PropertyTextHighlightBrush;
		}
		int left = base.Margin.Left;
		int num2 = (RowHeight - base.FontHeight) / 2;
		int num3 = 0;
		Rectangle bounds = new Rectangle(x + left, y + num2, width - (x + left) - 1, rowHeight - 1);
		if (ShowLabels)
		{
			int num4 = GetDepth(property) + (ShowCategories ? 1 : 0);
			int num5 = 16 * num4;
			if (property.ChildProperties != null && property.ChildProperties.Count > 0)
			{
				GdiUtil.DrawExpander(x + num5, y + (RowHeight - 8) / 2, property.ChildrenExpanded, g, PropertyExpanderBrush);
			}
			int num6;
			if (property.NameHasWholeRow)
			{
				num6 = width - 2 * left;
				num3 = RowHeight + 1;
			}
			else
			{
				num6 = middle - 2 * left;
				num3 = 0;
			}
			Rectangle rectangle = new Rectangle(x + left, y + num2, num6, rowHeight - 1);
			if (!(property.Descriptor is ICustomPropertyDisplayName customPropertyDisplayName))
			{
				g.DrawString(property.Descriptor.Name, Font, brush2, rectangle, PropertyView.LeftStringFormat);
			}
			else
			{
				g.DrawString(customPropertyDisplayName.GetDisplayName(base.LastSelectedObject), Font, brush2, rectangle, PropertyView.LeftStringFormat);
			}
			bounds.X = middle + 1;
			bounds.Y = y + 1;
			bounds.Width = width - middle - 1;
			bounds.Height = rowHeight - 1;
		}
		if (property.Control == null)
		{
			Font font = (property.Descriptor.CanResetValue(base.LastSelectedObject) ? BoldFont : Font);
			Brush brush3 = (property.Descriptor.IsReadOnly ? PropertyReadOnlyTextBrush : PropertyTextBrush);
			TypeDescriptorContext context = new TypeDescriptorContext(base.LastSelectedObject, property.Descriptor, null);
			PropertyEditingControl.DrawProperty(property.Descriptor, context, bounds, font, brush3, g);
		}
		else if (ShowRowStriping)
		{
			property.Control.BackColor = (evenRow ? EvenRowColor : ColorUtil.GetShade(EvenRowColor, 1.2f));
		}
		if (ShowLabels)
		{
			g.DrawLine(PropertyLinePen, middle, y - 1 + num3, middle, y + rowHeight - 1);
		}
		g.DrawLine(PropertyLinePen, x, y + rowHeight - 1, width, y + rowHeight - 1);
		if (base.SelectedProperty == property && ShowResetButton && base.CanResetCurrent && middle > m_resetButton.Width)
		{
			m_resetButton.Visible = true;
			m_resetButton.Top = (rowHeight - m_resetButton.Height) / 2 + y;
			m_resetButton.Left = middle - m_resetButton.Width - 3;
			m_resetButton.Draw(g);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Controls;

public class DataBoundListView : ListView
{
	public class ListViewCellEventArgs : EventArgs
	{
		public int RowIndex;

		public int ColumnIndex;

		public ListViewCellEventArgs(int rowIndex, int columnIndex)
		{
			RowIndex = rowIndex;
			ColumnIndex = columnIndex;
		}
	}

	public class ListViewCellCancelEventArgs : CancelEventArgs
	{
		public int RowIndex;

		public int ColumnIndex;

		public ListViewCellCancelEventArgs(int rowIndex, int columnIndex)
		{
			RowIndex = rowIndex;
			ColumnIndex = columnIndex;
		}
	}

	public class ListViewCellValidatingEventArgs : CancelEventArgs
	{
		public int RowIndex;

		public int ColumnIndex;

		public object FormattedValue;

		public ListViewCellValidatingEventArgs(int rowIndex, int columnIndex, object formattedValue)
		{
			RowIndex = rowIndex;
			ColumnIndex = columnIndex;
			FormattedValue = formattedValue;
		}
	}

	private Color m_alternatingRowColor1 = Color.White;

	private Color m_alternatingRowColor2 = Color.FromArgb(233, 236, 241);

	private Color m_readOnlyTextColor;

	private Color m_externalEditorTextColor;

	private Color m_defaultBackColor;

	private Color m_highlightTextColor;

	private Color m_highlightBackColor;

	private Color m_normalTextColor;

	private Color m_columnHeaderTextColor;

	private Color m_columnHeaderTextColorDisabled;

	private Color m_columnHeaderCheckMarkColor;

	private Color m_columnHeaderCheckMarkColorDisabled;

	private Color m_columnHeaderSeparatorColor;

	private SolidBrush m_alternatingRowBrush1;

	private SolidBrush m_alternatingRowBrush2;

	private SolidBrush m_defaultBackBrush;

	private SolidBrush m_highlightTextBrush;

	private SolidBrush m_normalTextBrush;

	private SolidBrush m_highlightBackBrush;

	private SolidBrush m_readOnlyBrush;

	private SolidBrush m_externalEditorBrush;

	private SolidBrush m_columnHeaderTextBrush;

	private SolidBrush m_columnHeaderTextBrushDisabled;

	private SolidBrush m_columnHeaderCheckMarkBrush;

	private SolidBrush m_columnHeaderCheckMarkBrushDisabled;

	private Pen m_columnHeaderSeparatorPen;

	private Font m_headerFont = new Font("Helvetica", 9f, FontStyle.Bold);

	private Font m_tickFont = new Font("Helvetica", 9f, FontStyle.Regular);

	private Font m_boldFont;

	private int m_headerHeight;

	private bool m_settingValue;

	private bool m_itemAlreadySelected;

	private Multimap<string, string> m_groupReadOnlyColumns = new Multimap<string, string>();

	private Multimap<string, string> m_groupExternalEditorColumns = new Multimap<string, string>();

	private ListChangedEventHandler currencyManager_ListChanged;

	private EventHandler currencyManager_PositionChanged;

	private object m_dataSource;

	private string m_dataMember;

	private CurrencyManager m_currencyManager;

	private PropertyDescriptorCollection m_propertyDescriptors;

	private readonly TextBox m_textBox;

	private readonly ComboBox m_comboBox;

	private Control m_activeEditingControl;

	private int m_currentRow;

	private int m_currentCol;

	private ListSortDirection m_sortDirection = ListSortDirection.Ascending;

	private int m_sortColumn = -1;

	private List<object> m_selectedObjects = new List<object>();

	private List<object> m_checkeObjects = new List<object>();

	private int m_checkBoxWidth = 0;

	private bool m_autoColumnWidth = true;

	private bool m_adjustingColumnWidths;

	private int m_lastNewWidth;

	private int m_rowHeight = 17;

	private readonly string m_tickSymbol = new string('✔', 1);

	private const int TextOffset = 8;

	private static readonly Image s_sortAscendingImage = ResourceUtil.GetImage(Resources.SortAscendingImage);

	private static readonly Image s_sortDescendingImage = ResourceUtil.GetImage(Resources.SortDescendingImage);

	public bool SortingItems { get; set; }

	public Color AlternatingRowColor1
	{
		get
		{
			return m_alternatingRowColor1;
		}
		set
		{
			m_alternatingRowBrush1.Dispose();
			m_alternatingRowColor1 = value;
			m_alternatingRowBrush1 = new SolidBrush(m_alternatingRowColor1);
		}
	}

	public Color AlternatingRowColor2
	{
		get
		{
			return m_alternatingRowColor2;
		}
		set
		{
			m_alternatingRowBrush2.Dispose();
			m_alternatingRowColor2 = value;
			m_alternatingRowBrush2 = new SolidBrush(m_alternatingRowColor2);
		}
	}

	public Color ReadOnlyTextColor
	{
		get
		{
			return m_readOnlyTextColor;
		}
		set
		{
			m_readOnlyTextColor = value;
			if (m_readOnlyBrush != null)
			{
				m_readOnlyBrush.Dispose();
			}
			m_readOnlyBrush = new SolidBrush(m_readOnlyTextColor);
		}
	}

	public Color ExternalEditorTextColor
	{
		get
		{
			return m_externalEditorTextColor;
		}
		set
		{
			m_externalEditorTextColor = value;
			if (m_externalEditorBrush != null)
			{
				m_externalEditorBrush.Dispose();
			}
			m_externalEditorBrush = new SolidBrush(m_externalEditorTextColor);
		}
	}

	public Color DefaultBackgroundColor
	{
		get
		{
			return m_defaultBackColor;
		}
		set
		{
			m_defaultBackColor = value;
			if (m_defaultBackBrush != null)
			{
				m_defaultBackBrush.Dispose();
			}
			m_defaultBackBrush = new SolidBrush(m_defaultBackColor);
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
			if (m_highlightTextBrush != null)
			{
				m_highlightTextBrush.Dispose();
			}
			m_highlightTextBrush = new SolidBrush(m_highlightTextColor);
		}
	}

	public Color HighlightBackColor
	{
		get
		{
			return m_highlightTextColor;
		}
		set
		{
			m_highlightBackColor = value;
			if (m_highlightBackBrush != null)
			{
				m_highlightBackBrush.Dispose();
			}
			m_highlightBackBrush = new SolidBrush(m_highlightBackColor);
		}
	}

	public Color NormalTextColor
	{
		get
		{
			return m_normalTextColor;
		}
		set
		{
			m_normalTextColor = value;
			if (m_normalTextBrush != null)
			{
				m_normalTextBrush.Dispose();
			}
			m_normalTextBrush = new SolidBrush(m_normalTextColor);
		}
	}

	public Font HeaderFont
	{
		get
		{
			return m_headerFont;
		}
		set
		{
			if (m_headerFont != null)
			{
				m_headerFont.Dispose();
			}
			m_headerFont = value;
		}
	}

	public Color ColumnHeaderTextColor
	{
		get
		{
			return m_columnHeaderTextColor;
		}
		set
		{
			m_columnHeaderTextColor = value;
			if (m_columnHeaderTextBrush != null)
			{
				m_columnHeaderTextBrush.Dispose();
			}
			m_columnHeaderTextBrush = new SolidBrush(m_columnHeaderTextColor);
		}
	}

	public Color ColumnHeaderTextColorDisabled
	{
		get
		{
			return m_columnHeaderTextColorDisabled;
		}
		set
		{
			m_columnHeaderTextColorDisabled = value;
			if (m_columnHeaderTextBrushDisabled != null)
			{
				m_columnHeaderTextBrushDisabled.Dispose();
			}
			m_columnHeaderTextBrushDisabled = new SolidBrush(m_columnHeaderTextColorDisabled);
		}
	}

	public Color ColumnHeaderCheckMarkColor
	{
		get
		{
			return m_columnHeaderCheckMarkColor;
		}
		set
		{
			m_columnHeaderCheckMarkColor = value;
			if (m_columnHeaderCheckMarkBrush != null)
			{
				m_columnHeaderCheckMarkBrush.Dispose();
			}
			m_columnHeaderCheckMarkBrush = new SolidBrush(m_columnHeaderCheckMarkColor);
		}
	}

	public Color ColumnHeaderCheckMarkColorDisabled
	{
		get
		{
			return m_columnHeaderCheckMarkColorDisabled;
		}
		set
		{
			m_columnHeaderCheckMarkColorDisabled = value;
			if (m_columnHeaderCheckMarkBrushDisabled != null)
			{
				m_columnHeaderCheckMarkBrushDisabled.Dispose();
			}
			m_columnHeaderCheckMarkBrushDisabled = new SolidBrush(m_columnHeaderCheckMarkColorDisabled);
		}
	}

	public Color ColumnHeaderSeparatorColor
	{
		get
		{
			return m_columnHeaderSeparatorColor;
		}
		set
		{
			m_columnHeaderSeparatorColor = value;
			if (m_columnHeaderSeparatorPen != null)
			{
				m_columnHeaderSeparatorPen.Dispose();
			}
			m_columnHeaderSeparatorPen = new Pen(m_columnHeaderSeparatorColor, 1f);
		}
	}

	public object DataSource
	{
		get
		{
			return m_dataSource;
		}
		set
		{
			if (m_dataSource != value)
			{
				m_dataSource = value;
				UpdateDataBinding();
			}
		}
	}

	public string DataMember
	{
		get
		{
			return m_dataMember;
		}
		set
		{
			if (m_dataMember != value)
			{
				m_dataMember = value;
				UpdateDataBinding();
			}
		}
	}

	public PropertyDescriptorCollection ItemProperties => m_propertyDescriptors;

	public bool AlternatingRowColors { get; set; }

	public string Settings
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("Columns");
			xmlDocument.AppendChild(xmlElement);
			foreach (ColumnHeader column in base.Columns)
			{
				XmlElement xmlElement2 = xmlDocument.CreateElement("Column");
				xmlElement.AppendChild(xmlElement2);
				xmlElement2.SetAttribute("Name", column.Text);
				xmlElement2.SetAttribute("Width", column.Width.ToString());
			}
			return xmlDocument.InnerXml;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			SuspendLayout();
			m_adjustingColumnWidths = true;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(value);
			XmlElement documentElement = xmlDocument.DocumentElement;
			if (documentElement == null || documentElement.Name != "Columns")
			{
				throw new Exception("Invalid ListView settings");
			}
			m_autoColumnWidth = false;
			XmlNodeList xmlNodeList = documentElement.SelectNodes("Column");
			foreach (XmlElement item in xmlNodeList)
			{
				string attribute = item.GetAttribute("Name");
				string attribute2 = item.GetAttribute("Width");
				if (attribute2 == null || !int.TryParse(attribute2, out var result))
				{
					continue;
				}
				bool flag = false;
				foreach (ColumnHeader column in base.Columns)
				{
					if (column.Text == attribute)
					{
						column.Width = result;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					m_autoColumnWidth = true;
				}
			}
			m_adjustingColumnWidths = false;
			ResumeLayout();
			if (AutoSize)
			{
				ResizeToContent();
			}
		}
	}

	private int HeaderHeight
	{
		get
		{
			if (m_headerHeight == 0)
			{
				IntPtr hWnd = (IntPtr)User32.SendMessage(base.Handle, 4127, 0, 0);
				Rectangle r = default(Rectangle);
				User32.GetClientRect(hWnd, ref r);
				m_headerHeight = r.Height;
			}
			return m_headerHeight;
		}
	}

	private int CheckBoxWidth
	{
		get
		{
			if (m_checkBoxWidth == 0)
			{
				m_checkBoxWidth = CheckBoxRenderer.GetGlyphSize(CreateGraphics(), CheckBoxState.UncheckedNormal).Width + 6;
			}
			return m_checkBoxWidth;
		}
	}

	public event EventHandler<ListViewCellCancelEventArgs> CellBeginEdit;

	public event EventHandler<ListViewCellEventArgs> CellEndEdit;

	public event EventHandler<ListViewCellValidatingEventArgs> CellValidating;

	public DataBoundListView()
	{
		base.View = View.Details;
		base.FullRowSelect = true;
		base.ColumnClick += DataBoundListView_ColumnClick;
		base.LabelEdit = false;
		base.HideSelection = false;
		DoubleBuffered = true;
		currencyManager_ListChanged = bindingManager_ListChanged;
		currencyManager_PositionChanged = bindingManager_PositionChanged;
		base.SelectedIndexChanged += listView_SelectedIndexChanged;
		base.ColumnWidthChanged += listView_ColumnWidthChanged;
		base.ColumnWidthChanging += listView_ColumnWidthChanging;
		m_textBox = new TextBox();
		IntPtr handle = m_textBox.Handle;
		m_textBox.BorderStyle = BorderStyle.FixedSingle;
		m_textBox.AutoSize = false;
		m_textBox.LostFocus += textBox_LostFocus;
		m_textBox.DragOver += textBox_DragOver;
		m_textBox.DragDrop += textBox_DragDrop;
		m_textBox.MouseHover += textBox_MouseHover;
		m_textBox.MouseLeave += textBox_MouseLeave;
		m_textBox.Visible = false;
		base.Controls.Add(m_textBox);
		m_comboBox = new ComboBox();
		handle = m_comboBox.Handle;
		m_comboBox.Visible = false;
		m_comboBox.DropDownClosed += comboBox_DropDownClosed;
		base.Controls.Add(m_comboBox);
		base.OwnerDraw = true;
		m_alternatingRowBrush1 = new SolidBrush(m_alternatingRowColor1);
		m_alternatingRowBrush2 = new SolidBrush(m_alternatingRowColor2);
		m_defaultBackColor = BackColor;
		NormalTextColor = Color.Black;
		HighlightTextColor = Color.White;
		ReadOnlyTextColor = Color.DimGray;
		ExternalEditorTextColor = Color.Black;
		HighlightBackColor = SystemColors.Highlight;
		ColumnHeaderTextColor = NormalTextColor;
		ColumnHeaderTextColorDisabled = ReadOnlyTextColor;
		ColumnHeaderCheckMarkColor = Color.DarkSlateGray;
		ColumnHeaderCheckMarkColorDisabled = ReadOnlyTextColor;
		ColumnHeaderSeparatorColor = Color.FromArgb(228, 229, 230);
		m_boldFont = new Font(Font, FontStyle.Bold);
		base.ShowGroups = false;
	}

	protected override void OnBindingContextChanged(EventArgs e)
	{
		UpdateDataBinding();
		base.OnBindingContextChanged(e);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.Delete)
		{
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	public void ResizeToContent()
	{
		if (base.Items.Count > 0)
		{
			Rectangle itemRect = GetItemRect(base.Items.Count - 1);
			base.Height = itemRect.Y + itemRect.Height;
		}
		else
		{
			base.Height = 0;
		}
		IntPtr hWnd = (IntPtr)User32.SendMessage(base.Handle, 4127, 0, 0);
		Rectangle r = default(Rectangle);
		User32.GetClientRect(hWnd, ref r);
		base.Height += r.Height + base.Margin.Top + base.Margin.Bottom + SystemInformation.HorizontalScrollBarHeight;
		int num = 0;
		foreach (ColumnHeader column in base.Columns)
		{
			num += column.Width;
		}
		base.Width = num + base.Margin.Left + base.Margin.Right + SystemInformation.VerticalScrollBarWidth;
	}

	private void UpdateDataBinding()
	{
		if (DataSource == null || base.BindingContext == null)
		{
			return;
		}
		CurrencyManager currencyManager;
		try
		{
			currencyManager = (CurrencyManager)base.BindingContext[DataSource, DataMember];
		}
		catch (ArgumentException)
		{
			return;
		}
		if (m_currencyManager != currencyManager)
		{
			RemeberSelections();
			if (m_currencyManager != null)
			{
				m_currencyManager.ListChanged -= currencyManager_ListChanged;
				m_currencyManager.PositionChanged -= currencyManager_PositionChanged;
			}
			m_currencyManager = currencyManager;
			if (m_currencyManager != null)
			{
				m_currencyManager.ListChanged += currencyManager_ListChanged;
				m_currencyManager.PositionChanged += currencyManager_PositionChanged;
			}
			BuildListColumns();
			Reload();
			RestoreSelections();
		}
	}

	private void BuildListColumns()
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (ColumnHeader column in base.Columns)
		{
			dictionary[column.Text] = column.Width;
		}
		base.Columns.Clear();
		if (m_currencyManager == null)
		{
			return;
		}
		m_adjustingColumnWidths = true;
		m_propertyDescriptors = m_currencyManager.GetItemProperties();
		foreach (PropertyDescriptor propertyDescriptor in m_propertyDescriptors)
		{
			ColumnHeader columnHeader2 = new ColumnHeader();
			columnHeader2.Name = propertyDescriptor.Name;
			columnHeader2.Text = propertyDescriptor.DisplayName;
			if (dictionary.ContainsKey(columnHeader2.Text))
			{
				columnHeader2.Width = dictionary[columnHeader2.Text];
			}
			base.Columns.Add(columnHeader2);
		}
		if (AutoSize)
		{
			ResizeToContent();
		}
		m_adjustingColumnWidths = false;
	}

	private void Reload()
	{
		BeginUpdate();
		base.Items.Clear();
		ListViewItem[] array = new ListViewItem[m_currencyManager.Count];
		for (int i = 0; i < m_currencyManager.Count; i++)
		{
			array[i] = ListViewItemFromDataSource(i);
		}
		base.Items.AddRange(array);
		if (m_autoColumnWidth)
		{
			AutoResizeColumns();
		}
		EndUpdate();
	}

	private ListViewItem ListViewItemFromDataSource(int index)
	{
		PropertyDescriptorCollection itemProperties = m_currencyManager.GetItemProperties();
		ArrayList arrayList = new ArrayList();
		object obj = m_currencyManager.List[index];
		foreach (ColumnHeader column in base.Columns)
		{
			PropertyDescriptor propertyDescriptor = itemProperties.Find(column.Text, ignoreCase: false);
			if (propertyDescriptor != null)
			{
				arrayList.Add(propertyDescriptor.GetValue(obj).ToString());
			}
		}
		return new ListViewItem((string[])arrayList.ToArray(typeof(string)), GroupingDataItem(obj));
	}

	private void AddItem(int index)
	{
		ListViewItem item = ListViewItemFromDataSource(index);
		base.Items.Insert(index, item);
		if (m_autoColumnWidth)
		{
			AutoResizeColumns();
		}
	}

	private void UpdateItem(int index)
	{
		if (index < 0 || index >= base.Items.Count)
		{
			return;
		}
		object component = m_currencyManager.List[index];
		int num = 0;
		foreach (ColumnHeader column in base.Columns)
		{
			PropertyDescriptor propertyDescriptor = m_propertyDescriptors.Find(column.Name, ignoreCase: false);
			if (propertyDescriptor != null)
			{
				base.Items[index].SubItems[num].Text = propertyDescriptor.GetValue(component).ToString();
			}
			num++;
		}
	}

	private void DeleteItem(int index)
	{
		if (index >= 0 && index < base.Items.Count)
		{
			base.Items.RemoveAt(index);
		}
	}

	private void bindingManager_PositionChanged(object sender, EventArgs e)
	{
		if (m_currencyManager.Position >= 0 && base.Items.Count > m_currencyManager.Position)
		{
			base.Items[m_currencyManager.Position].Selected = true;
			EnsureVisible(m_currencyManager.Position);
		}
	}

	private void bindingManager_ListChanged(object sender, ListChangedEventArgs e)
	{
		if (e.ListChangedType == ListChangedType.Reset || e.ListChangedType == ListChangedType.ItemMoved)
		{
			Reload();
			return;
		}
		if (e.ListChangedType == ListChangedType.ItemAdded)
		{
			AddItem(e.NewIndex);
			return;
		}
		if (e.ListChangedType == ListChangedType.ItemChanged)
		{
			UpdateItem(e.NewIndex);
			return;
		}
		if (e.ListChangedType == ListChangedType.ItemDeleted)
		{
			DeleteItem(e.NewIndex);
			return;
		}
		BuildListColumns();
		Reload();
	}

	private void listView_SelectedIndexChanged(object sender, EventArgs e)
	{
		try
		{
			if (base.SelectedIndices.Count > 0 && m_currencyManager.Position != base.SelectedIndices[0])
			{
				m_currencyManager.Position = base.SelectedIndices[0];
			}
		}
		catch
		{
		}
		Invalidate();
	}

	private void listView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
	{
		if (!m_adjustingColumnWidths)
		{
			m_autoColumnWidth = false;
			if (m_lastNewWidth > 0 && Math.Abs(m_lastNewWidth - base.Columns[e.ColumnIndex].Width) > 100)
			{
				base.Columns[e.ColumnIndex].Width = m_lastNewWidth;
			}
		}
	}

	private void listView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
	{
		DisableEditingControl(acceptNewValue: true);
		if (m_adjustingColumnWidths)
		{
			return;
		}
		m_lastNewWidth = e.NewWidth;
		using Graphics graphics = CreateGraphics();
		float num = graphics.MeasureString(base.Columns[e.ColumnIndex].Text, HeaderFont).Width + (float)s_sortAscendingImage.Width + 8f + 2f;
		if (e.ColumnIndex == 0 && base.CheckBoxes)
		{
			num += (float)(CheckBoxWidth + 2);
		}
		if ((float)e.NewWidth < num)
		{
			e.Cancel = true;
			e.NewWidth = (int)num;
		}
	}

	protected override void OnDrawItem(DrawListViewItemEventArgs e)
	{
		base.OnDrawItem(e);
		if (!AlternatingRowColors)
		{
			return;
		}
		Rectangle rectangle = default(Rectangle);
		int num = 0;
		if (base.Items.Count > 0)
		{
			if (base.Items[base.Items.Count - 1] == null || base.Items[0] == null)
			{
				return;
			}
			rectangle = base.Items[base.Items.Count - 1].Bounds;
			Rectangle bounds = base.Items[0].SubItems[base.Items[0].SubItems.Count - 1].Bounds;
			m_rowHeight = rectangle.Height;
			if (bounds.Right < base.ClientRectangle.Width)
			{
				Rectangle rect = new Rectangle(bounds.Right, base.Items[0].Bounds.Top, base.ClientRectangle.Width - bounds.Right, m_rowHeight);
				while (rect.Y < rectangle.Bottom)
				{
					if (base.Enabled && base.SelectedIndices.Contains(num))
					{
						e.Graphics.FillRectangle(m_highlightBackBrush, rect);
					}
					else if (num % 2 == 0)
					{
						e.Graphics.FillRectangle(m_alternatingRowBrush1, rect);
					}
					else
					{
						e.Graphics.FillRectangle(m_alternatingRowBrush2, rect);
					}
					num++;
					rect.Y += m_rowHeight;
				}
			}
			num = base.Items.Count;
		}
		else
		{
			rectangle.Y = HeaderHeight;
		}
		if (rectangle.Top + rectangle.Height >= base.ClientRectangle.Height)
		{
			return;
		}
		Rectangle rect2 = new Rectangle(0, rectangle.Bottom, base.ClientRectangle.Width, m_rowHeight);
		while (rect2.Y < base.ClientRectangle.Bottom)
		{
			if (num % 2 == 0)
			{
				e.Graphics.FillRectangle(m_alternatingRowBrush1, rect2);
			}
			else
			{
				e.Graphics.FillRectangle(m_alternatingRowBrush2, rect2);
			}
			num++;
			rect2.Y += m_rowHeight;
		}
	}

	protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
	{
		base.OnDrawSubItem(e);
		if (base.CheckBoxes && e.ColumnIndex == 0)
		{
			if (e.Item.Selected && base.Enabled)
			{
				e.Graphics.FillRectangle(m_highlightBackBrush, e.Bounds);
			}
			else if (AlternatingRowColors)
			{
				if (e.ItemIndex % 2 == 0)
				{
					e.Graphics.FillRectangle(m_alternatingRowBrush1, e.Bounds);
				}
				else
				{
					e.Graphics.FillRectangle(m_alternatingRowBrush2, e.Bounds);
				}
			}
			else
			{
				e.Graphics.FillRectangle(m_defaultBackBrush, e.Bounds);
			}
			DrawCheckBox(e);
			Rectangle bounds = e.Bounds;
			bounds.X += CheckBoxWidth + 8;
			bounds.Width -= CheckBoxWidth + 8;
			Font font = (e.Item.Checked ? m_boldFont : Font);
			using StringFormat stringFormat = new StringFormat(StringFormatFlags.LineLimit);
			switch (e.Header.TextAlign)
			{
			case HorizontalAlignment.Center:
				stringFormat.Alignment = StringAlignment.Center;
				break;
			case HorizontalAlignment.Right:
				stringFormat.Alignment = StringAlignment.Far;
				break;
			}
			if (IsCellReadOnly(e.ItemIndex, e.ColumnIndex))
			{
				SolidBrush brush = (e.Item.Selected ? m_highlightTextBrush : m_readOnlyBrush);
				if (!base.Enabled)
				{
					brush = m_readOnlyBrush;
				}
				e.Graphics.DrawString(e.SubItem.Text, font, brush, bounds, stringFormat);
			}
			else if (IsCellExternalEditor(e.ItemIndex, e.ColumnIndex))
			{
				SolidBrush brush2 = (e.Item.Selected ? m_highlightTextBrush : m_externalEditorBrush);
				if (!base.Enabled)
				{
					brush2 = m_readOnlyBrush;
				}
				e.Graphics.DrawString(e.SubItem.Text, font, brush2, bounds, stringFormat);
			}
			else
			{
				SolidBrush brush3 = (e.Item.Selected ? m_highlightTextBrush : m_normalTextBrush);
				if (!base.Enabled)
				{
					brush3 = m_readOnlyBrush;
				}
				e.Graphics.DrawString(e.SubItem.Text, font, brush3, bounds, stringFormat);
			}
			return;
		}
		Rectangle bounds2 = e.Bounds;
		bounds2.Offset(8, 0);
		using StringFormat stringFormat2 = new StringFormat(StringFormatFlags.LineLimit);
		switch (e.Header.TextAlign)
		{
		case HorizontalAlignment.Center:
			stringFormat2.Alignment = StringAlignment.Center;
			break;
		case HorizontalAlignment.Right:
			stringFormat2.Alignment = StringAlignment.Far;
			break;
		}
		if (e.Item.Selected && base.Enabled)
		{
			e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
		}
		else if (e.ItemIndex % 2 == 0)
		{
			e.Graphics.FillRectangle(m_alternatingRowBrush1, e.Bounds);
		}
		else
		{
			e.Graphics.FillRectangle(m_alternatingRowBrush2, e.Bounds);
		}
		Font font2 = (e.Item.Checked ? m_boldFont : Font);
		if (IsCellReadOnly(e.ItemIndex, e.ColumnIndex))
		{
			SolidBrush brush4 = (e.Item.Selected ? m_highlightTextBrush : m_readOnlyBrush);
			if (!base.Enabled)
			{
				brush4 = m_readOnlyBrush;
			}
			e.Graphics.DrawString(e.SubItem.Text, font2, brush4, bounds2, stringFormat2);
		}
		else if (IsCellExternalEditor(e.ItemIndex, e.ColumnIndex))
		{
			SolidBrush brush5 = (e.Item.Selected ? m_highlightTextBrush : m_externalEditorBrush);
			if (!base.Enabled)
			{
				brush5 = m_readOnlyBrush;
			}
			e.Graphics.DrawString(e.SubItem.Text, font2, brush5, bounds2, stringFormat2);
		}
		else
		{
			SolidBrush brush6 = (e.Item.Selected ? m_highlightTextBrush : m_normalTextBrush);
			if (!base.Enabled)
			{
				brush6 = m_readOnlyBrush;
			}
			e.Graphics.DrawString(e.SubItem.Text, font2, brush6, bounds2, stringFormat2);
		}
	}

	protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
	{
		base.OnDrawColumnHeader(e);
		using StringFormat stringFormat = new StringFormat();
		switch (e.Header.TextAlign)
		{
		case HorizontalAlignment.Center:
			stringFormat.Alignment = StringAlignment.Center;
			break;
		case HorizontalAlignment.Right:
			stringFormat.Alignment = StringAlignment.Far;
			break;
		}
		e.DrawBackground();
		Rectangle bounds = e.Bounds;
		bounds.Y += 3;
		bounds.X += 8;
		if (e.ColumnIndex == 0)
		{
			bounds.X += CheckBoxWidth;
		}
		SolidBrush brush = (base.Enabled ? m_columnHeaderTextBrush : m_columnHeaderTextBrushDisabled);
		e.Graphics.DrawString(e.Header.Text, HeaderFont, brush, bounds, stringFormat);
		if (e.ColumnIndex == m_sortColumn)
		{
			float num = e.Graphics.MeasureString(e.Header.Text, HeaderFont).Width;
			if (m_sortColumn == 0)
			{
				num += (float)(CheckBoxWidth + 2);
			}
			if ((float)e.Bounds.Width > num + (float)s_sortAscendingImage.Width + 8f)
			{
				Point point = new Point(e.Bounds.Location.X + e.Bounds.Width - s_sortAscendingImage.Width - 4, e.Bounds.Top + (e.Bounds.Height - s_sortAscendingImage.Height) / 2);
				if (m_sortDirection == ListSortDirection.Ascending)
				{
					e.Graphics.DrawImage(s_sortAscendingImage, point);
				}
				else
				{
					e.Graphics.DrawImage(s_sortDescendingImage, point);
				}
			}
		}
		if (base.CheckBoxes && e.ColumnIndex == 0)
		{
			Point point2 = new Point(e.Bounds.Location.X, e.Bounds.Top + 3);
			e.Graphics.DrawString(m_tickSymbol, m_tickFont, base.Enabled ? m_columnHeaderCheckMarkBrush : m_columnHeaderCheckMarkBrushDisabled, point2);
			e.Graphics.DrawLine(m_columnHeaderSeparatorPen, CheckBoxWidth, e.Bounds.Top, CheckBoxWidth, e.Bounds.Bottom - 2);
		}
	}

	protected virtual void DrawCheckBox(DrawListViewSubItemEventArgs e)
	{
		bool flag = base.Items[e.ItemIndex].Checked;
		Point glyphLocation = new Point(e.Bounds.Location.X + base.Margin.Left, e.Bounds.Location.Y);
		if (base.MultiSelect)
		{
			CheckBoxState checkBoxState = ((!flag) ? CheckBoxState.UncheckedNormal : CheckBoxState.CheckedNormal);
			CheckBoxRenderer.DrawCheckBox(e.Graphics, glyphLocation, checkBoxState);
			return;
		}
		RadioButtonState radioButtonState = ((!flag) ? RadioButtonState.UncheckedNormal : RadioButtonState.CheckedNormal);
		if (!base.Enabled)
		{
			radioButtonState = (flag ? RadioButtonState.CheckedDisabled : RadioButtonState.UncheckedDisabled);
		}
		RadioButtonRenderer.DrawRadioButton(e.Graphics, glyphLocation, radioButtonState);
	}

	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);
		if (!AlternatingRowColors || base.Items.Count != 0 || m.Msg != 15)
		{
			return;
		}
		using Graphics graphics = CreateGraphics();
		int num = 0;
		Rectangle rectangle = new Rectangle(0, HeaderHeight, 0, 0);
		if (rectangle.Top + rectangle.Height >= base.ClientRectangle.Height)
		{
			return;
		}
		Rectangle rect = new Rectangle(0, rectangle.Bottom, base.ClientRectangle.Width, m_rowHeight);
		while (rect.Y < base.ClientRectangle.Bottom)
		{
			if (num % 2 == 0)
			{
				graphics.FillRectangle(m_alternatingRowBrush1, rect);
			}
			else
			{
				graphics.FillRectangle(m_alternatingRowBrush2, rect);
			}
			num++;
			rect.Y += m_rowHeight;
		}
	}

	private void DataBoundListView_ColumnClick(object sender, ColumnClickEventArgs e)
	{
		if (DataSource is IBindingList { SupportsSorting: not false } bindingList)
		{
			if (m_sortColumn == e.Column)
			{
				m_sortDirection = ((m_sortDirection == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending);
			}
			m_sortColumn = e.Column;
			SortingItems = true;
			DisableEditingControl(acceptNewValue: true);
			RemeberSelections();
			bindingList.ApplySort(ItemProperties[e.Column], m_sortDirection);
			RestoreSelections();
			SortingItems = false;
		}
	}

	private void RemeberSelections()
	{
		m_selectedObjects.Clear();
		m_checkeObjects.Clear();
		foreach (int selectedIndex in base.SelectedIndices)
		{
			m_selectedObjects.Add(m_currencyManager.List[selectedIndex]);
		}
		for (int i = 0; i < base.Items.Count; i++)
		{
			if (base.Items[i].Checked)
			{
				m_checkeObjects.Add(m_currencyManager.List[i]);
			}
		}
	}

	private void RestoreSelections()
	{
		foreach (object selectedObject in m_selectedObjects)
		{
			int num = m_currencyManager.List.IndexOf(selectedObject);
			if (num != -1)
			{
				base.Items[num].Selected = true;
			}
		}
		foreach (object checkeObject in m_checkeObjects)
		{
			int num2 = m_currencyManager.List.IndexOf(checkeObject);
			if (num2 != -1)
			{
				base.Items[num2].Checked = true;
			}
		}
	}

	private void AutoResizeColumns()
	{
		m_adjustingColumnWidths = true;
		using (Graphics graphics = CreateGraphics())
		{
			int count = base.Columns.Count;
			for (int i = 0; i < count; i++)
			{
				float num = graphics.MeasureString(base.Columns[i].Text, HeaderFont).Width + (float)(2 * s_sortAscendingImage.Width) + (float)base.Margin.Left + (float)base.Margin.Right + 18f;
				for (int j = 0; j < base.Items.Count; j++)
				{
					float num2 = graphics.MeasureString(base.Items[j].SubItems[i].Text, Font).Width + (float)base.Margin.Left + (float)base.Margin.Right + 18f;
					if (num2 > num)
					{
						num = num2;
					}
				}
				base.Columns[i].Width = (int)num;
			}
		}
		if (base.Items.Count > 0)
		{
			m_autoColumnWidth = false;
		}
		m_adjustingColumnWidths = false;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_alternatingRowBrush1 != null)
			{
				m_alternatingRowBrush1.Dispose();
				m_alternatingRowBrush1 = null;
			}
			if (m_alternatingRowBrush2 != null)
			{
				m_alternatingRowBrush2.Dispose();
				m_alternatingRowBrush2 = null;
			}
			if (m_defaultBackBrush != null)
			{
				m_defaultBackBrush.Dispose();
				m_defaultBackBrush = null;
			}
			if (m_highlightTextBrush != null)
			{
				m_highlightTextBrush.Dispose();
				m_highlightTextBrush = null;
			}
			if (m_normalTextBrush != null)
			{
				m_normalTextBrush.Dispose();
				m_normalTextBrush = null;
			}
			if (m_highlightBackBrush != null)
			{
				m_highlightBackBrush.Dispose();
				m_highlightBackBrush = null;
			}
			if (m_readOnlyBrush != null)
			{
				m_readOnlyBrush.Dispose();
				m_readOnlyBrush = null;
			}
			if (m_externalEditorBrush != null)
			{
				m_externalEditorBrush.Dispose();
				m_externalEditorBrush = null;
			}
			if (m_columnHeaderTextBrush != null)
			{
				m_columnHeaderTextBrush.Dispose();
				m_columnHeaderTextBrush = null;
			}
			if (m_columnHeaderTextBrushDisabled != null)
			{
				m_columnHeaderTextBrushDisabled.Dispose();
				m_columnHeaderTextBrushDisabled = null;
			}
			if (m_columnHeaderCheckMarkBrush != null)
			{
				m_columnHeaderCheckMarkBrush.Dispose();
				m_columnHeaderCheckMarkBrush = null;
			}
			if (m_columnHeaderCheckMarkBrushDisabled != null)
			{
				m_columnHeaderCheckMarkBrushDisabled.Dispose();
				m_columnHeaderCheckMarkBrushDisabled = null;
			}
			if (m_columnHeaderSeparatorPen != null)
			{
				m_columnHeaderSeparatorPen.Dispose();
				m_columnHeaderSeparatorPen = null;
			}
		}
		base.Dispose(disposing);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button == MouseButtons.Left)
		{
			m_itemAlreadySelected = false;
			ListViewItem itemAt = GetItemAt(e.X, e.Y);
			if (itemAt != null && base.SelectedItems.Contains(itemAt))
			{
				m_itemAlreadySelected = true;
			}
			else
			{
				DisableEditingControl(acceptNewValue: true);
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		ListViewItem itemAt = GetItemAt(e.X, e.Y);
		if (e.Button != MouseButtons.Left || itemAt == null || !m_itemAlreadySelected)
		{
			return;
		}
		int index = itemAt.Index;
		int num = -1;
		int num2 = itemAt.GetBounds(ItemBoundsPortion.Entire).Left;
		foreach (ColumnHeader column in base.Columns)
		{
			if (e.X < num2 + column.Width)
			{
				num = column.Index;
				break;
			}
			num2 += column.Width;
		}
		if (index >= 0 && num >= 0 && (num != 0 || !base.CheckBoxes || e.X >= CheckBoxWidth))
		{
			if (m_currentRow != index || m_currentCol != num)
			{
				DisableEditingControl(acceptNewValue: true);
			}
			m_currentRow = index;
			m_currentCol = num;
			OnCellClicked(itemAt);
		}
	}

	protected void OnCellClicked(ListViewItem item)
	{
		if (!IsCellReadOnly(m_currentRow, m_currentCol) && !IsCellExternalEditor(m_currentRow, m_currentCol))
		{
			StartCellEdit(item);
		}
	}

	private void StartCellEdit(ListViewItem item)
	{
		ListViewCellCancelEventArgs e = new ListViewCellCancelEventArgs(m_currentRow, m_currentCol);
		OnCellBeginEdit(e);
		if (!e.Cancel)
		{
			Rectangle bounds = item.GetBounds(ItemBoundsPortion.Entire);
			int num = bounds.Left;
			for (int i = 0; i < m_currentCol; i++)
			{
				num += base.Columns[i].Width;
			}
			int num2 = base.Columns[m_currentCol].Width;
			if (m_currentCol == 0 && base.CheckBoxes)
			{
				num += CheckBoxWidth;
				num2 -= CheckBoxWidth;
			}
			num += 8;
			num2 -= 8;
			Rectangle bounds2 = new Rectangle(num, bounds.Y, num2, bounds.Height);
			if (m_propertyDescriptors[m_currentCol].PropertyType.IsEnum)
			{
				m_comboBox.DataSource = Enum.GetValues(m_propertyDescriptors[m_currentCol].PropertyType);
				m_comboBox.Bounds = bounds2;
				m_activeEditingControl = m_comboBox;
			}
			else
			{
				m_textBox.Bounds = bounds2;
				m_activeEditingControl = m_textBox;
			}
			EnableEditingControl();
		}
	}

	private bool IsCellReadOnly(int row, int col)
	{
		if (m_propertyDescriptors[col].IsReadOnly)
		{
			return true;
		}
		ListViewGroup listViewGroup = base.Items[row].Group;
		if (listViewGroup != null && m_groupReadOnlyColumns.ContainsKeyValue(listViewGroup.Name, base.Columns[col].Text))
		{
			return true;
		}
		return false;
	}

	private bool IsCellExternalEditor(int row, int col)
	{
		ListViewGroup listViewGroup = base.Items[row].Group;
		if (listViewGroup != null && m_groupExternalEditorColumns.ContainsKeyValue(listViewGroup.Name, base.Columns[col].Text))
		{
			return true;
		}
		return false;
	}

	protected void OnCellBeginEdit(ListViewCellCancelEventArgs e)
	{
		if (this.CellBeginEdit != null)
		{
			this.CellBeginEdit(this, e);
		}
	}

	protected void OnCellEndEdit(ListViewCellEventArgs e)
	{
		if (this.CellEndEdit != null)
		{
			this.CellEndEdit(this, e);
		}
	}

	protected void OnCellCellValidating(ListViewCellValidatingEventArgs e)
	{
		if (this.CellValidating != null)
		{
			this.CellValidating(this, e);
		}
	}

	private void textBox_LostFocus(object sender, EventArgs e)
	{
		DisableEditingControl(acceptNewValue: true);
	}

	private void textBox_DragOver(object sender, DragEventArgs e)
	{
		OnDragOver(e);
	}

	private void textBox_DragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void textBox_MouseHover(object sender, EventArgs e)
	{
		OnMouseHover(e);
	}

	private void textBox_MouseLeave(object sender, EventArgs e)
	{
		OnMouseLeave(e);
	}

	private void EnableEditingControl()
	{
		m_activeEditingControl.Leave += activeEditingControl_Leave;
		m_activeEditingControl.KeyPress += activeEditingControl_KeyPress;
		if (m_activeEditingControl == m_textBox)
		{
			SetTextBoxFromProperty();
			EnableTextBox();
		}
		else if (m_activeEditingControl == m_comboBox)
		{
			SetComboBoxFromProperty();
			EnableComboBox();
		}
	}

	private void activeEditingControl_KeyPress(object sender, KeyPressEventArgs e)
	{
		switch (e.KeyChar)
		{
		case '\u001b':
			DisableEditingControl(acceptNewValue: false);
			break;
		case '\r':
			DisableEditingControl(acceptNewValue: true);
			break;
		}
	}

	private void activeEditingControl_Leave(object sender, EventArgs e)
	{
	}

	private void comboBox_DropDownClosed(object sender, EventArgs e)
	{
		if (m_comboBox.SelectedIndex >= 0)
		{
			m_comboBox.Text = m_comboBox.Items[m_comboBox.SelectedIndex].ToString();
			DisableEditingControl(acceptNewValue: true);
		}
		else
		{
			DisableEditingControl(acceptNewValue: false);
		}
	}

	private void DisableEditingControl(bool acceptNewValue)
	{
		if (m_activeEditingControl == null)
		{
			return;
		}
		bool flag = !acceptNewValue;
		if (acceptNewValue)
		{
			ListViewCellValidatingEventArgs e = new ListViewCellValidatingEventArgs(m_currentRow, m_currentCol, m_activeEditingControl.Text);
			OnCellCellValidating(e);
			if (e.Cancel)
			{
				flag = true;
			}
			else if (m_activeEditingControl == m_textBox)
			{
				SetPropertyFromTextBox();
			}
			else if (m_activeEditingControl == m_comboBox)
			{
				SetPropertyFromComboBox();
			}
		}
		if (flag)
		{
			if (m_activeEditingControl == m_textBox)
			{
				SetTextBoxFromProperty();
			}
			else if (m_activeEditingControl == m_comboBox)
			{
				SetComboBoxFromProperty();
			}
		}
		m_activeEditingControl.Leave -= activeEditingControl_Leave;
		m_activeEditingControl.KeyPress -= activeEditingControl_KeyPress;
		if (m_activeEditingControl == m_textBox)
		{
			DisableTextBox();
		}
		else if (m_activeEditingControl == m_comboBox)
		{
			DisableComboBox();
		}
		m_activeEditingControl = null;
		OnCellEndEdit(new ListViewCellEventArgs(m_currentRow, m_currentCol));
		Focus();
	}

	private void EnableTextBox()
	{
		m_textBox.Show();
		m_textBox.Focus();
	}

	private void DisableTextBox()
	{
		m_textBox.Hide();
	}

	private void EnableComboBox()
	{
		m_comboBox.DroppedDown = true;
		m_comboBox.Show();
	}

	private void DisableComboBox()
	{
		m_comboBox.Hide();
	}

	private void SetTextBoxFromProperty()
	{
		string propertyText = PropertyUtils.GetPropertyText(m_currencyManager.List[m_currentRow], m_propertyDescriptors[m_currentCol]);
		m_textBox.Text = propertyText;
		m_textBox.ReadOnly = m_propertyDescriptors[m_currentCol].IsReadOnly;
	}

	private void SetPropertyFromTextBox()
	{
		if (m_settingValue)
		{
			return;
		}
		try
		{
			m_settingValue = true;
			string newText = m_textBox.Text;
			if (TryConvertString(newText, out var value))
			{
				PropertyUtils.SetProperty(m_currencyManager.List[m_currentRow], m_propertyDescriptors[m_currentCol], value);
			}
		}
		finally
		{
			m_settingValue = false;
		}
	}

	private bool TryConvertString(string newText, out object value)
	{
		bool result = false;
		value = newText;
		try
		{
			TypeConverter converter = m_propertyDescriptors[m_currentCol].Converter;
			if (converter != null && value != null && converter.CanConvertFrom(value.GetType()))
			{
				value = converter.ConvertFrom(value);
			}
			result = true;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "Error".Localize());
		}
		return result;
	}

	private void SetComboBoxFromProperty()
	{
		string propertyText = PropertyUtils.GetPropertyText(m_currencyManager.List[m_currentRow], m_propertyDescriptors[m_currentCol]);
		m_comboBox.Text = propertyText;
		m_comboBox.SelectedItem = m_propertyDescriptors[m_currentCol].GetValue(m_currencyManager.List[m_currentRow]);
	}

	private void SetPropertyFromComboBox()
	{
		if (m_settingValue)
		{
			return;
		}
		try
		{
			m_settingValue = true;
			PropertyUtils.SetProperty(m_currencyManager.List[m_currentRow], m_propertyDescriptors[m_currentCol], m_comboBox.SelectedItem);
		}
		finally
		{
			m_settingValue = false;
		}
	}

	private ListViewGroup GroupingDataItem(object dataItem)
	{
		ListViewGroup listViewGroup = null;
		MemberInfo type = dataItem.GetType();
		object[] customAttributes = type.GetCustomAttributes(inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (!(customAttributes[i] is GroupAttribute))
			{
				continue;
			}
			GroupAttribute groupAttribute = (GroupAttribute)customAttributes[i];
			foreach (ListViewGroup group in base.Groups)
			{
				if (group.Name == groupAttribute.GroupName)
				{
					listViewGroup = group;
					break;
				}
			}
			if (listViewGroup == null)
			{
				listViewGroup = new ListViewGroup(groupAttribute.GroupName, groupAttribute.Header);
				base.Groups.Add(listViewGroup);
			}
			if (groupAttribute.ReadOnlyProperties != null)
			{
				string[] array = groupAttribute.ReadOnlyProperties.Split(',');
				string[] array2 = array;
				foreach (string value in array2)
				{
					m_groupReadOnlyColumns.Add(groupAttribute.GroupName, value);
				}
			}
			if (groupAttribute.ExternalEditorProperties != null)
			{
				string[] array3 = groupAttribute.ExternalEditorProperties.Split(',');
				string[] array4 = array3;
				foreach (string value2 in array4)
				{
					m_groupExternalEditorColumns.Add(groupAttribute.GroupName, value2);
				}
			}
			break;
		}
		return listViewGroup;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

public class ListViewAdapter
{
	public class ListViewItemSorter : IComparer
	{
		private readonly ListView m_listView;

		private int m_column = -1;

		private int m_direction = 1;

		public ListViewItemSorter(ListView listView)
		{
			m_listView = listView;
			listView.ColumnClick += listview_ColumnClick;
		}

		public int Compare(object x, object y)
		{
			if (m_column == -1)
			{
				return 0;
			}
			ListViewItem.ListViewSubItem listViewSubItem = ((ListViewItem)x).SubItems[m_column];
			ListViewItem.ListViewSubItem listViewSubItem2 = ((ListViewItem)y).SubItems[m_column];
			if (listViewSubItem == null && listViewSubItem2 == null)
			{
				return 0;
			}
			if (listViewSubItem == null)
			{
				return -1;
			}
			if (listViewSubItem2 == null)
			{
				return 1;
			}
			object obj;
			object obj2;
			if (listViewSubItem.Tag == null && listViewSubItem2.Tag == null)
			{
				obj = listViewSubItem.Text;
				obj2 = listViewSubItem2.Text;
			}
			else
			{
				obj = listViewSubItem.Tag;
				obj2 = listViewSubItem2.Tag;
			}
			int num;
			if (obj is IComparable comparable)
			{
				num = comparable.CompareTo(obj2);
			}
			else
			{
				string objectString = GetObjectString(obj);
				string objectString2 = GetObjectString(obj2);
				num = string.Compare(objectString, objectString2, StringComparison.CurrentCultureIgnoreCase);
			}
			return num * m_direction;
		}

		public void DetachFromListView()
		{
			m_listView.ColumnClick -= listview_ColumnClick;
		}

		private string GetObjectString(object obj)
		{
			if (obj is IFormattable formattable)
			{
				return formattable.ToString(null, null);
			}
			return obj.ToString();
		}

		private void listview_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			IntPtr handle = (IntPtr)User32.SendMessage(m_listView.Handle, 4127, (int)IntPtr.Zero, (int)IntPtr.Zero);
			IntPtr wParam = new IntPtr(e.Column);
			IntPtr wParam2 = new IntPtr(m_column);
			User32.HDITEM lParam;
			if (m_column == e.Column)
			{
				m_direction = -m_direction;
			}
			else if (m_column != -1)
			{
				lParam = new User32.HDITEM
				{
					mask = 4u
				};
				User32.SendMessageITEM(handle, 4619, wParam2, ref lParam);
				lParam.fmt &= -1537;
				User32.SendMessageITEM(handle, 4620, wParam2, ref lParam);
			}
			lParam = new User32.HDITEM
			{
				mask = 4u
			};
			User32.SendMessageITEM(handle, 4619, wParam, ref lParam);
			if (m_direction == 1)
			{
				lParam.fmt &= -513;
				lParam.fmt |= 1024;
			}
			else
			{
				lParam.fmt &= -1025;
				lParam.fmt |= 512;
			}
			User32.SendMessageITEM(handle, 4620, wParam, ref lParam);
			m_column = e.Column;
			m_listView.ListViewItemSorter = null;
			m_listView.ListViewItemSorter = this;
		}
	}

	private readonly ListView m_control;

	private IListView m_listView;

	private IItemView m_itemView;

	private IValidationContext m_validationContext;

	private IObservableContext m_observableContext;

	private ISelectionContext m_selectionContext;

	private bool m_inTransaction;

	private readonly Dictionary<object, int> m_itemsInserted = new Dictionary<object, int>();

	private readonly HashSet<object> m_itemsRemoved = new HashSet<object>();

	private readonly HashSet<object> m_itemsChanged = new HashSet<object>();

	private object m_lastHit;

	private readonly Dictionary<object, ListViewItem> m_itemToListItemMap = new Dictionary<object, ListViewItem>();

	private string[] m_columnNames = EmptyArray<string>.Instance;

	private readonly Dictionary<string, int> m_columnWidths = new Dictionary<string, int>();

	private bool m_allowSorting;

	private bool m_changingSelection;

	private ListViewItem m_selectionStartItem;

	public ListView Control => m_control;

	public bool AllowSorting
	{
		get
		{
			return m_allowSorting;
		}
		set
		{
			if (m_allowSorting != value)
			{
				m_allowSorting = value;
				if (!value)
				{
					((ListViewItemSorter)m_control.ListViewItemSorter).DetachFromListView();
				}
				m_control.ListViewItemSorter = (value ? new ListViewItemSorter(m_control) : null);
			}
		}
	}

	public IListView ListView
	{
		get
		{
			return m_listView;
		}
		set
		{
			if (m_listView != value)
			{
				if (m_listView != null)
				{
					m_itemView = null;
					if (m_observableContext != null)
					{
						m_observableContext.ItemInserted -= list_ItemInserted;
						m_observableContext.ItemRemoved -= list_ItemRemoved;
						m_observableContext.ItemChanged -= list_ItemChanged;
						m_observableContext.Reloaded -= list_Reloaded;
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
				m_listView = value;
				if (m_listView != null)
				{
					m_itemView = m_listView.As<IItemView>();
					m_observableContext = m_listView.As<IObservableContext>();
					if (m_observableContext != null)
					{
						m_observableContext.ItemInserted += list_ItemInserted;
						m_observableContext.ItemRemoved += list_ItemRemoved;
						m_observableContext.ItemChanged += list_ItemChanged;
						m_observableContext.Reloaded += list_Reloaded;
					}
					m_validationContext = m_listView.As<IValidationContext>();
					if (m_validationContext != null)
					{
						m_validationContext.Beginning += validationContext_Beginning;
						m_validationContext.Ended += validationContext_Ended;
						m_validationContext.Cancelled += validationContext_Cancelled;
					}
					m_selectionContext = m_listView.As<ISelectionContext>();
					if (m_selectionContext != null)
					{
						m_selectionContext.SelectionChanged += selection_Changed;
					}
				}
			}
			Load();
		}
	}

	public object LastHit => m_lastHit;

	public string Settings
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
				throw new Exception("Invalid ListView settings");
			}
			XmlNodeList xmlNodeList = documentElement.SelectNodes("Column");
			foreach (XmlElement item in xmlNodeList)
			{
				string attribute = item.GetAttribute("Name");
				string attribute2 = item.GetAttribute("Width");
				if (attribute2 != null && int.TryParse(attribute2, out var result))
				{
					m_columnWidths[attribute] = result;
				}
			}
			m_control.SuspendLayout();
			foreach (ColumnHeader column in m_control.Columns)
			{
				SetColumnWidth(column);
			}
			m_control.ResumeLayout();
		}
	}

	public event EventHandler LastHitChanged;

	public event EventHandler<LabelEditedEventArgs<object>> LabelEdited;

	public event EventHandler<ItemSelectedEventArgs<object>> ItemSelected;

	public ListViewAdapter()
		: this(new ListView())
	{
	}

	public ListViewAdapter(ListView listView)
	{
		m_control = listView;
		m_control.View = View.Details;
		m_control.FullRowSelect = true;
		m_control.HideSelection = false;
		m_allowSorting = true;
		m_control.ListViewItemSorter = new ListViewItemSorter(m_control);
		m_control.AfterLabelEdit += control_AfterLabelEdit;
		m_control.ColumnWidthChanged += control_ColumnWidthChanged;
		m_control.MouseDown += control_MouseDown;
		m_control.MouseUp += control_MouseUp;
		m_control.DragOver += control_DragOver;
		m_control.DragDrop += control_DragDrop;
	}

	public void SetColumnWidth(string columnName, int columnWidth)
	{
		m_columnWidths[columnName] = columnWidth;
		m_control.SuspendLayout();
		foreach (ColumnHeader column in m_control.Columns)
		{
			if (column.Text == columnName)
			{
				SetColumnWidth(column);
				break;
			}
		}
		m_control.ResumeLayout();
	}

	protected virtual void OnLastHitChanged(EventArgs e)
	{
		this.LastHitChanged.Raise(this, e);
	}

	public object GetItemAt(Point clientPoint)
	{
		return m_control.GetItemAt(clientPoint.X, clientPoint.Y)?.Tag;
	}

	public object[] GetSelectedItems()
	{
		List<object> list = new List<object>();
		foreach (ListViewItem selectedItem in m_control.SelectedItems)
		{
			list.Add(selectedItem.Tag);
		}
		return list.ToArray();
	}

	public ListViewItem GetListViewItem(object item)
	{
		m_itemToListItemMap.TryGetValue(item, out var value);
		return value;
	}

	private void control_DragDrop(object sender, DragEventArgs e)
	{
		Point lastHit = m_control.PointToClient(new Point(e.X, e.Y));
		SetLastHit(lastHit);
	}

	private void control_DragOver(object sender, DragEventArgs e)
	{
		Point lastHit = m_control.PointToClient(new Point(e.X, e.Y));
		SetLastHit(lastHit);
	}

	private void control_MouseUp(object sender, MouseEventArgs e)
	{
		if (m_selectionContext == null)
		{
			return;
		}
		Point lastHit = new Point(e.X, e.Y);
		SetLastHit(lastHit);
		ListViewItem itemAt = Control.GetItemAt(e.X, e.Y);
		if (itemAt == null)
		{
			return;
		}
		object tag = itemAt.Tag;
		if (tag == null)
		{
			return;
		}
		HashSet<object> hashSet = null;
		HashSet<object> hashSet2 = null;
		EventHandler<ItemSelectedEventArgs<object>> eventHandler = this.ItemSelected;
		if (eventHandler != null)
		{
			hashSet = new HashSet<object>(m_selectionContext.Selection);
		}
		switch (System.Windows.Forms.Control.ModifierKeys)
		{
		case Keys.Shift:
			if (m_selectionStartItem != null)
			{
				int num = Math.Min(m_selectionStartItem.Index, itemAt.Index);
				int num2 = Math.Max(m_selectionStartItem.Index, itemAt.Index);
				hashSet2 = new HashSet<object>();
				for (int i = num; i <= num2; i++)
				{
					ListViewItem listViewItem = Control.Items[i];
					if (listViewItem != null && listViewItem.Tag != null)
					{
						hashSet2.Add(listViewItem.Tag);
					}
				}
				m_selectionContext.SetRange(hashSet2);
			}
			else
			{
				m_selectionContext.Set(itemAt);
				m_selectionStartItem = itemAt;
			}
			break;
		case Keys.Control:
			if (m_selectionContext.SelectionContains(tag))
			{
				m_selectionContext.Remove(tag);
			}
			else
			{
				m_selectionContext.Add(tag);
			}
			m_selectionStartItem = itemAt;
			break;
		default:
			m_selectionContext.Set(tag);
			m_selectionStartItem = itemAt;
			break;
		}
		if (eventHandler == null)
		{
			return;
		}
		if (hashSet2 == null)
		{
			hashSet2 = new HashSet<object>(m_selectionContext.Selection);
		}
		foreach (object item in hashSet.Except(hashSet2))
		{
			OnItemSelected(item, selected: false);
		}
		foreach (object item2 in hashSet2.Except(hashSet))
		{
			OnItemSelected(item2, selected: true);
		}
	}

	private void control_MouseDown(object sender, MouseEventArgs e)
	{
		Point lastHit = new Point(e.X, e.Y);
		SetLastHit(lastHit);
	}

	private void OnItemSelected(object t, bool selected)
	{
		this.ItemSelected?.Invoke(this, new ItemSelectedEventArgs<object>(t, selected));
	}

	private void control_AfterLabelEdit(object sender, LabelEditEventArgs e)
	{
		EventHandler<LabelEditedEventArgs<object>> eventHandler = this.LabelEdited;
		if (eventHandler != null && e.Label != null)
		{
			ListViewItem listViewItem = m_control.Items[e.Item];
			eventHandler(this, new LabelEditedEventArgs<object>(listViewItem.Tag, e.Label));
		}
	}

	private void control_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
	{
		ColumnHeader columnHeader = m_control.Columns[e.ColumnIndex];
		m_columnWidths[columnHeader.Text] = columnHeader.Width;
	}

	private void list_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		if (m_inTransaction && !m_itemsInserted.ContainsKey(e.Item))
		{
			m_itemsInserted.Add(e.Item, e.Index);
		}
		else
		{
			OnItemInserted(e.Item, e.Index);
		}
	}

	private void OnItemInserted(object item, int index)
	{
		if (GetListViewItem(item) == null)
		{
			ListViewItem listViewItem = CreateItem(item);
			if (index > -1 && index < m_control.Items.Count)
			{
				m_control.Items.Insert(index, listViewItem);
			}
			else
			{
				m_control.Items.Add(listViewItem);
			}
		}
	}

	private void list_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		if (m_inTransaction)
		{
			m_itemsRemoved.Add(e.Item);
		}
		else
		{
			OnItemRemoved(e.Item);
		}
	}

	private void OnItemRemoved(object item)
	{
		if (m_itemToListItemMap.TryGetValue(item, out var value))
		{
			m_itemToListItemMap.Remove(value.Tag);
			if (object.Equals(m_lastHit, value.Tag))
			{
				SetLastHit(null);
			}
			m_changingSelection = true;
			m_control.Items.Remove(value);
			m_changingSelection = false;
		}
	}

	private void list_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		if (m_inTransaction)
		{
			m_itemsChanged.Add(e.Item);
		}
		else
		{
			OnItemChanged(e.Item);
		}
	}

	private void OnItemChanged(object item)
	{
		if (m_itemToListItemMap.TryGetValue(item, out var value))
		{
			UpdateItem(value);
		}
	}

	private void list_Reloaded(object sender, EventArgs e)
	{
		Load();
	}

	private void validationContext_Beginning(object sender, EventArgs e)
	{
		m_inTransaction = true;
	}

	private void validationContext_Ended(object sender, EventArgs e)
	{
		m_inTransaction = false;
		m_itemsChanged.RemoveWhere((object item) => m_itemsInserted.ContainsKey(item) || m_itemsRemoved.Contains(item));
		if (!m_itemsInserted.Any() && !m_itemsRemoved.Any() && !m_itemsChanged.Any())
		{
			return;
		}
		m_control.SuspendLayout();
		m_control.BeginUpdate();
		foreach (KeyValuePair<object, int> item in m_itemsInserted)
		{
			OnItemInserted(item.Key, item.Value);
		}
		m_itemsInserted.Clear();
		foreach (object item2 in m_itemsRemoved)
		{
			OnItemRemoved(item2);
		}
		m_itemsRemoved.Clear();
		foreach (object item3 in m_itemsChanged)
		{
			OnItemChanged(item3);
		}
		m_itemsChanged.Clear();
		if (m_selectionContext != null)
		{
			foreach (KeyValuePair<object, ListViewItem> item4 in m_itemToListItemMap)
			{
				item4.Value.Selected = m_selectionContext.SelectionContains(item4.Key);
			}
		}
		m_control.FocusedItem = null;
		m_control.EndUpdate();
		m_control.ResumeLayout();
	}

	private void validationContext_Cancelled(object sender, EventArgs e)
	{
		m_inTransaction = false;
		m_itemsInserted.Clear();
		m_itemsRemoved.Clear();
		m_itemsChanged.Clear();
	}

	private void selection_Changed(object sender, EventArgs e)
	{
		if (m_changingSelection)
		{
			return;
		}
		try
		{
			m_changingSelection = true;
			m_control.SelectedItems.Clear();
			foreach (object item in m_selectionContext.Selection)
			{
				if (m_itemToListItemMap.TryGetValue(item, out var value))
				{
					value.Selected = true;
				}
			}
		}
		finally
		{
			m_changingSelection = false;
		}
	}

	private void Load()
	{
		Unload();
		if (m_listView == null)
		{
			return;
		}
		BuildListColumns();
		List<ListViewItem> list = new List<ListViewItem>();
		foreach (object item in m_listView.Items)
		{
			list.Add(CreateItem(item));
		}
		ListViewItem[] array = new ListViewItem[list.Count];
		list.CopyTo(array);
		m_control.Items.AddRange(array);
	}

	private void Unload()
	{
		m_control.Items.Clear();
		m_itemToListItemMap.Clear();
	}

	private List<object> GetSelectedObjects()
	{
		List<object> list = new List<object>();
		foreach (ListViewItem selectedItem in m_control.SelectedItems)
		{
			if (selectedItem.Tag != null)
			{
				object tag = selectedItem.Tag;
				ItemInfo itemInfo = new WinFormsItemInfo(m_control.SmallImageList);
				m_itemView.GetInfo(tag, itemInfo);
				if (itemInfo.AllowSelect)
				{
					list.Add(tag);
				}
			}
		}
		return list;
	}

	private ListViewItem CreateItem(object item)
	{
		m_changingSelection = true;
		ListViewItem listViewItem = new ListViewItem();
		m_changingSelection = false;
		listViewItem.Tag = item;
		m_itemToListItemMap.Add(item, listViewItem);
		UpdateItem(listViewItem);
		return listViewItem;
	}

	private void UpdateItem(ListViewItem item)
	{
		ItemInfo itemInfo = new WinFormsItemInfo(m_control.SmallImageList);
		m_itemView.GetInfo(item.Tag, itemInfo);
		string label = itemInfo.Label;
		item.SubItems.Clear();
		if (itemInfo.Properties != null)
		{
			int num = Math.Min(m_columnNames.Length, itemInfo.Properties.Length);
			for (int i = 0; i < num; i++)
			{
				object obj = itemInfo.Properties[i];
				ListViewItem.ListViewSubItem listViewSubItem = new ListViewItem.ListViewSubItem(item, GetObjectString(obj));
				listViewSubItem.Tag = obj;
				item.SubItems.Add(listViewSubItem);
			}
		}
		item.Text = label;
		item.ImageIndex = itemInfo.ImageIndex;
		item.Checked = itemInfo.Checked;
		if (item.Font.Style != itemInfo.FontStyle)
		{
			using (item.Font)
			{
				item.Font = new Font(item.Font, itemInfo.FontStyle);
			}
		}
	}

	private string GetObjectString(object obj)
	{
		if (obj is IFormattable formattable)
		{
			return formattable.ToString(null, null);
		}
		return obj.ToString();
	}

	private void BuildListColumns()
	{
		List<string> list = new List<string>();
		if (m_listView != null)
		{
			list.AddRange(m_listView.ColumnNames);
		}
		bool flag = m_columnNames.Length != list.Count;
		if (!flag)
		{
			for (int i = 0; i < m_columnNames.Length; i++)
			{
				if (m_columnNames[i] != list[i])
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			m_control.Columns.Clear();
			m_columnNames = list.ToArray();
			string[] columnNames = m_columnNames;
			foreach (string text in columnNames)
			{
				ColumnHeader columnHeader = new ColumnHeader();
				columnHeader.Text = text;
				SetColumnWidth(columnHeader);
				m_control.Columns.Add(columnHeader);
			}
		}
	}

	private void SetColumnWidth(ColumnHeader column)
	{
		if (m_columnWidths.TryGetValue(column.Text, out var value))
		{
			column.Width = value;
		}
		else
		{
			m_columnWidths[column.Text] = column.Width;
		}
	}

	private void SetLastHit(Point clientPoint)
	{
		object obj = GetItemAt(clientPoint);
		if (obj == null)
		{
			obj = ListView;
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

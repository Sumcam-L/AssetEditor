using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.PropertyEditing;

public class ArrayEditingControl : Control
{
	private class ItemControl : Panel
	{
		private bool m_useModifierKeys;

		private int m_index;

		private bool m_selected;

		private Label m_selectButton;

		private readonly NumericTextBox m_editControl;

		public int Index
		{
			get
			{
				return m_index;
			}
			set
			{
				if (m_index != value)
				{
					m_index = value;
					if (m_selectButton != null)
					{
						m_selectButton.Text = value.ToString();
						m_selectButton.ForeColor = (m_selected ? SystemColors.HighlightText : ForeColor);
						m_selectButton.BackColor = (m_selected ? SystemColors.Highlight : UnselectedColor);
					}
				}
			}
		}

		public object Value
		{
			get
			{
				return m_editControl.Value;
			}
			set
			{
				m_editControl.Value = value;
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
				m_selected = value;
				m_selectButton.ForeColor = (value ? SystemColors.HighlightText : ForeColor);
				m_selectButton.BackColor = (value ? SystemColors.Highlight : UnselectedColor);
				m_selectButton.Refresh();
			}
		}

		private Color UnselectedColor
		{
			get
			{
				Color backColor = BackColor;
				backColor = ((backColor.GetBrightness() > 0.5f) ? ControlPaint.Dark(backColor, 0.15f) : ControlPaint.Light(backColor, 0.15f));
				return (Index % 2 == 0) ? BackColor : backColor;
			}
		}

		public event EventHandler ValueChanged;

		public ItemControl(int index, object item, int indexColumnWidth)
		{
			m_index = index;
			m_editControl = new NumericTextBox(item.GetType())
			{
				Dock = DockStyle.Fill,
				BorderStyle = BorderStyle.None
			};
			m_editControl.Width = base.Width - indexColumnWidth;
			m_editControl.Value = item;
			m_editControl.Invalidated += editControl_Invalidated;
			m_editControl.ValueEdited += m_editControl_ValueChanged;
			m_selectButton = new Label
			{
				Width = indexColumnWidth,
				Dock = DockStyle.Left,
				Text = Index.ToString(),
				TextAlign = ContentAlignment.MiddleCenter,
				BackColor = UnselectedColor,
				FlatStyle = FlatStyle.Flat,
				Font = s_regularFont
			};
			m_selectButton.MouseDown += selectButton_MouseDown;
			base.Height = m_editControl.Height;
			base.Controls.Add(m_editControl);
			base.Controls.Add(m_selectButton);
			m_editControl.SizeChanged += delegate
			{
				base.Height = m_editControl.Height;
			};
			base.GotFocus += delegate
			{
				m_editControl.Focus();
			};
			m_editControl.GotFocus += delegate
			{
				UpdateSelection();
			};
		}

		private void UpdateSelection()
		{
			ArrayEditingControl arrayEditingControl = (ArrayEditingControl)base.Parent;
			arrayEditingControl.SelectItemControl(this, m_useModifierKeys);
		}

		private void m_editControl_ValueChanged(object sender, EventArgs e)
		{
			this.ValueChanged.Raise(this, e);
		}

		private void editControl_Invalidated(object sender, InvalidateEventArgs e)
		{
			base.Height = m_editControl.Height;
		}

		public override void Refresh()
		{
			base.Refresh();
			m_editControl.Refresh();
		}

		private void selectButton_MouseDown(object sender, EventArgs e)
		{
			try
			{
				m_useModifierKeys = true;
				if (!m_editControl.Focused)
				{
					m_editControl.Focus();
				}
				else
				{
					UpdateSelection();
				}
			}
			finally
			{
				m_useModifierKeys = false;
			}
		}
	}

	private int m_firstSelectedIndex;

	private PropertyEditorControlContext m_context;

	private bool m_showToolStripLabels = true;

	private int m_showToolStripLabelThreshold = 300;

	private object m_activeCollectionNode;

	private int m_indexColumnWidth = 30;

	private int m_lastKnownSize = int.MinValue;

	private DomNode m_initialSelectedObject;

	private const int Up = -1;

	private const int Down = 1;

	private readonly Dictionary<int, ItemControl> m_itemControls = new Dictionary<int, ItemControl>();

	private readonly ToolStrip m_toolStrip;

	private ToolStripButton m_addButton;

	private ToolStripButton m_deleteButton;

	private ToolStripButton m_moveUpButton;

	private ToolStripButton m_moveDownButton;

	private static readonly Image s_addImage = ResourceUtil.GetImage16(Resources.AddImage);

	private static readonly Image s_removeImage = ResourceUtil.GetImage16(Resources.RemoveImage);

	private static readonly Image s_moveUpImage = ResourceUtil.GetImage16(Resources.ArrowUpImage);

	private static readonly Image s_moveDownImage = ResourceUtil.GetImage16(Resources.ArrowDownImage);

	private static readonly Font s_regularFont = new Font(FontFamily.GenericSansSerif, 8.25f, FontStyle.Regular);

	private ITransactionContext TransactionContext { get; set; }

	public ArrayEditingControl(PropertyEditorControlContext context)
	{
		m_context = context;
		m_initialSelectedObject = m_context.LastSelectedObject.As<DomNodeAdapter>().DomNode;
		m_initialSelectedObject.AttributeChanged += DomNode_AttributeChanged;
		m_toolStrip = new ToolStrip
		{
			Dock = DockStyle.Top
		};
		InitToolStrip();
		base.Controls.Add(m_toolStrip);
		base.Height = m_toolStrip.Height;
		m_toolStrip.SizeChanged += toolStrip_SizeChanged;
		IContextRegistry contextRegistry = m_context.ContextRegistry;
		if (contextRegistry != null)
		{
			contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
			TransactionContext = contextRegistry.GetActiveContext<ITransactionContext>();
		}
		else if (context.TransactionContext != null)
		{
			TransactionContext = context.TransactionContext;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_initialSelectedObject.AttributeChanged -= DomNode_AttributeChanged;
		}
		base.Dispose(disposing);
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		int num = 0;
		for (int i = 1; i < base.Controls.Count; i++)
		{
			if (base.Controls[i].ContainsFocus)
			{
				num = i;
				break;
			}
		}
		if (keyData == Keys.Tab || keyData == Keys.Return)
		{
			if (num < base.Controls.Count - 1)
			{
				base.Controls[num + 1].Select();
				return true;
			}
		}
		else if (keyData == (Keys.Tab | Keys.Shift) && num > 1)
		{
			base.Controls[num - 1].Select();
			return true;
		}
		return base.ProcessDialogKey(keyData);
	}

	private void InitToolStrip()
	{
		m_toolStrip.TabStop = false;
		m_addButton = new ToolStripButton
		{
			Text = "Add".Localize(),
			Image = s_addImage,
			ToolTipText = "Add array element".Localize()
		};
		m_addButton.Click += addButton_Click;
		m_toolStrip.Items.Add(m_addButton);
		m_deleteButton = new ToolStripButton
		{
			Text = "Delete".Localize(),
			Image = s_removeImage,
			ToolTipText = "Delete array element(s)"
		};
		UpdateDeleteButton(countSelected: false);
		m_deleteButton.Click += deleteButton_Click;
		m_toolStrip.Items.Add(m_deleteButton);
		m_moveUpButton = new ToolStripButton
		{
			Text = "Up".Localize("this is the name of a button that causes the selected item to be moved up in a list"),
			Image = s_moveUpImage,
			ToolTipText = "Move array element up"
		};
		m_moveDownButton = new ToolStripButton
		{
			Text = "Down".Localize("this is the name of a button that causes the selected item to be moved down in a list"),
			Image = s_moveDownImage,
			ToolTipText = "Move array element down"
		};
		UpdateMoveButtons(countSelected: false);
		m_moveUpButton.Click += m_moveUpButton_Click;
		m_moveDownButton.Click += m_moveDownButton_Click;
		m_toolStrip.Items.Add(m_moveUpButton);
		m_toolStrip.Items.Add(m_moveDownButton);
	}

	private void toolStrip_SizeChanged(object sender, EventArgs e)
	{
		if (m_showToolStripLabels)
		{
			m_showToolStripLabelThreshold = m_toolStrip.Items.Cast<ToolStripItem>().Sum((ToolStripItem item) => item.Width);
		}
		m_showToolStripLabels = m_toolStrip.Width > m_showToolStripLabelThreshold;
		ToolStripItemDisplayStyle displayStyle = (m_showToolStripLabels ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image);
		m_addButton.DisplayStyle = displayStyle;
		m_deleteButton.DisplayStyle = displayStyle;
	}

	private void addButton_Click(object sender, EventArgs e)
	{
		TransactionContext.DoTransaction(delegate
		{
			Array array = m_context.Descriptor.GetValue(m_context.LastSelectedObject) as Array;
			Type elementType = array.GetType().GetElementType();
			Array array2 = Array.CreateInstance(elementType, array.Length + 1);
			array.CopyTo(array2, 0);
			m_context.Descriptor.SetValue(m_context.LastSelectedObject, array2);
		}, "add array element".Localize());
	}

	private void deleteButton_Click(object sender, EventArgs e)
	{
		DeleteSelectedItems();
	}

	private void m_moveUpButton_Click(object sender, EventArgs e)
	{
		MoveSelectedItems(-1);
	}

	private void m_moveDownButton_Click(object sender, EventArgs e)
	{
		MoveSelectedItems(1);
	}

	private void UpdateMoveButtons(bool countSelected)
	{
		if (m_context.IsReadOnly)
		{
			m_moveUpButton.Enabled = false;
			m_moveDownButton.Enabled = false;
			return;
		}
		int num = 0;
		int num2 = int.MaxValue;
		int num3 = int.MinValue;
		if (countSelected)
		{
			foreach (ItemControl value in m_itemControls.Values)
			{
				if (value.Selected)
				{
					num++;
					if (value.Index < num2)
					{
						num2 = value.Index;
					}
					if (value.Index > num3)
					{
						num3 = value.Index;
					}
				}
			}
		}
		if (num == 0)
		{
			m_moveUpButton.Enabled = false;
			m_moveUpButton.ToolTipText = "Disabled because no elements selected".Localize();
			m_moveDownButton.Enabled = false;
			m_moveDownButton.ToolTipText = "Disabled because no elements selected".Localize();
			return;
		}
		if (num2 > 0)
		{
			m_moveUpButton.Enabled = true;
			m_moveUpButton.ToolTipText = string.Format("Move {0} selected elements up".Localize(), num);
		}
		else
		{
			m_moveUpButton.Enabled = false;
			m_moveUpButton.ToolTipText = string.Format("Can't move up because first element selected".Localize(), num);
		}
		if (num3 < m_itemControls.Count - 1)
		{
			m_moveDownButton.Enabled = true;
			m_moveDownButton.ToolTipText = string.Format("Move {0} selected elements down".Localize(), num);
		}
		else
		{
			m_moveDownButton.Enabled = false;
			m_moveDownButton.ToolTipText = string.Format("Can't move down because last element selected".Localize(), num);
		}
	}

	private void MoveSelectedItems(int direction)
	{
		if (direction != -1 && direction != 1)
		{
			return;
		}
		List<KeyValuePair<int, ItemControl>> movePairs = new List<KeyValuePair<int, ItemControl>>();
		foreach (KeyValuePair<int, ItemControl> itemControl in m_itemControls)
		{
			if (itemControl.Value != null && itemControl.Value.Selected)
			{
				movePairs.Add(itemControl);
			}
		}
		if (direction < 0)
		{
			movePairs.Sort((KeyValuePair<int, ItemControl> a, KeyValuePair<int, ItemControl> b) => a.Value.Index.CompareTo(b.Value.Index));
		}
		else
		{
			movePairs.Sort((KeyValuePair<int, ItemControl> a, KeyValuePair<int, ItemControl> b) => -a.Value.Index.CompareTo(b.Value.Index));
		}
		TransactionContext.DoTransaction(delegate
		{
			foreach (KeyValuePair<int, ItemControl> item in movePairs)
			{
				Array array = m_context.Descriptor.GetValue(m_context.LastSelectedObject) as Array;
				object value = array.GetValue(item.Key);
				array.SetValue(array.GetValue(item.Key + direction), item.Key);
				array.SetValue(value, item.Key + direction);
			}
		}, "Move elements".Localize());
		RebuildItemControls(m_context.LastSelectedObject);
		foreach (KeyValuePair<int, ItemControl> item2 in movePairs)
		{
			m_itemControls[item2.Key + direction].Selected = true;
		}
		UpdateMoveButtons(countSelected: true);
		UpdateDeleteButton(countSelected: true);
	}

	private void UpdateAddButton()
	{
		m_addButton.Enabled = m_context.Descriptor.Is<AttributePropertyDescriptor>() && !m_context.Descriptor.IsReadOnly;
	}

	private void UpdateDeleteButton(bool countSelected)
	{
		if (!m_context.Descriptor.Is<AttributePropertyDescriptor>() || m_context.Descriptor.IsReadOnly)
		{
			m_deleteButton.Enabled = false;
			return;
		}
		int num = 0;
		if (countSelected)
		{
			num = m_itemControls.Values.Count((ItemControl itemControl) => itemControl.Selected);
		}
		if (num == 0)
		{
			m_deleteButton.Enabled = false;
			m_deleteButton.ToolTipText = "Disabled because no array elements selected".Localize();
			return;
		}
		m_deleteButton.Enabled = true;
		if (num > 1)
		{
			m_deleteButton.ToolTipText = string.Format("Delete {0} selected array elements".Localize(), num);
		}
		else
		{
			m_deleteButton.ToolTipText = "Delete selected array element".Localize();
		}
	}

	private void DeleteSelectedItems()
	{
		List<int> deleteItemIndexes = new List<int>();
		foreach (KeyValuePair<int, ItemControl> itemControl in m_itemControls)
		{
			int key = itemControl.Key;
			ItemControl value = itemControl.Value;
			if (value != null && value.Selected)
			{
				deleteItemIndexes.Add(key);
			}
		}
		TransactionContext.DoTransaction(delegate
		{
			Array array = m_context.Descriptor.GetValue(m_context.LastSelectedObject) as Array;
			Array array2 = Array.CreateInstance(array.GetType().GetElementType(), array.Length - deleteItemIndexes.Count);
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (!deleteItemIndexes.Contains(i))
				{
					array2.SetValue(array.GetValue(i), num);
					num++;
				}
			}
			m_context.SetValue(array2);
		}, (deleteItemIndexes.Count > 1) ? "delete array elements".Localize() : "delete array element".Localize());
	}

	private void OnItemChanged(object item, object newValue)
	{
		Array array = newValue as Array;
		if (array.Length != m_lastKnownSize)
		{
			RebuildItemControls(m_context.LastSelectedObject);
		}
		for (int i = 0; i < m_itemControls.Values.Count; i++)
		{
			m_itemControls[i].Value = array.GetValue(i);
			m_itemControls[i].Refresh();
		}
	}

	private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		TransactionContext = m_context.ContextRegistry.GetActiveContext<ITransactionContext>();
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		DomNodeAdapter domNodeAdapter = m_context.LastSelectedObject.As<DomNodeAdapter>();
		if (domNodeAdapter != null && e.DomNode == domNodeAdapter.DomNode && m_context.Descriptor.Is<AttributePropertyDescriptor>() && e.AttributeInfo == m_context.Descriptor.As<AttributePropertyDescriptor>().AttributeInfo)
		{
			OnItemChanged(e.DomNode, e.NewValue);
		}
	}

	private void itemControl_ValueChanged(object sender, EventArgs e)
	{
		Array array = m_context.Descriptor.GetValue(m_context.LastSelectedObject) as Array;
		Array new_values = array.Clone() as Array;
		ItemControl selectedItem = sender as ItemControl;
		new_values.SetValue(selectedItem.Value, selectedItem.Index);
		TransactionContext.DoTransaction(delegate
		{
			m_context.SetValue(new_values);
			selectedItem.Value = selectedItem.Value;
		}, "edit array element".Localize());
	}

	private void SubscribeItemEvents(ItemControl itemControl)
	{
		itemControl.ValueChanged += itemControl_ValueChanged;
	}

	private void UnsubscribeItemEvents(ItemControl itemControl)
	{
		itemControl.ValueChanged -= itemControl_ValueChanged;
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		int num = ((base.Controls.Count > 1) ? base.Controls[1].Height : 0);
		int num2 = base.Controls[0].Height + (base.Controls.Count - 1) * num;
		if (base.Height != num2)
		{
			base.Height = num2;
		}
		int num3 = 0;
		foreach (Control control in base.Controls)
		{
			control.Top = num3;
			num3 += control.Height;
		}
		base.OnSizeChanged(e);
	}

	private void SelectItemControl(ItemControl selectedControl, bool useModifierKeys = false)
	{
		bool flag = useModifierKeys && (Control.ModifierKeys & Keys.Shift) != 0;
		bool flag2 = useModifierKeys && (Control.ModifierKeys & Keys.Control) != 0;
		int num = -1;
		if (selectedControl != null)
		{
			num = selectedControl.Index;
		}
		if (selectedControl == null || num < 0)
		{
			foreach (ItemControl value in m_itemControls.Values)
			{
				value.Selected = false;
			}
			return;
		}
		if (flag)
		{
			if (flag2)
			{
				int num2 = num;
				int num3 = num;
				foreach (ItemControl value2 in m_itemControls.Values)
				{
					if (value2.Selected)
					{
						if (value2.Index < num2)
						{
							num2 = value2.Index;
						}
						else if (value2.Index > num3)
						{
							num3 = value2.Index;
						}
					}
				}
				foreach (ItemControl value3 in m_itemControls.Values)
				{
					value3.Selected = value3.Index >= num2 && value3.Index <= num3;
				}
			}
			else
			{
				int num4 = Math.Min(num, m_firstSelectedIndex);
				int num5 = Math.Max(num, m_firstSelectedIndex);
				foreach (ItemControl value4 in m_itemControls.Values)
				{
					value4.Selected = value4.Index >= num4 && value4.Index <= num5;
				}
			}
		}
		else if (flag2)
		{
			m_firstSelectedIndex = num;
			selectedControl.Selected = !selectedControl.Selected;
		}
		else
		{
			m_firstSelectedIndex = num;
			foreach (ItemControl value5 in m_itemControls.Values)
			{
				value5.Selected = value5.Index == num;
			}
		}
		UpdateDeleteButton(countSelected: true);
		UpdateMoveButtons(countSelected: true);
	}

	public override void Refresh()
	{
		object lastSelectedObject = m_context.LastSelectedObject;
		if (m_activeCollectionNode != lastSelectedObject && lastSelectedObject != null)
		{
			RebuildItemControls(lastSelectedObject);
		}
	}

	private void RebuildItemControls(object collectionObject)
	{
		m_activeCollectionNode = collectionObject;
		Array array = m_context.Descriptor.GetValue(m_context.LastSelectedObject) as Array;
		m_lastKnownSize = array.Length;
		m_indexColumnWidth = Math.Max(30, (array.Length * 2).ToString().Length * 10);
		try
		{
			SuspendLayout();
			m_toolStrip.Enabled = false;
			foreach (ItemControl value in m_itemControls.Values)
			{
				UnsubscribeItemEvents(value);
				base.Controls.Remove(value);
				value.Dispose();
			}
			m_itemControls.Clear();
			int num = m_toolStrip.Height;
			List<ItemControl> list = new List<ItemControl>();
			int num2 = 0;
			foreach (object item in array)
			{
				Font font = ((base.Parent != null) ? base.Parent.Font : Font);
				ItemControl itemControl = new ItemControl(m_itemControls.Count, item, m_indexColumnWidth)
				{
					Width = m_toolStrip.Width,
					Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
					Top = num,
					Index = num2,
					Font = font
				};
				num += itemControl.Height;
				m_itemControls.Add(num2, itemControl);
				list.Add(itemControl);
				SubscribeItemEvents(itemControl);
				num2++;
			}
			base.Controls.AddRange(list.ToArray());
			base.Height = num;
			UpdateAddButton();
			UpdateDeleteButton(countSelected: true);
			UpdateMoveButtons(countSelected: true);
		}
		catch (Win32Exception ex)
		{
			foreach (ItemControl value2 in m_itemControls.Values)
			{
				if (base.Controls.Contains(value2))
				{
					base.Controls.Remove(value2);
				}
				value2.Dispose();
			}
			m_itemControls.Clear();
			MessageBox.Show("Failed to create item controls, probably because there were not enough Window handles available. Consider using a different editor for collections of this size and nature.\r\n\r\n" + ex.GetType().ToString() + Environment.NewLine + ex.Message, "Failed to create item controls", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		finally
		{
			ResumeLayout();
			m_toolStrip.Enabled = true;
		}
		SkinService.ApplyActiveSkin(this);
		foreach (ItemControl value3 in m_itemControls.Values)
		{
			value3.Selected = value3.Selected;
		}
	}
}

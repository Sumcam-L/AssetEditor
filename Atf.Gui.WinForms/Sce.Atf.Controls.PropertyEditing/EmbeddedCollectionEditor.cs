using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class EmbeddedCollectionEditor : IPropertyEditor, IAnnotatedParams
{
	public enum ItemLabelType
	{
		kIndex,
		kName,
		kNone
	}

	public class ItemInserter
	{
		public string ItemTypeName { get; set; }

		public Image Image { get; set; }

		public Func<object> InsertItemFunc { get; set; }

		public ItemInserter(string itemTypeName, Func<object> insertItemFunc)
		{
			ItemTypeName = itemTypeName;
			InsertItemFunc = insertItemFunc;
		}

		public ItemInserter(string itemTypeName, Image image, Func<object> insertItemFunc)
		{
			ItemTypeName = itemTypeName;
			Image = image;
			InsertItemFunc = insertItemFunc;
		}
	}

	public class CollectionControl : Control, ICacheablePropertyControl
	{
		private class ItemControl : Panel
		{
			private class EmbeddedPropertyEditingContext : PropertyEditingContext, IAdaptable
			{
				private object m_context;

				public EmbeddedPropertyEditingContext(object[] selection, object context)
					: base(selection)
				{
					m_context = context;
				}

				public object GetAdapter(Type type)
				{
					return m_context.As<IAdaptable>()?.GetAdapter(type);
				}
			}

			private bool m_showLabels;

			private ItemLabelType m_labelType;

			private bool m_useModifierKeys;

			private int m_index;

			private int m_visibleIndex;

			private bool m_selected;

			private Label m_selectButton;

			private PropertyGridView m_editControl;

			private PropertyGridView m_parentPropertyGridView;

			private bool m_singletonMode;

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
							m_selectButton.Text = GetLabelText();
						}
					}
				}
			}

			public int VisibleIndex
			{
				get
				{
					return m_visibleIndex;
				}
				set
				{
					m_visibleIndex = value;
					if (m_selectButton != null)
					{
						m_selectButton.ForeColor = (m_selected ? SystemColors.HighlightText : ForeColor);
						m_selectButton.BackColor = (m_selected ? SystemColors.Highlight : UnselectedColor);
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
					if (!m_singletonMode)
					{
						m_selected = value;
						m_selectButton.ForeColor = (value ? SystemColors.HighlightText : ForeColor);
						m_selectButton.BackColor = (value ? SystemColors.Highlight : UnselectedColor);
						if (!value)
						{
							m_editControl.ClearSelectedProperty();
						}
						m_selectButton.Refresh();
					}
				}
			}

			private Color UnselectedColor
			{
				get
				{
					Color backColor = BackColor;
					backColor = ((backColor.GetBrightness() > 0.5f) ? ControlPaint.Dark(backColor, 0.2f) : ControlPaint.Light(backColor, 0.2f));
					return (m_visibleIndex % 2 == 0) ? BackColor : backColor;
				}
			}

			public ItemControl(int index, object item, bool singletonMode, int indexColumnWidth, object context, PropertyGridView parentPropertyGridView, bool showLabels, ItemLabelType labelType)
			{
				m_showLabels = showLabels;
				m_labelType = labelType;
				m_editControl = new PropertyGridView
				{
					ShowScrollbar = false,
					PropertySorting = PropertySorting.None,
					Dock = DockStyle.Fill,
					ShowLabels = (m_showLabels && m_labelType == ItemLabelType.kName)
				};
				m_parentPropertyGridView = parentPropertyGridView;
				if (m_parentPropertyGridView != null)
				{
					m_editControl.CustomizeAttributes = m_parentPropertyGridView.CustomizeAttributes;
					m_editControl.DescriptionSetter = parentPropertyGridView.DescriptionSetter;
					m_parentPropertyGridView.SelectedPropertyChanged += ParentSelectedPropertyChanged;
				}
				m_editControl.MouseUp += editControl_MouseUp;
				base.Controls.Add(m_editControl);
				Init(index, item, singletonMode, indexColumnWidth, context);
				base.GotFocus += delegate
				{
					m_editControl.Focus();
				};
				m_editControl.GotFocus += delegate
				{
					UpdateSelection();
				};
				m_editControl.EditingContextUpdated += delegate
				{
					base.Height = m_editControl.GetPreferredHeight();
				};
			}

			private void ParentSelectedPropertyChanged(object sender, EventArgs eventArgs)
			{
				m_editControl.ClearSelectedProperty();
				CollectionControl collectionControl = (CollectionControl)base.Parent;
				collectionControl.SelectItemControl(null);
			}

			private void UpdateSelection()
			{
				CollectionControl collectionControl = (CollectionControl)base.Parent;
				collectionControl.SelectItemControl(this, m_useModifierKeys);
			}

			protected override void Dispose(bool disposing)
			{
				Clear();
				if (disposing && m_parentPropertyGridView != null)
				{
					m_parentPropertyGridView.SelectedPropertyChanged -= ParentSelectedPropertyChanged;
					m_parentPropertyGridView = null;
				}
				base.Dispose(disposing);
			}

			public void Clear()
			{
				m_editControl.BindingContext = null;
			}

			public void Init(int index, object item, bool singletonMode, int indexColumnWidth, object context)
			{
				m_index = index;
				m_visibleIndex = index;
				m_singletonMode = singletonMode;
				EmbeddedPropertyEditingContext editingContext = new EmbeddedPropertyEditingContext(new object[1] { item }, context);
				if (!m_singletonMode)
				{
					if (m_selectButton == null)
					{
						m_selectButton = new Label();
						base.Controls.Add(m_selectButton);
					}
					m_selectButton.Width = indexColumnWidth;
					m_selectButton.Dock = DockStyle.Left;
					m_selectButton.Text = GetLabelText();
					m_selectButton.TextAlign = ContentAlignment.MiddleCenter;
					m_selectButton.FlatStyle = FlatStyle.Flat;
					m_selectButton.MouseDown += selectButton_MouseDown;
				}
				else if (m_selectButton != null)
				{
					m_selectButton.MouseDown -= selectButton_MouseDown;
					base.Controls.Remove(m_selectButton);
					m_selectButton.Dispose();
					m_selectButton = null;
				}
				m_editControl.EditingContext = editingContext;
				base.Height = m_editControl.GetPreferredHeight();
			}

			private void editControl_MouseUp(object sender, MouseEventArgs e)
			{
				Point p = ((Control)sender).PointToScreen(e.Location);
				Point point = PointToClient(p);
				MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta);
				OnMouseUp(e2);
			}

			public override void Refresh()
			{
				base.Refresh();
				m_editControl.Refresh();
			}

			private string GetLabelText()
			{
				if (m_labelType == ItemLabelType.kIndex)
				{
					return m_index.ToString();
				}
				if (m_labelType == ItemLabelType.kName)
				{
					return m_editControl.EditingContext?.PropertyDescriptors.FirstOrDefault()?.Name ?? m_index.ToString();
				}
				return string.Empty;
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

		private bool m_processingPendingChanges;

		private readonly PropertyEditorControlContext m_context;

		private readonly EmbeddedCollectionEditor m_editor;

		private readonly PropertyGridView m_parentPropertyGridView;

		private readonly Dictionary<object, ItemControl> m_itemControls = new Dictionary<object, ItemControl>();

		private readonly List<ItemControl> m_unusedItemControls = new List<ItemControl>();

		private IObservableContext m_observableContext;

		private IValidationContext m_validationContext;

		private bool m_inTransaction;

		private readonly HashSet<object> m_pendingItemsInserted = new HashSet<object>();

		private readonly HashSet<object> m_pendingItemsRemoved = new HashSet<object>();

		private readonly HashSet<object> m_pendingItemsChanged = new HashSet<object>();

		private object m_activeCollectionNode;

		private int m_firstSelectedIndex;

		private bool m_singletonMode;

		private int m_indexColumnWidth = 30;

		private bool m_showToolStripLabels = true;

		private int m_showToolStripLabelThreshold = 300;

		private ToolStrip m_toolStrip;

		private ToolStripButton m_addButton;

		private ToolStripSplitButton m_addSplitButton;

		private ToolStripButton m_deleteButton;

		private ToolStripButton m_upButton;

		private ToolStripButton m_downButton;

		private ToolStripStatusLabel m_itemsCountLabel;

		public bool Cacheable => true;

		private IObservableContext ObservableContext
		{
			get
			{
				return m_observableContext;
			}
			set
			{
				if (m_observableContext != value)
				{
					if (m_observableContext != null)
					{
						m_observableContext.ItemInserted -= observableContext_ItemInserted;
						m_observableContext.ItemRemoved -= observableContext_ItemRemoved;
						m_observableContext.ItemChanged -= observableContext_ItemChanged;
					}
					m_observableContext = value;
					if (m_observableContext != null)
					{
						m_observableContext.ItemInserted += observableContext_ItemInserted;
						m_observableContext.ItemRemoved += observableContext_ItemRemoved;
						m_observableContext.ItemChanged += observableContext_ItemChanged;
					}
				}
			}
		}

		private IValidationContext ValidationContext
		{
			set
			{
				if (m_validationContext != value)
				{
					if (m_validationContext != null)
					{
						m_validationContext.Beginning -= validationContext_Beginning;
						m_validationContext.Cancelled -= validationContext_Cancelled;
						m_validationContext.Ended -= validationContext_Ended;
					}
					m_validationContext = value;
					if (m_validationContext != null)
					{
						m_validationContext.Beginning += validationContext_Beginning;
						m_validationContext.Cancelled += validationContext_Cancelled;
						m_validationContext.Ended += validationContext_Ended;
					}
				}
			}
		}

		private ITransactionContext TransactionContext { get; set; }

		public CollectionControl(EmbeddedCollectionEditor editor, PropertyEditorControlContext context, bool toolStripLabelsEnabled)
		{
			m_editor = editor;
			m_context = context;
			m_parentPropertyGridView = context.EditingControlOwner as PropertyGridView;
			IContextRegistry contextRegistry = m_context.ContextRegistry;
			if (contextRegistry != null)
			{
				contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
				ObservableContext = contextRegistry.GetActiveContext<IObservableContext>();
				ValidationContext = contextRegistry.GetActiveContext<IValidationContext>();
				TransactionContext = contextRegistry.GetActiveContext<ITransactionContext>();
			}
			else if (context.TransactionContext != null)
			{
				ObservableContext = context.TransactionContext.As<IObservableContext>();
				ValidationContext = context.TransactionContext.As<IValidationContext>();
				TransactionContext = context.TransactionContext;
			}
			m_toolStrip = new ToolStrip
			{
				Dock = DockStyle.Top
			};
			m_addButton = new ToolStripButton
			{
				Text = "Add".Localize(),
				Image = AddImage,
				Enabled = false,
				DisplayStyle = ToolStripItemDisplayStyle.Image
			};
			m_addButton.Click += addButton_Click;
			m_toolStrip.Items.Add(m_addButton);
			m_addSplitButton = new ToolStripSplitButton
			{
				Image = AddImage,
				Visible = false
			};
			m_addSplitButton.ButtonClick += addButton_Click;
			m_toolStrip.Items.Add(m_addSplitButton);
			m_deleteButton = new ToolStripButton
			{
				Text = "Delete".Localize(),
				Image = RemoveImage,
				DisplayStyle = ToolStripItemDisplayStyle.Image
			};
			UpdateDeleteButton(countSelected: false);
			m_deleteButton.Click += deleteButton_Click;
			m_toolStrip.Items.Add(m_deleteButton);
			m_upButton = new ToolStripButton
			{
				Text = "Up".Localize("this is the name of a button that causes the selected item to be moved up in a list"),
				Image = UpImage,
				DisplayStyle = ToolStripItemDisplayStyle.Image
			};
			m_upButton.Click += upButton_Click;
			m_toolStrip.Items.Add(m_upButton);
			m_downButton = new ToolStripButton
			{
				Text = "Down".Localize("this is the name of a button that causes the selected item to be moved down in a list"),
				Image = DownImage,
				DisplayStyle = ToolStripItemDisplayStyle.Image
			};
			m_downButton.Click += downButton_Click;
			m_toolStrip.Items.Add(m_downButton);
			UpdateMoveButtons(countSelected: false);
			m_itemsCountLabel = new ToolStripStatusLabel
			{
				Text = "[0 items]",
				DisplayStyle = ToolStripItemDisplayStyle.Text
			};
			m_toolStrip.Items.Add(m_itemsCountLabel);
			base.Controls.Add(m_toolStrip);
			base.Height = m_toolStrip.Height;
			m_toolStrip.GripStyle = ToolStripGripStyle.Hidden;
			if (toolStripLabelsEnabled)
			{
				m_toolStrip.SizeChanged += toolStrip_SizeChanged;
			}
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
			m_addSplitButton.DisplayStyle = displayStyle;
			m_deleteButton.DisplayStyle = displayStyle;
			m_upButton.DisplayStyle = displayStyle;
			m_downButton.DisplayStyle = displayStyle;
		}

		private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
		{
			ObservableContext = m_context.ContextRegistry.GetActiveContext<IObservableContext>();
			ValidationContext = m_context.ContextRegistry.GetActiveContext<IValidationContext>();
			TransactionContext = m_context.ContextRegistry.GetActiveContext<ITransactionContext>();
			foreach (ItemControl value in m_itemControls.Values)
			{
				value.Clear();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ObservableContext = null;
				ValidationContext = null;
				foreach (ItemControl unusedItemControl in m_unusedItemControls)
				{
					unusedItemControl.Dispose();
				}
				m_unusedItemControls.Clear();
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

		public override void Refresh()
		{
			object lastSelectedObject = m_context.LastSelectedObject;
			if ((m_activeCollectionNode != lastSelectedObject && lastSelectedObject != null) || !m_inTransaction)
			{
				ProcessPendingChanges();
			}
		}

		private IEnumerable<object> GetItemsFromContext()
		{
			object value = m_context.Descriptor.GetValue(m_activeCollectionNode);
			if (value != null && value is string)
			{
				return new object[1] { value };
			}
			if (value is IEnumerable enumerable)
			{
				return enumerable.AsIEnumerable<object>();
			}
			if (value != null)
			{
				return new object[1] { value };
			}
			return EmptyEnumerable<object>.Instance;
		}

		private void observableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
		{
			if (GetItemsFromContext().Contains(e.Item))
			{
				OnItemInserted(e.Item);
			}
		}

		private void OnItemInserted(object item)
		{
			if (m_pendingItemsRemoved.Contains(item))
			{
				m_pendingItemsRemoved.Remove(item);
			}
			else
			{
				m_pendingItemsInserted.Add(item);
			}
			if (!m_inTransaction)
			{
				ProcessPendingChanges();
			}
		}

		private void observableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
		{
			if (m_itemControls.ContainsKey(e.Item))
			{
				OnItemRemoved(e.Item);
			}
		}

		private void OnItemRemoved(object item)
		{
			if (m_pendingItemsInserted.Contains(item))
			{
				m_pendingItemsInserted.Remove(item);
			}
			else
			{
				m_pendingItemsRemoved.Add(item);
			}
			if (!m_inTransaction)
			{
				ProcessPendingChanges();
			}
		}

		private void observableContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
		{
			if (m_itemControls.TryGetValue(e.Item, out var _))
			{
				OnItemChanged(e.Item);
			}
		}

		private void OnItemChanged(object item)
		{
			m_pendingItemsChanged.Add(item);
			if (!m_inTransaction)
			{
				ProcessPendingChanges();
			}
		}

		private void validationContext_Beginning(object sender, EventArgs e)
		{
			m_inTransaction = true;
		}

		private void validationContext_Cancelled(object sender, EventArgs e)
		{
			m_inTransaction = false;
			ClearPendingChanges();
		}

		private void validationContext_Ended(object sender, EventArgs e)
		{
			m_inTransaction = false;
			ProcessPendingChanges();
		}

		private void ProcessPendingChanges()
		{
			if (m_processingPendingChanges)
			{
				return;
			}
			try
			{
				m_processingPendingChanges = true;
				SuspendLayout();
				object lastSelectedObject = m_context.LastSelectedObject;
				if (m_activeCollectionNode != lastSelectedObject && lastSelectedObject != null)
				{
					ClearPendingChanges();
					m_activeCollectionNode = lastSelectedObject;
					IEnumerable itemsFromContext = GetItemsFromContext();
					int num = itemsFromContext.Cast<object>().Count();
					m_singletonMode = (m_editor.GetItemInsertersFunc == null || !m_editor.GetItemInsertersFunc(m_context).Any()) && m_editor.RemoveItemFunc == null && num == 1;
					m_indexColumnWidth = Math.Max(30, (num * 2).ToString().Length * 10);
					m_toolStrip.Visible = !m_singletonMode && m_editor.ShowCollectionToolbar;
					foreach (ItemControl value4 in m_itemControls.Values)
					{
						value4.Visible = false;
						value4.Clear();
						m_unusedItemControls.Add(value4);
					}
					m_itemControls.Clear();
					foreach (object item in itemsFromContext)
					{
						m_pendingItemsInserted.Add(item);
					}
					UpdateAddButton();
				}
				int num2 = int.MaxValue;
				foreach (object item2 in m_pendingItemsRemoved)
				{
					if (m_itemControls.TryGetValue(item2, out var value))
					{
						if (value.Index < num2)
						{
							num2 = value.Index;
						}
						value.Visible = false;
						UnsubscribeItemEvents(value);
						m_itemControls.Remove(item2);
						value.Clear();
						m_unusedItemControls.Add(value);
					}
				}
				if (num2 != int.MaxValue)
				{
					int num3 = m_itemControls.Count - 1;
					foreach (ItemControl item3 in m_itemControls.Values.OrderBy((ItemControl itemControl2) => itemControl2.Index))
					{
						if (item3.Index > num2 || num3 == 0)
						{
							item3.Selected = true;
							break;
						}
						num3--;
					}
				}
				foreach (object item4 in m_pendingItemsChanged)
				{
					if (m_itemControls.TryGetValue(item4, out var value2))
					{
						value2.Refresh();
					}
				}
				string text = m_parentPropertyGridView?.FilterPattern;
				ITransactionContext transactionContext = m_context?.As<ITransactionContext>();
				List<Control> list = new List<Control>();
				if (!string.IsNullOrEmpty(text))
				{
					foreach (ItemControl value5 in m_itemControls.Values)
					{
						if (value5.Controls.Count == 0)
						{
							continue;
						}
						PropertyGridView propertyGridView = value5.Controls[0] as PropertyGridView;
						foreach (PropertyDescriptor propertyDescriptor in propertyGridView.EditingContext.PropertyDescriptors)
						{
							PropertyEditorControlContext propertyEditorControlContext = new PropertyEditorControlContext(propertyGridView, propertyDescriptor, transactionContext, m_context.ContextRegistry);
							string propertyText = propertyEditorControlContext.GetPropertyText();
							if (!propertyText.ToLower().Contains(text.ToLower()) && text.Length > 0)
							{
								list.Add(value5);
							}
						}
					}
				}
				int num4 = 0;
				int num5 = 0;
				int num6 = ((!m_singletonMode) ? m_toolStrip.Height : 0);
				foreach (object item5 in GetItemsFromContext())
				{
					if (m_itemControls.TryGetValue(item5, out var value3))
					{
						value3.Index = num4;
						num4++;
						if (list.Contains(value3))
						{
							value3.Visible = false;
							continue;
						}
						value3.VisibleIndex = num5;
						num5++;
						value3.Top = num6;
						num6 += value3.Height;
					}
				}
				foreach (object item6 in m_pendingItemsInserted)
				{
					ItemControl itemControl;
					if (m_unusedItemControls.Count > 0)
					{
						itemControl = m_unusedItemControls[m_unusedItemControls.Count - 1];
						m_unusedItemControls.RemoveAt(m_unusedItemControls.Count - 1);
						itemControl.Init(m_itemControls.Count, item6, m_singletonMode, m_indexColumnWidth, TransactionContext);
						itemControl.Visible = true;
					}
					else
					{
						itemControl = new ItemControl(m_itemControls.Count, item6, m_singletonMode, m_indexColumnWidth, TransactionContext, m_parentPropertyGridView, m_editor.ShowItemLabels, m_editor.ItemLabelsType);
						base.Controls.Add(itemControl);
						itemControl.MouseUp += itemControl_MouseUp;
					}
					itemControl.Width = m_toolStrip.Width;
					itemControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
					itemControl.Top = num6;
					if (m_singletonMode)
					{
						itemControl.Dock = DockStyle.Fill;
					}
					num6 += itemControl.Height;
					m_itemControls.Add(item6, itemControl);
					SubscribeItemEvents(itemControl);
					itemControl.Selected = false;
				}
				m_toolStrip.ForeColor = Color.Black;
				base.Height = num6;
				ClearPendingChanges();
				UpdateDeleteButton(countSelected: true);
				UpdateMoveButtons(countSelected: true);
				UpdateItemsCountLabel();
			}
			catch (Win32Exception ex)
			{
				foreach (ItemControl value6 in m_itemControls.Values)
				{
					if (base.Controls.Contains(value6))
					{
						base.Controls.Remove(value6);
					}
					value6.Clear();
					value6.Dispose();
				}
				m_itemControls.Clear();
				foreach (ItemControl unusedItemControl in m_unusedItemControls)
				{
					unusedItemControl.Dispose();
				}
				m_unusedItemControls.Clear();
				ClearPendingChanges();
				MessageBox.Show("Failed to create item controls, probably because there were not enough Window handles available. Consider using a different editor for collections of this size and nature.\r\n\r\n" + ex.GetType().ToString() + Environment.NewLine + ex.Message, "Failed to create item controls", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			finally
			{
				ResumeLayout();
				m_processingPendingChanges = false;
			}
		}

		private void itemControl_MouseUp(object sender, MouseEventArgs e)
		{
			Point p = ((Control)sender).PointToScreen(e.Location);
			Point point = PointToClient(p);
			MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta);
			OnMouseUp(e2);
		}

		private void ClearPendingChanges()
		{
			if (m_pendingItemsInserted.Count > 0)
			{
				m_pendingItemsInserted.Clear();
			}
			m_pendingItemsRemoved.Clear();
			m_pendingItemsChanged.Clear();
		}

		private void UpdateAddButton()
		{
			if (m_editor.GetItemInsertersFunc == null)
			{
				return;
			}
			foreach (ToolStripItem dropDownItem in m_addSplitButton.DropDownItems)
			{
				dropDownItem.Click -= addButton_Click;
			}
			m_addSplitButton.DropDownItems.Clear();
			m_addSplitButton.Tag = null;
			m_addButton.Tag = null;
			IEnumerable<ItemInserter> enumerable = m_editor.GetItemInsertersFunc(m_context);
			switch (enumerable.Count())
			{
			case 0:
				m_addSplitButton.Visible = false;
				m_addButton.Visible = true;
				m_addButton.Enabled = false;
				m_addButton.Text = "Add".Localize();
				m_addButton.ToolTipText = "Always disabled".Localize();
				return;
			case 1:
			{
				m_addSplitButton.Visible = false;
				m_addButton.Visible = true;
				ItemInserter itemInserter = enumerable.First();
				m_addButton.Tag = itemInserter;
				m_addButton.Enabled = true;
				m_addButton.Text = "Add".Localize();
				m_addButton.ToolTipText = string.Format("Add {0}".Localize(), itemInserter.ItemTypeName);
				return;
			}
			}
			m_addButton.Visible = false;
			m_addSplitButton.Visible = true;
			m_addSplitButton.Enabled = true;
			m_addSplitButton.ToolTipText = "Choose child type to add".Localize("Could be phrased, 'Choose the type of child to add to this list of child objects'");
			foreach (ItemInserter item in enumerable)
			{
				ToolStripItem toolStripItem2 = new ToolStripButton(item.ItemTypeName, item.Image);
				toolStripItem2.Width = TextRenderer.MeasureText(item.ItemTypeName, toolStripItem2.Font).Width;
				toolStripItem2.ToolTipText = string.Format("Add {0}".Localize(), item.ItemTypeName);
				toolStripItem2.Tag = item;
				toolStripItem2.Click += addButton_Click;
				m_addSplitButton.DropDownItems.Add(toolStripItem2);
			}
			SetDefaultInserter(enumerable.First());
		}

		private void UpdateDeleteButton(bool countSelected)
		{
			if (m_editor.RemoveItemFunc == null)
			{
				m_deleteButton.Enabled = false;
				m_deleteButton.ToolTipText = "Always disabled".Localize();
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
				m_deleteButton.ToolTipText = "Disabled because no items are selected".Localize();
			}
			else
			{
				m_deleteButton.Enabled = true;
				m_deleteButton.ToolTipText = string.Format("Delete {0} selected items".Localize(), num);
			}
		}

		private void UpdateMoveButtons(bool countSelected)
		{
			if (m_editor.MoveItemFunc == null)
			{
				m_upButton.Enabled = false;
				m_upButton.ToolTipText = "Always disabled".Localize();
				m_downButton.Enabled = false;
				m_downButton.ToolTipText = "Always disabled".Localize();
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
				m_upButton.Enabled = false;
				m_upButton.ToolTipText = "Disabled because no items are selected".Localize();
				m_downButton.Enabled = false;
				m_downButton.ToolTipText = "Disabled because no items are selected".Localize();
				return;
			}
			if (num2 > 0)
			{
				m_upButton.Enabled = true;
				m_upButton.ToolTipText = string.Format("Move {0} selected items up".Localize(), num);
			}
			else
			{
				m_upButton.Enabled = false;
				m_upButton.ToolTipText = string.Format("Can't move up because first item is selected".Localize(), num);
			}
			if (num3 < m_itemControls.Count - 1)
			{
				m_downButton.Enabled = true;
				m_downButton.ToolTipText = string.Format("Move {0} selected items down".Localize(), num);
			}
			else
			{
				m_downButton.Enabled = false;
				m_downButton.ToolTipText = string.Format("Can't move down because last item is selected".Localize(), num);
			}
		}

		private void UpdateItemsCountLabel()
		{
			int num = ((m_itemControls != null) ? m_itemControls.Values.Count : 0);
			m_itemsCountLabel.Text = string.Format("[{0} items]".Localize("a number of items"), num);
			m_itemsCountLabel.Visible = true;
		}

		private void SetDefaultInserter(ItemInserter inserter)
		{
			m_addSplitButton.Tag = inserter;
			m_addSplitButton.Enabled = true;
			string toolTipText = string.Format("Add {0}".Localize(), inserter.ItemTypeName);
			m_addSplitButton.Text = toolTipText;
			m_addSplitButton.ToolTipText = toolTipText;
		}

		private void SubscribeItemEvents(ItemControl itemControl)
		{
			itemControl.SizeChanged += itemControl_SizeChanged;
		}

		private void UnsubscribeItemEvents(ItemControl itemControl)
		{
			itemControl.SizeChanged -= itemControl_SizeChanged;
		}

		private void itemControl_SizeChanged(object sender, EventArgs e)
		{
			if (m_processingPendingChanges || !base.Visible)
			{
				return;
			}
			int num = (m_toolStrip.Visible ? m_toolStrip.Height : 0);
			List<ItemControl> list = new List<ItemControl>(m_itemControls.Values);
			list.Sort((ItemControl a, ItemControl b) => a.Index.CompareTo(b.Index));
			bool visible = base.Visible;
			foreach (ItemControl item in list)
			{
				item.Top = num;
				num += item.Height;
			}
			base.Height = num;
		}

		private void addButton_Click(object sender, EventArgs e)
		{
			if (!(sender is ToolStripItem toolStripItem))
			{
				return;
			}
			ItemInserter inserter = toolStripItem.Tag as ItemInserter;
			if (inserter != null)
			{
				m_context.TransactionContext.DoTransaction(delegate
				{
					object obj = inserter.InsertItemFunc();
				}, "Insert child".Localize());
				if (m_addSplitButton.Visible && m_addSplitButton.DropDownItems.Count > 1)
				{
					SetDefaultInserter(inserter);
				}
			}
		}

		private void deleteButton_Click(object sender, EventArgs e)
		{
			DeleteSelectedItems();
		}

		private void upButton_Click(object sender, EventArgs e)
		{
			MoveSelectedItems(-1);
		}

		private void downButton_Click(object sender, EventArgs e)
		{
			MoveSelectedItems(1);
		}

		private void DeleteSelectedItems()
		{
			if (m_editor.RemoveItemFunc == null)
			{
				return;
			}
			List<object> deleteItems = new List<object>();
			foreach (KeyValuePair<object, ItemControl> itemControl in m_itemControls)
			{
				object key = itemControl.Key;
				ItemControl value = itemControl.Value;
				if (value != null && value.Selected)
				{
					value.Selected = false;
					deleteItems.Add(key);
				}
			}
			TransactionContext.DoTransaction(delegate
			{
				foreach (object item in deleteItems)
				{
					if (item != null)
					{
						m_editor.RemoveItemFunc(m_context, item);
					}
				}
			}, "Remove children".Localize());
		}

		private void MoveSelectedItems(int direction)
		{
			if (m_editor.MoveItemFunc == null || direction == 0)
			{
				return;
			}
			List<KeyValuePair<object, ItemControl>> movePairs = new List<KeyValuePair<object, ItemControl>>();
			List<ItemControl> list = new List<ItemControl>();
			foreach (KeyValuePair<object, ItemControl> itemControl in m_itemControls)
			{
				if (itemControl.Value != null && itemControl.Value.Selected)
				{
					movePairs.Add(itemControl);
				}
				else
				{
					list.Add(itemControl.Value);
				}
			}
			if (direction < 0)
			{
				movePairs.Sort((KeyValuePair<object, ItemControl> a, KeyValuePair<object, ItemControl> b) => a.Value.Index.CompareTo(b.Value.Index));
			}
			else
			{
				movePairs.Sort((KeyValuePair<object, ItemControl> a, KeyValuePair<object, ItemControl> b) => -a.Value.Index.CompareTo(b.Value.Index));
			}
			TransactionContext.DoTransaction(delegate
			{
				foreach (KeyValuePair<object, ItemControl> item in movePairs)
				{
					m_editor.MoveItemFunc(m_context, item.Key, direction);
					OnItemChanged(item.Key);
				}
			}, "Move children".Localize());
			ProcessPendingChanges();
			foreach (KeyValuePair<object, ItemControl> item2 in movePairs)
			{
				m_itemControls[item2.Key].Selected = true;
			}
			foreach (ItemControl item3 in list)
			{
				item3.Selected = false;
			}
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
	}

	public static Image AddImage { get; set; } = ResourceUtil.GetImage16(Resources.AddImage);

	public static Image RemoveImage { get; set; } = ResourceUtil.GetImage16(Resources.RemoveImage);

	public static Image UpImage { get; set; } = ResourceUtil.GetImage16(Resources.ArrowUpImage);

	public static Image DownImage { get; set; } = ResourceUtil.GetImage16(Resources.ArrowDownImage);

	public bool ShowCollectionToolbar { get; set; }

	public bool ShowItemLabels { get; set; }

	public ItemLabelType ItemLabelsType { get; set; }

	public string[] Parameters { get; private set; }

	public Func<PropertyEditorControlContext, IEnumerable<ItemInserter>> GetItemInsertersFunc { get; set; }

	public Action<PropertyEditorControlContext, object> RemoveItemFunc { get; set; }

	public Action<PropertyEditorControlContext, object, int> MoveItemFunc { get; set; }

	public EmbeddedCollectionEditor()
	{
		ShowCollectionToolbar = true;
		ShowItemLabels = true;
		ItemLabelsType = ItemLabelType.kIndex;
	}

	public void Initialize(string[] parameters)
	{
		Parameters = parameters;
	}

	public virtual Control GetEditingControl(PropertyEditorControlContext context)
	{
		if (context.LastSelectedObject == null)
		{
			return null;
		}
		bool toolStripLabelsEnabled = Parameters != null && Parameters.Length > 2 && bool.Parse(Parameters[2]);
		return new CollectionControl(this, context, toolStripLabelsEnabled);
	}

	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return new SizeF(0f, 0f);
	}
}

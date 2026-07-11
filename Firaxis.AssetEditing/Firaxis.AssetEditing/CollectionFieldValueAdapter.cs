using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class CollectionFieldValueAdapter : FieldValueAdapter, ICollectionFieldValueAdapter, IFieldContainerAdapter, ISizableFieldContainerAdapter, IPropertyEditingContext, ISelectionContext
{
	private class NestedCollectionEditor : IPropertyEditor
	{
		private class CollectionToolControl : Control, ICacheablePropertyControl
		{
			private readonly ITransactionContext TransactionContext;

			private readonly ISelectionContext SelectionContext;

			private readonly PropertyEditorControlContext m_context;

			private Func<IFieldValueAdapter> m_addItemFunctor;

			private Action m_deleteItemFunctor;

			private Func<int> m_countItemFunctor;

			private Button m_addItem;

			private Button m_deleteItem;

			private Label m_itemCount;

			public bool Cacheable => true;

			public CollectionToolControl(PropertyEditorControlContext context, Func<IFieldValueAdapter> addItem, ISelectionContext selectionContext, Action delItem, Func<int> cntItems)
			{
				m_context = context;
				TransactionContext = m_context.TransactionContext.As<ITransactionContext>();
				SelectionContext = selectionContext;
				m_addItemFunctor = addItem;
				m_deleteItemFunctor = delItem;
				m_countItemFunctor = cntItems;
				Dock = DockStyle.None;
				base.Height = 24;
				Button button = new Button();
				button.Left = 1;
				button.FlatAppearance.BorderSize = 0;
				button.FlatStyle = FlatStyle.Flat;
				button.Size = new Size(22, 22);
				button.Image = ResourceUtil.GetImage16(Sce.Atf.Resources.AddImage);
				button.ImageAlign = ContentAlignment.MiddleCenter;
				button.Enabled = m_addItemFunctor != null;
				m_addItem = button;
				Button button2 = new Button();
				button2.Left = 24;
				button2.FlatStyle = FlatStyle.Flat;
				button2.FlatAppearance.BorderSize = 0;
				button2.Size = new Size(22, 22);
				button2.Image = ResourceUtil.GetImage16(Sce.Atf.Resources.RemoveImage);
				button2.ImageAlign = ContentAlignment.MiddleCenter;
				button2.Enabled = m_deleteItemFunctor != null && SelectionContext != null && SelectionContext.SelectionCount > 0;
				m_deleteItem = button2;
				m_itemCount = new Label
				{
					Dock = DockStyle.Right,
					FlatStyle = FlatStyle.Flat,
					Size = new Size(96, 22),
					TextAlign = ContentAlignment.MiddleRight,
					Text = $"{m_countItemFunctor()} Items"
				};
				m_addItem.Click += AddItem_Click;
				m_deleteItem.Click += DeleteItem_Click;
				SelectionContext.SelectionChanged += SelectionContext_SelectionChanged;
				SuspendLayout();
				base.Controls.Add(m_addItem);
				base.Controls.Add(m_deleteItem);
				base.Controls.Add(m_itemCount);
				ResumeLayout();
			}

			private void SelectionContext_SelectionChanged(object sender, EventArgs e)
			{
				m_deleteItem.Enabled = SelectionContext.SelectionCount > 0;
			}

			private void AddItem_Click(object sender, EventArgs e)
			{
				TransactionContext.DoTransaction(delegate
				{
					m_addItemFunctor();
				}, "Add Item".Localize());
				m_itemCount.Text = $"{m_countItemFunctor()} Items";
			}

			private void DeleteItem_Click(object sender, EventArgs e)
			{
				TransactionContext.DoTransaction(delegate
				{
					m_deleteItemFunctor();
				}, "Delete Item".Localize());
				m_itemCount.Text = $"{m_countItemFunctor()} Items";
			}
		}

		private Func<IFieldValueAdapter> AddItemFunctor;

		private ISelectionContext SelectionContext;

		private Action DeleteItemFunctor;

		private Func<int> ItemCountFunctor;

		public NestedCollectionEditor(Func<IFieldValueAdapter> addItem, ISelectionContext selectionContext, Action delItem, Func<int> itemCnt)
		{
			AddItemFunctor = addItem;
			SelectionContext = selectionContext;
			DeleteItemFunctor = delItem;
			ItemCountFunctor = itemCnt;
		}

		public SizeF GetDesiredSize(Graphics g, Font f)
		{
			return new SizeF(0f, 24f);
		}

		public Control GetEditingControl(PropertyEditorControlContext context)
		{
			return new CollectionToolControl(context, AddItemFunctor, SelectionContext, DeleteItemFunctor, ItemCountFunctor);
		}
	}

	private System.ComponentModel.PropertyDescriptor m_userSizableDescriptor;

	private ISelectionContext m_selectionContext = new SelectionContext();

	public ICollectionParameter CollectionParameter => base.Parameter as ICollectionParameter;

	public ICollectionValue CollectionValue => base.Value as ICollectionValue;

	public override object ValueDataAsObject
	{
		get
		{
			return Values.Select((IFieldValueAdapter val) => val.ValueDataAsObject).ToArray();
		}
		set
		{
			object[] array = (object[])value;
			while (array.Length > Values.Count)
			{
				AddValue(Values.Count);
			}
			while (array.Length < Values.Count)
			{
				RemoveValue(Values.Count - 1);
			}
			BugSubmitter.SilentAssert(array.Length != Values.Count, "CollectionFieldValueAdapter \"{0}\" attempted to set {1} values but Values.Count was {2} after the sizing operation @summary CollectionFieldValueAdapter.ValueDataAsObject has mis-matched values and Values! @assign bwhitman", Name, array.Length, Values.Count);
			int i = 0;
			Math.Min(array.Length, Values.Count);
			for (; i < array.Length; i++)
			{
				Values[i].ValueDataAsObject = array[i];
			}
		}
	}

	public IList<IFieldValueAdapter> Values { get; private set; }

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => ((IPropertyEditingContext)this).PropertyDescriptors;

	IEnumerable<object> IPropertyEditingContext.Items
	{
		get
		{
			if (!CollectionParameter.FixedSize)
			{
				yield return this;
			}
			if (IsArtDefElementOwned())
			{
				yield break;
			}
			foreach (IFieldValueAdapter field in Fields)
			{
				yield return field;
			}
		}
	}

	IEnumerable<System.ComponentModel.PropertyDescriptor> IPropertyEditingContext.PropertyDescriptors
	{
		get
		{
			if (!CollectionParameter.FixedSize)
			{
				yield return m_userSizableDescriptor;
			}
			if (IsArtDefElementOwned())
			{
				yield break;
			}
			foreach (IFieldValueAdapter field in Fields)
			{
				foreach (FieldPropertyDescriptorBase item in field.PropertyDescriptors.OfType<FieldPropertyDescriptorBase>())
				{
					yield return CreateProxyPropertyDescriptorIfNeeded(item, Name);
				}
			}
		}
	}

	public IList<IFieldValueAdapter> Fields => Values;

	public IEnumerable<object> Selection
	{
		get
		{
			return m_selectionContext.Selection;
		}
		set
		{
			foreach (ISelectionContext item in m_selectionContext.Selection.Except(value).OfType<ISelectionContext>())
			{
				item.Clear();
			}
			m_selectionContext.Selection = value;
		}
	}

	public object LastSelected => m_selectionContext.LastSelected;

	public int SelectionCount => m_selectionContext.SelectionCount;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	private object CreateCollectionEditor()
	{
		if (IsArtDefElementOwned())
		{
			return new EmbeddedCollectionEditor
			{
				ShowItemLabels = true,
				ItemLabelsType = (CollectionParameter.HasNamedEntries ? EmbeddedCollectionEditor.ItemLabelType.kName : EmbeddedCollectionEditor.ItemLabelType.kIndex),
				ShowCollectionToolbar = !CollectionParameter.FixedSize,
				GetItemInsertersFunc = (PropertyEditorControlContext _003Cp0_003E) => new EmbeddedCollectionEditor.ItemInserter[1]
				{
					new EmbeddedCollectionEditor.ItemInserter(insertItemFunc: () => AddValue(-1), itemTypeName: FieldValueHelper.GetFieldDomNodeType(CollectionParameter.EntryValueType).Name)
				},
				RemoveItemFunc = delegate(PropertyEditorControlContext context, object item)
				{
					IFieldValueAdapter item2 = item.Cast<DomNode>().As<IFieldValueAdapter>();
					int num = Values.IndexOf(item2);
					if (num >= 0 && num < Values.Count)
					{
						RemoveValue(num);
					}
				}
			};
		}
		return new NestedCollectionEditor(delegate
		{
			IFieldValueAdapter fieldValueAdapter = AddValue(-1);
			fieldValueAdapter.AssignDefaultValue();
			return fieldValueAdapter;
		}, this, delegate
		{
			foreach (object item3 in Selection)
			{
				int index = Fields.IndexOf(item3);
				RemoveValue(index);
			}
		}, () => Fields.Count);
	}

	public override void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		base.InitializeEditor(readOnlyFunctor);
		if (!CollectionParameter.FixedSize)
		{
			string name = BindDynamicValueOrDefault(base.Parameter?.Name, "Value".Localize());
			ChildInfo valueChild = FieldSchema.CollectionFieldValueType.ValueChild;
			string category = BindDynamicValueOrDefault(base.Parameter?.Category, "Value".Localize());
			m_userSizableDescriptor = CreateProxyPropertyDescriptorIfNeeded(new ChildFieldPropertyDescriptor(name, valueChild, category, BindDynamicValueOrDefault(base.Parameter?.Category, "Values in this collection".Localize()), isReadOnly: false, CreateCollectionEditor(), null), Name, null);
		}
	}

	public IFieldValueAdapter AddNamedValue(string name, int index)
	{
		BugSubmitter.Assert(CollectionParameter.HasNamedEntries, "Named values are only valid in collection values that explicitly support named values!");
		return DomAddValueImpl(name, index);
	}

	public IFieldValueAdapter AddValue(int index)
	{
		string name = ArtDefContext.GenerateUniqueName(CollectionValue.Items, Name);
		return DomAddValueImpl(name, index);
	}

	public void RemoveValue(int index)
	{
		DomRemoveValueImpl(index);
	}

	public override void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		ICollectionParameter param = valParam as ICollectionParameter;
		base.Value = FieldValueHelper.NativeAddElementCollectionField(valSet, param);
	}

	private void DomRemoveValueImpl(int index)
	{
		IFieldContainerAdapter fieldContainerAdapter = Values[index].As<IFieldContainerAdapter>();
		if (fieldContainerAdapter != null)
		{
			for (int num = fieldContainerAdapter.Fields.Count - 1; num >= 0; num--)
			{
				fieldContainerAdapter.Fields.RemoveAt(num);
			}
		}
		Values.RemoveAt(index);
	}

	private FieldValueAdapter DomAddValueImpl(string name, int index)
	{
		FieldValueAdapter fieldValueAdapter = FieldValueHelper.CreateField(CollectionParameter.EntryParameter);
		if (fieldValueAdapter == null)
		{
			BugSubmitter.SilentReport("Tried to create a parameter \"" + CollectionParameter.EntryParameter.Name + "\" of type " + CollectionParameter.EntryParameter.ParameterType.ToString() + " that's not supported by this collection value \"" + Name + "\"");
			return null;
		}
		fieldValueAdapter.Initialize(CollectionParameter.EntryParameter);
		fieldValueAdapter.Name = name;
		if (index == -1)
		{
			Values.Add(fieldValueAdapter);
		}
		else
		{
			Values.Insert(index, fieldValueAdapter);
		}
		return fieldValueAdapter;
	}

	public override void UpdateDomFromNative(IValue val)
	{
		base.UpdateDomFromNative(val);
		UnregisterForDomNotifications();
		if (!CollectionParameter.HasNamedEntries)
		{
			UpdateDomItems();
		}
		else
		{
			RebuildDomItems();
		}
		RegisterForDomNotifications();
	}

	public override void UpdateNativeFromDom()
	{
		UpdateNativeItems();
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
		while (Values.Count > 0)
		{
			Values.RemoveAt(0);
		}
		ICollectionValue collectionValue = (ICollectionValue)val;
		if (CollectionParameter.HasNamedEntries)
		{
			foreach (IValue item in collectionValue.Items)
			{
				AddNamedValue(item.ParameterName, -1).CopyValue(item);
			}
			return;
		}
		foreach (IValue item2 in collectionValue.Items)
		{
			AddValue(-1).CopyValue(item2);
		}
	}

	protected override void OnNodeSet()
	{
		m_selectionContext.SelectionChanging += SelectionContext_SelectionChanging;
		m_selectionContext.SelectionChanged += SelectionContext_SelectionChanged;
		Values = new DomNodeListAdapter<IFieldValueAdapter>(base.DomNode, base.DomNode.Type.GetChildInfo("Value"));
		RegisterForDomNotifications();
		base.OnNodeSet();
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (!e.Parent.Equals(base.DomNode))
		{
			return;
		}
		FieldValueAdapter fieldValueAdapter = e.Child.As<FieldValueAdapter>();
		if (fieldValueAdapter != null)
		{
			NativeAddValue(fieldValueAdapter);
			IFieldContainerAdapter fieldContainerAdapter = fieldValueAdapter.As<IFieldContainerAdapter>();
			if (fieldContainerAdapter != null)
			{
				fieldContainerAdapter.ItemInserted += FieldContainerAdapter_ItemInserted;
				fieldContainerAdapter.ItemRemoved += FieldContainerAdapter_ItemRemoved;
			}
		}
		OnItemInserted(e.Index, e.Child);
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.Parent.Equals(base.DomNode))
		{
			IFieldContainerAdapter fieldContainerAdapter = e.Child.As<IFieldContainerAdapter>();
			if (fieldContainerAdapter != null)
			{
				fieldContainerAdapter.ItemInserted -= FieldContainerAdapter_ItemInserted;
				fieldContainerAdapter.ItemRemoved -= FieldContainerAdapter_ItemRemoved;
			}
			NativeRemoveValue(e.Index);
			OnItemRemoved(e.Index, e.Child);
		}
	}

	private void FieldContainerAdapter_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		OnItemInserted(e.Index, e.Item);
	}

	private void FieldContainerAdapter_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		OnItemRemoved(e.Index, e.Item);
	}

	private void NativeAddValue(FieldValueAdapter adapter)
	{
		IValue val = FieldValueHelper.NativeAddCollectionValue(CollectionValue, CollectionParameter, adapter.Name);
		adapter.UpdateDomFromNative(val);
	}

	private void NativeRemoveValue(int index)
	{
		int num = CollectionValue.Items.Count();
		BugSubmitter.SilentAssert(index >= 0, string.Format("Attempted to remove collection item at index {0} from collection \"{1}\"! @assign bwhitman @summary CollectionFieldEditor remove collection item index invalid", index, Name ?? "null"));
		BugSubmitter.SilentAssert(index < num, string.Format("Attempted to remove collection item at index {0} from collection \"{1}\" but there were only {2} items in the collection! @assign bwhitman @summary CollectionFieldEditor remove collection item index invalid", index, Name ?? "null", num));
		CollectionValue.Remove(CollectionValue.Items.ElementAt(index));
	}

	private void RegisterForDomNotifications()
	{
		UnregisterForDomNotifications();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterForDomNotifications()
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
	}

	private void UpdateDomItems()
	{
		IList<IFieldValueAdapter> list = new List<IFieldValueAdapter>();
		foreach (IValue valueEntry in CollectionValue.Items)
		{
			IFieldValueAdapter fieldValueAdapter = Values.FirstOrDefault((IFieldValueAdapter fa) => fa.Name == valueEntry.ParameterName);
			if (fieldValueAdapter == null)
			{
				fieldValueAdapter = FieldValueHelper.CreateField(CollectionParameter.EntryParameter);
				PlatformAssert.If(fieldValueAdapter == null, "Tried to create a parameter that's not supported by this parameter set.");
				Values.Add(fieldValueAdapter);
				((FieldValueAdapter)fieldValueAdapter).UpdateDomFromNative(valueEntry);
				IFieldContainerAdapter fieldContainerAdapter = fieldValueAdapter.As<IFieldContainerAdapter>();
				if (fieldContainerAdapter != null)
				{
					fieldContainerAdapter.ItemInserted += FieldContainerAdapter_ItemInserted;
					fieldContainerAdapter.ItemRemoved += FieldContainerAdapter_ItemRemoved;
				}
			}
			else
			{
				((FieldValueAdapter)fieldValueAdapter).UpdateDomFromNative(valueEntry);
			}
			list.Add(fieldValueAdapter);
		}
		foreach (var fldAdapter in Values.Except(list).ToArray())
		{
			IFieldContainerAdapter fieldContainerAdapter2 = fldAdapter.As<IFieldContainerAdapter>();
			if (fieldContainerAdapter2 != null)
			{
				fieldContainerAdapter2.ItemInserted -= FieldContainerAdapter_ItemInserted;
				fieldContainerAdapter2.ItemRemoved -= FieldContainerAdapter_ItemRemoved;
			}
			Values.Remove(fldAdapter);
		}
	}

	private void RebuildDomItems()
	{
		foreach (IFieldValueAdapter value in Values)
		{
			IFieldContainerAdapter fieldContainerAdapter = value.As<IFieldContainerAdapter>();
			if (fieldContainerAdapter != null)
			{
				fieldContainerAdapter.ItemInserted -= FieldContainerAdapter_ItemInserted;
				fieldContainerAdapter.ItemRemoved -= FieldContainerAdapter_ItemRemoved;
			}
		}
		Values.Clear();
		foreach (IValue item in CollectionValue.Items)
		{
			IFieldValueAdapter fieldValueAdapter = ((!CollectionParameter.HasNamedEntries) ? AddValue(-1) : AddNamedValue(item.ParameterName, -1));
			if (fieldValueAdapter != null)
			{
				((FieldValueAdapter)fieldValueAdapter).UpdateDomFromNative(item);
			}
		}
	}

	private void UpdateNativeItems()
	{
		foreach (IFieldValueAdapter value in Values)
		{
			value.UpdateNativeFromDom();
		}
	}

	public IEnumerable<T> GetSelection<T>() where T : class
	{
		return m_selectionContext.GetSelection<T>();
	}

	public T GetLastSelected<T>() where T : class
	{
		return m_selectionContext.GetLastSelected<T>();
	}

	public bool SelectionContains(object item)
	{
		return m_selectionContext.SelectionContains(item);
	}

	private void SelectionContext_SelectionChanged(object sender, EventArgs e)
	{
		this.SelectionChanged.Raise(sender, e);
	}

	private void SelectionContext_SelectionChanging(object sender, EventArgs e)
	{
		this.SelectionChanging.Raise(sender, e);
	}
}

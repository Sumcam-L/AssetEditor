using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TupleFieldValueAdapter : FieldValueAdapter, IFieldContainerAdapter, IPropertyEditingContext, ISelectionContext
{
	private IEnumerable<System.ComponentModel.PropertyDescriptor> m_descriptors;

	private ISelectionContext m_selectionContext = new SelectionContext();

	public ITupleParameter TupleParameter => base.Parameter as ITupleParameter;

	public ITupleValue TupleValue => base.Value as ITupleValue;

	public override object ValueDataAsObject
	{
		get
		{
			return Fields.Select((IFieldValueAdapter val) => val.ValueDataAsObject).ToArray();
		}
		set
		{
			object[] array = (object[])value;
			BugSubmitter.SilentAssert(array.Length != Fields.Count, "TupleFieldValueAdapter \"{0}\" attempted to set {1} values but Fields.Count was {2} after the sizing operation @summary TupleFieldValueAdapter.ValueDataAsObject has mis-matched values and Fields! @assign bwhitman", Name, array.Length, Fields.Count);
			int i = 0;
			Math.Min(array.Length, Fields.Count);
			for (; i < array.Length; i++)
			{
				Fields[i].ValueDataAsObject = array[i];
			}
		}
	}

	private IParameterSet ParameterSet { get; set; }

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => ((IPropertyEditingContext)this).PropertyDescriptors;

	IEnumerable<object> IPropertyEditingContext.Items => Fields;

	IEnumerable<System.ComponentModel.PropertyDescriptor> IPropertyEditingContext.PropertyDescriptors
	{
		get
		{
			foreach (IFieldValueAdapter field in Fields)
			{
				foreach (FieldPropertyDescriptorBase item in field.PropertyDescriptors.OfType<FieldPropertyDescriptorBase>())
				{
					yield return CreateProxyPropertyDescriptorIfNeeded(item, Name);
				}
			}
		}
	}

	public IList<IFieldValueAdapter> Fields { get; private set; }

	public IEnumerable<object> Selection
	{
		get
		{
			return m_selectionContext.Selection;
		}
		set
		{
			m_selectionContext.Selection = value;
		}
	}

	public object LastSelected => m_selectionContext.LastSelected;

	public int SelectionCount => m_selectionContext.SelectionCount;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	protected override System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
	{
		return ((IPropertyEditingContext)this).PropertyDescriptors.ToArray();
	}

	public override void AssignDefaultValue()
	{
		foreach (IParameter fldDef in CivTechRegistry.CivTechService.PrimaryProject.Config.Classes.FindByName(TupleParameter.ClassName).CookParameters.Items)
		{
			IFieldValueAdapter fieldValueAdapter = Fields.FirstOrDefault((IFieldValueAdapter fld) => fld.Name == fldDef.Name);
			if (fieldValueAdapter == null)
			{
				fieldValueAdapter = FieldValueHelper.CreateField(fldDef);
				Fields.Add(fieldValueAdapter);
			}
			fieldValueAdapter.AssignDefaultValue();
		}
	}

	public override void UpdateDomFromNative(IValue val)
	{
		base.UpdateDomFromNative(val);
		UnregisterForDomNotifications();
		RebuildDomItems();
		RegisterForDomNotifications();
	}

	public override void UpdateNativeFromDom()
	{
		UpdateNativeItems();
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
	}

	protected override void OnNodeSet()
	{
		Fields = new DomNodeListAdapter<IFieldValueAdapter>(base.DomNode, base.DomNode.Type.GetChildInfo("Value"));
		RegisterForDomNotifications();
		base.OnNodeSet();
	}

	private IFieldValueAdapter DomAddValue(IValue value)
	{
		IParameter parameter = CivTechRegistry.CivTechService.PrimaryProject.Config.Classes.FindByName(TupleParameter.ClassName)?.CookParameters.Items.FirstOrDefault((IParameter cp) => cp.Name == value.ParameterName);
		if (parameter == null)
		{
			return null;
		}
		FieldValueAdapter fieldValueAdapter = FieldValueHelper.CreateField(parameter);
		if (fieldValueAdapter == null)
		{
			return null;
		}
		fieldValueAdapter.Initialize(parameter);
		fieldValueAdapter.Name = value.ParameterName;
		fieldValueAdapter.Value = value;
		Fields.Add(fieldValueAdapter);
		return fieldValueAdapter;
	}

	private void RebuildDomItems()
	{
		foreach (IFieldValueAdapter field in Fields)
		{
			IFieldContainerAdapter fieldContainerAdapter = field.As<IFieldContainerAdapter>();
			if (fieldContainerAdapter != null)
			{
				fieldContainerAdapter.ItemInserted -= FieldContainerAdapter_ItemInserted;
				fieldContainerAdapter.ItemRemoved -= FieldContainerAdapter_ItemRemoved;
			}
		}
		Fields.Clear();
		foreach (IValue item in TupleValue.Elements.Items)
		{
			IFieldValueAdapter fieldValueAdapter = DomAddValue(item);
			if (fieldValueAdapter != null)
			{
				((FieldValueAdapter)fieldValueAdapter).UpdateDomFromNative(item);
				IFieldContainerAdapter fieldContainerAdapter2 = fieldValueAdapter.As<IFieldContainerAdapter>();
				if (fieldContainerAdapter2 != null)
				{
					fieldContainerAdapter2.ItemInserted += FieldContainerAdapter_ItemInserted;
					fieldContainerAdapter2.ItemRemoved += FieldContainerAdapter_ItemRemoved;
				}
			}
		}
	}

	private void UpdateNativeItems()
	{
		foreach (IFieldValueAdapter field in Fields)
		{
			field.UpdateNativeFromDom();
		}
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		OnItemInserted(e.Index, e.Child);
		if (e.Parent != base.DomNode)
		{
			return;
		}
		IFieldValueAdapter fieldValueAdapter = e.Child.As<IFieldValueAdapter>();
		if (fieldValueAdapter != null)
		{
			fieldValueAdapter.AddNativeField(TupleValue.Elements, fieldValueAdapter.Parameter);
			IFieldContainerAdapter fieldContainerAdapter = fieldValueAdapter.As<IFieldContainerAdapter>();
			if (fieldContainerAdapter != null)
			{
				fieldContainerAdapter.ItemInserted += FieldContainerAdapter_ItemInserted;
				fieldContainerAdapter.ItemRemoved += FieldContainerAdapter_ItemRemoved;
			}
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.Parent == base.DomNode)
		{
			IFieldValueAdapter fieldValueAdapter = e.Child.As<IFieldValueAdapter>();
			if (fieldValueAdapter != null)
			{
				IFieldContainerAdapter fieldContainerAdapter = fieldValueAdapter.As<IFieldContainerAdapter>();
				if (fieldContainerAdapter != null)
				{
					fieldContainerAdapter.ItemInserted -= FieldContainerAdapter_ItemInserted;
					fieldContainerAdapter.ItemRemoved -= FieldContainerAdapter_ItemRemoved;
				}
				TupleValue.Elements.Remove(fieldValueAdapter.Value);
			}
		}
		OnItemRemoved(e.Index, e.Child);
	}

	private void FieldContainerAdapter_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		OnItemInserted(e.Index, e.Item);
	}

	private void FieldContainerAdapter_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		OnItemRemoved(e.Index, e.Item);
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
}

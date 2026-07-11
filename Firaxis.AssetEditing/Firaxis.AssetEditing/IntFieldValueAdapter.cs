using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class IntFieldValueAdapter : FieldValueAdapter
{
	private IEnumerable<System.ComponentModel.PropertyDescriptor> m_descriptors;

	public override object DefaultDataAsObject => ((IIntValue)base.Parameter.DefaultValue).ParameterValue;

	public int ParameterValue
	{
		get
		{
			return GetAttribute<int>(FieldSchema.IntFieldValueType.ValueAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.IntFieldValueType.ValueAttribute, value);
		}
	}

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => m_descriptors;

	public override object ValueDataAsObject
	{
		get
		{
			return ParameterValue;
		}
		set
		{
			ParameterValue = (int)value;
		}
	}

	private IIntParameter IntParameter => base.Parameter as IIntParameter;

	private IIntValue IntValue => base.Value as IIntValue;

	public override void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		base.AddNativeField(valSet, valParam);
		IntValue.ParameterValue = ParameterValue;
	}

	public override void AssignDefaultValue()
	{
		ParameterValue = (IntParameter.DefaultValue as IIntValue).ParameterValue;
	}

	public override void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		base.InitializeEditor(readOnlyFunctor);
		if (base.Parameter is IIntParameter intParameter)
		{
			if (intParameter.Min < intParameter.Max)
			{
				m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1] { CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(BindDynamicValueOrDefault(base.Parameter.Name, "Integer".Localize()), FieldSchema.IntFieldValueType.ValueAttribute, BindDynamicValueOrDefault(base.Parameter.Category, "Value".Localize()), BindDynamicValueOrDefault(base.Parameter.Description, "Integer value description".Localize()), readOnlyFunctor, new BoundedIntEditor(intParameter.Min, intParameter.Max), new BoundedIntConverter()), Name) });
			}
			else
			{
				m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1] { CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(BindDynamicValueOrDefault(base.Parameter.Name, "Integer".Localize()), FieldSchema.IntFieldValueType.ValueAttribute, BindDynamicValueOrDefault(base.Parameter.Category, "Value".Localize()), BindDynamicValueOrDefault(base.Parameter.Description, "Integer value description".Localize()), isReadOnly: true), Name) });
			}
		}
		else
		{
			m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1] { CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(BindDynamicValueOrDefault(base.Parameter.Name, "Integer".Localize()), FieldSchema.IntFieldValueType.ValueAttribute, BindDynamicValueOrDefault(base.Parameter.Category, "Value".Localize()), BindDynamicValueOrDefault(base.Parameter.Description, "Integer value description".Localize()), readOnlyFunctor, new NumericEditor(typeof(int))), Name) });
		}
	}

	public override bool RequiresUpdate(IValue val)
	{
		bool num = base.RequiresUpdate(val);
		bool flag = false;
		if (!num && val is IIntValue intValue)
		{
			flag = intValue.ParameterValue == ParameterValue;
		}
		if (!num)
		{
			return !flag;
		}
		return true;
	}

	public override void UpdateDomFromNative(IValue val)
	{
		base.UpdateDomFromNative(val);
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		CopyValue(val);
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	public override void UpdateNativeFromDom()
	{
		IntValue.ParameterValue = ParameterValue;
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
		ParameterValue = ((IIntValue)val).ParameterValue;
	}

	protected override System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
	{
		return PropertyDescriptors.AsIEnumerable<System.ComponentModel.PropertyDescriptor>().ToArray();
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.OnNodeSet();
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == FieldSchema.IntFieldValueType.ValueAttribute)
		{
			IntValue.ParameterValue = (int)e.NewValue;
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class BoolFieldValueAdapter : FieldValueAdapter
{
	private IEnumerable<System.ComponentModel.PropertyDescriptor> m_descriptors;

	public override object DefaultDataAsObject => ((IBoolValue)base.Parameter.DefaultValue).ParameterValue;

	public bool ParameterValue
	{
		get
		{
			return GetAttribute<bool>(FieldSchema.BoolFieldValueType.ValueAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.BoolFieldValueType.ValueAttribute, value);
		}
	}

	public override object ValueDataAsObject
	{
		get
		{
			return ParameterValue;
		}
		set
		{
			ParameterValue = (bool)value;
		}
	}

	private IBoolParameter BoolParameter => base.Parameter as IBoolParameter;

	private IBoolValue BoolValue => base.Value as IBoolValue;

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => m_descriptors;

	public override void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		base.InitializeEditor(readOnlyFunctor);
		System.ComponentModel.PropertyDescriptor[] array = new System.ComponentModel.PropertyDescriptor[1];
		int num = 0;
		string name = BindDynamicValueOrDefault(base.Parameter?.Name, "Boolean".Localize());
		AttributeInfo valueAttribute = FieldSchema.BoolFieldValueType.ValueAttribute;
		string category = BindDynamicValueOrDefault(base.Parameter?.Category, "Value".Localize());
		array[num] = CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(name, valueAttribute, category, BindDynamicValueOrDefault(base.Parameter?.Description, "Boolean value description".Localize()), readOnlyFunctor, new BoolEditor()), Name);
		m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(array);
	}

	public override void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		base.AddNativeField(valSet, valParam);
		BoolValue.ParameterValue = ParameterValue;
	}

	public override void AssignDefaultValue()
	{
		ParameterValue = (BoolParameter.DefaultValue as IBoolValue).ParameterValue;
	}

	public override bool RequiresUpdate(IValue val)
	{
		bool num = base.RequiresUpdate(val);
		bool flag = false;
		if (!num && val is IBoolValue boolValue)
		{
			flag = boolValue.ParameterValue == ParameterValue;
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
		BoolValue.ParameterValue = ParameterValue;
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
		ParameterValue = ((IBoolValue)val).ParameterValue;
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.OnNodeSet();
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == FieldSchema.BoolFieldValueType.ValueAttribute)
		{
			BoolValue.ParameterValue = (bool)e.NewValue;
		}
	}
}

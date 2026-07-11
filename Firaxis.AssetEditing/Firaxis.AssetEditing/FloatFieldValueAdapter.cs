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

public class FloatFieldValueAdapter : FieldValueAdapter
{
	private IEnumerable<System.ComponentModel.PropertyDescriptor> m_descriptors;

	public override object DefaultDataAsObject => FloatParameter.Default;

	public float ParameterValue
	{
		get
		{
			return GetAttribute<float>(FieldSchema.FloatFieldValueType.ValueAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.FloatFieldValueType.ValueAttribute, value);
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
			ParameterValue = (float)value;
		}
	}

	private IFloatParameter FloatParameter => base.Parameter as IFloatParameter;

	private IFloatValue FloatValue => base.Value as IFloatValue;

	public override void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		base.AddNativeField(valSet, valParam);
		FloatValue.ParameterValue = ParameterValue;
	}

	public override void AssignDefaultValue()
	{
		ParameterValue = (FloatParameter.DefaultValue as IFloatValue).ParameterValue;
	}

	public override void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		base.InitializeEditor(readOnlyFunctor);
		if (FloatParameter != null)
		{
			if (FloatParameter.Min < FloatParameter.Max)
			{
				CreateBoundedFloatEditor(readOnlyFunctor);
			}
			else
			{
				CreateFloatEditor();
			}
			return;
		}
		System.ComponentModel.PropertyDescriptor[] array = new System.ComponentModel.PropertyDescriptor[1];
		int num = 0;
		string name = BindDynamicValueOrDefault(base.Parameter?.Name, "Float".Localize());
		AttributeInfo valueAttribute = FieldSchema.FloatFieldValueType.ValueAttribute;
		string category = BindDynamicValueOrDefault(base.Parameter?.Category, "Value".Localize());
		array[num] = CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(name, valueAttribute, category, BindDynamicValueOrDefault(base.Parameter?.Description, "Float value description".Localize()), readOnlyFunctor, new NumericEditor(typeof(float))), Name);
		m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(array);
	}

	public override bool RequiresUpdate(IValue val)
	{
		bool num = base.RequiresUpdate(val);
		bool flag = false;
		if (!num)
		{
			flag = (val as IFloatValue).ParameterValue == ParameterValue;
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
		FloatValue.ParameterValue = ParameterValue;
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
		ParameterValue = ((IFloatValue)val).ParameterValue;
	}

	protected override System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
	{
		return PropertyDescriptors.AsIEnumerable<System.ComponentModel.PropertyDescriptor>().ToArray();
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	private void CreateBoundedFloatEditor(Func<bool> readOnlyFunctor)
	{
		m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1] { CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(BindDynamicValueOrDefault(FloatParameter.Name, "Float".Localize()), FieldSchema.FloatFieldValueType.ValueAttribute, BindDynamicValueOrDefault(FloatParameter.Category, "Value".Localize()), BindDynamicValueOrDefault(FloatParameter.Description, "Floating point value".Localize()), readOnlyFunctor, new BoundedFloatEditor(FloatParameter.Min, FloatParameter.Max), new BoundedFloatConverter()), Name) });
	}

	private void CreateFloatEditor()
	{
		m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1] { CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(BindDynamicValueOrDefault(FloatParameter.Name, "Float".Localize()), FieldSchema.FloatFieldValueType.ValueAttribute, BindDynamicValueOrDefault(FloatParameter.Category, "Value".Localize()), BindDynamicValueOrDefault(FloatParameter.Description, "Floating point value".Localize()), isReadOnly: true), Name) });
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == FieldSchema.FloatFieldValueType.ValueAttribute)
		{
			FloatValue.ParameterValue = (float)e.NewValue;
		}
	}
}

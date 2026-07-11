using System;
using System.Collections.Generic;
using System.ComponentModel;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class Coord3DFieldValueAdapter : FieldValueAdapter
{
	private IEnumerable<System.ComponentModel.PropertyDescriptor> m_descriptors;

	public override object DefaultDataAsObject => ((ICoord3DValue)base.Parameter.DefaultValue).ParameterValue;

	public float[] ParameterValue
	{
		get
		{
			return GetAttribute<float[]>(FieldSchema.Coord3DFieldValueType.ValueAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.Coord3DFieldValueType.ValueAttribute, value);
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
			ParameterValue = (float[])value;
		}
	}

	private ICoord3DParameter Coord3DParameter => base.Parameter as ICoord3DParameter;

	private ICoord3DValue Coord3DValue => base.Value as ICoord3DValue;

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => m_descriptors;

	public override void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		base.InitializeEditor(readOnlyFunctor);
		System.ComponentModel.PropertyDescriptor[] array = new System.ComponentModel.PropertyDescriptor[1];
		int num = 0;
		string name = BindDynamicValueOrDefault(base.Parameter?.Name, "Coord 3D".Localize());
		AttributeInfo valueAttribute = FieldSchema.Coord3DFieldValueType.ValueAttribute;
		string category = BindDynamicValueOrDefault(base.Parameter?.Category, "Value".Localize());
		array[num] = CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(name, valueAttribute, category, BindDynamicValueOrDefault(base.Parameter?.Description, "3D position for this value".Localize()), readOnlyFunctor, new NumericTupleEditor(typeof(float), new string[3] { "X", "Y", "Z" }), new FloatArrayConverter()), Name);
		m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(array);
	}

	public override void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		base.AddNativeField(valSet, valParam);
		Coord3DValue.ParameterValue.x = ParameterValue[0];
		Coord3DValue.ParameterValue.y = ParameterValue[1];
		Coord3DValue.ParameterValue.z = ParameterValue[2];
	}

	public override void AssignDefaultValue()
	{
		Point3F parameterValue = (Coord3DParameter.DefaultValue as ICoord3DValue).ParameterValue;
		ParameterValue = new float[3] { parameterValue.x, parameterValue.y, parameterValue.z };
	}

	public override bool RequiresUpdate(IValue val)
	{
		bool num = base.RequiresUpdate(val);
		bool flag = false;
		if (!num && val is ICoord3DValue coord3DValue && ParameterValue != null)
		{
			flag = coord3DValue.ParameterValue.x == ParameterValue[0] && coord3DValue.ParameterValue.y == ParameterValue[1] && coord3DValue.ParameterValue.z == ParameterValue[2];
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
		float[] parameterValue = ParameterValue;
		Coord3DValue.ParameterValue = new Point3F(parameterValue[0], parameterValue[1], parameterValue[2]);
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
		Point3F parameterValue = ((ICoord3DValue)val).ParameterValue;
		ParameterValue = new float[3] { parameterValue.x, parameterValue.y, parameterValue.z };
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.OnNodeSet();
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo != FieldSchema.FieldValueType.NameAttribute)
		{
			Coord3DValue.ParameterValue = new Point3F(((float[])e.NewValue)[0], ((float[])e.NewValue)[1], ((float[])e.NewValue)[2]);
		}
	}
}

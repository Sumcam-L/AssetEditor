using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class Coord2DFieldValueAdapter : FieldValueAdapter
{
	private IEnumerable<System.ComponentModel.PropertyDescriptor> m_descriptors;

	private ICoord2DValue Coord2DValue => base.Value as ICoord2DValue;

	private ICoord2DParameter Coord2DParameter => base.Parameter as ICoord2DParameter;

	public override object DefaultDataAsObject => ((ICoord2DValue)base.Parameter.DefaultValue).ParameterValue;

	public override object ValueDataAsObject
	{
		get
		{
			return ParameterValue;
		}
		set
		{
			Coord2DValue.ParameterValue = new PointF(((float[])value)[0], ((float[])value)[1]);
		}
	}

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => m_descriptors;

	public float[] ParameterValue
	{
		get
		{
			return GetAttribute<float[]>(FieldSchema.Coord2DFieldValueType.ValueAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.Coord2DFieldValueType.ValueAttribute, value);
		}
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.OnNodeSet();
	}

	public override void AssignDefaultValue()
	{
		PointF parameterValue = (Coord2DParameter.DefaultValue as ICoord2DValue).ParameterValue;
		ParameterValue = new float[2] { parameterValue.X, parameterValue.Y };
	}

	public void SetParameter(float X, float Y)
	{
		Coord2DValue.ParameterValue = new PointF(X, Y);
	}

	public override void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		base.InitializeEditor(readOnlyFunctor);
		System.ComponentModel.PropertyDescriptor[] array = new System.ComponentModel.PropertyDescriptor[1];
		int num = 0;
		string name = BindDynamicValueOrDefault(base.Parameter?.Name, "Coord 2D".Localize());
		AttributeInfo valueAttribute = FieldSchema.Coord2DFieldValueType.ValueAttribute;
		string category = BindDynamicValueOrDefault(base.Parameter?.Category, "Value".Localize());
		array[num] = CreateProxyPropertyDescriptorIfNeeded(new AttributeFieldPropertyDescriptor(name, valueAttribute, category, BindDynamicValueOrDefault(base.Parameter?.Description, "2D position for this value".Localize()), readOnlyFunctor, new NumericTupleEditor(typeof(float), new string[2] { "X", "Y" }), new FloatArrayConverter()), Name);
		m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(array);
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo != FieldSchema.FieldValueType.NameAttribute)
		{
			Coord2DValue.ParameterValue = new PointF(((float[])e.NewValue)[0], ((float[])e.NewValue)[1]);
		}
	}

	public override bool RequiresUpdate(IValue val)
	{
		bool num = base.RequiresUpdate(val);
		bool flag = false;
		if (!num && val is ICoord2DValue coord2DValue && ParameterValue != null)
		{
			flag = coord2DValue.ParameterValue.X == ParameterValue[0] && coord2DValue.ParameterValue.Y == ParameterValue[1];
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
		Coord2DValue.ParameterValue = new PointF(parameterValue[0], parameterValue[1]);
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
		PointF parameterValue = ((ICoord2DValue)val).ParameterValue;
		ParameterValue = new float[2] { parameterValue.X, parameterValue.Y };
	}

	public override void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		base.AddNativeField(valSet, valParam);
		Coord2DValue.ParameterValue = new PointF(ParameterValue[0], ParameterValue[1]);
	}
}

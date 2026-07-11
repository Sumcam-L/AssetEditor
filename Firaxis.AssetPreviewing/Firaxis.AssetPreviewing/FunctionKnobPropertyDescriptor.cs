using System;
using System.ComponentModel;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.AssetPreviewing;

public class FunctionKnobPropertyDescriptor : PropertyDescriptor
{
	public readonly IFunctionKnob Knob;

	public readonly IPaintedPropertyEditor LocalTypeEditor;

	public override string DisplayName => Knob.Label;

	public override Type ComponentType => Knob.GetType();

	public override bool IsReadOnly => false;

	public override Type PropertyType => Knob.GetType();

	public FunctionKnobPropertyDescriptor(IFunctionKnob knob)
		: base(knob.Name, new Attribute[3]
		{
			BrowsableAttribute.Yes,
			new CategoryAttribute(knob.CategoryName),
			new DescriptionAttribute(knob.ToolTip)
		})
	{
		Knob = knob;
		LocalTypeEditor = new PaintedFunctorPropertyEditor(this, Knob);
	}

	public override bool CanResetValue(object component)
	{
		return false;
	}

	public override object GetValue(object component)
	{
		return null;
	}

	public override void ResetValue(object component)
	{
	}

	public override void SetValue(object component, object value)
	{
	}

	public override bool ShouldSerializeValue(object component)
	{
		return false;
	}

	public override object GetEditor(Type editorBaseType)
	{
		return LocalTypeEditor ?? base.GetEditor(editorBaseType);
	}
}

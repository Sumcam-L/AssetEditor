using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetPreviewing;

public class ValueKnobPropertyDescriptor<T> : PropertyDescriptor
{
	public readonly IValueKnob<T> Knob;

	public readonly TypeConverter LocalTypeConverter;

	public readonly object LocalTypeEditor;

	public override string DisplayName => Knob.Label;

	public override Type ComponentType => Knob.GetType();

	public override bool IsReadOnly => Knob.IsReadOnly;

	public override Type PropertyType => Knob.GetValueType();

	public override bool SupportsChangeEvents => true;

	public override TypeConverter Converter => LocalTypeConverter ?? base.Converter;

	public ValueKnobPropertyDescriptor(IValueKnob<T> knob)
		: base(knob.Name, new Attribute[3]
		{
			BrowsableAttribute.Yes,
			new CategoryAttribute(knob.CategoryName),
			new DescriptionAttribute(knob.ToolTip)
		})
	{
		Knob = knob;
		Knob.HasUpdateEvent += Knob_HasUpdateEvent;
		IContainerKnob<string> containerKnob = Knob.As<IContainerKnob<string>>();
		if (containerKnob != null)
		{
			string[] array = containerKnob.Values.ToArray();
			LocalTypeEditor = new CustomEnumPropertyEditor(this, containerKnob);
		}
		else if (Knob.KnobType == KnobType.KT_VALUE_ENTITY)
		{
			LocalTypeEditor = new CustomEntityPickerPropertyEditor(this, knob.As<IValueKnob<string>>());
		}
		else if (typeof(Color).IsAssignableFrom(typeof(T)))
		{
			LocalTypeEditor = new CustomColorPropertyEditor(this, knob.As<IValueKnob<Color>>());
		}
		else if (typeof(bool).IsAssignableFrom(typeof(T)))
		{
			LocalTypeEditor = new CustomBoolPropertyEditor(this);
		}
		else if (typeof(int).IsAssignableFrom(typeof(T)))
		{
			LocalTypeConverter = new Int32Converter();
			LocalTypeEditor = new CustomNumericPropertyEditor<int>(this, knob.As<IValueKnob<int>>());
		}
		else if (typeof(uint).IsAssignableFrom(typeof(T)))
		{
			LocalTypeConverter = new UInt32Converter();
			LocalTypeEditor = new CustomNumericPropertyEditor<uint>(this, knob.As<IValueKnob<uint>>());
		}
		else if (typeof(long).IsAssignableFrom(typeof(T)))
		{
			LocalTypeConverter = new Int64Converter();
			LocalTypeEditor = new CustomNumericPropertyEditor<long>(this, knob.As<IValueKnob<long>>());
		}
		else if (typeof(ulong).IsAssignableFrom(typeof(T)))
		{
			LocalTypeConverter = new UInt64Converter();
			LocalTypeEditor = new CustomNumericPropertyEditor<ulong>(this, knob.As<IValueKnob<ulong>>());
		}
		else if (typeof(float).IsAssignableFrom(typeof(T)))
		{
			LocalTypeConverter = new SingleConverter();
			LocalTypeEditor = new CustomNumericPropertyEditor<float>(this, knob.As<IValueKnob<float>>());
		}
		else if (typeof(double).IsAssignableFrom(typeof(T)))
		{
			LocalTypeConverter = new DoubleConverter();
			LocalTypeEditor = new CustomNumericPropertyEditor<double>(this, knob.As<IValueKnob<double>>());
		}
	}

	private void Knob_HasUpdateEvent(object sender, EventArgs e)
	{
		OnValueChanged(this, EventArgs.Empty);
	}

	public override bool CanResetValue(object component)
	{
		return false;
	}

	public override object GetValue(object component)
	{
		return Knob.Value;
	}

	public override void ResetValue(object component)
	{
	}

	public override void SetValue(object component, object value)
	{
		BugSubmitter.SilentAssert(value is T, "Not a T");
		if (value is T val && !Knob.Value.Equals(val))
		{
			Knob.Value = val;
			OnValueChanged(this, EventArgs.Empty);
		}
	}

	public override bool ShouldSerializeValue(object component)
	{
		return true;
	}

	public override object GetEditor(Type editorBaseType)
	{
		return LocalTypeEditor ?? base.GetEditor(editorBaseType);
	}
}

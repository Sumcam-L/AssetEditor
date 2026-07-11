using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.AssetPreviewing;

public class KnobSetPropertyProxy : ICustomTypeDescriptor, IPropertyEditingContext
{
	private readonly IKnobSet TargetKnobSet;

	public IEnumerable<object> Items
	{
		get
		{
			yield return this;
		}
	}

	public IEnumerable<PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			foreach (IKnob knob in TargetKnobSet.Knobs)
			{
				switch (knob.KnobType)
				{
				case KnobType.KT_VALUE_BOOL:
					yield return new ValueKnobPropertyDescriptor<bool>(knob.As<IValueKnob<bool>>());
					break;
				case KnobType.KT_VALUE_COLOR:
					yield return new ValueKnobPropertyDescriptor<Color>(knob.As<IValueKnob<Color>>());
					break;
				case KnobType.KT_VALUE_FLOAT:
					yield return new ValueKnobPropertyDescriptor<float>(knob.As<IValueKnob<float>>());
					break;
				case KnobType.KT_VALUE_INT:
					yield return new ValueKnobPropertyDescriptor<int>(knob.As<IValueKnob<int>>());
					break;
				case KnobType.KT_VALUE_STRING:
					yield return new ValueKnobPropertyDescriptor<string>(knob.As<IValueKnob<string>>());
					break;
				case KnobType.KT_FUNCTION:
					yield return new FunctionKnobPropertyDescriptor(knob.As<IFunctionKnob>());
					break;
				default:
					Outputs.WriteLine(OutputMessageType.Debug, $"Knob not handled: {knob}");
					break;
				}
			}
		}
	}

	public KnobSetPropertyProxy(IKnobSet knobSet)
	{
		TargetKnobSet = knobSet;
	}

	public virtual AttributeCollection GetAttributes()
	{
		return AttributeCollection.Empty;
	}

	public virtual PropertyDescriptorCollection GetProperties()
	{
		return new PropertyDescriptorCollection(PropertyDescriptors.ToArray());
	}

	public virtual PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		return new PropertyDescriptorCollection(PropertyDescriptors.ToArray());
	}

	public virtual object GetEditor(Type editorBaseType)
	{
		return null;
	}

	public string GetClassName()
	{
		return "KnobProxy";
	}

	public string GetComponentName()
	{
		return TargetKnobSet.KnobSetName;
	}

	public TypeConverter GetConverter()
	{
		return null;
	}

	public EventDescriptor GetDefaultEvent()
	{
		return null;
	}

	public PropertyDescriptor GetDefaultProperty()
	{
		return null;
	}

	public EventDescriptorCollection GetEvents()
	{
		return EventDescriptorCollection.Empty;
	}

	public EventDescriptorCollection GetEvents(Attribute[] attributes)
	{
		return EventDescriptorCollection.Empty;
	}

	public object GetPropertyOwner(PropertyDescriptor pd)
	{
		return this;
	}
}

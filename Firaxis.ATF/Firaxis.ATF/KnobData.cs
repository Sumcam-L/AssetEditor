using System.Drawing;
using System.Xml.Serialization;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

[XmlInclude(typeof(KnobData<int>))]
[XmlInclude(typeof(KnobData<bool>))]
[XmlInclude(typeof(KnobData<float>))]
[XmlInclude(typeof(KnobData<string>))]
[XmlInclude(typeof(ColorKnobData))]
public class KnobData
{
	public string KnobName { get; set; }

	public static void ApplyKnobData(IKnob knob, KnobData knobData)
	{
		if (knobData is KnobData<int>)
		{
			knob.SetValue((knobData as KnobData<int>).KnobValue);
		}
		else if (knobData is KnobData<float>)
		{
			knob.SetValue((knobData as KnobData<float>).KnobValue);
		}
		else if (knobData is KnobData<bool>)
		{
			knob.SetValue((knobData as KnobData<bool>).KnobValue);
		}
		else if (knobData is KnobData<string>)
		{
			knob.SetValue((knobData as KnobData<string>).KnobValue);
		}
		else if (knobData is ColorKnobData)
		{
			Color data = Color.FromArgb((knobData as ColorKnobData).KnobValue);
			knob.SetValue(data);
		}
	}

	public virtual void BuildKnobData(IKnob knob)
	{
		KnobName = knob.Name;
	}

	public static KnobData CreateKnobData(IKnob knob)
	{
		switch (knob.KnobType)
		{
		case KnobType.KT_VALUE_INT:
		case KnobType.KT_RANGE_INT:
		case KnobType.KT_CONTAINER_INT:
			return new KnobData<int>();
		case KnobType.KT_VALUE_FLOAT:
		case KnobType.KT_RANGE_FLOAT:
		case KnobType.KT_CONTAINER_FLOAT:
			return new KnobData<float>();
		case KnobType.KT_VALUE_BOOL:
			return new KnobData<bool>();
		case KnobType.KT_VALUE_COLOR:
		case KnobType.KT_CONTAINER_COLOR:
			return new ColorKnobData();
		case KnobType.KT_VALUE_STRING:
		case KnobType.KT_CONTAINER_STRING:
		case KnobType.KT_VALUE_ENTITY:
			return new KnobData<string>();
		default:
			return null;
		}
	}

	public static void UpdateKnobSetData(KnobSetData data, IKnobSet knobSet, bool checkNameConsistency)
	{
		if (knobSet == null || (checkNameConsistency && knobSet.KnobSetName != data.KnobSetName))
		{
			return;
		}
		foreach (KnobData knobDatum in data.KnobData)
		{
			IKnob knob = knobSet.FindKnobByName(knobDatum.KnobName);
			if (knob != null)
			{
				ApplyKnobData(knob, knobDatum);
			}
		}
	}
}
public class KnobData<T> : KnobData
{
	public T KnobValue { get; set; }

	public override void BuildKnobData(IKnob knob)
	{
		base.BuildKnobData(knob);
		if (knob is IValueKnob<T> valueKnob)
		{
			KnobValue = valueKnob.Value;
		}
	}
}

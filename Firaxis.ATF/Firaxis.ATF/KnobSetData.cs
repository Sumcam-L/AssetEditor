using System.Collections.Generic;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

public class KnobSetData
{
	public List<KnobData> KnobData { get; set; }

	public string KnobSetName { get; set; }

	public KnobSetData()
	{
		KnobData = new List<KnobData>();
	}

	public void BuildKnobSetData(IKnobSet knobSet)
	{
		KnobSetName = string.Empty;
		KnobData.Clear();
		if (knobSet == null)
		{
			return;
		}
		KnobSetName = knobSet.KnobSetName;
		foreach (IKnob knob in knobSet.Knobs)
		{
			KnobData knobData = Firaxis.ATF.KnobData.CreateKnobData(knob);
			if (knobData != null)
			{
				knobData.BuildKnobData(knob);
				KnobData.Add(knobData);
			}
		}
	}
}

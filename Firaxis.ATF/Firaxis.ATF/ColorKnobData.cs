using System.Drawing;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

public class ColorKnobData : KnobData
{
	public int KnobValue { get; set; }

	public override void BuildKnobData(IKnob knob)
	{
		base.BuildKnobData(knob);
		if (knob is IValueKnob<Color> valueKnob)
		{
			KnobValue = valueKnob.Value.ToArgb();
		}
	}
}

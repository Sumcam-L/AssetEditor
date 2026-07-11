using System.Drawing;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IColorValueKnob : IValueKnob<Color>, IValueKnobBase, IKnob
{
	int RValue { get; set; }

	int BValue { get; set; }

	int GValue { get; set; }
}

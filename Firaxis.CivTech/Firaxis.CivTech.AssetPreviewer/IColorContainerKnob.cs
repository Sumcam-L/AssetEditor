using System.Collections.Generic;
using System.Drawing;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IColorContainerKnob : IColorValueKnob, IValueKnob<Color>, IValueKnobBase, IKnob
{
	IEnumerable<Color> Values { get; }
}

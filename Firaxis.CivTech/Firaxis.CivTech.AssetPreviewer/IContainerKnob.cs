using System.Collections.Generic;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IContainerKnob<T> : IValueKnob<T>, IValueKnobBase, IKnob
{
	IEnumerable<T> Values { get; }
}

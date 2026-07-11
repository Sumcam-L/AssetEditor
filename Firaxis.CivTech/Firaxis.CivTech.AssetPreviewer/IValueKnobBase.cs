using System;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IValueKnobBase : IKnob
{
	bool IsReadOnly { get; }

	Type GetValueType();
}

using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IKnobSet : IDisposable
{
	string KnobSetName { get; }

	IEnumerable<IKnob> Knobs { get; }

	IDictionary<string, IKnobSubgroup> KnobsBySubgroup { get; }

	event EventHandler KnobSetChanged;

	event EventHandler KnobSetCleared;

	IKnob FindKnobByName(string name);
}

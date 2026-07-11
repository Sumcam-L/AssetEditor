using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IKnobSubgroup : IAssemblyInstance, IDisposable
{
	string SubgroupName { get; }

	IEnumerable<IKnob> Knobs { get; }

	IKnob FindKnobByName(string name);
}

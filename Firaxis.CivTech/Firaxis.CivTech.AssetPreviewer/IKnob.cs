using System;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IKnob
{
	string Name { get; }

	string GroupName { get; }

	string SubgroupName { get; }

	string CategoryName { get; }

	string Label { get; }

	string ToolTip { get; }

	KnobType KnobType { get; }

	event EventHandler HasUpdateEvent;
}

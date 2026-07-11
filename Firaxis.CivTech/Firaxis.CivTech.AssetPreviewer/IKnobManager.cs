using System;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IKnobManager : IDisposable
{
	event KnobGroupChangedEventHandler KnobGroupChanged;

	event KnobGroupClearedEventHandler KnobGroupCleared;

	IKnobSet GetKnobSet(string groupName);
}

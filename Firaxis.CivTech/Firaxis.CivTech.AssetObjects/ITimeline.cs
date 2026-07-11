using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface ITimeline
{
	string Name { get; set; }

	string Description { get; set; }

	string AnimationName { get; set; }

	float Duration { get; set; }

	IEnumerable<ITrack> Tracks { get; }

	IEnumerable<ITrigger> Triggers { get; }

	ITrack AddTrack(string name, TriggerType type);

	void RemoveTrack(ITrack track);

	ITrigger FindTrigger(string name);

	ITrigger GetTrigger(string name, TriggerType type);

	void ClearTriggers();

	void RemoveTrigger(string name);
}

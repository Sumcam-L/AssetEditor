using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface ITimelineSet
{
	IEnumerable<ITimeline> Timelines { get; }

	ITimeline FindTimeline(string name);

	ITimeline AddTimeline();

	ITimeline GetTimeline(string name);

	bool RemoveTimeline(string name);

	void ClearTimelines();
}

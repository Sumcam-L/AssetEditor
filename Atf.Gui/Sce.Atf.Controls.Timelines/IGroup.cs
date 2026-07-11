using System.Collections.Generic;

namespace Sce.Atf.Controls.Timelines;

public interface IGroup : ITimelineObject
{
	string Name { get; set; }

	bool Expanded { get; set; }

	ITimeline Timeline { get; }

	IList<ITrack> Tracks { get; }

	ITrack CreateTrack();
}

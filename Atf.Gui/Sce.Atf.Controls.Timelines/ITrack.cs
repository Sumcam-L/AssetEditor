using System.Collections.Generic;

namespace Sce.Atf.Controls.Timelines;

public interface ITrack : ITimelineObject
{
	string Name { get; set; }

	IGroup Group { get; }

	IList<IInterval> Intervals { get; }

	IList<IKey> Keys { get; }

	IInterval CreateInterval();

	IKey CreateKey();
}

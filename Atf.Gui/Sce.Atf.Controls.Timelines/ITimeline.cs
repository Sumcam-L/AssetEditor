using System.Collections.Generic;

namespace Sce.Atf.Controls.Timelines;

public interface ITimeline
{
	IList<IGroup> Groups { get; }

	IList<IMarker> Markers { get; }

	IGroup CreateGroup();

	IMarker CreateMarker();
}

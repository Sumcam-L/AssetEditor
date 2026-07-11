using System.Collections.Generic;

namespace Sce.Atf.Controls.Timelines;

public interface IHierarchicalTimeline : ITimeline
{
	IEnumerable<ITimelineReference> References { get; }
}

using System.Collections.Generic;

namespace Sce.Atf.Controls.Timelines;

public interface IHierarchicalTimelineList
{
	IList<ITimelineReference> References { get; }
}

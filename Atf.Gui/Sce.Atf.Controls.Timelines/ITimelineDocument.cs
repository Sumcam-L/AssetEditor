using Sce.Atf.Adaptation;

namespace Sce.Atf.Controls.Timelines;

public interface ITimelineDocument : IDocument, IResource, IObservableContext, IAdaptable
{
	ITimeline Timeline { get; }
}

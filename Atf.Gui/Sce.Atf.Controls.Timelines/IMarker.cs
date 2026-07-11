namespace Sce.Atf.Controls.Timelines;

public interface IMarker : IEvent, ITimelineObject
{
	ITimeline Timeline { get; }
}

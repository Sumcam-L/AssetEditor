namespace Sce.Atf.Controls.Timelines;

public interface IInterval : IEvent, ITimelineObject
{
	ITrack Track { get; }
}

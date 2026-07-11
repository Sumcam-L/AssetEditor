namespace Sce.Atf.Controls.Timelines;

public interface IKey : IEvent, ITimelineObject
{
	ITrack Track { get; }
}

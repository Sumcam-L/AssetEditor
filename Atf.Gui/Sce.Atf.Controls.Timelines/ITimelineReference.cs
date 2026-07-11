namespace Sce.Atf.Controls.Timelines;

public interface ITimelineReference : IEvent, ITimelineObject
{
	IHierarchicalTimeline Target { get; }

	IHierarchicalTimeline Parent { get; }

	TimelineReferenceOptions Options { get; }
}

namespace Sce.Atf.Controls.Timelines;

public interface ITimelineObjectCreator : ITimelineObject
{
	ITimelineObject Create();
}

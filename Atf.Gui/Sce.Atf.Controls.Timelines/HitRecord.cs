namespace Sce.Atf.Controls.Timelines;

public class HitRecord
{
	public readonly HitType Type;

	public readonly object HitObject;

	public readonly ITimelineObject HitTimelineObject;

	public readonly TimelinePath HitPath;

	public HitRecord(HitType type, object hitObject)
	{
		Type = type;
		HitObject = hitObject;
		HitPath = null;
		HitTimelineObject = null;
	}

	public HitRecord(HitType type, TimelinePath path)
	{
		Type = type;
		HitPath = path;
		HitTimelineObject = ((path != null) ? path.Last : null);
		HitObject = HitTimelineObject;
	}
}

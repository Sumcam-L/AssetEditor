using System.Drawing;

namespace Sce.Atf.Controls.Timelines;

public class GhostInfo
{
	public readonly ITimelineObject Object;

	public readonly ITimelineObject Target;

	public readonly float Start;

	public readonly float Length;

	public readonly RectangleF Bounds;

	public readonly bool Valid;

	public GhostInfo(ITimelineObject obj, ITimelineObject target, float start, float length, RectangleF bounds, bool valid)
	{
		Object = obj;
		Target = target;
		Start = start;
		Length = length;
		Bounds = bounds;
		Valid = valid;
	}
}

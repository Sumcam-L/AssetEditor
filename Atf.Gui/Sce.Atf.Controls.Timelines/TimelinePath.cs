using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Controls.Timelines;

public class TimelinePath : AdaptablePath<ITimelineObject>
{
	public TimelinePath(ITimelineObject last)
		: base(last)
	{
	}

	public TimelinePath(IEnumerable<ITimelineObject> path)
		: base(path)
	{
	}

	public static TimelinePath operator +(ITimelineObject lhs, TimelinePath rhs)
	{
		if (rhs == null)
		{
			return new TimelinePath(lhs);
		}
		ITimelineObject[] array = new ITimelineObject[1 + rhs.Count];
		array[0] = lhs;
		rhs.CopyTo(array, 1);
		return new TimelinePath(array);
	}

	public static TimelinePath operator +(TimelinePath lhs, ITimelineObject rhs)
	{
		if (lhs == null)
		{
			return new TimelinePath(rhs);
		}
		ITimelineObject[] array = new ITimelineObject[lhs.Count + 1];
		lhs.CopyTo(array, 0);
		array[lhs.Count] = rhs;
		return new TimelinePath(array);
	}

	public static TimelinePath operator +(TimelinePath lhs, TimelinePath rhs)
	{
		if (lhs == null)
		{
			return rhs;
		}
		if (rhs == null)
		{
			return lhs;
		}
		ITimelineObject[] array = new ITimelineObject[lhs.Count + rhs.Count];
		lhs.CopyTo(array, 0);
		rhs.CopyTo(array, lhs.Count);
		return new TimelinePath(array);
	}
}

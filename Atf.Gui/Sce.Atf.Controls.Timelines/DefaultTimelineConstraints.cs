using System.Collections.Generic;

namespace Sce.Atf.Controls.Timelines;

public class DefaultTimelineConstraints : TimelineConstraints
{
	public override bool IsStartValid(IEvent _event, ref float start)
	{
		if (start >= 0f)
		{
			return true;
		}
		start = 0f;
		return false;
	}

	public override bool IsLengthValid(IInterval interval, ref float length)
	{
		if (length > 0f)
		{
			return true;
		}
		length = 1f;
		return false;
	}

	public override bool IsIntervalValid(IInterval interval, ref float start, ref float length, IInterval other)
	{
		if (ClipAgainst(interval, ref start, ref length, other))
		{
			return true;
		}
		start = other.Start + other.Length;
		if (interval.Track != null)
		{
			IList<IInterval> intervals = interval.Track.Intervals;
			for (int i = 0; i < intervals.Count; i++)
			{
				other = intervals[i];
				if (other != interval && !ClipAgainst(interval, ref start, ref length, other))
				{
					start = other.Start + other.Length;
					i = -1;
				}
			}
		}
		return true;
	}

	private bool ClipAgainst(IInterval interval, ref float start, ref float length, IInterval other)
	{
		float num = start + length;
		float start2 = other.Start;
		float num2 = start2 + other.Length;
		if (start >= num2 || num <= start2)
		{
			return true;
		}
		if (start <= start2 && num >= num2 && length > other.Length)
		{
			if (num - num2 < start2 - start)
			{
				length = start2 - start;
			}
			else
			{
				start = num2;
				length = num - start;
			}
			return true;
		}
		if (start < start2)
		{
			length = start2 - start;
			return true;
		}
		if (start < num2 && num > num2)
		{
			length = num - num2;
			start = num2;
			return true;
		}
		return false;
	}
}

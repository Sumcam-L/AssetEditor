namespace Sce.Atf.Controls.Timelines;

public static class Intervals
{
	public static void SetTrack(this IInterval interval, ITrack newTrack)
	{
		interval.Track?.Intervals.Remove(interval);
		newTrack?.Intervals.Add(interval);
	}
}

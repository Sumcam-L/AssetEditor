using System;
using System.Diagnostics;

namespace Firaxis.Utility;

public static class Ticks
{
	public static long Count => Stopwatch.GetTimestamp();

	public static long Frequency => Stopwatch.Frequency;

	public static float SecondsCount => (float)CountToTimeSpan(Count).TotalSeconds;

	public static TimeSpan CountToTimeSpan(long value)
	{
		long value2 = value * 10000000 / Frequency;
		return TimeSpan.FromTicks(value2);
	}
}

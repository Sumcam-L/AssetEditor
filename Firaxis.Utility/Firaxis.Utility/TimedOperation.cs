using System;
using System.Diagnostics;

namespace Firaxis.Utility;

public static class TimedOperation
{
	public static float Do(Action op)
	{
		long timestamp = Stopwatch.GetTimestamp();
		op();
		return (float)(Stopwatch.GetTimestamp() - timestamp) / (float)Stopwatch.Frequency;
	}
}

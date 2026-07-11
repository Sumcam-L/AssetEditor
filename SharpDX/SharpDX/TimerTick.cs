using System;
using System.Diagnostics;

namespace SharpDX;

public class TimerTick
{
	private long startRawTime;

	private long lastRawTime;

	private int pauseCount;

	private long pauseStartTime;

	private long timePaused;

	public TimeSpan TotalTime { get; private set; }

	public TimeSpan ElapsedAdjustedTime { get; private set; }

	public TimeSpan ElapsedTime { get; private set; }

	public bool IsPaused => pauseCount > 0;

	public TimerTick()
	{
		Reset();
	}

	public void Reset()
	{
		TotalTime = TimeSpan.Zero;
		startRawTime = Stopwatch.GetTimestamp();
		lastRawTime = startRawTime;
	}

	public void Resume()
	{
		pauseCount--;
		if (pauseCount <= 0)
		{
			timePaused += Stopwatch.GetTimestamp() - pauseStartTime;
			pauseStartTime = 0L;
		}
	}

	public void Tick()
	{
		if (!IsPaused)
		{
			long timestamp = Stopwatch.GetTimestamp();
			TotalTime = ConvertRawToTimestamp(timestamp - startRawTime);
			ElapsedTime = ConvertRawToTimestamp(timestamp - lastRawTime);
			ElapsedAdjustedTime = ConvertRawToTimestamp(timestamp - (lastRawTime + timePaused));
			if (ElapsedAdjustedTime < TimeSpan.Zero)
			{
				ElapsedAdjustedTime = TimeSpan.Zero;
			}
			timePaused = 0L;
			lastRawTime = timestamp;
		}
	}

	public void Pause()
	{
		pauseCount++;
		if (pauseCount == 1)
		{
			pauseStartTime = Stopwatch.GetTimestamp();
		}
	}

	private static TimeSpan ConvertRawToTimestamp(long delta)
	{
		return TimeSpan.FromTicks(delta * 10000000 / Stopwatch.Frequency);
	}
}

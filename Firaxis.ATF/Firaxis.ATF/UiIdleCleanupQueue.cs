using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public static class UiIdleCleanupQueue
{
	private sealed class CleanupItem
	{
		public string Name;

		public Action Cleanup;
	}

	private static readonly Queue<CleanupItem> s_items = new Queue<CleanupItem>();

	private static bool s_idleSubscribed;

	private static bool s_applicationExitSubscribed;

	private static Timer s_rescheduleTimer;

	public static void Enqueue(string name, Action cleanup)
	{
		if (cleanup == null)
		{
			throw new ArgumentNullException(nameof(cleanup));
		}
		s_items.Enqueue(new CleanupItem
		{
			Name = name,
			Cleanup = cleanup
		});
		PaintTimingLog.Write("UiIdleCleanupQueue: queued name={0}, count={1}", name, s_items.Count);
		if (!s_applicationExitSubscribed)
		{
			Application.ApplicationExit += Application_ApplicationExit;
			s_applicationExitSubscribed = true;
		}
		SubscribeIdle();
	}

	public static void Drain()
	{
		StopRescheduleTimer();
		UnsubscribeIdle();
		while (s_items.Count > 0)
		{
			Execute(s_items.Dequeue());
		}
	}

	private static void Application_Idle(object sender, EventArgs e)
	{
		UnsubscribeIdle();
		if (s_items.Count == 0)
		{
			return;
		}
		Execute(s_items.Dequeue());
		if (s_items.Count == 0)
		{
			return;
		}
		ScheduleNextIdle();
	}

	private static void Application_ApplicationExit(object sender, EventArgs e)
	{
		try
		{
			Drain();
		}
		finally
		{
			Application.ApplicationExit -= Application_ApplicationExit;
			s_applicationExitSubscribed = false;
		}
	}

	private static void SubscribeIdle()
	{
		if (s_idleSubscribed || s_items.Count == 0)
		{
			return;
		}
		Application.Idle += Application_Idle;
		s_idleSubscribed = true;
	}

	private static void UnsubscribeIdle()
	{
		if (!s_idleSubscribed)
		{
			return;
		}
		Application.Idle -= Application_Idle;
		s_idleSubscribed = false;
	}

	private static void ScheduleNextIdle()
	{
		if (s_rescheduleTimer == null)
		{
			s_rescheduleTimer = new Timer
			{
				Interval = 150
			};
			s_rescheduleTimer.Tick += RescheduleTimer_Tick;
		}
		s_rescheduleTimer.Stop();
		s_rescheduleTimer.Start();
	}

	private static void RescheduleTimer_Tick(object sender, EventArgs e)
	{
		StopRescheduleTimer();
		SubscribeIdle();
	}

	private static void StopRescheduleTimer()
	{
		s_rescheduleTimer?.Stop();
	}

	private static void Execute(CleanupItem item)
	{
		PaintTimingLog.Write("UiIdleCleanupQueue: begin name={0}, remaining={1}", item.Name, s_items.Count);
		var sw = Stopwatch.StartNew();
		try
		{
			item.Cleanup();
		}
		catch (System.Exception ex)
		{
			PaintTimingLog.Write("UiIdleCleanupQueue: failed name={0}, error={1}: {2}", item.Name, ex.GetType().Name, ex.Message);
		}
		finally
		{
			PaintTimingLog.Write("UiIdleCleanupQueue: end name={0}, duration={1}ms", item.Name, sw.ElapsedMilliseconds);
		}
	}
}

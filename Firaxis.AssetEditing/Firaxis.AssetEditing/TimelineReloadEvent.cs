using System;

namespace Firaxis.AssetEditing;

public class TimelineReloadEvent : EventArgs
{
	public TimelineAdapter Timeline { get; private set; }

	public TimelineReloadEvent(TimelineAdapter adapter)
	{
		Timeline = adapter;
	}
}

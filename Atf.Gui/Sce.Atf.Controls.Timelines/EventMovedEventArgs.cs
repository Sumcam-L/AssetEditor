using System;

namespace Sce.Atf.Controls.Timelines;

public class EventMovedEventArgs : EventArgs
{
	public readonly IEvent Event;

	public readonly float NewStart;

	public readonly ITrack NewTrack;

	public EventMovedEventArgs(IEvent _event, float newStart, ITrack newTrack)
	{
		Event = _event;
		NewStart = newStart;
		NewTrack = newTrack;
	}
}

using System;

namespace ScintillaNET;

internal sealed class SCNotificationEventArgs : EventArgs
{
	public NativeMethods.SCNotification SCNotification { get; private set; }

	public SCNotificationEventArgs(NativeMethods.SCNotification scn)
	{
		SCNotification = scn;
	}
}

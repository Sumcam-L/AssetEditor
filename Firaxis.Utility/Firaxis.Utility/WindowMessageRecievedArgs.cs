using System;

namespace Firaxis.Utility;

public class WindowMessageRecievedArgs : EventArgs
{
	public IntPtr Sender;

	public string Message;
}

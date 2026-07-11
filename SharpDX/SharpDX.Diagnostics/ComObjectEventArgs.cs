using System;

namespace SharpDX.Diagnostics;

public class ComObjectEventArgs : EventArgs
{
	public ComObject Object;

	public ComObjectEventArgs(ComObject o)
	{
		Object = o;
	}
}

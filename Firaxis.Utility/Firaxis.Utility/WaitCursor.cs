using System;
using System.Windows.Input;

namespace Firaxis.Utility;

public class WaitCursor : IDisposable
{
	public WaitCursor()
	{
		Mouse.OverrideCursor = Cursors.Wait;
	}

	public void Dispose()
	{
		Mouse.OverrideCursor = null;
	}
}

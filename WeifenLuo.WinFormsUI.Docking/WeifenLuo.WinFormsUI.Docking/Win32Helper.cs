using System;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public static class Win32Helper
{
	private static readonly bool _isRunningOnMono = Type.GetType("Mono.Runtime") != null;

	public static bool IsRunningOnMono => _isRunningOnMono;

	internal static Control ControlAtPoint(Point pt)
	{
		return Control.FromChildHandle(NativeMethods.WindowFromPoint(pt));
	}

	internal static uint MakeLong(int low, int high)
	{
		return (uint)((high << 16) + low);
	}

	internal static uint HitTestCaption(Control control)
	{
		Rectangle rectangle = new Rectangle(0, 0, control.Width, control.ClientRectangle.Top - control.PointToClient(control.Location).X);
		if (!rectangle.Contains(Control.MousePosition))
		{
			return 0u;
		}
		return 2u;
	}
}

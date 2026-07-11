using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Behaviors;

public class MouseUtilities
{
	private struct Win32Point
	{
		public int X;

		public int Y;
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool SetCursorPos(int x, int y);

	[DllImport("user32.dll")]
	private static extern bool GetCursorPos(ref Win32Point pt);

	[DllImport("user32.dll")]
	private static extern bool ScreenToClient(IntPtr hwnd, ref Win32Point pt);

	public static Point CorrectGetPosition(Visual relativeTo)
	{
		Win32Point pt = default(Win32Point);
		GetCursorPos(ref pt);
		return relativeTo.PointFromScreen(new Point(pt.X, pt.Y));
	}
}

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Docking;

internal class Win32Calls
{
	internal struct Win32Point
	{
		public int X;

		public int Y;
	}

	internal static Point GetPosition(Visual relativeTo)
	{
		Win32Point pt = default(Win32Point);
		GetCursorPos(ref pt);
		return relativeTo.PointFromScreen(new Point(pt.X, pt.Y));
	}

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool GetCursorPos(ref Win32Point pt);
}

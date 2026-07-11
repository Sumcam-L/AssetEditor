using System;

namespace SharpDX.Win32;

public struct NativeMessage
{
	public IntPtr handle;

	public uint msg;

	public IntPtr wParam;

	public IntPtr lParam;

	public uint time;

	public Point p;
}

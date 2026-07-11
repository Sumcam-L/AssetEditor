using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct BitmapPattern
{
	public long Position;

	public int Length;

	public IntPtr Pattern;

	public IntPtr Mask;

	public Bool EndOfStream;
}

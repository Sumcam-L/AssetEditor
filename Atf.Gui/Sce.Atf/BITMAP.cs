using System;

namespace Sce.Atf;

public struct BITMAP
{
	public long bmType;

	public long bmWidth;

	public long bmHeight;

	public long bmWidthBytes;

	public short bmPlanes;

	public short bmBitsPixel;

	public IntPtr bmBits;
}

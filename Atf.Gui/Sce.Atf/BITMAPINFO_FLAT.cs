using System.Runtime.InteropServices;

namespace Sce.Atf;

public struct BITMAPINFO_FLAT
{
	public int biSize;

	public int biWidth;

	public int biHeight;

	public short biPlanes;

	public short biBitCount;

	public int biCompression;

	public int biSizeImage;

	public int biXPelsPerMeter;

	public int biYPelsPerMeter;

	public int biClrUsed;

	public int biClrImportant;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
	public byte[] bmiColors;
}

using System.Runtime.InteropServices;

namespace Sce.Atf;

[StructLayout(LayoutKind.Sequential)]
public class BITMAPINFOHEADER
{
	public int biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER));

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
}

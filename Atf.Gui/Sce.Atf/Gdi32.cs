using System;
using System.Runtime.InteropServices;

namespace Sce.Atf;

public static class Gdi32
{
	public enum DIBUsage
	{
		RGB,
		Palette
	}

	public const int SRCCOPY = 13369376;

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int Width, int Heigth);

	[DllImport("gdi32.dll")]
	public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

	[DllImport("gdi32.dll")]
	public static extern IntPtr DeleteDC(IntPtr hDC);

	[DllImport("gdi32.dll")]
	public static extern bool DeleteObject(IntPtr hObject);

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO_FLAT bmi, DIBUsage usage, ref int ppvBits, IntPtr hSection, int dwOffset);

	[DllImport("gdi32.dll")]
	public static extern int GetDIBits(IntPtr hDC, IntPtr hbm, int StartScan, int ScanLines, int lpBits, BITMAPINFOHEADER bmi, int usage);

	[DllImport("gdi32.dll")]
	public static extern int GetDIBits(IntPtr hdc, IntPtr hbm, int StartScan, int ScanLines, int lpBits, ref BITMAPINFO_FLAT bmi, int usage);

	[DllImport("gdi32.dll")]
	public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);
}

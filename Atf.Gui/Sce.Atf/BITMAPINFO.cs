using System.Runtime.InteropServices;

namespace Sce.Atf;

[StructLayout(LayoutKind.Sequential)]
public class BITMAPINFO
{
	public BITMAPINFOHEADER bmiHeader = new BITMAPINFOHEADER();

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
	public byte[] bmiColors;
}

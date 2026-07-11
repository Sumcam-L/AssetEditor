using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct RawToneCurvePoint
{
	public double Input;

	public double Output;
}

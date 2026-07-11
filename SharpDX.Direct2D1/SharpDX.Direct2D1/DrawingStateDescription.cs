using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct DrawingStateDescription
{
	public AntialiasMode AntialiasMode;

	public TextAntialiasMode TextAntialiasMode;

	public long Tag1;

	public long Tag2;

	public Matrix3x2 Transform;
}

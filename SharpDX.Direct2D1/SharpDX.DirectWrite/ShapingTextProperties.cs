using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Explicit, Pack = 2)]
public struct ShapingTextProperties
{
	[FieldOffset(0)]
	internal short _IsShapedAlone;

	[FieldOffset(0)]
	internal short _Reserved;

	public bool IsShapedAlone
	{
		get
		{
			return 0 != (_IsShapedAlone & 1);
		}
		set
		{
			_IsShapedAlone = (short)((_IsShapedAlone & -2) | ((value ? 1 : 0) & 1));
		}
	}

	internal short Reserved
	{
		get
		{
			return (short)((_Reserved >> 1) & 0x7FFF);
		}
		set
		{
			_Reserved = (short)((_Reserved & -65535) | ((value & 0x7FFF) << 1));
		}
	}
}

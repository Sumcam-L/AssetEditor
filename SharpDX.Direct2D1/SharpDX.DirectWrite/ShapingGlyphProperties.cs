using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Explicit, Pack = 2)]
public struct ShapingGlyphProperties
{
	[FieldOffset(0)]
	internal short _Justification_;

	[FieldOffset(0)]
	internal short _IsClusterStart;

	[FieldOffset(0)]
	internal short _IsDiacritic;

	[FieldOffset(0)]
	internal short _IsZeroWidthSpace;

	[FieldOffset(0)]
	internal short _Reserved;

	public ScriptJustify Justification
	{
		get
		{
			return (ScriptJustify)Justification_;
		}
		set
		{
			Justification_ = (short)value;
		}
	}

	internal short Justification_
	{
		get
		{
			return (short)(_Justification_ & 0xF);
		}
		set
		{
			_Justification_ = (short)((_Justification_ & -16) | (value & 0xF));
		}
	}

	public bool IsClusterStart
	{
		get
		{
			return 0 != ((_IsClusterStart >> 4) & 1);
		}
		set
		{
			_IsClusterStart = (short)((_IsClusterStart & -17) | (((value ? 1 : 0) & 1) << 4));
		}
	}

	public bool IsDiacritic
	{
		get
		{
			return 0 != ((_IsDiacritic >> 5) & 1);
		}
		set
		{
			_IsDiacritic = (short)((_IsDiacritic & -33) | (((value ? 1 : 0) & 1) << 5));
		}
	}

	public bool IsZeroWidthSpace
	{
		get
		{
			return 0 != ((_IsZeroWidthSpace >> 6) & 1);
		}
		set
		{
			_IsZeroWidthSpace = (short)((_IsZeroWidthSpace & -65) | (((value ? 1 : 0) & 1) << 6));
		}
	}

	internal short Reserved
	{
		get
		{
			return (short)((_Reserved >> 7) & 0x1FF);
		}
		set
		{
			_Reserved = (short)((_Reserved & -65409) | ((value & 0x1FF) << 7));
		}
	}
}

using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct LineBreakpoint
{
	[FieldOffset(0)]
	internal byte _BreakConditionBefore_;

	[FieldOffset(0)]
	internal byte _BreakConditionAfter_;

	[FieldOffset(0)]
	internal byte _IsWhitespace;

	[FieldOffset(0)]
	internal byte _IsSoftHyphen;

	[FieldOffset(0)]
	internal byte _Padding;

	public BreakCondition BreakConditionBefore
	{
		get
		{
			return (BreakCondition)BreakConditionBefore_;
		}
		set
		{
			BreakConditionBefore_ = (byte)value;
		}
	}

	public BreakCondition BreakConditionAfter
	{
		get
		{
			return (BreakCondition)BreakConditionAfter_;
		}
		set
		{
			BreakConditionAfter_ = (byte)value;
		}
	}

	internal byte BreakConditionBefore_
	{
		get
		{
			return (byte)(_BreakConditionBefore_ & 3);
		}
		set
		{
			_BreakConditionBefore_ = (byte)((_BreakConditionBefore_ & -4) | (value & 3));
		}
	}

	internal byte BreakConditionAfter_
	{
		get
		{
			return (byte)((_BreakConditionAfter_ >> 2) & 3);
		}
		set
		{
			_BreakConditionAfter_ = (byte)((_BreakConditionAfter_ & -13) | ((value & 3) << 2));
		}
	}

	public bool IsWhitespace
	{
		get
		{
			return 0 != ((_IsWhitespace >> 4) & 1);
		}
		set
		{
			_IsWhitespace = (byte)((_IsWhitespace & -17) | (((value ? 1 : 0) & 1) << 4));
		}
	}

	public bool IsSoftHyphen
	{
		get
		{
			return 0 != ((_IsSoftHyphen >> 5) & 1);
		}
		set
		{
			_IsSoftHyphen = (byte)((_IsSoftHyphen & -33) | (((value ? 1 : 0) & 1) << 5));
		}
	}

	internal byte Padding
	{
		get
		{
			return (byte)((_Padding >> 6) & 3);
		}
		set
		{
			_Padding = (byte)((_Padding & -193) | ((value & 3) << 6));
		}
	}
}

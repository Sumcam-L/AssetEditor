using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

public struct RawToneCurve
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct __Native
	{
		public int CPoints;

		public RawToneCurvePoint APoints;

		internal void __MarshalFree()
		{
		}
	}

	public int CPoints;

	internal RawToneCurvePoint[] _APoints;

	public RawToneCurvePoint[] APoints => _APoints ?? (_APoints = new RawToneCurvePoint[1]);

	internal void __MarshalFree(ref __Native @ref)
	{
		@ref.__MarshalFree();
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		CPoints = @ref.CPoints;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RawToneCurvePoint, IntPtr>(ref APoints[0]))
		{
			fixed (IntPtr* aPoints = &System.Runtime.CompilerServices.Unsafe.As<RawToneCurvePoint, IntPtr>(ref @ref.APoints))
			{
				Utilities.CopyMemory((IntPtr)ptr, (IntPtr)aPoints, sizeof(RawToneCurvePoint));
			}
		}
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		@ref.CPoints = CPoints;
		fixed (IntPtr* aPoints = &System.Runtime.CompilerServices.Unsafe.As<RawToneCurvePoint, IntPtr>(ref @ref.APoints))
		{
			fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RawToneCurvePoint, IntPtr>(ref APoints[0]))
			{
				Utilities.CopyMemory((IntPtr)aPoints, (IntPtr)ptr, sizeof(RawToneCurvePoint));
			}
		}
	}
}

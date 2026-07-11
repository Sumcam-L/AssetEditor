using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906a3-12e2-11dc-9fed-001143a055f9")]
public class RoundedRectangleGeometry : Geometry
{
	public RoundedRectangle RoundedRect
	{
		get
		{
			GetRoundedRect(out var roundedRect);
			return roundedRect;
		}
	}

	public RoundedRectangleGeometry(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RoundedRectangleGeometry(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new RoundedRectangleGeometry(nativePointer);
		}
		return null;
	}

	internal unsafe void GetRoundedRect(out RoundedRectangle roundedRect)
	{
		roundedRect = default(RoundedRectangle);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RoundedRectangle, IntPtr>(ref roundedRect))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	public RoundedRectangleGeometry(Factory factory, RoundedRectangle roundedRectangle)
		: base(IntPtr.Zero)
	{
		factory.CreateRoundedRectangleGeometry(ref roundedRectangle, this);
	}
}

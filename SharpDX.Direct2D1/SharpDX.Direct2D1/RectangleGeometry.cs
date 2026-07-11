using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906a2-12e2-11dc-9fed-001143a055f9")]
public class RectangleGeometry : Geometry
{
	public RectangleF Rectangle
	{
		get
		{
			GetRectangle(out var rect);
			return rect;
		}
	}

	public RectangleGeometry(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RectangleGeometry(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new RectangleGeometry(nativePointer);
		}
		return null;
	}

	internal unsafe void GetRectangle(out RectangleF rect)
	{
		rect = default(RectangleF);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RectangleF, IntPtr>(ref rect))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	public RectangleGeometry(Factory factory, RectangleF rectangle)
		: base(IntPtr.Zero)
	{
		factory.CreateRectangleGeometry(rectangle, this);
	}
}

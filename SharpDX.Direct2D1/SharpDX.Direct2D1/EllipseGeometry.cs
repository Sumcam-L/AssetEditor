using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906a4-12e2-11dc-9fed-001143a055f9")]
public class EllipseGeometry : Geometry
{
	public Ellipse Ellipse
	{
		get
		{
			GetEllipse(out var ellipse);
			return ellipse;
		}
	}

	public EllipseGeometry(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator EllipseGeometry(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new EllipseGeometry(nativePointer);
		}
		return null;
	}

	internal unsafe void GetEllipse(out Ellipse ellipse)
	{
		ellipse = default(Ellipse);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Ellipse, IntPtr>(ref ellipse))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	public EllipseGeometry(Factory factory, Ellipse ellipse)
		: base(IntPtr.Zero)
	{
		factory.CreateEllipseGeometry(ellipse, this);
	}
}

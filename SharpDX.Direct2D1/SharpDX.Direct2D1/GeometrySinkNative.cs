using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd9069f-12e2-11dc-9fed-001143a055f9")]
internal class GeometrySinkNative : SimplifiedGeometrySinkNative, GeometrySink, SimplifiedGeometrySink, ICallbackable, IDisposable
{
	public GeometrySinkNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GeometrySinkNative(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new GeometrySinkNative(nativePointer);
		}
		return null;
	}

	internal unsafe void AddLine_(Vector2 point)
	{
		((delegate* unmanaged[Stdcall]<void*, Vector2, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, point);
	}

	internal unsafe void AddBezier_(ref BezierSegment bezier)
	{
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<BezierSegment, IntPtr>(ref bezier))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	internal unsafe void AddQuadraticBezier_(QuadraticBezierSegment bezier)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &bezier);
	}

	internal unsafe void AddQuadraticBeziers_(QuadraticBezierSegment[] beziers, int beziersCount)
	{
		fixed (IntPtr* ptr = beziers)
		{
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, ptr, beziersCount);
		}
	}

	internal unsafe void AddArc_(ref ArcSegment arc)
	{
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<ArcSegment, IntPtr>(ref arc))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	public void AddLine(Vector2 point)
	{
		AddLine_(point);
	}

	public void AddBezier(BezierSegment bezier)
	{
		AddBezier_(ref bezier);
	}

	public void AddQuadraticBezier(QuadraticBezierSegment bezier)
	{
		AddQuadraticBezier_(bezier);
	}

	public void AddQuadraticBeziers(QuadraticBezierSegment[] beziers)
	{
		AddQuadraticBeziers_(beziers, beziers.Length);
	}

	public void AddArc(ArcSegment arc)
	{
		AddArc_(ref arc);
	}
}

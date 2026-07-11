using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd9069d-12e2-11dc-9fed-001143a055f9")]
public class StrokeStyle : Resource
{
	public CapStyle StartCap => GetStartCap();

	public CapStyle EndCap => GetEndCap();

	public CapStyle DashCap => GetDashCap();

	public float MiterLimit => GetMiterLimit();

	public LineJoin LineJoin => GetLineJoin();

	public float DashOffset => GetDashOffset();

	public DashStyle DashStyle => GetDashStyle();

	public int DashesCount => GetDashesCount();

	public StrokeStyle(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator StrokeStyle(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new StrokeStyle(nativePointer);
		}
		return null;
	}

	internal unsafe CapStyle GetStartCap()
	{
		return ((delegate* unmanaged[Stdcall]<void*, CapStyle>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe CapStyle GetEndCap()
	{
		return ((delegate* unmanaged[Stdcall]<void*, CapStyle>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe CapStyle GetDashCap()
	{
		return ((delegate* unmanaged[Stdcall]<void*, CapStyle>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe float GetMiterLimit()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe LineJoin GetLineJoin()
	{
		return ((delegate* unmanaged[Stdcall]<void*, LineJoin>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe float GetDashOffset()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe DashStyle GetDashStyle()
	{
		return ((delegate* unmanaged[Stdcall]<void*, DashStyle>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe int GetDashesCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetDashes(float[] dashes, int dashesCount)
	{
		fixed (IntPtr* ptr = dashes)
		{
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, ptr, dashesCount);
		}
	}

	public StrokeStyle(Factory factory, StrokeStyleProperties properties)
		: base(IntPtr.Zero)
	{
		factory.CreateStrokeStyle(ref properties, null, 0, this);
	}

	public StrokeStyle(Factory factory, StrokeStyleProperties properties, float[] dashes)
		: base(IntPtr.Zero)
	{
		factory.CreateStrokeStyle(ref properties, dashes, dashes.Length, this);
	}
}

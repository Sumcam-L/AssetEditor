using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906a7-12e2-11dc-9fed-001143a055f9")]
public class GradientStopCollection : Resource
{
	public int GradientStopCount => GetGradientStopCount();

	public Gamma ColorInterpolationGamma => GetColorInterpolationGamma();

	public ExtendMode ExtendMode => GetExtendMode();

	public GradientStopCollection(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GradientStopCollection(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new GradientStopCollection(nativePointer);
		}
		return null;
	}

	internal unsafe int GetGradientStopCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetGradientStops(GradientStop[] gradientStops, int gradientStopsCount)
	{
		fixed (IntPtr* ptr = gradientStops)
		{
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr, gradientStopsCount);
		}
	}

	internal unsafe Gamma GetColorInterpolationGamma()
	{
		return ((delegate* unmanaged[Stdcall]<void*, Gamma>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe ExtendMode GetExtendMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ExtendMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	public GradientStopCollection(RenderTarget renderTarget, GradientStop[] gradientStops)
		: this(renderTarget, gradientStops, Gamma.StandardRgb, ExtendMode.Clamp)
	{
	}

	public GradientStopCollection(RenderTarget renderTarget, GradientStop[] gradientStops, ExtendMode extendMode)
		: this(renderTarget, gradientStops, Gamma.StandardRgb, extendMode)
	{
	}

	public GradientStopCollection(RenderTarget renderTarget, GradientStop[] gradientStops, Gamma colorInterpolationGamma)
		: this(renderTarget, gradientStops, colorInterpolationGamma, ExtendMode.Clamp)
	{
	}

	public GradientStopCollection(RenderTarget renderTarget, GradientStop[] gradientStops, Gamma colorInterpolationGamma, ExtendMode extendMode)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateGradientStopCollection(gradientStops, gradientStops.Length, colorInterpolationGamma, extendMode, this);
	}
}

using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906ab-12e2-11dc-9fed-001143a055f9")]
public class LinearGradientBrush : Brush
{
	public Vector2 StartPoint
	{
		get
		{
			return GetStartPoint();
		}
		set
		{
			SetStartPoint(value);
		}
	}

	public Vector2 EndPoint
	{
		get
		{
			return GetEndPoint();
		}
		set
		{
			SetEndPoint(value);
		}
	}

	public GradientStopCollection GradientStopCollection
	{
		get
		{
			GetGradientStopCollection(out var gradientStopCollection);
			return gradientStopCollection;
		}
	}

	public LinearGradientBrush(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator LinearGradientBrush(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new LinearGradientBrush(nativePointer);
		}
		return null;
	}

	internal unsafe void SetStartPoint(Vector2 startPoint)
	{
		((delegate* unmanaged[Stdcall]<void*, Vector2, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, startPoint);
	}

	internal unsafe void SetEndPoint(Vector2 endPoint)
	{
		((delegate* unmanaged[Stdcall]<void*, Vector2, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, endPoint);
	}

	internal unsafe Vector2 GetStartPoint()
	{
		Vector2 result = default(Vector2);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe Vector2 GetEndPoint()
	{
		Vector2 result = default(Vector2);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe void GetGradientStopCollection(out GradientStopCollection gradientStopCollection)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &zero);
		gradientStopCollection = ((zero == IntPtr.Zero) ? null : new GradientStopCollection(zero));
	}

	public LinearGradientBrush(RenderTarget renderTarget, LinearGradientBrushProperties linearGradientBrushProperties, GradientStopCollection gradientStopCollection)
		: this(renderTarget, linearGradientBrushProperties, null, gradientStopCollection)
	{
	}

	public LinearGradientBrush(RenderTarget renderTarget, LinearGradientBrushProperties linearGradientBrushProperties, BrushProperties? brushProperties, GradientStopCollection gradientStopCollection)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateLinearGradientBrush(linearGradientBrushProperties, brushProperties, gradientStopCollection, this);
	}
}

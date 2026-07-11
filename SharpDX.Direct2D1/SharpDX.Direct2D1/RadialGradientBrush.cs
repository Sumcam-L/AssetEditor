using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906ac-12e2-11dc-9fed-001143a055f9")]
public class RadialGradientBrush : Brush
{
	public Vector2 Center
	{
		get
		{
			return GetCenter();
		}
		set
		{
			SetCenter(value);
		}
	}

	public Vector2 GradientOriginOffset
	{
		get
		{
			return GetGradientOriginOffset();
		}
		set
		{
			SetGradientOriginOffset(value);
		}
	}

	public float RadiusX
	{
		get
		{
			return GetRadiusX();
		}
		set
		{
			SetRadiusX(value);
		}
	}

	public float RadiusY
	{
		get
		{
			return GetRadiusY();
		}
		set
		{
			SetRadiusY(value);
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

	public RadialGradientBrush(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RadialGradientBrush(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new RadialGradientBrush(nativePointer);
		}
		return null;
	}

	internal unsafe void SetCenter(Vector2 center)
	{
		((delegate* unmanaged[Stdcall]<void*, Vector2, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, center);
	}

	internal unsafe void SetGradientOriginOffset(Vector2 gradientOriginOffset)
	{
		((delegate* unmanaged[Stdcall]<void*, Vector2, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, gradientOriginOffset);
	}

	internal unsafe void SetRadiusX(float radiusX)
	{
		((delegate* unmanaged[Stdcall]<void*, float, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, radiusX);
	}

	internal unsafe void SetRadiusY(float radiusY)
	{
		((delegate* unmanaged[Stdcall]<void*, float, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, radiusY);
	}

	internal unsafe Vector2 GetCenter()
	{
		Vector2 result = default(Vector2);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe Vector2 GetGradientOriginOffset()
	{
		Vector2 result = default(Vector2);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe float GetRadiusX()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe float GetRadiusY()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetGradientStopCollection(out GradientStopCollection gradientStopCollection)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, &zero);
		gradientStopCollection = ((zero == IntPtr.Zero) ? null : new GradientStopCollection(zero));
	}

	public RadialGradientBrush(RenderTarget renderTarget, ref RadialGradientBrushProperties radialGradientBrushProperties, GradientStopCollection gradientStopCollection)
		: this(renderTarget, ref radialGradientBrushProperties, null, gradientStopCollection)
	{
	}

	public RadialGradientBrush(RenderTarget renderTarget, RadialGradientBrushProperties radialGradientBrushProperties, GradientStopCollection gradientStopCollection)
		: this(renderTarget, ref radialGradientBrushProperties, null, gradientStopCollection)
	{
	}

	public RadialGradientBrush(RenderTarget renderTarget, RadialGradientBrushProperties radialGradientBrushProperties, BrushProperties brushProperties, GradientStopCollection gradientStopCollection)
		: this(renderTarget, ref radialGradientBrushProperties, brushProperties, gradientStopCollection)
	{
	}

	public RadialGradientBrush(RenderTarget renderTarget, ref RadialGradientBrushProperties radialGradientBrushProperties, BrushProperties? brushProperties, GradientStopCollection gradientStopCollection)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateRadialGradientBrush(ref radialGradientBrushProperties, brushProperties, gradientStopCollection, this);
	}
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;

namespace SharpDX.DirectWrite;

[Guid("5e5a32a3-8dff-4773-9ff6-0696eab77267")]
public class BitmapRenderTarget : ComObject
{
	public IntPtr MemoryDC => GetMemoryDC();

	public float PixelsPerDip
	{
		get
		{
			return GetPixelsPerDip();
		}
		set
		{
			SetPixelsPerDip(value);
		}
	}

	public Matrix3x2 CurrentTransform
	{
		get
		{
			GetCurrentTransform(out var transform);
			return transform;
		}
		set
		{
			SetCurrentTransform(value);
		}
	}

	public Size2 Size
	{
		get
		{
			GetSize(out var size);
			return size;
		}
	}

	public void DrawGlyphRun(float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, RenderingParams renderingParams, Color4 textColor)
	{
		DrawGlyphRun(baselineOriginX, baselineOriginY, measuringMode, glyphRun, renderingParams, textColor, out var _);
	}

	public BitmapRenderTarget(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapRenderTarget(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapRenderTarget(nativePointer);
		}
		return null;
	}

	public unsafe void DrawGlyphRun(float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, RenderingParams renderingParams, Color4 textColor, out Rectangle blackBoxRect)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		glyphRun.__MarshalTo(ref @ref);
		blackBoxRect = default(Rectangle);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Rectangle, IntPtr>(ref blackBoxRect))
		{
			void* nativePointer = _nativePointer;
			GlyphRun.__Native* num = &@ref;
			IntPtr intPtr = renderingParams?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, float, float, int, void*, void*, Color4, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(nativePointer, baselineOriginX, baselineOriginY, (int)measuringMode, num, (void*)intPtr, textColor, ptr);
		}
		glyphRun.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe IntPtr GetMemoryDC()
	{
		return ((delegate* unmanaged[Stdcall]<void*, IntPtr>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe float GetPixelsPerDip()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetPixelsPerDip(float pixelsPerDip)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, float, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, pixelsPerDip)).CheckError();
	}

	internal unsafe void GetCurrentTransform(out Matrix3x2 transform)
	{
		transform = default(Matrix3x2);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Matrix3x2, IntPtr>(ref transform))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void SetCurrentTransform(Matrix3x2? transform)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (transform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, intPtr)).CheckError();
	}

	internal unsafe void GetSize(out Size2 size)
	{
		size = default(Size2);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Size2, IntPtr>(ref size))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public unsafe void Resize(int width, int height)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, width, height)).CheckError();
	}
}

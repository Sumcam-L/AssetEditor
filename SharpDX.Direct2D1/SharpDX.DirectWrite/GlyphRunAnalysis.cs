using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;

namespace SharpDX.DirectWrite;

[Guid("7d97dbf7-e085-42d4-81e3-6a883bded118")]
public class GlyphRunAnalysis : ComObject
{
	public GlyphRunAnalysis(Factory factory, GlyphRun glyphRun, float pixelsPerDip, RenderingMode renderingMode, MeasuringMode measuringMode, float baselineOriginX, float baselineOriginY)
		: this(factory, glyphRun, pixelsPerDip, null, renderingMode, measuringMode, baselineOriginX, baselineOriginY)
	{
	}

	public GlyphRunAnalysis(Factory factory, GlyphRun glyphRun, float pixelsPerDip, Matrix? transform, RenderingMode renderingMode, MeasuringMode measuringMode, float baselineOriginX, float baselineOriginY)
	{
		factory.CreateGlyphRunAnalysis(glyphRun, pixelsPerDip, transform, renderingMode, measuringMode, baselineOriginX, baselineOriginY, this);
	}

	public GlyphRunAnalysis(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GlyphRunAnalysis(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new GlyphRunAnalysis(nativePointer);
		}
		return null;
	}

	public unsafe Rectangle GetAlphaTextureBounds(TextureType textureType)
	{
		Rectangle result = default(Rectangle);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (int)textureType, &result)).CheckError();
		return result;
	}

	public unsafe void CreateAlphaTexture(TextureType textureType, Rectangle textureBounds, byte[] alphaValues, int bufferSize)
	{
		Result result;
		fixed (IntPtr* ptr = alphaValues)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (int)textureType, &textureBounds, ptr, bufferSize);
		}
		result.CheckError();
	}

	public unsafe void GetAlphaBlendParams(RenderingParams renderingParams, out float blendGamma, out float blendEnhancedContrast, out float blendClearTypeLevel)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref blendGamma))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref blendEnhancedContrast))
			{
				fixed (IntPtr* ptr3 = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref blendClearTypeLevel))
				{
					void* nativePointer = _nativePointer;
					IntPtr intPtr = renderingParams?.NativePointer ?? IntPtr.Zero;
					result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, ptr, ptr2, ptr3);
				}
			}
		}
		result.CheckError();
	}
}

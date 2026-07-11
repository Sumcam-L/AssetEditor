using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("2f0da53a-2add-47cd-82ee-d9ec34688e75")]
public class RenderingParams : ComObject
{
	public float Gamma => GetGamma();

	public float EnhancedContrast => GetEnhancedContrast();

	public float ClearTypeLevel => GetClearTypeLevel();

	public PixelGeometry PixelGeometry => GetPixelGeometry();

	public RenderingMode RenderingMode => GetRenderingMode();

	public RenderingParams(Factory factory)
	{
		factory.CreateRenderingParams(this);
	}

	public RenderingParams(Factory factory, IntPtr monitorHandle)
	{
		factory.CreateMonitorRenderingParams(monitorHandle, this);
	}

	public RenderingParams(Factory factory, float gamma, float enhancedContrast, float clearTypeLevel, PixelGeometry pixelGeometry, RenderingMode renderingMode)
	{
		factory.CreateCustomRenderingParams(gamma, enhancedContrast, clearTypeLevel, pixelGeometry, renderingMode, this);
	}

	public RenderingParams(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RenderingParams(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new RenderingParams(nativePointer);
		}
		return null;
	}

	internal unsafe float GetGamma()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe float GetEnhancedContrast()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe float GetClearTypeLevel()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe PixelGeometry GetPixelGeometry()
	{
		return ((delegate* unmanaged[Stdcall]<void*, PixelGeometry>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe RenderingMode GetRenderingMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RenderingMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}
}

using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd90698-12e2-11dc-9fed-001143a055f9")]
public class WindowRenderTarget : RenderTarget
{
	public IntPtr Hwnd => GetHwnd();

	public WindowRenderTarget(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator WindowRenderTarget(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new WindowRenderTarget(nativePointer);
		}
		return null;
	}

	public unsafe WindowState CheckWindowState()
	{
		return ((delegate* unmanaged[Stdcall]<void*, WindowState>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)57 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void Resize(Size2 ixelSizeRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)58 * (nint)sizeof(void*))))(_nativePointer, &ixelSizeRef)).CheckError();
	}

	internal unsafe IntPtr GetHwnd()
	{
		return ((delegate* unmanaged[Stdcall]<void*, IntPtr>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)59 * (nint)sizeof(void*))))(_nativePointer);
	}

	public WindowRenderTarget(Factory factory, RenderTargetProperties renderTargetProperties, HwndRenderTargetProperties hwndProperties)
		: base(IntPtr.Zero)
	{
		factory.CreateHwndRenderTarget(ref renderTargetProperties, hwndProperties, this);
	}
}

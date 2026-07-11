using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("1c51bc64-de61-46fd-9899-63a5d8f03950")]
public class DeviceContextRenderTarget : RenderTarget
{
	public DeviceContextRenderTarget(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceContextRenderTarget(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new DeviceContextRenderTarget(nativePointer);
		}
		return null;
	}

	public unsafe void BindDeviceContext(IntPtr hDC, Rectangle subRectRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)57 * (nint)sizeof(void*))))(_nativePointer, (void*)hDC, &subRectRef)).CheckError();
	}

	public DeviceContextRenderTarget(Factory factory, RenderTargetProperties properties)
		: base(IntPtr.Zero)
	{
		factory.CreateDCRenderTarget(ref properties, this);
	}
}

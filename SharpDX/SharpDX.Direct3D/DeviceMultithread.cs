using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D;

[Guid("9B7E4E00-342C-4106-A19F-4F2704F689F0")]
public class DeviceMultithread : ComObject
{
	public DeviceMultithread(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceMultithread(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new DeviceMultithread(nativePointer);
		}
		return null;
	}

	public unsafe void Enter()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void Leave()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe Bool SetMultithreadProtected(Bool bMTProtect)
	{
		return ((delegate* unmanaged[Stdcall]<void*, Bool, Bool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, bMTProtect);
	}

	public unsafe Bool GetMultithreadProtected()
	{
		return ((delegate* unmanaged[Stdcall]<void*, Bool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}
}

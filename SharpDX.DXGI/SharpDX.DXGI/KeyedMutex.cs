using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("9d8e1289-d7b3-465f-8126-250e349af85d")]
public class KeyedMutex : DeviceChild
{
	public KeyedMutex(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator KeyedMutex(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new KeyedMutex(nativePointer);
		}
		return null;
	}

	public unsafe Result Acquire(long key, int dwMilliseconds)
	{
		Result result = ((delegate* unmanaged[Stdcall]<void*, long, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, key, dwMilliseconds);
		result.CheckError();
		return result;
	}

	public unsafe void Release(long key)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, long, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, key)).CheckError();
	}
}

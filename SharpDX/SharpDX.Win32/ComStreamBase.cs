using System;
using System.Runtime.InteropServices;

namespace SharpDX.Win32;

[Guid("0c733a30-2a1c-11ce-ade5-00aa0044773d")]
public class ComStreamBase : ComObjectCallback, IStreamBase, ICallbackable, IDisposable
{
	public ComStreamBase(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ComStreamBase(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new ComStreamBase(nativePointer);
		}
		return null;
	}

	public unsafe int Read(IntPtr vRef, int cb)
	{
		int result = default(int);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)vRef, cb, &result)).CheckError();
		return result;
	}

	public unsafe int Write(IntPtr vRef, int cb)
	{
		int result = default(int);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)vRef, cb, &result)).CheckError();
		return result;
	}
}

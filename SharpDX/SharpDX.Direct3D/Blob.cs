using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D;

[Guid("8BA5FB08-5195-40e2-AC58-0D989C3A0102")]
public class Blob : ComObject
{
	public IntPtr BufferPointer => GetBufferPointer();

	public PointerSize BufferSize => GetBufferSize();

	public Blob(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Blob(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Blob(nativePointer);
		}
		return null;
	}

	internal unsafe IntPtr GetBufferPointer()
	{
		return ((delegate* unmanaged[Stdcall]<void*, IntPtr>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe PointerSize GetBufferSize()
	{
		return ((delegate* unmanaged[Stdcall]<void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}
}

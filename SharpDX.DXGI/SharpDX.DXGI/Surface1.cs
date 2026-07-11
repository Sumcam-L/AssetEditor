using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("4AE63092-6327-4c1b-80AE-BFE12EA32B86")]
public class Surface1 : Surface
{
	public Surface1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Surface1(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Surface1(nativePointer);
		}
		return null;
	}

	public unsafe IntPtr GetDC(Bool discard)
	{
		IntPtr result = default(IntPtr);
		((Result)((delegate* unmanaged[Stdcall]<void*, Bool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, discard, &result)).CheckError();
		return result;
	}

	internal unsafe void ReleaseDC_(Rectangle? dirtyRectRef)
	{
		Rectangle value = default(Rectangle);
		if (dirtyRectRef.HasValue)
		{
			value = dirtyRectRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (dirtyRectRef.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(nativePointer, intPtr)).CheckError();
	}

	public void ReleaseDC()
	{
		ReleaseDC_(null);
	}

	public void ReleaseDC(Rectangle dirtyRect)
	{
		ReleaseDC_(dirtyRect);
	}
}

using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906c1-12e2-11dc-9fed-001143a055f9")]
internal class TessellationSinkNative : ComObjectCallback, TessellationSink, ICallbackable, IDisposable
{
	public TessellationSinkNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TessellationSinkNative(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new TessellationSinkNative(nativePointer);
		}
		return null;
	}

	internal unsafe void AddTriangles_(Triangle[] triangles, int trianglesCount)
	{
		fixed (IntPtr* ptr = triangles)
		{
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr, trianglesCount);
		}
	}

	internal unsafe void Close_()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public void AddTriangles(Triangle[] triangles)
	{
		AddTriangles_(triangles, triangles.Length);
	}

	public void Close()
	{
		Close_();
	}
}

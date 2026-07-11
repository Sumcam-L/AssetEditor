using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("B66F034F-D0E2-40ab-B436-6DE39E321A94")]
public class ColorTransform : BitmapSource
{
	public ColorTransform(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ColorTransform(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new ColorTransform(nativePointer);
		}
		return null;
	}

	public unsafe void Initialize(BitmapSource bitmapSourceRef, ColorContext contextSourceRef, ColorContext contextDestRef, Guid ixelFmtDestRef)
	{
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(bitmapSourceRef?.NativePointer ?? IntPtr.Zero);
		void* intPtr2 = (void*)(contextSourceRef?.NativePointer ?? IntPtr.Zero);
		IntPtr intPtr3 = contextDestRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, (void*)intPtr3, &ixelFmtDestRef)).CheckError();
	}

	public ColorTransform(ImagingFactory factory)
		: base(IntPtr.Zero)
	{
		factory.CreateColorTransformer(this);
	}
}

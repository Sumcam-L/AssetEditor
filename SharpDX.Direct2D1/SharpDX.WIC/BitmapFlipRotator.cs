using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("5009834F-2D6A-41ce-9E1B-17C5AFF7A782")]
public class BitmapFlipRotator : BitmapSource
{
	public BitmapFlipRotator(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapFlipRotator(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapFlipRotator(nativePointer);
		}
		return null;
	}

	public unsafe void Initialize(BitmapSource sourceRef, BitmapTransformOptions options)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = sourceRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, (int)options)).CheckError();
	}

	public BitmapFlipRotator(ImagingFactory factory)
		: base(IntPtr.Zero)
	{
		factory.CreateBitmapFlipRotator(this);
	}
}

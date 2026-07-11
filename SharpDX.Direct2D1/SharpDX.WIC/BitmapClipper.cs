using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("E4FBCF03-223D-4e81-9333-D635556DD1B5")]
public class BitmapClipper : BitmapSource
{
	public BitmapClipper(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapClipper(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapClipper(nativePointer);
		}
		return null;
	}

	internal unsafe void Initialize(BitmapSource sourceRef, IntPtr rectangleRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = sourceRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, (void*)rectangleRef)).CheckError();
	}

	public BitmapClipper(ImagingFactory factory)
		: base(IntPtr.Zero)
	{
		factory.CreateBitmapClipper(this);
	}

	public unsafe void Initialize(BitmapSource sourceRef, Rectangle rectangleRef)
	{
		rectangleRef.MakeXYAndWidthHeight();
		Initialize(sourceRef, new IntPtr(&rectangleRef));
	}
}

using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("00000301-a8f2-4877-ba0a-fd2b6645fb94")]
public class FormatConverter : BitmapSource
{
	public FormatConverter(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FormatConverter(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new FormatConverter(nativePointer);
		}
		return null;
	}

	public unsafe void Initialize(BitmapSource sourceRef, Guid dstFormat, BitmapDitherType dither, Palette paletteRef, double alphaThresholdPercent, BitmapPaletteType paletteTranslate)
	{
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(sourceRef?.NativePointer ?? IntPtr.Zero);
		Guid* num = &dstFormat;
		IntPtr intPtr2 = paletteRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, double, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, intPtr, num, (int)dither, (void*)intPtr2, alphaThresholdPercent, (int)paletteTranslate)).CheckError();
	}

	public unsafe Bool CanConvert(Guid srcPixelFormat, Guid dstPixelFormat)
	{
		Bool result = default(Bool);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, &srcPixelFormat, &dstPixelFormat, &result)).CheckError();
		return result;
	}

	public FormatConverter(FormatConverterInfo converterInfo)
		: base(IntPtr.Zero)
	{
		converterInfo.CreateInstance(this);
	}

	public void Initialize(BitmapSource sourceRef, Guid dstFormat)
	{
		Initialize(sourceRef, dstFormat, BitmapDitherType.None, null, 0.0, BitmapPaletteType.Custom);
	}

	public FormatConverter(ImagingFactory factory)
		: base(IntPtr.Zero)
	{
		factory.CreateFormatConverter(this);
	}
}

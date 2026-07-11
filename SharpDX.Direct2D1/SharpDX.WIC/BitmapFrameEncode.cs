using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("00000105-a8f2-4877-ba0a-fd2b6645fb94")]
public class BitmapFrameEncode : ComObject
{
	public Palette Palette
	{
		set
		{
			SetPalette(value);
		}
	}

	public BitmapSource Thumbnail
	{
		set
		{
			SetThumbnail(value);
		}
	}

	public MetadataQueryWriter MetadataQueryWriter
	{
		get
		{
			GetMetadataQueryWriter(out var metadataQueryWriterOut);
			return metadataQueryWriterOut;
		}
	}

	public BitmapEncoderOptions Options { get; private set; }

	public BitmapFrameEncode(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapFrameEncode(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapFrameEncode(nativePointer);
		}
		return null;
	}

	internal unsafe void Initialize(PropertyBag encoderOptionsRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = encoderOptionsRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}

	public unsafe void SetSize(int width, int height)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, width, height)).CheckError();
	}

	public unsafe void SetResolution(double dpiX, double dpiY)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, double, double, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, dpiX, dpiY)).CheckError();
	}

	public unsafe void SetPixelFormat(ref Guid pixelFormatRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Guid, IntPtr>(ref pixelFormatRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void SetColorContexts(int count, ColorContext[] colorContextOut)
	{
		IntPtr* ptr = null;
		if (colorContextOut != null)
		{
			IntPtr* ptr2 = stackalloc IntPtr[colorContextOut.Length];
			ptr = ptr2;
			for (int i = 0; i < colorContextOut.Length; i++)
			{
				ptr[i] = ((colorContextOut[i] == null) ? IntPtr.Zero : colorContextOut[i].NativePointer);
			}
		}
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, count, ptr)).CheckError();
	}

	internal unsafe void SetColorContexts(int count, ComArray<ColorContext> colorContextOut)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = colorContextOut?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, count, (void*)intPtr)).CheckError();
	}

	internal unsafe void SetPalette(Palette paletteRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = paletteRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}

	internal unsafe void SetThumbnail(BitmapSource thumbnailRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = thumbnailRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}

	internal unsafe void WritePixels(int lineCount, int stride, int bufferSize, IntPtr pixelsRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, lineCount, stride, bufferSize, (void*)pixelsRef)).CheckError();
	}

	internal unsafe void WriteSource(BitmapSource bitmapSourceRef, IntPtr rectangleRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = bitmapSourceRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, (void*)rectangleRef)).CheckError();
	}

	public unsafe void Commit()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	internal unsafe void GetMetadataQueryWriter(out MetadataQueryWriter metadataQueryWriterOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &zero);
		metadataQueryWriterOut = ((zero == IntPtr.Zero) ? null : new MetadataQueryWriter(zero));
		result.CheckError();
	}

	public BitmapFrameEncode(BitmapEncoder encoder)
	{
		Options = new BitmapEncoderOptions(IntPtr.Zero);
		encoder.CreateNewFrame(this, Options);
	}

	public void Initialize()
	{
		Initialize(Options);
	}

	public void SetColorContexts(ColorContext[] colorContextOut)
	{
		SetColorContexts((colorContextOut != null) ? colorContextOut.Length : 0, colorContextOut);
	}

	public void WritePixels(int lineCount, DataRectangle buffer, int totalSizeInBytes = 0)
	{
		WritePixels(lineCount, buffer.DataPointer, buffer.Pitch, totalSizeInBytes);
	}

	public void WritePixels(int lineCount, IntPtr buffer, int rowStride, int totalSizeInBytes = 0)
	{
		if (totalSizeInBytes == 0)
		{
			totalSizeInBytes = lineCount * rowStride;
		}
		WritePixels(lineCount, rowStride, totalSizeInBytes, buffer);
	}

	public unsafe void WritePixels<T>(int lineCount, int stride, T[] pixelBuffer) where T : struct
	{
		if (lineCount * stride > Utilities.SizeOf<T>() * pixelBuffer.Length)
		{
			throw new ArgumentException("lineCount * stride must be <= to sizeof(pixelBuffer)");
		}
		int bufferSize = lineCount * stride;
		fixed (T* ptr = &pixelBuffer[0])
		{
			WritePixels(lineCount, stride, bufferSize, (IntPtr)ptr);
		}
	}

	public void WriteSource(BitmapSource bitmapSource)
	{
		WriteSource(bitmapSource, IntPtr.Zero);
	}

	public unsafe void WriteSource(BitmapSource bitmapSourceRef, Rectangle rectangleRef)
	{
		rectangleRef.MakeXYAndWidthHeight();
		WriteSource(bitmapSourceRef, new IntPtr(&rectangleRef));
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Options.Dispose();
			Options = null;
		}
		base.Dispose(disposing);
	}
}

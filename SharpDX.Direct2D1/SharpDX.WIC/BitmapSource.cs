using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("00000120-a8f2-4877-ba0a-fd2b6645fb94")]
public class BitmapSource : ComObject
{
	public Guid PixelFormat
	{
		get
		{
			GetPixelFormat(out var pixelFormatRef);
			return pixelFormatRef;
		}
	}

	public Size2 Size
	{
		get
		{
			GetSize(out var widthRef, out var heightRef);
			return new Size2(widthRef, heightRef);
		}
	}

	public BitmapSource(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapSource(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapSource(nativePointer);
		}
		return null;
	}

	internal unsafe void GetSize(out int widthRef, out int heightRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref widthRef))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref heightRef))
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetPixelFormat(out Guid pixelFormatRef)
	{
		pixelFormatRef = default(Guid);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Guid, IntPtr>(ref pixelFormatRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public unsafe void GetResolution(out double dpiXRef, out double dpiYRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<double, IntPtr>(ref dpiXRef))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<double, IntPtr>(ref dpiYRef))
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void CopyPalette(Palette paletteRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = paletteRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}

	internal unsafe void CopyPixels(IntPtr rectangleRef, int stride, int bufferSize, IntPtr bufferRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)rectangleRef, stride, bufferSize, (void*)bufferRef)).CheckError();
	}

	public unsafe void CopyPixels(Rectangle rectangle, int stride, DataPointer dataPointer)
	{
		Rectangle rectangle2 = rectangle;
		rectangle2.MakeXYAndWidthHeight();
		CopyPixels(new IntPtr(&rectangle2), stride, dataPointer.Size, dataPointer.Pointer);
	}

	public void CopyPixels(int stride, DataPointer dataPointer)
	{
		CopyPixels(IntPtr.Zero, stride, dataPointer.Size, dataPointer.Pointer);
	}

	public void CopyPixels(int stride, IntPtr dataPointer, int size)
	{
		CopyPixels(IntPtr.Zero, stride, size, dataPointer);
	}

	public unsafe void CopyPixels<T>(Rectangle rectangle, T[] output) where T : struct
	{
		if (rectangle.Width * rectangle.Height != output.Length)
		{
			throw new ArgumentException("output.Length must be equal to Width * Height");
		}
		Rectangle rectangle2 = rectangle;
		rectangle2.MakeXYAndWidthHeight();
		IntPtr rectangleRef = new IntPtr(&rectangle2);
		int stride = rectangle.Width * Utilities.SizeOf<T>();
		int bufferSize = output.Length * Utilities.SizeOf<T>();
		fixed (T* ptr = &output[0])
		{
			CopyPixels(rectangleRef, stride, bufferSize, (IntPtr)ptr);
		}
	}

	public unsafe void CopyPixels<T>(T[] output) where T : struct
	{
		Size2 size = Size;
		if (size.Width * size.Height != output.Length)
		{
			throw new ArgumentException("output.Length must be equal to Width * Height");
		}
		IntPtr zero = IntPtr.Zero;
		int stride = Size.Width * Utilities.SizeOf<T>();
		int bufferSize = output.Length * Utilities.SizeOf<T>();
		fixed (T* ptr = &output[0])
		{
			CopyPixels(zero, stride, bufferSize, (IntPtr)ptr);
		}
	}

	public unsafe void CopyPixels(Rectangle rectangle, byte[] output, int stride)
	{
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		if (stride <= 0)
		{
			throw new ArgumentOutOfRangeException("stride", "Must be > 0");
		}
		if (output.Length % stride != 0)
		{
			throw new ArgumentException("output.Length must be a modulo of stride");
		}
		Rectangle rectangle2 = rectangle;
		rectangle2.MakeXYAndWidthHeight();
		IntPtr rectangleRef = new IntPtr(&rectangle2);
		int bufferSize = output.Length;
		fixed (byte* ptr = &output[0])
		{
			CopyPixels(rectangleRef, stride, bufferSize, (IntPtr)ptr);
		}
	}

	public unsafe void CopyPixels(byte[] output, int stride)
	{
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		if (stride <= 0)
		{
			throw new ArgumentOutOfRangeException("stride", "Must be > 0");
		}
		if (output.Length % stride != 0)
		{
			throw new ArgumentException("output.Length must be a modulo of stride");
		}
		IntPtr zero = IntPtr.Zero;
		int bufferSize = output.Length;
		fixed (byte* ptr = &output[0])
		{
			CopyPixels(zero, stride, bufferSize, (IntPtr)ptr);
		}
	}
}

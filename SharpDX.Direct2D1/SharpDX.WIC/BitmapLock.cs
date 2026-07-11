using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("00000123-a8f2-4877-ba0a-fd2b6645fb94")]
public class BitmapLock : ComObject
{
	public int Stride
	{
		get
		{
			GetStride(out var strideRef);
			return strideRef;
		}
	}

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

	public DataRectangle Data
	{
		get
		{
			int bufferSizeRef;
			IntPtr dataPointer = GetDataPointer(out bufferSizeRef);
			return new DataRectangle(dataPointer, Stride);
		}
	}

	public BitmapLock(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapLock(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapLock(nativePointer);
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

	internal unsafe void GetStride(out int strideRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref strideRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe IntPtr GetDataPointer(out int bufferSizeRef)
	{
		Result result;
		IntPtr result2 = default(IntPtr);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref bufferSizeRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr, &result2);
		}
		result.CheckError();
		return result2;
	}

	internal unsafe void GetPixelFormat(out Guid pixelFormatRef)
	{
		pixelFormatRef = default(Guid);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Guid, IntPtr>(ref pixelFormatRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}
}

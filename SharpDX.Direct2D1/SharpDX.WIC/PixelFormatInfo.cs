using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("E8EDA601-3D48-431a-AB44-69059BE88BBE")]
public class PixelFormatInfo : ComponentInfo
{
	public Guid FormatGUID
	{
		get
		{
			GetFormatGUID(out var formatRef);
			return formatRef;
		}
	}

	public ColorContext ColorContext
	{
		get
		{
			GetColorContext(out var colorContextOut);
			return colorContextOut;
		}
	}

	public int BitsPerPixel
	{
		get
		{
			GetBitsPerPixel(out var bitsPerPixelRef);
			return bitsPerPixelRef;
		}
	}

	public int ChannelCount
	{
		get
		{
			GetChannelCount(out var channelCountRef);
			return channelCountRef;
		}
	}

	public PixelFormatInfo(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator PixelFormatInfo(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new PixelFormatInfo(nativePointer);
		}
		return null;
	}

	internal unsafe void GetFormatGUID(out Guid formatRef)
	{
		formatRef = default(Guid);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Guid, IntPtr>(ref formatRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetColorContext(out ColorContext colorContextOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &zero);
		colorContextOut = ((zero == IntPtr.Zero) ? null : new ColorContext(zero));
		result.CheckError();
	}

	internal unsafe void GetBitsPerPixel(out int bitsPerPixelRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref bitsPerPixelRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetChannelCount(out int channelCountRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref channelCountRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetChannelMask(int channelIndex, int maskBuffer, IntPtr maskBufferRef, out int actualRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref actualRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, channelIndex, maskBuffer, (void*)maskBufferRef, ptr);
		}
		result.CheckError();
	}

	public unsafe byte[] GetChannelMask(int channelIndex)
	{
		int actualRef = 0;
		GetChannelMask(channelIndex, actualRef, IntPtr.Zero, out actualRef);
		if (actualRef == 0)
		{
			return new byte[0];
		}
		byte[] array = new byte[actualRef];
		fixed (IntPtr* ptr = array)
		{
			GetChannelMask(channelIndex, actualRef, (IntPtr)ptr, out actualRef);
		}
		return array;
	}
}

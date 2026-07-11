using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("D8CD007F-D08F-4191-9BFC-236EA7F0E4B5")]
public class BitmapDecoderInfo : BitmapCodecInfo
{
	public BitmapPattern[] Patterns
	{
		get
		{
			int patternsActualRef = 0;
			int atternCountRef = 0;
			GetPatterns(0, null, out atternCountRef, out patternsActualRef);
			if (patternsActualRef == 0)
			{
				return new BitmapPattern[0];
			}
			atternCountRef = patternsActualRef;
			BitmapPattern[] array = new BitmapPattern[patternsActualRef];
			GetPatterns(atternCountRef, array, out atternCountRef, out patternsActualRef);
			return array;
		}
	}

	public BitmapDecoderInfo(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapDecoderInfo(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapDecoderInfo(nativePointer);
		}
		return null;
	}

	internal unsafe void GetPatterns(int sizePatterns, BitmapPattern[] patternsRef, out int atternCountRef, out int patternsActualRef)
	{
		Result result;
		fixed (IntPtr* ptr = patternsRef)
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref atternCountRef))
			{
				fixed (IntPtr* ptr3 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref patternsActualRef))
				{
					result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, sizePatterns, ptr, ptr2, ptr3);
				}
			}
		}
		result.CheckError();
	}

	internal unsafe Bool MatchesPattern_(IntPtr streamRef)
	{
		Bool result = default(Bool);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, (void*)streamRef, &result)).CheckError();
		return result;
	}

	internal unsafe void CreateInstance(BitmapDecoder bitmapDecoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, &zero);
		bitmapDecoderOut.NativePointer = zero;
		result.CheckError();
	}

	public bool MatchesPattern(IStream stream)
	{
		return MatchesPattern_(ComStream.ToIntPtr(stream));
	}
}

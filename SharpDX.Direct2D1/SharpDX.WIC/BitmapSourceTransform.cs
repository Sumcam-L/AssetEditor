using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("3B16811B-6A43-4ec9-B713-3D5A0C13B940")]
public class BitmapSourceTransform : ComObject
{
	public BitmapSourceTransform(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapSourceTransform(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapSourceTransform(nativePointer);
		}
		return null;
	}

	internal unsafe void CopyPixels(IntPtr rectangleRef, int width, int height, Guid? guidDstFormatRef, BitmapTransformOptions dstTransform, int nStride, int bufferSize, IntPtr bufferRef)
	{
		Guid value = default(Guid);
		if (guidDstFormatRef.HasValue)
		{
			value = guidDstFormatRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)rectangleRef;
		void* intPtr2 = (guidDstFormatRef.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(nativePointer, intPtr, width, height, intPtr2, (int)dstTransform, nStride, bufferSize, (void*)bufferRef)).CheckError();
	}

	internal unsafe void GetClosestSize(ref int widthRef, ref int heightRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref widthRef))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref heightRef))
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void GetClosestPixelFormat(ref Guid guidDstFormatRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Guid, IntPtr>(ref guidDstFormatRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public unsafe void IsSupportingTransform(BitmapTransformOptions dstTransform, out Bool fIsSupportedRef)
	{
		fIsSupportedRef = default(Bool);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref fIsSupportedRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (int)dstTransform, ptr);
		}
		result.CheckError();
	}

	public void CopyPixels(int width, int height, int stride, DataStream output)
	{
		CopyPixels(IntPtr.Zero, width, height, null, BitmapTransformOptions.Rotate0, stride, (int)(output.Length - output.Position), output.PositionPointer);
	}

	public void CopyPixels(int width, int height, BitmapTransformOptions dstTransform, int stride, DataStream output)
	{
		CopyPixels(IntPtr.Zero, width, height, null, dstTransform, stride, (int)(output.Length - output.Position), output.PositionPointer);
	}

	public void CopyPixels(int width, int height, Guid guidDstFormat, BitmapTransformOptions dstTransform, int stride, DataStream output)
	{
		CopyPixels(IntPtr.Zero, width, height, guidDstFormat, dstTransform, stride, (int)(output.Length - output.Position), output.PositionPointer);
	}

	public unsafe void CopyPixels(Rectangle rectangle, int width, int height, Guid guidDstFormat, BitmapTransformOptions dstTransform, int stride, DataStream output)
	{
		rectangle.MakeXYAndWidthHeight();
		CopyPixels(new IntPtr(&rectangle), width, height, guidDstFormat, dstTransform, stride, (int)(output.Length - output.Position), output.PositionPointer);
	}

	public void GetClosestSize(ref Size2 size)
	{
		int widthRef = size.Width;
		int heightRef = size.Height;
		GetClosestSize(ref widthRef, ref heightRef);
		size.Width = widthRef;
		size.Height = heightRef;
	}
}

using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("94C9B4EE-A09F-4f92-8A1E-4A9BCE7E76FB")]
public class BitmapEncoderInfo : BitmapCodecInfo
{
	public BitmapEncoderInfo(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapEncoderInfo(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapEncoderInfo(nativePointer);
		}
		return null;
	}

	public unsafe void CreateInstance(out BitmapEncoder bitmapEncoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, &zero);
		bitmapEncoderOut = ((zero == IntPtr.Zero) ? null : new BitmapEncoder(zero));
		result.CheckError();
	}
}

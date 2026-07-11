using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("55f1112b-1dc2-4b3c-9541-f46894ed85b6")]
public class Typography : ComObject
{
	public int FontFeatureCount => GetFontFeatureCount();

	public Typography(Factory factory)
	{
		factory.CreateTypography(this);
	}

	public Typography(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Typography(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Typography(nativePointer);
		}
		return null;
	}

	public unsafe void AddFontFeature(FontFeature fontFeature)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, FontFeature, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, fontFeature)).CheckError();
	}

	internal unsafe int GetFontFeatureCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe FontFeature GetFontFeature(int fontFeatureIndex)
	{
		FontFeature result = default(FontFeature);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, fontFeatureIndex, &result)).CheckError();
		return result;
	}
}

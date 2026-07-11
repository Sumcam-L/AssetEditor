using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("1a0d8438-1d97-4ec1-aef9-a2fb86ed6acb")]
public class FontList : ComObject
{
	public FontCollection FontCollection
	{
		get
		{
			GetFontCollection(out var fontCollection);
			return fontCollection;
		}
	}

	public int FontCount => GetFontCount();

	public FontList(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontList(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new FontList(nativePointer);
		}
		return null;
	}

	internal unsafe void GetFontCollection(out FontCollection fontCollection)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, &zero);
		fontCollection = ((zero == IntPtr.Zero) ? null : new FontCollection(zero));
		result.CheckError();
	}

	internal unsafe int GetFontCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe Font GetFont(int index)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, index, &zero);
		Font result2 = ((zero == IntPtr.Zero) ? null : new Font(zero));
		result.CheckError();
		return result2;
	}
}

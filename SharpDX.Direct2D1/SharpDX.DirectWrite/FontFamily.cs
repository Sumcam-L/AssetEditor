using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("da20d8ef-812a-4c43-9802-62ec4abd7add")]
public class FontFamily : FontList
{
	public LocalizedStrings FamilyNames
	{
		get
		{
			GetFamilyNames(out var names);
			return names;
		}
	}

	public FontFamily(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFamily(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new FontFamily(nativePointer);
		}
		return null;
	}

	internal unsafe void GetFamilyNames(out LocalizedStrings names)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, &zero);
		names = ((zero == IntPtr.Zero) ? null : new LocalizedStrings(zero));
		result.CheckError();
	}

	public unsafe Font GetFirstMatchingFont(FontWeight weight, FontStretch stretch, FontStyle style)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (int)weight, (int)stretch, (int)style, &zero);
		Font result2 = ((zero == IntPtr.Zero) ? null : new Font(zero));
		result.CheckError();
		return result2;
	}

	public unsafe FontList GetMatchingFonts(FontWeight weight, FontStretch stretch, FontStyle style)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)weight, (int)stretch, (int)style, &zero);
		FontList result2 = ((zero == IntPtr.Zero) ? null : new FontList(zero));
		result.CheckError();
		return result2;
	}
}

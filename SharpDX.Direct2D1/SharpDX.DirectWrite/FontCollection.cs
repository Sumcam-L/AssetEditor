using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("a84cee02-3eea-4eee-a827-87c1a02a0fcc")]
public class FontCollection : ComObject
{
	public int FontFamilyCount => GetFontFamilyCount();

	public FontCollection(Factory factory, FontCollectionLoader collectionLoader, DataPointer collectionKey)
	{
		factory.CreateCustomFontCollection_(FontCollectionLoaderShadow.ToIntPtr(collectionLoader), collectionKey.Pointer, collectionKey.Size, this);
	}

	public FontCollection(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontCollection(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new FontCollection(nativePointer);
		}
		return null;
	}

	internal unsafe int GetFontFamilyCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe FontFamily GetFontFamily(int index)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, index, &zero);
		FontFamily result2 = ((zero == IntPtr.Zero) ? null : new FontFamily(zero));
		result.CheckError();
		return result2;
	}

	public unsafe Bool FindFamilyName(string familyName, out int index)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(familyName);
		Bool result = default(Bool);
		Result result2;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref index))
		{
			result2 = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)intPtr, ptr, &result);
		}
		Marshal.FreeHGlobal(intPtr);
		result2.CheckError();
		return result;
	}

	public unsafe Font GetFontFromFontFace(FontFace fontFace)
	{
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		IntPtr intPtr = fontFace?.NativePointer ?? IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, &zero);
		Font result2 = ((zero == IntPtr.Zero) ? null : new Font(zero));
		result.CheckError();
		return result2;
	}
}

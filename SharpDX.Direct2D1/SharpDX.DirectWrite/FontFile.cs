using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("739d886a-cef5-47dc-8769-1a8b41bebbb0")]
public class FontFile : ComObject
{
	private FontFileLoaderShadow fontLoaderShadow;

	public FontFileLoader Loader
	{
		get
		{
			if (fontLoaderShadow != null)
			{
				return (FontFileLoader)fontLoaderShadow.Callback;
			}
			GetLoader(out var fontFileLoader);
			return fontFileLoader;
		}
	}

	public FontFile(Factory factory, string filePath)
		: this(factory, filePath, null)
	{
	}

	public FontFile(Factory factory, string filePath, long? lastWriteTime)
	{
		factory.CreateFontFileReference(filePath, lastWriteTime, this);
	}

	public FontFile(Factory factory, IntPtr fontFileReferenceKey, int fontFileReferenceKeySize, FontFileLoader fontFileLoader)
	{
		factory.CreateCustomFontFileReference_(fontFileReferenceKey, fontFileReferenceKeySize, FontFileLoaderShadow.ToIntPtr(fontFileLoader), this);
	}

	public unsafe DataPointer GetReferenceKey()
	{
		IntPtr pointer = default(IntPtr);
		GetReferenceKey(new IntPtr(&pointer), out var fontFileReferenceKeySize);
		return new DataPointer(pointer, fontFileReferenceKeySize);
	}

	public FontFile(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFile(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new FontFile(nativePointer);
		}
		return null;
	}

	internal unsafe void GetReferenceKey(IntPtr fontFileReferenceKey, out int fontFileReferenceKeySize)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref fontFileReferenceKeySize))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileReferenceKey, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetLoader(out FontFileLoader fontFileLoader)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &zero);
		fontFileLoader = ((zero == IntPtr.Zero) ? null : new FontFileLoaderNative(zero));
		result.CheckError();
	}

	public unsafe void Analyze(out Bool isSupportedFontType, out FontFileType fontFileType, out FontFaceType fontFaceType, out int numberOfFaces)
	{
		isSupportedFontType = default(Bool);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref isSupportedFontType))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<FontFileType, IntPtr>(ref fontFileType))
			{
				fixed (IntPtr* ptr3 = &System.Runtime.CompilerServices.Unsafe.As<FontFaceType, IntPtr>(ref fontFaceType))
				{
					fixed (IntPtr* ptr4 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref numberOfFaces))
					{
						result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2, ptr3, ptr4);
					}
				}
			}
		}
		result.CheckError();
	}
}

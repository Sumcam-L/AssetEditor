using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("727cad4e-d6af-4c9e-8a08-d695b11caa49")]
public class FontFileLoaderNative : ComObjectCallback, FontFileLoader, ICallbackable, IDisposable
{
	public FontFileStream CreateStreamFromKey(DataPointer fontFileReferenceKey)
	{
		CreateStreamFromKey_(fontFileReferenceKey.Pointer, fontFileReferenceKey.Size, out var fontFileStream);
		return fontFileStream;
	}

	public FontFileLoaderNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFileLoaderNative(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new FontFileLoaderNative(nativePointer);
		}
		return null;
	}

	internal unsafe void CreateStreamFromKey_(IntPtr fontFileReferenceKey, int fontFileReferenceKeySize, out FontFileStream fontFileStream)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileReferenceKey, fontFileReferenceKeySize, &zero);
		fontFileStream = ((zero == IntPtr.Zero) ? null : new FontFileStreamNative(zero));
		result.CheckError();
	}
}

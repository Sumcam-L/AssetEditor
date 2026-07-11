using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("b2d9f3ec-c9fe-4a11-a2ec-d86208f7c0a2")]
public class LocalFontFileLoader : FontFileLoaderNative
{
	public unsafe string GetFilePath(DataPointer referenceKey)
	{
		if (referenceKey.IsEmpty)
		{
			throw new ArgumentNullException("referenceKey", "DatePointer cannot be null");
		}
		int filePathLengthFromKey = GetFilePathLengthFromKey(referenceKey.Pointer, referenceKey.Size);
		char[] array = new char[filePathLengthFromKey + 1];
		fixed (IntPtr* value = array)
		{
			GetFilePathFromKey(referenceKey.Pointer, referenceKey.Size, new IntPtr(value), filePathLengthFromKey + 1);
		}
		return new string(array, 0, filePathLengthFromKey);
	}

	public DateTime GetLastWriteTime(DataPointer referenceKey)
	{
		if (referenceKey.IsEmpty)
		{
			throw new ArgumentNullException("referenceKey", "DatePointer cannot be null");
		}
		long lastWriteTimeFromKey = GetLastWriteTimeFromKey(referenceKey.Pointer, referenceKey.Size);
		return DateTime.FromFileTime(lastWriteTimeFromKey);
	}

	public LocalFontFileLoader(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator LocalFontFileLoader(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new LocalFontFileLoader(nativePointer);
		}
		return null;
	}

	internal unsafe int GetFilePathLengthFromKey(IntPtr fontFileReferenceKey, int fontFileReferenceKeySize)
	{
		int result = default(int);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileReferenceKey, fontFileReferenceKeySize, &result)).CheckError();
		return result;
	}

	internal unsafe void GetFilePathFromKey(IntPtr fontFileReferenceKey, int fontFileReferenceKeySize, IntPtr filePath, int filePathSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileReferenceKey, fontFileReferenceKeySize, (void*)filePath, filePathSize)).CheckError();
	}

	internal unsafe long GetLastWriteTimeFromKey(IntPtr fontFileReferenceKey, int fontFileReferenceKeySize)
	{
		long result = default(long);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileReferenceKey, fontFileReferenceKeySize, &result)).CheckError();
		return result;
	}
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("08256209-099a-4b34-b86d-c22b110e7771")]
public class LocalizedStrings : ComObject
{
	public int Count => GetCount();

	public unsafe string GetLocaleName(int index)
	{
		GetLocaleNameLength(index, out var length);
		char* value = stackalloc char[length + 1];
		GetLocaleName(index, new IntPtr(value), length + 1);
		return new string(value, 0, length);
	}

	public unsafe string GetString(int index)
	{
		GetStringLength(index, out var length);
		char* value = stackalloc char[length + 1];
		GetString(index, new IntPtr(value), length + 1);
		return new string(value, 0, length);
	}

	public LocalizedStrings(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator LocalizedStrings(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new LocalizedStrings(nativePointer);
		}
		return null;
	}

	internal unsafe int GetCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe Bool FindLocaleName(string localeName, out int index)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(localeName);
		Bool result = default(Bool);
		Result result2;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref index))
		{
			result2 = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)intPtr, ptr, &result);
		}
		Marshal.FreeHGlobal(intPtr);
		result2.CheckError();
		return result;
	}

	internal unsafe void GetLocaleNameLength(int index, out int length)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref length))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, index, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetLocaleName(int index, IntPtr localeName, int size)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, index, (void*)localeName, size)).CheckError();
	}

	internal unsafe void GetStringLength(int index, out int length)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref length))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, index, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetString(int index, IntPtr stringBuffer, int size)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, index, (void*)stringBuffer, size)).CheckError();
	}
}

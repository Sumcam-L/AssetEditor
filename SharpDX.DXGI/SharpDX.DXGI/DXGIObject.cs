using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("aec22fb8-76f3-4639-9be0-28eb43a67a2e")]
public class DXGIObject : ComObject
{
	public T GetParent<T>() where T : ComObject
	{
		GetParent(Utilities.GetGuidFromType(typeof(T)), out var parentOut);
		return CppObject.FromPointer<T>(parentOut);
	}

	public DXGIObject(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DXGIObject(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new DXGIObject(nativePointer);
		}
		return null;
	}

	public unsafe void SetPrivateData(Guid name, int dataSize, IntPtr dataRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, &name, dataSize, (void*)dataRef)).CheckError();
	}

	public unsafe void SetPrivateDataInterface(Guid name, ComObject unknownRef)
	{
		void* nativePointer = _nativePointer;
		Guid* num = &name;
		IntPtr intPtr = unknownRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(nativePointer, num, (void*)intPtr)).CheckError();
	}

	public unsafe Result GetPrivateData(Guid name, ref int dataSizeRef, IntPtr dataRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref dataSizeRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &name, ptr, (void*)dataRef);
		}
		return result;
	}

	public unsafe void GetParent(Guid riid, out IntPtr parentOut)
	{
		Result result;
		fixed (IntPtr* ptr = &parentOut)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, &riid, ptr);
		}
		result.CheckError();
	}
}

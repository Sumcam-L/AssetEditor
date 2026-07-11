using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("035f3ab4-482e-4e50-b41f-8a7f8bd8960b")]
public class Resource : DeviceChild
{
	public IntPtr SharedHandle
	{
		get
		{
			GetSharedHandle(out var sharedHandleRef);
			return sharedHandleRef;
		}
	}

	public int Usage
	{
		get
		{
			GetUsage(out var usageRef);
			return usageRef;
		}
	}

	public int EvictionPriority
	{
		get
		{
			GetEvictionPriority(out var evictionPriorityRef);
			return evictionPriorityRef;
		}
		set
		{
			SetEvictionPriority(value);
		}
	}

	public Resource(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Resource(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Resource(nativePointer);
		}
		return null;
	}

	internal unsafe void GetSharedHandle(out IntPtr sharedHandleRef)
	{
		Result result;
		fixed (IntPtr* ptr = &sharedHandleRef)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetUsage(out int usageRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref usageRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void SetEvictionPriority(int evictionPriority)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, evictionPriority)).CheckError();
	}

	internal unsafe void GetEvictionPriority(out int evictionPriorityRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref evictionPriorityRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}
}

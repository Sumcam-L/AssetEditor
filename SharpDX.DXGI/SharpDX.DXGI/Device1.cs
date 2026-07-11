using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("77db970f-6276-48ba-ba28-070143b4392c")]
public class Device1 : Device
{
	public int MaximumFrameLatency
	{
		get
		{
			GetMaximumFrameLatency(out var maxLatencyRef);
			return maxLatencyRef;
		}
		set
		{
			SetMaximumFrameLatency(value);
		}
	}

	public Device1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device1(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Device1(nativePointer);
		}
		return null;
	}

	internal unsafe void SetMaximumFrameLatency(int maxLatency)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, maxLatency)).CheckError();
	}

	internal unsafe void GetMaximumFrameLatency(out int maxLatencyRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref maxLatencyRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}
}

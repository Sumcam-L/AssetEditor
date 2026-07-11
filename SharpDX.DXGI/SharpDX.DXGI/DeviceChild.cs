using System;
using System.Runtime.InteropServices;
using SharpDX.Direct3D;

namespace SharpDX.DXGI;

[Guid("3d3e0379-f9de-4d58-bb6c-18d62992f1a6")]
public class DeviceChild : DXGIObject
{
	public unsafe string DebugName
	{
		get
		{
			byte* ptr = stackalloc byte[1024];
			int dataSizeRef = 1023;
			if (GetPrivateData(CommonGuid.DebugObjectName, ref dataSizeRef, new IntPtr(ptr)).Failure)
			{
				return string.Empty;
			}
			ptr[dataSizeRef] = 0;
			return Marshal.PtrToStringAnsi(new IntPtr(ptr));
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				SetPrivateData(CommonGuid.DebugObjectName, 0, IntPtr.Zero);
				return;
			}
			IntPtr dataRef = Utilities.StringToHGlobalAnsi(value);
			SetPrivateData(CommonGuid.DebugObjectName, value.Length, dataRef);
		}
	}

	public T GetDevice<T>() where T : ComObject
	{
		GetDevice(Utilities.GetGuidFromType(typeof(T)), out var deviceOut);
		return CppObject.FromPointer<T>(deviceOut);
	}

	public DeviceChild(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceChild(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new DeviceChild(nativePointer);
		}
		return null;
	}

	public unsafe void GetDevice(Guid riid, out IntPtr deviceOut)
	{
		Result result;
		fixed (IntPtr* ptr = &deviceOut)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &riid, ptr);
		}
		result.CheckError();
	}
}

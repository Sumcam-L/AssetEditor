using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("95c75a6e-3e8c-4ec2-85a8-aebcc551e59b")]
internal class DevelopRawNotificationCallback : ComObject
{
	public DevelopRawNotificationCallback(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DevelopRawNotificationCallback(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new DevelopRawNotificationCallback(nativePointer);
		}
		return null;
	}

	public unsafe void Notify(int notificationMask)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, notificationMask)).CheckError();
	}
}

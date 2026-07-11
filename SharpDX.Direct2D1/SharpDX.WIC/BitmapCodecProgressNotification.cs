using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("64C1024E-C3CF-4462-8078-88C2B11C46D9")]
internal class BitmapCodecProgressNotification : ComObject
{
	public BitmapCodecProgressNotification(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapCodecProgressNotification(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapCodecProgressNotification(nativePointer);
		}
		return null;
	}

	internal unsafe void RegisterProgressNotification(FunctionCallback fnProgressNotificationRef, IntPtr vDataRef, int progressFlags)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, fnProgressNotificationRef, (void*)vDataRef, progressFlags)).CheckError();
	}
}

using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("4776F9CD-9517-45FA-BF24-E89C5EC5C60C")]
internal class ProgressCallback : ComObject
{
	public ProgressCallback(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ProgressCallback(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new ProgressCallback(nativePointer);
		}
		return null;
	}

	public unsafe void Notify(int frameNum, ProgressOperation operation, double dblProgress)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, double, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, frameNum, (int)operation, dblProgress)).CheckError();
	}
}

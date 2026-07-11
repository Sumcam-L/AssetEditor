using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX.IO;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("135FF860-22B7-4ddf-B0F6-218F4F299A43")]
public class WICStream : ComStream
{
	private ComStreamProxy streamProxy;

	public WICStream(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator WICStream(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new WICStream(nativePointer);
		}
		return null;
	}

	internal unsafe void InitializeFromIStream_(IntPtr streamRef)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, (void*)streamRef)).CheckError();
	}

	internal unsafe void InitializeFromFilename(string fileName, int desiredAccess)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(fileName);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, (void*)intPtr, desiredAccess);
		Marshal.FreeHGlobal(intPtr);
		result.CheckError();
	}

	internal unsafe void InitializeFromMemory(IntPtr bufferRef, int bufferSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, (void*)bufferRef, bufferSize)).CheckError();
	}

	internal unsafe void InitializeFromIStreamRegion_(IntPtr streamRef, long ulOffset, long ulMaxSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, long, long, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, (void*)streamRef, ulOffset, ulMaxSize)).CheckError();
	}

	public WICStream(ImagingFactory factory, string fileName, NativeFileAccess fileAccess)
		: base(IntPtr.Zero)
	{
		factory.CreateStream(this);
		InitializeFromFilename(fileName, (int)fileAccess);
	}

	public WICStream(ImagingFactory factory, Stream stream)
		: base(IntPtr.Zero)
	{
		factory.CreateStream(this);
		streamProxy = new ComStreamProxy(stream);
		IntPtr streamRef = ComStreamShadow.ToIntPtr(streamProxy);
		InitializeFromIStream_(streamRef);
	}

	public WICStream(ImagingFactory factory, DataPointer dataStream)
		: base(IntPtr.Zero)
	{
		factory.CreateStream(this);
		InitializeFromMemory(dataStream.Pointer, dataStream.Size);
	}

	protected override void Dispose(bool disposing)
	{
		if (streamProxy != null)
		{
			streamProxy.Dispose();
			streamProxy = null;
		}
		base.Dispose(disposing);
	}
}

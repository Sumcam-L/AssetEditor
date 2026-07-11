using System;
using System.Runtime.InteropServices;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("A721791A-0DEF-4d06-BD91-2118BF1DB10B")]
public class MetadataQueryWriter : MetadataQueryReader
{
	public MetadataQueryWriter(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator MetadataQueryWriter(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new MetadataQueryWriter(nativePointer);
		}
		return null;
	}

	internal unsafe void SetMetadataByName(string name, IntPtr varValueRef)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(name);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)intPtr, (void*)varValueRef);
		Marshal.FreeHGlobal(intPtr);
		result.CheckError();
	}

	public unsafe void RemoveMetadataByName(string name)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(name);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)intPtr);
		Marshal.FreeHGlobal(intPtr);
		result.CheckError();
	}

	public MetadataQueryWriter(ImagingFactory factory, Guid guidMetadataFormat)
		: base(IntPtr.Zero)
	{
		factory.CreateQueryWriter(guidMetadataFormat, null, this);
	}

	public MetadataQueryWriter(ImagingFactory factory, Guid guidMetadataFormat, Guid guidVendorRef)
		: base(IntPtr.Zero)
	{
		factory.CreateQueryWriter(guidMetadataFormat, guidVendorRef, this);
	}

	public MetadataQueryWriter(ImagingFactory factory, MetadataQueryReader metadataQueryReader)
		: base(IntPtr.Zero)
	{
		factory.CreateQueryWriterFromReader(metadataQueryReader, null, this);
	}

	public MetadataQueryWriter(ImagingFactory factory, MetadataQueryReader metadataQueryReader, Guid guidVendorRef)
		: base(IntPtr.Zero)
	{
		factory.CreateQueryWriterFromReader(metadataQueryReader, guidVendorRef, this);
	}

	public unsafe void SetMetadataByName(string name, object value)
	{
		byte* ptr = stackalloc byte[512];
		Variant* ptr2 = (Variant*)ptr;
		ptr2->Value = value;
		SetMetadataByName(name, (IntPtr)ptr);
	}
}

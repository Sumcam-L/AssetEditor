using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("DC2BB46D-3F07-481E-8625-220C4AEDBB33")]
internal class EnumMetadataItem : ComObject
{
	public EnumMetadataItem(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator EnumMetadataItem(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new EnumMetadataItem(nativePointer);
		}
		return null;
	}

	public unsafe void Skip(int celt)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, celt)).CheckError();
	}

	public unsafe void Reset()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void Clone(out EnumMetadataItem enumMetadataItemOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, &zero);
		enumMetadataItemOut = ((zero == IntPtr.Zero) ? null : new EnumMetadataItem(zero));
		result.CheckError();
	}
}

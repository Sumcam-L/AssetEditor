using System;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("29038f61-3839-4626-91fd-086879011a05")]
public class Adapter1 : Adapter
{
	public AdapterDescription1 Description1
	{
		get
		{
			GetDescription1(out var descRef);
			return descRef;
		}
	}

	public Adapter1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Adapter1(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Adapter1(nativePointer);
		}
		return null;
	}

	internal unsafe void GetDescription1(out AdapterDescription1 descRef)
	{
		AdapterDescription1.__Native @ref = default(AdapterDescription1.__Native);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		descRef = default(AdapterDescription1);
		descRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}
}

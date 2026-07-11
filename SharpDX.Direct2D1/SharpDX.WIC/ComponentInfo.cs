using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("23BC3F0A-698B-4357-886B-F24D50671334")]
public class ComponentInfo : ComObject
{
	public ComponentType ComponentType
	{
		get
		{
			GetComponentType(out var typeRef);
			return typeRef;
		}
	}

	public Guid CLSID
	{
		get
		{
			GetCLSID(out var clsidRef);
			return clsidRef;
		}
	}

	public int SigningStatus
	{
		get
		{
			GetSigningStatus(out var statusRef);
			return statusRef;
		}
	}

	public Guid VendorGUID
	{
		get
		{
			GetVendorGUID(out var guidVendorRef);
			return guidVendorRef;
		}
	}

	public unsafe string Author
	{
		get
		{
			int cchActualRef = 0;
			GetAuthor(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetAuthor(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public unsafe string Version
	{
		get
		{
			int cchActualRef = 0;
			GetVersion(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetVersion(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public unsafe string SpecVersion
	{
		get
		{
			int cchActualRef = 0;
			GetSpecVersion(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetSpecVersion(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public unsafe string FriendlyName
	{
		get
		{
			int cchActualRef = 0;
			GetFriendlyName(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetFriendlyName(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public ComponentInfo(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ComponentInfo(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new ComponentInfo(nativePointer);
		}
		return null;
	}

	internal unsafe void GetComponentType(out ComponentType typeRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<ComponentType, IntPtr>(ref typeRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetCLSID(out Guid clsidRef)
	{
		clsidRef = default(Guid);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Guid, IntPtr>(ref clsidRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetSigningStatus(out int statusRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref statusRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetAuthor(int cchAuthor, IntPtr author, out int cchActualRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref cchActualRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, cchAuthor, (void*)author, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetVendorGUID(out Guid guidVendorRef)
	{
		guidVendorRef = default(Guid);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Guid, IntPtr>(ref guidVendorRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetVersion(int cchVersion, IntPtr version, out int cchActualRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref cchActualRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, cchVersion, (void*)version, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetSpecVersion(int cchSpecVersion, IntPtr specVersion, out int cchActualRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref cchActualRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, cchSpecVersion, (void*)specVersion, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetFriendlyName(int cchFriendlyName, IntPtr friendlyName, out int cchActualRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref cchActualRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, cchFriendlyName, (void*)friendlyName, ptr);
		}
		result.CheckError();
	}

	public ComponentInfo(ImagingFactory factory, Guid clsidComponent)
		: base(IntPtr.Zero)
	{
		factory.CreateComponentInfo(clsidComponent, this);
	}
}

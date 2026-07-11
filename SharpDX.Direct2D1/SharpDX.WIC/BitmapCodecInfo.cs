using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("E87A44C4-B76E-4c47-8B09-298EB12A2714")]
public class BitmapCodecInfo : ComponentInfo
{
	public Guid ContainerFormat
	{
		get
		{
			GetContainerFormat(out var guidContainerFormatRef);
			return guidContainerFormatRef;
		}
	}

	public Bool IsAnimationSupported
	{
		get
		{
			IsAnimationSupported_(out var fSupportAnimationRef);
			return fSupportAnimationRef;
		}
	}

	public Bool IsChromakeySupported
	{
		get
		{
			IsChromakeySupported_(out var fSupportChromakeyRef);
			return fSupportChromakeyRef;
		}
	}

	public Bool IsLosslessSupported
	{
		get
		{
			IsLosslessSupported_(out var fSupportLosslessRef);
			return fSupportLosslessRef;
		}
	}

	public Bool IsMultiframeSupported
	{
		get
		{
			IsMultiframeSupported_(out var fSupportMultiframeRef);
			return fSupportMultiframeRef;
		}
	}

	public Guid[] PixelFormats
	{
		get
		{
			int actualRef = 0;
			GetPixelFormats(0, null, out actualRef);
			if (actualRef == 0)
			{
				return new Guid[0];
			}
			Guid[] array = new Guid[actualRef];
			GetPixelFormats(actualRef, array, out actualRef);
			return array;
		}
	}

	public unsafe string ColorManagementVersion
	{
		get
		{
			int cchActualRef = 0;
			GetColorManagementVersion(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetColorManagementVersion(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public unsafe string DeviceManufacturer
	{
		get
		{
			int cchActualRef = 0;
			GetDeviceManufacturer(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetDeviceManufacturer(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public unsafe string DeviceModels
	{
		get
		{
			int cchActualRef = 0;
			GetDeviceModels(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetDeviceModels(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public unsafe string MimeTypes
	{
		get
		{
			int cchActualRef = 0;
			GetMimeTypes(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetMimeTypes(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public unsafe string FileExtensions
	{
		get
		{
			int cchActualRef = 0;
			GetFileExtensions(0, IntPtr.Zero, out cchActualRef);
			if (cchActualRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualRef];
			GetFileExtensions(cchActualRef, (IntPtr)ptr, out cchActualRef);
			return new string(ptr, 0, cchActualRef);
		}
	}

	public BitmapCodecInfo(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapCodecInfo(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapCodecInfo(nativePointer);
		}
		return null;
	}

	internal unsafe void GetContainerFormat(out Guid guidContainerFormatRef)
	{
		guidContainerFormatRef = default(Guid);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Guid, IntPtr>(ref guidContainerFormatRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetPixelFormats(int formats, Guid[] guidPixelFormatsRef, out int actualRef)
	{
		Result result;
		fixed (IntPtr* ptr = guidPixelFormatsRef)
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref actualRef))
			{
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, formats, ptr, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetColorManagementVersion(int cchColorManagementVersion, IntPtr colorManagementVersion, out int cchActualRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref cchActualRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, cchColorManagementVersion, (void*)colorManagementVersion, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetDeviceManufacturer(int cchDeviceManufacturer, IntPtr deviceManufacturer, out int cchActualRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref cchActualRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, cchDeviceManufacturer, (void*)deviceManufacturer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetDeviceModels(int cchDeviceModels, IntPtr deviceModels, out int cchActualRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref cchActualRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, cchDeviceModels, (void*)deviceModels, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetMimeTypes(int cchMimeTypes, IntPtr mimeTypes, out int cchActualRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref cchActualRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, cchMimeTypes, (void*)mimeTypes, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetFileExtensions(int cchFileExtensions, IntPtr fileExtensions, out int cchActualRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref cchActualRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, cchFileExtensions, (void*)fileExtensions, ptr);
		}
		result.CheckError();
	}

	internal unsafe void IsAnimationSupported_(out Bool fSupportAnimationRef)
	{
		fSupportAnimationRef = default(Bool);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref fSupportAnimationRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void IsChromakeySupported_(out Bool fSupportChromakeyRef)
	{
		fSupportChromakeyRef = default(Bool);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref fSupportChromakeyRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void IsLosslessSupported_(out Bool fSupportLosslessRef)
	{
		fSupportLosslessRef = default(Bool);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref fSupportLosslessRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void IsMultiframeSupported_(out Bool fSupportMultiframeRef)
	{
		fSupportMultiframeRef = default(Bool);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref fSupportMultiframeRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public unsafe Bool MatchesMimeType(string mimeType)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(mimeType);
		Bool result = default(Bool);
		Result result2 = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, (void*)intPtr, &result);
		Marshal.FreeHGlobal(intPtr);
		result2.CheckError();
		return result;
	}
}

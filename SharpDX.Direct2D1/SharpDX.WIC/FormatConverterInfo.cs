using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("9F34FB65-13F4-4f15-BC57-3726B5E53D9F")]
public class FormatConverterInfo : ComponentInfo
{
	public unsafe Guid[] PixelFormats
	{
		get
		{
			int actualRef = 0;
			GetPixelFormats(actualRef, IntPtr.Zero, out actualRef);
			if (actualRef == 0)
			{
				return new Guid[0];
			}
			Guid[] array = new Guid[actualRef];
			fixed (IntPtr* value = array)
			{
				GetPixelFormats(actualRef, new IntPtr(value), out actualRef);
			}
			return array;
		}
	}

	public FormatConverterInfo(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FormatConverterInfo(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new FormatConverterInfo(nativePointer);
		}
		return null;
	}

	internal unsafe void GetPixelFormats(int formats, IntPtr pixelFormatGUIDsRef, out int actualRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref actualRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, formats, (void*)pixelFormatGUIDsRef, ptr);
		}
		result.CheckError();
	}

	internal unsafe void CreateInstance(FormatConverter converterOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &zero);
		converterOut.NativePointer = zero;
		result.CheckError();
	}
}

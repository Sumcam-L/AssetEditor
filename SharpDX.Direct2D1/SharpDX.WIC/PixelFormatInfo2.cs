using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("A9DB33A2-AF5F-43C7-B679-74F5984B5AA4")]
public class PixelFormatInfo2 : PixelFormatInfo
{
	public Bool IsSupportingTransparency
	{
		get
		{
			IsSupportingTransparency_(out var fSupportsTransparencyRef);
			return fSupportsTransparencyRef;
		}
	}

	public PixelFormatNumericRepresentation NumericRepresentation
	{
		get
		{
			GetNumericRepresentation(out var numericRepresentationRef);
			return numericRepresentationRef;
		}
	}

	public PixelFormatInfo2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator PixelFormatInfo2(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new PixelFormatInfo2(nativePointer);
		}
		return null;
	}

	internal unsafe void IsSupportingTransparency_(out Bool fSupportsTransparencyRef)
	{
		fSupportsTransparencyRef = default(Bool);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref fSupportsTransparencyRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetNumericRepresentation(out PixelFormatNumericRepresentation numericRepresentationRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<PixelFormatNumericRepresentation, IntPtr>(ref numericRepresentationRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("3C613A02-34B2-44ea-9A7C-45AEA9C6FD6D")]
public class ColorContext : ComObject
{
	public ColorContextType TypeInfo
	{
		get
		{
			GetTypeInfo(out var typeRef);
			return typeRef;
		}
	}

	public int ExifColorSpace
	{
		get
		{
			GetExifColorSpace(out var valueRef);
			return valueRef;
		}
	}

	public DataStream Profile
	{
		get
		{
			GetProfileBytes(0, IntPtr.Zero, out var actualRef);
			if (actualRef == 0)
			{
				return null;
			}
			DataStream dataStream = new DataStream(actualRef, canRead: true, canWrite: true);
			GetProfileBytes(actualRef, dataStream.DataPointer, out actualRef);
			return dataStream;
		}
	}

	public ColorContext(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ColorContext(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new ColorContext(nativePointer);
		}
		return null;
	}

	public unsafe void InitializeFromFilename(string filename)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(filename);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)intPtr);
		Marshal.FreeHGlobal(intPtr);
		result.CheckError();
	}

	internal unsafe void InitializeFromMemory(IntPtr bufferRef, int bufferSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)bufferRef, bufferSize)).CheckError();
	}

	public unsafe void InitializeFromExifColorSpace(int value)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, value)).CheckError();
	}

	internal unsafe void GetTypeInfo(out ColorContextType typeRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<ColorContextType, IntPtr>(ref typeRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetProfileBytes(int buffer, IntPtr bufferRef, out int actualRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref actualRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, buffer, (void*)bufferRef, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetExifColorSpace(out int valueRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref valueRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public ColorContext(ImagingFactory factory)
		: base(IntPtr.Zero)
	{
		factory.CreateColorContext(this);
	}

	public void InitializeFromMemory(DataPointer dataPointer)
	{
		InitializeFromMemory(dataPointer.Pointer, dataPointer.Size);
	}
}

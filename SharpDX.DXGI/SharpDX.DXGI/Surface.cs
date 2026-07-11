using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("cafcb56c-6ac3-4889-bf47-9e23bbd260ec")]
public class Surface : DeviceChild
{
	public SurfaceDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public Surface(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Surface(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Surface(nativePointer);
		}
		return null;
	}

	internal unsafe void GetDescription(out SurfaceDescription descRef)
	{
		descRef = default(SurfaceDescription);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<SurfaceDescription, IntPtr>(ref descRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void Map(out MappedRectangle lockedRectRef, int mapFlags)
	{
		lockedRectRef = default(MappedRectangle);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<MappedRectangle, IntPtr>(ref lockedRectRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr, mapFlags);
		}
		result.CheckError();
	}

	public unsafe void Unmap()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public DataRectangle Map(MapFlags flags)
	{
		Map(out var lockedRectRef, (int)flags);
		return new DataRectangle(lockedRectRef.PBits, lockedRectRef.Pitch);
	}

	public DataRectangle Map(MapFlags flags, out DataStream dataStream)
	{
		DataRectangle result = Map(flags);
		dataStream = new DataStream(result.DataPointer, Description.Height * result.Pitch, canRead: true, canWrite: true);
		return result;
	}

	public static Surface FromSwapChain(SwapChain swapChain, int index)
	{
		swapChain.GetBuffer(index, Utilities.GetGuidFromType(typeof(Surface)), out var surfaceOut);
		return new Surface(surfaceOut);
	}
}

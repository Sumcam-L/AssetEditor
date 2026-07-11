using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("54ec77fa-1377-44e6-8c32-88fd5f44c84c")]
public class Device : DXGIObject
{
	public Adapter Adapter
	{
		get
		{
			GetAdapter(out var adapterRef);
			return adapterRef;
		}
	}

	public int GPUThreadPriority
	{
		get
		{
			GetGPUThreadPriority(out var priorityRef);
			return priorityRef;
		}
		set
		{
			SetGPUThreadPriority(value);
		}
	}

	public Residency[] QueryResourceResidency(params ComObject[] comObjects)
	{
		int num = comObjects.Length;
		Residency[] array = new Residency[num];
		QueryResourceResidency(comObjects, array, num);
		return array;
	}

	public Device(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Device(nativePointer);
		}
		return null;
	}

	internal unsafe void GetAdapter(out Adapter adapterRef)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &zero);
		adapterRef = ((zero == IntPtr.Zero) ? null : new Adapter(zero));
		result.CheckError();
	}

	internal unsafe void CreateSurface(ref SurfaceDescription descRef, int numSurfaces, int usage, SharedResource? sharedResourceRef, out Surface surfaceOut)
	{
		SharedResource value = default(SharedResource);
		if (sharedResourceRef.HasValue)
		{
			value = sharedResourceRef.Value;
		}
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<SurfaceDescription, IntPtr>(ref descRef))
		{
			void* nativePointer = _nativePointer;
			void* intPtr = (sharedResourceRef.HasValue ? (&value) : ((void*)IntPtr.Zero));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, ptr, numSurfaces, usage, intPtr, &zero);
		}
		surfaceOut = ((zero == IntPtr.Zero) ? null : new Surface(zero));
		result.CheckError();
	}

	internal unsafe void QueryResourceResidency(ComObject[] resourcesOut, Residency[] residencyStatusRef, int numResources)
	{
		IntPtr* ptr = null;
		if (resourcesOut != null)
		{
			IntPtr* ptr2 = stackalloc IntPtr[resourcesOut.Length];
			ptr = ptr2;
			for (int i = 0; i < resourcesOut.Length; i++)
			{
				ptr[i] = ((resourcesOut[i] == null) ? IntPtr.Zero : resourcesOut[i].NativePointer);
			}
		}
		Result result;
		fixed (IntPtr* ptr3 = residencyStatusRef)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr3, numResources);
		}
		result.CheckError();
	}

	internal unsafe void QueryResourceResidency(ComArray<ComObject> resourcesOut, Residency[] residencyStatusRef, int numResources)
	{
		Result result;
		fixed (IntPtr* ptr = residencyStatusRef)
		{
			void* nativePointer = _nativePointer;
			IntPtr intPtr = resourcesOut?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, ptr, numResources);
		}
		result.CheckError();
	}

	internal unsafe void SetGPUThreadPriority(int priority)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, priority)).CheckError();
	}

	internal unsafe void GetGPUThreadPriority(out int priorityRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref priorityRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}
}

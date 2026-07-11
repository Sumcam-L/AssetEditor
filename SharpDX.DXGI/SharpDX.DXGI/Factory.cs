using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("7b7166ec-21c7-44ae-b21a-c9ae321ae369")]
public class Factory : DXGIObject
{
	public Adapter[] Adapters
	{
		get
		{
			List<Adapter> list = new List<Adapter>();
			while (true)
			{
				Adapter adapterOut;
				Result adapter = GetAdapter(list.Count, out adapterOut);
				if (adapter == ResultCode.NotFound)
				{
					break;
				}
				list.Add(adapterOut);
			}
			return list.ToArray();
		}
	}

	public Factory()
		: base(IntPtr.Zero)
	{
		DXGI.CreateDXGIFactory(Utilities.GetGuidFromType(GetType()), out var factoryOut);
		base.NativePointer = factoryOut;
	}

	public Adapter CreateSoftwareAdapter(Module module)
	{
		return CreateSoftwareAdapter(Marshal.GetHINSTANCE(module));
	}

	public Adapter GetAdapter(int index)
	{
		GetAdapter(index, out var adapterOut).CheckError();
		return adapterOut;
	}

	public int GetAdapterCount()
	{
		int num = 0;
		while (true)
		{
			Adapter adapterOut;
			Result adapter = GetAdapter(num, out adapterOut);
			adapterOut?.Dispose();
			if (adapter == ResultCode.NotFound)
			{
				break;
			}
			num++;
		}
		return num;
	}

	public Factory(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Factory(nativePointer);
		}
		return null;
	}

	internal unsafe Result GetAdapter(int adapter, out Adapter adapterOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, adapter, &zero);
		adapterOut = ((zero == IntPtr.Zero) ? null : new Adapter(zero));
		return result;
	}

	public unsafe void MakeWindowAssociation(IntPtr windowHandle, WindowAssociationFlags flags)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)windowHandle, (int)flags)).CheckError();
	}

	public unsafe IntPtr GetWindowAssociation()
	{
		IntPtr result = default(IntPtr);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, &result)).CheckError();
		return result;
	}

	internal unsafe void CreateSwapChain(ComObject deviceRef, ref SwapChainDescription descRef, SwapChain swapChainOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<SwapChainDescription, IntPtr>(ref descRef))
		{
			void* nativePointer = _nativePointer;
			IntPtr intPtr = deviceRef?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, ptr, &zero);
		}
		swapChainOut.NativePointer = zero;
		result.CheckError();
	}

	public unsafe Adapter CreateSoftwareAdapter(IntPtr module)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (void*)module, &zero);
		Adapter result2 = ((zero == IntPtr.Zero) ? null : new Adapter(zero));
		result.CheckError();
		return result2;
	}
}

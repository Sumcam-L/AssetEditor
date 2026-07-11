using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("770aae78-f26f-4dba-a829-253c83d1b387")]
public class Factory1 : Factory
{
	public Adapter1[] Adapters1
	{
		get
		{
			List<Adapter1> list = new List<Adapter1>();
			while (true)
			{
				Adapter1 adapterOut;
				Result adapter = GetAdapter1(list.Count, out adapterOut);
				if (adapter == ResultCode.NotFound)
				{
					break;
				}
				list.Add(adapterOut);
			}
			return list.ToArray();
		}
	}

	public Bool IsCurrent => IsCurrent_();

	public Factory1()
		: base(IntPtr.Zero)
	{
		DXGI.CreateDXGIFactory1(Utilities.GetGuidFromType(GetType()), out var factoryOut);
		base.NativePointer = factoryOut;
	}

	public Adapter1 GetAdapter1(int index)
	{
		GetAdapter1(index, out var adapterOut).CheckError();
		return adapterOut;
	}

	public int GetAdapterCount1()
	{
		int num = 0;
		while (true)
		{
			Adapter1 adapterOut;
			Result adapter = GetAdapter1(num, out adapterOut);
			adapterOut?.Dispose();
			if (adapter == ResultCode.NotFound)
			{
				break;
			}
			num++;
		}
		return num;
	}

	public Factory1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory1(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Factory1(nativePointer);
		}
		return null;
	}

	internal unsafe Result GetAdapter1(int adapter, out Adapter1 adapterOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, adapter, &zero);
		adapterOut = ((zero == IntPtr.Zero) ? null : new Adapter1(zero));
		return result;
	}

	internal unsafe Bool IsCurrent_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, Bool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer);
	}
}

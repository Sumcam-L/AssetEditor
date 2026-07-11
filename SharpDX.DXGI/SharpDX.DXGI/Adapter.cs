using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("2411e7e1-12ac-4ccf-bd14-9798e8534dc0")]
public class Adapter : DXGIObject
{
	public Output[] Outputs
	{
		get
		{
			List<Output> list = new List<Output>();
			while (true)
			{
				Output outputOut;
				Result output = GetOutput(list.Count, out outputOut);
				if (output == ResultCode.NotFound || outputOut == null)
				{
					break;
				}
				list.Add(outputOut);
			}
			return list.ToArray();
		}
	}

	public AdapterDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public bool IsInterfaceSupported(Type type)
	{
		long userModeVersion;
		return IsInterfaceSupported(type, out userModeVersion);
	}

	public bool IsInterfaceSupported<T>() where T : ComObject
	{
		long userModeVersion;
		return IsInterfaceSupported(typeof(T), out userModeVersion);
	}

	public bool IsInterfaceSupported<T>(out long userModeVersion) where T : ComObject
	{
		return IsInterfaceSupported(typeof(T), out userModeVersion);
	}

	public bool IsInterfaceSupported(Type type, out long userModeVersion)
	{
		return CheckInterfaceSupport(Utilities.GetGuidFromType(type), out userModeVersion).Success;
	}

	public Output GetOutput(int outputIndex)
	{
		GetOutput(outputIndex, out var outputOut).CheckError();
		return outputOut;
	}

	public int GetOutputCount()
	{
		int num = 0;
		while (true)
		{
			Output outputOut;
			Result output = GetOutput(num, out outputOut);
			if (output == ResultCode.NotFound || outputOut == null)
			{
				break;
			}
			outputOut.Dispose();
			num++;
		}
		return num;
	}

	public Adapter(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Adapter(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Adapter(nativePointer);
		}
		return null;
	}

	internal unsafe Result GetOutput(int output, out Output outputOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, output, &zero);
		outputOut = ((zero == IntPtr.Zero) ? null : new Output(zero));
		return result;
	}

	internal unsafe void GetDescription(out AdapterDescription descRef)
	{
		AdapterDescription.__Native @ref = default(AdapterDescription.__Native);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		descRef = default(AdapterDescription);
		descRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	internal unsafe Result CheckInterfaceSupport(Guid interfaceName, out long uMDVersionRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<long, IntPtr>(ref uMDVersionRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, &interfaceName, ptr);
		}
		return result;
	}
}

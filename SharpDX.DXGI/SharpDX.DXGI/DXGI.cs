using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SharpDX.DXGI;

internal static class DXGI
{
	public unsafe static void CreateDXGIFactory1(Guid riid, out IntPtr factoryOut)
	{
		Result result;
		fixed (IntPtr* arg = &factoryOut)
		{
			result = CreateDXGIFactory1_(&riid, arg);
		}
		result.CheckError();
	}

	[DllImport("dxgi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateDXGIFactory1")]
	[SuppressUnmanagedCodeSecurity]
	private unsafe static extern int CreateDXGIFactory1_(void* arg0, void* arg1);

	public unsafe static void CreateDXGIFactory(Guid riid, out IntPtr factoryOut)
	{
		Result result;
		fixed (IntPtr* arg = &factoryOut)
		{
			result = CreateDXGIFactory_(&riid, arg);
		}
		result.CheckError();
	}

	[DllImport("dxgi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateDXGIFactory")]
	[SuppressUnmanagedCodeSecurity]
	private unsafe static extern int CreateDXGIFactory_(void* arg0, void* arg1);
}

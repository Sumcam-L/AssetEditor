using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SharpDX.DirectWrite;

internal static class DWrite
{
	public unsafe static void CreateFactory(FactoryType factoryType, Guid iid, ComObject factory)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = DWriteCreateFactory_((int)factoryType, &iid, &zero);
		factory.NativePointer = zero;
		result.CheckError();
	}

	[DllImport("dwrite.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "DWriteCreateFactory")]
	[SuppressUnmanagedCodeSecurity]
	private unsafe static extern int DWriteCreateFactory_(int arg0, void* arg1, void* arg2);
}

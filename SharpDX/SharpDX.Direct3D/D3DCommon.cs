using System;
using System.Runtime.InteropServices;
using System.Security;

namespace SharpDX.Direct3D;

internal static class D3DCommon
{
	public unsafe static void CreateBlob(PointerSize size, Blob blobOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = D3DCreateBlob_(size, &zero);
		blobOut.NativePointer = zero;
		result.CheckError();
	}

	[DllImport("d3dcompiler_43.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D3DCreateBlob")]
	[SuppressUnmanagedCodeSecurity]
	private unsafe static extern int D3DCreateBlob_(void* arg0, void* arg1);
}

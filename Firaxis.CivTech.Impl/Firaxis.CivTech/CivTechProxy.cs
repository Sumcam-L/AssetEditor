using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Firaxis.CivTech;

internal class CivTechProxy : ICivTechProxy
{
	public static int TempHeapSize = 4194304;

	public unsafe CivTechProxy(string logFolder)
	{
		//IL_003b: Expected I, but got I8
		char* ptr = (char*)Marshal.StringToHGlobalUni(logFolder).ToPointer();
		global::_003CModule_003E.Platform_002ESetLogRoot(ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		global::_003CModule_003E.Platform_002ETempHeap_002EStartup(global::_003CModule_003E.Platform_002EGetTempHeap(), 4194304uL, null);
		global::_003CModule_003E.Firaxis_002ECivTech_002ERegisterReflectionTypes();
		global::_003CModule_003E.Granny_002EInitialize();
	}

	private unsafe void _007ECivTechProxy()
	{
		global::_003CModule_003E.Platform_002ETempHeap_002EShutdown(global::_003CModule_003E.Platform_002EGetTempHeap());
	}

	private unsafe void _0021CivTechProxy()
	{
		global::_003CModule_003E.Platform_002ETempHeap_002EShutdown(global::_003CModule_003E.Platform_002EGetTempHeap());
	}

	[HandleProcessCorruptedStateExceptions]
	protected unsafe virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			global::_003CModule_003E.Platform_002ETempHeap_002EShutdown(global::_003CModule_003E.Platform_002EGetTempHeap());
			return;
		}
		try
		{
			_0021CivTechProxy();
		}
		finally
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~CivTechProxy()
	{
		Dispose(A_0: false);
	}
}

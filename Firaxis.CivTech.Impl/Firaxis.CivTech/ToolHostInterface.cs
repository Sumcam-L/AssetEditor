using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Platform;
using std;
using ToolHost;

namespace Firaxis.CivTech;

internal class ToolHostInterface : IToolHostInterface
{
	private unsafe shared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E* m_toolHostModuleMgrPtr;

	private string m_libraryPath;

	private string m_logFolder;

	public unsafe virtual bool IsLoaded
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			//IL_0024: Expected I, but got I8
			shared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E* toolHostModuleMgrPtr = m_toolHostModuleMgrPtr;
			if (toolHostModuleMgrPtr == null)
			{
				return false;
			}
			IToolHostModuleMgrClient* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E_002Eget(toolHostModuleMgrPtr);
			if (ptr == null)
			{
				return false;
			}
			return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, byte>)(*(ulong*)(*(long*)ptr + 8)))((nint)ptr) != 0;
		}
	}

	public virtual string DllPath => m_libraryPath;

	public unsafe ToolHostInterface(string libraryPath, string logFolder)
	{
		//IL_0008: Expected I, but got I8
		//IL_0043: Expected I, but got I8
		m_toolHostModuleMgrPtr = null;
		m_libraryPath = libraryPath;
		m_logFolder = logFolder;
		base._002Ector();
		shared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E* ptr = (shared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E*)global::_003CModule_003E.@new(16uL, (int)global::_003CModule_003E.Platform_002EGetMemBlockType(), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FP_0040FBAECFFE_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 15, 23, 0);
		m_toolHostModuleMgrPtr = ((ptr == null) ? null : global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E_002E_007Bctor_007D(ptr));
		char* ptr2 = (char*)Marshal.StringToHGlobalUni(m_libraryPath).ToPointer();
		char* ptr3 = (char*)Marshal.StringToHGlobalUni(m_logFolder).ToPointer();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out shared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E shared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E2);
		shared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E* ptr4 = global::_003CModule_003E.ToolHost_002ECreateToolHostModuleMgrClient(&shared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E2, ptr2, ptr3);
		try
		{
			global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E_002E_003D(m_toolHostModuleMgrPtr, ptr4);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<shared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E*, void>)(&global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E_002E_007Bdtor_007D), &shared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E2);
			throw;
		}
		global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E_002E_007Bdtor_007D(&shared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E2);
		IntPtr hglobal = new IntPtr(ptr3);
		Marshal.FreeHGlobal(hglobal);
		IntPtr hglobal2 = new IntPtr(ptr2);
		Marshal.FreeHGlobal(hglobal2);
	}

	private void _007EToolHostInterface()
	{
		_0021ToolHostInterface();
	}

	private unsafe void _0021ToolHostInterface()
	{
		//IL_0057: Expected I, but got I8
		//IL_0028: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0xac82c0d4_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003CFinalizer_003E_0040ToolHostInterface_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && m_toolHostModuleMgrPtr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040LIMBIBAJ_0040m_toolHostModuleMgrPtr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FP_0040FBAECFFE_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 33u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xac82c0d4_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003CFinalizer_003E_0040ToolHostInterface_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E_002Ereset(m_toolHostModuleMgrPtr);
		shared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E* toolHostModuleMgrPtr = m_toolHostModuleMgrPtr;
		if (toolHostModuleMgrPtr != null)
		{
			global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E_002E_007Bdtor_007D(toolHostModuleMgrPtr);
			global::_003CModule_003E.delete(toolHostModuleMgrPtr, 16uL);
		}
		m_toolHostModuleMgrPtr = null;
	}

	public unsafe IToolHostModuleMgrClient* GetToolHostModuleMgr()
	{
		//IL_002d: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0xac82c0d4_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetToolHostModuleMgr_0040ToolHostInterface_0040CivTech_0040Firaxis_0040_0040QE_0024AAMAEAVIToolHostModuleMgrClient_0040ToolHost_0040_0040XZ_00404_NA && global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E_002Eget(m_toolHostModuleMgrPtr) == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BO_0040EIOPEJP_0040m_toolHostModuleMgrPtr_003F9_003F_0024DOget_003F_0024CI_003F_0024CJ_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FP_0040FBAECFFE_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xac82c0d4_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetToolHostModuleMgr_0040ToolHostInterface_0040CivTech_0040Firaxis_0040_0040QE_0024AAMAEAVIToolHostModuleMgrClient_0040ToolHost_0040_0040XZ_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		return global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AIToolHostModuleMgrClient_003E_002Eget(m_toolHostModuleMgrPtr);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021ToolHostInterface();
			return;
		}
		try
		{
			_0021ToolHostInterface();
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

	~ToolHostInterface()
	{
		Dispose(A_0: false);
	}
}

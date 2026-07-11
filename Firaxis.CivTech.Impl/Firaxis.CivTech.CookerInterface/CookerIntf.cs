using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetCooker;
using AssetCookerHelpers;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.CivTech.AssetPreviewer._INTERNAL;
using Platform;
using Reflection;
using std;
using ToolHost;

namespace Firaxis.CivTech.CookerInterface;

public class CookerIntf : ICookerIntf
{
	private LogEventHandler _003Cbacking_store_003ECookerLog;

	private unsafe shared_ptr_003CToolHost_003A_003AICookerIntf_003E* m_pCookerIntfDLL;

	private unsafe ToolLogger* m_pToolLogger;

	[SpecialName]
	public virtual event LogEventHandler CookerLog
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003ECookerLog = (LogEventHandler)Delegate.Combine(_003Cbacking_store_003ECookerLog, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003ECookerLog = (LogEventHandler)Delegate.Remove(_003Cbacking_store_003ECookerLog, value);
		}
	}

	[SpecialName]
	protected virtual void raise_CookerLog(string value0, Firaxis.CivTech.AssetPreviewer.LogLevel value1, string value2)
	{
		_003Cbacking_store_003ECookerLog?.Invoke(value0, value1, value2);
	}

	public unsafe CookerIntf()
	{
		//IL_0032: Expected I, but got I8
		//IL_00a2: Expected I, but got I8
		//IL_0073: Expected I8, but got I
		uint num = 0u;
		base._002Ector();
		shared_ptr_003CToolHost_003A_003AICookerIntf_003E* ptr = (shared_ptr_003CToolHost_003A_003AICookerIntf_003E*)global::_003CModule_003E.@new(16uL, (int)global::_003CModule_003E.Platform_002EGetMemBlockType(), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GJ_0040NDLLDHFF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 20, 23, 0);
		m_pCookerIntfDLL = ((ptr == null) ? null : global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AICookerIntf_003E_002E_007Bctor_007D(ptr));
		ManagedLogger value = new ManagedLogger(RaiseCookLog);
		ToolLogger* ptr2 = (ToolLogger*)global::_003CModule_003E.@new(384uL);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AManagedLogger_0020_005E_003E obj);
		ToolLogger* pToolLogger;
		try
		{
			if (ptr2 != null)
			{
				*(long*)(&obj) = (nint)((IntPtr)GCHandle.Alloc(value)).ToPointer();
				try
				{
					num = 1u;
					pToolLogger = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EToolLogger_002E_007Bctor_007D(ptr2, &obj, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0M_0040LCABFHCA_0040Game_003F5Cooker_003F_0024AA_0040));
				}
				catch
				{
					//try-fault
					if ((num & 1) != 0)
					{
						num &= 0xFFFFFFFEu;
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AManagedLogger_0020_005E_003E*, void>)(&global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AManagedLogger_0020_005E_003E_002E_007Bdtor_007D), &obj);
					}
					throw;
				}
			}
			else
			{
				pToolLogger = null;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr2, 384uL);
			throw;
		}
		try
		{
			m_pToolLogger = pToolLogger;
		}
		catch
		{
			//try-fault
			if ((num & 1) != 0)
			{
				num &= 0xFFFFFFFEu;
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AManagedLogger_0020_005E_003E*, void>)(&global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AManagedLogger_0020_005E_003E_002E_007Bdtor_007D), &obj);
			}
			throw;
		}
		if ((num & 1) != 0)
		{
			num &= 0xFFFFFFFEu;
			global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AManagedLogger_0020_005E_003E_002E_007Bdtor_007D(&obj);
		}
	}

	private unsafe void _007ECookerIntf()
	{
		//IL_003a: Expected I, but got I8
		//IL_0043: Expected I, but got I8
		global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AICookerIntf_003E_002Ereset(m_pCookerIntfDLL);
		shared_ptr_003CToolHost_003A_003AICookerIntf_003E* pCookerIntfDLL = m_pCookerIntfDLL;
		if (pCookerIntfDLL != null)
		{
			global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AICookerIntf_003E_002E_007Bdtor_007D(pCookerIntfDLL);
			global::_003CModule_003E.delete(pCookerIntfDLL, 16uL);
		}
		ToolLogger* pToolLogger = m_pToolLogger;
		if (pToolLogger != null)
		{
			ToolLogger* ptr = pToolLogger;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, uint, void*>)(*(ulong*)(*(ulong*)ptr)))((nint)ptr, 1u);
			m_pToolLogger = null;
		}
	}

	public unsafe virtual void Startup(IToolHostInterface ToolHost)
	{
		//IL_0048: Expected I, but got I8
		//IL_0092: Expected I, but got I8
		//IL_0093: Expected I8, but got I
		//IL_002a: Expected I, but got I8
		//IL_009f: Expected I, but got I8
		//IL_00df: Expected I, but got I8
		ToolHostInterface toolHostInterface = (ToolHostInterface)ToolHost;
		if (!global::_003CModule_003E._003FA0xc073e141_002E_003FbIgnoreAlways_0040_003F2_003F_003FStartup_0040CookerIntf_0040CookerInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIToolHostInterface_004045_0040_0040Z_00404_NA && toolHostInterface == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040JPOMFLOK_0040toolHostInterface_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GJ_0040NDLLDHFF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 46u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc073e141_002E_003FbIgnoreAlways_0040_003F2_003F_003FStartup_0040CookerIntf_0040CookerInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIToolHostInterface_004045_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		IToolHostModuleMgrClient* toolHostModuleMgr = toolHostInterface.GetToolHostModuleMgr();
		global::ToolHost.CookerInterface* ptr = (global::ToolHost.CookerInterface*)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, TypeInfo*, int, void*>)(*(ulong*)(*(long*)toolHostModuleMgr + 16)))((nint)toolHostModuleMgr, global::_003CModule_003E.ToolHost_002ECookerInterface_002EGetTypeInfo(), 9);
		if (ptr == null)
		{
			ulong num = 1uL;
			ulong num2 = 1uL;
			global::_003CModule_003E.Platform_002EGetLoggingOptions(&num, &num2);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BBH_0040D _0024ArrayType_0024_0024_0024BY0BBH_0040D2);
			uint num3 = (uint)global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), 279uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DN_0040FIHBCBON_0040Unable_003F5to_003F5get_003F5CookerInterface_003F5ve_0040), __arglist(9));
			global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CA_0040NPFADNJP_0040C_003F_0024AAo_003F_0024AAo_003F_0024AAk_003F_0024AAe_003F_0024AAr_003F_0024AAI_003F_0024AAn_003F_0024AAt_003F_0024AAe_003F_0024AAr_003F_0024AAf_003F_0024AAa_003F_0024AAc_003F_0024AAe_003F_0024AA_003F_0024AA_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), num3);
			return;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out shared_ptr_003CToolHost_003A_003AICookerIntf_003E shared_ptr_003CToolHost_003A_003AICookerIntf_003E2);
		long num4 = (nint)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, shared_ptr_003CToolHost_003A_003AICookerIntf_003E*, shared_ptr_003CToolHost_003A_003AICookerIntf_003E*>)(*(ulong*)(*(long*)ptr + 32)))((nint)ptr, &shared_ptr_003CToolHost_003A_003AICookerIntf_003E2);
		try
		{
			global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AICookerIntf_003E_002E_003D(m_pCookerIntfDLL, (shared_ptr_003CToolHost_003A_003AICookerIntf_003E*)num4);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<shared_ptr_003CToolHost_003A_003AICookerIntf_003E*, void>)(&global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AICookerIntf_003E_002E_007Bdtor_007D), &shared_ptr_003CToolHost_003A_003AICookerIntf_003E2);
			throw;
		}
		global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AICookerIntf_003E_002E_007Bdtor_007D(&shared_ptr_003CToolHost_003A_003AICookerIntf_003E2);
		if (!global::_003CModule_003E._003FA0xc073e141_002E_003FbIgnoreAlways_0040_003FDO_0040_003F_003FStartup_0040CookerIntf_0040CookerInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIToolHostInterface_004045_0040_0040Z_00404_NA && m_pCookerIntfDLL == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BB_0040JJPHAMAM_0040m_pCookerIntfDLL_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GJ_0040NDLLDHFF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 58u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc073e141_002E_003FbIgnoreAlways_0040_003FDO_0040_003F_003FStartup_0040CookerIntf_0040CookerInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIToolHostInterface_004045_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
	}

	public unsafe virtual void Configure(ICookerOptions cookOptions)
	{
		//IL_0022: Expected I, but got I8
		CookerOptions cookerOptions = (CookerOptions)cookOptions;
		ToolHost.ICookerIntf* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AICookerIntf_003E_002E_002D_003E(m_pCookerIntfDLL);
		((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, INewCookerOptions*, void>)(*(ulong*)(*(ulong*)ptr)))((nint)ptr, (INewCookerOptions*)cookerOptions.Options);
	}

	public unsafe virtual void Shutdown()
	{
		//IL_001b: Expected I, but got I8
		shared_ptr_003CToolHost_003A_003AICookerIntf_003E* pCookerIntfDLL = m_pCookerIntfDLL;
		if (pCookerIntfDLL != null)
		{
			ToolHost.ICookerIntf* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AICookerIntf_003E_002E_002D_003E(pCookerIntfDLL);
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(long*)intPtr + 8)))((nint)intPtr);
		}
	}

	public unsafe virtual ICookerResult Cook(ICookerOptions cookOptions)
	{
		//IL_004b: Expected I, but got I8
		//IL_0045: Expected I, but got I8
		//IL_007f: Expected I, but got I8
		//IL_007a: Expected I, but got I8
		//IL_0097: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out StubStatCollecter stubStatCollecter);
		*(long*)(&stubStatCollecter) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7StatCollecter_0040AssetCooker_0040_00406B_0040);
		try
		{
			*(long*)(&stubStatCollecter) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7StubStatCollecter_0040AssetCooker_0040_00406B_0040);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<StatCollecter*, void>)(&global::_003CModule_003E.AssetCooker_002EStatCollecter_002E_007Bdtor_007D), &stubStatCollecter);
			throw;
		}
		ICookerResult result;
		try
		{
			CookerOptions cookerOptions = (CookerOptions)cookOptions;
			cookerOptions.ReconcileFilesToCook();
			ToolLogger* pToolLogger = m_pToolLogger;
			ToolLogger* ptr = (ToolLogger*)((pToolLogger == null) ? 0 : ((ulong)(nint)pToolLogger + 256uL));
			ToolHost.ICookerResult toolHostResult;
			if (global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EValidateOptions(cookerOptions.Options, (Logger*)ptr))
			{
				ToolHost.ICookerIntf* ptr2 = global::_003CModule_003E.std_002Eshared_ptr_003CToolHost_003A_003AICookerIntf_003E_002Eget(m_pCookerIntfDLL);
				ToolLogger* pToolLogger2 = m_pToolLogger;
				ToolLogger* ptr3 = (ToolLogger*)((pToolLogger2 == null) ? 0 : ((ulong)(nint)pToolLogger2 + 256uL));
				toolHostResult = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, INewCookerOptions*, Logger*, StatCollecter*, ToolHost.ICookerResult>)(*(ulong*)(*(long*)ptr2 + 16)))((nint)ptr2, (INewCookerOptions*)cookerOptions.Options, (Logger*)ptr3, (StatCollecter*)(&stubStatCollecter));
			}
			else
			{
				toolHostResult = (ToolHost.ICookerResult)4;
			}
			result = global::_003CModule_003E.Firaxis_002ECivTech_002ECookerInterface_002EConvert(toolHostResult);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<StubStatCollecter*, void>)(&global::_003CModule_003E.AssetCooker_002EStubStatCollecter_002E_007Bdtor_007D), &stubStatCollecter);
			throw;
		}
		global::_003CModule_003E.AssetCooker_002EStatCollecter_002E_007Bdtor_007D((StatCollecter*)(&stubStatCollecter));
		return result;
	}

	private void RaiseCookLog(string context, Firaxis.CivTech.AssetPreviewer.LogLevel logLevel, string s)
	{
		raise_CookerLog(context, logLevel, s);
	}

	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007ECookerIntf();
		}
		else
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}
}

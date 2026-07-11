using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using _003CCppImplementationDetails_003E;
using AssetCooker;
using AssetObjects;
using AssetPreviewer;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer._INTERNAL;
using Platform;
using Reflection;
using String;
using ToolHost;

namespace Firaxis.CivTech.AssetPreviewer;

public class ManagedAssetPreviewer : IAssetPreviewer
{
	private LogEventHandler _003Cbacking_store_003ELogger;

	private EventHandler _003Cbacking_store_003EKnobChangesComplete;

	private IXLPRegistry m_pmXLPRegistry;

	private ICivTechService m_pmCivTechSvc;

	private unsafe HINSTANCE__* m_hDLL;

	private unsafe IPreviewer* m_pAppPreviewer;

	private unsafe IToolContext* m_pToolContext;

	private Control m_pmOwningControl;

	private KnobManager m_pmKnobManager;

	private WidgetManager m_pmWidgetManager;

	private EventHandler m_pmKnobChangeCompleteHandler;

	public unsafe virtual uint FrameNumber
	{
		get
		{
			//IL_0019: Expected I, but got I8
			IPreviewer* pAppPreviewer = m_pAppPreviewer;
			if (pAppPreviewer == null)
			{
				return 0u;
			}
			return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, uint>)(*(ulong*)(*(long*)pAppPreviewer + 88)))((nint)pAppPreviewer);
		}
	}

	public virtual IKnobManager KnobManager => m_pmKnobManager;

	[SpecialName]
	public virtual event EventHandler KnobChangesComplete
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003EKnobChangesComplete = (EventHandler)Delegate.Combine(_003Cbacking_store_003EKnobChangesComplete, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003EKnobChangesComplete = (EventHandler)Delegate.Remove(_003Cbacking_store_003EKnobChangesComplete, value);
		}
	}

	[SpecialName]
	public virtual event LogEventHandler Logger
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003ELogger = (LogEventHandler)Delegate.Combine(_003Cbacking_store_003ELogger, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003ELogger = (LogEventHandler)Delegate.Remove(_003Cbacking_store_003ELogger, value);
		}
	}

	[SpecialName]
	protected virtual void raise_Logger(string value0, LogLevel value1, string value2)
	{
		_003Cbacking_store_003ELogger?.Invoke(value0, value1, value2);
	}

	[SpecialName]
	protected virtual void raise_KnobChangesComplete(object value0, EventArgs value1)
	{
		_003Cbacking_store_003EKnobChangesComplete?.Invoke(value0, value1);
	}

	public unsafe ManagedAssetPreviewer(Control owningControl)
	{
		//IL_0050: Expected I, but got I8
		m_pmOwningControl = owningControl;
		m_pmKnobManager = new KnobManager(owningControl);
		m_pmWidgetManager = new WidgetManager(owningControl);
		base._002Ector();
		if (!global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVControl_0040Forms_0040Windows_0040System_0040_0040_0040Z_00404_NA && m_pmKnobManager == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040LHNFHABO_0040m_pmKnobManager_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 449u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVControl_0040Forms_0040Windows_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		EventHandler value = (m_pmKnobChangeCompleteHandler = OnKnobMgrEvent_KnobChangesComplete);
		m_pmKnobManager.KnobChangesComplete += value;
	}

	private void _007EManagedAssetPreviewer()
	{
		KnobManager pmKnobManager = m_pmKnobManager;
		if (pmKnobManager != null)
		{
			pmKnobManager.KnobChangesComplete -= m_pmKnobChangeCompleteHandler;
		}
		Shutdown();
	}

	private void _0021ManagedAssetPreviewer()
	{
		Shutdown();
	}

	public unsafe virtual void Startup(ICivTechService pmCivTechSvc, IXLPRegistry pmXLPRegistry)
	{
		//IL_00cf: Expected I, but got I8
		//IL_0058: Expected I, but got I8
		//IL_01e2: Expected I, but got I8
		//IL_024d: Expected I, but got I8
		//IL_01c1: Expected I, but got I8
		if (m_pAppPreviewer != null)
		{
			throw new Exception("ManagedAssetPreviewer was initialized twice");
		}
		m_pmCivTechSvc = pmCivTechSvc;
		m_pmXLPRegistry = pmXLPRegistry;
		ToolHostInterface toolHostInterface = (ToolHostInterface)pmCivTechSvc.ToolHostLoader.ToolHostInterface;
		if (!global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F5_003F_003FStartup_0040ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICivTechService_004045_0040PE_0024AAUIXLPRegistry_004045_0040_0040Z_00404_NA && toolHostInterface == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040JPOMFLOK_0040toolHostInterface_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 496u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F5_003F_003FStartup_0040ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICivTechService_004045_0040PE_0024AAUIXLPRegistry_004045_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!File.Exists(toolHostInterface.DllPath))
		{
			ulong num = 1uL;
			ulong num2 = 1uL;
			global::_003CModule_003E.Platform_002EGetLoggingOptions(&num, &num2);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BBH_0040D _0024ArrayType_0024_0024_0024BY0BBH_0040D2);
			uint num3 = (uint)global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), 279uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CF_0040GDINFLLF_0040Unable_003F5to_003F5find_003F5ToolHost_003F5DLL_003F5on_003F5d_0040), __arglist());
			global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BO_0040PICHLLLM_0040A_003F_0024AAs_003F_0024AAs_003F_0024AAe_003F_0024AAt_003F_0024AAP_003F_0024AAr_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAe_003F_0024AAw_003F_0024AAe_003F_0024AAr_003F_0024AA_003F_0024AA_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), num3);
			throw new Exception($"ToolHost .DLL located at path:\n\t{toolHostInterface.DllPath}\n\n does not exist on disk.  Consider reinstalling the tools to correct this issue.");
		}
		IToolHostModuleMgrClient* toolHostModuleMgr = toolHostInterface.GetToolHostModuleMgr();
		AssetPreviewerInterface* ptr = (AssetPreviewerInterface*)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, TypeInfo*, int, void*>)(*(ulong*)(*(long*)toolHostModuleMgr + 16)))((nint)toolHostModuleMgr, global::_003CModule_003E.ToolHost_002EAssetPreviewerInterface_002EGetTypeInfo(), 38);
		if (ptr == null)
		{
			ulong num4 = 1uL;
			ulong num5 = 1uL;
			global::_003CModule_003E.Platform_002EGetLoggingOptions(&num4, &num5);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BBH_0040D _0024ArrayType_0024_0024_0024BY0BBH_0040D3);
			uint num6 = (uint)global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D3), 279uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EF_0040IPMCGNH_0040Unable_003F5to_003F5get_003F5AssetPreviewerInte_0040), __arglist(38));
			global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BO_0040PICHLLLM_0040A_003F_0024AAs_003F_0024AAs_003F_0024AAe_003F_0024AAt_003F_0024AAP_003F_0024AAr_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAe_003F_0024AAw_003F_0024AAe_003F_0024AAr_003F_0024AA_003F_0024AA_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D3), num6);
			throw new Exception($"Unable to get AssetPreviewerInterface version {38} from tool host DLL!");
		}
		Path.GetDirectoryName(m_pmCivTechSvc.ToolHostLoader.ToolHostDllPath);
		ToolContextImpl* ptr2 = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_003FA0xbc03a39c_002ECreateToolContext(m_pmCivTechSvc, m_pmKnobManager, m_pmWidgetManager, RaiseLogEvent);
		if (m_pmCivTechSvc.AssetCloudSettings.EnablePreviewerTracing)
		{
			string previewerTraceLocation = m_pmCivTechSvc.AssetCloudSettings.PreviewerTraceLocation;
			if (!Directory.Exists(previewerTraceLocation))
			{
				Directory.CreateDirectory(previewerTraceLocation);
			}
			char* ptr3 = (char*)Marshal.StringToHGlobalUni(Path.Combine(previewerTraceLocation, "Previewer.trace")).ToPointer();
			*(sbyte*)((ulong)(nint)ptr2 + 184uL) = 1;
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_003D((BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*)((ulong)(nint)ptr2 + 176uL), ptr3);
			IntPtr hglobal = new IntPtr(ptr3);
			Marshal.FreeHGlobal(hglobal);
		}
		IPreviewer* ptr4 = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, IToolContext*, IPreviewer*>)(*(ulong*)(*(long*)ptr + 32)))((nint)ptr, (IToolContext*)ptr2);
		if (ptr4 == null)
		{
			ulong num7 = 1uL;
			ulong num8 = 1uL;
			global::_003CModule_003E.Platform_002EGetLoggingOptions(&num7, &num8);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BBH_0040D _0024ArrayType_0024_0024_0024BY0BBH_0040D4);
			uint num9 = (uint)global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D4), 279uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ED_0040FGMHINPG_0040Unable_003F5to_003F5initialize_003F5AssetPrevie_0040), __arglist(38));
			global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BO_0040PICHLLLM_0040A_003F_0024AAs_003F_0024AAs_003F_0024AAe_003F_0024AAt_003F_0024AAP_003F_0024AAr_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAe_003F_0024AAw_003F_0024AAe_003F_0024AAr_003F_0024AA_003F_0024AA_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D4), num9);
			throw new Exception($"Unable to initialize AssetPreviewer version {38} from tool host DLL!");
		}
		m_pToolContext = (IToolContext*)ptr2;
		m_pAppPreviewer = ptr4;
		global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002ESetPreviewerLog(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, Logger*>)(*(ulong*)(*(long*)ptr2 + 104)))((nint)ptr2));
		m_pmWidgetManager.SetPreviewer(ptr4);
	}

	public unsafe virtual void SetTargetFPS(float targetFPS)
	{
		//IL_0053: Expected I, but got I8
		IPreviewer* pAppPreviewer = m_pAppPreviewer;
		if (pAppPreviewer == null)
		{
			if (!global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F6_003F_003FSetTargetFPS_0040ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXM_0040Z_00404_NA)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EE_0040IBFEIAKI_0040Tried_003F5to_003F5set_003F5the_003F5target_003F5FPS_003F5afte_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BD_0040OIKMPLCE_0040_003F_0024CB_003F_0024CCm_pAppPreviewer_003F_0024CC_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 583u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F6_003F_003FSetTargetFPS_0040ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXM_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
		}
		else
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, float, void>)(*(ulong*)(*(long*)pAppPreviewer + 96)))((nint)pAppPreviewer, targetFPS);
		}
	}

	public unsafe virtual void SetThrottleMode([MarshalAs(UnmanagedType.U1)] bool shouldThrottle)
	{
		//IL_0053: Expected I, but got I8
		IPreviewer* pAppPreviewer = m_pAppPreviewer;
		if (pAppPreviewer == null)
		{
			if (!global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F6_003F_003FSetThrottleMode_0040ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMX_N_0040Z_00404_NA)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EH_0040GNPLKAIE_0040Tried_003F5to_003F5set_003F5the_003F5throttle_003F5mode_003F5a_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BD_0040OIKMPLCE_0040_003F_0024CB_003F_0024CCm_pAppPreviewer_003F_0024CC_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 590u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F6_003F_003FSetThrottleMode_0040ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMX_N_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
		}
		else
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, byte, void>)(*(ulong*)(*(long*)pAppPreviewer + 104)))((nint)pAppPreviewer, shouldThrottle ? ((byte)1) : ((byte)0));
		}
	}

	public unsafe virtual void FlushMessages()
	{
		//IL_0055: Expected I, but got I8
		IPreviewer* pAppPreviewer = m_pAppPreviewer;
		if (pAppPreviewer == null)
		{
			if (!global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F6_003F_003FFlushMessages_0040ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EH_0040PHBKLIAP_0040Tried_003F5to_003F5flush_003F5engine_003F5messages_003F5a_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BD_0040OIKMPLCE_0040_003F_0024CB_003F_0024CCm_pAppPreviewer_003F_0024CC_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 577u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F6_003F_003FFlushMessages_0040ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
		}
		else
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(long*)pAppPreviewer + 200)))((nint)pAppPreviewer);
		}
	}

	public unsafe virtual ICachedAsset CacheAsset(IInstanceEntity entity)
	{
		//IL_007a: Expected I, but got I8
		//IL_009c: Expected I, but got I8
		Uri uri = null;
		IOStringWrapper iOStringWrapper = null;
		StandardStringWrapper standardStringWrapper = null;
		if (entity == null)
		{
			return null;
		}
		if (m_pAppPreviewer == null)
		{
			return null;
		}
		string xMLPath = entity.GetXMLPath();
		uri = null;
		if (Uri.TryCreate(xMLPath, UriKind.Absolute, out uri) && !(uri == null))
		{
			IOStringWrapper iOStringWrapper2 = new IOStringWrapper(uri.LocalPath);
			ICachedAsset result;
			try
			{
				iOStringWrapper = iOStringWrapper2;
				Firaxis.CivTech.AssetObjects.InstanceEntity instanceEntity = (Firaxis.CivTech.AssetObjects.InstanceEntity)entity;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer);
				try
				{
					instanceEntity.SerializeInternal(&memoryBuffer);
					StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(entity.Name);
					try
					{
						standardStringWrapper = standardStringWrapper2;
						IPreviewer* ptr = (IPreviewer*)((ulong)(nint)m_pAppPreviewer + 24uL);
						((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.InstanceType, sbyte*, char*, sbyte*, void>)(*(ulong*)(*(ulong*)ptr)))((nint)ptr, (global::AssetObjects.InstanceType)entity.Type, standardStringWrapper.Value, iOStringWrapper.Value, (sbyte*)global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer));
						result = new ManagedCachedAsset(m_pAppPreviewer, iOStringWrapper.Value, entity, new string((sbyte*)global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer)));
					}
					catch
					{
						//try-fault
						((IDisposable)standardStringWrapper).Dispose();
						throw;
					}
					((IDisposable)standardStringWrapper).Dispose();
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
					throw;
				}
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			((IDisposable)iOStringWrapper).Dispose();
			return result;
		}
		return null;
	}

	public unsafe virtual void ReloadEntity(Firaxis.CivTech.AssetObjects.InstanceType enttype, string entname)
	{
		//IL_0016: Expected I, but got I8
		//IL_002a: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(entname);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			IPreviewer* ptr = (IPreviewer*)((ulong)(nint)m_pAppPreviewer + 24uL);
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.InstanceType, sbyte*, void>)(*(ulong*)(*(long*)ptr + 24)))((nint)ptr, (global::AssetObjects.InstanceType)enttype, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	public unsafe virtual IPreviewDisplay CreateDisplay(IntPtr hWindow)
	{
		//IL_0068: Expected I8, but got I
		//IL_0081: Expected I, but got I8
		//IL_0038: Expected I, but got I8
		if (m_pAppPreviewer == null)
		{
			return null;
		}
		HWND__* ptr = (HWND__*)hWindow.ToPointer();
		if (!global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F4_003F_003FCreateDisplay_0040ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIPreviewDisplay_0040345_0040VIntPtr_0040System_0040_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_04IFJNELPM_0040hWnd_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 678u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F4_003F_003FCreateDisplay_0040ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIPreviewDisplay_0040345_0040VIntPtr_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out tagRECT tagRECT2);
		global::_003CModule_003E.GetWindowRect(ptr, &tagRECT2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out DisplayParameters displayParameters);
		System.Runtime.CompilerServices.Unsafe.As<DisplayParameters, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref displayParameters, 8)) = System.Runtime.CompilerServices.Unsafe.As<tagRECT, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref tagRECT2, 8)) - *(int*)(&tagRECT2);
		System.Runtime.CompilerServices.Unsafe.As<DisplayParameters, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref displayParameters, 12)) = System.Runtime.CompilerServices.Unsafe.As<tagRECT, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref tagRECT2, 12)) - System.Runtime.CompilerServices.Unsafe.As<tagRECT, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref tagRECT2, 4));
		*(long*)(&displayParameters) = (nint)ptr;
		IPreviewer* pAppPreviewer = m_pAppPreviewer;
		DisplayID displayID = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, DisplayParameters*, DisplayID>)(*(ulong*)(*(long*)pAppPreviewer + 136)))((nint)pAppPreviewer, &displayParameters);
		if (displayID == (DisplayID)0u)
		{
			return null;
		}
		return new DisplayWrapper(displayID, m_pAppPreviewer);
	}

	public virtual void DestroyDisplay(IPreviewDisplay pmDisplay)
	{
		((DisplayWrapper)pmDisplay).Destroy();
	}

	public unsafe virtual IPreviewWindow OpenWindow(IntPtr hWindow, IInstanceSet pmInstanceSet)
	{
		//IL_0068: Expected I8, but got I
		//IL_007e: Expected I, but got I8
		//IL_0038: Expected I, but got I8
		if (m_pAppPreviewer == null)
		{
			return null;
		}
		HWND__* ptr = (HWND__*)hWindow.ToPointer();
		if (!global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F4_003F_003FOpenWindow_0040ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIPreviewWindow_0040345_0040VIntPtr_0040System_0040_0040PE_0024AAUIInstanceSet_0040AssetObjects_004045_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_04IFJNELPM_0040hWnd_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 641u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F4_003F_003FOpenWindow_0040ManagedAssetPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIPreviewWindow_0040345_0040VIntPtr_0040System_0040_0040PE_0024AAUIInstanceSet_0040AssetObjects_004045_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out tagRECT tagRECT2);
		global::_003CModule_003E.GetWindowRect(ptr, &tagRECT2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out WindowParameters windowParameters);
		System.Runtime.CompilerServices.Unsafe.As<WindowParameters, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref windowParameters, 8)) = System.Runtime.CompilerServices.Unsafe.As<tagRECT, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref tagRECT2, 8)) - *(int*)(&tagRECT2);
		System.Runtime.CompilerServices.Unsafe.As<WindowParameters, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref windowParameters, 12)) = System.Runtime.CompilerServices.Unsafe.As<tagRECT, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref tagRECT2, 12)) - System.Runtime.CompilerServices.Unsafe.As<tagRECT, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref tagRECT2, 4));
		*(long*)(&windowParameters) = (nint)ptr;
		IPreviewer* pAppPreviewer = m_pAppPreviewer;
		WindowID windowID = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowParameters*, WindowID>)(*(ulong*)(*(long*)pAppPreviewer + 120)))((nint)pAppPreviewer, &windowParameters);
		if (windowID == (WindowID)0u)
		{
			return null;
		}
		WindowWrapper windowWrapper = new WindowWrapper(windowID, m_pAppPreviewer, m_pmCivTechSvc, m_pmXLPRegistry, pmInstanceSet);
		windowWrapper.KnobManagerProp = m_pmKnobManager;
		windowWrapper.m_pmWidgetManager = m_pmWidgetManager;
		return windowWrapper;
	}

	public virtual void CloseWindow(IPreviewWindow pmWindow)
	{
		((WindowWrapper)pmWindow).Close();
	}

	public unsafe virtual IEnumerable<string> GetAllowedLightRigClasses(string pmModuleName)
	{
		return global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EGetAllowedLightRigClasses(m_pAppPreviewer, pmModuleName, m_pmCivTechSvc.PrimaryProject.Config.XLPClasses);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DoesModuleSupportsLighting(string pmModuleName)
	{
		return global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EDoesModuleSupportsLighting(m_pAppPreviewer, pmModuleName, m_pmCivTechSvc.PrimaryProject.Config.XLPClasses);
	}

	public unsafe virtual IEnumerable<IPreviewerSlotInfo> GetSlotsInfo(string pmModuleName)
	{
		return global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EGetSlotsInfo(m_pAppPreviewer, pmModuleName);
	}

	public unsafe string GetDefaultLightRigName(string pmModuleName)
	{
		return global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EGetDefaultLightRigName(m_pAppPreviewer, pmModuleName, m_pmCivTechSvc.PrimaryProject.Config.XLPClasses);
	}

	public unsafe string GetDefaultAssetName(string pmModuleName)
	{
		return global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EGetDefaultAssetName(m_pAppPreviewer, pmModuleName);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DoesSupportsModule(string pmModuleName)
	{
		//IL_0025: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmModuleName);
		byte result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			IPreviewer* pAppPreviewer = m_pAppPreviewer;
			result = (byte)(((long)(nint)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, IPreviewModule*>)(*(ulong*)(*(long*)pAppPreviewer + 112)))((nint)pAppPreviewer, standardStringWrapper.Value) != 0) ? 1u : 0u);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result != 0;
	}

	public unsafe virtual void Shutdown()
	{
		//IL_0054: Expected I, but got I8
		//IL_005c: Expected I, but got I8
		//IL_0089: Expected I, but got I8
		//IL_0092: Expected I, but got I8
		KnobManager pmKnobManager = m_pmKnobManager;
		if (pmKnobManager != null)
		{
			pmKnobManager.KnobChangesComplete -= m_pmKnobChangeCompleteHandler;
			((IDisposable)m_pmKnobManager)?.Dispose();
			m_pmKnobManager = null;
			m_pmKnobChangeCompleteHandler = null;
		}
		IPreviewer* pAppPreviewer = m_pAppPreviewer;
		if (pAppPreviewer != null)
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(long*)pAppPreviewer + 224)))((nint)pAppPreviewer);
			m_pAppPreviewer = null;
		}
		WidgetManager pmWidgetManager = m_pmWidgetManager;
		if (pmWidgetManager != null)
		{
			((IDisposable)pmWidgetManager).Dispose();
			m_pmWidgetManager = null;
		}
		IToolContext* pToolContext = m_pToolContext;
		if (pToolContext != null)
		{
			IToolContext* ptr = pToolContext;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, uint, void*>)(*(ulong*)(*(ulong*)ptr)))((nint)ptr, 1u);
			m_pToolContext = null;
		}
	}

	public unsafe virtual void LogResources()
	{
		//IL_0017: Expected I, but got I8
		IPreviewer* pAppPreviewer = m_pAppPreviewer;
		if (pAppPreviewer != null)
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(long*)pAppPreviewer + 80)))((nint)pAppPreviewer);
		}
	}

	internal void RemoveReferences()
	{
		Shutdown();
	}

	private void RaiseLogEvent(string c, LogLevel logLevel, string s)
	{
		raise_Logger(c, logLevel, s);
	}

	private void OnKnobMgrEvent_KnobChangesComplete(object obj, EventArgs args)
	{
		raise_KnobChangesComplete(this, args);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EManagedAssetPreviewer();
			return;
		}
		try
		{
			Shutdown();
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

	~ManagedAssetPreviewer()
	{
		Dispose(A_0: false);
	}
}

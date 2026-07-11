using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetPreviewer;
using Firaxis.Utility;
using Platform;
using Reflection;
using std;
using ToolHost;

namespace Firaxis.CivTech.AssetPreviewer;

public class AudioEventPreviewer : IAudioPreviewer
{
	private unsafe shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E* m_audioPreviewer;

	private unsafe IToolHostProjectContext* m_toolHostContext;

	public unsafe AudioEventPreviewer()
	{
		//IL_000e: Expected I, but got I8
		//IL_0038: Expected I, but got I8
		m_toolHostContext = null;
		shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E* ptr = (shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E*)global::_003CModule_003E.@new(16uL, (int)global::_003CModule_003E.Platform_002EGetMemBlockType(), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040FHIEBNJB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 240, 23, 0);
		m_audioPreviewer = ((ptr == null) ? null : global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_007Bctor_007D(ptr));
	}

	private unsafe void _007EAudioEventPreviewer()
	{
		//IL_002a: Expected I, but got I8
		//IL_0040: Expected I, but got I8
		//IL_0049: Expected I, but got I8
		Shutdown(bStopWwise: false);
		shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E* audioPreviewer = m_audioPreviewer;
		if (audioPreviewer != null)
		{
			shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E* ptr = audioPreviewer;
			global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_007Bdtor_007D(ptr);
			global::_003CModule_003E.delete(ptr, 16uL);
			m_audioPreviewer = null;
		}
		IToolHostProjectContext* toolHostContext = m_toolHostContext;
		if (toolHostContext != null)
		{
			IToolHostProjectContext* ptr2 = toolHostContext;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, uint, void*>)(*(ulong*)(*(ulong*)ptr2)))((nint)ptr2, 1u);
			m_toolHostContext = null;
		}
	}

	public unsafe virtual void Startup(ICivTechService civTech, [MarshalAs(UnmanagedType.U1)] bool bColdStart)
	{
		//IL_0058: Expected I, but got I8
		//IL_0039: Expected I, but got I8
		//IL_00f4: Expected I, but got I8
		//IL_00f6: Expected I8, but got I
		//IL_0103: Expected I, but got I8
		//IL_00d3: Expected I, but got I8
		//IL_017f: Expected I, but got I8
		//IL_014b: Expected I, but got I8
		ToolHostInterface toolHostInterface = (ToolHostInterface)civTech.ToolHostLoader.ToolHostInterface;
		if (!global::_003CModule_003E._003FA0x1e01b2d9_002E_003FbIgnoreAlways_0040_003F2_003F_003FStartup_0040AudioEventPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICivTechService_004045_0040_N_0040Z_00404_NA && toolHostInterface == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040JPOMFLOK_0040toolHostInterface_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040FHIEBNJB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 266u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1e01b2d9_002E_003FbIgnoreAlways_0040_003F2_003F_003FStartup_0040AudioEventPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICivTechService_004045_0040_N_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		IToolHostModuleMgrClient* toolHostModuleMgr = toolHostInterface.GetToolHostModuleMgr();
		AudioPreviewerInterface* ptr = (AudioPreviewerInterface*)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, TypeInfo*, int, void*>)(*(ulong*)(*(long*)toolHostModuleMgr + 16)))((nint)toolHostModuleMgr, global::_003CModule_003E.ToolHost_002EAudioPreviewerInterface_002EGetTypeInfo(), 11);
		if (ptr == null)
		{
			ulong num = 1uL;
			ulong num2 = 1uL;
			global::_003CModule_003E.Platform_002EGetLoggingOptions(&num, &num2);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BBH_0040D _0024ArrayType_0024_0024_0024BY0BBH_0040D2);
			uint num3 = (uint)global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), 279uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EF_0040NNFHFDLM_0040Unable_003F5to_003F5get_003F5AudioPreviewerInte_0040), __arglist(11));
			global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BO_0040BNMNPGEM_0040A_003F_0024AAu_003F_0024AAd_003F_0024AAi_003F_0024AAo_003F_0024AAP_003F_0024AAr_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAe_003F_0024AAw_003F_0024AAe_003F_0024AAr_003F_0024AA_003F_0024AA_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), num3);
			throw new InvalidOperationException(string.Format("Unable to get AudioPreviewerInterface version %d from tool host DLL!", 11));
		}
		if (!global::_003CModule_003E._003FA0x1e01b2d9_002E_003FbIgnoreAlways_0040_003FDO_0040_003F_003FStartup_0040AudioEventPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICivTechService_004045_0040_N_0040Z_00404_NA && m_toolHostContext != null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BD_0040KDPOHFBP_0040_003F_0024CBm_toolHostContext_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040FHIEBNJB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 277u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1e01b2d9_002E_003FbIgnoreAlways_0040_003FDO_0040_003F_003FStartup_0040AudioEventPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICivTechService_004045_0040_N_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		IToolHostProjectContext* ptr2 = (m_toolHostContext = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_003FA0x1e01b2d9_002ECreateProjectContext(civTech));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E2);
		long num4 = (nint)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E*, IToolHostProjectContext*, shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E*>)(*(ulong*)(*(long*)ptr + 32)))((nint)ptr, &shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E2, ptr2);
		try
		{
			global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_003D(m_audioPreviewer, (shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E*)num4);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E*, void>)(&global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_007Bdtor_007D), &shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E2);
			throw;
		}
		global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_007Bdtor_007D(&shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E2);
		if (!global::_003CModule_003E._003FA0x1e01b2d9_002E_003FbIgnoreAlways_0040_003FEH_0040_003F_003FStartup_0040AudioEventPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICivTechService_004045_0040_N_0040Z_00404_NA && !global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040HGHNNLCD_0040_003F_0024CKm_audioPreviewer_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040FHIEBNJB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 282u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1e01b2d9_002E_003FbIgnoreAlways_0040_003FEH_0040_003F_003FStartup_0040AudioEventPreviewer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICivTechService_004045_0040_N_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			throw new InvalidOperationException("Unable to create the Audio Previewer!");
		}
		global::AssetPreviewer.IAudioPreviewer* ptr3 = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
		if (((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, byte, byte>)(*(ulong*)(*(long*)ptr3 + 8)))((nint)ptr3, bColdStart ? ((byte)1) : ((byte)0)) == 0)
		{
			throw new InvalidOperationException("Unable to start up the Audio System!");
		}
		Context.Add(new SoundEventProvider(this));
	}

	public unsafe virtual void Shutdown([MarshalAs(UnmanagedType.U1)] bool bStopWwise)
	{
		//IL_0031: Expected I, but got I8
		shared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E* audioPreviewer = m_audioPreviewer;
		if (audioPreviewer != null && global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(audioPreviewer))
		{
			Context.Remove<SoundEventProvider>();
			global::AssetPreviewer.IAudioPreviewer* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, byte, void>)(*(ulong*)(*(long*)ptr + 16)))((nint)ptr, bStopWwise ? ((byte)1) : ((byte)0));
			global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002Ereset(m_audioPreviewer);
		}
	}

	public unsafe virtual void ReloadSoundBanks()
	{
		//IL_0024: Expected I, but got I8
		if (global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			global::AssetPreviewer.IAudioPreviewer* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(long*)intPtr + 24)))((nint)intPtr);
		}
	}

	public unsafe virtual int GetBankID(string bankName)
	{
		//IL_0039: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			return 0;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(bankName);
		int result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetPreviewer.IAudioPreviewer* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
			result = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, int>)(*(ulong*)(*(long*)ptr + 32)))((nint)ptr, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual int GetNumBankEventNames(int iBankID)
	{
		//IL_0029: Expected I, but got I8
		if (!global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			return 0;
		}
		global::AssetPreviewer.IAudioPreviewer* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
		return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, int, int>)(*(ulong*)(*(long*)ptr + 40)))((nint)ptr, iBankID);
	}

	public unsafe virtual string GetBankEventName(int iBankID, int iEventIndex)
	{
		//IL_002e: Expected I, but got I8
		if (!global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			return string.Empty;
		}
		global::AssetPreviewer.IAudioPreviewer* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
		sbyte* ptr2 = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, int, int, sbyte*>)(*(ulong*)(*(long*)ptr + 48)))((nint)ptr, iBankID, iEventIndex);
		if (ptr2 == null)
		{
			return string.Empty;
		}
		return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(ptr2);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool GetBankCategory(string sBankName, int iTypeToCheck)
	{
		//IL_003a: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			return false;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(sBankName);
		bool result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetPreviewer.IAudioPreviewer* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
			result = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, int, byte>)(*(ulong*)(*(long*)ptr + 56)))((nint)ptr, standardStringWrapper.Value, iTypeToCheck) != 0;
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual void PlaySoundEvent(string sEventName)
	{
		//IL_0037: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(sEventName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::AssetPreviewer.IAudioPreviewer* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, void>)(*(ulong*)(*(long*)ptr + 64)))((nint)ptr, standardStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
	}

	public unsafe virtual void StopAllSounds()
	{
		//IL_0017: Expected I, but got I8
		global::AssetPreviewer.IAudioPreviewer* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
		((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(long*)intPtr + 72)))((nint)intPtr);
	}

	public unsafe virtual int GetNumSoundBanks()
	{
		//IL_0026: Expected I, but got I8
		if (!global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			return 0;
		}
		global::AssetPreviewer.IAudioPreviewer* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
		return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, int>)(*(ulong*)(*(long*)intPtr + 80)))((nint)intPtr);
	}

	public unsafe virtual string GetSoundBankName(int iBank)
	{
		//IL_002d: Expected I, but got I8
		if (!global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			return string.Empty;
		}
		global::AssetPreviewer.IAudioPreviewer* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
		return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, int, sbyte*>)(*(ulong*)(*(long*)ptr + 88)))((nint)ptr, iBank));
	}

	public unsafe virtual uint GetNumSwitchGroups()
	{
		//IL_0026: Expected I, but got I8
		if (!global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			return 0u;
		}
		global::AssetPreviewer.IAudioPreviewer* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
		return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, uint>)(*(ulong*)(*(long*)intPtr + 96)))((nint)intPtr);
	}

	public unsafe virtual uint GetNumSwitchSettings(string sGroupName)
	{
		//IL_0039: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			return 0u;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(sGroupName);
		uint result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetPreviewer.IAudioPreviewer* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
			result = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, uint>)(*(ulong*)(*(long*)ptr + 104)))((nint)ptr, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual string GetSwitchGroupName(uint uSwitch)
	{
		//IL_002d: Expected I, but got I8
		if (!global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			return string.Empty;
		}
		global::AssetPreviewer.IAudioPreviewer* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
		return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, uint, sbyte*>)(*(ulong*)(*(long*)ptr + 112)))((nint)ptr, uSwitch));
	}

	public unsafe virtual string GetSwitchSettingName(string sGroupName, uint uSwitch)
	{
		//IL_003e: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			return string.Empty;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(sGroupName);
		string result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetPreviewer.IAudioPreviewer* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
			result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, uint, sbyte*>)(*(ulong*)(*(long*)ptr + 120)))((nint)ptr, standardStringWrapper.Value, uSwitch));
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual void SetPlaybackSwitch(string sGroupName, string sSwitchName)
	{
		//IL_004d: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		if (!global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			return;
		}
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(sGroupName);
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(sSwitchName);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				global::AssetPreviewer.IAudioPreviewer* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, sbyte*, void>)(*(ulong*)(*(long*)ptr + 128)))((nint)ptr, standardStringWrapper.Value, standardStringWrapper2.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper2).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper2).Dispose();
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	public unsafe virtual void UnloadProjectData()
	{
		//IL_0027: Expected I, but got I8
		if (global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			global::AssetPreviewer.IAudioPreviewer* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(long*)intPtr + 136)))((nint)intPtr);
		}
	}

	public unsafe virtual void LoadProjectData()
	{
		//IL_0027: Expected I, but got I8
		if (global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002E_N(m_audioPreviewer))
		{
			global::AssetPreviewer.IAudioPreviewer* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AIAudioPreviewer_003E_002E_002D_003E(m_audioPreviewer);
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(long*)intPtr + 144)))((nint)intPtr);
		}
	}

	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EAudioEventPreviewer();
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

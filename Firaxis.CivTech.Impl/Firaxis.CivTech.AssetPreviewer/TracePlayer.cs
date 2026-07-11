using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetPreviewer;
using Firaxis.CivTech.AssetPreviewer._INTERNAL;
using Platform;
using ToolHost;

namespace Firaxis.CivTech.AssetPreviewer;

internal class TracePlayer : ITracePlayer
{
	private LogEventHandler _003Cbacking_store_003ELogger;

	private unsafe IPreviewer* m_pPreviewer;

	private unsafe IToolContext* m_pToolContext;

	private unsafe global::AssetPreviewer.ITracePlayer* m_pPlayer;

	private unsafe IPlaybackContext* m_pPlayerContext;

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

	public unsafe virtual void Play()
	{
		//IL_0014: Expected I, but got I8
		global::AssetPreviewer.ITracePlayer* pPlayer = m_pPlayer;
		if (pPlayer != null)
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetPreviewer.ITracePlayer.TimingMode, void>)(*(ulong*)(*(ulong*)pPlayer)))((nint)pPlayer, (global::AssetPreviewer.ITracePlayer.TimingMode)0);
		}
	}

	internal unsafe TracePlayer(ICivTechService civ, string TracePath)
	{
		//IL_0013: Expected I, but got I8
		//IL_001b: Expected I, but got I8
		//IL_0023: Expected I, but got I8
		//IL_002b: Expected I, but got I8
		//IL_0068: Expected I, but got I8
		//IL_0163: Expected I, but got I8
		//IL_0165: Expected I8, but got I
		//IL_016d: Expected I, but got I8
		//IL_0239: Expected I, but got I8
		LogEventHandler logEventHandler = null;
		ManagedLogger managedLogger = null;
		Exception ex = null;
		uint num = 0u;
		m_pPreviewer = null;
		m_pToolContext = null;
		m_pPlayer = null;
		m_pPlayerContext = null;
		base._002Ector();
		try
		{
			ToolHostInterface toolHostInterface = (ToolHostInterface)civ.ToolHostLoader.ToolHostInterface;
			if (!global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F3_003F_003F_003F0TracePlayer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAUICivTechService_004034_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && toolHostInterface == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040JPOMFLOK_0040toolHostInterface_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 880u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xbc03a39c_002E_003FbIgnoreAlways_0040_003F3_003F_003F_003F0TracePlayer_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAUICivTechService_004034_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			if (!File.Exists(toolHostInterface.DllPath))
			{
				uint num2 = 279u;
				uint num3 = 0u;
				ulong num4 = 1uL;
				ulong num5 = 1uL;
				global::_003CModule_003E.Platform_002EGetLoggingOptions(&num4, &num5);
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BBH_0040D _0024ArrayType_0024_0024_0024BY0BBH_0040D2);
				num3 = (uint)global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), 279uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CF_0040GDINFLLF_0040Unable_003F5to_003F5find_003F5ToolHost_003F5DLL_003F5on_003F5d_0040), __arglist());
				global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BO_0040PICHLLLM_0040A_003F_0024AAs_003F_0024AAs_003F_0024AAe_003F_0024AAt_003F_0024AAP_003F_0024AAr_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAe_003F_0024AAw_003F_0024AAe_003F_0024AAr_003F_0024AA_003F_0024AA_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), num3);
				throw new Exception($"ToolHost .DLL located at path:\n\t{toolHostInterface.DllPath}\n\n does not exist on disk.  Consider reinstalling the tools to correct this issue.");
			}
			AssetPreviewerInterface* ptr = global::_003CModule_003E.ToolHost_002EIToolHostModuleMgrClient_002EGetInterface_003Cclass_0020ToolHost_003A_003AAssetPreviewerInterface_003E(toolHostInterface.GetToolHostModuleMgr(), 38);
			if (ptr == null)
			{
				uint num6 = 279u;
				uint num7 = 0u;
				ulong num8 = 1uL;
				ulong num9 = 1uL;
				global::_003CModule_003E.Platform_002EGetLoggingOptions(&num8, &num9);
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BBH_0040D _0024ArrayType_0024_0024_0024BY0BBH_0040D3);
				num7 = (uint)global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D3), 279uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EF_0040IPMCGNH_0040Unable_003F5to_003F5get_003F5AssetPreviewerInte_0040), __arglist(38));
				global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BO_0040PICHLLLM_0040A_003F_0024AAs_003F_0024AAs_003F_0024AAe_003F_0024AAt_003F_0024AAP_003F_0024AAr_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAe_003F_0024AAw_003F_0024AAe_003F_0024AAr_003F_0024AA_003F_0024AA_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D3), num7);
				throw new Exception($"Unable to get AssetPreviewerInterface version {38} from tool host DLL!");
			}
			ToolContextImpl* ptr2 = (ToolContextImpl*)(m_pToolContext = (IToolContext*)global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_003FA0xbc03a39c_002ECreateToolContext(civ, null, null, RaiseLogEvent));
			long num10 = (nint)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, IToolContext*, IPreviewer*>)(*(ulong*)(*(long*)ptr + 32)))((nint)ptr, (IToolContext*)ptr2);
			m_pPreviewer = (IPreviewer*)num10;
			if (num10 == 0L)
			{
				uint num11 = 279u;
				uint num12 = 0u;
				ulong num13 = 1uL;
				ulong num14 = 1uL;
				global::_003CModule_003E.Platform_002EGetLoggingOptions(&num13, &num14);
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BBH_0040D _0024ArrayType_0024_0024_0024BY0BBH_0040D4);
				num12 = (uint)global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D4), 279uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ED_0040FGMHINPG_0040Unable_003F5to_003F5initialize_003F5AssetPrevie_0040), __arglist(38));
				global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BO_0040PICHLLLM_0040A_003F_0024AAs_003F_0024AAs_003F_0024AAe_003F_0024AAt_003F_0024AAP_003F_0024AAr_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAe_003F_0024AAw_003F_0024AAe_003F_0024AAr_003F_0024AA_003F_0024AA_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D4), num12);
				throw new Exception($"Unable to initialize AssetPreviewer version {38} from tool host DLL!");
			}
			logEventHandler = RaiseLogEvent;
			managedLogger = new ManagedLogger(logEventHandler);
			int num15 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			PlaybackContextImpl* ptr3 = (PlaybackContextImpl*)global::_003CModule_003E.@new(448uL, num15, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 916, 23, 0);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AManagedLogger_0020_005E_003E obj);
			PlaybackContextImpl* ptr4;
			try
			{
				if (ptr3 != null)
				{
					global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AManagedLogger_0020_005E_003E_002E_007Bctor_007D(&obj, managedLogger);
					try
					{
						num |= 1;
						ptr4 = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EPlaybackContextImpl_002E_007Bctor_007D(ptr3, &obj);
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
					ptr4 = null;
				}
				try
				{
					PlaybackContextImpl* ptr5 = ptr4;
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
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr3, num15, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040NPBIJLKF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 916, 23, 0);
				throw;
			}
			try
			{
				m_pPlayerContext = (IPlaybackContext*)ptr4;
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
			char* ptr6 = (char*)Marshal.StringToHGlobalUni(TracePath).ToPointer();
			m_pPlayer = global::_003CModule_003E.AssetPreviewer_002ECreatePlayer(m_pPlayerContext, m_pPreviewer, ptr6);
			IntPtr hglobal = new IntPtr(ptr6);
			Marshal.FreeHGlobal(hglobal);
			if (m_pPlayer == null)
			{
				throw new Exception($"Unable to create player for trace file {TracePath}");
			}
		}
		catch (Exception ex2)
		{
			Teardown();
			throw ex2;
		}
	}

	private void _007ETracePlayer()
	{
		Teardown();
	}

	private unsafe void Teardown()
	{
		//IL_0019: Expected I, but got I8
		//IL_0021: Expected I, but got I8
		//IL_003b: Expected I, but got I8
		//IL_0055: Expected I, but got I8
		//IL_005d: Expected I, but got I8
		//IL_0073: Expected I, but got I8
		//IL_007c: Expected I, but got I8
		global::AssetPreviewer.ITracePlayer* pPlayer = m_pPlayer;
		if (pPlayer != null)
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(long*)pPlayer + 8)))((nint)pPlayer);
			m_pPlayer = null;
		}
		IPlaybackContext* pPlayerContext = m_pPlayerContext;
		if (pPlayerContext != null)
		{
			global::_003CModule_003E.delete(pPlayerContext, 8uL);
			m_pPlayerContext = null;
		}
		IPreviewer* pPreviewer = m_pPreviewer;
		if (pPreviewer != null)
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(long*)pPreviewer + 224)))((nint)pPreviewer);
			m_pPreviewer = null;
		}
		IToolContext* pToolContext = m_pToolContext;
		if (pToolContext != null)
		{
			IToolContext* ptr = pToolContext;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, uint, void*>)(*(ulong*)(*(ulong*)ptr)))((nint)ptr, 1u);
			m_pToolContext = null;
		}
	}

	private void RaiseLogEvent(string c, LogLevel logLevel, string s)
	{
		raise_Logger(c, logLevel, s);
	}

	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			Teardown();
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

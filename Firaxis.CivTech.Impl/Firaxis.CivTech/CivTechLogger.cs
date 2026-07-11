using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Platform;

namespace Firaxis.CivTech;

internal class CivTechLogger : ICivTechLogger
{
	private SourcedLogEventHandler _003Cbacking_store_003EEngineLog;

	private unsafe delegate* unmanaged[Cdecl, Cdecl]<char*, sbyte*, uint, void> m_defaultEventCallback;

	private static CivTechLogger s_this = null;

	[SpecialName]
	public virtual event SourcedLogEventHandler EngineLog
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003EEngineLog = (SourcedLogEventHandler)Delegate.Combine(_003Cbacking_store_003EEngineLog, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003EEngineLog = (SourcedLogEventHandler)Delegate.Remove(_003Cbacking_store_003EEngineLog, value);
		}
	}

	public unsafe CivTechLogger()
	{
		//IL_002d: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x75fc24aa_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CivTechLogger_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040XZ_00404_NA && s_this != null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040NPCJFNJ_0040s_this_003F5_003F_0024DN_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040HPLDEGCL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 43u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x75fc24aa_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CivTechLogger_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040XZ_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		s_this = this;
		m_defaultEventCallback = global::_003CModule_003E.Platform_002EGetLogEventCallback();
		global::_003CModule_003E.Platform_002ESetLogEventCallback((delegate* unmanaged[Cdecl, Cdecl]<char*, sbyte*, uint, void>)global::_003CModule_003E.__unep_0040_003FEngineLogCallback_0040CivTech_0040Firaxis_0040_0040_0024_0024FYAXPEB_SPEBDI_0040Z);
	}

	private void _007ECivTechLogger()
	{
		_0021CivTechLogger();
	}

	private unsafe void _0021CivTechLogger()
	{
		//IL_0008: Expected I, but got I8
		//IL_000f: Expected I, but got I8
		m_defaultEventCallback = null;
		global::_003CModule_003E.Platform_002ESetLogEventCallback(null);
		s_this = null;
	}

	[SpecialName]
	protected virtual void raise_EngineLog(LogEventType value0, string value1, string value2)
	{
		_003Cbacking_store_003EEngineLog?.Invoke(value0, value1, value2);
	}

	public virtual void AddLogItem(LogEventType evtType, string source, string msg)
	{
		raise_EngineLog(evtType, source, msg);
	}

	public unsafe static CivTechLogger GetInstance()
	{
		//IL_0027: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x75fc24aa_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetInstance_0040CivTechLogger_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAV234_0040XZ_00404_NA && s_this == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06PDBPBDGM_0040s_this_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040HPLDEGCL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 15u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x75fc24aa_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetInstance_0040CivTechLogger_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAV234_0040XZ_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		return s_this;
	}

	public unsafe delegate* unmanaged[Cdecl, Cdecl]<char*, sbyte*, uint, void> GetDefaultCallback()
	{
		return m_defaultEventCallback;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021CivTechLogger();
			return;
		}
		try
		{
			_0021CivTechLogger();
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

	~CivTechLogger()
	{
		Dispose(A_0: false);
	}
}

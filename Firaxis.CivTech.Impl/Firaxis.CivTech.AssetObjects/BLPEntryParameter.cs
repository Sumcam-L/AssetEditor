using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class BLPEntryParameter : Parameter, IBLPEntryParameter
{
	public unsafe virtual string LibraryName
	{
		get
		{
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EBLPEntryParameter_002EGetLibraryName((global::AssetObjects.BLPEntryParameter*)m_pkParameter));
		}
		set
		{
			//IL_0051: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			StandardStringWrapper standardStringWrapper = null;
			if (!global::_003CModule_003E._003FA0xe5ac78fd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040LibraryName_0040BLPEntryParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && value == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06GENFNPB_0040pmName_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HJ_0040DLOAJGHN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 93u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xe5ac78fd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040LibraryName_0040BLPEntryParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::AssetObjects.BLPEntryParameter* pkParameter = (global::AssetObjects.BLPEntryParameter*)m_pkParameter;
				global::_003CModule_003E.AssetObjects_002EBLPEntryParameter_002ESetLibraryName(pkParameter, standardStringWrapper.Value);
				global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002ESetLibraryName(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.BLPEntryValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), standardStringWrapper.Value);
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

	public unsafe virtual string XLPClassName
	{
		get
		{
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EBLPEntryParameter_002EGetXLPClassName((global::AssetObjects.BLPEntryParameter*)m_pkParameter));
		}
		set
		{
			//IL_0051: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			StandardStringWrapper standardStringWrapper = null;
			if (!global::_003CModule_003E._003FA0xe5ac78fd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040XLPClassName_0040BLPEntryParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && value == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06GENFNPB_0040pmName_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HJ_0040DLOAJGHN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 76u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xe5ac78fd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040XLPClassName_0040BLPEntryParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::AssetObjects.BLPEntryParameter* pkParameter = (global::AssetObjects.BLPEntryParameter*)m_pkParameter;
				global::_003CModule_003E.AssetObjects_002EBLPEntryParameter_002ESetXLPClassName(pkParameter, standardStringWrapper.Value);
				global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002ESetXLPClass(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.BLPEntryValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), standardStringWrapper.Value);
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

	public unsafe virtual string DefaultXLPPath
	{
		get
		{
			return ((IBLPEntryValue)DefaultValue).XLPPath;
		}
		set
		{
			//IL_0043: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			StandardStringWrapper standardStringWrapper = null;
			if (!global::_003CModule_003E._003FA0xe5ac78fd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultXLPPath_0040BLPEntryParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && value == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06HPMEJCKA_0040pmPath_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HJ_0040DLOAJGHN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 59u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xe5ac78fd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultXLPPath_0040BLPEntryParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::AssetObjects.Parameter* pkParameter = m_pkParameter;
				global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002ESetXLPPath(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.BLPEntryValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), standardStringWrapper.Value);
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

	public unsafe virtual string DefaultBLPPackage
	{
		get
		{
			return ((IBLPEntryValue)DefaultValue).BLPPackage;
		}
		set
		{
			//IL_0043: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			StandardStringWrapper standardStringWrapper = null;
			if (!global::_003CModule_003E._003FA0xe5ac78fd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultBLPPackage_0040BLPEntryParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && value == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09IJPNCJPM_0040pmPackage_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HJ_0040DLOAJGHN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 43u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xe5ac78fd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultBLPPackage_0040BLPEntryParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::AssetObjects.Parameter* pkParameter = m_pkParameter;
				global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002ESetBLPPackage(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.BLPEntryValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), standardStringWrapper.Value);
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

	public unsafe virtual string DefaultEntryName
	{
		get
		{
			return ((IBLPEntryValue)DefaultValue).EntryName;
		}
		set
		{
			//IL_0043: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			StandardStringWrapper standardStringWrapper = null;
			if (!global::_003CModule_003E._003FA0xe5ac78fd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultEntryName_0040BLPEntryParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && value == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040NHICJNAN_0040pmBLPEntry_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HJ_0040DLOAJGHN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 27u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xe5ac78fd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultEntryName_0040BLPEntryParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::AssetObjects.Parameter* pkParameter = m_pkParameter;
				global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002ESetEntryName(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.BLPEntryValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), standardStringWrapper.Value);
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

	public unsafe virtual bool IsNullAllowed
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002EBLPEntryParameter_002EIsNullAllowed((global::AssetObjects.BLPEntryParameter*)m_pkParameter);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetObjects_002EBLPEntryParameter_002ESetNullAllowed((global::AssetObjects.BLPEntryParameter*)m_pkParameter, value);
		}
	}

	public unsafe BLPEntryParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
	{
		bool flag = true;
		base._002Ector((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003ABLPEntryParameter_002Cbool_003E(pkParameterSet, szName, &flag));
	}

	public unsafe BLPEntryParameter(global::AssetObjects.BLPEntryParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
	}

	private unsafe global::AssetObjects.BLPEntryParameter* GetParameter()
	{
		return (global::AssetObjects.BLPEntryParameter*)m_pkParameter;
	}
}

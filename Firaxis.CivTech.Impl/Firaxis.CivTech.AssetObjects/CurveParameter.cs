using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class CurveParameter : Parameter, ICurveParameter
{
	private CurveValue DefaultCurveValue => (CurveValue)DefaultValue;

	public unsafe virtual bool ClampDomain
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			//IL_002d: Expected I, but got I8
			global::AssetObjects.CurveParameter* pkParameter = (global::AssetObjects.CurveParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040ClampDomain_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NXZ_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 128u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040ClampDomain_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return global::_003CModule_003E.AssetObjects_002ECurveParameter_002EGetClampDomain(pkParameter);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			//IL_002d: Expected I, but got I8
			global::AssetObjects.CurveParameter* pkParameter = (global::AssetObjects.CurveParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040ClampDomain_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMX_N_0040Z_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 137u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040ClampDomain_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMX_N_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			global::_003CModule_003E.AssetObjects_002ECurveParameter_002ESetClampDomain(pkParameter, value);
		}
	}

	public unsafe virtual bool AllowEmpty
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			//IL_002a: Expected I, but got I8
			global::AssetObjects.CurveParameter* pkParameter = (global::AssetObjects.CurveParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040AllowEmpty_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NXZ_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 110u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040AllowEmpty_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return global::_003CModule_003E.AssetObjects_002ECurveParameter_002EGetAllowEmptyCurves(pkParameter);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			//IL_002a: Expected I, but got I8
			global::AssetObjects.CurveParameter* pkParameter = (global::AssetObjects.CurveParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040AllowEmpty_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMX_N_0040Z_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 119u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040AllowEmpty_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMX_N_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			global::_003CModule_003E.AssetObjects_002ECurveParameter_002ESetAllowEmptyCurves(pkParameter, value);
		}
	}

	public unsafe virtual float DomainMinValue
	{
		get
		{
			//IL_002a: Expected I, but got I8
			global::AssetObjects.CurveParameter* pkParameter = (global::AssetObjects.CurveParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040DomainMinValue_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMMXZ_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 92u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040DomainMinValue_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMMXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return global::_003CModule_003E.AssetObjects_002ECurveParameter_002EGetDomainMin(pkParameter);
		}
		set
		{
			//IL_002a: Expected I, but got I8
			global::AssetObjects.CurveParameter* pkParameter = (global::AssetObjects.CurveParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DomainMinValue_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXM_0040Z_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 101u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DomainMinValue_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXM_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			global::_003CModule_003E.AssetObjects_002ECurveParameter_002ESetDomainMin(pkParameter, value);
		}
	}

	public unsafe virtual float DomainMaxValue
	{
		get
		{
			//IL_002a: Expected I, but got I8
			global::AssetObjects.CurveParameter* pkParameter = (global::AssetObjects.CurveParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040DomainMaxValue_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMMXZ_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 74u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040DomainMaxValue_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMMXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return global::_003CModule_003E.AssetObjects_002ECurveParameter_002EGetDomainMax(pkParameter);
		}
		set
		{
			//IL_002a: Expected I, but got I8
			global::AssetObjects.CurveParameter* pkParameter = (global::AssetObjects.CurveParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DomainMaxValue_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXM_0040Z_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 83u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DomainMaxValue_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXM_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			global::_003CModule_003E.AssetObjects_002ECurveParameter_002ESetDomainMax(pkParameter, value);
		}
	}

	public unsafe virtual IEnumerable<string> AllowedCurveClasses
	{
		get
		{
			//IL_0030: Expected I, but got I8
			List<string> list = new List<string>();
			global::AssetObjects.CurveParameter* pkParameter = (global::AssetObjects.CurveParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040AllowedCurveClasses_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040XZ_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 30u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040AllowedCurveClasses_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040XZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002ECurveParameter_002Eclasses_begin(pkParameter, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002ECurveParameter_002Eclasses_end(pkParameter, &const_iterator2)))
			{
				do
				{
					string item = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(item);
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002ECurveParameter_002Eclasses_end(pkParameter, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe CurveParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003ACurveParameter_003E(pkParameterSet, szName))
	{
		//IL_0035: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVParameterSet_00402_0040PEBD_0040Z_00404_NA && m_pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 21u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVParameterSet_00402_0040PEBD_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
	}

	public unsafe CurveParameter(global::AssetObjects.CurveParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
		//IL_002a: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0M_0040LANNKKMG_0040pkParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 14u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
	}

	public unsafe virtual void AllowCurveClass(string className)
	{
		//IL_002c: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.CurveParameter* pkParameter = (global::AssetObjects.CurveParameter*)m_pkParameter;
		if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003FAllowCurveClass_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 45u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003FAllowCurveClass_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(className);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.AssetObjects_002ECurveParameter_002EAllowCurveClass(pkParameter, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsCurveClassAllowed(string className)
	{
		//IL_002c: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.CurveParameter* pkParameter = (global::AssetObjects.CurveParameter*)m_pkParameter;
		if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003FIsCurveClassAllowed_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003FIsCurveClassAllowed_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(className);
		bool result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			result = global::_003CModule_003E.AssetObjects_002ECurveParameter_002EIsCurveClassAllowed(pkParameter, standardStringWrapper.Value);
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

	public unsafe virtual void ClearAllowedCurveClasses()
	{
		//IL_002a: Expected I, but got I8
		global::AssetObjects.CurveParameter* pkParameter = (global::AssetObjects.CurveParameter*)m_pkParameter;
		if (!global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003FClearAllowedCurveClasses_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040CFLKANF_0040nativeParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HG_0040LNBNNEKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 65u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x406026dd_002E_003FbIgnoreAlways_0040_003F2_003F_003FClearAllowedCurveClasses_0040CurveParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::_003CModule_003E.AssetObjects_002ECurveParameter_002EClearAllowedCurveClasses(pkParameter);
	}

	private unsafe global::AssetObjects.CurveParameter* GetParameter()
	{
		return (global::AssetObjects.CurveParameter*)m_pkParameter;
	}
}

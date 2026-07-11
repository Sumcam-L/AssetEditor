using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

internal class SplineSet : ISplineSet
{
	private List<ISpline> m_pmSplines;

	private unsafe global::AssetObjects.SplineSet* m_pkSplineSet;

	private unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	public virtual IEnumerable<ISpline> Splines => m_pmSplines;

	public unsafe SplineSet(global::AssetObjects.SplineSet* pkSplineSet, global::AssetObjects.Deserializer* pkDeserializer)
	{
		m_pmSplines = new List<ISpline>();
		m_pkSplineSet = pkSplineSet;
		m_pkDeserializer = pkDeserializer;
		base._002Ector();
		AddReferences();
	}

	public unsafe virtual ISpline AddSpline(string pmClassName)
	{
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddSpline_0040SplineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUISpline_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && m_pkSplineSet == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040DPOGDCCO_0040Using_003F5zombie_003F5object_003F_0024AA_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BJ_0040CPJFHDAK_0040m_pkSplineSet_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 210u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddSpline_0040SplineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUISpline_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		if (m_pkSplineSet == null)
		{
			return null;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmClassName);
		Spline spline;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			spline = new Spline(global::_003CModule_003E.AssetObjects_002ESplineSet_002EAppendSpline(m_pkSplineSet, standardStringWrapper.Value), m_pkDeserializer);
			m_pmSplines.Add(spline);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return spline;
	}

	public unsafe virtual void RemoveSpline(ISpline pmSpline)
	{
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveSpline_0040SplineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUISpline_0040345_0040_0040Z_00404_NA && m_pkSplineSet == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040DPOGDCCO_0040Using_003F5zombie_003F5object_003F_0024AA_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BJ_0040CPJFHDAK_0040m_pkSplineSet_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 226u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveSpline_0040SplineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUISpline_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		if (m_pkSplineSet != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmSpline.Name);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002ESplineSet_002ERemoveSpline(m_pkSplineSet, standardStringWrapper.Value);
				m_pmSplines.Remove(pmSpline);
				PatchReferences();
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

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkSplineSet = null;
		List<ISpline>.Enumerator enumerator = m_pmSplines.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((Spline)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmSplines.Clear();
	}

	internal unsafe virtual void PatchReferences()
	{
		if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FPatchReferences_0040SplineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA && m_pkSplineSet == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040DPOGDCCO_0040Using_003F5zombie_003F5object_003F_0024AA_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BJ_0040CPJFHDAK_0040m_pkSplineSet_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 252u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FPatchReferences_0040SplineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F9_003F_003FPatchReferences_0040SplineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA)
		{
			ulong num = (ulong)m_pmSplines.Count;
			if (global::_003CModule_003E.AssetObjects_002ESplineSet_002Esize(m_pkSplineSet) != num)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CD_0040MDGOHAMK_0040Managed_003F5and_003F5native_003F5are_003F5out_003F5of_003F5sy_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CM_0040EPBOBFEB_0040m_pkSplineSet_003F9_003F_0024DOsize_003F_0024CI_003F_0024CJ_003F5_003F_0024DN_003F_0024DN_003F5m_pmSpl_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 253u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F9_003F_003FPatchReferences_0040SplineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ASpline_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002ESplineSet_002Ebegin(m_pkSplineSet, &iterator);
		List<ISpline>.Enumerator enumerator = m_pmSplines.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((Spline)enumerator.Current).PatchReferences(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASpline_002C4096_003E_002Eiterator_002E_002A(&iterator));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASpline_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (enumerator.MoveNext());
		}
	}

	private unsafe void AddReferences()
	{
		if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040SplineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && m_pkSplineSet == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040DPOGDCCO_0040Using_003F5zombie_003F5object_003F_0024AA_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BJ_0040CPJFHDAK_0040m_pkSplineSet_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 240u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040SplineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddReferences_0040SplineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && m_pmSplines.Count != 0)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DM_0040PDMMDFKP_0040Adding_003F5references_003F5to_003F5managed_003F5lis_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BI_0040EDBJDLEG_0040m_pmSplines_003F9_003F_0024DOCount_003F5_003F_0024DN_003F_0024DN_003F50_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 241u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddReferences_0040SplineSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ASpline_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002ESplineSet_002Ebegin(m_pkSplineSet, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ASpline_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASpline_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ESplineSet_002Eend(m_pkSplineSet, &iterator2)))
		{
			do
			{
				Spline item = new Spline(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASpline_002C4096_003E_002Eiterator_002E_002A(&iterator), m_pkDeserializer);
				m_pmSplines.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASpline_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASpline_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ESplineSet_002Eend(m_pkSplineSet, &iterator2)));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

internal class Spline : ISpline
{
	private unsafe global::AssetObjects.Spline* m_pkSpline;

	private unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	private string m_pmName;

	private List<ISplineVertex> m_pmVerts;

	private ValueSet m_pmCookParams;

	public virtual IValueSet CookParameters => m_pmCookParams;

	public virtual int VertexCount => m_pmVerts.Count;

	public unsafe virtual bool ClosedLoop
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002ESpline_002EIsClosedLoop(m_pkSpline);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetObjects_002ESpline_002ESetIsClosedLoop(m_pkSpline, value);
		}
	}

	public unsafe virtual string ClassName
	{
		get
		{
			if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040ClassName_0040Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040XZ_00404_NA && m_pkSpline == null)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040DPOGDCCO_0040Using_003F5zombie_003F5object_003F_0024AA_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040MDBJEHEG_0040m_pkSpline_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 100u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040ClassName_0040Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040XZ_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			return new string(global::_003CModule_003E.AssetObjects_002ESpline_002EGetClassName(m_pkSpline));
		}
	}

	public unsafe virtual string Name
	{
		get
		{
			return m_pmName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040Name_0040Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && m_pkSpline == null)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040DPOGDCCO_0040Using_003F5zombie_003F5object_003F_0024AA_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040MDBJEHEG_0040m_pkSpline_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 92u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040Name_0040Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			m_pmName = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002ESpline_002ESetName(m_pkSpline, standardStringWrapper.Value);
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

	public virtual IEnumerable<ISplineVertex> Vertices => m_pmVerts;

	public unsafe virtual ISplineVertex AppendVertex(float[] position)
	{
		global::AssetObjects.SplineVertex* ptr = global::_003CModule_003E.AssetObjects_002ESpline_002EAppendVertex(m_pkSpline);
		global::_003CModule_003E.AssetObjects_002ESplineVertex_002ESetPosition(ptr, position[0], position[1], position[2]);
		ISplineVertex splineVertex = new SplineVertex(ptr);
		m_pmVerts.Add(splineVertex);
		return splineVertex;
	}

	public unsafe virtual ISplineVertex InsertVertex(int index, float[] position)
	{
		global::AssetObjects.SplineVertex* ptr = global::_003CModule_003E.AssetObjects_002ESpline_002EInsertVertex(m_pkSpline, (ulong)index);
		global::_003CModule_003E.AssetObjects_002ESplineVertex_002ESetPosition(ptr, position[0], position[1], position[2]);
		ISplineVertex splineVertex = new SplineVertex(ptr);
		m_pmVerts.Insert(index, splineVertex);
		PatchReferences(m_pkSpline);
		return splineVertex;
	}

	public unsafe virtual void RemoveVertex(int index)
	{
		m_pmVerts.RemoveAt(index);
		global::_003CModule_003E.AssetObjects_002ESpline_002ERemoveVertex(m_pkSpline, (ulong)index);
		PatchReferences(m_pkSpline);
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		//IL_0010: Expected I, but got I8
		m_pkSpline = null;
		m_pkDeserializer = null;
		List<ISplineVertex>.Enumerator enumerator = m_pmVerts.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((SplineVertex)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmVerts.Clear();
		m_pmCookParams.RemoveReferences(bDisposing: true);
		m_pmCookParams = null;
	}

	internal unsafe virtual void PatchReferences(global::AssetObjects.Spline* pSpline)
	{
		if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FPatchReferences_0040Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPEAV23_0040_0040Z_00404_NA && m_pkSpline == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040DPOGDCCO_0040Using_003F5zombie_003F5object_003F_0024AA_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040MDBJEHEG_0040m_pkSpline_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 167u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FPatchReferences_0040Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPEAV23_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F9_003F_003FPatchReferences_0040Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPEAV23_0040_0040Z_00404_NA)
		{
			ulong num = (ulong)m_pmVerts.Count;
			if (global::_003CModule_003E.AssetObjects_002ESpline_002Esize(pSpline) != num)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CD_0040MDGOHAMK_0040Managed_003F5and_003F5native_003F5are_003F5out_003F5of_003F5sy_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CE_0040GKMDOCIC_0040pSpline_003F9_003F_0024DOsize_003F_0024CI_003F_0024CJ_003F5_003F_0024DN_003F_0024DN_003F5m_pmVerts_003F9_003F_0024DOCo_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 168u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F9_003F_003FPatchReferences_0040Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPEAV23_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
		}
		m_pkSpline = pSpline;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ASplineVertex_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002ESpline_002Ebegin(pSpline, &iterator);
		List<ISplineVertex>.Enumerator enumerator = m_pmVerts.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((SplineVertex)enumerator.Current).PatchReferences(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASplineVertex_002C4096_003E_002Eiterator_002E_002A(&iterator));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASplineVertex_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (enumerator.MoveNext());
		}
		m_pmCookParams.PatchReferences(global::_003CModule_003E.AssetObjects_002ESpline_002EGetCookParams(pSpline));
	}

	internal unsafe Spline(global::AssetObjects.Spline* pSpline, global::AssetObjects.Deserializer* pkDeserializer)
	{
		//IL_0047: Expected I, but got I8
		m_pkSpline = pSpline;
		m_pkDeserializer = pkDeserializer;
		m_pmVerts = new List<ISplineVertex>();
		base._002Ector();
		if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA && m_pkSpline == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040MDBJEHEG_0040m_pkSpline_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 79u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_pmName = new string(global::_003CModule_003E.AssetObjects_002ESpline_002EGetName(pSpline));
		m_pmCookParams = new ValueSet(global::_003CModule_003E.AssetObjects_002ESpline_002EGetCookParams(pSpline), pkDeserializer);
		AddReferences();
	}

	private unsafe void AddReferences()
	{
		if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && m_pkSpline == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040DPOGDCCO_0040Using_003F5zombie_003F5object_003F_0024AA_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040MDBJEHEG_0040m_pkSpline_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 155u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddReferences_0040Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && m_pmVerts.Count != 0)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DM_0040PDMMDFKP_0040Adding_003F5references_003F5to_003F5managed_003F5lis_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040GCGADMEA_0040m_pmVerts_003F9_003F_0024DOCount_003F5_003F_0024DN_003F_0024DN_003F50_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 156u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddReferences_0040Spline_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ASplineVertex_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002ESpline_002Ebegin(m_pkSpline, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ASplineVertex_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASplineVertex_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ESpline_002Eend(m_pkSpline, &iterator2)))
		{
			do
			{
				ISplineVertex item = new SplineVertex(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASplineVertex_002C4096_003E_002Eiterator_002E_002A(&iterator));
				m_pmVerts.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASplineVertex_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASplineVertex_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ESpline_002Eend(m_pkSpline, &iterator2)));
		}
	}
}

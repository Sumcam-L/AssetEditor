using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

internal class SplineVertex : ISplineVertex
{
	private unsafe global::AssetObjects.SplineVertex* m_pkVertex;

	public unsafe virtual bool SharpCorner
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040SharpCorner_0040SplineVertex_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NXZ_00404_NA && m_pkVertex == null)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040DPOGDCCO_0040Using_003F5zombie_003F5object_003F_0024AA_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040GMGILGP_0040m_pkVertex_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 63u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040SharpCorner_0040SplineVertex_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NXZ_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			global::AssetObjects.SplineVertex* pkVertex = m_pkVertex;
			if (pkVertex == null)
			{
				return false;
			}
			return global::_003CModule_003E.AssetObjects_002ESplineVertex_002EIsSharpCorner(pkVertex);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040SharpCorner_0040SplineVertex_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMX_N_0040Z_00404_NA && m_pkVertex == null)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040DPOGDCCO_0040Using_003F5zombie_003F5object_003F_0024AA_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040GMGILGP_0040m_pkVertex_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040SharpCorner_0040SplineVertex_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMX_N_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			global::AssetObjects.SplineVertex* pkVertex = m_pkVertex;
			if (pkVertex != null)
			{
				global::_003CModule_003E.AssetObjects_002ESplineVertex_002ESetSharpCorner(pkVertex, value);
			}
		}
	}

	public unsafe virtual Point3F Position
	{
		get
		{
			if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040Position_0040SplineVertex_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVPoint3F_0040456_0040XZ_00404_NA && m_pkVertex == null)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040DPOGDCCO_0040Using_003F5zombie_003F5object_003F_0024AA_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040GMGILGP_0040m_pkVertex_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 37u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040Position_0040SplineVertex_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVPoint3F_0040456_0040XZ_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			global::AssetObjects.SplineVertex* pkVertex = m_pkVertex;
			if (pkVertex == null)
			{
				return new Point3F(0f, 0f, 0f);
			}
			float zVal = *(float*)((ulong)(nint)global::_003CModule_003E.AssetObjects_002ESplineVertex_002EGetPosition(pkVertex) + 8uL);
			float yVal = *(float*)((ulong)(nint)global::_003CModule_003E.AssetObjects_002ESplineVertex_002EGetPosition(m_pkVertex) + 4uL);
			return new Point3F(*(float*)global::_003CModule_003E.AssetObjects_002ESplineVertex_002EGetPosition(m_pkVertex), yVal, zVal);
		}
		set
		{
			if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040Position_0040SplineVertex_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVPoint3F_0040456_0040_0040Z_00404_NA && m_pkVertex == null)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040DPOGDCCO_0040Using_003F5zombie_003F5object_003F_0024AA_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040GMGILGP_0040m_pkVertex_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 46u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040Position_0040SplineVertex_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVPoint3F_0040456_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			global::AssetObjects.SplineVertex* pkVertex = m_pkVertex;
			if (pkVertex != null)
			{
				global::_003CModule_003E.AssetObjects_002ESplineVertex_002ESetPosition(pkVertex, value.x, value.y, value.z);
			}
		}
	}

	internal unsafe SplineVertex(global::AssetObjects.SplineVertex* pkVertex)
	{
		m_pkVertex = pkVertex;
		base._002Ector();
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkVertex = null;
	}

	internal unsafe virtual void PatchReferences(global::AssetObjects.SplineVertex* pVertex)
	{
		if (!global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FPatchReferences_0040SplineVertex_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPEAV23_0040_0040Z_00404_NA && m_pkVertex == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BF_0040DIDEKEKE_0040Using_003F5zombine_003F5object_003F_0024AA_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BG_0040GMGILGP_0040m_pkVertex_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040JLHKEFDH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 26u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xebc6952e_002E_003FbIgnoreAlways_0040_003F2_003F_003FPatchReferences_0040SplineVertex_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPEAV23_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		m_pkVertex = pVertex;
	}
}

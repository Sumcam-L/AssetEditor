using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class Coord3DValue : Value, ICoord3DValue
{
	public override ValueType ParameterType => ValueType.VT_COORD3D;

	public unsafe virtual Point3F ParameterValue
	{
		get
		{
			global::AssetObjects.Coord3DValue* pkValue = (global::AssetObjects.Coord3DValue*)m_pkValue;
			return new Point3F(global::_003CModule_003E.AssetObjects_002ECoord3DValue_002EGetX(pkValue), global::_003CModule_003E.AssetObjects_002ECoord3DValue_002EGetY(pkValue), global::_003CModule_003E.AssetObjects_002ECoord3DValue_002EGetZ(pkValue));
		}
		set
		{
			if (value == null)
			{
				if (!global::_003CModule_003E._003FA0xf2460be8_002E_003FbIgnoreAlways_0040_003F6_003F_003Fset_0040ParameterValue_0040Coord3DValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVPoint3F_0040456_0040_0040Z_00404_NA)
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
					global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DP_0040FDFAAHLI_0040Null_003F5values_003F5are_003F5not_003F5valid_003F5for_003F5Co_0040), __arglist());
					if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040EJKPDOCN_0040_003F_0024CB_003F_0024CCpmValue_003F_0024CC_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040FBIKHPIC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 18u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xf2460be8_002E_003FbIgnoreAlways_0040_003F6_003F_003Fset_0040ParameterValue_0040Coord3DValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVPoint3F_0040456_0040_0040Z_00404_NA), (Platform.ErrorType)0))
					{
						/*OpCode not supported: DebugBreak*/;
					}
				}
				return;
			}
			global::AssetObjects.Coord3DValue* pkValue = (global::AssetObjects.Coord3DValue*)m_pkValue;
			if (pkValue == null)
			{
				if (!global::_003CModule_003E._003FA0xf2460be8_002E_003FbIgnoreAlways_0040_003FBC_0040_003F_003Fset_0040ParameterValue_0040Coord3DValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVPoint3F_0040456_0040_0040Z_00404_NA)
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
					global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EF_0040FGKJNLJE_0040Attempting_003F5to_003F5modify_003F5a_003F5value_003F5on_003F5_0040), __arglist());
					if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08JCLCKEHN_0040_003F_0024CB_003F_0024CCvalue_003F_0024CC_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040FBIKHPIC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 21u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xf2460be8_002E_003FbIgnoreAlways_0040_003FBC_0040_003F_003Fset_0040ParameterValue_0040Coord3DValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVPoint3F_0040456_0040_0040Z_00404_NA), (Platform.ErrorType)0))
					{
						/*OpCode not supported: DebugBreak*/;
					}
				}
			}
			else
			{
				global::_003CModule_003E.AssetObjects_002ECoord3DValue_002ESet(pkValue, value.x, value.y, value.z);
			}
		}
	}

	public unsafe Coord3DValue(global::AssetObjects.CollectionValue* pkCollectionValue, sbyte* szName)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EPush_003Cclass_0020AssetObjects_003A_003ACoord3DValue_002Cfloat_002Cfloat_002Cfloat_003E(pkCollectionValue, szName, &num3, &num2, &num));
	}

	public unsafe Coord3DValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003ACoord3DValue_002Cfloat_002Cfloat_002Cfloat_003E(pkValueSet, szName, &num3, &num2, &num));
	}

	public unsafe Coord3DValue(global::AssetObjects.Coord3DValue* pkValue)
		: base((global::AssetObjects.Value*)pkValue)
	{
	}

	public override void CopyDataFrom(IValue otherValue)
	{
		if (otherValue.ParameterType == ValueType.VT_COORD3D)
		{
			ICoord3DValue coord3DValue = (ICoord3DValue)otherValue;
			ParameterValue = coord3DValue.ParameterValue;
		}
	}

	private unsafe global::AssetObjects.Coord3DValue* GetValue()
	{
		return (global::AssetObjects.Coord3DValue*)m_pkValue;
	}
}

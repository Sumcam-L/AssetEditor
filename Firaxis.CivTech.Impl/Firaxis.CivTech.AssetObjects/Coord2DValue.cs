using System.Drawing;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class Coord2DValue : Value, ICoord2DValue
{
	public override ValueType ParameterType => ValueType.VT_COORD2D;

	public unsafe virtual PointF ParameterValue
	{
		get
		{
			global::AssetObjects.Coord2DValue* pkValue = (global::AssetObjects.Coord2DValue*)m_pkValue;
			return new PointF(global::_003CModule_003E.AssetObjects_002ECoord2DValue_002EGetX(pkValue), global::_003CModule_003E.AssetObjects_002ECoord2DValue_002EGetY(pkValue));
		}
		set
		{
			global::AssetObjects.Coord2DValue* pkValue = (global::AssetObjects.Coord2DValue*)m_pkValue;
			if (pkValue == null)
			{
				if (!global::_003CModule_003E._003FA0x69e34787_002E_003FbIgnoreAlways_0040_003F6_003F_003Fset_0040ParameterValue_0040Coord2DValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXVPointF_0040Drawing_0040System_0040_0040_0040Z_00404_NA)
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
					global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EF_0040FGKJNLJE_0040Attempting_003F5to_003F5modify_003F5a_003F5value_003F5on_003F5_0040), __arglist());
					if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08JCLCKEHN_0040_003F_0024CB_003F_0024CCvalue_003F_0024CC_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040MKCPDDON_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 20u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x69e34787_002E_003FbIgnoreAlways_0040_003F6_003F_003Fset_0040ParameterValue_0040Coord2DValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXVPointF_0040Drawing_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
					{
						/*OpCode not supported: DebugBreak*/;
					}
				}
			}
			else
			{
				global::_003CModule_003E.AssetObjects_002ECoord2DValue_002ESet(pkValue, value.X, value.Y);
			}
		}
	}

	public unsafe Coord2DValue(global::AssetObjects.CollectionValue* pkCollectionValue, sbyte* szName)
	{
		float num = 0f;
		float num2 = 0f;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EPush_003Cclass_0020AssetObjects_003A_003ACoord2DValue_002Cfloat_002Cfloat_003E(pkCollectionValue, szName, &num2, &num));
	}

	public unsafe Coord2DValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
	{
		float num = 0f;
		float num2 = 0f;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003ACoord2DValue_002Cfloat_002Cfloat_003E(pkValueSet, szName, &num2, &num));
	}

	public unsafe Coord2DValue(global::AssetObjects.Coord2DValue* pkValue)
		: base((global::AssetObjects.Value*)pkValue)
	{
	}

	public override void CopyDataFrom(IValue otherValue)
	{
		if (otherValue.ParameterType == ValueType.VT_COORD2D)
		{
			PointF parameterValue = ((ICoord2DValue)otherValue).ParameterValue;
			ParameterValue = parameterValue;
		}
	}

	private unsafe global::AssetObjects.Coord2DValue* GetValue()
	{
		return (global::AssetObjects.Coord2DValue*)m_pkValue;
	}
}

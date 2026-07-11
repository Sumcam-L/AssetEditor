using System;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class StringValue : Value, IStringValue
{
	private string m_value;

	public override ValueType ParameterType => ValueType.VT_STRING;

	public unsafe virtual string ParameterValue
	{
		get
		{
			return m_value;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = null;
			m_value = value;
			global::AssetObjects.StringValue* pkValue = (global::AssetObjects.StringValue*)m_pkValue;
			if (pkValue != null)
			{
				StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper3;
					global::_003CModule_003E.AssetObjects_002EStringValue_002ESetValue(pkValue, standardStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
				return;
			}
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(ParameterValue);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				if (!global::_003CModule_003E._003FA0x063cb7f5_002E_003FbIgnoreAlways_0040_003F7_003F_003Fset_0040ParameterValue_0040StringValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
					global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EF_0040HENLLLKL_0040String_003F5value_003F5_003F8_003F_0024CFs_003F8_003F5will_003F5not_003F5be_003F5se_0040), __arglist(standardStringWrapper2.Value));
					if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05MFEJDJP_0040value_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040IDLOBLKD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 45u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x063cb7f5_002E_003FbIgnoreAlways_0040_003F7_003F_003Fset_0040ParameterValue_0040StringValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
					{
						/*OpCode not supported: DebugBreak*/;
					}
				}
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper2).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper2).Dispose();
		}
	}

	public unsafe StringValue(global::AssetObjects.CollectionValue* pkCollectionValue, sbyte* szName)
	{
		m_value = string.Empty;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EPush_003Cclass_0020AssetObjects_003A_003AStringValue_002Cchar_0020const_0020_0028_0026_0029_005B1_005D_003E(pkCollectionValue, szName, (_0024ArrayType_0024_0024_0024BY00_0024_0024CBD*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040)));
	}

	public unsafe StringValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
	{
		m_value = string.Empty;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003AStringValue_002Cchar_0020const_0020_0028_0026_0029_005B1_005D_003E(pkValueSet, szName, (_0024ArrayType_0024_0024_0024BY00_0024_0024CBD*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040)));
	}

	public unsafe StringValue(global::AssetObjects.StringValue* pkValue)
	{
		//IL_002a: Expected I, but got I8
		base._002Ector((global::AssetObjects.Value*)pkValue);
		if (!global::_003CModule_003E._003FA0x063cb7f5_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0StringValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA && pkValue == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_07PKPJGBOM_0040pkValue_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040IDLOBLKD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 12u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x063cb7f5_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0StringValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_value = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EStringValue_002EGetValue(pkValue));
	}

	public override void CopyDataFrom(IValue otherValue)
	{
		if (otherValue.ParameterType == ValueType.VT_STRING)
		{
			IStringValue stringValue = (IStringValue)otherValue;
			ParameterValue = stringValue.ParameterValue;
		}
	}

	private unsafe global::AssetObjects.StringValue* GetValue()
	{
		return (global::AssetObjects.StringValue*)m_pkValue;
	}
}

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public abstract class Value : IValue
{
	protected unsafe global::AssetObjects.Value* m_pkValue;

	private string m_parameterName;

	public virtual ValueType ParameterType => ValueType.VT_COUNT;

	public unsafe virtual string ParameterName
	{
		get
		{
			return m_parameterName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (!(m_parameterName == value))
			{
				m_parameterName = value;
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EValue_002ESetParameterName(m_pkValue, standardStringWrapper.Value);
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
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool Equals(IValue otherValue)
	{
		Value value = (Value)otherValue;
		return m_pkValue == value.m_pkValue;
	}

	public abstract void CopyDataFrom(IValue otherValue);

	internal unsafe void PatchReference(global::AssetObjects.ValueSet* valueSet)
	{
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E._003FA0x3a45c7e6_002E_003FbIgnoreAlways_0040_003F2_003F_003FPatchReference_0040Value_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVValueSet_00403_0040_0040Z_00404_NA && valueSet == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DC_0040BOHFKDAE_0040Attempting_003F5to_003F5patch_003F5a_003F5Value_003F5with_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08BMNCKGIA_0040valueSet_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GJ_0040FKKIDEDN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 50u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x3a45c7e6_002E_003FbIgnoreAlways_0040_003F2_003F_003FPatchReference_0040Value_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVValueSet_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(ParameterName);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.Value* ptr = (m_pkValue = global::_003CModule_003E.AssetObjects_002EValueSet_002EFindValue(valueSet, standardStringWrapper.Value));
			if (!global::_003CModule_003E._003FA0x3a45c7e6_002E_003FbIgnoreAlways_0040_003F9_003F_003FPatchReference_0040Value_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVValueSet_00403_0040_0040Z_00404_NA && ptr == null)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EO_0040FMHCIGEM_0040Value_003F5with_003F5parameter_003F5name_003F5_003F8_003F_0024CFs_003F8_003F5h_0040), __arglist(standardStringWrapper.Value));
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09PFKDNHJE_0040m_pkValue_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GJ_0040FKKIDEDN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 56u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x3a45c7e6_002E_003FbIgnoreAlways_0040_003F9_003F_003FPatchReference_0040Value_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVValueSet_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	internal unsafe global::AssetObjects.Value* GetAssetObject()
	{
		return m_pkValue;
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0013: Expected I, but got I8
		m_parameterName = string.Empty;
		m_pkValue = null;
	}

	protected unsafe Value(global::AssetObjects.Value* value)
	{
		//IL_0030: Expected I, but got I8
		m_pkValue = value;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x3a45c7e6_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0Value_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAM_0040PEAV12_0040_0040Z_00404_NA && value == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05MFEJDJP_0040value_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GJ_0040FKKIDEDN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 15u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x3a45c7e6_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0Value_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAM_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_parameterName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EValue_002EGetParameterName(value));
	}
}

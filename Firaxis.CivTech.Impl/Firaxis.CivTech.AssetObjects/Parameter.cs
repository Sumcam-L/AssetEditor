using System;
using System.Runtime.CompilerServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class Parameter : IParameter
{
	protected unsafe global::AssetObjects.Parameter* m_pkParameter = null;

	protected Value m_pmDefaultValue = null;

	public virtual IValue DefaultValue => m_pmDefaultValue;

	public unsafe virtual ParameterType ParameterType => (ParameterType)global::_003CModule_003E.AssetObjects_002EParameter_002EGetType(m_pkParameter);

	public unsafe virtual ValueType ParameterValueType => (ValueType)global::_003CModule_003E.AssetObjects_002EParameter_002EGetValueType(m_pkParameter);

	public unsafe virtual string Category
	{
		get
		{
			//IL_0028: Expected I, but got I8
			if (!global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040Category_0040Parameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040XZ_00404_NA && m_pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040PGOAIFI_0040m_pkParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HB_0040COPHBLMB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 36u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040Category_0040Parameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040XZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return new string(global::_003CModule_003E.AssetObjects_002EParameter_002EGetCategoryName(m_pkParameter));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EParameter_002ESetCategoryName(m_pkParameter, standardStringWrapper.Value);
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

	public unsafe virtual string Description
	{
		get
		{
			//IL_0028: Expected I, but got I8
			if (!global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040Description_0040Parameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040XZ_00404_NA && m_pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040PGOAIFI_0040m_pkParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HB_0040COPHBLMB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 24u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040Description_0040Parameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040XZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return new string(global::_003CModule_003E.AssetObjects_002EParameter_002EGetDescription(m_pkParameter));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EParameter_002ESetDescription(m_pkParameter, standardStringWrapper.Value);
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

	public unsafe virtual string Name
	{
		get
		{
			//IL_0028: Expected I, but got I8
			if (!global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040Name_0040Parameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040XZ_00404_NA && m_pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040PGOAIFI_0040m_pkParameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HB_0040COPHBLMB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 18u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040Name_0040Parameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040XZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return new string(global::_003CModule_003E.AssetObjects_002EParameter_002EGetName(m_pkParameter));
		}
	}

	internal unsafe global::AssetObjects.Parameter* GetAssetObject()
	{
		return m_pkParameter;
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0018: Expected I, but got I8
		m_pmDefaultValue?.RemoveReferences();
		m_pkParameter = null;
		m_pmDefaultValue = null;
	}

	internal unsafe void SetNativeParameter(global::AssetObjects.Parameter* parameter)
	{
		//IL_003b: Expected I, but got I8
		//IL_0023: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetNativeParameter_0040Parameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAV23_0040_0040Z_00404_NA && parameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09DAGAJHIH_0040parameter_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HB_0040COPHBLMB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 73u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetNativeParameter_0040Parameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAV23_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_pkParameter = parameter;
		m_pmDefaultValue = ValueFactory.CreateValue(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Value*>)(*(ulong*)(*(long*)parameter + 24)))((nint)parameter));
	}

	protected unsafe Parameter(global::AssetObjects.Parameter* parameter)
	{
		//IL_0008: Expected I, but got I8
		SetNativeParameter(parameter);
	}
}

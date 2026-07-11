using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class CollectionParameter : Parameter, ICollectionParameter
{
	private IParameter m_pmEntryParameter;

	public virtual ParameterType EntryParameterType => m_pmEntryParameter.ParameterType;

	public unsafe virtual InstanceType EntryObjectType => (InstanceType)global::_003CModule_003E.AssetObjects_002ECollectionParameter_002EGetEntryObjectType((global::AssetObjects.CollectionParameter*)m_pkParameter);

	public virtual ValueType EntryValueType => m_pmEntryParameter.ParameterValueType;

	public virtual IParameter EntryParameter => m_pmEntryParameter;

	public virtual IValue EntryDefaultValue => DefaultValue;

	public unsafe virtual bool FixedSize
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			//IL_002a: Expected I, but got I8
			global::AssetObjects.CollectionParameter* pkParameter = (global::AssetObjects.CollectionParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040FixedSize_0040CollectionParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NXZ_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040OAMJKIEK_0040pkCollectionParam_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HB_0040COPHBLMB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 107u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040FixedSize_0040CollectionParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return global::_003CModule_003E.AssetObjects_002ECollectionParameter_002EGetIsFixedSize(pkParameter);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			//IL_002a: Expected I, but got I8
			global::AssetObjects.CollectionParameter* pkParameter = (global::AssetObjects.CollectionParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040FixedSize_0040CollectionParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMX_N_0040Z_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040OAMJKIEK_0040pkCollectionParam_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HB_0040COPHBLMB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 114u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040FixedSize_0040CollectionParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMX_N_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			global::_003CModule_003E.AssetObjects_002ECollectionParameter_002ESetIsFixedSize(pkParameter, value);
		}
	}

	public unsafe virtual bool HasNamedEntries
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			//IL_002a: Expected I, but got I8
			global::AssetObjects.CollectionParameter* pkParameter = (global::AssetObjects.CollectionParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040HasNamedEntries_0040CollectionParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NXZ_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040OAMJKIEK_0040pkCollectionParam_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HB_0040COPHBLMB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 93u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040HasNamedEntries_0040CollectionParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return global::_003CModule_003E.AssetObjects_002ECollectionParameter_002EGetHasNamedEntries(pkParameter);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			//IL_002a: Expected I, but got I8
			global::AssetObjects.CollectionParameter* pkParameter = (global::AssetObjects.CollectionParameter*)m_pkParameter;
			if (!global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040HasNamedEntries_0040CollectionParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMX_N_0040Z_00404_NA && pkParameter == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040OAMJKIEK_0040pkCollectionParam_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HB_0040COPHBLMB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 100u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x3679e010_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040HasNamedEntries_0040CollectionParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMX_N_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			global::_003CModule_003E.AssetObjects_002ECollectionParameter_002ESetHasNamedEntries(pkParameter, value);
		}
	}

	public unsafe CollectionParameter(global::AssetObjects.CollectionParameter* pkParameter)
	{
		m_pmEntryParameter = ParameterSet.CreateParameter(global::_003CModule_003E.AssetObjects_002ECollectionParameter_002EGetEntryParameter(pkParameter));
		base._002Ector((global::AssetObjects.Parameter*)pkParameter);
	}

	private unsafe global::AssetObjects.CollectionParameter* GetNativeParameter()
	{
		return (global::AssetObjects.CollectionParameter*)m_pkParameter;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;
using Serialization;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ReadOnlyValueSet : IValueSet
{
	private List<IValue> m_values;

	private unsafe global::AssetObjects.ValueSet* m_valueSet;

	private bool m_isValid;

	public virtual IEnumerable<IValue> Items
	{
		get
		{
			if (m_isValid)
			{
				return m_values;
			}
			return Enumerable.Empty<IValue>();
		}
	}

	public unsafe ReadOnlyValueSet(global::AssetObjects.ValueSet* valueSet)
	{
		//IL_0047: Expected I, but got I8
		//IL_0097: Expected I, but got I8
		m_values = new List<IValue>();
		m_valueSet = valueSet;
		m_isValid = true;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVValueSet_00402_0040_0040Z_00404_NA && m_valueSet == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040ECADAFDI_0040m_valueSet_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 31u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVValueSet_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E obj);
		global::_003CModule_003E.AssetObjects_002EValueSet_002Ebegin(valueSet, &obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E obj2);
		global::_003CModule_003E.AssetObjects_002EValueSet_002Eend(valueSet, &obj2);
		if (!global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, &obj2))
		{
			return;
		}
		do
		{
			IValue value = CreateValue(global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_002A(&obj));
			if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003FO_0040_003F_003F_003F0ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVValueSet_00402_0040_0040Z_00404_NA)
			{
				if (value != null)
				{
					goto IL_009d;
				}
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BD_0040HEFMPMCA_0040pmValue_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 36u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003FO_0040_003F_003F_003F0ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVValueSet_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			if (value != null)
			{
				goto IL_009d;
			}
			goto IL_00a9;
			IL_00a9:
			global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_002B_002B(&obj);
			continue;
			IL_009d:
			m_values.Add(value);
			goto IL_00a9;
		}
		while (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, &obj2));
	}

	private void _007EReadOnlyValueSet()
	{
		Destroy();
		GC.SuppressFinalize(this);
	}

	private void _0021ReadOnlyValueSet()
	{
		Destroy();
		GC.SuppressFinalize(this);
	}

	public unsafe virtual T Push<T>(string pmParamName, InstanceType eInstanceType, string pmObjectName) where T : IObjectValue
	{
		if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F_0024Push_0040_0024VMAAABAAB_0040_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_003FAV_0024VMAAABAAB_0040_0040PE_0024AAVString_0040System_0040_0040W4InstanceType_0040234_00400_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EB_0040IPBEBGJF_0040Unable_003F5to_003F5push_003F5values_003F5onto_003F5a_003F5Rea_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 206u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F_0024Push_0040_0024VMAAABAAB_0040_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_003FAV_0024VMAAABAAB_0040_0040PE_0024AAVString_0040System_0040_0040W4InstanceType_0040234_00400_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		return default(T);
	}

	public unsafe virtual T Push<T>(string pmParamName) where T : IValue
	{
		if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F_0024Push_0040_0024VMAAABAAB_0040_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_003FAV_0024VMAAABAAB_0040_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EB_0040IPBEBGJF_0040Unable_003F5to_003F5push_003F5values_003F5onto_003F5a_003F5Rea_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 199u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F_0024Push_0040_0024VMAAABAAB_0040_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_003FAV_0024VMAAABAAB_0040_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		return default(T);
	}

	public unsafe virtual T PushCollection<T>(string pmParamName, InstanceType eInstanceType) where T : IObjectCollectionValue
	{
		if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F_0024PushCollection_0040_0024VMAAABAAB_0040_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_003FAV_0024VMAAABAAB_0040_0040PE_0024AAVString_0040System_0040_0040W4InstanceType_0040234_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EB_0040IPBEBGJF_0040Unable_003F5to_003F5push_003F5values_003F5onto_003F5a_003F5Rea_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 213u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F_0024PushCollection_0040_0024VMAAABAAB_0040_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_003FAV_0024VMAAABAAB_0040_0040PE_0024AAVString_0040System_0040_0040W4InstanceType_0040234_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		return default(T);
	}

	public unsafe virtual void Clear()
	{
		if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FClear_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ED_0040BJKNGHAI_0040Unable_003F5to_003F5remove_003F5values_003F5from_003F5a_003F5R_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 219u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FClear_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual void Remove(IValue pmValue)
	{
		if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemove_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIValue_0040345_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EE_0040OBPNOGBD_0040Unable_003F5to_003F5remove_003F5a_003F5value_003F5from_003F5a_003F5_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 224u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemove_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIValue_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual void RemoveAllValues()
	{
		if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveAllValues_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ED_0040BJKNGHAI_0040Unable_003F5to_003F5remove_003F5values_003F5from_003F5a_003F5R_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 229u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveAllValues_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public virtual IValue FindValue(string pmParamName)
	{
		List<IValue>.Enumerator enumerator = m_values.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				IValue current = enumerator.Current;
				if (current.ParameterName == pmParamName)
				{
					return current;
				}
			}
			while (enumerator.MoveNext());
		}
		return null;
	}

	public unsafe virtual void RemoveUnusedValues(IParameterSet parameters)
	{
		if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveUnusedValues_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIParameterSet_0040345_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ED_0040BJKNGHAI_0040Unable_003F5to_003F5remove_003F5values_003F5from_003F5a_003F5R_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 235u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FRemoveUnusedValues_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIParameterSet_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual void AddDefaultValuesAsNecessary(IParameterSet parameters)
	{
		if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddDefaultValuesAsNecessary_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIParameterSet_0040345_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DO_0040DJJDBOJF_0040Unable_003F5to_003F5add_003F5values_003F5to_003F5a_003F5ReadOn_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 240u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddDefaultValuesAsNecessary_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIParameterSet_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool UpdateFromXML(string pmXml)
	{
		return false;
	}

	public unsafe virtual string SerializeIntoXML()
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out Serializer serializer);
		global::_003CModule_003E.Serialization_002EContext_002E_007Bctor_007D((Context*)(&serializer));
		try
		{
			global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)));
			System.Runtime.CompilerServices.Unsafe.As<Serializer, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &serializer);
			throw;
		}
		string result;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.String obj);
			global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D(&obj);
			try
			{
				global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeIntoString_003Cclass_0020AssetObjects_003A_003AValueSet_003E(&serializer, &obj, m_valueSet);
				result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(&obj));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), &obj);
				throw;
			}
			global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D(&obj);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Serializer*, void>)(&global::_003CModule_003E.AssetObjects_002ESerializer_002E_007Bdtor_007D), &serializer);
			throw;
		}
		global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&serializer));
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DeserializeFromXML(string pmXml)
	{
		if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FDeserializeFromXML_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DM_0040GGFHJAIN_0040Unable_003F5to_003F5deserialize_003F5a_003F5ReadOnly_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 245u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FDeserializeFromXML_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAM_NPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		return false;
	}

	public unsafe virtual void CopyFrom(IValueSet other)
	{
		if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FCopyFrom_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIValueSet_0040345_0040_0040Z_00404_NA)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DH_0040CMAIBLLA_0040Unable_003F5to_003F5modify_003F5a_003F5ReadOnlyValue_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 251u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FCopyFrom_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIValueSet_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	internal unsafe static Value CreateValue(global::AssetObjects.Value* pkValue)
	{
		//IL_00b8: Expected I, but got I8
		switch (global::_003CModule_003E.AssetObjects_002EValue_002EGetType(pkValue))
		{
		case (global::AssetObjects.ValueType)0:
			return new FloatValue((global::AssetObjects.FloatValue*)pkValue);
		case (global::AssetObjects.ValueType)1:
			return new IntValue((global::AssetObjects.IntValue*)pkValue);
		case (global::AssetObjects.ValueType)2:
			return new BoolValue((global::AssetObjects.BoolValue*)pkValue);
		case (global::AssetObjects.ValueType)3:
			return new RGBValue((global::AssetObjects.RGBValue*)pkValue);
		case (global::AssetObjects.ValueType)4:
			return new StringValue((global::AssetObjects.StringValue*)pkValue);
		case (global::AssetObjects.ValueType)5:
			return new ObjectValue((global::AssetObjects.ObjectValue*)pkValue);
		case (global::AssetObjects.ValueType)6:
			return new Coord2DValue((global::AssetObjects.Coord2DValue*)pkValue);
		case (global::AssetObjects.ValueType)7:
			return new Coord3DValue((global::AssetObjects.Coord3DValue*)pkValue);
		case (global::AssetObjects.ValueType)8:
			return new BLPEntryValue((global::AssetObjects.BLPEntryValue*)pkValue);
		case (global::AssetObjects.ValueType)9:
			return new ArtDefRefValue((ArtDefReferenceValue*)pkValue);
		case (global::AssetObjects.ValueType)12:
			return new TupleValue((global::AssetObjects.TupleValue*)pkValue);
		case (global::AssetObjects.ValueType)10:
			return CreateCollectionValue((global::AssetObjects.CollectionValue*)pkValue);
		default:
			if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F4_003F_003FCreateValue_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAVValue_0040345_0040AEAV63_0040_0040Z_00404_NA && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_01GBGANLPD_00400_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 145u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F4_003F_003FCreateValue_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAVValue_0040345_0040AEAV63_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return null;
		}
	}

	internal unsafe void ResolveReferences()
	{
		//IL_0057: Expected I, but got I8
		global::AssetObjects.ValueSet* valueSet = m_valueSet;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E obj);
		global::_003CModule_003E.AssetObjects_002EValueSet_002Ebegin(valueSet, &obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E obj2);
		global::_003CModule_003E.AssetObjects_002EValueSet_002Eend(valueSet, &obj2);
		if (!global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, &obj2))
		{
			return;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
		do
		{
			IValue value = CreateValue(global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_002A(&obj));
			if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F6_003F_003FResolveReferences_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXXZ_00404_NA)
			{
				if (value != null)
				{
					goto IL_005d;
				}
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BD_0040HEFMPMCA_0040pmValue_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 82u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F6_003F_003FResolveReferences_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			if (value != null)
			{
				goto IL_005d;
			}
			goto IL_00ba;
			IL_00ba:
			global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_002B_002B(&obj);
			continue;
			IL_005d:
			IValue value2 = FindValue(value.ParameterName);
			if (value2 != null)
			{
				if (value2.ParameterType == value.ParameterType)
				{
					value2.CopyDataFrom(value);
				}
				else if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003FBJ_0040_003F_003FResolveReferences_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXXZ_00404_NA)
				{
					global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0JL_0040HEOCLCGP_0040When_003F5resolving_003F5references_003F0_003F5the_003F5u_0040), __arglist());
					if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 95u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003FBJ_0040_003F_003FResolveReferences_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
					{
						/*OpCode not supported: DebugBreak*/;
					}
				}
			}
			goto IL_00ba;
		}
		while (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, &obj2));
	}

	internal void Invalidate()
	{
		m_isValid = false;
		m_values.Clear();
	}

	internal void Destroy()
	{
		m_isValid = false;
		m_values.Clear();
	}

	private unsafe static Value CreateCollectionValue(global::AssetObjects.CollectionValue* pkValue)
	{
		//IL_0026: Expected I, but got I8
		//IL_00dd: Expected I, but got I8
		//IL_0105: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FCreateCollectionValue_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040CMPE_0024AAVValue_0040345_0040PEAVCollectionValue_00403_0040_0040Z_00404_NA && pkValue == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05MFEJDJP_0040value_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 152u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003F2_003F_003FCreateCollectionValue_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040CMPE_0024AAVValue_0040345_0040PEAVCollectionValue_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		switch (global::_003CModule_003E.AssetObjects_002ECollectionValue_002EGetEntryValueType(pkValue))
		{
		case (global::AssetObjects.ValueType)0:
			return new FloatCollectionValue(pkValue);
		case (global::AssetObjects.ValueType)1:
			return new IntCollectionValue(pkValue);
		case (global::AssetObjects.ValueType)2:
			return new BoolCollectionValue(pkValue);
		case (global::AssetObjects.ValueType)3:
			return new RGBCollectionValue(pkValue);
		case (global::AssetObjects.ValueType)4:
			return new StringCollectionValue(pkValue);
		case (global::AssetObjects.ValueType)5:
			return new ObjectCollectionValue(pkValue);
		case (global::AssetObjects.ValueType)6:
			return new Coord2DCollectionValue(pkValue);
		case (global::AssetObjects.ValueType)7:
			return new Coord3DCollectionValue(pkValue);
		case (global::AssetObjects.ValueType)8:
			return new BLPEntryCollectionValue(pkValue);
		case (global::AssetObjects.ValueType)9:
			return new ArtDefRefCollectionValue(pkValue);
		case (global::AssetObjects.ValueType)12:
			return new TupleCollectionValue(pkValue);
		case (global::AssetObjects.ValueType)10:
			if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FCreateCollectionValue_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040CMPE_0024AAVValue_0040345_0040PEAVCollectionValue_00403_0040_0040Z_00404_NA && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_01GBGANLPD_00400_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 179u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FCreateCollectionValue_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040CMPE_0024AAVValue_0040345_0040PEAVCollectionValue_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return null;
		default:
			if (!global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003FBD_0040_003F_003FCreateCollectionValue_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040CMPE_0024AAVValue_0040345_0040PEAVCollectionValue_00403_0040_0040Z_00404_NA && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_01GBGANLPD_00400_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DJJDGDBK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 182u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7d0d9ad8_002E_003FbIgnoreAlways_0040_003FBD_0040_003F_003FCreateCollectionValue_0040ReadOnlyValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040CMPE_0024AAVValue_0040345_0040PEAVCollectionValue_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return null;
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EReadOnlyValueSet();
			return;
		}
		try
		{
			_0021ReadOnlyValueSet();
		}
		finally
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~ReadOnlyValueSet()
	{
		Dispose(A_0: false);
	}
}

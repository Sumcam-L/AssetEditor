using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;
using Serialization;
using std;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ValueSet : IValueSet
{
	private List<IValue> m_values;

	private unsafe global::AssetObjects.ValueSet* m_valueSet;

	private unsafe global::AssetObjects.Deserializer* m_deserializer;

	private bool m_bOwnNativeValueSet;

	private bool m_bOwnNativeDeserializer;

	public virtual IEnumerable<IValue> Items => m_values;

	public unsafe ValueSet(global::AssetObjects.ValueSet* pkValues, global::AssetObjects.Deserializer* pkDeserializer)
	{
		//IL_0055: Expected I, but got I8
		//IL_0080: Expected I, but got I8
		m_values = new List<IValue>();
		m_valueSet = pkValues;
		m_deserializer = pkDeserializer;
		m_bOwnNativeValueSet = false;
		m_bOwnNativeDeserializer = false;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x03c5a697_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA && m_valueSet == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040ECADAFDI_0040m_valueSet_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 51u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x03c5a697_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x03c5a697_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0ValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA && m_deserializer == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0P_0040EJIFHLEC_0040m_deserializer_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 52u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x03c5a697_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0ValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		AddReferences(pkValues);
	}

	public unsafe ValueSet(global::AssetObjects.ValueSet* pkValues)
	{
		//IL_004e: Expected I, but got I8
		//IL_007d: Expected I, but got I8
		m_values = new List<IValue>();
		m_valueSet = pkValues;
		m_bOwnNativeValueSet = false;
		m_bOwnNativeDeserializer = true;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x03c5a697_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA && m_valueSet == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040ECADAFDI_0040m_valueSet_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 37u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x03c5a697_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		global::AssetObjects.Deserializer* ptr = (global::AssetObjects.Deserializer*)global::_003CModule_003E.@new(384uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 38, 23, 0);
		global::AssetObjects.Deserializer* deserializer;
		try
		{
			deserializer = ((ptr == null) ? null : global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(ptr));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 38, 23, 0);
			throw;
		}
		m_deserializer = deserializer;
		AddReferences(pkValues);
	}

	public unsafe ValueSet()
	{
		//IL_0013: Expected I, but got I8
		//IL_001b: Expected I, but got I8
		//IL_005c: Expected I, but got I8
		//IL_00a5: Expected I, but got I8
		m_values = new List<IValue>();
		m_valueSet = null;
		m_deserializer = null;
		m_bOwnNativeValueSet = true;
		m_bOwnNativeDeserializer = true;
		base._002Ector();
		int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		global::AssetObjects.ValueSet* ptr = (global::AssetObjects.ValueSet*)global::_003CModule_003E.@new(64uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 26, 23, 0);
		global::AssetObjects.ValueSet* valueSet;
		try
		{
			valueSet = ((ptr == null) ? null : global::_003CModule_003E.AssetObjects_002EValueSet_002E_007Bctor_007D(ptr));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 26, 23, 0);
			throw;
		}
		m_valueSet = valueSet;
		int num2 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		global::AssetObjects.Deserializer* ptr2 = (global::AssetObjects.Deserializer*)global::_003CModule_003E.@new(384uL, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 27, 23, 0);
		global::AssetObjects.Deserializer* deserializer;
		try
		{
			deserializer = ((ptr2 == null) ? null : global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(ptr2));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr2, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 27, 23, 0);
			throw;
		}
		m_deserializer = deserializer;
	}

	private void _007EValueSet()
	{
		_0021ValueSet();
	}

	private unsafe void _0021ValueSet()
	{
		RemoveReferences(bDisposing: true);
		if (m_bOwnNativeValueSet)
		{
			global::AssetObjects.ValueSet* valueSet = m_valueSet;
			if (valueSet != null)
			{
				global::_003CModule_003E.AssetObjects_002EValueSet_002E_007Bdtor_007D(valueSet);
				global::_003CModule_003E.delete(valueSet, 64uL);
			}
		}
		if (m_bOwnNativeDeserializer)
		{
			global::AssetObjects.Deserializer* deserializer = m_deserializer;
			if (deserializer != null)
			{
				global::_003CModule_003E.AssetObjects_002EDeserializer_002E__delDtor(deserializer, 1u);
			}
		}
	}

	public unsafe virtual T Push<T>(string pmParamName, InstanceType eInstanceType, string pmObjectName) where T : IObjectValue
	{
		if (typeof(T) == typeof(IValue))
		{
			return default(T);
		}
		T val = (T)ValueFactory.CreateValue(m_valueSet, pmParamName, eInstanceType, pmObjectName);
		m_values.Add(val);
		return val;
	}

	public unsafe virtual T Push<T>(string pmParamName) where T : IValue
	{
		if (typeof(T) == typeof(IValue))
		{
			return default(T);
		}
		T val = (T)ValueFactory.CreateValue<T>(m_valueSet, pmParamName);
		m_values.Add(val);
		return val;
	}

	public unsafe virtual T PushCollection<T>(string pmParamName, InstanceType eInstanceType) where T : IObjectCollectionValue
	{
		if (typeof(T) == typeof(IValue))
		{
			return default(T);
		}
		T val = (T)ValueFactory.CreateValue(m_valueSet, pmParamName, eInstanceType);
		m_values.Add(val);
		return val;
	}

	public unsafe virtual void Clear()
	{
		List<IValue>.Enumerator enumerator = m_values.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((Value)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		global::_003CModule_003E.AssetObjects_002EValueSet_002Eclear(m_valueSet);
		m_values.Clear();
	}

	public unsafe virtual void Remove(IValue pmValue)
	{
		int num = m_values.IndexOf(pmValue);
		if (num >= 0)
		{
			Value value = (Value)pmValue;
			global::AssetObjects.Value* assetObject = value.GetAssetObject();
			global::_003CModule_003E.AssetObjects_002EValueSet_002ERemove(m_valueSet, assetObject);
			value.RemoveReferences();
			m_values.RemoveAt(num);
			PatchReferences(m_valueSet);
		}
	}

	public virtual void RemoveAllValues()
	{
		Clear();
	}

	private unsafe Value FindValue(global::AssetObjects.Value* pkValue)
	{
		if (pkValue != null)
		{
			List<IValue>.Enumerator enumerator = m_values.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					Value value = (Value)enumerator.Current;
					if (value.GetAssetObject() == pkValue)
					{
						return value;
					}
				}
				while (enumerator.MoveNext());
			}
		}
		return null;
	}

	public unsafe virtual IValue FindValue(string pmParamName)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmParamName);
		IValue result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.Value* pkValue = global::_003CModule_003E.AssetObjects_002EValueSet_002EFindValue(m_valueSet, standardStringWrapper.Value);
			result = FindValue(pkValue);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual void RemoveUnusedValues(IParameterSet parameters)
	{
		global::AssetObjects.ParameterSet* parameterSet = ((ParameterSet)parameters).GetParameterSet();
		if (global::_003CModule_003E.AssetObjects_002EValueSet_002ERemoveUnusedValues(m_valueSet, parameterSet) == 0)
		{
			return;
		}
		List<IValue>.Enumerator enumerator = m_values.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((Value)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_values.Clear();
		AddReferences(m_valueSet);
	}

	public unsafe virtual void AddDefaultValuesAsNecessary(IParameterSet parameters)
	{
		global::AssetObjects.ParameterSet* parameterSet = ((ParameterSet)parameters).GetParameterSet();
		if (global::_003CModule_003E.AssetObjects_002EValueSet_002EAddDefaultValuesAsNecessary(m_valueSet, parameterSet) == 0)
		{
			return;
		}
		List<IValue>.Enumerator enumerator = m_values.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((Value)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_values.Clear();
		AddReferences(m_valueSet);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool UpdateFromXML(string pmXml)
	{
		//IL_003b: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmXml);
		bool flag;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.ValueSet* ptr = (global::AssetObjects.ValueSet*)global::_003CModule_003E.@new(64uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 215, 23, 0);
			global::AssetObjects.ValueSet* ptr2;
			try
			{
				ptr2 = ((ptr == null) ? null : global::_003CModule_003E.AssetObjects_002EValueSet_002E_007Bctor_007D(ptr));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 215, 23, 0);
				throw;
			}
			System.Runtime.CompilerServices.Unsafe.SkipInit(out unique_ptr_003CAssetObjects_003A_003AValueSet_002Cstd_003A_003Adefault_delete_003CAssetObjects_003A_003AValueSet_003E_0020_003E obj);
			global::_003CModule_003E.std_002Eunique_ptr_003CAssetObjects_003A_003AValueSet_002Cstd_003A_003Adefault_delete_003CAssetObjects_003A_003AValueSet_003E_0020_003E_002E_007Bctor_007D(&obj, ptr2);
			try
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out ResultCode resultCode);
				flag = global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserialize_003Cclass_0020AssetObjects_003A_003AValueSet_003E(m_deserializer, &resultCode, global::_003CModule_003E.std_002Eunique_ptr_003CAssetObjects_003A_003AValueSet_002Cstd_003A_003Adefault_delete_003CAssetObjects_003A_003AValueSet_003E_0020_003E_002Eget(&obj), standardStringWrapper.Value));
				if (flag)
				{
					UpdateFromValueSet(global::_003CModule_003E.std_002Eunique_ptr_003CAssetObjects_003A_003AValueSet_002Cstd_003A_003Adefault_delete_003CAssetObjects_003A_003AValueSet_003E_0020_003E_002Eget(&obj));
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<unique_ptr_003CAssetObjects_003A_003AValueSet_002Cstd_003A_003Adefault_delete_003CAssetObjects_003A_003AValueSet_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Eunique_ptr_003CAssetObjects_003A_003AValueSet_002Cstd_003A_003Adefault_delete_003CAssetObjects_003A_003AValueSet_003E_0020_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			global::_003CModule_003E.std_002Eunique_ptr_003CAssetObjects_003A_003AValueSet_002Cstd_003A_003Adefault_delete_003CAssetObjects_003A_003AValueSet_003E_0020_003E_002E_007Bdtor_007D(&obj);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return flag;
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
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmXml);
		bool flag;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ResultCode resultCode);
			flag = global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserialize_003Cclass_0020AssetObjects_003A_003AValueSet_003E(m_deserializer, &resultCode, m_valueSet, standardStringWrapper.Value));
			if (flag)
			{
				RemoveReferences(bDisposing: false);
				AddReferences(m_valueSet);
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return flag;
	}

	public unsafe override string ToString()
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E obj);
		global::_003CModule_003E.AssetObjects_002EValueSet_002EToString(m_valueSet, &obj);
		string result;
		try
		{
			result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(&obj);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D), &obj);
			throw;
		}
		global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D(&obj);
		return result;
	}

	public unsafe virtual void CopyFrom(IValueSet other)
	{
		//IL_0026: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x03c5a697_002E_003FbIgnoreAlways_0040_003F2_003F_003FCopyFrom_0040ValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIValueSet_0040345_0040_0040Z_00404_NA && other == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05BGEKGIIP_0040other_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 247u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x03c5a697_002E_003FbIgnoreAlways_0040_003F2_003F_003FCopyFrom_0040ValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIValueSet_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		RemoveReferences(bDisposing: false);
		global::_003CModule_003E.AssetObjects_002EValueSet_002Eclear(m_valueSet);
		ValueSet valueSet = (ValueSet)other;
		global::_003CModule_003E.AssetObjects_002EValueSet_002E_003D(m_valueSet, valueSet.m_valueSet);
		AddReferences(m_valueSet);
	}

	internal unsafe void FixupNativePointer(global::AssetObjects.ValueSet* valueSet)
	{
		//IL_0026: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x03c5a697_002E_003FbIgnoreAlways_0040_003F2_003F_003FFixupNativePointer_0040ValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAV23_0040_0040Z_00404_NA && valueSet == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08BMNCKGIA_0040valueSet_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 308u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x03c5a697_002E_003FbIgnoreAlways_0040_003F2_003F_003FFixupNativePointer_0040ValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAV23_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_valueSet = valueSet;
	}

	internal unsafe void PatchReferences(global::AssetObjects.ValueSet* pNewNativeSet)
	{
		if (m_valueSet == pNewNativeSet)
		{
			return;
		}
		FixupNativePointer(pNewNativeSet);
		List<IValue>.Enumerator enumerator = m_values.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((Value)enumerator.Current).PatchReference(pNewNativeSet);
			}
			while (enumerator.MoveNext());
		}
	}

	internal unsafe void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		//IL_0055: Expected I, but got I8
		//IL_004d: Expected I, but got I8
		List<IValue>.Enumerator enumerator = m_values.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((Value)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_values.Clear();
		if (bDisposing)
		{
			if (m_valueSet != null)
			{
				m_valueSet = null;
			}
			m_deserializer = null;
		}
	}

	internal unsafe global::AssetObjects.ValueSet* GetNativeObject()
	{
		return m_valueSet;
	}

	private unsafe void UpdateFromValueSet(global::AssetObjects.ValueSet* srcVS)
	{
		//IL_0026: Expected I, but got I8
		//IL_008c: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x03c5a697_002E_003FbIgnoreAlways_0040_003F2_003F_003FUpdateFromValueSet_0040ValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXPEAV23_0040_0040Z_00404_NA && srcVS == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05JAKNJGOE_0040srcVS_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040FFPOGFDI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 229u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x03c5a697_002E_003FbIgnoreAlways_0040_003F2_003F_003FUpdateFromValueSet_0040ValueSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXPEAV23_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E obj);
		global::_003CModule_003E.AssetObjects_002EValueSet_002Ebegin(srcVS, &obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E obj2);
		if (!global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, global::_003CModule_003E.AssetObjects_002EValueSet_002Eend(srcVS, &obj2)))
		{
			return;
		}
		do
		{
			global::AssetObjects.Value* ptr = global::_003CModule_003E.AssetObjects_002EValueSet_002EFindValue(m_valueSet, global::_003CModule_003E.AssetObjects_002EValue_002EGetParameterName(global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_002D_003E(&obj)));
			if (ptr != null)
			{
				((IValue)FindValue(ptr)).CopyDataFrom((IValue)ValueFactory.CreateValue(global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_002A(&obj)));
				global::AssetObjects.Value* ptr2 = global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_002D_003E(&obj);
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Value*, void>)(*(ulong*)(*(long*)ptr2 + 40)))((nint)ptr2, ptr);
			}
			global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_002B_002B(&obj);
		}
		while (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, global::_003CModule_003E.AssetObjects_002EValueSet_002Eend(srcVS, &obj2)));
	}

	private unsafe void AddReferences(global::AssetObjects.ValueSet* pkValues)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E obj);
		global::_003CModule_003E.AssetObjects_002EValueSet_002Ebegin(pkValues, &obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E obj2);
		global::_003CModule_003E.AssetObjects_002EValueSet_002Eend(pkValues, &obj2);
		if (!global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, &obj2))
		{
			return;
		}
		do
		{
			IValue value = ValueFactory.CreateValue(global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_002A(&obj));
			if (value != null)
			{
				m_values.Add(value);
			}
			global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_002B_002B(&obj);
		}
		while (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, &obj2));
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EValueSet();
			return;
		}
		try
		{
			_0021ValueSet();
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

	~ValueSet()
	{
		Dispose(A_0: false);
	}
}

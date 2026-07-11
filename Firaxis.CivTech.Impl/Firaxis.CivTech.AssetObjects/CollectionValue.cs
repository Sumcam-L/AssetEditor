using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class CollectionValue : Value, ICollectionValue
{
	private List<IValue> m_values = new List<IValue>();

	public unsafe virtual ValueType EntryValueType => (ValueType)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EGetEntryValueType((global::AssetObjects.CollectionValue*)m_pkValue);

	public unsafe virtual InstanceType EntryObjectType => (InstanceType)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EGetEntryObjectType((global::AssetObjects.CollectionValue*)m_pkValue);

	public virtual IEnumerable<IValue> Items => m_values;

	public override ValueType ParameterType => ValueType.VT_COLLECTION;

	public unsafe CollectionValue(global::AssetObjects.CollectionValue* value)
		: base((global::AssetObjects.Value*)value)
	{
		AddReferences(value);
	}

	public unsafe virtual T Push<T>(string pmParamName, InstanceType eInstanceType, string pmObjectName) where T : IObjectValue
	{
		if (typeof(T) == typeof(IValue))
		{
			return default(T);
		}
		T val = (T)ValueFactory.CreateValue((global::AssetObjects.CollectionValue*)m_pkValue, pmParamName, eInstanceType, pmObjectName);
		if (((Value)(object)val).m_pkValue == null)
		{
			return default(T);
		}
		m_values.Add(val);
		return val;
	}

	public unsafe virtual T Push<T>(string pmParamName) where T : IValue
	{
		if (typeof(T) == typeof(IValue))
		{
			return default(T);
		}
		T val = (T)ValueFactory.CreateValue<T>((global::AssetObjects.CollectionValue*)m_pkValue, pmParamName);
		if (((Value)(object)val).m_pkValue == null)
		{
			return default(T);
		}
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
		global::_003CModule_003E.AssetObjects_002ECollectionValue_002Eclear((global::AssetObjects.CollectionValue*)m_pkValue);
		m_values.Clear();
	}

	public unsafe virtual void Remove(IValue pmValue)
	{
		int num = m_values.IndexOf(pmValue);
		if (num >= 0)
		{
			Value value = (Value)pmValue;
			global::_003CModule_003E.AssetObjects_002ECollectionValue_002ERemove((global::AssetObjects.CollectionValue*)m_pkValue, value.m_pkValue);
			value.RemoveReferences();
			m_values.RemoveAt(num);
		}
	}

	public unsafe override void CopyDataFrom(IValue otherValue)
	{
		IValue value = null;
		if (otherValue.ParameterType != ValueType.VT_COLLECTION)
		{
			return;
		}
		CollectionValue collectionValue = (CollectionValue)otherValue;
		if (EntryValueType == collectionValue.EntryValueType)
		{
			Dictionary<string, IValue> dictionary = new Dictionary<string, IValue>();
			Dictionary<string, IValue> dictionary2 = new Dictionary<string, IValue>();
			foreach (IValue item in Items)
			{
				dictionary[item.ParameterName] = item;
			}
			foreach (IValue item2 in collectionValue.Items)
			{
				dictionary2[item2.ParameterName] = item2;
			}
			Dictionary<string, IValue>.KeyCollection.Enumerator enumerator3 = dictionary.Keys.GetEnumerator();
			if (enumerator3.MoveNext())
			{
				do
				{
					string current3 = enumerator3.Current;
					if (!dictionary2.ContainsKey(current3))
					{
						Remove(dictionary[current3]);
					}
				}
				while (enumerator3.MoveNext());
			}
			Func<IValue, IValue> func = ((EntryValueType != ValueType.VT_OBJECT) ? new Func<IValue, IValue>(CreateValue) : new Func<IValue, IValue>(CreateObjectValue));
			Dictionary<string, IValue>.Enumerator enumerator4 = dictionary2.GetEnumerator();
			if (!enumerator4.MoveNext())
			{
				return;
			}
			do
			{
				KeyValuePair<string, IValue> current4 = enumerator4.Current;
				value = null;
				if (!dictionary.TryGetValue(current4.Key, out value))
				{
					value = func(current4.Value);
				}
				value.CopyDataFrom(current4.Value);
			}
			while (enumerator4.MoveNext());
		}
		else if (!global::_003CModule_003E._003FA0x93932aa3_002E_003FbIgnoreAlways_0040_003FCK_0040_003F_003FCopyDataFrom_0040CollectionValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIValue_0040345_0040_0040Z_00404_NA && EntryValueType != collectionValue.EntryValueType)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040FJBIAPPN_0040Collection_003F5value_003F5is_003F5trying_003F5to_003F5co_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CN_0040DNDBKGHA_0040EntryValueType_003F5_003F_0024DN_003F_0024DN_003F5typedValue_003F9_003F_0024DOEn_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HD_0040PBKODBJB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 174u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x93932aa3_002E_003FbIgnoreAlways_0040_003FCK_0040_003F_003FCopyDataFrom_0040CollectionValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIValue_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	internal override void RemoveReferences()
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
		m_values.Clear();
		base.RemoveReferences();
	}

	internal unsafe void AddReferences(global::AssetObjects.CollectionValue* pkValues)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E obj);
		global::_003CModule_003E.AssetObjects_002ECollectionValue_002Ebegin(pkValues, &obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AValue_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AValue_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AValue_003E_003A_003ADereferencer_003E obj2);
		global::_003CModule_003E.AssetObjects_002ECollectionValue_002Eend(pkValues, &obj2);
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

	internal unsafe global::AssetObjects.CollectionValue* GetCollectionValue()
	{
		return (global::AssetObjects.CollectionValue*)m_pkValue;
	}

	private unsafe IValue CreateValue(IValue seed)
	{
		string parameterName = seed.ParameterName;
		switch (seed.ParameterType)
		{
		case ValueType.VT_FLOAT:
			return Push<IFloatValue>(parameterName);
		case ValueType.VT_INT:
			return Push<IIntValue>(parameterName);
		case ValueType.VT_BOOL:
			return Push<IBoolValue>(parameterName);
		case ValueType.VT_RGB:
			return Push<IRGBValue>(parameterName);
		case ValueType.VT_STRING:
			return Push<IStringValue>(parameterName);
		case ValueType.VT_COORD2D:
			return Push<ICoord2DValue>(parameterName);
		case ValueType.VT_COORD3D:
			return Push<ICoord3DValue>(parameterName);
		case ValueType.VT_BLP_ENTRY:
			return Push<IBLPEntryValue>(parameterName);
		case ValueType.VT_ARTDEF_REFERENCE:
			return Push<IArtDefRefValue>(parameterName);
		case ValueType.VT_CURVE:
			return Push<ICurveValue>(parameterName);
		case ValueType.VT_OBJECT:
		case ValueType.VT_COLLECTION:
			if (!global::_003CModule_003E._003FA0x93932aa3_002E_003FbIgnoreAlways_0040_003F4_003F_003FCreateValue_0040CollectionValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAUIValue_0040345_0040PE_0024AAU6345_0040_0040Z_00404_NA)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FE_0040MHNJNBMI_0040CreateValue_003F5cannot_003F5be_003F5used_003F5to_003F5cr_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HD_0040PBKODBJB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 207u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x93932aa3_002E_003FbIgnoreAlways_0040_003F4_003F_003FCreateValue_0040CollectionValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAUIValue_0040345_0040PE_0024AAU6345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			break;
		default:
			if (!global::_003CModule_003E._003FA0x93932aa3_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FCreateValue_0040CollectionValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAUIValue_0040345_0040PE_0024AAU6345_0040_0040Z_00404_NA)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DO_0040FAOPDJPM_0040CreateValue_003F5detected_003F5an_003F5unknown_003F5_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HD_0040PBKODBJB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 210u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x93932aa3_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FCreateValue_0040CollectionValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAUIValue_0040345_0040PE_0024AAU6345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			break;
		}
		return null;
	}

	private unsafe IValue CreateObjectValue(IValue seed)
	{
		if (!global::_003CModule_003E._003FA0x93932aa3_002E_003FbIgnoreAlways_0040_003F2_003F_003FCreateObjectValue_0040CollectionValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAUIValue_0040345_0040PE_0024AAU6345_0040_0040Z_00404_NA && seed.ParameterType != ValueType.VT_OBJECT)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DO_0040FNNMGEMJ_0040This_003F5can_003F5only_003F5be_003F5used_003F5to_003F5constru_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DK_0040ECBHGHP_0040seed_003F9_003F_0024DOParameterType_003F5_003F_0024DN_003F_0024DN_003F5AssetObje_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HD_0040PBKODBJB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 219u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x93932aa3_002E_003FbIgnoreAlways_0040_003F2_003F_003FCreateObjectValue_0040CollectionValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAUIValue_0040345_0040PE_0024AAU6345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		IObjectValue objectValue = (IObjectValue)seed;
		return Push<IObjectValue>(objectValue.ParameterName, objectValue.GetBoundObjectType(), objectValue.GetBoundObjectName());
	}
}

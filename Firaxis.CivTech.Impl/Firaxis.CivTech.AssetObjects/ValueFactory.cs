using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class ValueFactory
{
	private static Dictionary<Type, ValueType> m_typeToValueTypeMap;

	static ValueFactory()
	{
		m_typeToValueTypeMap = null;
		m_typeToValueTypeMap = new Dictionary<Type, ValueType>();
		PopulateTypeDictionary();
	}

	public unsafe static Value CreateValue(global::AssetObjects.Value* value)
	{
		//IL_00d9: Expected I, but got I8
		byte condition = (byte)(((long)(nint)value != 0) ? 1 : 0);
		BugSubmitter.SilentAssert(condition != 0, "Attempting to create a managed value with a null native value.  @assign bwhitman");
		if (value == null)
		{
			return null;
		}
		switch (global::_003CModule_003E.AssetObjects_002EValue_002EGetType(value))
		{
		case (global::AssetObjects.ValueType)0:
			return new FloatValue((global::AssetObjects.FloatValue*)value);
		case (global::AssetObjects.ValueType)1:
			return new IntValue((global::AssetObjects.IntValue*)value);
		case (global::AssetObjects.ValueType)2:
			return new BoolValue((global::AssetObjects.BoolValue*)value);
		case (global::AssetObjects.ValueType)3:
			return new RGBValue((global::AssetObjects.RGBValue*)value);
		case (global::AssetObjects.ValueType)4:
			return new StringValue((global::AssetObjects.StringValue*)value);
		case (global::AssetObjects.ValueType)5:
			return new ObjectValue((global::AssetObjects.ObjectValue*)value);
		case (global::AssetObjects.ValueType)6:
			return new Coord2DValue((global::AssetObjects.Coord2DValue*)value);
		case (global::AssetObjects.ValueType)7:
			return new Coord3DValue((global::AssetObjects.Coord3DValue*)value);
		case (global::AssetObjects.ValueType)8:
			return new BLPEntryValue((global::AssetObjects.BLPEntryValue*)value);
		case (global::AssetObjects.ValueType)9:
			return new ArtDefRefValue((ArtDefReferenceValue*)value);
		case (global::AssetObjects.ValueType)10:
			return CreateCollectionValue((global::AssetObjects.CollectionValue*)value);
		case (global::AssetObjects.ValueType)11:
			return new CurveValue((global::AssetObjects.CurveValue*)value);
		case (global::AssetObjects.ValueType)12:
			return new TupleValue((global::AssetObjects.TupleValue*)value);
		default:
			if (!global::_003CModule_003E._003FA0x557ea6cf_002E_003FbIgnoreAlways_0040_003F6_003F_003FCreateValue_0040ValueFactory_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAVValue_0040345_0040PEAV63_0040_0040Z_00404_NA && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_01GBGANLPD_00400_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 311u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x557ea6cf_002E_003FbIgnoreAlways_0040_003F6_003F_003FCreateValue_0040ValueFactory_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAVValue_0040345_0040PEAV63_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return null;
		}
	}

	public unsafe static IObjectCollectionValue CreateValue(global::AssetObjects.ValueSet* valueSet, string parameterName, InstanceType instanceType)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(parameterName);
		IObjectCollectionValue result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			result = new ObjectCollectionValue(valueSet, standardStringWrapper.Value, instanceType);
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

	public unsafe static IObjectValue CreateValue(global::AssetObjects.CollectionValue* collectionValue, string parameterName, InstanceType instanceType, string objectName)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(parameterName);
		IObjectValue result;
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(objectName);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				result = new ObjectValue(collectionValue, standardStringWrapper.Value, instanceType, standardStringWrapper2.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper2).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper2).Dispose();
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

	public unsafe static IObjectValue CreateValue(global::AssetObjects.ValueSet* valueSet, string parameterName, InstanceType instanceType, string objectName)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(parameterName);
		IObjectValue result;
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(objectName);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				result = new ObjectValue(valueSet, standardStringWrapper.Value, instanceType, standardStringWrapper2.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper2).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper2).Dispose();
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

	public unsafe static IValue CreateValue<T>(global::AssetObjects.CollectionValue* collectionValue, string parameterName) where T : IValue
	{
		StandardStringWrapper standardStringWrapper = null;
		Type typeFromHandle = typeof(T);
		ValueType value = ValueType.VT_FLOAT;
		if (!m_typeToValueTypeMap.TryGetValue(typeFromHandle, out value))
		{
			BugSubmitter.SilentAssert(m_typeToValueTypeMap.ContainsKey(typeFromHandle), "");
			return default(T);
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(parameterName);
		IValue result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			switch (value)
			{
			case ValueType.VT_COLLECTION:
				break;
			case ValueType.VT_OBJECT:
				break;
			case ValueType.VT_FLOAT:
				result = new FloatValue(collectionValue, standardStringWrapper.Value);
				goto IL_00d7;
			case ValueType.VT_INT:
				goto IL_00e1;
			case ValueType.VT_BOOL:
				goto IL_0102;
			case ValueType.VT_RGB:
				goto IL_0123;
			case ValueType.VT_STRING:
				goto IL_0144;
			case ValueType.VT_COORD2D:
				goto IL_0165;
			case ValueType.VT_COORD3D:
				goto IL_0186;
			case ValueType.VT_BLP_ENTRY:
				goto IL_01a7;
			case ValueType.VT_ARTDEF_REFERENCE:
				goto IL_01c8;
			case ValueType.VT_CURVE:
				goto IL_01e9;
			case ValueType.VT_TUPLE:
				goto IL_020a;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		IValue result2;
		try
		{
			BugSubmitter.SilentAssert(condition: false, $"Unknown value type found in CreateValue.  Type: '{value}'.  @assign bwhitman");
			result2 = default(T);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result2;
		IL_0186:
		IValue result3;
		try
		{
			result3 = new Coord3DValue(collectionValue, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result3;
		IL_00d7:
		((IDisposable)standardStringWrapper).Dispose();
		return result;
		IL_00e1:
		IValue result4;
		try
		{
			result4 = new IntValue(collectionValue, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result4;
		IL_020a:
		IValue result5;
		try
		{
			result5 = new TupleValue(collectionValue, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result5;
		IL_0102:
		IValue result6;
		try
		{
			result6 = new BoolValue(collectionValue, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result6;
		IL_01a7:
		IValue result7;
		try
		{
			result7 = new BLPEntryValue(collectionValue, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result7;
		IL_0123:
		IValue result8;
		try
		{
			result8 = new RGBValue(collectionValue, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result8;
		IL_01c8:
		IValue result9;
		try
		{
			result9 = new ArtDefRefValue(collectionValue, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result9;
		IL_0144:
		IValue result10;
		try
		{
			result10 = new StringValue(collectionValue, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result10;
		IL_01e9:
		IValue result11;
		try
		{
			result11 = new CurveValue(collectionValue, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result11;
		IL_0165:
		IValue result12;
		try
		{
			result12 = new Coord2DValue(collectionValue, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result12;
	}

	public unsafe static IValue CreateValue<T>(global::AssetObjects.ValueSet* valueSet, string parameterName) where T : IValue
	{
		StandardStringWrapper standardStringWrapper = null;
		Type typeFromHandle = typeof(T);
		ValueType value = ValueType.VT_FLOAT;
		if (!m_typeToValueTypeMap.TryGetValue(typeFromHandle, out value))
		{
			BugSubmitter.SilentAssert(fmtParams: new object[1] { typeFromHandle.ToString() }, condition: m_typeToValueTypeMap.ContainsKey(typeFromHandle), fmtText: "Type dictionary does not contain a mapping between the type '{0}' and a ValueType.  @assign bwhitman");
			return default(T);
		}
		bool flag = false;
		Type[] interfaces = typeFromHandle.GetInterfaces();
		int num = 0;
		if (0 < (nint)interfaces.LongLength)
		{
			do
			{
				if (!interfaces[num].Equals(typeof(ICollectionValue)))
				{
					num++;
					continue;
				}
				flag = true;
				break;
			}
			while (num < (nint)interfaces.LongLength);
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(parameterName);
		IValue result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			switch (value)
			{
			case ValueType.VT_COLLECTION:
				break;
			case ValueType.VT_OBJECT:
				break;
			case ValueType.VT_FLOAT:
				if (!flag)
				{
					goto IL_0137;
				}
				result = new FloatCollectionValue(valueSet, standardStringWrapper.Value);
				goto IL_012d;
			case ValueType.VT_INT:
				goto IL_0158;
			case ValueType.VT_BOOL:
				goto IL_019f;
			case ValueType.VT_RGB:
				goto IL_01e6;
			case ValueType.VT_STRING:
				goto IL_022d;
			case ValueType.VT_COORD2D:
				goto IL_0274;
			case ValueType.VT_COORD3D:
				goto IL_02bb;
			case ValueType.VT_BLP_ENTRY:
				goto IL_0302;
			case ValueType.VT_ARTDEF_REFERENCE:
				goto IL_0349;
			case ValueType.VT_CURVE:
				goto IL_0390;
			case ValueType.VT_TUPLE:
				goto IL_03b1;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		IValue result2;
		try
		{
			BugSubmitter.SilentAssert(condition: false, $"Unknown value type found in CreateValue.  Type: '{value}'.  @assign bwhitman");
			result2 = default(T);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result2;
		IL_0302:
		IValue result3;
		try
		{
			if (flag)
			{
				result3 = new BLPEntryCollectionValue(valueSet, standardStringWrapper.Value);
				goto IL_031e;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		IValue result4;
		try
		{
			result4 = new BLPEntryValue(valueSet, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result4;
		IL_0274:
		IValue result5;
		try
		{
			if (flag)
			{
				result5 = new Coord2DCollectionValue(valueSet, standardStringWrapper.Value);
				goto IL_0290;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		IValue result6;
		try
		{
			result6 = new Coord2DValue(valueSet, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result6;
		IL_0365:
		((IDisposable)standardStringWrapper).Dispose();
		IValue result7;
		return result7;
		IL_0249:
		((IDisposable)standardStringWrapper).Dispose();
		IValue result8;
		return result8;
		IL_0390:
		IValue result9;
		try
		{
			result9 = new CurveValue(valueSet, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result9;
		IL_012d:
		((IDisposable)standardStringWrapper).Dispose();
		return result;
		IL_0137:
		IValue result10;
		try
		{
			result10 = new FloatValue(valueSet, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result10;
		IL_0349:
		try
		{
			if (flag)
			{
				result7 = new ArtDefRefCollectionValue(valueSet, standardStringWrapper.Value);
				goto IL_0365;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		IValue result11;
		try
		{
			result11 = new ArtDefRefValue(valueSet, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result11;
		IL_0158:
		IValue result12;
		try
		{
			if (flag)
			{
				result12 = new IntCollectionValue(valueSet, standardStringWrapper.Value);
				goto IL_0174;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		IValue result13;
		try
		{
			result13 = new IntValue(valueSet, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result13;
		IL_0174:
		((IDisposable)standardStringWrapper).Dispose();
		return result12;
		IL_02d7:
		((IDisposable)standardStringWrapper).Dispose();
		IValue result14;
		return result14;
		IL_0290:
		((IDisposable)standardStringWrapper).Dispose();
		return result5;
		IL_019f:
		IValue result15;
		try
		{
			if (flag)
			{
				result15 = new BoolCollectionValue(valueSet, standardStringWrapper.Value);
				goto IL_01bb;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		IValue result16;
		try
		{
			result16 = new BoolValue(valueSet, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result16;
		IL_01bb:
		((IDisposable)standardStringWrapper).Dispose();
		return result15;
		IL_03cd:
		((IDisposable)standardStringWrapper).Dispose();
		IValue result17;
		return result17;
		IL_02bb:
		try
		{
			if (flag)
			{
				result14 = new Coord3DCollectionValue(valueSet, standardStringWrapper.Value);
				goto IL_02d7;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		IValue result18;
		try
		{
			result18 = new Coord3DValue(valueSet, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result18;
		IL_01e6:
		IValue result19;
		try
		{
			if (flag)
			{
				result19 = new RGBCollectionValue(valueSet, standardStringWrapper.Value);
				goto IL_0202;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		IValue result20;
		try
		{
			result20 = new RGBValue(valueSet, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result20;
		IL_0202:
		((IDisposable)standardStringWrapper).Dispose();
		return result19;
		IL_031e:
		((IDisposable)standardStringWrapper).Dispose();
		return result3;
		IL_03b1:
		try
		{
			if (flag)
			{
				result17 = new TupleCollectionValue(valueSet, standardStringWrapper.Value);
				goto IL_03cd;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		IValue result21;
		try
		{
			result21 = new TupleValue(valueSet, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result21;
		IL_022d:
		try
		{
			if (flag)
			{
				result8 = new StringCollectionValue(valueSet, standardStringWrapper.Value);
				goto IL_0249;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		IValue result22;
		try
		{
			result22 = new StringValue(valueSet, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result22;
	}

	public unsafe static CollectionValue CreateCollectionValue(global::AssetObjects.CollectionValue* value)
	{
		//IL_00ce: Expected I, but got I8
		//IL_00f6: Expected I, but got I8
		byte condition = (byte)(((long)(nint)value != 0) ? 1 : 0);
		BugSubmitter.SilentAssert(condition != 0, "Attempting to create a managed value with a null native value.  @assign bwhitman");
		if (value == null)
		{
			return null;
		}
		switch (global::_003CModule_003E.AssetObjects_002ECollectionValue_002EGetEntryValueType(value))
		{
		case (global::AssetObjects.ValueType)0:
			return new FloatCollectionValue(value);
		case (global::AssetObjects.ValueType)1:
			return new IntCollectionValue(value);
		case (global::AssetObjects.ValueType)2:
			return new BoolCollectionValue(value);
		case (global::AssetObjects.ValueType)3:
			return new RGBCollectionValue(value);
		case (global::AssetObjects.ValueType)4:
			return new StringCollectionValue(value);
		case (global::AssetObjects.ValueType)5:
			return new ObjectCollectionValue(value);
		case (global::AssetObjects.ValueType)6:
			return new Coord2DCollectionValue(value);
		case (global::AssetObjects.ValueType)7:
			return new Coord3DCollectionValue(value);
		case (global::AssetObjects.ValueType)8:
			return new BLPEntryCollectionValue(value);
		case (global::AssetObjects.ValueType)9:
			return new ArtDefRefCollectionValue(value);
		case (global::AssetObjects.ValueType)12:
			return new TupleCollectionValue(value);
		case (global::AssetObjects.ValueType)10:
			if (!global::_003CModule_003E._003FA0x557ea6cf_002E_003FbIgnoreAlways_0040_003F6_003F_003FCreateCollectionValue_0040ValueFactory_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAVCollectionValue_0040345_0040PEAV63_0040_0040Z_00404_NA && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_01GBGANLPD_00400_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 348u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x557ea6cf_002E_003FbIgnoreAlways_0040_003F6_003F_003FCreateCollectionValue_0040ValueFactory_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAVCollectionValue_0040345_0040PEAV63_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return null;
		default:
			if (!global::_003CModule_003E._003FA0x557ea6cf_002E_003FbIgnoreAlways_0040_003FO_0040_003F_003FCreateCollectionValue_0040ValueFactory_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAVCollectionValue_0040345_0040PEAV63_0040_0040Z_00404_NA && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_01GBGANLPD_00400_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 351u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x557ea6cf_002E_003FbIgnoreAlways_0040_003FO_0040_003F_003FCreateCollectionValue_0040ValueFactory_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAVCollectionValue_0040345_0040PEAV63_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return null;
		}
	}

	public unsafe static global::AssetObjects.Value* CloneValue(global::AssetObjects.Value* value)
	{
		//IL_0078: Expected I, but got I8
		//IL_00c8: Expected I, but got I8
		//IL_0118: Expected I, but got I8
		//IL_0168: Expected I, but got I8
		//IL_01b8: Expected I, but got I8
		//IL_0208: Expected I, but got I8
		//IL_0258: Expected I, but got I8
		//IL_02a8: Expected I, but got I8
		//IL_02fb: Expected I, but got I8
		//IL_034b: Expected I, but got I8
		//IL_0397: Expected I, but got I8
		//IL_03e6: Expected I, but got I8
		//IL_0432: Expected I, but got I8
		//IL_0476: Expected I, but got I8
		//IL_0470: Expected I, but got I8
		switch (global::_003CModule_003E.AssetObjects_002EValue_002EGetType(value))
		{
		case (global::AssetObjects.ValueType)0:
		{
			int num2 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.FloatValue* ptr2 = (global::AssetObjects.FloatValue*)global::_003CModule_003E.@new(56uL, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 362, 23, 0);
			try
			{
				if (ptr2 != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EFloatValue_002E_007Bctor_007D(ptr2, (global::AssetObjects.FloatValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr2, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 362, 23, 0);
				throw;
			}
		}
		case (global::AssetObjects.ValueType)1:
		{
			int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.IntValue* ptr = (global::AssetObjects.IntValue*)global::_003CModule_003E.@new(56uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 364, 23, 0);
			try
			{
				if (ptr != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EIntValue_002E_007Bctor_007D(ptr, (global::AssetObjects.IntValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 364, 23, 0);
				throw;
			}
		}
		case (global::AssetObjects.ValueType)2:
		{
			int num3 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.BoolValue* ptr3 = (global::AssetObjects.BoolValue*)global::_003CModule_003E.@new(56uL, num3, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 366, 23, 0);
			try
			{
				if (ptr3 != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EBoolValue_002E_007Bctor_007D(ptr3, (global::AssetObjects.BoolValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr3, num3, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 366, 23, 0);
				throw;
			}
		}
		case (global::AssetObjects.ValueType)3:
		{
			int num13 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.RGBValue* ptr13 = (global::AssetObjects.RGBValue*)global::_003CModule_003E.@new(64uL, num13, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 368, 23, 0);
			try
			{
				if (ptr13 != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ERGBValue_002E_007Bctor_007D(ptr13, (global::AssetObjects.RGBValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr13, num13, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 368, 23, 0);
				throw;
			}
		}
		case (global::AssetObjects.ValueType)4:
		{
			int num12 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.StringValue* ptr12 = (global::AssetObjects.StringValue*)global::_003CModule_003E.@new(72uL, num12, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 370, 23, 0);
			try
			{
				if (ptr12 != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EStringValue_002E_007Bctor_007D(ptr12, (global::AssetObjects.StringValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr12, num12, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 370, 23, 0);
				throw;
			}
		}
		case (global::AssetObjects.ValueType)5:
		{
			int num11 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.ObjectValue* ptr11 = (global::AssetObjects.ObjectValue*)global::_003CModule_003E.@new(88uL, num11, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 372, 23, 0);
			try
			{
				if (ptr11 != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EObjectValue_002E_007Bctor_007D(ptr11, (global::AssetObjects.ObjectValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr11, num11, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 372, 23, 0);
				throw;
			}
		}
		case (global::AssetObjects.ValueType)6:
		{
			int num10 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.Coord2DValue* ptr10 = (global::AssetObjects.Coord2DValue*)global::_003CModule_003E.@new(56uL, num10, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 374, 23, 0);
			try
			{
				if (ptr10 != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECoord2DValue_002E_007Bctor_007D(ptr10, (global::AssetObjects.Coord2DValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr10, num10, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 374, 23, 0);
				throw;
			}
		}
		case (global::AssetObjects.ValueType)7:
		{
			int num9 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.Coord3DValue* ptr9 = (global::AssetObjects.Coord3DValue*)global::_003CModule_003E.@new(64uL, num9, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 376, 23, 0);
			try
			{
				if (ptr9 != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECoord3DValue_002E_007Bctor_007D(ptr9, (global::AssetObjects.Coord3DValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr9, num9, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 376, 23, 0);
				throw;
			}
		}
		case (global::AssetObjects.ValueType)8:
		{
			int num8 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.BLPEntryValue* ptr8 = (global::AssetObjects.BLPEntryValue*)global::_003CModule_003E.@new(176uL, num8, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 378, 23, 0);
			try
			{
				if (ptr8 != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002E_007Bctor_007D(ptr8, (global::AssetObjects.BLPEntryValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr8, num8, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 378, 23, 0);
				throw;
			}
		}
		case (global::AssetObjects.ValueType)9:
		{
			int num7 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			ArtDefReferenceValue* ptr7 = (ArtDefReferenceValue*)global::_003CModule_003E.@new(160uL, num7, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 380, 23, 0);
			try
			{
				if (ptr7 != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002E_007Bctor_007D(ptr7, (ArtDefReferenceValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr7, num7, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 380, 23, 0);
				throw;
			}
		}
		case (global::AssetObjects.ValueType)10:
		{
			int num6 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.CollectionValue* ptr6 = (global::AssetObjects.CollectionValue*)global::_003CModule_003E.@new(120uL, num6, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 382, 23, 0);
			try
			{
				if (ptr6 != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECollectionValue_002E_007Bctor_007D(ptr6, (global::AssetObjects.CollectionValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr6, num6, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 382, 23, 0);
				throw;
			}
		}
		case (global::AssetObjects.ValueType)11:
		{
			int num5 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.CurveValue* ptr5 = (global::AssetObjects.CurveValue*)global::_003CModule_003E.@new(128uL, num5, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 384, 23, 0);
			try
			{
				if (ptr5 != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECurveValue_002E_007Bctor_007D(ptr5, (global::AssetObjects.CurveValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr5, num5, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 384, 23, 0);
				throw;
			}
		}
		case (global::AssetObjects.ValueType)12:
		{
			int num4 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			global::AssetObjects.TupleValue* ptr4 = (global::AssetObjects.TupleValue*)global::_003CModule_003E.@new(112uL, num4, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 386, 23, 0);
			try
			{
				if (ptr4 != null)
				{
					return (global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ETupleValue_002E_007Bctor_007D(ptr4, (global::AssetObjects.TupleValue*)value);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr4, num4, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 386, 23, 0);
				throw;
			}
		}
		default:
			if (!global::_003CModule_003E._003FA0x557ea6cf_002E_003FbIgnoreAlways_0040_003F4_003F_003FCloneValue_0040ValueFactory_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPEAVValue_00403_0040PEAV63_0040_0040Z_00404_NA && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_01GBGANLPD_00400_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040CHECIFFK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 388u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x557ea6cf_002E_003FbIgnoreAlways_0040_003F4_003F_003FCloneValue_0040ValueFactory_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPEAVValue_00403_0040PEAV63_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return null;
		}
	}

	private static void PopulateTypeDictionary()
	{
		m_typeToValueTypeMap[typeof(IFloatValue)] = ValueType.VT_FLOAT;
		m_typeToValueTypeMap[typeof(IFloatCollectionValue)] = ValueType.VT_FLOAT;
		m_typeToValueTypeMap[typeof(IIntValue)] = ValueType.VT_INT;
		m_typeToValueTypeMap[typeof(IIntCollectionValue)] = ValueType.VT_INT;
		m_typeToValueTypeMap[typeof(IBoolValue)] = ValueType.VT_BOOL;
		m_typeToValueTypeMap[typeof(IBoolCollectionValue)] = ValueType.VT_BOOL;
		m_typeToValueTypeMap[typeof(IRGBValue)] = ValueType.VT_RGB;
		m_typeToValueTypeMap[typeof(IRGBCollectionValue)] = ValueType.VT_RGB;
		m_typeToValueTypeMap[typeof(IStringValue)] = ValueType.VT_STRING;
		m_typeToValueTypeMap[typeof(IStringCollectionValue)] = ValueType.VT_STRING;
		m_typeToValueTypeMap[typeof(ICoord2DValue)] = ValueType.VT_COORD2D;
		m_typeToValueTypeMap[typeof(ICoord2DCollectionValue)] = ValueType.VT_COORD2D;
		m_typeToValueTypeMap[typeof(ICoord3DValue)] = ValueType.VT_COORD3D;
		m_typeToValueTypeMap[typeof(ICoord3DCollectionValue)] = ValueType.VT_COORD3D;
		m_typeToValueTypeMap[typeof(IBLPEntryValue)] = ValueType.VT_BLP_ENTRY;
		m_typeToValueTypeMap[typeof(IBLPEntryCollectionValue)] = ValueType.VT_BLP_ENTRY;
		m_typeToValueTypeMap[typeof(IArtDefRefValue)] = ValueType.VT_ARTDEF_REFERENCE;
		m_typeToValueTypeMap[typeof(IArtDefRefCollectionValue)] = ValueType.VT_ARTDEF_REFERENCE;
		m_typeToValueTypeMap[typeof(ICurveValue)] = ValueType.VT_CURVE;
		m_typeToValueTypeMap[typeof(ITupleValue)] = ValueType.VT_TUPLE;
		m_typeToValueTypeMap[typeof(ITupleCollectionValue)] = ValueType.VT_TUPLE;
	}
}

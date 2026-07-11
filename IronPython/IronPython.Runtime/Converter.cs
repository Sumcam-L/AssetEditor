using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

public static class Converter
{
	private static readonly CallSite<Func<CallSite, object, int>> _intSite = MakeExplicitConvertSite<int>();

	private static readonly CallSite<Func<CallSite, object, double>> _doubleSite = MakeExplicitConvertSite<double>();

	private static readonly CallSite<Func<CallSite, object, Complex>> _complexSite = MakeExplicitConvertSite<Complex>();

	private static readonly CallSite<Func<CallSite, object, BigInteger>> _bigIntSite = MakeExplicitConvertSite<BigInteger>();

	private static readonly CallSite<Func<CallSite, object, string>> _stringSite = MakeExplicitConvertSite<string>();

	private static readonly CallSite<Func<CallSite, object, bool>> _boolSite = MakeExplicitConvertSite<bool>();

	private static readonly CallSite<Func<CallSite, object, char>> _charSite = MakeImplicitConvertSite<char>();

	private static readonly CallSite<Func<CallSite, object, char>> _explicitCharSite = MakeExplicitConvertSite<char>();

	private static readonly CallSite<Func<CallSite, object, IEnumerable>> _ienumerableSite = MakeImplicitConvertSite<IEnumerable>();

	private static readonly CallSite<Func<CallSite, object, IEnumerator>> _ienumeratorSite = MakeImplicitConvertSite<IEnumerator>();

	private static readonly Dictionary<Type, CallSite<Func<CallSite, object, object>>> _siteDict = new Dictionary<Type, CallSite<Func<CallSite, object, object>>>();

	private static readonly CallSite<Func<CallSite, object, byte>> _byteSite = MakeExplicitConvertSite<byte>();

	private static readonly CallSite<Func<CallSite, object, sbyte>> _sbyteSite = MakeExplicitConvertSite<sbyte>();

	private static readonly CallSite<Func<CallSite, object, short>> _int16Site = MakeExplicitConvertSite<short>();

	private static readonly CallSite<Func<CallSite, object, ushort>> _uint16Site = MakeExplicitConvertSite<ushort>();

	private static readonly CallSite<Func<CallSite, object, uint>> _uint32Site = MakeExplicitConvertSite<uint>();

	private static readonly CallSite<Func<CallSite, object, long>> _int64Site = MakeExplicitConvertSite<long>();

	private static readonly CallSite<Func<CallSite, object, ulong>> _uint64Site = MakeExplicitConvertSite<ulong>();

	private static readonly CallSite<Func<CallSite, object, decimal>> _decimalSite = MakeExplicitConvertSite<decimal>();

	private static readonly CallSite<Func<CallSite, object, float>> _floatSite = MakeExplicitConvertSite<float>();

	private static readonly CallSite<Func<CallSite, object, object>> _tryByteSite = MakeExplicitTrySite<byte>();

	private static readonly CallSite<Func<CallSite, object, object>> _trySByteSite = MakeExplicitTrySite<sbyte>();

	private static readonly CallSite<Func<CallSite, object, object>> _tryInt16Site = MakeExplicitTrySite<short>();

	private static readonly CallSite<Func<CallSite, object, object>> _tryInt32Site = MakeExplicitTrySite<int>();

	private static readonly CallSite<Func<CallSite, object, object>> _tryInt64Site = MakeExplicitTrySite<long>();

	private static readonly CallSite<Func<CallSite, object, object>> _tryUInt16Site = MakeExplicitTrySite<ushort>();

	private static readonly CallSite<Func<CallSite, object, object>> _tryUInt32Site = MakeExplicitTrySite<uint>();

	private static readonly CallSite<Func<CallSite, object, object>> _tryUInt64Site = MakeExplicitTrySite<ulong>();

	private static readonly CallSite<Func<CallSite, object, object>> _tryDoubleSite = MakeExplicitTrySite<double>();

	private static readonly CallSite<Func<CallSite, object, object>> _tryCharSite = MakeExplicitTrySite<char>();

	private static readonly CallSite<Func<CallSite, object, object>> _tryBigIntegerSite = MakeExplicitTrySite<BigInteger>();

	private static readonly CallSite<Func<CallSite, object, object>> _tryComplexSite = MakeExplicitTrySite<Complex>();

	private static readonly CallSite<Func<CallSite, object, object>> _tryStringSite = MakeExplicitTrySite<string>();

	private static readonly Type StringType = typeof(string);

	private static readonly Type Int32Type = typeof(int);

	private static readonly Type DoubleType = typeof(double);

	private static readonly Type DecimalType = typeof(decimal);

	private static readonly Type Int64Type = typeof(long);

	private static readonly Type CharType = typeof(char);

	private static readonly Type SingleType = typeof(float);

	private static readonly Type BooleanType = typeof(bool);

	private static readonly Type BigIntegerType = typeof(BigInteger);

	private static readonly Type ComplexType = typeof(Complex);

	private static readonly Type DelegateType = typeof(Delegate);

	private static readonly Type IEnumerableType = typeof(IEnumerable);

	private static readonly Type TypeType = typeof(Type);

	private static readonly Type NullableOfTType = typeof(Nullable<>);

	private static readonly Type IListOfTType = typeof(IList<>);

	private static readonly Type IDictOfTType = typeof(IDictionary<, >);

	private static readonly Type IEnumerableOfTType = typeof(IEnumerable<>);

	private static readonly Type IListOfObjectType = typeof(IList<object>);

	private static readonly Type IEnumerableOfObjectType = typeof(IEnumerable<object>);

	private static readonly Type IDictionaryOfObjectType = typeof(IDictionary<object, object>);

	private static CallSite<Func<CallSite, object, T>> MakeImplicitConvertSite<T>()
	{
		return MakeConvertSite<T>(ConversionResultKind.ImplicitCast);
	}

	private static CallSite<Func<CallSite, object, T>> MakeExplicitConvertSite<T>()
	{
		return MakeConvertSite<T>(ConversionResultKind.ExplicitCast);
	}

	private static CallSite<Func<CallSite, object, T>> MakeConvertSite<T>(ConversionResultKind kind)
	{
		return CallSite<Func<CallSite, object, T>>.Create(DefaultContext.DefaultPythonContext.Convert(typeof(T), kind));
	}

	private static CallSite<Func<CallSite, object, object>> MakeExplicitTrySite<T>()
	{
		return MakeTrySite<T>(ConversionResultKind.ExplicitTry);
	}

	private static CallSite<Func<CallSite, object, object>> MakeTrySite<T>(ConversionResultKind kind)
	{
		return CallSite<Func<CallSite, object, object>>.Create(DefaultContext.DefaultPythonContext.Convert(typeof(T), kind));
	}

	public static int ConvertToInt32(object value)
	{
		return _intSite.Target(_intSite, value);
	}

	public static string ConvertToString(object value)
	{
		return _stringSite.Target(_stringSite, value);
	}

	public static BigInteger ConvertToBigInteger(object value)
	{
		return _bigIntSite.Target(_bigIntSite, value);
	}

	public static double ConvertToDouble(object value)
	{
		return _doubleSite.Target(_doubleSite, value);
	}

	public static Complex ConvertToComplex(object value)
	{
		return _complexSite.Target(_complexSite, value);
	}

	public static bool ConvertToBoolean(object value)
	{
		return _boolSite.Target(_boolSite, value);
	}

	public static long ConvertToInt64(object value)
	{
		return _int64Site.Target(_int64Site, value);
	}

	public static byte ConvertToByte(object value)
	{
		return _byteSite.Target(_byteSite, value);
	}

	public static sbyte ConvertToSByte(object value)
	{
		return _sbyteSite.Target(_sbyteSite, value);
	}

	public static short ConvertToInt16(object value)
	{
		return _int16Site.Target(_int16Site, value);
	}

	public static ushort ConvertToUInt16(object value)
	{
		return _uint16Site.Target(_uint16Site, value);
	}

	public static uint ConvertToUInt32(object value)
	{
		return _uint32Site.Target(_uint32Site, value);
	}

	public static ulong ConvertToUInt64(object value)
	{
		return _uint64Site.Target(_uint64Site, value);
	}

	public static float ConvertToSingle(object value)
	{
		return _floatSite.Target(_floatSite, value);
	}

	public static decimal ConvertToDecimal(object value)
	{
		return _decimalSite.Target(_decimalSite, value);
	}

	public static char ConvertToChar(object value)
	{
		return _charSite.Target(_charSite, value);
	}

	internal static bool TryConvertToByte(object value, out byte result)
	{
		object obj = _tryByteSite.Target(_tryByteSite, value);
		if (obj != null)
		{
			result = (byte)obj;
			return true;
		}
		result = 0;
		return false;
	}

	internal static bool TryConvertToSByte(object value, out sbyte result)
	{
		object obj = _trySByteSite.Target(_trySByteSite, value);
		if (obj != null)
		{
			result = (sbyte)obj;
			return true;
		}
		result = 0;
		return false;
	}

	internal static bool TryConvertToInt16(object value, out short result)
	{
		object obj = _tryInt16Site.Target(_tryInt16Site, value);
		if (obj != null)
		{
			result = (short)obj;
			return true;
		}
		result = 0;
		return false;
	}

	internal static bool TryConvertToInt32(object value, out int result)
	{
		object obj = _tryInt32Site.Target(_tryInt32Site, value);
		if (obj != null)
		{
			result = (int)obj;
			return true;
		}
		result = 0;
		return false;
	}

	internal static bool TryConvertToInt64(object value, out long result)
	{
		object obj = _tryInt64Site.Target(_tryInt64Site, value);
		if (obj != null)
		{
			result = (long)obj;
			return true;
		}
		result = 0L;
		return false;
	}

	internal static bool TryConvertToUInt16(object value, out ushort result)
	{
		object obj = _tryUInt16Site.Target(_tryUInt16Site, value);
		if (obj != null)
		{
			result = (ushort)obj;
			return true;
		}
		result = 0;
		return false;
	}

	internal static bool TryConvertToUInt32(object value, out uint result)
	{
		object obj = _tryUInt32Site.Target(_tryUInt32Site, value);
		if (obj != null)
		{
			result = (uint)obj;
			return true;
		}
		result = 0u;
		return false;
	}

	internal static bool TryConvertToUInt64(object value, out ulong result)
	{
		object obj = _tryUInt64Site.Target(_tryUInt64Site, value);
		if (obj != null)
		{
			result = (ulong)obj;
			return true;
		}
		result = 0uL;
		return false;
	}

	internal static bool TryConvertToDouble(object value, out double result)
	{
		object obj = _tryDoubleSite.Target(_tryDoubleSite, value);
		if (obj != null)
		{
			result = (double)obj;
			return true;
		}
		result = 0.0;
		return false;
	}

	internal static bool TryConvertToBigInteger(object value, out BigInteger result)
	{
		object obj = _tryBigIntegerSite.Target(_tryBigIntegerSite, value);
		if (obj != null)
		{
			result = (BigInteger)obj;
			return true;
		}
		result = default(BigInteger);
		return false;
	}

	internal static bool TryConvertToComplex(object value, out Complex result)
	{
		object obj = _tryComplexSite.Target(_tryComplexSite, value);
		if (obj != null)
		{
			result = (Complex)obj;
			return true;
		}
		result = default(Complex);
		return false;
	}

	internal static bool TryConvertToString(object value, out string result)
	{
		object obj = _tryStringSite.Target(_tryStringSite, value);
		if (obj != null)
		{
			result = (string)obj;
			return true;
		}
		result = null;
		return false;
	}

	internal static bool TryConvertToChar(object value, out char result)
	{
		object obj = _tryCharSite.Target(_tryCharSite, value);
		if (obj != null)
		{
			result = (char)obj;
			return true;
		}
		result = '\0';
		return false;
	}

	internal static char ExplicitConvertToChar(object value)
	{
		return _explicitCharSite.Target(_explicitCharSite, value);
	}

	public static T Convert<T>(object value)
	{
		return (T)Convert(value, typeof(T));
	}

	internal static bool TryConvert(object value, Type to, out object result)
	{
		try
		{
			result = Convert(value, to);
			return true;
		}
		catch
		{
			result = null;
			return false;
		}
	}

	internal static object Convert(object value, Type to)
	{
		CallSite<Func<CallSite, object, object>> value2;
		lock (_siteDict)
		{
			if (!_siteDict.TryGetValue(to, out value2))
			{
				value2 = (_siteDict[to] = CallSite<Func<CallSite, object, object>>.Create(DefaultContext.DefaultPythonContext.ConvertRetObject(to, ConversionResultKind.ExplicitCast)));
			}
		}
		object obj = value2.Target(value2, value);
		if (to.IsValueType() && obj == null && (!to.IsGenericType() || to.GetGenericTypeDefinition() != typeof(Nullable<>)))
		{
			throw MakeTypeError(to, value);
		}
		return obj;
	}

	internal static bool TryConvertToIEnumerator(object o, out IEnumerator e)
	{
		try
		{
			e = _ienumeratorSite.Target(_ienumeratorSite, o);
			return e != null;
		}
		catch
		{
			e = null;
			return false;
		}
	}

	internal static IEnumerator ConvertToIEnumerator(object o)
	{
		return _ienumeratorSite.Target(_ienumeratorSite, o);
	}

	public static IEnumerable ConvertToIEnumerable(object o)
	{
		return _ienumerableSite.Target(_ienumerableSite, o);
	}

	internal static bool TryConvertToIndex(object value, out int index)
	{
		return TryConvertToIndex(value, true, out index);
	}

	internal static bool TryConvertToIndex(object value, bool throwOverflowError, out int index)
	{
		int? num = ConvertToSliceIndexHelper(value, throwOverflowError);
		if (!num.HasValue && PythonOps.TryGetBoundAttr(value, "__index__", out var ret))
		{
			num = ConvertToSliceIndexHelper(PythonCalls.Call(ret), throwOverflowError);
		}
		index = (num.HasValue ? num.Value : 0);
		return num.HasValue;
	}

	internal static bool TryConvertToIndex(object value, out object index)
	{
		return TryConvertToIndex(value, true, out index);
	}

	internal static bool TryConvertToIndex(object value, bool throwOverflowError, out object index)
	{
		index = ConvertToSliceIndexHelper(value);
		if (index == null && PythonOps.TryGetBoundAttr(value, "__index__", out var ret))
		{
			index = ConvertToSliceIndexHelper(PythonCalls.Call(ret));
		}
		return index != null;
	}

	public static int ConvertToIndex(object value)
	{
		int? num = ConvertToSliceIndexHelper(value, throwOverflowError: false);
		if (num.HasValue)
		{
			return num.Value;
		}
		if (PythonOps.TryGetBoundAttr(value, "__index__", out var ret))
		{
			object obj = PythonCalls.Call(ret);
			num = ConvertToSliceIndexHelper(obj, throwOverflowError: false);
			if (num.HasValue)
			{
				return num.Value;
			}
			throw PythonOps.TypeError("__index__ returned bad value: {0}", DynamicHelpers.GetPythonType(obj).Name);
		}
		throw PythonOps.TypeError("expected index value, got {0}", DynamicHelpers.GetPythonType(value).Name);
	}

	private static int? ConvertToSliceIndexHelper(object value, bool throwOverflowError)
	{
		if (value is int)
		{
			return (int)value;
		}
		if (value is Extensible<int>)
		{
			return ((Extensible<int>)value).Value;
		}
		BigInteger bigInteger;
		if (value is BigInteger)
		{
			bigInteger = (BigInteger)value;
		}
		else
		{
			if (!(value is Extensible<BigInteger> extensible))
			{
				return null;
			}
			bigInteger = extensible.Value;
		}
		if (bigInteger.AsInt32(out var ret))
		{
			return ret;
		}
		if (throwOverflowError)
		{
			throw PythonOps.OverflowError("can't fit long into index");
		}
		return (!(bigInteger == BigInteger.Zero)) ? ((bigInteger > 0L) ? int.MaxValue : int.MinValue) : 0;
	}

	private static object ConvertToSliceIndexHelper(object value)
	{
		if (value is int)
		{
			return value;
		}
		if (value is Extensible<int>)
		{
			return ScriptingRuntimeHelpers.Int32ToObject(((Extensible<int>)value).Value);
		}
		if (value is BigInteger)
		{
			return value;
		}
		if (value is Extensible<BigInteger>)
		{
			return ((Extensible<BigInteger>)value).Value;
		}
		return null;
	}

	internal static Exception CannotConvertOverflow(string name, object value)
	{
		return PythonOps.OverflowError("Cannot convert {0}({1}) to {2}", PythonTypeOps.GetName(value), value, name);
	}

	private static Exception MakeTypeError(Type expectedType, object o)
	{
		return MakeTypeError(DynamicHelpers.GetPythonTypeFromType(expectedType).Name.ToString(), o);
	}

	private static Exception MakeTypeError(string expectedType, object o)
	{
		return PythonOps.TypeErrorForTypeMismatch(expectedType, o);
	}

	public static object ConvertToReferenceType(object fromObject, RuntimeTypeHandle typeHandle)
	{
		if (fromObject == null)
		{
			return null;
		}
		return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
	}

	public static object ConvertToNullableType(object fromObject, RuntimeTypeHandle typeHandle)
	{
		if (fromObject == null)
		{
			return null;
		}
		return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
	}

	public static object ConvertToValueType(object fromObject, RuntimeTypeHandle typeHandle)
	{
		if (fromObject == null)
		{
			throw PythonOps.InvalidType(fromObject, typeHandle);
		}
		return Convert(fromObject, Type.GetTypeFromHandle(typeHandle));
	}

	public static Type ConvertToType(object value)
	{
		if (value == null)
		{
			return null;
		}
		Type type = value as Type;
		if (type != null)
		{
			return type;
		}
		if (value is PythonType pythonType)
		{
			return pythonType.UnderlyingSystemType;
		}
		if (value is TypeGroup typeGroup && typeGroup.TryGetNonGenericType(out var nonGenericType))
		{
			return nonGenericType;
		}
		throw MakeTypeError("Type", value);
	}

	public static object ConvertToDelegate(object value, Type to)
	{
		if (value == null)
		{
			return null;
		}
		return DefaultContext.DefaultCLS.LanguageContext.DelegateCreator.GetDelegate(value, to);
	}

	public static bool CanConvertFrom(Type fromType, Type toType, NarrowingLevel allowNarrowing)
	{
		ContractUtils.RequiresNotNull(fromType, "fromType");
		ContractUtils.RequiresNotNull(toType, "toType");
		if (toType == fromType)
		{
			return true;
		}
		if (toType.IsAssignableFrom(fromType))
		{
			return true;
		}
		if (fromType.IsCOMObject && toType.IsInterface)
		{
			return true;
		}
		if (HasImplicitNumericConversion(fromType, toType))
		{
			return true;
		}
		if (toType == TypeType && (typeof(PythonType).IsAssignableFrom(fromType) || typeof(TypeGroup).IsAssignableFrom(fromType)))
		{
			return true;
		}
		if (typeof(Extensible<int>).IsAssignableFrom(fromType) && CanConvertFrom(Int32Type, toType, allowNarrowing))
		{
			return true;
		}
		if (typeof(Extensible<BigInteger>).IsAssignableFrom(fromType) && CanConvertFrom(BigIntegerType, toType, allowNarrowing))
		{
			return true;
		}
		if (typeof(ExtensibleString).IsAssignableFrom(fromType) && CanConvertFrom(StringType, toType, allowNarrowing))
		{
			return true;
		}
		if (typeof(Extensible<double>).IsAssignableFrom(fromType) && CanConvertFrom(DoubleType, toType, allowNarrowing))
		{
			return true;
		}
		if (typeof(Extensible<Complex>).IsAssignableFrom(fromType) && CanConvertFrom(ComplexType, toType, allowNarrowing))
		{
			return true;
		}
		object[] customAttributes = toType.GetCustomAttributes(typeof(TypeConverterAttribute), inherit: true);
		object[] array = customAttributes;
		for (int i = 0; i < array.Length; i++)
		{
			TypeConverterAttribute tca = (TypeConverterAttribute)array[i];
			TypeConverter typeConverter = GetTypeConverter(tca);
			if (typeConverter != null && typeConverter.CanConvertFrom(fromType))
			{
				return true;
			}
		}
		if (allowNarrowing == NarrowingLevel.None)
		{
			return false;
		}
		return HasNarrowingConversion(fromType, toType, allowNarrowing);
	}

	private static TypeConverter GetTypeConverter(TypeConverterAttribute tca)
	{
		try
		{
			ConstructorInfo constructor = Type.GetType(tca.ConverterTypeName).GetConstructor(ReflectionUtils.EmptyTypes);
			if (constructor != null)
			{
				return constructor.Invoke(ArrayUtils.EmptyObjects) as TypeConverter;
			}
		}
		catch (TargetInvocationException)
		{
		}
		return null;
	}

	private static bool HasImplicitNumericConversion(Type fromType, Type toType)
	{
		if (fromType.IsEnum())
		{
			return false;
		}
		if (fromType == typeof(BigInteger))
		{
			if (toType == typeof(double))
			{
				return true;
			}
			if (toType == typeof(float))
			{
				return true;
			}
			if (toType == typeof(Complex))
			{
				return true;
			}
			return false;
		}
		if (fromType == typeof(bool))
		{
			if (toType == typeof(int))
			{
				return true;
			}
			return HasImplicitNumericConversion(typeof(int), toType);
		}
		switch (fromType.GetTypeCode())
		{
		case TypeCode.SByte:
			switch (toType.GetTypeCode())
			{
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return true;
			default:
				if (toType == BigIntegerType)
				{
					return true;
				}
				if (toType == ComplexType)
				{
					return true;
				}
				return false;
			}
		case TypeCode.Byte:
			switch (toType.GetTypeCode())
			{
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return true;
			default:
				if (toType == BigIntegerType)
				{
					return true;
				}
				if (toType == ComplexType)
				{
					return true;
				}
				return false;
			}
		case TypeCode.Int16:
			switch (toType.GetTypeCode())
			{
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return true;
			default:
				if (toType == BigIntegerType)
				{
					return true;
				}
				if (toType == ComplexType)
				{
					return true;
				}
				return false;
			}
		case TypeCode.UInt16:
			switch (toType.GetTypeCode())
			{
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return true;
			default:
				if (toType == BigIntegerType)
				{
					return true;
				}
				if (toType == ComplexType)
				{
					return true;
				}
				return false;
			}
		case TypeCode.Int32:
			switch (toType.GetTypeCode())
			{
			case TypeCode.Int64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return true;
			default:
				if (toType == BigIntegerType)
				{
					return true;
				}
				if (toType == ComplexType)
				{
					return true;
				}
				return false;
			}
		case TypeCode.UInt32:
			switch (toType.GetTypeCode())
			{
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return true;
			default:
				if (toType == BigIntegerType)
				{
					return true;
				}
				if (toType == ComplexType)
				{
					return true;
				}
				return false;
			}
		case TypeCode.Int64:
			switch (toType.GetTypeCode())
			{
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return true;
			default:
				if (toType == BigIntegerType)
				{
					return true;
				}
				if (toType == ComplexType)
				{
					return true;
				}
				return false;
			}
		case TypeCode.UInt64:
			switch (toType.GetTypeCode())
			{
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return true;
			default:
				if (toType == BigIntegerType)
				{
					return true;
				}
				if (toType == ComplexType)
				{
					return true;
				}
				return false;
			}
		case TypeCode.Char:
			switch (toType.GetTypeCode())
			{
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return true;
			default:
				if (toType == BigIntegerType)
				{
					return true;
				}
				if (toType == ComplexType)
				{
					return true;
				}
				return false;
			}
		case TypeCode.Single:
		{
			TypeCode typeCode = toType.GetTypeCode();
			if (typeCode == TypeCode.Double)
			{
				return true;
			}
			if (toType == ComplexType)
			{
				return true;
			}
			return false;
		}
		case TypeCode.Double:
			toType.GetTypeCode();
			if (toType == ComplexType)
			{
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	public static Candidate PreferConvert(Type t1, Type t2)
	{
		if (t1 == typeof(bool) && t2 == typeof(int))
		{
			return Candidate.Two;
		}
		if (t1 == typeof(decimal) && t2 == typeof(BigInteger))
		{
			return Candidate.Two;
		}
		switch (t1.GetTypeCode())
		{
		case TypeCode.SByte:
			switch (t2.GetTypeCode())
			{
			case TypeCode.Byte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
				return Candidate.Two;
			default:
				return Candidate.Equivalent;
			}
		case TypeCode.Int16:
			switch (t2.GetTypeCode())
			{
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
				return Candidate.Two;
			default:
				return Candidate.Equivalent;
			}
		case TypeCode.Int32:
			switch (t2.GetTypeCode())
			{
			case TypeCode.UInt32:
			case TypeCode.UInt64:
				return Candidate.Two;
			default:
				return Candidate.Equivalent;
			}
		case TypeCode.Int64:
		{
			TypeCode typeCode = t2.GetTypeCode();
			if (typeCode == TypeCode.UInt64)
			{
				return Candidate.Two;
			}
			return Candidate.Equivalent;
		}
		default:
			return Candidate.Equivalent;
		}
	}

	private static bool HasNarrowingConversion(Type fromType, Type toType, NarrowingLevel allowNarrowing)
	{
		if (allowNarrowing == NarrowingLevel.Three)
		{
			if (toType == CharType && fromType == StringType)
			{
				return true;
			}
			if (toType == StringType && fromType == CharType)
			{
				return true;
			}
			if (HasImplicitConversion(fromType, toType))
			{
				return true;
			}
		}
		if (toType == DoubleType && fromType == DecimalType)
		{
			return true;
		}
		if (toType == SingleType && fromType == DecimalType)
		{
			return true;
		}
		if (toType.IsArray)
		{
			return typeof(PythonTuple).IsAssignableFrom(fromType);
		}
		if (allowNarrowing == NarrowingLevel.Three)
		{
			if (IsNumeric(fromType) && IsNumeric(toType) && fromType != typeof(float) && fromType != typeof(double) && fromType != typeof(decimal) && fromType != typeof(Complex))
			{
				return true;
			}
			if (fromType == typeof(bool) && IsNumeric(toType))
			{
				return true;
			}
			if (toType == CharType && fromType == StringType)
			{
				return true;
			}
			if (toType == Int32Type && fromType == BooleanType)
			{
				return true;
			}
			if (toType == BooleanType)
			{
				return true;
			}
			if (DelegateType.IsAssignableFrom(toType) && IsPythonType(fromType))
			{
				return true;
			}
			if (IEnumerableType == toType && IsPythonType(fromType))
			{
				return true;
			}
			if (toType == typeof(IEnumerator))
			{
				if (IsPythonType(fromType))
				{
					return true;
				}
			}
			else if (toType.IsGenericType())
			{
				Type genericTypeDefinition = toType.GetGenericTypeDefinition();
				if (genericTypeDefinition == IEnumerableOfTType)
				{
					if (!IEnumerableOfObjectType.IsAssignableFrom(fromType) && !IEnumerableType.IsAssignableFrom(fromType))
					{
						return fromType == typeof(OldInstance);
					}
					return true;
				}
				if (genericTypeDefinition == typeof(IEnumerator<>) && IsPythonType(fromType))
				{
					return true;
				}
			}
		}
		if (allowNarrowing == NarrowingLevel.All)
		{
			if (IsNumeric(fromType) && IsNumeric(toType))
			{
				return true;
			}
			if (toType == Int32Type && HasPythonProtocol(fromType, "__int__"))
			{
				return true;
			}
			if (toType == DoubleType && HasPythonProtocol(fromType, "__float__"))
			{
				return true;
			}
			if (toType == BigIntegerType && HasPythonProtocol(fromType, "__long__"))
			{
				return true;
			}
		}
		if (toType.IsGenericType())
		{
			Type genericTypeDefinition2 = toType.GetGenericTypeDefinition();
			if (genericTypeDefinition2 == IListOfTType)
			{
				return IListOfObjectType.IsAssignableFrom(fromType);
			}
			if (genericTypeDefinition2 == NullableOfTType)
			{
				if (fromType == typeof(DynamicNull) || CanConvertFrom(fromType, toType.GetGenericArguments()[0], allowNarrowing))
				{
					return true;
				}
			}
			else if (genericTypeDefinition2 == IDictOfTType)
			{
				return IDictionaryOfObjectType.IsAssignableFrom(fromType);
			}
		}
		if (fromType == BigIntegerType && toType == Int64Type)
		{
			return true;
		}
		if (toType.IsEnum() && fromType == Enum.GetUnderlyingType(toType))
		{
			return true;
		}
		return false;
	}

	private static bool HasImplicitConversion(Type fromType, Type toType)
	{
		if (!HasImplicitConversionWorker(fromType, fromType, toType))
		{
			return HasImplicitConversionWorker(toType, fromType, toType);
		}
		return true;
	}

	private static bool HasImplicitConversionWorker(Type lookupType, Type fromType, Type toType)
	{
		while (lookupType != null)
		{
			MethodInfo[] methods = lookupType.GetMethods();
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.Name == "op_Implicit" && methodInfo.GetParameters()[0].ParameterType.IsAssignableFrom(fromType) && toType.IsAssignableFrom(methodInfo.ReturnType))
				{
					return true;
				}
			}
			lookupType = lookupType.GetBaseType();
		}
		return false;
	}

	public static int? ImplicitConvertToInt32(object o)
	{
		if (o is int)
		{
			return (int)o;
		}
		if (o is BigInteger)
		{
			if (((BigInteger)o).AsInt32(out var ret))
			{
				return ret;
			}
		}
		else
		{
			if (o is Extensible<int>)
			{
				return ConvertToInt32(o);
			}
			if (o is Extensible<BigInteger> && TryConvertToInt32(o, out var result))
			{
				return result;
			}
		}
		if (!(o is double) && !(o is float) && !(o is Extensible<double>) && PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, "__int__", out var value) && value is int)
		{
			return (int)value;
		}
		return null;
	}

	internal static bool IsNumeric(Type t)
	{
		if (t.IsEnum())
		{
			return false;
		}
		switch (t.GetTypeCode())
		{
		case TypeCode.Empty:
		case TypeCode.DBNull:
		case TypeCode.Boolean:
		case TypeCode.Char:
		case TypeCode.DateTime:
		case TypeCode.String:
			return false;
		case TypeCode.Object:
			if (!(t == BigIntegerType))
			{
				return t == ComplexType;
			}
			return true;
		default:
			return true;
		}
	}

	private static bool IsPythonType(Type t)
	{
		return t.FullName.StartsWith("IronPython.");
	}

	private static bool HasPythonProtocol(Type t, string name)
	{
		if (t.FullName.StartsWith("IronPython.NewTypes."))
		{
			return true;
		}
		if (t == typeof(OldInstance))
		{
			return true;
		}
		PythonTypeSlot slot;
		return DynamicHelpers.GetPythonTypeFromType(t)?.TryResolveSlot(DefaultContext.Default, name, out slot) ?? false;
	}
}

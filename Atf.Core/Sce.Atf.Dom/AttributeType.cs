using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Sce.Atf.Dom;

public class AttributeType : NamedMetadata
{
	private static readonly AttributeType s_stringType = new AttributeType("string", typeof(string), 1);

	private static readonly AttributeType s_intType = new AttributeType("int", typeof(int), 1);

	private static readonly AttributeType s_floatType = new AttributeType("float", typeof(float), 1);

	private static readonly AttributeType s_booleanType = new AttributeType("boolean", typeof(bool), 1);

	private readonly Type m_clrType;

	private readonly AttributeTypes m_type;

	private readonly int m_length;

	private List<AttributeRule> m_rules;

	private static readonly object s_defaultBoolean = false;

	private static readonly object s_defaultInt8 = (sbyte)0;

	private static readonly object s_defaultUInt8 = (byte)0;

	private static readonly object s_defaultInt16 = (short)0;

	private static readonly object s_defaultUInt16 = (ushort)0;

	private static readonly object s_defaultInt32 = 0;

	private static readonly object s_defaultUInt32 = 0u;

	private static readonly object s_defaultInt64 = 0L;

	private static readonly object s_defaultUInt64 = 0uL;

	private static readonly object s_defaultSingle = 0f;

	private static readonly object s_defaultDouble = 0.0;

	private static readonly object s_defaultDecimal = 0m;

	private static readonly DateTime s_defaultDateTime = new DateTime(0L);

	private static readonly char[] s_arraySeparator = new char[3] { ' ', '\n', '\t' };

	public Type ClrType => m_clrType;

	public AttributeTypes Type => m_type;

	public int Length => m_length;

	public bool IsArray => m_clrType.IsArray;

	public IEnumerable<AttributeRule> Rules
	{
		get
		{
			if (m_rules != null)
			{
				return m_rules;
			}
			return EmptyEnumerable<AttributeRule>.Instance;
		}
	}

	public static AttributeType StringType => s_stringType;

	public static AttributeType IntType => s_intType;

	public static AttributeType FloatType => s_floatType;

	public static AttributeType BooleanType => s_booleanType;

	public AttributeType(string name, Type type)
		: this(name, type, 1)
	{
	}

	public AttributeType(string name, Type type, int length)
		: base(name)
	{
		m_length = length;
		m_clrType = type;
		Type type2 = type;
		bool isArray = type.IsArray;
		if (isArray)
		{
			type2 = type.GetElementType();
		}
		if (length != 1 && !isArray)
		{
			throw new InvalidOperationException("The length of an AttributeType must be 1 for CLR types that are scalars. AttributeType: " + name);
		}
		if (type2 == typeof(sbyte))
		{
			m_type = (isArray ? AttributeTypes.Int8Array : AttributeTypes.Int8);
			return;
		}
		if (type2 == typeof(byte))
		{
			m_type = (isArray ? AttributeTypes.UInt8Array : AttributeTypes.UInt8);
			return;
		}
		if (type2 == typeof(short))
		{
			m_type = (isArray ? AttributeTypes.Int16Array : AttributeTypes.Int16);
			return;
		}
		if (type2 == typeof(ushort))
		{
			m_type = (isArray ? AttributeTypes.UInt16Array : AttributeTypes.UInt16);
			return;
		}
		if (type2 == typeof(int))
		{
			m_type = (isArray ? AttributeTypes.Int32Array : AttributeTypes.Int32);
			return;
		}
		if (type2 == typeof(uint))
		{
			m_type = (isArray ? AttributeTypes.UInt32Array : AttributeTypes.UInt32);
			return;
		}
		if (type2 == typeof(long))
		{
			m_type = (isArray ? AttributeTypes.Int64Array : AttributeTypes.Int64);
			return;
		}
		if (type2 == typeof(ulong))
		{
			m_type = (isArray ? AttributeTypes.UInt64Array : AttributeTypes.UInt64);
			return;
		}
		if (type2 == typeof(float))
		{
			m_type = (isArray ? AttributeTypes.SingleArray : AttributeTypes.Single);
			return;
		}
		if (type2 == typeof(double))
		{
			m_type = (isArray ? AttributeTypes.DoubleArray : AttributeTypes.Double);
			return;
		}
		if (type2 == typeof(decimal))
		{
			m_type = (isArray ? AttributeTypes.DecimalArray : AttributeTypes.Decimal);
			return;
		}
		if (type2 == typeof(string))
		{
			m_type = (isArray ? AttributeTypes.StringArray : AttributeTypes.String);
			return;
		}
		if (type == typeof(DomNode))
		{
			m_type = AttributeTypes.Reference;
			return;
		}
		if (type2 == typeof(bool))
		{
			m_type = (isArray ? AttributeTypes.BooleanArray : AttributeTypes.Boolean);
			return;
		}
		if (type2 == typeof(Uri))
		{
			m_type = AttributeTypes.Uri;
			return;
		}
		if (type2 == typeof(DateTime))
		{
			m_type = AttributeTypes.DateTime;
			return;
		}
		m_clrType = typeof(string);
		m_type = AttributeTypes.String;
	}

	public void AddRule(AttributeRule rule)
	{
		if (rule == null)
		{
			throw new ArgumentNullException("rule");
		}
		if (m_rules == null)
		{
			m_rules = new List<AttributeRule>();
		}
		m_rules.Add(rule);
	}

	public virtual bool Validate(object value, AttributeInfo info)
	{
		if (value != null && !m_clrType.IsAssignableFrom(value.GetType()))
		{
			return false;
		}
		if (m_rules != null)
		{
			foreach (AttributeRule rule in m_rules)
			{
				if (!rule.Validate(value, info))
				{
					return false;
				}
			}
		}
		return true;
	}

	public object GetDefault()
	{
		object result = null;
		switch (m_type)
		{
		case AttributeTypes.Boolean:
			result = s_defaultBoolean;
			break;
		case AttributeTypes.BooleanArray:
			result = GetDefaultArrayValue<bool>();
			break;
		case AttributeTypes.Int8:
			result = s_defaultInt8;
			break;
		case AttributeTypes.Int8Array:
			result = GetDefaultArrayValue<sbyte>();
			break;
		case AttributeTypes.UInt8:
			result = s_defaultUInt8;
			break;
		case AttributeTypes.UInt8Array:
			result = GetDefaultArrayValue<byte>();
			break;
		case AttributeTypes.Int16:
			result = s_defaultInt16;
			break;
		case AttributeTypes.Int16Array:
			result = GetDefaultArrayValue<short>();
			break;
		case AttributeTypes.UInt16:
			result = s_defaultUInt16;
			break;
		case AttributeTypes.UInt16Array:
			result = GetDefaultArrayValue<ushort>();
			break;
		case AttributeTypes.Int32:
			result = s_defaultInt32;
			break;
		case AttributeTypes.Int32Array:
			result = GetDefaultArrayValue<int>();
			break;
		case AttributeTypes.UInt32:
			result = s_defaultUInt32;
			break;
		case AttributeTypes.UInt32Array:
			result = GetDefaultArrayValue<uint>();
			break;
		case AttributeTypes.Int64:
			result = s_defaultInt64;
			break;
		case AttributeTypes.Int64Array:
			result = GetDefaultArrayValue<long>();
			break;
		case AttributeTypes.UInt64:
			result = s_defaultUInt64;
			break;
		case AttributeTypes.UInt64Array:
			result = GetDefaultArrayValue<ulong>();
			break;
		case AttributeTypes.Single:
			result = s_defaultSingle;
			break;
		case AttributeTypes.SingleArray:
			result = GetDefaultArrayValue<float>();
			break;
		case AttributeTypes.Double:
			result = s_defaultDouble;
			break;
		case AttributeTypes.DoubleArray:
			result = GetDefaultArrayValue<double>();
			break;
		case AttributeTypes.Decimal:
			result = s_defaultDecimal;
			break;
		case AttributeTypes.DecimalArray:
			result = GetDefaultArrayValue<decimal>();
			break;
		case AttributeTypes.String:
			result = string.Empty;
			break;
		case AttributeTypes.StringArray:
			result = GetDefaultArrayValue<string>();
			break;
		case AttributeTypes.DateTime:
			result = s_defaultDateTime;
			break;
		}
		return result;
	}

	public bool AreEqual(object val1, object val2)
	{
		if (val1 == null)
		{
			return val2 == null;
		}
		bool result = false;
		switch (m_type)
		{
		case AttributeTypes.Boolean:
		case AttributeTypes.Int8:
		case AttributeTypes.UInt8:
		case AttributeTypes.Int16:
		case AttributeTypes.UInt16:
		case AttributeTypes.Int32:
		case AttributeTypes.UInt32:
		case AttributeTypes.Int64:
		case AttributeTypes.UInt64:
		case AttributeTypes.Single:
		case AttributeTypes.Double:
		case AttributeTypes.Decimal:
		case AttributeTypes.String:
		case AttributeTypes.DateTime:
		case AttributeTypes.Uri:
		case AttributeTypes.Reference:
			result = val1.Equals(val2);
			break;
		case AttributeTypes.BooleanArray:
			result = AreEqualArraysOf<bool>(val1, val2);
			break;
		case AttributeTypes.Int8Array:
			result = AreEqualArraysOf<sbyte>(val1, val2);
			break;
		case AttributeTypes.UInt8Array:
			result = AreEqualArraysOf<byte>(val1, val2);
			break;
		case AttributeTypes.Int16Array:
			result = AreEqualArraysOf<short>(val1, val2);
			break;
		case AttributeTypes.UInt16Array:
			result = AreEqualArraysOf<ushort>(val1, val2);
			break;
		case AttributeTypes.Int32Array:
			result = AreEqualArraysOf<int>(val1, val2);
			break;
		case AttributeTypes.UInt32Array:
			result = AreEqualArraysOf<uint>(val1, val2);
			break;
		case AttributeTypes.Int64Array:
			result = AreEqualArraysOf<long>(val1, val2);
			break;
		case AttributeTypes.UInt64Array:
			result = AreEqualArraysOf<ulong>(val1, val2);
			break;
		case AttributeTypes.SingleArray:
			result = AreEqualArraysOf<float>(val1, val2);
			break;
		case AttributeTypes.DoubleArray:
			result = AreEqualArraysOf<double>(val1, val2);
			break;
		case AttributeTypes.DecimalArray:
			result = AreEqualArraysOf<decimal>(val1, val2);
			break;
		case AttributeTypes.StringArray:
			result = AreEqualArraysOf<string>(val1, val2);
			break;
		}
		return result;
	}

	public object Clone(object value)
	{
		object result = null;
		if (value != null)
		{
			switch (m_type)
			{
			case AttributeTypes.Boolean:
			case AttributeTypes.Int8:
			case AttributeTypes.UInt8:
			case AttributeTypes.Int16:
			case AttributeTypes.UInt16:
			case AttributeTypes.Int32:
			case AttributeTypes.UInt32:
			case AttributeTypes.Int64:
			case AttributeTypes.UInt64:
			case AttributeTypes.Single:
			case AttributeTypes.Double:
			case AttributeTypes.Decimal:
			case AttributeTypes.String:
			case AttributeTypes.DateTime:
			case AttributeTypes.Uri:
			case AttributeTypes.Reference:
				result = value;
				break;
			case AttributeTypes.BooleanArray:
				result = ((bool[])value).Clone();
				break;
			case AttributeTypes.Int8Array:
				result = ((sbyte[])value).Clone();
				break;
			case AttributeTypes.UInt8Array:
				result = ((byte[])value).Clone();
				break;
			case AttributeTypes.Int16Array:
				result = ((short[])value).Clone();
				break;
			case AttributeTypes.UInt16Array:
				result = ((ushort[])value).Clone();
				break;
			case AttributeTypes.Int32Array:
				result = ((int[])value).Clone();
				break;
			case AttributeTypes.UInt32Array:
				result = ((uint[])value).Clone();
				break;
			case AttributeTypes.Int64Array:
				result = ((long[])value).Clone();
				break;
			case AttributeTypes.UInt64Array:
				result = ((ulong[])value).Clone();
				break;
			case AttributeTypes.SingleArray:
				result = ((float[])value).Clone();
				break;
			case AttributeTypes.DoubleArray:
				result = ((double[])value).Clone();
				break;
			case AttributeTypes.DecimalArray:
				result = ((decimal[])value).Clone();
				break;
			case AttributeTypes.StringArray:
				result = ((string[])value).Clone();
				break;
			}
		}
		return result;
	}

	public virtual string Convert(object value)
	{
		string result = string.Empty;
		switch (m_type)
		{
		case AttributeTypes.Int8:
		case AttributeTypes.UInt8:
		case AttributeTypes.Int16:
		case AttributeTypes.UInt16:
		case AttributeTypes.Int32:
		case AttributeTypes.UInt32:
		case AttributeTypes.Int64:
		case AttributeTypes.UInt64:
		case AttributeTypes.Decimal:
			result = ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture);
			break;
		case AttributeTypes.DateTime:
			result = ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
			break;
		case AttributeTypes.Single:
		case AttributeTypes.Double:
			result = ((IFormattable)value).ToString("R", CultureInfo.InvariantCulture);
			break;
		case AttributeTypes.String:
			result = value.ToString();
			break;
		case AttributeTypes.Uri:
			result = Uri.EscapeUriString(Uri.UnescapeDataString(value.ToString()));
			break;
		case AttributeTypes.Boolean:
			result = (((bool)value) ? "true" : "false");
			break;
		case AttributeTypes.Int8Array:
			result = Convert<sbyte>(value);
			break;
		case AttributeTypes.UInt8Array:
			result = Convert<byte>(value);
			break;
		case AttributeTypes.Int16Array:
			result = Convert<short>(value);
			break;
		case AttributeTypes.UInt16Array:
			result = Convert<ushort>(value);
			break;
		case AttributeTypes.Int32Array:
			result = Convert<int>(value);
			break;
		case AttributeTypes.UInt32Array:
			result = Convert<uint>(value);
			break;
		case AttributeTypes.Int64Array:
			result = Convert<long>(value);
			break;
		case AttributeTypes.UInt64Array:
			result = Convert<ulong>(value);
			break;
		case AttributeTypes.SingleArray:
			result = Convert<float>(value);
			break;
		case AttributeTypes.DoubleArray:
			result = Convert<double>(value);
			break;
		case AttributeTypes.DecimalArray:
			result = Convert<decimal>(value);
			break;
		case AttributeTypes.StringArray:
			result = ConvertStringArrayToString((string[])value);
			break;
		case AttributeTypes.BooleanArray:
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool[] array = value as bool[];
			foreach (bool flag in array)
			{
				stringBuilder.Append(flag ? "true" : "false");
				stringBuilder.Append(" ");
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Length--;
			}
			result = stringBuilder.ToString();
			break;
		}
		}
		return result;
	}

	public virtual object Convert(string s)
	{
		object result = null;
		string[] strings;
		int stringsToParse;
		switch (m_type)
		{
		case AttributeTypes.Boolean:
		{
			if (bool.TryParse(s, out var result6))
			{
				result = result6;
			}
			break;
		}
		case AttributeTypes.Int8:
		{
			if (sbyte.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result11))
			{
				result = result11;
			}
			break;
		}
		case AttributeTypes.UInt8:
		{
			if (byte.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2))
			{
				result = result2;
			}
			break;
		}
		case AttributeTypes.Int16:
		{
			if (short.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result9))
			{
				result = result9;
			}
			break;
		}
		case AttributeTypes.UInt16:
		{
			if (ushort.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result4))
			{
				result = result4;
			}
			break;
		}
		case AttributeTypes.Int32:
		{
			if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result14))
			{
				result = result14;
			}
			break;
		}
		case AttributeTypes.UInt32:
		{
			if (uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result10))
			{
				result = result10;
			}
			break;
		}
		case AttributeTypes.Int64:
		{
			if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result7))
			{
				result = result7;
			}
			break;
		}
		case AttributeTypes.UInt64:
		{
			if (ulong.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result5))
			{
				result = result5;
			}
			break;
		}
		case AttributeTypes.Single:
		{
			if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
			{
				result = result3;
			}
			break;
		}
		case AttributeTypes.Double:
		{
			if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result15))
			{
				result = result15;
			}
			break;
		}
		case AttributeTypes.Decimal:
		{
			if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var result13))
			{
				result = result13;
			}
			break;
		}
		case AttributeTypes.String:
			result = s;
			break;
		case AttributeTypes.Uri:
		{
			Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var result12);
			result = result12;
			break;
		}
		case AttributeTypes.DateTime:
		{
			DateTime result8;
			if (s.EndsWith("Z"))
			{
				result = XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.Local);
			}
			else if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out result8))
			{
				result = result8;
			}
			break;
		}
		case AttributeTypes.BooleanArray:
		{
			bool[] array7 = CreateArrayValue<bool>(s, out strings, out stringsToParse);
			for (int num = 0; num < stringsToParse; num++)
			{
				bool.TryParse(strings[num], out array7[num]);
			}
			result = array7;
			break;
		}
		case AttributeTypes.Int8Array:
		{
			sbyte[] array4 = CreateArrayValue<sbyte>(s, out strings, out stringsToParse);
			for (int l = 0; l < stringsToParse; l++)
			{
				sbyte.TryParse(strings[l], NumberStyles.Integer, CultureInfo.InvariantCulture, out array4[l]);
			}
			result = array4;
			break;
		}
		case AttributeTypes.UInt8Array:
		{
			byte[] array2 = CreateArrayValue<byte>(s, out strings, out stringsToParse);
			for (int j = 0; j < stringsToParse; j++)
			{
				byte.TryParse(strings[j], NumberStyles.Integer, CultureInfo.InvariantCulture, out array2[j]);
			}
			result = array2;
			break;
		}
		case AttributeTypes.Int16Array:
		{
			short[] array12 = CreateArrayValue<short>(s, out strings, out stringsToParse);
			for (int num6 = 0; num6 < stringsToParse; num6++)
			{
				short.TryParse(strings[num6], NumberStyles.Integer, CultureInfo.InvariantCulture, out array12[num6]);
			}
			result = array12;
			break;
		}
		case AttributeTypes.UInt16Array:
		{
			ushort[] array11 = CreateArrayValue<ushort>(s, out strings, out stringsToParse);
			for (int num5 = 0; num5 < stringsToParse; num5++)
			{
				ushort.TryParse(strings[num5], NumberStyles.Integer, CultureInfo.InvariantCulture, out array11[num5]);
			}
			result = array11;
			break;
		}
		case AttributeTypes.Int32Array:
		{
			int[] array10 = CreateArrayValue<int>(s, out strings, out stringsToParse);
			for (int num4 = 0; num4 < stringsToParse; num4++)
			{
				int.TryParse(strings[num4], NumberStyles.Integer, CultureInfo.InvariantCulture, out array10[num4]);
			}
			result = array10;
			break;
		}
		case AttributeTypes.UInt32Array:
		{
			uint[] array9 = CreateArrayValue<uint>(s, out strings, out stringsToParse);
			for (int num3 = 0; num3 < stringsToParse; num3++)
			{
				uint.TryParse(strings[num3], NumberStyles.Integer, CultureInfo.InvariantCulture, out array9[num3]);
			}
			result = array9;
			break;
		}
		case AttributeTypes.Int64Array:
		{
			long[] array8 = CreateArrayValue<long>(s, out strings, out stringsToParse);
			for (int num2 = 0; num2 < stringsToParse; num2++)
			{
				long.TryParse(strings[num2], NumberStyles.Integer, CultureInfo.InvariantCulture, out array8[num2]);
			}
			result = array8;
			break;
		}
		case AttributeTypes.UInt64Array:
		{
			ulong[] array6 = CreateArrayValue<ulong>(s, out strings, out stringsToParse);
			for (int n = 0; n < stringsToParse; n++)
			{
				ulong.TryParse(strings[n], NumberStyles.Integer, CultureInfo.InvariantCulture, out array6[n]);
			}
			result = array6;
			break;
		}
		case AttributeTypes.SingleArray:
		{
			float[] array5 = CreateArrayValue<float>(s, out strings, out stringsToParse);
			for (int m = 0; m < stringsToParse; m++)
			{
				float.TryParse(strings[m], NumberStyles.Float, CultureInfo.InvariantCulture, out array5[m]);
			}
			result = array5;
			break;
		}
		case AttributeTypes.DoubleArray:
		{
			double[] array3 = CreateArrayValue<double>(s, out strings, out stringsToParse);
			for (int k = 0; k < stringsToParse; k++)
			{
				double.TryParse(strings[k], NumberStyles.Float, CultureInfo.InvariantCulture, out array3[k]);
			}
			result = array3;
			break;
		}
		case AttributeTypes.DecimalArray:
		{
			decimal[] array = CreateArrayValue<decimal>(s, out strings, out stringsToParse);
			for (int i = 0; i < stringsToParse; i++)
			{
				decimal.TryParse(strings[i], NumberStyles.Number, CultureInfo.InvariantCulture, out array[i]);
			}
			result = array;
			break;
		}
		case AttributeTypes.StringArray:
			result = ConvertStringToStringArray(s);
			break;
		}
		return result;
	}

	private object GetDefaultArrayValue<T>()
	{
		int num = 0;
		if (m_length < int.MaxValue)
		{
			num = m_length;
		}
		return new T[num];
	}

	private T[] CreateArrayValue<T>(string s, out string[] strings, out int stringsToParse)
	{
		string[] array = s.Split(s_arraySeparator, StringSplitOptions.RemoveEmptyEntries);
		int num = ((m_length < int.MaxValue) ? m_length : array.Length);
		strings = array;
		stringsToParse = Math.Min(num, array.Length);
		return new T[num];
	}

	private string Convert<T>(object array) where T : IFormattable
	{
		IEnumerable<T> enumerable = array as IEnumerable<T>;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (T item in enumerable)
		{
			if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				stringBuilder.Append(item.ToString("R", invariantCulture));
			}
			else
			{
				CultureInfo invariantCulture2 = CultureInfo.InvariantCulture;
				stringBuilder.Append(item.ToString(null, invariantCulture2));
			}
			stringBuilder.Append(" ");
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Length--;
		}
		return stringBuilder.ToString();
	}

	private static bool AreEqualArraysOf<T>(object val1, object val2)
	{
		T[] array = val1 as T[];
		if (!(val2 is T[] array2) || array.Length != array2.Length)
		{
			return false;
		}
		for (int i = 0; i < array.Length; i++)
		{
			ref readonly T reference = ref array[i];
			object obj = array2[i];
			if (!reference.Equals(obj))
			{
				return false;
			}
		}
		return true;
	}

	private static string ConvertStringArrayToString(string[] array)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			text = ((!string.IsNullOrEmpty(text)) ? text.Replace("\"", "\\\"") : "\"\"");
			if (text.Contains(" "))
			{
				stringBuilder.Append('"');
				stringBuilder.Append(text);
				stringBuilder.Append("\" ");
			}
			else
			{
				stringBuilder.Append(text);
				stringBuilder.Append(' ');
			}
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Length--;
		}
		return stringBuilder.ToString();
	}

	private static string[] ConvertStringToStringArray(string concatenation)
	{
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < concatenation.Length; i++)
		{
			if (concatenation[i] == '"')
			{
				i++;
				char result;
				while (GetUnescapedChar(concatenation, ref i, '"', out result))
				{
					stringBuilder.Append(result);
				}
				i++;
			}
			else
			{
				char result2;
				while (GetUnescapedChar(concatenation, ref i, ' ', out result2))
				{
					stringBuilder.Append(result2);
				}
			}
			string text = stringBuilder.ToString();
			list.Add(text);
			stringBuilder.Remove(0, text.Length);
		}
		return list.ToArray();
	}

	private static bool GetUnescapedChar(string s, ref int index, char terminator, out char result)
	{
		if (index >= s.Length || s[index] == terminator)
		{
			result = terminator;
			return false;
		}
		if (s[index] == '\\' && index + 1 < s.Length && s[index + 1] == '"')
		{
			result = '"';
			index += 2;
			return true;
		}
		result = s[index++];
		return true;
	}
}

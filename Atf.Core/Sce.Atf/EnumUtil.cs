using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sce.Atf;

public static class EnumUtil
{
	private class EnumData
	{
		public readonly Type EnumType;

		public readonly Dictionary<object, string> DisplayStrings = new Dictionary<object, string>();

		public EnumData(Type enumType)
		{
			EnumType = enumType;
		}
	}

	private static Dictionary<Type, EnumData> s_cache = new Dictionary<Type, EnumData>();

	public static bool TryParse<T>(string value, out T result)
	{
		if (!typeof(T).IsEnum)
		{
			throw new InvalidOperationException("can only be used on Enums");
		}
		result = default(T);
		if (int.TryParse(value, out var result2) && Enum.IsDefined(typeof(T), result2))
		{
			result = (T)(object)result2;
			return true;
		}
		return false;
	}

	public static void ParseEnumDefinitions(string[] enumDefinitions, out string[] names, out string[] displayNames, out int[] values)
	{
		names = new string[enumDefinitions.Length];
		displayNames = new string[enumDefinitions.Length];
		values = new int[enumDefinitions.Length];
		int value = 0;
		for (int i = 0; i < enumDefinitions.Length; i++)
		{
			names[i] = ParseDefinition(enumDefinitions[i], out var displayName, ref value);
			displayNames[i] = displayName;
			values[i] = value;
			value++;
		}
	}

	public static void ParseEnumDefinitions(string[] enumDefinitions, out string[] names, out int[] values)
	{
		ParseEnumDefinitions(enumDefinitions, out names, out var _, out values);
	}

	public static void ParseFlagDefinitions(string[] flagDefinitions, out string[] names, out string[] displayNames, out int[] values)
	{
		names = new string[flagDefinitions.Length];
		displayNames = new string[flagDefinitions.Length];
		values = new int[flagDefinitions.Length];
		int value = 1;
		for (int i = 0; i < flagDefinitions.Length; i++)
		{
			names[i] = ParseDefinition(flagDefinitions[i], out var displayName, ref value);
			displayNames[i] = displayName;
			values[i] = value;
			value *= 2;
		}
	}

	public static void ParseFlagDefinitions(string[] flagDefinitions, out string[] names, out int[] values)
	{
		ParseFlagDefinitions(flagDefinitions, out names, out var _, out values);
	}

	public static string GetDisplayString(Type enumType, object value)
	{
		Requires.NotNull(enumType, "enumType");
		Requires.NotNull(value, "value");
		Requires.Require<ArgumentException>(enumType.IsEnum, "enumType must by an enum type");
		return GetEnumData(enumType).DisplayStrings[value];
	}

	public static string GetDisplayString<T>(object value)
	{
		Requires.NotNull(value, "value");
		Requires.Require<ArgumentException>(typeof(T).IsEnum, "enumType must by an enum type");
		return GetEnumData(typeof(T)).DisplayStrings[value];
	}

	private static string ParseDefinition(string definition, out string displayName, ref int value)
	{
		string[] array = definition.Split('=');
		string text = array[0];
		if (array.Length == 1)
		{
			displayName = text;
		}
		else if (array.Length == 2)
		{
			displayName = text;
			value = int.Parse(array[1]);
		}
		else if (array.Length == 3)
		{
			displayName = array[2];
		}
		else
		{
			if (array.Length != 4)
			{
				throw new FormatException($"This enum or flag definition is bad:{definition}");
			}
			displayName = array[2];
			value = int.Parse(array[3]);
		}
		return text;
	}

	private static EnumData GetEnumData(Type enumType)
	{
		if (!s_cache.TryGetValue(enumType, out var value))
		{
			value = new EnumData(enumType);
			FieldInfo[] fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				string text = null;
				object value2 = fieldInfo.GetValue(null);
				object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayStringAttribute), inherit: false);
				if (customAttributes != null && customAttributes.Length != 0)
				{
					text = ((DisplayStringAttribute)customAttributes[0]).Value;
				}
				if (text == null)
				{
					text = fieldInfo.Name;
				}
				if (!value.DisplayStrings.ContainsKey(value2))
				{
					value.DisplayStrings.Add(value2, text);
				}
			}
			lock (s_cache)
			{
				if (!s_cache.ContainsKey(enumType))
				{
					s_cache.Add(enumType, value);
				}
			}
		}
		return value;
	}
}

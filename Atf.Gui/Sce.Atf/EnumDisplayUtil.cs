using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sce.Atf;

public static class EnumDisplayUtil
{
	private class EnumData
	{
		public readonly Type EnumType;

		public readonly Dictionary<object, string> DisplayStrings = new Dictionary<object, string>();

		public readonly Dictionary<string, object> EnumValues = new Dictionary<string, object>();

		public EnumData(Type enumType)
		{
			EnumType = enumType;
		}
	}

	private static Dictionary<Type, EnumData> s_cache = new Dictionary<Type, EnumData>();

	public static string GetDisplayString(Type enumType, object value)
	{
		Requires.NotNull(enumType, "enumType");
		Requires.NotNull(value, "value");
		Requires.Require<ArgumentException>(enumType.IsEnum, "enumType must by an enum type");
		return GetEnumData(enumType).DisplayStrings[value];
	}

	public static string GetDisplayString<TEnum>(object value)
	{
		Requires.NotNull(value, "value");
		Requires.Require<ArgumentException>(typeof(TEnum).IsEnum, "enumType must by an enum type");
		return GetEnumData(typeof(TEnum)).DisplayStrings[value];
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
				value.DisplayStrings.Add(value2, text);
				value.EnumValues.Add(text, value2);
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Firaxis.Utility;

public static class Transpose
{
	public delegate object SafeFromStringHandler(string value, object defaultValue);

	private static Dictionary<Type, SafeFromStringHandler> safeHandlers;

	public static Dictionary<Type, SafeFromStringHandler> SafeFromStringHandlers => safeHandlers;

	static Transpose()
	{
		safeHandlers = new Dictionary<Type, SafeFromStringHandler>();
		safeHandlers.Add(typeof(string), FromStringString);
		safeHandlers.Add(typeof(bool), FromStringBool);
		safeHandlers.Add(typeof(int), FromStringInt);
		safeHandlers.Add(typeof(float), FromStringFloat);
	}

	public static string ToString(object obj, Type t)
	{
		if (obj is Array)
		{
			string text = null;
			foreach (object item in (IEnumerable)obj)
			{
				text = ((text == null) ? ToString(item) : (text + ", " + ToString(item)));
			}
			return text;
		}
		return TypeDescriptor.GetConverter(t).ConvertToString(obj);
	}

	public static string ToString<T>(T t)
	{
		return ToString(t, typeof(T));
	}

	public static object FromString(string value, string type)
	{
		return FromString(value, Type.GetType(type));
	}

	public static object FromString(string value, Type t)
	{
		if (!safeHandlers.ContainsKey(t))
		{
			if (typeof(Array).IsAssignableFrom(t))
			{
				string[] array = value.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				Array array2 = Array.CreateInstance(t.GetElementType(), array.Length);
				for (int i = 0; i < array.Length; i++)
				{
					array2.SetValue(FromString(array[i], t.GetElementType()), i);
				}
				return array2;
			}
			return TypeDescriptor.GetConverter(t).ConvertFromString(value);
		}
		return safeHandlers[t](value, t.IsValueType ? ((object)0) : null);
	}

	public static T FromString<T>(string value, T defaultValue)
	{
		Type typeFromHandle = typeof(T);
		if (!safeHandlers.ContainsKey(typeFromHandle))
		{
			return (T)TypeDescriptor.GetConverter(typeFromHandle).ConvertFromString(value);
		}
		return (T)safeHandlers[typeFromHandle](value, defaultValue);
	}

	public static T FromString<T>(string value)
	{
		return FromString(value, default(T));
	}

	private static object FromStringString(string value, object defaultValue)
	{
		return value;
	}

	private static object FromStringBool(string value, object defaultValue)
	{
		if (bool.TryParse(value, out var result))
		{
			return result;
		}
		if (int.TryParse(value, out var result2))
		{
			return result2 != 0;
		}
		return defaultValue;
	}

	private static object FromStringInt(string value, object defaultValue)
	{
		if (int.TryParse(value, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	private static object FromStringFloat(string value, object defaultValue)
	{
		if (float.TryParse(value, out var result))
		{
			return result;
		}
		return defaultValue;
	}
}

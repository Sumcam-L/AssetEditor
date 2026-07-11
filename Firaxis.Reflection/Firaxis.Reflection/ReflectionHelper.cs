using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Windows.Forms;
using Firaxis.Error;

namespace Firaxis.Reflection;

public static class ReflectionHelper
{
	public delegate bool PropertyFilter(PropertyInfo p);

	public static bool Is64Bit => IntPtr.Size == 8;

	public static T CreateInstance<T>()
	{
		return Activator.CreateInstance<T>();
	}

	public static object CreateInstance(Type type)
	{
		return Activator.CreateInstance(type);
	}

	public static object CreateInstance(string assembly, string name)
	{
		ObjectHandle objectHandle = Activator.CreateInstance(assembly, name);
		return objectHandle.Unwrap();
	}

	public static T GetAttribute<T>(Type type)
	{
		object[] customAttributes = type.GetCustomAttributes(typeof(T), inherit: true);
		if (customAttributes != null && customAttributes.Length != 0)
		{
			return (T)customAttributes[0];
		}
		return default(T);
	}

	public static T GetAttribute<T>(object obj)
	{
		Type type = obj.GetType();
		if (type.IsEnum)
		{
			FieldInfo field = type.GetField(obj.ToString());
			object[] customAttributes = field.GetCustomAttributes(typeof(T), inherit: true);
			if (customAttributes != null && customAttributes.GetLength(0) > 0)
			{
				return (T)customAttributes[0];
			}
		}
		return GetAttribute<T>(obj.GetType());
	}

	public static string GetDisplayName(Type type)
	{
		DisplayNameAttribute attribute = GetAttribute<DisplayNameAttribute>(type);
		return (attribute != null) ? attribute.DisplayName : type.Name;
	}

	public static string GetDisplayName(object obj)
	{
		DisplayNameAttribute attribute = GetAttribute<DisplayNameAttribute>(obj);
		if (attribute != null)
		{
			return attribute.DisplayName;
		}
		Type type = obj.GetType();
		if (!type.IsEnum)
		{
			return type.Name;
		}
		string text = obj.ToString();
		FieldInfo field = type.GetField(text);
		object[] customAttributes = field.GetCustomAttributes(typeof(DisplayAttribute), inherit: false);
		return (customAttributes == null || customAttributes.Length == 0) ? text : ((DisplayAttribute)customAttributes[0]).Name;
	}

	public static string GetDescription(Type type)
	{
		DescriptionAttribute attribute = GetAttribute<DescriptionAttribute>(type);
		return (attribute != null) ? attribute.Description : string.Empty;
	}

	public static string GetDescription(object obj)
	{
		DescriptionAttribute attribute = GetAttribute<DescriptionAttribute>(obj);
		return (attribute != null) ? attribute.Description : string.Empty;
	}

	public static int GetStride(Type type)
	{
		return GetAttribute<StrideAttribute>(type)?.Size ?? 0;
	}

	public static int GetStride(object obj)
	{
		return GetAttribute<StrideAttribute>(obj)?.Size ?? 0;
	}

	public static bool IsType<T>(Type type)
	{
		return typeof(T).IsAssignableFrom(type);
	}

	public static bool IsFloatType(Type type)
	{
		return type == typeof(float) || type == typeof(double);
	}

	public static bool IsIntegralType(Type type)
	{
		return type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) || type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);
	}

	public static bool ValidEnumValue<T>(int value)
	{
		if (!typeof(T).IsEnum)
		{
			throw new InvalidCastException("Type is not an enum");
		}
		Array values = Enum.GetValues(typeof(T));
		foreach (int item in values)
		{
			if (item == value)
			{
				return true;
			}
		}
		return false;
	}

	public static T EnumType<T>(string name)
	{
		if (int.TryParse(name, out var result))
		{
			name = Enum.GetName(typeof(T), result);
		}
		return (T)Enum.Parse(typeof(T), name);
	}

	public static Color GetColor(Type type)
	{
		return GetAttribute<ColorProviderAttribute>(type)?.Color ?? Color.White;
	}

	public static Color GetColor(object obj)
	{
		if (!(obj is IColorProvider { Color: var color }))
		{
			return GetAttribute<ColorProviderAttribute>(obj)?.Color ?? Color.White;
		}
		return color;
	}

	public static Assembly ProxyAssembly(string baseAssemblyName)
	{
		return ProxyAssembly(baseAssemblyName, bLoadDependenciesFromExePath: true);
	}

	public static Assembly ProxyAssembly(string baseAssemblyName, bool bLoadDependenciesFromExePath)
	{
		return ProxyAssembly(baseAssemblyName, bLoadDependenciesFromExePath, "Win32", "x64", ".dll");
	}

	public static Assembly ProxyAssembly(string baseAssemblyName, bool bLoadDependenciesFromExePath, string x32, string x64, string ext)
	{
		string text = baseAssemblyName + (Is64Bit ? x64 : x32);
		if (!Path.IsPathRooted(text))
		{
			string directoryName = Path.GetDirectoryName(Application.ExecutablePath);
			text = Path.Combine(directoryName, text);
		}
		text += ext;
		if (bLoadDependenciesFromExePath)
		{
			return Assembly.LoadFile(text);
		}
		return Assembly.LoadFrom(text);
	}

	public static T TypeLoader<T>(Assembly assembly)
	{
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			if (!type.IsAbstract && type.IsClass && typeof(T).IsAssignableFrom(type))
			{
				return (T)Activator.CreateInstance(type);
			}
		}
		throw new IOException($"Module '{assembly.FullName}' does not contain an {typeof(T).Name} object");
	}

	public static T TypeLoader<T>(Assembly assembly, params object[] args)
	{
		try
		{
			Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (!type.IsAbstract && type.IsClass && typeof(T).IsAssignableFrom(type))
                {
                    return (T)Activator.CreateInstance(type, args);
                }
            }
            throw new IOException($"Module '{assembly.FullName}' does not contain an {typeof(T).Name} object");

        }
		catch (ReflectionTypeLoadException ex)
		{
            Console.WriteLine($"º”‘ÿ≥Ã–ÚºØ ß∞‹: {assembly.FullName}");
            foreach (Exception loaderEx in ex.LoaderExceptions)
            {
                Console.WriteLine($"  - {loaderEx.Message}");
            }
            throw;
        }
            
	}

	public static IEnumerable<PropertyInfo> CollectProperties(object obj)
	{
		return CollectProperties(obj, PropertyFilterOnlyPubliclyModifiable);
	}

	private static bool PropertyFilterOnlyPubliclyModifiable(PropertyInfo p)
	{
		MethodInfo[] accessors = p.GetAccessors(nonPublic: false);
		return p.CanRead && p.CanWrite && accessors != null && accessors.Length == 2;
	}

	public static IEnumerable<PropertyInfo> CollectProperties(object obj, PropertyFilter pfnFilterFunction)
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		Type t = obj.GetType();
		if (!t.IsClass)
		{
			throw new ArgumentException("Object must be a class");
		}
		MemberInfo[] members = t.GetMembers();
		MemberInfo[] array = members;
		foreach (MemberInfo m in array)
		{
			PropertyInfo p = m as PropertyInfo;
			if (!(p != null))
			{
				continue;
			}
			object[] attrib = p.GetCustomAttributes(typeof(NoSerializeAttribute), inherit: true);
			if (attrib == null || attrib.Length == 0)
			{
				attrib = p.GetCustomAttributes(typeof(NonSerializedAttribute), inherit: true);
				if ((attrib == null || attrib.Length == 0) && (pfnFilterFunction?.Invoke(p) ?? true))
				{
					yield return p;
				}
			}
		}
	}

	public static ResultCode SetMemberByExpression(object Base, string Expression, object value)
	{
		string[] array = Expression.Split('.');
		for (int i = 0; i < array.Length - 1; i++)
		{
			Type type = Base.GetType();
			PropertyInfo property = type.GetProperty(array[i]);
			if (property != null && !property.PropertyType.IsPrimitive)
			{
				object value2 = property.GetValue(Base);
				if (value2 != null)
				{
					Base = value2;
					continue;
				}
			}
			FieldInfo field = type.GetField(array[i]);
			if (field != null && !field.FieldType.IsPrimitive)
			{
				object value3 = field.GetValue(Base);
				if (value3 != null)
				{
					Base = value3;
					continue;
				}
			}
			return new ResultCode($"Failed to find member with name {array[i]}");
		}
		Type type2 = Base.GetType();
		PropertyInfo property2 = type2.GetProperty(array[array.Length - 1]);
		if (property2 != null)
		{
			property2.SetValue(Base, value);
			return ResultCode.Success;
		}
		FieldInfo field2 = type2.GetField(array[array.Length - 1]);
		if (field2 != null)
		{
			field2.SetValue(Base, value);
			return ResultCode.Success;
		}
		return new ResultCode($"Failed to find member with name {array[array.Length - 1]}");
	}

	public static object GetMemberByExpression(object Base, string Expression)
	{
		string[] array = Expression.Split('.');
		for (int i = 0; i < array.Length - 1; i++)
		{
			Type type = Base.GetType();
			PropertyInfo property = type.GetProperty(array[i]);
			if (property != null && !property.PropertyType.IsPrimitive)
			{
				object value = property.GetValue(Base);
				if (value != null)
				{
					Base = value;
					continue;
				}
			}
			FieldInfo field = type.GetField(array[i]);
			if (field != null && !field.FieldType.IsPrimitive)
			{
				object value2 = field.GetValue(Base);
				if (value2 != null)
				{
					Base = value2;
					continue;
				}
			}
			return null;
		}
		Type type2 = Base.GetType();
		PropertyInfo property2 = type2.GetProperty(array[array.Length - 1]);
		if (property2 != null)
		{
			return property2.GetValue(Base);
		}
		FieldInfo field2 = type2.GetField(array[array.Length - 1]);
		if (field2 != null)
		{
			return field2.GetValue(Base);
		}
		return null;
	}
}

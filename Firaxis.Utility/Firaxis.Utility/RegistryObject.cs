using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Firaxis.Reflection;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace Firaxis.Utility;

public static class RegistryObject
{
	private enum RegWow64Options
	{
		None = 0,
		KEY_WOW64_64KEY = 0x100,
		KEY_WOW64_32KEY = 0x200
	}

	private enum RegistryRights
	{
		ReadKey = 131097,
		WriteKey = 131078
	}

	public class Ignore : Attribute
	{
	}

	public static void Save(object obj, RegistryKey regParentKey, string regKeyName)
	{
		RegistryKey registryKey = null;
		List<PropertyInfo> list = new List<PropertyInfo>(ReflectionHelper.CollectProperties(obj));
		try
		{
			registryKey = regParentKey.CreateSubKey(regKeyName);
			if (registryKey == null)
			{
				return;
			}
			foreach (PropertyInfo item in list)
			{
				Ignore attribute = ReflectionHelper.GetAttribute<Ignore>(item);
				if (attribute == null)
				{
					registryKey.SetValue(item.Name, Transpose.ToString(item.GetValue(obj, null), item.PropertyType));
				}
			}
		}
		finally
		{
			registryKey?.Close();
		}
	}

	public static void Load(object obj, RegistryKey regParentKey, string regKeyName)
	{
		RegistryKey registryKey = null;
		List<PropertyInfo> list = new List<PropertyInfo>(ReflectionHelper.CollectProperties(obj));
		try
		{
			registryKey = regParentKey.OpenSubKey(regKeyName, writable: false);
			if (registryKey == null)
			{
				return;
			}
			foreach (PropertyInfo item in list)
			{
				object value = registryKey.GetValue(item.Name, null);
				if (value != null)
				{
					Ignore attribute = ReflectionHelper.GetAttribute<Ignore>(item);
					if (attribute == null)
					{
						item.SetValue(obj, Transpose.FromString(value.ToString(), item.PropertyType), null);
					}
				}
			}
		}
		finally
		{
			registryKey?.Close();
		}
	}

	public static void Load32Bit(object obj, RegistryKey regParentKey, string regKeyName)
	{
		RegistryKey registryKey = null;
		List<PropertyInfo> list = new List<PropertyInfo>(ReflectionHelper.CollectProperties(obj));
		try
		{
			registryKey = OpenSubKey(regParentKey, regKeyName, writable: false, RegWow64Options.KEY_WOW64_32KEY);
			if (registryKey == null)
			{
				return;
			}
			foreach (PropertyInfo item in list)
			{
				object value = registryKey.GetValue(item.Name, null);
				if (value != null)
				{
					Ignore attribute = ReflectionHelper.GetAttribute<Ignore>(item);
					if (attribute == null)
					{
						item.SetValue(obj, Transpose.FromString(value.ToString(), item.PropertyType), null);
					}
				}
			}
		}
		finally
		{
			registryKey?.Close();
		}
	}

	public static void Load64Bit(object obj, RegistryKey regParentKey, string regKeyName)
	{
		RegistryKey registryKey = null;
		List<PropertyInfo> list = new List<PropertyInfo>(ReflectionHelper.CollectProperties(obj));
		try
		{
			registryKey = OpenSubKey(regParentKey, regKeyName, writable: false, RegWow64Options.KEY_WOW64_64KEY);
			if (registryKey == null)
			{
				return;
			}
			foreach (PropertyInfo item in list)
			{
				object value = registryKey.GetValue(item.Name, null);
				if (value != null)
				{
					Ignore attribute = ReflectionHelper.GetAttribute<Ignore>(item);
					if (attribute == null)
					{
						item.SetValue(obj, Transpose.FromString(value.ToString(), item.PropertyType), null);
					}
				}
			}
		}
		finally
		{
			registryKey?.Close();
		}
	}

	private static RegistryKey OpenSubKey(RegistryKey parentKey, string subKeyName, bool writable, RegWow64Options options)
	{
		if (parentKey == null || GetRegistryKeyHandle(parentKey) == IntPtr.Zero)
		{
			return null;
		}
		int num = 131097;
		if (writable)
		{
			num = 131078;
		}
		if (RegOpenKeyEx(GetRegistryKeyHandle(parentKey), subKeyName, 0, num | (int)options, out var phkResult) != 0)
		{
			return null;
		}
		return PointerToRegistryKey((IntPtr)phkResult, writable, ownsHandle: false);
	}

	private static IntPtr GetRegistryKeyHandle(RegistryKey registryKey)
	{
		Type typeFromHandle = typeof(RegistryKey);
		FieldInfo field = typeFromHandle.GetField("hkey", BindingFlags.Instance | BindingFlags.NonPublic);
		SafeHandle safeHandle = (SafeHandle)field.GetValue(registryKey);
		return safeHandle.DangerousGetHandle();
	}

	private static RegistryKey PointerToRegistryKey(IntPtr hKey, bool writable, bool ownsHandle)
	{
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
		Type type = typeof(SafeHandleZeroOrMinusOneIsInvalid).Assembly.GetType("Microsoft.Win32.SafeHandles.SafeRegistryHandle");
		Type[] types = new Type[2]
		{
			typeof(IntPtr),
			typeof(bool)
		};
		ConstructorInfo constructor = type.GetConstructor(bindingAttr, null, types, null);
		object obj = constructor.Invoke(new object[2] { hKey, ownsHandle });
		Type typeFromHandle = typeof(RegistryKey);
		Type[] types2 = new Type[2]
		{
			type,
			typeof(bool)
		};
		ConstructorInfo constructor2 = typeFromHandle.GetConstructor(bindingAttr, null, types2, null);
		return (RegistryKey)constructor2.Invoke(new object[2] { obj, writable });
	}

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int RegOpenKeyEx(IntPtr hKey, string subKey, int ulOptions, int samDesired, out int phkResult);
}

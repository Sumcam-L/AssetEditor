using System;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace IronPython.Modules;

public static class PythonWinReg
{
	[PythonType]
	public class HKEYType : IDisposable
	{
		private RegistryKey key;

		public int handle
		{
			get
			{
				lock (this)
				{
					if (key == null)
					{
						return 0;
					}
					return key.GetHashCode();
				}
			}
		}

		internal HKEYType(RegistryKey key)
		{
			this.key = key;
			HKeyHandleCache.cache[key.GetHashCode()] = new WeakReference(this);
		}

		public void Close()
		{
			lock (this)
			{
				if (key != null)
				{
					HKeyHandleCache.cache.Remove(key.GetHashCode());
					key.Close();
					key = null;
				}
			}
		}

		public int Detach()
		{
			return 0;
		}

		public static implicit operator int(HKEYType hKey)
		{
			return hKey.handle;
		}

		[PythonHidden]
		public RegistryKey GetKey()
		{
			lock (this)
			{
				if (key == null)
				{
					throw PythonExceptions.CreateThrowable(PythonExceptions.EnvironmentError, "key has been closed");
				}
				return key;
			}
		}

		void IDisposable.Dispose()
		{
			Close();
		}
	}

	public const string __doc__ = "Provides access to the Windows registry.";

	public const int KEY_QUERY_VALUE = 1;

	public const int KEY_SET_VALUE = 2;

	public const int KEY_CREATE_SUB_KEY = 4;

	public const int KEY_ENUMERATE_SUB_KEYS = 8;

	public const int KEY_NOTIFY = 16;

	public const int KEY_CREATE_LINK = 32;

	public const int KEY_ALL_ACCESS = 983103;

	public const int KEY_EXECUTE = 131097;

	public const int KEY_READ = 131097;

	public const int KEY_WRITE = 131078;

	public const int REG_CREATED_NEW_KEY = 1;

	public const int REG_OPENED_EXISTING_KEY = 2;

	public const int REG_NONE = 0;

	public const int REG_SZ = 1;

	public const int REG_EXPAND_SZ = 2;

	public const int REG_BINARY = 3;

	public const int REG_DWORD = 4;

	public const int REG_DWORD_LITTLE_ENDIAN = 4;

	public const int REG_DWORD_BIG_ENDIAN = 5;

	public const int REG_LINK = 6;

	public const int REG_MULTI_SZ = 7;

	public const int REG_RESOURCE_LIST = 8;

	public const int REG_FULL_RESOURCE_DESCRIPTOR = 9;

	public const int REG_RESOURCE_REQUIREMENTS_LIST = 10;

	public const int REG_NOTIFY_CHANGE_NAME = 1;

	public const int REG_NOTIFY_CHANGE_ATTRIBUTES = 2;

	public const int REG_NOTIFY_CHANGE_LAST_SET = 4;

	public const int REG_NOTIFY_CHANGE_SECURITY = 8;

	public const int REG_OPTION_RESERVED = 0;

	public const int REG_OPTION_NON_VOLATILE = 0;

	public const int REG_OPTION_VOLATILE = 1;

	public const int REG_OPTION_CREATE_LINK = 2;

	public const int REG_OPTION_BACKUP_RESTORE = 4;

	public const int REG_OPTION_OPEN_LINK = 8;

	public const int REG_NO_LAZY_FLUSH = 4;

	public const int REG_REFRESH_HIVE = 2;

	public const int REG_LEGAL_CHANGE_FILTER = 15;

	public const int REG_LEGAL_OPTION = 15;

	public const int REG_WHOLE_HIVE_VOLATILE = 1;

	private const int ERROR_MORE_DATA = 234;

	private const int ERROR_SUCCESS = 0;

	public static PythonType error = PythonExceptions.WindowsError;

	public static BigInteger HKEY_CLASSES_ROOT = 2147483648L;

	public static BigInteger HKEY_CURRENT_USER = 2147483649L;

	public static BigInteger HKEY_LOCAL_MACHINE = 2147483650L;

	public static BigInteger HKEY_USERS = 2147483651L;

	public static BigInteger HKEY_PERFORMANCE_DATA = 2147483652L;

	public static BigInteger HKEY_CURRENT_CONFIG = 2147483653L;

	public static BigInteger HKEY_DYN_DATA = 2147483654L;

	public static void CloseKey(HKEYType key)
	{
		key.Close();
	}

	public static HKEYType CreateKey(object key, string subKeyName)
	{
		HKEYType rootKey = GetRootKey(key);
		if (key is BigInteger && string.IsNullOrEmpty(subKeyName))
		{
			return rootKey;
		}
		return new HKEYType(rootKey.GetKey().CreateSubKey(subKeyName));
	}

	public static HKEYType CreateKeyEx(object key, string subKeyName, int res, int sam)
	{
		HKEYType rootKey = GetRootKey(key);
		if (key is BigInteger && string.IsNullOrEmpty(subKeyName))
		{
			return rootKey;
		}
		SafeRegistryHandle phkResult;
		int lpdwDisposition;
		int num = RegCreateKeyEx(rootKey.GetKey().Handle, subKeyName, 0, null, RegistryOptions.None, (RegistryRights)sam, IntPtr.Zero, out phkResult, out lpdwDisposition);
		if (num != 0)
		{
			throw PythonExceptions.CreateThrowable(error, num, CTypes.FormatError(num));
		}
		return new HKEYType(RegistryKey.FromHandle(phkResult));
	}

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern int RegCreateKeyEx(SafeRegistryHandle hKey, string lpSubKey, int Reserved, string lpClass, RegistryOptions dwOptions, RegistryRights samDesired, IntPtr lpSecurityAttributes, out SafeRegistryHandle phkResult, out int lpdwDisposition);

	[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int RegQueryValueEx(SafeRegistryHandle hKey, string lpValueName, IntPtr lpReserved, out int lpType, byte[] lpData, ref uint lpcbData);

	public static void DeleteKey(object key, string subKeyName)
	{
		HKEYType rootKey = GetRootKey(key);
		if (key is BigInteger && string.IsNullOrEmpty(subKeyName))
		{
			throw new InvalidCastException("DeleteKey() argument 2 must be string, not None");
		}
		try
		{
			rootKey.GetKey().DeleteSubKey(subKeyName);
		}
		catch (ArgumentException ex)
		{
			throw new ExternalException(ex.Message);
		}
	}

	public static void DeleteValue(object key, string value)
	{
		HKEYType rootKey = GetRootKey(key);
		rootKey.GetKey().DeleteValue(value, throwOnMissingValue: true);
	}

	public static string EnumKey(object key, int index)
	{
		HKEYType rootKey = GetRootKey(key);
		if (index >= rootKey.GetKey().SubKeyCount)
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.WindowsError, 22, "No more data is available");
		}
		return rootKey.GetKey().GetSubKeyNames()[index];
	}

	public static PythonTuple EnumValue(object key, int index)
	{
		HKEYType rootKey = GetRootKey(key);
		if (index >= rootKey.GetKey().ValueCount)
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.WindowsError, 22, "No more data is available");
		}
		RegistryKey key2 = rootKey.GetKey();
		string text = key2.GetValueNames()[index];
		QueryValueExImpl(key2, text, out var valueKind, out var value);
		return PythonTuple.MakeTuple(text, value, valueKind);
	}

	private static void QueryValueExImpl(RegistryKey nativeRootKey, string valueName, out int valueKind, out object value)
	{
		valueKind = 0;
		byte[] array = new byte[128];
		uint lpcbData = (uint)array.Length;
		for (int num = RegQueryValueEx(nativeRootKey.Handle, valueName, IntPtr.Zero, out valueKind, array, ref lpcbData); num == 234; num = RegQueryValueEx(nativeRootKey.Handle, valueName, IntPtr.Zero, out valueKind, array, ref lpcbData))
		{
			array = new byte[array.Length * 2];
			lpcbData = (uint)array.Length;
		}
		switch (valueKind)
		{
		case 7:
		{
			List list = new List();
			int num2 = 0;
			while (num2 < lpcbData)
			{
				for (int i = num2; i < lpcbData; i += 2)
				{
					if (array[i] == 0 && array[i + 1] == 0)
					{
						list.Add(ExtractString(array, num2, i));
						num2 = i + 2;
						if (num2 + 2 <= lpcbData && array[num2] == 0 && array[num2 + 1] == 0)
						{
							num2 = array.Length;
							break;
						}
					}
				}
				if (num2 != array.Length)
				{
					list.Add(ExtractString(array, num2, array.Length));
				}
			}
			value = list;
			break;
		}
		case 3:
			value = array.MakeString((int)lpcbData);
			break;
		case 1:
		case 2:
			if (lpcbData >= 2 && array[lpcbData - 1] == 0 && array[lpcbData - 2] == 0)
			{
				value = ExtractString(array, 0, (int)(lpcbData - 2));
			}
			else
			{
				value = ExtractString(array, 0, (int)lpcbData);
			}
			break;
		case 4:
			if (BitConverter.IsLittleEndian)
			{
				value = (array[3] << 24) | (array[2] << 16) | (array[1] << 8) | array[0];
			}
			else
			{
				value = (array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3];
			}
			break;
		default:
			value = null;
			break;
		}
	}

	public static string ExpandEnvironmentStrings(string value)
	{
		if (value == null)
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.TypeError, "must be unicode, not None");
		}
		return Environment.ExpandEnvironmentVariables(value);
	}

	private static string ExtractString(byte[] data, int start, int end)
	{
		if (end <= start)
		{
			return string.Empty;
		}
		char[] array = new char[(end - start) / 2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (char)(data[i * 2 + start] | (data[i * 2 + start + 1] << 8));
		}
		return new string(array);
	}

	public static void FlushKey(object key)
	{
		HKEYType rootKey = GetRootKey(key);
		rootKey.GetKey().Flush();
	}

	public static HKEYType OpenKey(object key, string subKeyName)
	{
		return OpenKey(key, subKeyName, 0, 131097);
	}

	public static HKEYType OpenKey(object key, string subKeyName, [DefaultParameterValue(0)] int res, [DefaultParameterValue(131097)] int sam)
	{
		HKEYType rootKey = GetRootKey(key);
		RegistryKey registryKey = null;
		RegistryKey key2 = rootKey.GetKey();
		try
		{
			if ((sam & 2) == 2 || (sam & 4) == 4)
			{
				registryKey = ((res == 0) ? key2.OpenSubKey(subKeyName, writable: true) : key2.OpenSubKey(subKeyName, RegistryKeyPermissionCheck.Default, (RegistryRights)res));
			}
			else
			{
				if ((sam & 1) != 1 && (sam & 8) != 8 && (sam & 0x10) != 16)
				{
					throw new Win32Exception("Unexpected mode");
				}
				registryKey = ((res == 0) ? key2.OpenSubKey(subKeyName, writable: false) : key2.OpenSubKey(subKeyName, RegistryKeyPermissionCheck.ReadSubTree, (RegistryRights)res));
			}
		}
		catch (SecurityException)
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.WindowsError, 5, "Access is denied");
		}
		if (registryKey == null)
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.WindowsError, 2, "The system cannot find the file specified");
		}
		return new HKEYType(registryKey);
	}

	public static HKEYType OpenKeyEx(object key, string subKeyName, [DefaultParameterValue(0)] int res, [DefaultParameterValue(131097)] int sam)
	{
		return OpenKey(key, subKeyName, res, sam);
	}

	public static PythonTuple QueryInfoKey(object key)
	{
		HKEYType hKEYType = null;
		if (key is int)
		{
			if (HKeyHandleCache.cache.ContainsKey((int)key) && HKeyHandleCache.cache[(int)key].IsAlive)
			{
				hKEYType = HKeyHandleCache.cache[(int)key].Target as HKEYType;
			}
		}
		else
		{
			hKEYType = GetRootKey(key);
		}
		if (hKEYType == null)
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.EnvironmentError, "key has been closed");
		}
		try
		{
			RegistryKey key2 = hKEYType.GetKey();
			return PythonTuple.MakeTuple(key2.SubKeyCount, key2.ValueCount, 0);
		}
		catch (ObjectDisposedException ex)
		{
			throw new ExternalException(ex.Message);
		}
	}

	public static object QueryValue(object key, string subKeyName)
	{
		HKEYType hKEYType = OpenKey(key, subKeyName);
		return hKEYType.GetKey().GetValue(null);
	}

	public static PythonTuple QueryValueEx(object key, string valueName)
	{
		HKEYType rootKey = GetRootKey(key);
		QueryValueExImpl(rootKey.GetKey(), valueName, out var valueKind, out var value);
		return PythonTuple.MakeTuple(value, valueKind);
	}

	public static void SetValue(object key, string subKeyName, int type, string value)
	{
		HKEYType hKEYType = CreateKey(key, subKeyName);
		hKEYType.GetKey().SetValue(null, value);
	}

	public static void SetValueEx(object key, string valueName, int reserved, int type, object value)
	{
		HKEYType rootKey = GetRootKey(key);
		switch (type)
		{
		case 7:
		{
			int size = ((List)value)._size;
			string[] array = new string[size];
			Array.Copy(((List)value)._data, array, size);
			rootKey.GetKey().SetValue(valueName, array, (RegistryValueKind)type);
			break;
		}
		case 3:
		{
			byte[] value2 = null;
			if (value is string)
			{
				string s = value as string;
				ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
				value2 = aSCIIEncoding.GetBytes(s);
			}
			rootKey.GetKey().SetValue(valueName, value2, (RegistryValueKind)type);
			break;
		}
		default:
			rootKey.GetKey().SetValue(valueName, value, (RegistryValueKind)type);
			break;
		}
	}

	public static HKEYType ConnectRegistry(string computerName, BigInteger key)
	{
		if (string.IsNullOrEmpty(computerName))
		{
			computerName = string.Empty;
		}
		RegistryKey key2;
		try
		{
			key2 = RegistryKey.OpenRemoteBaseKey(MapSystemKey(key), computerName);
		}
		catch (IOException ex)
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.WindowsError, 53, ex.Message);
		}
		catch (Exception ex2)
		{
			throw new ExternalException(ex2.Message);
		}
		return new HKEYType(key2);
	}

	private static HKEYType GetRootKey(object key)
	{
		HKEYType hKEYType = key as HKEYType;
		if (hKEYType == null)
		{
			if (!(key is BigInteger))
			{
				throw new InvalidCastException("The object is not a PyHKEY object");
			}
			hKEYType = new HKEYType(RegistryKey.OpenRemoteBaseKey(MapSystemKey((BigInteger)key), string.Empty));
		}
		return hKEYType;
	}

	private static RegistryHive MapSystemKey(BigInteger hKey)
	{
		if (hKey == HKEY_CLASSES_ROOT)
		{
			return RegistryHive.ClassesRoot;
		}
		if (hKey == HKEY_CURRENT_CONFIG)
		{
			return RegistryHive.CurrentConfig;
		}
		if (hKey == HKEY_CURRENT_USER)
		{
			return RegistryHive.CurrentUser;
		}
		if (hKey == HKEY_DYN_DATA)
		{
			return RegistryHive.DynData;
		}
		if (hKey == HKEY_LOCAL_MACHINE)
		{
			return RegistryHive.LocalMachine;
		}
		if (hKey == HKEY_PERFORMANCE_DATA)
		{
			return RegistryHive.PerformanceData;
		}
		if (hKey == HKEY_USERS)
		{
			return RegistryHive.Users;
		}
		throw new ValueErrorException("Unknown system key");
	}

	private static int MapRegistryValueKind(RegistryValueKind registryValueKind)
	{
		return (int)registryValueKind;
	}
}

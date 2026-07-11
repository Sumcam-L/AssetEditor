using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Bespoke.Common;

public static class Utility
{
	[DllImport("user32.dll")]
	public static extern bool SetForegroundWindow(IntPtr hWnd);

	public static string ASCIIByteArrayToString(byte[] source)
	{
		if (source == null)
		{
			return string.Empty;
		}
		ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
		return aSCIIEncoding.GetString(source);
	}

	public static string UnicodeByteArrayToString(byte[] source)
	{
		if (source == null)
		{
			return string.Empty;
		}
		UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
		return unicodeEncoding.GetString(source);
	}

	public static void CopyDirectory(string sourceDirectory, string destinationDirectory, bool overwrite = true)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirectory);
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException(directoryInfo.FullName);
		}
		DirectoryInfo directoryInfo2 = new DirectoryInfo(destinationDirectory);
		if (!directoryInfo2.Exists)
		{
			directoryInfo2.Create();
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		foreach (DirectoryInfo directoryInfo3 in directories)
		{
			CopyDirectory(directoryInfo3.FullName, directoryInfo2.FullName + Path.DirectorySeparatorChar + directoryInfo3.Name);
		}
		FileInfo[] files = directoryInfo.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			fileInfo.CopyTo(directoryInfo2.FullName + Path.DirectorySeparatorChar + fileInfo.Name, overwrite);
		}
	}

	public static bool FindFile(string fileName, string startDirectory, out FileInfo foundFile)
	{
		bool result = false;
		foundFile = null;
		FileInfo fileInfo = new FileInfo(fileName);
		if (fileInfo.Exists)
		{
			foundFile = fileInfo;
			result = true;
		}
		else
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(startDirectory);
			FileInfo[] files = directoryInfo.GetFiles(fileInfo.Name, SearchOption.AllDirectories);
			if (files.Length > 0)
			{
				foundFile = files[0];
				result = true;
			}
		}
		return result;
	}

	public static bool ValuesInProximity(int value1, int value2, int proximityThreshold)
	{
		return Math.Abs(value1 - value2) <= proximityThreshold;
	}

	public static bool ValuesInProximity(double value1, double value2, double proximityThreshold)
	{
		return Math.Abs(value1 - value2) <= proximityThreshold;
	}

	public static bool ValuesInProximity(DateTime value1, DateTime value2, TimeSpan proximityThreshold)
	{
		TimeSpan timeSpan = ((!(value1 > value2)) ? value2.Subtract(value1) : value1.Subtract(value2));
		return timeSpan <= proximityThreshold;
	}

	public static bool IsNumeric(string value)
	{
		int result;
		return IsNumeric(value, out result);
	}

	public static bool IsNumeric(string value, out int result)
	{
		if (int.TryParse(value, out result))
		{
			return true;
		}
		return false;
	}

	public static float ToDegrees(float radians)
	{
		return radians * 57.29578f;
	}

	public static float ToRadians(float degrees)
	{
		return degrees * 0.01745329f;
	}

	public static Enum[] GetEnumValues(Type enumType)
	{
		Assert.IsTrue(enumType.IsEnum);
		FieldInfo[] fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
		Enum[] array = new Enum[fields.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (Enum)fields[i].GetValue(null);
		}
		return array;
	}

	public static int GetEnumMaxValue(Type enumType)
	{
		Assert.IsTrue(enumType.IsEnum);
		int num = int.MinValue;
		foreach (int value in Enum.GetValues(enumType))
		{
			if (value > num)
			{
				num = value;
			}
		}
		return num;
	}

	public static T[] CopySubArray<T>(this T[] source, int start, int length)
	{
		T[] array = new T[length];
		Array.Copy(source, start, array, 0, length);
		return array;
	}

	public static byte[] SwapEndian(byte[] data)
	{
		byte[] array = new byte[data.Length];
		int num = data.Length - 1;
		int num2 = 0;
		while (num >= 0)
		{
			array[num2] = data[num];
			num--;
			num2++;
		}
		return array;
	}

	public static bool IsFlagSet(Enum input, Enum flagToMatch)
	{
		return (Convert.ToUInt32(input) & Convert.ToUInt32(flagToMatch)) != 0;
	}

	public static int[] BuildLatinSquareRow(int seed, int minValue, int conditionCount)
	{
		Assert.IsTrue(minValue < conditionCount);
		Assert.IsTrue(seed >= minValue);
		Assert.IsTrue(seed < conditionCount);
		List<int> list = new List<int>();
		for (int i = seed; i < conditionCount; i++)
		{
			list.Add(i);
		}
		if (list.Count < conditionCount)
		{
			for (int j = minValue; j < seed; j++)
			{
				list.Add(j);
			}
		}
		return list.ToArray();
	}
}

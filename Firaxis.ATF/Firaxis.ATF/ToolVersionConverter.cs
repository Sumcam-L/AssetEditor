using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Firaxis.ATF;

public static class ToolVersionConverter
{
	private static readonly int m_majorVersion = 4;

	public static Version GetVersion(string changelistNumber, Version fallbackVersion)
	{
		if (changelistNumber == "0")
		{
			return fallbackVersion;
		}
		IEnumerable<int> chunkLengths = GetChunkLengths(changelistNumber);
		string versionString = GetVersionString(changelistNumber, chunkLengths);
		Version result = null;
		if (!Version.TryParse(versionString, out result))
		{
			result = fallbackVersion;
		}
		return result;
	}

	private static IEnumerable<int> GetChunkLengths(string changelistNumber)
	{
		int[] array = new int[3];
		int num = 3;
		int num2 = changelistNumber.Length;
		for (int num3 = array.Length - 1; num3 >= 0; num3--)
		{
			if (num > num2 || num3 == 0)
			{
				num = num2;
			}
			array[num3] = num;
			num2 -= num;
		}
		return array;
	}

	private static string GetVersionString(string changelistNumber, IEnumerable<int> chunkLengths)
	{
		StringBuilder stringBuilder = new StringBuilder(changelistNumber.Length + chunkLengths.Count());
		int num = 0;
		foreach (int chunkLength in chunkLengths)
		{
			for (int i = chunkLength; i < 1; i++)
			{
				stringBuilder.Append("0");
			}
			stringBuilder.Append(changelistNumber.Substring(num, chunkLength));
			num += chunkLength;
			stringBuilder.Append(".");
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		stringBuilder.Insert(0, m_majorVersion + ".");
		return stringBuilder.ToString();
	}
}

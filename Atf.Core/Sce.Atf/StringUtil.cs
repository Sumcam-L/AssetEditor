using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Sce.Atf;

public static class StringUtil
{
	public static bool IsNullOrEmptyOrWhitespace(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return true;
		}
		string text = s.Trim();
		return text.Length == 0;
	}

	public static string RemoveAllWhiteSpace(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		StringBuilder stringBuilder = new StringBuilder(s.Length);
		foreach (char c in s)
		{
			if (!char.IsWhiteSpace(c))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static IList<int> GetUnicodeCodePoints(string text)
	{
		List<int> list = new List<int>(text.Length);
		int num = 0;
		foreach (char c in text)
		{
			int num2 = c;
			if (num2 >= 55296 && num2 <= 57343)
			{
				if (num2 <= 56319)
				{
					num = num2;
					continue;
				}
				num2 = (num - 55296) * 1024 + (num2 - 56320) + 65536;
			}
			list.Add(num2);
		}
		return list;
	}

	public static int CompareNaturalOrder(string strA, string strB)
	{
		int i;
		for (i = 0; i < strA.Length && i < strB.Length && strA[i] == strB[i]; i++)
		{
		}
		int j;
		for (j = i; j < strA.Length && char.IsDigit(strA[j]); j++)
		{
		}
		int k;
		for (k = i; k < strB.Length && char.IsDigit(strB[k]); k++)
		{
		}
		if (j > i && k > i && long.TryParse(strA.Substring(i, j - i), out var result) && long.TryParse(strB.Substring(i, k - i), out var result2))
		{
			if (result < result2)
			{
				return -1;
			}
			if (result > result2)
			{
				return 1;
			}
		}
		return string.Compare(strA, strB);
	}

	public static string EscapeQuotes(string source)
	{
		StringBuilder stringBuilder = new StringBuilder(source);
		stringBuilder.Replace("\"", "\\\"");
		stringBuilder.Replace("'", "\\'");
		return stringBuilder.ToString();
	}

	internal static string GetNumberListSeparator(IFormatProvider formatProvider)
	{
		CultureInfo cultureInfo = (formatProvider as CultureInfo) ?? Thread.CurrentThread.CurrentCulture;
		return cultureInfo.TextInfo.ListSeparator;
	}
}

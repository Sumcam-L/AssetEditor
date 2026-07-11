using System;
using System.Text.RegularExpressions;

namespace Firaxis.CivTech.AssetObjects;

public static class RegexFilterHelper
{
	public static Regex BuildRegex(string filterText)
	{
		if (string.IsNullOrEmpty(filterText))
		{
			return null;
		}
		string pattern = SanitizeFilter(filterText);
		return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}

	private static string SanitizeFilter(string inFilter)
	{
		if (inFilter.StartsWith("REGEX:", StringComparison.CurrentCultureIgnoreCase))
		{
			return inFilter.Substring("REGEX:".Length);
		}
		if (inFilter == "*")
		{
			return ".*";
		}
		string[] array = inFilter.Split(' ');
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Regex.Escape(array[i]);
		}
		return string.Join(".*", array);
	}
}

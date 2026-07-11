using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Modules;

public static class unicodedata
{
	private const string UnicodedataResourceName = "IronPython.Modules.unicodedata.IPyUnicodeData.txt.gz";

	private const string OtherNotAssigned = "Cn";

	private static Dictionary<int, CharInfo> database;

	private static List<RangeInfo> ranges;

	private static Dictionary<string, int> nameLookup;

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, IDictionary dict)
	{
		EnsureLoaded();
	}

	public static string lookup(string name)
	{
		EnsureLoaded();
		return Convert.ToChar(nameLookup[name]).ToString();
	}

	public static string name(char unichr, string @default = null)
	{
		try
		{
			return GetInfo(unichr).Name;
		}
		catch (KeyNotFoundException)
		{
			if (@default != null)
			{
				return @default;
			}
			throw;
		}
	}

	public static int @decimal(char unichr, int @default)
	{
		try
		{
			int? numeric_Value_Decimal = GetInfo(unichr).Numeric_Value_Decimal;
			if (numeric_Value_Decimal.HasValue)
			{
				return numeric_Value_Decimal.Value;
			}
			return @default;
		}
		catch (KeyNotFoundException)
		{
			return @default;
		}
	}

	public static int @decimal(char unichr)
	{
		try
		{
			int? numeric_Value_Decimal = GetInfo(unichr).Numeric_Value_Decimal;
			if (numeric_Value_Decimal.HasValue)
			{
				return numeric_Value_Decimal.Value;
			}
			throw PythonOps.ValueError("not a decimal");
		}
		catch (KeyNotFoundException)
		{
			throw PythonOps.ValueError("not a decimal");
		}
	}

	public static object @decimal(char unichr, object @default)
	{
		try
		{
			int? numeric_Value_Decimal = GetInfo(unichr).Numeric_Value_Decimal;
			if (numeric_Value_Decimal.HasValue)
			{
				return numeric_Value_Decimal.Value;
			}
			return @default;
		}
		catch (KeyNotFoundException)
		{
			return @default;
		}
	}

	public static int digit(char unichr, int @default)
	{
		try
		{
			int? numeric_Value_Digit = GetInfo(unichr).Numeric_Value_Digit;
			if (numeric_Value_Digit.HasValue)
			{
				return numeric_Value_Digit.Value;
			}
			return @default;
		}
		catch (KeyNotFoundException)
		{
			return @default;
		}
	}

	public static object digit(char unichr, object @default)
	{
		try
		{
			int? numeric_Value_Digit = GetInfo(unichr).Numeric_Value_Digit;
			if (numeric_Value_Digit.HasValue)
			{
				return numeric_Value_Digit.Value;
			}
			return @default;
		}
		catch (KeyNotFoundException)
		{
			return @default;
		}
	}

	public static int digit(char unichr)
	{
		try
		{
			int? numeric_Value_Digit = GetInfo(unichr).Numeric_Value_Digit;
			if (numeric_Value_Digit.HasValue)
			{
				return numeric_Value_Digit.Value;
			}
			throw PythonOps.ValueError("not a digit");
		}
		catch (KeyNotFoundException)
		{
			throw PythonOps.ValueError("not a digit");
		}
	}

	public static double numeric(char unichr, double @default)
	{
		try
		{
			double? numeric_Value_Numeric = GetInfo(unichr).Numeric_Value_Numeric;
			if (numeric_Value_Numeric.HasValue)
			{
				return numeric_Value_Numeric.Value;
			}
			return @default;
		}
		catch (KeyNotFoundException)
		{
			return @default;
		}
	}

	public static double numeric(char unichr)
	{
		try
		{
			double? numeric_Value_Numeric = GetInfo(unichr).Numeric_Value_Numeric;
			if (numeric_Value_Numeric.HasValue)
			{
				return numeric_Value_Numeric.Value;
			}
			throw PythonOps.ValueError("not a numeric character");
		}
		catch (KeyNotFoundException)
		{
			throw PythonOps.ValueError("not a numeric character");
		}
	}

	public static object numeric(char unichr, object @default)
	{
		try
		{
			double? numeric_Value_Numeric = GetInfo(unichr).Numeric_Value_Numeric;
			if (numeric_Value_Numeric.HasValue)
			{
				return numeric_Value_Numeric.Value;
			}
			return @default;
		}
		catch (KeyNotFoundException)
		{
			return @default;
		}
	}

	public static string category(char unichr)
	{
		if (!database.ContainsKey(unichr))
		{
			return "Cn";
		}
		return GetInfo(unichr).General_Category;
	}

	public static string bidirectional(char unichr)
	{
		if (!database.ContainsKey(unichr))
		{
			return string.Empty;
		}
		return GetInfo(unichr).Bidi_Class;
	}

	public static int combining(char unichr)
	{
		if (!database.ContainsKey(unichr))
		{
			return 0;
		}
		return GetInfo(unichr).Canonical_Combining_Class;
	}

	public static string east_asian_width(char unichr)
	{
		if (!database.ContainsKey(unichr))
		{
			return string.Empty;
		}
		return GetInfo(unichr).East_Asian_Width;
	}

	public static int mirrored(char unichr)
	{
		if (!database.ContainsKey(unichr))
		{
			return 0;
		}
		return GetInfo(unichr).Bidi_Mirrored;
	}

	public static string decomposition(char unichr)
	{
		if (!database.ContainsKey(unichr))
		{
			return string.Empty;
		}
		return GetInfo(unichr).Decomposition_Type;
	}

	public static string normalize(string form, string unistr)
	{
		return unistr.Normalize(form switch
		{
			"NFC" => NormalizationForm.FormC, 
			"NFD" => NormalizationForm.FormD, 
			"NFKC" => NormalizationForm.FormKC, 
			"NFKD" => NormalizationForm.FormKD, 
			_ => throw new ArgumentException("Invalid normalization form " + form, "form"), 
		});
	}

	private static void BuildDatabase(StreamReader data)
	{
		char[] separator = new char[1] { ';' };
		database = new Dictionary<int, CharInfo>();
		ranges = new List<RangeInfo>();
		foreach (string item in data.ReadLines())
		{
			int num = item.IndexOf('#');
			string text = ((num == -1) ? item : item.Substring(item.Length - num).Trim());
			if (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split(separator, 2);
				Match match = Regex.Match(array[0], "([0-9a-fA-F]{4})\\.\\.([0-9a-fA-F]{4})");
				if (match.Success)
				{
					int first = Convert.ToInt32(match.Groups[1].Value, 16);
					int last = Convert.ToInt32(match.Groups[2].Value, 16);
					ranges.Add(new RangeInfo(first, last, array[1].Split(separator)));
				}
				else
				{
					database[Convert.ToInt32(array[0], 16)] = new CharInfo(array[1].Split(separator));
				}
			}
		}
	}

	private static void BuildNameLookup()
	{
		nameLookup = database.Where((KeyValuePair<int, CharInfo> c) => !c.Value.Name.StartsWith("<")).ToDictionary((KeyValuePair<int, CharInfo> c) => c.Value.Name, (KeyValuePair<int, CharInfo> c) => c.Key);
	}

	private static CharInfo GetInfo(char unichr)
	{
		EnsureLoaded();
		return database[unichr];
	}

	private static void EnsureLoaded()
	{
		if (database == null || nameLookup == null)
		{
			Stream manifestResourceStream = typeof(unicodedata).Assembly.GetManifestResourceStream("IronPython.Modules.unicodedata.IPyUnicodeData.txt.gz");
			GZipStream stream = new GZipStream(manifestResourceStream, CompressionMode.Decompress);
			StreamReader data = new StreamReader(stream, Encoding.UTF8);
			BuildDatabase(data);
			BuildNameLookup();
		}
	}
}

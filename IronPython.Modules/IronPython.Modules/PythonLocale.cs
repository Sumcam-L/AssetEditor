using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonLocale
{
	private enum LocaleCategories
	{
		All,
		Collate,
		CType,
		Monetary,
		Numeric,
		Time
	}

	internal class LocaleInfo
	{
		private readonly PythonContext _context;

		private PythonDictionary conv;

		public CultureInfo Collate
		{
			get
			{
				return _context.CollateCulture;
			}
			set
			{
				_context.CollateCulture = value;
			}
		}

		public CultureInfo CType
		{
			get
			{
				return _context.CTypeCulture;
			}
			set
			{
				_context.CTypeCulture = value;
			}
		}

		public CultureInfo Time
		{
			get
			{
				return _context.TimeCulture;
			}
			set
			{
				_context.TimeCulture = value;
			}
		}

		public CultureInfo Monetary
		{
			get
			{
				return _context.MonetaryCulture;
			}
			set
			{
				_context.MonetaryCulture = value;
			}
		}

		public CultureInfo Numeric
		{
			get
			{
				return _context.NumericCulture;
			}
			set
			{
				_context.NumericCulture = value;
			}
		}

		public LocaleInfo(PythonContext context)
		{
			_context = context;
		}

		public override string ToString()
		{
			return base.ToString();
		}

		public PythonDictionary GetConventionsTable()
		{
			CreateConventionsDict();
			return conv;
		}

		public string SetLocale(CodeContext context, int category, string locale)
		{
			switch ((LocaleCategories)category)
			{
			case LocaleCategories.All:
				SetLocale(context, 1, locale);
				SetLocale(context, 2, locale);
				SetLocale(context, 3, locale);
				SetLocale(context, 4, locale);
				return SetLocale(context, 5, locale);
			case LocaleCategories.Collate:
			{
				CultureInfo culture3 = (Collate = LocaleToCulture(context, locale));
				return CultureToName(culture3);
			}
			case LocaleCategories.CType:
			{
				CultureInfo culture2 = (CType = LocaleToCulture(context, locale));
				return CultureToName(culture2);
			}
			case LocaleCategories.Time:
			{
				CultureInfo culture = (Time = LocaleToCulture(context, locale));
				return CultureToName(culture);
			}
			case LocaleCategories.Monetary:
				Monetary = LocaleToCulture(context, locale);
				conv = null;
				return CultureToName(Monetary);
			case LocaleCategories.Numeric:
				Numeric = LocaleToCulture(context, locale);
				conv = null;
				return CultureToName(Numeric);
			default:
				throw PythonExceptions.CreateThrowable(_localeerror(context), "unknown locale category");
			}
		}

		public string GetLocale(CodeContext context, int category)
		{
			switch ((LocaleCategories)category)
			{
			case LocaleCategories.All:
				if (Collate != CType || Collate != Time || Collate != Monetary || Collate != Numeric)
				{
					return $"LC_COLLATE={GetLocale(context, 1)};LC_CTYPE={GetLocale(context, 2)};LC_MONETARY={GetLocale(context, 3)};LC_NUMERIC={GetLocale(context, 4)};LC_TIME={GetLocale(context, 5)}";
				}
				goto case LocaleCategories.Collate;
			case LocaleCategories.Collate:
				return CultureToName(Collate);
			case LocaleCategories.CType:
				return CultureToName(CType);
			case LocaleCategories.Time:
				return CultureToName(Time);
			case LocaleCategories.Monetary:
				return CultureToName(Monetary);
			case LocaleCategories.Numeric:
				return CultureToName(Numeric);
			default:
				throw PythonExceptions.CreateThrowable(_localeerror(context), "unknown locale category");
			}
		}

		public string CultureToName(CultureInfo culture)
		{
			if (culture == PythonContext.CCulture)
			{
				return "C";
			}
			return culture.Name.Replace('-', '_');
		}

		private CultureInfo LocaleToCulture(CodeContext context, string locale)
		{
			if (locale == "C")
			{
				return PythonContext.CCulture;
			}
			locale = locale.Replace('_', '-');
			try
			{
				return StringUtils.GetCultureInfo(locale);
			}
			catch (ArgumentException)
			{
				throw PythonExceptions.CreateThrowable(_localeerror(context), $"unknown locale: {locale}");
			}
		}

		private void CreateConventionsDict()
		{
			conv = new PythonDictionary();
			conv["decimal_point"] = Numeric.NumberFormat.NumberDecimalSeparator;
			conv["grouping"] = GroupsToList(Numeric.NumberFormat.NumberGroupSizes);
			conv["thousands_sep"] = Numeric.NumberFormat.NumberGroupSeparator;
			conv["mon_decimal_point"] = Monetary.NumberFormat.CurrencyDecimalSeparator;
			conv["mon_thousands_sep"] = Monetary.NumberFormat.CurrencyGroupSeparator;
			conv["mon_grouping"] = GroupsToList(Monetary.NumberFormat.CurrencyGroupSizes);
			conv["int_curr_symbol"] = Monetary.NumberFormat.CurrencySymbol;
			conv["currency_symbol"] = Monetary.NumberFormat.CurrencySymbol;
			conv["frac_digits"] = Monetary.NumberFormat.CurrencyDecimalDigits;
			conv["int_frac_digits"] = Monetary.NumberFormat.CurrencyDecimalDigits;
			conv["positive_sign"] = Monetary.NumberFormat.PositiveSign;
			conv["negative_sign"] = Monetary.NumberFormat.NegativeSign;
			conv["p_sign_posn"] = Monetary.NumberFormat.CurrencyPositivePattern;
			conv["n_sign_posn"] = Monetary.NumberFormat.CurrencyNegativePattern;
		}

		private static List GroupsToList(int[] groups)
		{
			List list = new List(groups);
			if (groups.Length > 0 && groups[groups.Length - 1] == 0)
			{
				list[list.__len__() - 1] = 127;
			}
			else
			{
				list.AddNoLock(0);
			}
			return list;
		}
	}

	public const string __doc__ = "Provides access for querying and manipulating the current locale settings";

	public const int CHAR_MAX = 127;

	public const int LC_ALL = 0;

	public const int LC_COLLATE = 1;

	public const int LC_CTYPE = 2;

	public const int LC_MONETARY = 3;

	public const int LC_NUMERIC = 4;

	public const int LC_TIME = 5;

	private static readonly object _localeKey = new object();

	internal static string PreferredEncoding => "cp" + CultureInfo.CurrentCulture.TextInfo.ANSICodePage;

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		EnsureLocaleInitialized(context);
		context.EnsureModuleException("_localeerror", dict, "Error", "_locale");
	}

	internal static void EnsureLocaleInitialized(PythonContext context)
	{
		if (!context.HasModuleState(_localeKey))
		{
			context.SetModuleState(_localeKey, new LocaleInfo(context));
		}
	}

	[Documentation("gets the default locale tuple")]
	public static object _getdefaultlocale()
	{
		return PythonTuple.MakeTuple(CultureInfo.CurrentCulture.Name.Replace('-', '_').Replace(' ', '_'), PreferredEncoding);
	}

	[Documentation("gets the locale's convetions table.  \r\n\r\nThe conventions table is a dictionary that contains information on how to use \r\nthe locale for numeric and monetary formatting")]
	public static object localeconv(CodeContext context)
	{
		return GetLocaleInfo(context).GetConventionsTable();
	}

	[Documentation("Sets the current locale for the given category.\r\n\r\nLC_ALL:       sets locale for all options below\r\nLC_COLLATE:   sets locale for collation (strcoll and strxfrm) only\r\nLC_CTYPE:     sets locale for CType [unused]\r\nLC_MONETARY:  sets locale for the monetary functions (localeconv())\r\nLC_NUMERIC:   sets the locale for numeric functions (slocaleconv())\r\nLC_TIME:      sets the locale for time functions [unused]\r\n\r\nIf locale is None then the current setting is returned.\r\n")]
	public static object setlocale(CodeContext context, int category, [DefaultParameterValue(null)] string locale)
	{
		LocaleInfo localeInfo = GetLocaleInfo(context);
		if (locale == null)
		{
			return localeInfo.GetLocale(context, category);
		}
		return localeInfo.SetLocale(context, category, locale);
	}

	[Documentation("compares two strings using the current locale")]
	public static int strcoll(CodeContext context, string string1, string string2)
	{
		return GetLocaleInfo(context).Collate.CompareInfo.Compare(string1, string2, CompareOptions.None);
	}

	[Documentation("returns a transformed string that can be compared using the built-in cmp.\r\n        \r\nCurrently returns the string unmodified")]
	public static object strxfrm(string @string)
	{
		return @string;
	}

	internal static LocaleInfo GetLocaleInfo(CodeContext context)
	{
		EnsureLocaleInitialized(PythonContext.GetContext(context));
		return (LocaleInfo)PythonContext.GetContext(context).GetModuleState(_localeKey);
	}

	private static PythonType _localeerror(CodeContext context)
	{
		return (PythonType)PythonContext.GetContext(context).GetModuleState("_localeerror");
	}
}

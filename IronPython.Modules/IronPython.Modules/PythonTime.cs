using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonTime
{
	private enum FormatInfoType
	{
		UserText,
		SimpleFormat,
		CustomFormat
	}

	private class FormatInfo
	{
		public FormatInfoType Type;

		public string Text;

		public FormatInfo(string text)
		{
			Type = FormatInfoType.SimpleFormat;
			Text = text;
		}

		public FormatInfo(FormatInfoType type, string text)
		{
			Type = type;
			Text = text;
		}

		public override string ToString()
		{
			return $"{Type}:{Text}";
		}
	}

	[Flags]
	private enum FoundDateComponents
	{
		None = 0,
		Year = 1,
		Date = 1,
		DayOfWeek = 2
	}

	[PythonType]
	public class struct_time : PythonTuple
	{
		private static PythonType _StructTimeType = DynamicHelpers.GetPythonTypeFromType(typeof(struct_time));

		public object tm_year => _data[0];

		public object tm_mon => _data[1];

		public object tm_mday => _data[2];

		public object tm_hour => _data[3];

		public object tm_min => _data[4];

		public object tm_sec => _data[5];

		public object tm_wday => _data[6];

		public object tm_yday => _data[7];

		public object tm_isdst => _data[8];

		public int n_fields => _data.Length;

		public int n_sequence_fields => _data.Length;

		public int n_unnamed_fields => 0;

		internal struct_time(int year, int month, int day, int hour, int minute, int second, int dayOfWeek, int dayOfYear, int isDst)
			: base(new object[9] { year, month, day, hour, minute, second, dayOfWeek, dayOfYear, isDst })
		{
		}

		internal struct_time(PythonTuple sequence)
			: base(sequence)
		{
		}

		public static struct_time __new__(CodeContext context, PythonType cls, int year, int month, int day, int hour, int minute, int second, int dayOfWeek, int dayOfYear, int isDst)
		{
			if (cls == _StructTimeType)
			{
				return new struct_time(year, month, day, hour, minute, second, dayOfWeek, dayOfYear, isDst);
			}
			if (!(cls.CreateInstance(context, year, month, day, hour, minute, second, dayOfWeek, dayOfYear, isDst) is struct_time result))
			{
				throw PythonOps.TypeError("{0} is not a subclass of time.struct_time", cls);
			}
			return result;
		}

		public static struct_time __new__(CodeContext context, PythonType cls, [NotNull] PythonTuple sequence)
		{
			if (sequence.__len__() != 9)
			{
				throw PythonOps.TypeError("time.struct_time() takes a 9-sequence ({0}-sequence given)", sequence.__len__());
			}
			if (cls == _StructTimeType)
			{
				return new struct_time(sequence);
			}
			if (!(cls.CreateInstance(context, sequence) is struct_time result))
			{
				throw PythonOps.TypeError("{0} is not a subclass of time.struct_time", cls);
			}
			return result;
		}

		public static struct_time __new__(CodeContext context, PythonType cls, [NotNull] IEnumerable sequence)
		{
			return __new__(context, cls, PythonTuple.Make(sequence));
		}

		public PythonTuple __reduce__()
		{
			return PythonTuple.MakeTuple(_StructTimeType, PythonTuple.MakeTuple(tm_year, tm_mon, tm_mday, tm_hour, tm_min, tm_sec, tm_wday, tm_yday, tm_isdst));
		}

		public static object __getnewargs__(CodeContext context, int year, int month, int day, int hour, int minute, int second, int dayOfWeek, int dayOfYear, int isDst)
		{
			return PythonTuple.MakeTuple(__new__(context, _StructTimeType, year, month, day, hour, minute, second, dayOfWeek, dayOfYear, isDst));
		}

		public override string ToString()
		{
			return string.Format("time.struct_time(tm_year={0}, tm_mon={1}, tm_mday={2}, tm_hour={3}, tm_min={4}, tm_sec={5}, tm_wday={6}, tm_yday={7}, tm_isdst={8})", _data);
		}

		public override string __repr__(CodeContext context)
		{
			return ToString();
		}
	}

	private const int YearIndex = 0;

	private const int MonthIndex = 1;

	private const int DayIndex = 2;

	private const int HourIndex = 3;

	private const int MinuteIndex = 4;

	private const int SecondIndex = 5;

	private const int WeekdayIndex = 6;

	private const int DayOfYearIndex = 7;

	private const int IsDaylightSavingsIndex = 8;

	private const int MaxIndex = 9;

	private const int minYear = 1900;

	private const double epochDifference = 62135596800.0;

	private const double ticksPerSecond = 10000000.0;

	public const bool accept2dyear = true;

	public const string __doc__ = "This module provides various functions to manipulate time values.";

	public static readonly int altzone;

	public static readonly int daylight;

	public static readonly int timezone;

	public static readonly PythonTuple tzname;

	private static Stopwatch sw;

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		PythonLocale.EnsureLocaleInitialized(context);
	}

	static PythonTime()
	{
		DaylightTime daylightChanges = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year);
		daylight = ((!(daylightChanges.Start == daylightChanges.End) || !(daylightChanges.Start == DateTime.MinValue) || daylightChanges.Delta.Ticks != 0) ? 1 : 0);
		tzname = PythonTuple.MakeTuple(TimeZone.CurrentTimeZone.StandardName, TimeZone.CurrentTimeZone.DaylightName);
		altzone = (int)(0.0 - TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalSeconds);
		timezone = altzone;
		if (daylight != 0)
		{
			if (TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now))
			{
				timezone += (int)daylightChanges.Delta.TotalSeconds;
			}
			else
			{
				altzone -= (int)daylightChanges.Delta.TotalSeconds;
			}
		}
	}

	internal static long TimestampToTicks(double seconds)
	{
		return (long)((seconds + 62135596800.0) * 10000000.0);
	}

	internal static double TicksToTimestamp(long ticks)
	{
		return (double)ticks / 10000000.0 - 62135596800.0;
	}

	public static string asctime(CodeContext context)
	{
		return asctime(context, null);
	}

	public static string asctime(CodeContext context, object time)
	{
		DateTime dateTime;
		if (time is PythonTuple)
		{
			dateTime = GetDateTimeFromTupleNoDst(context, (PythonTuple)time);
		}
		else
		{
			if (time != null)
			{
				throw PythonOps.TypeError("expected struct_time or None");
			}
			dateTime = DateTime.Now;
		}
		return dateTime.ToString("ddd MMM dd HH:mm:ss yyyy", null);
	}

	public static double clock()
	{
		InitStopWatch();
		return (double)sw.ElapsedTicks / (double)Stopwatch.Frequency;
	}

	public static string ctime(CodeContext context)
	{
		return asctime(context, localtime());
	}

	public static string ctime(CodeContext context, object seconds)
	{
		if (seconds == null)
		{
			return ctime(context);
		}
		return asctime(context, localtime(seconds));
	}

	public static void sleep(double tm)
	{
		Thread.Sleep((int)(tm * 1000.0));
	}

	public static double time()
	{
		return TicksToTimestamp(DateTime.Now.ToUniversalTime().Ticks);
	}

	public static PythonTuple localtime()
	{
		return GetDateTimeTuple(DateTime.Now, DateTime.Now.IsDaylightSavingTime());
	}

	public static PythonTuple localtime(object seconds)
	{
		if (seconds == null)
		{
			return localtime();
		}
		DateTime dt = TimestampToDateTime(GetTimestampFromObject(seconds)).AddSeconds(-timezone);
		return GetDateTimeTuple(dt, dt.IsDaylightSavingTime());
	}

	public static PythonTuple gmtime()
	{
		return GetDateTimeTuple(DateTime.Now.ToUniversalTime(), dstMode: false);
	}

	public static PythonTuple gmtime(object seconds)
	{
		if (seconds == null)
		{
			return gmtime();
		}
		DateTime dt = new DateTime(TimestampToTicks(GetTimestampFromObject(seconds)), DateTimeKind.Unspecified);
		return GetDateTimeTuple(dt, dstMode: false);
	}

	public static double mktime(CodeContext context, PythonTuple localTime)
	{
		return TicksToTimestamp(GetDateTimeFromTuple(context, localTime).AddSeconds(timezone).Ticks);
	}

	public static string strftime(CodeContext context, string format)
	{
		return strftime(context, format, DateTime.Now, null);
	}

	public static string strftime(CodeContext context, string format, PythonTuple dateTime)
	{
		return strftime(context, format, GetDateTimeFromTupleNoDst(context, dateTime), null);
	}

	public static object strptime(CodeContext context, string @string)
	{
		return DateTime.Parse(@string, PythonLocale.GetLocaleInfo(context).Time.DateTimeFormat);
	}

	public static object strptime(CodeContext context, string @string, string format)
	{
		bool postProcess;
		FoundDateComponents found;
		List<FormatInfo> list = PythonFormatToCLIFormat(format, forParse: true, out postProcess, out found);
		DateTime result;
		if (postProcess)
		{
			int num = FindFormat(list, "\\%j");
			int num2 = FindFormat(list, "\\%W");
			int num3 = FindFormat(list, "\\%U");
			if (num != -1 && num2 == -1 && num3 == -1)
			{
				result = new DateTime(1900, 1, 1).AddDays(int.Parse(@string));
			}
			else if (num2 != -1 && num == -1 && num3 == -1)
			{
				result = new DateTime(1900, 1, 1).AddDays(int.Parse(@string) * 7);
			}
			else
			{
				if (num3 == -1 || num != -1 || num2 != -1)
				{
					throw PythonOps.ValueError("cannot parse %j, %W, or %U w/ other values");
				}
				result = new DateTime(1900, 1, 1).AddDays(int.Parse(@string) * 7);
			}
		}
		else
		{
			string[] array = new string[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				switch (list[i].Type)
				{
				case FormatInfoType.UserText:
					array[i] = "'" + list[i].Text + "'";
					break;
				case FormatInfoType.SimpleFormat:
					array[i] = list[i].Text;
					break;
				case FormatInfoType.CustomFormat:
					if (list.Count == 1 && list[i].Text.Length == 1)
					{
						array[i] = "%" + list[i].Text;
					}
					else
					{
						array[i] = list[i].Text;
					}
					break;
				}
			}
			try
			{
				if (!StringUtils.TryParseDateTimeExact(@string, string.Join("", array), PythonLocale.GetLocaleInfo(context).Time.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault, out result))
				{
					result = DateTime.Parse(@string, PythonLocale.GetLocaleInfo(context).Time.DateTimeFormat);
				}
			}
			catch (FormatException ex)
			{
				throw PythonOps.ValueError(ex.Message + Environment.NewLine + "data=" + @string + ", fmt=" + format + ", to: " + string.Join("", array));
			}
		}
		DayOfWeek? dayOfWeek = null;
		if ((found & FoundDateComponents.DayOfWeek) != FoundDateComponents.None)
		{
			dayOfWeek = result.DayOfWeek;
		}
		if ((found & FoundDateComponents.Year) == 0)
		{
			result = new DateTime(1900, result.Month, result.Day, result.Hour, result.Minute, result.Second, result.Millisecond, result.Kind);
		}
		return GetDateTimeTuple(result, dayOfWeek);
	}

	internal static string strftime(CodeContext context, string format, DateTime dt, int? microseconds)
	{
		bool postProcess;
		FoundDateComponents found;
		List<FormatInfo> list = PythonFormatToCLIFormat(format, forParse: false, out postProcess, out found);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			switch (list[i].Type)
			{
			case FormatInfoType.UserText:
				stringBuilder.Append(list[i].Text);
				break;
			case FormatInfoType.SimpleFormat:
				stringBuilder.Append(dt.ToString(list[i].Text, PythonLocale.GetLocaleInfo(context).Time.DateTimeFormat));
				break;
			case FormatInfoType.CustomFormat:
				stringBuilder.Append(dt.ToString("%" + list[i].Text, PythonLocale.GetLocaleInfo(context).Time.DateTimeFormat));
				break;
			}
		}
		if (postProcess)
		{
			stringBuilder = stringBuilder.Replace("%f", microseconds.HasValue ? $"{microseconds:D6}" : "");
			stringBuilder = stringBuilder.Replace("%j", dt.DayOfYear.ToString("D03"));
			DateTime dateTime = new DateTime(dt.Year, 1, 1);
			int num = (int)(7 - dateTime.DayOfWeek) % 7;
			int num2 = (int)(8 - dateTime.DayOfWeek) % 7;
			stringBuilder = stringBuilder.Replace("%U", ((dt.DayOfYear + 6 - num) / 7).ToString());
			stringBuilder = stringBuilder.Replace("%W", ((dt.DayOfYear + 6 - num2) / 7).ToString());
			stringBuilder = stringBuilder.Replace("%w", ((int)dt.DayOfWeek).ToString());
		}
		return stringBuilder.ToString();
	}

	internal static double DateTimeToTimestamp(DateTime dateTime)
	{
		return TicksToTimestamp(RemoveDst(dateTime).Ticks);
	}

	internal static DateTime TimestampToDateTime(double timeStamp)
	{
		return AddDst(new DateTime(TimestampToTicks(timeStamp)));
	}

	private static DateTime RemoveDst(DateTime dt)
	{
		return RemoveDst(dt, always: false);
	}

	private static DateTime RemoveDst(DateTime dt, bool always)
	{
		DaylightTime daylightChanges = TimeZone.CurrentTimeZone.GetDaylightChanges(dt.Year);
		if (always || (dt > daylightChanges.Start && dt < daylightChanges.End))
		{
			dt -= daylightChanges.Delta;
		}
		return dt;
	}

	private static DateTime AddDst(DateTime dt)
	{
		DaylightTime daylightChanges = TimeZone.CurrentTimeZone.GetDaylightChanges(dt.Year);
		if (dt > daylightChanges.Start && dt < daylightChanges.End)
		{
			dt += daylightChanges.Delta;
		}
		return dt;
	}

	private static double GetTimestampFromObject(object seconds)
	{
		if (Converter.TryConvertToInt32(seconds, out var result))
		{
			return result;
		}
		if (Converter.TryConvertToDouble(seconds, out var result2))
		{
			if (result2 > 9.223372036854776E+18 || result2 < -9.223372036854776E+18)
			{
				throw PythonOps.ValueError("unreasonable date/time");
			}
			return result2;
		}
		throw PythonOps.TypeError("expected int, got {0}", DynamicHelpers.GetPythonType(seconds));
	}

	private static void AddTime(List<FormatInfo> newFormat)
	{
		newFormat.Add(new FormatInfo("HH"));
		newFormat.Add(new FormatInfo(FormatInfoType.UserText, ":"));
		newFormat.Add(new FormatInfo("mm"));
		newFormat.Add(new FormatInfo(FormatInfoType.UserText, ":"));
		newFormat.Add(new FormatInfo("ss"));
	}

	private static void AddDate(List<FormatInfo> newFormat)
	{
		newFormat.Add(new FormatInfo("MM"));
		newFormat.Add(new FormatInfo(FormatInfoType.UserText, "/"));
		newFormat.Add(new FormatInfo("dd"));
		newFormat.Add(new FormatInfo(FormatInfoType.UserText, "/"));
		newFormat.Add(new FormatInfo("yy"));
	}

	private static List<FormatInfo> PythonFormatToCLIFormat(string format, bool forParse, out bool postProcess, out FoundDateComponents found)
	{
		postProcess = false;
		found = FoundDateComponents.None;
		List<FormatInfo> list = new List<FormatInfo>();
		for (int i = 0; i < format.Length; i++)
		{
			if (format[i] == '%')
			{
				if (i + 1 == format.Length)
				{
					throw PythonOps.ValueError("badly formatted string");
				}
				switch (format[++i])
				{
				case 'a':
					found |= FoundDateComponents.DayOfWeek;
					list.Add(new FormatInfo("ddd"));
					break;
				case 'A':
					found |= FoundDateComponents.DayOfWeek;
					list.Add(new FormatInfo("dddd"));
					break;
				case 'b':
					list.Add(new FormatInfo("MMM"));
					break;
				case 'B':
					list.Add(new FormatInfo("MMMM"));
					break;
				case 'c':
					found |= FoundDateComponents.Year;
					AddDate(list);
					list.Add(new FormatInfo(FormatInfoType.UserText, " "));
					AddTime(list);
					break;
				case 'd':
					if (forParse)
					{
						list.Add(new FormatInfo(FormatInfoType.CustomFormat, "d"));
					}
					else
					{
						list.Add(new FormatInfo("dd"));
					}
					break;
				case 'H':
					list.Add(new FormatInfo(forParse ? "H" : "HH"));
					break;
				case 'I':
					list.Add(new FormatInfo(forParse ? "h" : "hh"));
					break;
				case 'm':
					list.Add(new FormatInfo(forParse ? "M" : "MM"));
					break;
				case 'M':
					list.Add(new FormatInfo(forParse ? "m" : "mm"));
					break;
				case 'p':
					list.Add(new FormatInfo(FormatInfoType.CustomFormat, "t"));
					list.Add(new FormatInfo(FormatInfoType.UserText, "M"));
					break;
				case 'S':
					list.Add(new FormatInfo("ss"));
					break;
				case 'x':
					found |= FoundDateComponents.Year;
					AddDate(list);
					break;
				case 'X':
					AddTime(list);
					break;
				case 'y':
					found |= FoundDateComponents.Year;
					list.Add(new FormatInfo("yy"));
					break;
				case 'Y':
					found |= FoundDateComponents.Year;
					list.Add(new FormatInfo("yyyy"));
					break;
				case '%':
					list.Add(new FormatInfo("\\%"));
					break;
				case 'j':
					list.Add(new FormatInfo("\\%j"));
					postProcess = true;
					break;
				case 'f':
					postProcess = true;
					list.Add(new FormatInfo(FormatInfoType.UserText, "%f"));
					break;
				case 'W':
					list.Add(new FormatInfo("\\%W"));
					postProcess = true;
					break;
				case 'U':
					list.Add(new FormatInfo("\\%U"));
					postProcess = true;
					break;
				case 'w':
					list.Add(new FormatInfo("\\%w"));
					postProcess = true;
					break;
				case 'Z':
				case 'z':
					list.Add(new FormatInfo(FormatInfoType.UserText, ""));
					break;
				default:
					list.Add(new FormatInfo(FormatInfoType.UserText, ""));
					break;
				}
			}
			else if (list.Count == 0 || list[list.Count - 1].Type != FormatInfoType.UserText)
			{
				list.Add(new FormatInfo(FormatInfoType.UserText, format[i].ToString()));
			}
			else
			{
				list[list.Count - 1].Text = list[list.Count - 1].Text + format[i];
			}
		}
		return list;
	}

	internal static int Weekday(DateTime dt)
	{
		return Weekday(dt.DayOfWeek);
	}

	internal static int Weekday(DayOfWeek dayOfWeek)
	{
		if (dayOfWeek == DayOfWeek.Sunday)
		{
			return 6;
		}
		return (int)(dayOfWeek - 1);
	}

	internal static int IsoWeekday(DateTime dt)
	{
		if (dt.DayOfWeek == DayOfWeek.Sunday)
		{
			return 7;
		}
		return (int)dt.DayOfWeek;
	}

	internal static PythonTuple GetDateTimeTuple(DateTime dt)
	{
		return GetDateTimeTuple(dt, null);
	}

	internal static PythonTuple GetDateTimeTuple(DateTime dt, DayOfWeek? dayOfWeek)
	{
		return GetDateTimeTuple(dt, dayOfWeek, null);
	}

	internal static PythonTuple GetDateTimeTuple(DateTime dt, DayOfWeek? dayOfWeek, PythonDateTime.tzinfo tz)
	{
		int num = -1;
		if (tz == null)
		{
			num = -1;
		}
		else
		{
			PythonDateTime.timedelta timedelta = tz.dst(dt);
			PythonDateTime.ThrowIfInvalid(timedelta, "dst");
			num = ((timedelta != null) ? (timedelta.__nonzero__() ? 1 : 0) : (-1));
		}
		return new struct_time(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, Weekday(dayOfWeek ?? dt.DayOfWeek), dt.DayOfYear, num);
	}

	internal static struct_time GetDateTimeTuple(DateTime dt, bool dstMode)
	{
		return new struct_time(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, Weekday(dt), dt.DayOfYear, dstMode ? 1 : 0);
	}

	private static DateTime GetDateTimeFromTuple(CodeContext context, PythonTuple t)
	{
		int[] ints;
		DateTime dateTime = GetDateTimeFromTupleNoDst(context, t, out ints);
		if (ints != null)
		{
			switch (ints[8])
			{
			case -1:
				dateTime = RemoveDst(dateTime);
				break;
			case 1:
				dateTime = RemoveDst(dateTime, always: true);
				break;
			}
		}
		return dateTime;
	}

	private static DateTime GetDateTimeFromTupleNoDst(CodeContext context, PythonTuple t)
	{
		int[] ints;
		return GetDateTimeFromTupleNoDst(context, t, out ints);
	}

	private static DateTime GetDateTimeFromTupleNoDst(CodeContext context, PythonTuple t, out int[] ints)
	{
		if (t == null)
		{
			ints = null;
			return DateTime.Now;
		}
		ints = ValidateDateTimeTuple(context, t);
		return new DateTime(ints[0], ints[1], ints[2], ints[3], ints[4], ints[5]);
	}

	private static int[] ValidateDateTimeTuple(CodeContext context, PythonTuple t)
	{
		if (t.__len__() != 9)
		{
			throw PythonOps.TypeError("expected tuple of length {0}", 9);
		}
		int[] array = new int[9];
		for (int i = 0; i < 9; i++)
		{
			array[i] = PythonContext.GetContext(context).ConvertToInt32(t[i]);
		}
		int num = array[0];
		switch (num)
		{
		case 69:
		case 70:
		case 71:
		case 72:
		case 73:
		case 74:
		case 75:
		case 76:
		case 77:
		case 78:
		case 79:
		case 80:
		case 81:
		case 82:
		case 83:
		case 84:
		case 85:
		case 86:
		case 87:
		case 88:
		case 89:
		case 90:
		case 91:
		case 92:
		case 93:
		case 94:
		case 95:
		case 96:
		case 97:
		case 98:
		case 99:
			num += 1900;
			break;
		case 0:
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
		case 31:
		case 32:
		case 33:
		case 34:
		case 35:
		case 36:
		case 37:
		case 38:
		case 39:
		case 40:
		case 41:
		case 42:
		case 43:
		case 44:
		case 45:
		case 46:
		case 47:
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
		case 58:
		case 59:
		case 60:
		case 61:
		case 62:
		case 63:
		case 64:
		case 65:
		case 66:
		case 67:
		case 68:
			num += 2000;
			break;
		}
		if (num < DateTime.MinValue.Year || num <= 1900)
		{
			throw PythonOps.ValueError("year is too low");
		}
		if (num > DateTime.MaxValue.Year)
		{
			throw PythonOps.ValueError("year is too high");
		}
		if (array[6] < 0 || array[6] >= 7)
		{
			throw PythonOps.ValueError("day of week is outside of 0-6 range");
		}
		return array;
	}

	private static int FindFormat(List<FormatInfo> formatInfo, string format)
	{
		for (int i = 0; i < formatInfo.Count; i++)
		{
			if (formatInfo[i].Text == format)
			{
				return i;
			}
		}
		return -1;
	}

	private static void InitStopWatch()
	{
		if (sw == null)
		{
			sw = new Stopwatch();
			sw.Start();
		}
	}
}

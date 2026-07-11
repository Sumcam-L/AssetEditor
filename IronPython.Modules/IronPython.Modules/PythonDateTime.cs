using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public class PythonDateTime
{
	[PythonType]
	public class timedelta : ICodeFormattable
	{
		private const int MAXDAYS = 999999999;

		private const double SECONDSPERDAY = 86400.0;

		internal int _days;

		internal int _seconds;

		internal int _microseconds;

		private TimeSpan _tsWithDaysAndSeconds;

		private TimeSpan _tsWithSeconds;

		private bool _fWithDaysAndSeconds;

		private bool _fWithSeconds;

		internal static readonly timedelta _DayResolution = new timedelta(1.0, 0.0, 0.0);

		public static readonly timedelta resolution = new timedelta(0.0, 0.0, 1.0);

		public static readonly timedelta min = new timedelta(-999999999.0, 0.0, 0.0);

		public static readonly timedelta max = new timedelta(999999999.0, 86399.0, 999999.0);

		public int days => _days;

		public int seconds => _seconds;

		public int microseconds => _microseconds;

		internal TimeSpan TimeSpanWithDaysAndSeconds
		{
			get
			{
				if (!_fWithDaysAndSeconds)
				{
					_tsWithDaysAndSeconds = new TimeSpan(_days, 0, 0, _seconds);
					_fWithDaysAndSeconds = true;
				}
				return _tsWithDaysAndSeconds;
			}
		}

		internal TimeSpan TimeSpanWithSeconds
		{
			get
			{
				if (!_fWithSeconds)
				{
					_tsWithSeconds = TimeSpan.FromSeconds(_seconds);
					_fWithSeconds = true;
				}
				return _tsWithSeconds;
			}
		}

		internal timedelta(double days, double seconds, double microsecond)
			: this(days, seconds, microsecond, 0.0, 0.0, 0.0, 0.0)
		{
		}

		internal timedelta(TimeSpan ts, double microsecond)
			: this(ts.Days, ts.Seconds, microsecond, ts.Milliseconds, ts.Minutes, ts.Hours, 0.0)
		{
		}

		public timedelta(double days, double seconds, double microseconds, double milliseconds, double minutes, double hours, double weeks)
		{
			double num = weeks * 7.0 + days;
			double num2 = ((num * 24.0 + hours) * 60.0 + minutes) * 60.0 + seconds;
			double num3 = Math.Floor(num2);
			double num4 = num2 - num3;
			double num5 = Math.Round(num4 * 1000000.0 + milliseconds * 1000.0 + microseconds);
			double num6 = Math.Floor(num5 / 1000000.0);
			num3 += num6;
			num5 -= num6 * 1000000.0;
			if (num3 > 0.0 && num5 < 0.0)
			{
				num3 -= 1.0;
				num5 += 1000000.0;
			}
			_days = (int)(num3 / 86400.0);
			_seconds = (int)(num3 - (double)_days * 86400.0);
			if (_seconds < 0)
			{
				_days--;
				_seconds += 86400;
			}
			_microseconds = (int)num5;
			if (Math.Abs(_days) > 999999999)
			{
				throw PythonOps.OverflowError("days={0}; must have magnitude <= 999999999", _days);
			}
		}

		public static timedelta __new__(CodeContext context, PythonType cls, [DefaultParameterValue(0.0)] double days, [DefaultParameterValue(0.0)] double seconds, [DefaultParameterValue(0.0)] double microseconds, [DefaultParameterValue(0.0)] double milliseconds, [DefaultParameterValue(0.0)] double minutes, [DefaultParameterValue(0.0)] double hours, [DefaultParameterValue(0.0)] double weeks)
		{
			if (cls == DynamicHelpers.GetPythonTypeFromType(typeof(timedelta)))
			{
				return new timedelta(days, seconds, microseconds, milliseconds, minutes, hours, weeks);
			}
			if (!(cls.CreateInstance(context, days, seconds, microseconds, milliseconds, minutes, hours, weeks) is timedelta result))
			{
				throw PythonOps.TypeError("{0} is not a subclass of datetime.timedelta", cls);
			}
			return result;
		}

		public static timedelta operator +(timedelta self, timedelta other)
		{
			return new timedelta(self._days + other._days, self._seconds + other._seconds, self._microseconds + other._microseconds);
		}

		public static timedelta operator -(timedelta self, timedelta other)
		{
			return new timedelta(self._days - other._days, self._seconds - other._seconds, self._microseconds - other._microseconds);
		}

		public static timedelta operator -(timedelta self)
		{
			return new timedelta(-self._days, -self._seconds, -self._microseconds);
		}

		public static timedelta operator +(timedelta self)
		{
			return new timedelta(self._days, self._seconds, self._microseconds);
		}

		public static timedelta operator *(timedelta self, int other)
		{
			return new timedelta(self._days * other, self._seconds * other, self._microseconds * other);
		}

		public static timedelta operator *(int other, timedelta self)
		{
			return new timedelta(self._days * other, self._seconds * other, self._microseconds * other);
		}

		public static timedelta operator /(timedelta self, int other)
		{
			return new timedelta((double)self._days / (double)other, (double)self._seconds / (double)other, (double)self._microseconds / (double)other);
		}

		public static timedelta operator *(timedelta self, BigInteger other)
		{
			return self * (int)other;
		}

		public static timedelta operator *(BigInteger other, timedelta self)
		{
			return (int)other * self;
		}

		public static timedelta operator /(timedelta self, BigInteger other)
		{
			return self / (int)other;
		}

		public timedelta __pos__()
		{
			return +this;
		}

		public timedelta __neg__()
		{
			return -this;
		}

		public timedelta __abs__()
		{
			if (_days <= 0)
			{
				return -this;
			}
			return this;
		}

		[SpecialName]
		public timedelta FloorDivide(int y)
		{
			return this / y;
		}

		[SpecialName]
		public timedelta ReverseFloorDivide(int y)
		{
			return this / y;
		}

		public double total_seconds()
		{
			double num = (double)microseconds + ((double)seconds + (double)days * 24.0 * 3600.0) * 1000000.0;
			return num / 1000000.0;
		}

		public bool __nonzero__()
		{
			if (_days == 0 && _seconds == 0)
			{
				return _microseconds != 0;
			}
			return true;
		}

		public PythonTuple __reduce__()
		{
			return PythonTuple.MakeTuple(DynamicHelpers.GetPythonTypeFromType(GetType()), PythonTuple.MakeTuple(_days, _seconds, _microseconds));
		}

		public static object __getnewargs__(int days, int seconds, int microseconds)
		{
			return PythonTuple.MakeTuple(new timedelta(days, seconds, microseconds, 0.0, 0.0, 0.0, 0.0));
		}

		public override bool Equals(object obj)
		{
			if (!(obj is timedelta timedelta2))
			{
				return false;
			}
			if (_days == timedelta2._days && _seconds == timedelta2._seconds)
			{
				return _microseconds == timedelta2._microseconds;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _days ^ _seconds ^ _microseconds;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (_days != 0)
			{
				stringBuilder.Append(_days);
				if (Math.Abs(_days) == 1)
				{
					stringBuilder.Append(" day, ");
				}
				else
				{
					stringBuilder.Append(" days, ");
				}
			}
			stringBuilder.AppendFormat("{0}:{1:d2}:{2:d2}", TimeSpanWithSeconds.Hours, TimeSpanWithSeconds.Minutes, TimeSpanWithSeconds.Seconds);
			if (_microseconds != 0)
			{
				stringBuilder.AppendFormat(".{0:d6}", _microseconds);
			}
			return stringBuilder.ToString();
		}

		private int CompareTo(object other)
		{
			if (!(other is timedelta timedelta2))
			{
				throw PythonOps.TypeError("can't compare datetime.timedelta to {0}", PythonTypeOps.GetName(other));
			}
			int num = _days - timedelta2._days;
			if (num != 0)
			{
				return num;
			}
			num = _seconds - timedelta2._seconds;
			if (num != 0)
			{
				return num;
			}
			return _microseconds - timedelta2._microseconds;
		}

		public static bool operator >(timedelta self, object other)
		{
			return self.CompareTo(other) > 0;
		}

		public static bool operator <(timedelta self, object other)
		{
			return self.CompareTo(other) < 0;
		}

		public static bool operator >=(timedelta self, object other)
		{
			return self.CompareTo(other) >= 0;
		}

		public static bool operator <=(timedelta self, object other)
		{
			return self.CompareTo(other) <= 0;
		}

		public virtual string __repr__(CodeContext context)
		{
			if (_seconds == 0 && _microseconds == 0)
			{
				return $"datetime.timedelta({_days})";
			}
			if (_microseconds == 0)
			{
				return $"datetime.timedelta({_days}, {_seconds})";
			}
			return $"datetime.timedelta({_days}, {_seconds}, {_microseconds})";
		}
	}

	internal enum InputKind
	{
		Year,
		Month,
		Day,
		Hour,
		Minute,
		Second,
		Microsecond
	}

	[PythonType]
	public class date : ICodeFormattable
	{
		internal DateTime _dateTime;

		public static readonly date min = new date(new DateTime(1, 1, 1));

		public static readonly date max = new date(new DateTime(9999, 12, 31));

		public static readonly timedelta resolution = timedelta._DayResolution;

		public int year => _dateTime.Year;

		public int month => _dateTime.Month;

		public int day => _dateTime.Day;

		internal DateTime InternalDateTime
		{
			get
			{
				return _dateTime;
			}
			set
			{
				_dateTime = value;
			}
		}

		protected date()
		{
		}

		public date(int year, int month, int day)
		{
			ValidateInput(InputKind.Year, year);
			ValidateInput(InputKind.Month, month);
			ValidateInput(InputKind.Day, day);
			_dateTime = new DateTime(year, month, day);
		}

		internal date(DateTime value)
		{
			_dateTime = value.Date;
		}

		public static object today()
		{
			return new date(DateTime.Today);
		}

		public static date fromordinal(int d)
		{
			if (d < 1)
			{
				throw PythonOps.ValueError("ordinal must be >= 1");
			}
			return new date(min._dateTime.AddDays(d - 1));
		}

		public static date fromtimestamp(double timestamp)
		{
			DateTime dateTime = PythonTime.TimestampToDateTime(timestamp).AddSeconds(-PythonTime.timezone);
			return new date(dateTime.Year, dateTime.Month, dateTime.Day);
		}

		public static date operator +([NotNull] date self, [NotNull] timedelta other)
		{
			try
			{
				return new date(self._dateTime.AddDays(other.days));
			}
			catch
			{
				throw PythonOps.OverflowError("date value out of range");
			}
		}

		public static date operator +([NotNull] timedelta other, [NotNull] date self)
		{
			try
			{
				return new date(self._dateTime.AddDays(other.days));
			}
			catch
			{
				throw PythonOps.OverflowError("date value out of range");
			}
		}

		public static date operator -(date self, timedelta delta)
		{
			try
			{
				return new date(self._dateTime.AddDays(-1 * delta.days));
			}
			catch
			{
				throw PythonOps.OverflowError("date value out of range");
			}
		}

		public static timedelta operator -(date self, date other)
		{
			TimeSpan timeSpan = self._dateTime - other._dateTime;
			return new timedelta(0.0, timeSpan.TotalSeconds, timeSpan.Milliseconds * 1000);
		}

		public virtual PythonTuple __reduce__()
		{
			return PythonTuple.MakeTuple(DynamicHelpers.GetPythonTypeFromType(GetType()), PythonTuple.MakeTuple(_dateTime.Year, _dateTime.Month, _dateTime.Day));
		}

		public static object __getnewargs__(CodeContext context, int year, int month, int day)
		{
			return PythonTuple.MakeTuple(new date(year, month, day));
		}

		public object replace()
		{
			return this;
		}

		public virtual date replace(CodeContext context, [ParamDictionary] IDictionary<object, object> dict)
		{
			int num = _dateTime.Year;
			int num2 = _dateTime.Month;
			int num3 = _dateTime.Day;
			foreach (KeyValuePair<object, object> item in dict)
			{
				if (item.Key is string text)
				{
					switch (text)
					{
					case "year":
						num = PythonContext.GetContext(context).ConvertToInt32(item.Value);
						break;
					case "month":
						num2 = PythonContext.GetContext(context).ConvertToInt32(item.Value);
						break;
					case "day":
						num3 = PythonContext.GetContext(context).ConvertToInt32(item.Value);
						break;
					default:
						throw PythonOps.TypeError("{0} is an invalid keyword argument for this function", item.Key);
					}
				}
			}
			return new date(num, num2, num3);
		}

		public virtual object timetuple()
		{
			return PythonTime.GetDateTimeTuple(_dateTime);
		}

		public int toordinal()
		{
			return (_dateTime - min._dateTime).Days + 1;
		}

		public int weekday()
		{
			return PythonTime.Weekday(_dateTime);
		}

		public int isoweekday()
		{
			return PythonTime.IsoWeekday(_dateTime);
		}

		private DateTime FirstDayOfIsoYear(int year)
		{
			DateTime dateTime = new DateTime(year, 1, 1);
			DateTime result = dateTime;
			switch (dateTime.DayOfWeek)
			{
			case DayOfWeek.Sunday:
				result = dateTime.AddDays(1.0);
				break;
			case DayOfWeek.Monday:
			case DayOfWeek.Tuesday:
			case DayOfWeek.Wednesday:
			case DayOfWeek.Thursday:
				result = dateTime.AddDays(-1 * (int)(dateTime.DayOfWeek - 1));
				break;
			case DayOfWeek.Friday:
				result = dateTime.AddDays(3.0);
				break;
			case DayOfWeek.Saturday:
				result = dateTime.AddDays(2.0);
				break;
			}
			return result;
		}

		public PythonTuple isocalendar()
		{
			DateTime dateTime = FirstDayOfIsoYear(_dateTime.Year - 1);
			DateTime dateTime2 = FirstDayOfIsoYear(_dateTime.Year);
			DateTime dateTime3 = FirstDayOfIsoYear(_dateTime.Year + 1);
			int num;
			int days;
			if (dateTime2 <= _dateTime && _dateTime < dateTime3)
			{
				num = _dateTime.Year;
				days = (_dateTime - dateTime2).Days;
			}
			else if (_dateTime < dateTime2)
			{
				num = _dateTime.Year - 1;
				days = (_dateTime - dateTime).Days;
			}
			else
			{
				num = _dateTime.Year + 1;
				days = (_dateTime - dateTime3).Days;
			}
			return PythonTuple.MakeTuple(num, days / 7 + 1, days % 7 + 1);
		}

		public string isoformat()
		{
			return _dateTime.ToString("yyyy-MM-dd");
		}

		public override string ToString()
		{
			return isoformat();
		}

		public string ctime()
		{
			return _dateTime.ToString("ddd MMM ", CultureInfo.InvariantCulture) + string.Format(CultureInfo.InvariantCulture, "{0,2}", new object[1] { _dateTime.Day }) + _dateTime.ToString(" HH:mm:ss yyyy", CultureInfo.InvariantCulture);
		}

		public virtual string strftime(CodeContext context, string dateFormat)
		{
			return PythonTime.strftime(context, dateFormat, _dateTime, null);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is date date2 && !(obj is datetime))
			{
				return _dateTime == date2._dateTime;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _dateTime.GetHashCode();
		}

		internal virtual int CompareTo(object other)
		{
			date date2 = other as date;
			return _dateTime.CompareTo(date2._dateTime);
		}

		internal bool CheckType(object other)
		{
			return CheckType(other, shouldThrow: true);
		}

		internal bool CheckType(object other, bool shouldThrow)
		{
			if (other == null)
			{
				return CheckTypeError(other, shouldThrow);
			}
			if (other.GetType() != GetType())
			{
				if ((!(GetType() == typeof(date)) || !(other.GetType() == typeof(datetime))) && !((GetType() == typeof(datetime)) & (other.GetType() == typeof(date))) && PythonOps.HasAttr(DefaultContext.Default, other, "timetuple"))
				{
					return false;
				}
				return CheckTypeError(other, shouldThrow);
			}
			return true;
		}

		private static bool CheckTypeError(object other, bool shouldThrow)
		{
			if (shouldThrow)
			{
				throw PythonOps.TypeError("can't compare datetime.date to {0}", PythonTypeOps.GetName(other));
			}
			return true;
		}

		[return: MaybeNotImplemented]
		public static object operator >(date self, object other)
		{
			if (!self.CheckType(other))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(self.CompareTo(other) > 0);
		}

		[return: MaybeNotImplemented]
		public static object operator <(date self, object other)
		{
			if (!self.CheckType(other))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(self.CompareTo(other) < 0);
		}

		[return: MaybeNotImplemented]
		public static object operator >=(date self, object other)
		{
			if (!self.CheckType(other))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(self.CompareTo(other) >= 0);
		}

		[return: MaybeNotImplemented]
		public static object operator <=(date self, object other)
		{
			if (!self.CheckType(other))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(self.CompareTo(other) <= 0);
		}

		public object __eq__(object other)
		{
			if (!CheckType(other, shouldThrow: false))
			{
				return NotImplementedType.Value;
			}
			return Equals(other);
		}

		public object __ne__(object other)
		{
			if (!CheckType(other, shouldThrow: false))
			{
				return NotImplementedType.Value;
			}
			return !Equals(other);
		}

		public virtual string __repr__(CodeContext context)
		{
			return $"datetime.date({_dateTime.Year}, {_dateTime.Month}, {_dateTime.Day})";
		}

		public virtual string __format__(CodeContext context, string dateFormat)
		{
			if (string.IsNullOrEmpty(dateFormat))
			{
				return PythonOps.ToString(context, this);
			}
			return strftime(context, dateFormat);
		}
	}

	[PythonType]
	public class datetime : date, ICodeFormattable
	{
		private class UnifiedDateTime
		{
			public DateTime DateTime;

			public int LostMicroseconds;

			public override bool Equals(object obj)
			{
				if (!(obj is UnifiedDateTime unifiedDateTime))
				{
					return false;
				}
				if (DateTime == unifiedDateTime.DateTime)
				{
					return LostMicroseconds == unifiedDateTime.LostMicroseconds;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return DateTime.GetHashCode() ^ LostMicroseconds;
			}

			public int CompareTo(UnifiedDateTime other)
			{
				int num = DateTime.CompareTo(other.DateTime);
				if (num != 0)
				{
					return num;
				}
				return LostMicroseconds - other.LostMicroseconds;
			}
		}

		internal int _lostMicroseconds;

		internal tzinfo _tz;

		public new static readonly datetime max = new datetime(DateTime.MaxValue, 999, null);

		public new static readonly datetime min = new datetime(DateTime.MinValue, 0, null);

		public new static readonly timedelta resolution = timedelta.resolution;

		private UnifiedDateTime _utcDateTime;

		public int hour => base.InternalDateTime.Hour;

		public int minute => base.InternalDateTime.Minute;

		public int second => base.InternalDateTime.Second;

		public int microsecond => base.InternalDateTime.Millisecond * 1000 + _lostMicroseconds;

		public object tzinfo => _tz;

		private UnifiedDateTime UtcDateTime
		{
			get
			{
				if (_utcDateTime == null)
				{
					_utcDateTime = new UnifiedDateTime();
					_utcDateTime.DateTime = base.InternalDateTime;
					_utcDateTime.LostMicroseconds = _lostMicroseconds;
					timedelta timedelta2 = utcoffset();
					if (timedelta2 != null)
					{
						datetime datetime2 = this - timedelta2;
						_utcDateTime.DateTime = datetime2.InternalDateTime;
						_utcDateTime.LostMicroseconds = datetime2._lostMicroseconds;
					}
				}
				return _utcDateTime;
			}
		}

		public datetime(int year, int month, int day, [DefaultParameterValue(0)] int hour, [DefaultParameterValue(0)] int minute, [DefaultParameterValue(0)] int second, [DefaultParameterValue(0)] int microsecond, [DefaultParameterValue(null)] tzinfo tzinfo)
		{
			ValidateInput(InputKind.Year, year);
			ValidateInput(InputKind.Month, month);
			ValidateInput(InputKind.Day, day);
			ValidateInput(InputKind.Hour, hour);
			ValidateInput(InputKind.Minute, minute);
			ValidateInput(InputKind.Second, second);
			ValidateInput(InputKind.Microsecond, microsecond);
			base.InternalDateTime = new DateTime(year, month, day, hour, minute, second, microsecond / 1000);
			_lostMicroseconds = microsecond % 1000;
			_tz = tzinfo;
		}

		public datetime([NotNull] string str)
		{
			if (str.Length != 10)
			{
				throw PythonOps.TypeError("an integer is required");
			}
			int num = (int)(((uint)str[7] << 16) | ((uint)str[8] << 8) | str[9]);
			int num2 = str[2];
			if (num2 == 0 || num2 > 12)
			{
				throw PythonOps.TypeError("invalid month");
			}
			base.InternalDateTime = new DateTime((int)(((uint)str[0] << 8) | str[1]), num2, str[3], str[4], str[5], str[6], num / 1000);
			_lostMicroseconds = microsecond % 1000;
		}

		public datetime([NotNull] string str, [NotNull] tzinfo tzinfo)
			: this(str)
		{
			_tz = tzinfo;
		}

		private void Initialize(int year, int month, int day, int hour, int minute, int second, int microsecond, tzinfo tzinfo)
		{
		}

		public datetime(DateTime dt)
			: this(dt, 0, null)
		{
		}

		public datetime(params object[] args)
		{
			if (args.Length < 3)
			{
				throw PythonOps.TypeError("function takes at least 3 arguments ({0} given)", args.Length);
			}
			if (args.Length > 8)
			{
				throw PythonOps.TypeError("function takes at most 8 arguments ({0} given)", args.Length);
			}
			for (int i = 0; i < args.Length && i < 7; i++)
			{
				if (!(args[i] is int))
				{
					throw PythonOps.TypeError("an integer is required");
				}
			}
			if (args.Length > 7 && !(args[7] is tzinfo) && args[7] != null)
			{
				throw PythonOps.TypeError("tzinfo argument must be None or of a tzinfo subclass, not type '{0}'", PythonTypeOps.GetName(args[7]));
			}
			throw new InvalidOperationException();
		}

		internal datetime(DateTime dt, int lostMicroseconds, tzinfo tzinfo)
		{
			base.InternalDateTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
			_lostMicroseconds = dt.Millisecond * 1000 + lostMicroseconds;
			_tz = tzinfo;
			if (_lostMicroseconds < 0)
			{
				try
				{
					base.InternalDateTime = base.InternalDateTime.AddMilliseconds(_lostMicroseconds / 1000 - 1);
				}
				catch
				{
					throw PythonOps.OverflowError("date value out of range");
				}
				_lostMicroseconds = _lostMicroseconds % 1000 + 1000;
			}
			if (_lostMicroseconds > 999)
			{
				try
				{
					base.InternalDateTime = base.InternalDateTime.AddMilliseconds(_lostMicroseconds / 1000);
				}
				catch
				{
					throw PythonOps.OverflowError("date value out of range");
				}
				_lostMicroseconds %= 1000;
			}
		}

		public new static object today()
		{
			return new datetime(DateTime.Now, 0, null);
		}

		public static object now([DefaultParameterValue(null)] tzinfo tz)
		{
			if (tz != null)
			{
				return tz.fromutc(new datetime(DateTime.UtcNow, 0, tz));
			}
			return new datetime(DateTime.Now, 0, null);
		}

		public static object utcnow()
		{
			return new datetime(DateTime.UtcNow, 0, null);
		}

		public static object fromtimestamp(double timestamp, [DefaultParameterValue(null)] tzinfo tz)
		{
			DateTime dt = PythonTime.TimestampToDateTime(timestamp).AddSeconds(-PythonTime.timezone);
			if (tz != null)
			{
				dt = dt.ToUniversalTime();
				datetime dt2 = new datetime(dt, 0, tz);
				return tz.fromutc(dt2);
			}
			return new datetime(dt, 0, null);
		}

		public static datetime utcfromtimestamp(double timestamp)
		{
			DateTime dt = new DateTime(PythonTime.TimestampToTicks(timestamp), DateTimeKind.Utc);
			return new datetime(dt, 0, null);
		}

		public new static datetime fromordinal(int d)
		{
			if (d < 1)
			{
				throw PythonOps.ValueError("ordinal must be >= 1");
			}
			return new datetime(DateTime.MinValue + new TimeSpan(d - 1, 0, 0, 0), 0, null);
		}

		public static object combine(date date, time time)
		{
			return new datetime(date.year, date.month, date.day, time.hour, time.minute, time.second, time.microsecond, time.tzinfo);
		}

		public static datetime operator +([NotNull] datetime date, [NotNull] timedelta delta)
		{
			try
			{
				return new datetime(date.InternalDateTime.Add(delta.TimeSpanWithDaysAndSeconds), delta._microseconds + date._lostMicroseconds, date._tz);
			}
			catch (ArgumentException)
			{
				throw new OverflowException("date value out of range");
			}
		}

		public static datetime operator +([NotNull] timedelta delta, [NotNull] datetime date)
		{
			try
			{
				return new datetime(date.InternalDateTime.Add(delta.TimeSpanWithDaysAndSeconds), delta._microseconds + date._lostMicroseconds, date._tz);
			}
			catch (ArgumentException)
			{
				throw new OverflowException("date value out of range");
			}
		}

		public static datetime operator -(datetime date, timedelta delta)
		{
			return new datetime(date.InternalDateTime.Subtract(delta.TimeSpanWithDaysAndSeconds), date._lostMicroseconds - delta._microseconds, date._tz);
		}

		public static timedelta operator -(datetime date, datetime other)
		{
			if (CheckTzInfoBeforeCompare(date, other))
			{
				return new timedelta(date.InternalDateTime - other.InternalDateTime, date._lostMicroseconds - other._lostMicroseconds);
			}
			return new timedelta(date.UtcDateTime.DateTime - other.UtcDateTime.DateTime, date.UtcDateTime.LostMicroseconds - other.UtcDateTime.LostMicroseconds);
		}

		public date date()
		{
			return new date(base.year, base.month, base.day);
		}

		[Documentation("gets the datetime w/o the time zone component")]
		public time time()
		{
			return new time(hour, minute, second, microsecond, null);
		}

		public object timetz()
		{
			return new time(hour, minute, second, microsecond, _tz);
		}

		[Documentation("gets a new datetime object with the fields provided as keyword arguments replaced.")]
		public override date replace(CodeContext context, [ParamDictionary] IDictionary<object, object> dict)
		{
			int num = base.year;
			int num2 = base.month;
			int num3 = base.day;
			int num4 = hour;
			int num5 = minute;
			int num6 = second;
			int num7 = microsecond;
			tzinfo tzinfo2 = _tz;
			foreach (KeyValuePair<object, object> item in dict)
			{
				if (item.Key is string text)
				{
					switch (text)
					{
					case "year":
						num = (int)item.Value;
						break;
					case "month":
						num2 = (int)item.Value;
						break;
					case "day":
						num3 = (int)item.Value;
						break;
					case "hour":
						num4 = (int)item.Value;
						break;
					case "minute":
						num5 = (int)item.Value;
						break;
					case "second":
						num6 = (int)item.Value;
						break;
					case "microsecond":
						num7 = (int)item.Value;
						break;
					case "tzinfo":
						tzinfo2 = item.Value as tzinfo;
						break;
					default:
						throw PythonOps.TypeError("{0} is an invalid keyword argument for this function", item.Key);
					}
				}
			}
			return new datetime(num, num2, num3, num4, num5, num6, num7, tzinfo2);
		}

		public object astimezone(tzinfo tz)
		{
			if (tz == null)
			{
				throw PythonOps.TypeError("astimezone() argument 1 must be datetime.tzinfo, not None");
			}
			if (_tz == null)
			{
				throw PythonOps.ValueError("astimezone() cannot be applied to a naive datetime");
			}
			if (tz == _tz)
			{
				return this;
			}
			datetime datetime2 = this - utcoffset();
			datetime2._tz = tz;
			return tz.fromutc(datetime2);
		}

		public timedelta utcoffset()
		{
			if (_tz == null)
			{
				return null;
			}
			timedelta timedelta2 = _tz.utcoffset(this);
			ThrowIfInvalid(timedelta2, "utcoffset");
			return timedelta2;
		}

		public timedelta dst()
		{
			if (_tz == null)
			{
				return null;
			}
			timedelta timedelta2 = _tz.dst(this);
			ThrowIfInvalid(timedelta2, "dst");
			return timedelta2;
		}

		public object tzname()
		{
			if (_tz == null)
			{
				return null;
			}
			return _tz.tzname(this);
		}

		public override object timetuple()
		{
			return PythonTime.GetDateTimeTuple(base.InternalDateTime, null, _tz);
		}

		public object utctimetuple()
		{
			if (_tz == null)
			{
				return PythonTime.GetDateTimeTuple(base.InternalDateTime, dstMode: false);
			}
			datetime datetime2 = this - utcoffset();
			return PythonTime.GetDateTimeTuple(datetime2.InternalDateTime, dstMode: false);
		}

		public string isoformat([DefaultParameterValue('T')] char sep)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("{0:d4}-{1:d2}-{2:d2}{3}{4:d2}:{5:d2}:{6:d2}", base.year, base.month, base.day, sep, hour, minute, second);
			if (microsecond != 0)
			{
				stringBuilder.AppendFormat(".{0:d6}", microsecond);
			}
			timedelta timedelta2 = utcoffset();
			if (timedelta2 != null)
			{
				if (timedelta2.TimeSpanWithDaysAndSeconds >= TimeSpan.Zero)
				{
					stringBuilder.AppendFormat("+{0:d2}:{1:d2}", timedelta2.TimeSpanWithDaysAndSeconds.Hours, timedelta2.TimeSpanWithDaysAndSeconds.Minutes);
				}
				else
				{
					stringBuilder.AppendFormat("-{0:d2}:{1:d2}", -timedelta2.TimeSpanWithDaysAndSeconds.Hours, -timedelta2.TimeSpanWithDaysAndSeconds.Minutes);
				}
			}
			return stringBuilder.ToString();
		}

		internal static bool CheckTzInfoBeforeCompare(datetime self, datetime other)
		{
			if (self._tz != other._tz)
			{
				timedelta timedelta2 = self.utcoffset();
				timedelta timedelta3 = other.utcoffset();
				if ((timedelta2 == null && timedelta3 != null) || (timedelta2 != null && timedelta3 == null))
				{
					throw PythonOps.TypeError("can't compare offset-naive and offset-aware times");
				}
				return false;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is datetime datetime2))
			{
				return false;
			}
			if (CheckTzInfoBeforeCompare(this, datetime2))
			{
				if (base.InternalDateTime.Equals(datetime2.InternalDateTime))
				{
					return _lostMicroseconds == datetime2._lostMicroseconds;
				}
				return false;
			}
			if (Math.Abs((base.InternalDateTime - datetime2.InternalDateTime).TotalHours) > 48.0)
			{
				return false;
			}
			return UtcDateTime.Equals(datetime2.UtcDateTime);
		}

		public override int GetHashCode()
		{
			return UtcDateTime.DateTime.GetHashCode() ^ UtcDateTime.LostMicroseconds;
		}

		public override string ToString()
		{
			return isoformat(' ');
		}

		public override PythonTuple __reduce__()
		{
			return PythonTuple.MakeTuple(DynamicHelpers.GetPythonTypeFromType(GetType()), PythonTuple.MakeTuple(base.InternalDateTime.Year, base.InternalDateTime.Month, base.InternalDateTime.Day, base.InternalDateTime.Hour, base.InternalDateTime.Minute, base.InternalDateTime.Second, microsecond, tzinfo));
		}

		public override string strftime(CodeContext context, string dateFormat)
		{
			return PythonTime.strftime(context, dateFormat, _dateTime, _lostMicroseconds);
		}

		public static datetime strptime(CodeContext context, string date_string, string format)
		{
			if (!(PythonTime.strptime(context, date_string, format) is PythonTuple pythonTuple))
			{
				throw PythonOps.ValueError("time.strptime returned an invalid value.");
			}
			return new datetime((int)pythonTuple[0], (int)pythonTuple[1], (int)pythonTuple[2], (int)pythonTuple[3], (int)pythonTuple[4], (int)pythonTuple[5], 0, null);
		}

		internal override int CompareTo(object other)
		{
			if (other == null)
			{
				throw PythonOps.TypeError("can't compare datetime.datetime to NoneType");
			}
			if (!(other is datetime datetime2))
			{
				throw PythonOps.TypeError("can't compare datetime.datetime to {0}", PythonTypeOps.GetName(other));
			}
			if (CheckTzInfoBeforeCompare(this, datetime2))
			{
				int num = base.InternalDateTime.CompareTo(datetime2.InternalDateTime);
				if (num != 0)
				{
					return num;
				}
				return _lostMicroseconds - datetime2._lostMicroseconds;
			}
			TimeSpan timeSpan = base.InternalDateTime - datetime2.InternalDateTime;
			if (Math.Abs(timeSpan.TotalHours) > 48.0)
			{
				if (!(timeSpan > TimeSpan.Zero))
				{
					return -1;
				}
				return 1;
			}
			return UtcDateTime.CompareTo(datetime2.UtcDateTime);
		}

		public override string __repr__(CodeContext context)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("datetime.datetime({0}, {1}, {2}, {3}, {4}", base.InternalDateTime.Year, base.InternalDateTime.Month, base.InternalDateTime.Day, base.InternalDateTime.Hour, base.InternalDateTime.Minute);
			if (microsecond != 0)
			{
				stringBuilder.AppendFormat(", {0}, {1}", second, microsecond);
			}
			else if (second != 0)
			{
				stringBuilder.AppendFormat(", {0}", second);
			}
			if (_tz != null)
			{
				stringBuilder.AppendFormat(", tzinfo={0}", PythonOps.Repr(context, _tz));
			}
			stringBuilder.AppendFormat(")");
			return stringBuilder.ToString();
		}
	}

	[PythonType]
	public class time : ICodeFormattable
	{
		private class UnifiedTime
		{
			public TimeSpan TimeSpan;

			public int LostMicroseconds;

			public override bool Equals(object obj)
			{
				if (!(obj is UnifiedTime unifiedTime))
				{
					return false;
				}
				if (TimeSpan == unifiedTime.TimeSpan)
				{
					return LostMicroseconds == unifiedTime.LostMicroseconds;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return TimeSpan.GetHashCode() ^ LostMicroseconds;
			}

			public int CompareTo(UnifiedTime other)
			{
				int num = TimeSpan.CompareTo(other.TimeSpan);
				if (num != 0)
				{
					return num;
				}
				return LostMicroseconds - other.LostMicroseconds;
			}
		}

		internal TimeSpan _timeSpan;

		internal int _lostMicroseconds;

		internal tzinfo _tz;

		private UnifiedTime _utcTime;

		public static readonly time max = new time(23, 59, 59, 999999, null);

		public static readonly time min = new time(0, 0, 0, 0, null);

		public static readonly timedelta resolution = timedelta.resolution;

		public int hour => _timeSpan.Hours;

		public int minute => _timeSpan.Minutes;

		public int second => _timeSpan.Seconds;

		public int microsecond => _timeSpan.Milliseconds * 1000 + _lostMicroseconds;

		public tzinfo tzinfo => _tz;

		private UnifiedTime UtcTime
		{
			get
			{
				if (_utcTime == null)
				{
					_utcTime = new UnifiedTime();
					_utcTime.TimeSpan = _timeSpan;
					_utcTime.LostMicroseconds = _lostMicroseconds;
					timedelta timedelta2 = utcoffset();
					if (timedelta2 != null)
					{
						time time2 = Add(this, -timedelta2);
						_utcTime.TimeSpan = time2._timeSpan;
						_utcTime.LostMicroseconds = time2._lostMicroseconds;
					}
				}
				return _utcTime;
			}
		}

		public time([DefaultParameterValue(0)] int hour, [DefaultParameterValue(0)] int minute, [DefaultParameterValue(0)] int second, [DefaultParameterValue(0)] int microsecond, [DefaultParameterValue(null)] tzinfo tzinfo)
		{
			ValidateInput(InputKind.Hour, hour);
			ValidateInput(InputKind.Minute, minute);
			ValidateInput(InputKind.Second, second);
			ValidateInput(InputKind.Microsecond, microsecond);
			_timeSpan = new TimeSpan(0, hour, minute, second, microsecond / 1000);
			_lostMicroseconds = microsecond % 1000;
			_tz = tzinfo;
		}

		internal time(TimeSpan timeSpan, int lostMicroseconds, tzinfo tzinfo)
		{
			_timeSpan = timeSpan;
			_lostMicroseconds = lostMicroseconds;
			_tz = tzinfo;
		}

		private static time Add(time date, timedelta delta)
		{
			return new time(date._timeSpan.Add(delta.TimeSpanWithDaysAndSeconds), delta._microseconds + date._lostMicroseconds, date._tz);
		}

		public PythonTuple __reduce__()
		{
			return PythonTuple.MakeTuple(DynamicHelpers.GetPythonTypeFromType(GetType()), PythonTuple.MakeTuple(hour, minute, second, microsecond, tzinfo));
		}

		public bool __nonzero__()
		{
			if (UtcTime.TimeSpan.Ticks == 0)
			{
				return UtcTime.LostMicroseconds != 0;
			}
			return true;
		}

		public static explicit operator bool(time time)
		{
			return time.__nonzero__();
		}

		public object replace()
		{
			return this;
		}

		public object replace([ParamDictionary] IDictionary<object, object> dict)
		{
			int num = hour;
			int num2 = minute;
			int num3 = second;
			int num4 = microsecond;
			tzinfo tzinfo2 = tzinfo;
			foreach (KeyValuePair<object, object> item in dict)
			{
				if (item.Key is string text)
				{
					switch (text)
					{
					case "hour":
						num = (int)item.Value;
						break;
					case "minute":
						num2 = (int)item.Value;
						break;
					case "second":
						num3 = (int)item.Value;
						break;
					case "microsecond":
						num4 = (int)item.Value;
						break;
					case "tzinfo":
						tzinfo2 = item.Value as tzinfo;
						break;
					}
				}
			}
			return new time(num, num2, num3, num4, tzinfo2);
		}

		public object isoformat()
		{
			return ToString();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("{0:d2}:{1:d2}:{2:d2}", hour, minute, second);
			if (microsecond != 0)
			{
				stringBuilder.AppendFormat(".{0:d6}", microsecond);
			}
			timedelta timedelta2 = utcoffset();
			if (timedelta2 != null)
			{
				if (timedelta2.TimeSpanWithDaysAndSeconds >= TimeSpan.Zero)
				{
					stringBuilder.AppendFormat("+{0:d2}:{1:d2}", timedelta2.TimeSpanWithDaysAndSeconds.Hours, timedelta2.TimeSpanWithDaysAndSeconds.Minutes);
				}
				else
				{
					stringBuilder.AppendFormat("-{0:d2}:{1:d2}", -timedelta2.TimeSpanWithDaysAndSeconds.Hours, -timedelta2.TimeSpanWithDaysAndSeconds.Minutes);
				}
			}
			return stringBuilder.ToString();
		}

		public object strftime(CodeContext context, string format)
		{
			return PythonTime.strftime(context, format, new DateTime(1900, 1, 1, _timeSpan.Hours, _timeSpan.Minutes, _timeSpan.Seconds, _timeSpan.Milliseconds), _lostMicroseconds);
		}

		public timedelta utcoffset()
		{
			if (_tz == null)
			{
				return null;
			}
			timedelta timedelta2 = _tz.utcoffset(null);
			ThrowIfInvalid(timedelta2, "utcoffset");
			return timedelta2;
		}

		public object dst()
		{
			if (_tz == null)
			{
				return null;
			}
			timedelta timedelta2 = _tz.dst(null);
			ThrowIfInvalid(timedelta2, "dst");
			return timedelta2;
		}

		public object tzname()
		{
			if (_tz == null)
			{
				return null;
			}
			return _tz.tzname(null);
		}

		public override int GetHashCode()
		{
			return UtcTime.GetHashCode();
		}

		internal static bool CheckTzInfoBeforeCompare(time self, time other)
		{
			if (self._tz != other._tz)
			{
				timedelta timedelta2 = self.utcoffset();
				timedelta timedelta3 = other.utcoffset();
				if ((timedelta2 == null && timedelta3 != null) || (timedelta2 != null && timedelta3 == null))
				{
					throw PythonOps.TypeError("can't compare offset-naive and offset-aware times");
				}
				return false;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is time time2))
			{
				return false;
			}
			if (CheckTzInfoBeforeCompare(this, time2))
			{
				if (_timeSpan == time2._timeSpan)
				{
					return _lostMicroseconds == time2._lostMicroseconds;
				}
				return false;
			}
			return UtcTime.Equals(time2.UtcTime);
		}

		private int CompareTo(object other)
		{
			if (!(other is time time2))
			{
				throw PythonOps.TypeError("can't compare datetime.time to {0}", PythonTypeOps.GetName(other));
			}
			if (CheckTzInfoBeforeCompare(this, time2))
			{
				int num = _timeSpan.CompareTo(time2._timeSpan);
				if (num != 0)
				{
					return num;
				}
				return _lostMicroseconds - time2._lostMicroseconds;
			}
			return UtcTime.CompareTo(time2.UtcTime);
		}

		public static bool operator >(time self, object other)
		{
			return self.CompareTo(other) > 0;
		}

		public static bool operator <(time self, object other)
		{
			return self.CompareTo(other) < 0;
		}

		public static bool operator >=(time self, object other)
		{
			return self.CompareTo(other) >= 0;
		}

		public static bool operator <=(time self, object other)
		{
			return self.CompareTo(other) <= 0;
		}

		public virtual string __repr__(CodeContext context)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (microsecond != 0)
			{
				stringBuilder.AppendFormat("datetime.time({0}, {1}, {2}, {3}", hour, minute, second, microsecond);
			}
			else if (second != 0)
			{
				stringBuilder.AppendFormat("datetime.time({0}, {1}, {2}", hour, minute, second);
			}
			else
			{
				stringBuilder.AppendFormat("datetime.time({0}, {1}", hour, minute);
			}
			if (tzname() is string text)
			{
				stringBuilder.AppendFormat(", tzinfo={0}", text.ToLower());
			}
			stringBuilder.AppendFormat(")");
			return stringBuilder.ToString();
		}
	}

	[PythonType]
	public class tzinfo
	{
		public tzinfo()
		{
		}

		public tzinfo(params object[] args)
		{
		}

		public tzinfo([ParamDictionary] PythonDictionary dict, params object[] args)
		{
		}

		public virtual object fromutc(datetime dt)
		{
			timedelta timedelta2 = utcoffset(dt);
			if (timedelta2 == null)
			{
				throw PythonOps.ValueError("fromutc: non-None utcoffset() result required");
			}
			timedelta timedelta3 = dst(dt);
			if (timedelta3 == null)
			{
				throw PythonOps.ValueError("fromutc: non-None dst() result required");
			}
			timedelta timedelta4 = timedelta2 - timedelta3;
			dt += timedelta4;
			timedelta3 = dt.dst();
			return dt + timedelta3;
		}

		public virtual timedelta dst(object dt)
		{
			throw new NotImplementedException();
		}

		public virtual string tzname(object dt)
		{
			throw new NotImplementedException("a tzinfo subclass must implement tzname()");
		}

		public virtual timedelta utcoffset(object dt)
		{
			throw new NotImplementedException();
		}

		public PythonTuple __reduce__(CodeContext context)
		{
			if (GetType() == typeof(tzinfo) || !PythonOps.TryGetBoundAttr(context, this, "__dict__", out var ret))
			{
				return PythonTuple.MakeTuple(DynamicHelpers.GetPythonType(this), PythonTuple.EMPTY);
			}
			return PythonTuple.MakeTuple(DynamicHelpers.GetPythonType(this), PythonTuple.EMPTY, ret);
		}
	}

	public const string __doc__ = "Provides functions and types for working with dates and times.";

	public static readonly int MAXYEAR = DateTime.MaxValue.Year;

	public static readonly int MINYEAR = DateTime.MinValue.Year;

	internal static void ThrowIfInvalid(timedelta delta, string funcname)
	{
		if (delta != null)
		{
			if (delta._microseconds != 0 || delta._seconds % 60 != 0)
			{
				throw PythonOps.ValueError("tzinfo.{0}() must return a whole number of minutes", funcname);
			}
			int num = (int)(delta.TimeSpanWithDaysAndSeconds.TotalSeconds / 60.0);
			if (Math.Abs(num) >= 1440)
			{
				throw PythonOps.ValueError("tzinfo.{0}() returned {1}; must be in -1439 .. 1439", funcname, num);
			}
		}
	}

	internal static void ValidateInput(InputKind kind, int value)
	{
		switch (kind)
		{
		case InputKind.Year:
			if (value > DateTime.MaxValue.Year || value < DateTime.MinValue.Year)
			{
				throw PythonOps.ValueError("year is out of range");
			}
			break;
		case InputKind.Month:
			if (value > 12 || value < 1)
			{
				throw PythonOps.ValueError("month must be in 1..12");
			}
			break;
		case InputKind.Day:
			if (value > 31 || value < 1)
			{
				throw PythonOps.ValueError("day is out of range for month");
			}
			break;
		case InputKind.Hour:
			if (value > 23 || value < 0)
			{
				throw PythonOps.ValueError("hour must be in 0..23");
			}
			break;
		case InputKind.Minute:
			if (value > 59 || value < 0)
			{
				throw PythonOps.ValueError("minute must be in 0..59");
			}
			break;
		case InputKind.Second:
			if (value > 59 || value < 0)
			{
				throw PythonOps.ValueError("second must be in 0..59");
			}
			break;
		case InputKind.Microsecond:
			if (value > 999999 || value < 0)
			{
				throw PythonOps.ValueError("microsecond must be in 0..999999");
			}
			break;
		}
	}

	internal static bool IsNaiveTimeZone(tzinfo tz)
	{
		if (tz == null)
		{
			return true;
		}
		if (tz.utcoffset(null) == null)
		{
			return true;
		}
		return false;
	}
}

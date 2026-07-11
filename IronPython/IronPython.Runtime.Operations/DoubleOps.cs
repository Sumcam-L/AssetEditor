using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class DoubleOps
{
	internal const double PositiveZero = 0.0;

	internal const double NegativeZero = -0.0;

	private static Regex _fromHexRegex;

	private static char[] _whitespace = new char[6] { ' ', '\t', '\n', '\f', '\v', '\r' };

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls)
	{
		if (cls == TypeCache.Double)
		{
			return 0.0;
		}
		return cls.CreateInstance(context);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, object x)
	{
		object value = null;
		if (x is string)
		{
			value = ParseFloat((string)x);
		}
		else if (x is Extensible<string>)
		{
			if (!PythonTypeOps.TryInvokeUnaryOperator(context, x, "__float__", out value))
			{
				value = ParseFloat(((Extensible<string>)x).Value);
			}
		}
		else if (x is char)
		{
			value = ParseFloat(ScriptingRuntimeHelpers.CharToString((char)x));
		}
		else
		{
			if (x is Complex)
			{
				throw PythonOps.TypeError("can't convert complex to float; use abs(z)");
			}
			object obj = PythonOps.CallWithContext(context, PythonOps.GetBoundAttr(context, x, "__float__"));
			if (obj is double)
			{
				value = obj;
			}
			else
			{
				if (!(obj is Extensible<double>))
				{
					throw PythonOps.TypeError("__float__ returned non-float (type {0})", PythonTypeOps.GetName(obj));
				}
				value = ((Extensible<double>)obj).Value;
			}
		}
		if (cls == TypeCache.Double)
		{
			return value;
		}
		return cls.CreateInstance(context, value);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, IList<byte> s)
	{
		if (!(s is IPythonObject o) || !PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, "__float__", out var value))
		{
			value = ParseFloat(s.MakeString());
		}
		if (cls == TypeCache.Double)
		{
			return value;
		}
		return cls.CreateInstance(context, value);
	}

	public static PythonTuple as_integer_ratio(double self)
	{
		if (double.IsInfinity(self))
		{
			throw PythonOps.OverflowError("Cannot pass infinity to float.as_integer_ratio.");
		}
		if (double.IsNaN(self))
		{
			throw PythonOps.ValueError("Cannot pass nan to float.as_integer_ratio.");
		}
		BigInteger bigInteger = 1;
		while (self % 1.0 != 0.0)
		{
			self *= 2.0;
			bigInteger *= (BigInteger)2;
		}
		return PythonTuple.MakeTuple((BigInteger)self, bigInteger);
	}

	[StaticExtensionMethod]
	[ClassMethod]
	public static object fromhex(CodeContext context, PythonType cls, string self)
	{
		if (string.IsNullOrEmpty(self))
		{
			throw PythonOps.ValueError("expected non empty string");
		}
		self = self.Trim(_whitespace);
		double? num = TryParseSpecialFloat(self);
		if (num.HasValue)
		{
			return num.Value;
		}
		if (_fromHexRegex == null)
		{
			_fromHexRegex = new Regex("\\A\\s*(?<sign>[-+])?(?:0[xX])?(?<integer>[0-9a-fA-F]+)?(?<fraction>\\.[0-9a-fA-F]*)?(?<exponent>[pP][-+]?[0-9]+)?\\s*\\z");
		}
		Match match = _fromHexRegex.Match(self);
		if (!match.Success)
		{
			throw InvalidHexString();
		}
		Group obj = match.Groups["sign"];
		Group obj2 = match.Groups["integer"];
		Group obj3 = match.Groups["fraction"];
		Group obj4 = match.Groups["exponent"];
		bool flag = obj.Success && obj.Value == "-";
		BigInteger bigInteger = ((!obj2.Success) ? BigInteger.Zero : LiteralParser.ParseBigInteger(obj2.Value, 16));
		int num2 = 0;
		BigInteger bigInteger3;
		if (obj3.Success)
		{
			BigInteger bigInteger2 = 0;
			for (int i = 1; i < obj3.Value.Length; i++)
			{
				char c = obj3.Value[i];
				int num3;
				if (c >= '0' && c <= '9')
				{
					num3 = c - 48;
				}
				else if (c >= 'a' && c <= 'f')
				{
					num3 = 10 + c - 97;
				}
				else
				{
					if (c < 'A' || c > 'Z')
					{
						throw new InvalidOperationException();
					}
					num3 = 10 + c - 65;
				}
				bigInteger2 = (bigInteger2 << 4) | num3;
				num2 += 4;
			}
			bigInteger3 = (bigInteger << num2) | bigInteger2;
		}
		else
		{
			bigInteger3 = bigInteger;
		}
		if (obj4.Success)
		{
			int result = 0;
			if (!int.TryParse(obj4.Value.Substring(1), out result))
			{
				if (obj4.Value.lower().StartsWith("p-") || bigInteger3 == BigInteger.Zero)
				{
					double num4 = (flag ? -0.0 : 0.0);
					if (cls == TypeCache.Double)
					{
						return num4;
					}
					return PythonCalls.Call(cls, num4);
				}
				throw HexStringOverflow();
			}
			if (result > 0)
			{
				bigInteger3 <<= result;
			}
			else if (result < 0)
			{
				num2 -= result;
			}
		}
		if ((!obj4.Success && !obj3.Success && !obj2.Success) || (!obj2.Success && obj3.Length == 1))
		{
			throw PythonOps.ValueError("invalid hexidecimal floating point string '{0}'", self);
		}
		if (bigInteger3 == BigInteger.Zero)
		{
			if (flag)
			{
				return -0.0;
			}
			return 0.0;
		}
		int num5 = bigInteger3.GetBitCount();
		int j;
		for (j = num5 - num2 - 1; j < -1023; j++)
		{
			num5++;
		}
		if (j == -1023)
		{
			num5++;
		}
		int num6 = num5 - 53;
		bool flag2 = false;
		if (num6 > 0)
		{
			BigInteger bigInteger4 = bigInteger3 >> num6 - 1;
			if ((bigInteger4 & BigInteger.One) != BigInteger.Zero)
			{
				BigInteger bigInteger5 = bigInteger3 & ((BigInteger.One << num6 - 1) - 1);
				if (bigInteger5 != BigInteger.Zero || ((bigInteger3 >> num6) & BigInteger.One) != BigInteger.Zero)
				{
					BigInteger bigInteger6 = bigInteger4 + 1;
					bigInteger3 = (bigInteger6 >> 1) & 4503599627370495L;
					if (bigInteger6.GetBitCount() != bigInteger4.GetBitCount())
					{
						if (j != -1023)
						{
							bigInteger3 >>= 1;
							j++;
						}
						else if (bigInteger3 == BigInteger.Zero)
						{
							j++;
						}
					}
					flag2 = true;
				}
			}
		}
		if (!flag2)
		{
			bigInteger3 = (bigInteger3 >> num5 - 53) & 4503599627370495L;
		}
		if (j > 1023)
		{
			throw HexStringOverflow();
		}
		long num7 = (long)bigInteger3;
		num7 |= (long)j + 1023L << 52;
		if (flag)
		{
			num7 |= long.MinValue;
		}
		double num8 = BitConverter.Int64BitsToDouble(num7);
		if (cls == TypeCache.Double)
		{
			return num8;
		}
		return PythonCalls.Call(cls, num8);
	}

	private static double? TryParseSpecialFloat(string self)
	{
		switch (self.ToLower())
		{
		case "inf":
		case "+inf":
		case "infinity":
		case "+infinity":
			return double.PositiveInfinity;
		case "-inf":
		case "-infinity":
			return double.NegativeInfinity;
		case "nan":
		case "+nan":
		case "-nan":
			return double.NaN;
		default:
			return null;
		}
	}

	private static Exception HexStringOverflow()
	{
		return PythonOps.OverflowError("hexadecimal value too large to represent as a float");
	}

	private static Exception InvalidHexString()
	{
		return PythonOps.ValueError("invalid hexadecimal floating-point string");
	}

	public static string hex(double self)
	{
		if (double.IsPositiveInfinity(self))
		{
			return "inf";
		}
		if (double.IsNegativeInfinity(self))
		{
			return "-inf";
		}
		if (double.IsNaN(self))
		{
			return "nan";
		}
		ulong num = (ulong)BitConverter.DoubleToInt64Bits(self);
		int num2 = (int)((num >> 52) & 0x7FF) - 1023;
		long num3 = (long)(num & 0xFFFFFFFFFFFFFL);
		StringBuilder stringBuilder = new StringBuilder();
		if ((num & 0x8000000000000000uL) != 0)
		{
			stringBuilder.Append('-');
		}
		if (num2 == -1023)
		{
			stringBuilder.Append("0x0.");
			num2++;
		}
		else
		{
			stringBuilder.Append("0x1.");
		}
		stringBuilder.Append(StringFormatSpec.FromString("013").AlignNumericText(BigIntegerOps.AbsToHex(num3, lowercase: true), num3 == 0, isPos: true));
		stringBuilder.Append("p");
		if (num2 >= 0)
		{
			stringBuilder.Append('+');
		}
		stringBuilder.Append(num2.ToString());
		return stringBuilder.ToString();
	}

	public static bool is_integer(double self)
	{
		return self % 1.0 == 0.0;
	}

	private static double ParseFloat(string x)
	{
		try
		{
			double? num = TryParseSpecialFloat(x);
			if (num.HasValue)
			{
				return num.Value;
			}
			return LiteralParser.ParseFloat(x);
		}
		catch (FormatException)
		{
			throw PythonOps.ValueError("invalid literal for float(): {0}", x);
		}
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object DivMod(double x, double y)
	{
		object obj = FloorDivide(x, y);
		if (obj == NotImplementedType.Value)
		{
			return obj;
		}
		return PythonTuple.MakeTuple(obj, Mod(x, y));
	}

	[SpecialName]
	public static double Mod(double x, double y)
	{
		if (y == 0.0)
		{
			throw PythonOps.ZeroDivisionError();
		}
		double num = x % y;
		if (num > 0.0 && y < 0.0)
		{
			num += y;
		}
		else if (num < 0.0 && y > 0.0)
		{
			num += y;
		}
		return num;
	}

	[SpecialName]
	public static double Power(double x, double y)
	{
		if (x == 1.0 || y == 0.0)
		{
			return 1.0;
		}
		if (double.IsNaN(x) || double.IsNaN(y))
		{
			return double.NaN;
		}
		if (x == 0.0)
		{
			if (y > 0.0)
			{
				if (y % 2.0 == 1.0)
				{
					return x;
				}
				return 0.0;
			}
			if (y == 0.0)
			{
				return 1.0;
			}
			if (double.IsNegativeInfinity(y))
			{
				return double.PositiveInfinity;
			}
			throw PythonOps.ZeroDivisionError("0.0 cannot be raised to a negative power");
		}
		if (double.IsPositiveInfinity(y))
		{
			if (x > 1.0 || x < -1.0)
			{
				return double.PositiveInfinity;
			}
			if (x == -1.0)
			{
				return 1.0;
			}
			return 0.0;
		}
		if (double.IsNegativeInfinity(y))
		{
			if (x > 1.0 || x < -1.0)
			{
				return 0.0;
			}
			if (x == -1.0)
			{
				return 1.0;
			}
			return double.PositiveInfinity;
		}
		if (double.IsNegativeInfinity(x))
		{
			if (Math.Abs(y % 2.0) == 1.0)
			{
				if (!(y > 0.0))
				{
					return -0.0;
				}
				return double.NegativeInfinity;
			}
			if (!(y > 0.0))
			{
				return 0.0;
			}
			return double.PositiveInfinity;
		}
		if (x < 0.0 && Math.Floor(y) != y)
		{
			throw PythonOps.ValueError("negative number cannot be raised to fraction");
		}
		return PythonOps.CheckMath(x, y, Math.Pow(x, y));
	}

	public static PythonTuple __coerce__(CodeContext context, double x, object o)
	{
		double num = (double)__new__(context, TypeCache.Double, o);
		if (double.IsInfinity(num))
		{
			throw PythonOps.OverflowError("number too big");
		}
		return PythonTuple.MakeTuple(x, num);
	}

	public static object __int__(double d)
	{
		if (-2147483648.0 <= d && d <= 2147483647.0)
		{
			return (int)d;
		}
		if (double.IsInfinity(d))
		{
			throw PythonOps.OverflowError("cannot convert float infinity to integer");
		}
		if (double.IsNaN(d))
		{
			throw PythonOps.ValueError("cannot convert float NaN to integer");
		}
		return (BigInteger)d;
	}

	public static object __getnewargs__(CodeContext context, double self)
	{
		return PythonTuple.MakeTuple(__new__(context, TypeCache.Double, self));
	}

	public static string __str__(CodeContext context, double x)
	{
		StringFormatter stringFormatter = new StringFormatter(context, "%.12g", x);
		stringFormatter._TrailingZeroAfterWholeFloat = true;
		return stringFormatter.Format();
	}

	public static string __str__(double x, IFormatProvider provider)
	{
		return x.ToString(provider);
	}

	public static string __str__(double x, string format)
	{
		return x.ToString(format);
	}

	public static string __str__(double x, string format, IFormatProvider provider)
	{
		return x.ToString(format, provider);
	}

	public static int __hash__(double d)
	{
		if (d % 1.0 == 0.0)
		{
			if (-2147483648.0 <= d && d <= 2147483647.0)
			{
				return ((int)d).GetHashCode();
			}
			BigInteger self = (BigInteger)d;
			return BigIntegerOps.__hash__(self);
		}
		if (double.IsInfinity(d))
		{
			if (!(d > 0.0))
			{
				return -271828;
			}
			return 314159;
		}
		if (double.IsNaN(d))
		{
			return 0;
		}
		return d.GetHashCode();
	}

	[SpecialName]
	public static bool LessThan(double x, double y)
	{
		if (x < y && (!double.IsInfinity(x) || !double.IsNaN(y)))
		{
			if (double.IsNaN(x))
			{
				return !double.IsInfinity(y);
			}
			return true;
		}
		return false;
	}

	[SpecialName]
	public static bool LessThanOrEqual(double x, double y)
	{
		if (x == y)
		{
			return !double.IsNaN(x);
		}
		return x < y;
	}

	[SpecialName]
	public static bool GreaterThan(double x, double y)
	{
		if (x > y && (!double.IsInfinity(x) || !double.IsNaN(y)))
		{
			if (double.IsNaN(x))
			{
				return !double.IsInfinity(y);
			}
			return true;
		}
		return false;
	}

	[SpecialName]
	public static bool GreaterThanOrEqual(double x, double y)
	{
		if (x == y)
		{
			return !double.IsNaN(x);
		}
		return x > y;
	}

	[SpecialName]
	public static bool Equals(double x, double y)
	{
		if (x == y)
		{
			return !double.IsNaN(x);
		}
		return false;
	}

	[SpecialName]
	public static bool NotEquals(double x, double y)
	{
		if (x == y)
		{
			return double.IsNaN(x);
		}
		return true;
	}

	[SpecialName]
	public static bool LessThan(double x, BigInteger y)
	{
		return Compare(x, y) < 0;
	}

	[SpecialName]
	public static bool LessThanOrEqual(double x, BigInteger y)
	{
		return Compare(x, y) <= 0;
	}

	[SpecialName]
	public static bool GreaterThan(double x, BigInteger y)
	{
		return Compare(x, y) > 0;
	}

	[SpecialName]
	public static bool GreaterThanOrEqual(double x, BigInteger y)
	{
		return Compare(x, y) >= 0;
	}

	[SpecialName]
	public static bool Equals(double x, BigInteger y)
	{
		return Compare(x, y) == 0;
	}

	[SpecialName]
	public static bool NotEquals(double x, BigInteger y)
	{
		return Compare(x, y) != 0;
	}

	internal static bool IsPositiveZero(double value)
	{
		if (value == 0.0)
		{
			return double.IsPositiveInfinity(1.0 / value);
		}
		return false;
	}

	internal static bool IsNegativeZero(double value)
	{
		if (value == 0.0)
		{
			return double.IsNegativeInfinity(1.0 / value);
		}
		return false;
	}

	internal static int Sign(double value)
	{
		if (value == 0.0)
		{
			if (!double.IsPositiveInfinity(1.0 / value))
			{
				return -1;
			}
			return 1;
		}
		if (!(value > 0.0))
		{
			return -1;
		}
		return 1;
	}

	internal static int Compare(double x, double y)
	{
		if (double.IsInfinity(x) && double.IsNaN(y))
		{
			return 1;
		}
		if (double.IsNaN(x) && double.IsInfinity(y))
		{
			return -1;
		}
		if (!(x > y))
		{
			if (x != y)
			{
				return -1;
			}
			return 0;
		}
		return 1;
	}

	internal static int Compare(double x, BigInteger y)
	{
		return -Compare(y, x);
	}

	internal static int Compare(BigInteger x, double y)
	{
		if (double.IsNaN(y) || double.IsPositiveInfinity(y))
		{
			return -1;
		}
		if (y == double.NegativeInfinity)
		{
			return 1;
		}
		BigInteger bigInteger = (BigInteger)y;
		if (bigInteger == x)
		{
			double num = y % 1.0;
			if (num == 0.0)
			{
				return 0;
			}
			if (num > 0.0)
			{
				return -1;
			}
			return 1;
		}
		if (bigInteger > x)
		{
			return -1;
		}
		return 1;
	}

	[SpecialName]
	public static bool LessThan(double x, decimal y)
	{
		return Compare(x, y) < 0;
	}

	[SpecialName]
	public static bool LessThanOrEqual(double x, decimal y)
	{
		return Compare(x, y) <= 0;
	}

	[SpecialName]
	public static bool GreaterThan(double x, decimal y)
	{
		return Compare(x, y) > 0;
	}

	[SpecialName]
	public static bool GreaterThanOrEqual(double x, decimal y)
	{
		return Compare(x, y) >= 0;
	}

	[SpecialName]
	public static bool Equals(double x, decimal y)
	{
		return Compare(x, y) == 0;
	}

	[SpecialName]
	public static bool NotEquals(double x, decimal y)
	{
		return Compare(x, y) != 0;
	}

	internal static int Compare(double x, decimal y)
	{
		if (x > 7.922816251426434E+28)
		{
			return 1;
		}
		if (x < -7.922816251426434E+28)
		{
			return -1;
		}
		return ((decimal)x).CompareTo(y);
	}

	[SpecialName]
	public static bool LessThan(double x, int y)
	{
		return x < (double)y;
	}

	[SpecialName]
	public static bool LessThanOrEqual(double x, int y)
	{
		return x <= (double)y;
	}

	[SpecialName]
	public static bool GreaterThan(double x, int y)
	{
		return x > (double)y;
	}

	[SpecialName]
	public static bool GreaterThanOrEqual(double x, int y)
	{
		return x >= (double)y;
	}

	[SpecialName]
	public static bool Equals(double x, int y)
	{
		return x == (double)y;
	}

	[SpecialName]
	public static bool NotEquals(double x, int y)
	{
		return x != (double)y;
	}

	public static string __repr__(CodeContext context, double self)
	{
		if (double.IsNaN(self))
		{
			return "nan";
		}
		StringFormatter stringFormatter = new StringFormatter(context, "%.17g", self);
		stringFormatter._TrailingZeroAfterWholeFloat = true;
		string text = stringFormatter.Format();
		if (LiteralParser.ParseFloat(text) == self)
		{
			return text;
		}
		return self.ToString("R", CultureInfo.InvariantCulture);
	}

	public static BigInteger __long__(double self)
	{
		if (double.IsInfinity(self))
		{
			throw PythonOps.OverflowError("cannot convert float infinity to integer");
		}
		if (double.IsNaN(self))
		{
			throw PythonOps.ValueError("cannot convert float NaN to integer");
		}
		return (BigInteger)self;
	}

	public static double __float__(double self)
	{
		return self;
	}

	public static string __getformat__(CodeContext context, string typestr)
	{
		return (FloatFormat)(typestr switch
		{
			"float" => (int)PythonContext.GetContext(context).FloatFormat, 
			"double" => (int)PythonContext.GetContext(context).DoubleFormat, 
			_ => throw PythonOps.ValueError("__getformat__() argument 1 must be 'double' or 'float'"), 
		}) switch
		{
			FloatFormat.Unknown => "unknown", 
			FloatFormat.IEEE_BigEndian => "IEEE, big-endian", 
			FloatFormat.IEEE_LittleEndian => "IEEE, little-endian", 
			_ => DefaultFloatFormat(), 
		};
	}

	public static string __format__(CodeContext context, double self, [NotNull] string formatSpec)
	{
		StringFormatSpec stringFormatSpec = StringFormatSpec.FromString(formatSpec);
		string text = ((double.IsPositiveInfinity(self) || double.IsNegativeInfinity(self)) ? ((!((int?)stringFormatSpec.Type).HasValue || !char.IsUpper(stringFormatSpec.Type.Value)) ? "inf" : "INF") : ((!double.IsNaN(self)) ? DoubleToFormatString(context, self, stringFormatSpec) : ((!((int?)stringFormatSpec.Type).HasValue || !char.IsUpper(stringFormatSpec.Type.Value)) ? "nan" : "NAN")));
		if (!((int?)stringFormatSpec.Sign).HasValue)
		{
			return stringFormatSpec.AlignNumericText(text, isZero: false, double.IsNaN(self) || Sign(self) > 0);
		}
		return stringFormatSpec.AlignNumericText(text, isZero: false, double.IsNaN(self) || Sign(self) > 0);
	}

	private static string DoubleToFormatString(CodeContext context, double self, StringFormatSpec spec)
	{
		self = Math.Abs(self);
		int num = spec.Precision ?? 6;
		switch (spec.Type)
		{
		case '%':
		{
			string text4 = "0." + new string('0', num) + "%";
			if (spec.ThousandsComma)
			{
				text4 = "#," + text4;
			}
			return self.ToString(text4, CultureInfo.InvariantCulture);
		}
		case 'F':
		case 'f':
		{
			string text5 = "0." + new string('0', num);
			if (spec.ThousandsComma)
			{
				text5 = "#," + text5;
			}
			return self.ToString(text5, CultureInfo.InvariantCulture);
		}
		case 'E':
		case 'e':
		{
			string text3 = "0." + new string('0', num) + spec.Type + "+00";
			if (spec.ThousandsComma)
			{
				text3 = "#," + text3;
			}
			return self.ToString(text3, CultureInfo.InvariantCulture);
		}
		case null:
		case '\0':
			if (spec.Precision.HasValue)
			{
				int num7 = 1;
				double num8 = self;
				while (num8 >= 10.0)
				{
					num8 /= 10.0;
					num7++;
				}
				if (num7 > spec.Precision.Value && num7 != 1)
				{
					self = MathUtils.RoundAwayFromZero(self, 0);
					double num9 = Math.Pow(10.0, num7 - Math.Max(spec.Precision.Value, 1));
					self -= self % num9;
					string text2 = "0.0" + new string('#', spec.Precision.Value);
					return self.ToString(text2 + "e+00", CultureInfo.InvariantCulture);
				}
				int num10 = Math.Max(spec.Precision.Value - num7, 0);
				self = MathUtils.RoundAwayFromZero(self, num10);
				return self.ToString("0.0" + new string('#', num10));
			}
			if (IncludeExponent(self))
			{
				return self.ToString("0.#e+00", CultureInfo.InvariantCulture);
			}
			if (spec.ThousandsComma)
			{
				return self.ToString("#,0.0###", CultureInfo.InvariantCulture);
			}
			return self.ToString("0.0###", CultureInfo.InvariantCulture);
		case 'G':
		case 'g':
		case 'n':
		{
			int num2 = 1;
			double num3 = self;
			while (num3 >= 10.0)
			{
				num3 /= 10.0;
				num2++;
			}
			if (num2 > num && num2 != 1)
			{
				self = MathUtils.RoundAwayFromZero(self, 0);
				double num4 = Math.Pow(10.0, num2 - Math.Max(num, 1));
				double num5 = self / num4;
				self -= self % num4;
				if (num5 % 1.0 >= 0.5)
				{
					self += num4;
				}
				char? type = spec.Type;
				string text = ((type == 'n' && type.HasValue && PythonContext.GetContext(context).NumericCulture != PythonContext.CCulture) ? "0" : ((!(spec.Precision > 1) && num2 <= 6) ? "0" : ("0.#" + new string('#', num))));
				if (spec.ThousandsComma)
				{
					text = "#," + text;
				}
				return self.ToString(text + ((spec.Type == 'G') ? "E+00" : "e+00"), CultureInfo.InvariantCulture);
			}
			if (self < 1.0)
			{
				num2--;
			}
			int num6 = Math.Max(num - num2, 0);
			self = MathUtils.RoundAwayFromZero(self, num6);
			char? type2 = spec.Type;
			if (type2 == 'n' && type2.HasValue && PythonContext.GetContext(context).NumericCulture != PythonContext.CCulture)
			{
				if (num2 != num && self % 1.0 != 0.0)
				{
					return self.ToString("#,0.0" + new string('#', num6));
				}
				return self.ToString("#,0");
			}
			if (num2 != num && self % 1.0 != 0.0)
			{
				return self.ToString("0.0" + new string('#', num6));
			}
			return self.ToString("0");
		}
		default:
			throw PythonOps.ValueError("Unknown format code '{0}' for object of type 'float'", spec.Type.ToString());
		}
	}

	private static bool IncludeExponent(double self)
	{
		if (!(self >= 1000000000000.0))
		{
			if (self != 0.0)
			{
				return self <= 9E-05;
			}
			return false;
		}
		return true;
	}

	private static string DefaultFloatFormat()
	{
		if (BitConverter.IsLittleEndian)
		{
			return "IEEE, little-endian";
		}
		return "IEEE, big-endian";
	}

	public static void __setformat__(CodeContext context, string typestr, string fmt)
	{
		FloatFormat floatFormat;
		switch (fmt)
		{
		case "unknown":
			floatFormat = FloatFormat.Unknown;
			break;
		case "IEEE, little-endian":
			if (!BitConverter.IsLittleEndian)
			{
				throw PythonOps.ValueError("can only set double format to 'unknown' or the detected platform value");
			}
			floatFormat = FloatFormat.IEEE_LittleEndian;
			break;
		case "IEEE, big-endian":
			if (BitConverter.IsLittleEndian)
			{
				throw PythonOps.ValueError("can only set double format to 'unknown' or the detected platform value");
			}
			floatFormat = FloatFormat.IEEE_BigEndian;
			break;
		default:
			throw PythonOps.ValueError(" __setformat__() argument 2 must be 'unknown', 'IEEE, little-endian' or 'IEEE, big-endian'");
		}
		switch (typestr)
		{
		case "float":
			PythonContext.GetContext(context).FloatFormat = floatFormat;
			break;
		case "double":
			PythonContext.GetContext(context).DoubleFormat = floatFormat;
			break;
		default:
			throw PythonOps.ValueError("__setformat__() argument 1 must be 'double' or 'float'");
		}
	}

	[SpecialName]
	public static double Plus(double x)
	{
		return x;
	}

	[SpecialName]
	public static double Negate(double x)
	{
		return 0.0 - x;
	}

	[SpecialName]
	public static double Abs(double x)
	{
		return Math.Abs(x);
	}

	public static bool __nonzero__(double x)
	{
		return x != 0.0;
	}

	public static object __trunc__(double x)
	{
		if (x >= 2147483647.0 || x <= -2147483648.0)
		{
			return (BigInteger)x;
		}
		return (int)x;
	}

	[SpecialName]
	public static double Add(double x, double y)
	{
		return x + y;
	}

	[SpecialName]
	public static double Subtract(double x, double y)
	{
		return x - y;
	}

	[SpecialName]
	public static double Multiply(double x, double y)
	{
		return x * y;
	}

	[SpecialName]
	public static double Divide(double x, double y)
	{
		return TrueDivide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(double x, double y)
	{
		if (y == 0.0)
		{
			throw PythonOps.ZeroDivisionError();
		}
		return x / y;
	}

	[SpecialName]
	public static double FloorDivide(double x, double y)
	{
		if (y == 0.0)
		{
			throw PythonOps.ZeroDivisionError();
		}
		return Math.Floor(x / y);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static sbyte ConvertToSByte(double x)
	{
		if (-128.0 <= x && x <= 127.0)
		{
			return (sbyte)x;
		}
		throw Converter.CannotConvertOverflow("SByte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static byte ConvertToByte(double x)
	{
		if (0.0 <= x && x <= 255.0)
		{
			return (byte)x;
		}
		throw Converter.CannotConvertOverflow("Byte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static short ConvertToInt16(double x)
	{
		if (-32768.0 <= x && x <= 32767.0)
		{
			return (short)x;
		}
		throw Converter.CannotConvertOverflow("Int16", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ushort ConvertToUInt16(double x)
	{
		if (0.0 <= x && x <= 65535.0)
		{
			return (ushort)x;
		}
		throw Converter.CannotConvertOverflow("UInt16", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static int ConvertToInt32(double x)
	{
		if (-2147483648.0 <= x && x <= 2147483647.0)
		{
			return (int)x;
		}
		throw Converter.CannotConvertOverflow("Int32", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static uint ConvertToUInt32(double x)
	{
		if (0.0 <= x && x <= 4294967295.0)
		{
			return (uint)x;
		}
		throw Converter.CannotConvertOverflow("UInt32", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static long ConvertToInt64(double x)
	{
		if (-9.223372036854776E+18 <= x && x <= 9.223372036854776E+18)
		{
			return (long)x;
		}
		throw Converter.CannotConvertOverflow("Int64", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ulong ConvertToUInt64(double x)
	{
		if (0.0 <= x && x <= 1.8446744073709552E+19)
		{
			return (ulong)x;
		}
		throw Converter.CannotConvertOverflow("UInt64", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static float ConvertToSingle(double x)
	{
		return (float)x;
	}

	[SpecialName]
	[PropertyMethod]
	public static double Getreal(double x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static double Getimag(double x)
	{
		return 0.0;
	}

	public static double conjugate(double x)
	{
		return x;
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class Int32Ops
{
	private static object FastNew(CodeContext context, object o)
	{
		if (o is string)
		{
			return __new__(null, (string)o, 10);
		}
		if (o is double)
		{
			return DoubleOps.__int__((double)o);
		}
		if (o is int)
		{
			return o;
		}
		if (o is bool)
		{
			return ((bool)o) ? 1 : 0;
		}
		if (o is BigInteger)
		{
			if (((BigInteger)o).AsInt32(out var ret))
			{
				return ScriptingRuntimeHelpers.Int32ToObject(ret);
			}
			return o;
		}
		if (o is Extensible<BigInteger> extensible)
		{
			if (extensible.Value.AsInt32(out var ret2))
			{
				return ScriptingRuntimeHelpers.Int32ToObject(ret2);
			}
			return extensible.Value;
		}
		if (o is float)
		{
			return DoubleOps.__int__((float)o);
		}
		if (o is Complex)
		{
			throw PythonOps.TypeError("can't convert complex to int; use int(abs(z))");
		}
		if (o is long num)
		{
			if (int.MinValue <= num && num <= int.MaxValue)
			{
				return (int)num;
			}
			return (BigInteger)num;
		}
		if (o is uint num2)
		{
			if (num2 <= int.MaxValue)
			{
				return (int)num2;
			}
			return (BigInteger)num2;
		}
		if (o is ulong num3)
		{
			if (num3 <= int.MaxValue)
			{
				return (int)num3;
			}
			return (BigInteger)num3;
		}
		if (o is decimal num4)
		{
			if (-2147483648m <= num4 && num4 <= 2147483647m)
			{
				return (int)num4;
			}
			return (BigInteger)num4;
		}
		if (o is Enum)
		{
			return ((IConvertible)o).ToInt32(null);
		}
		if (o is Extensible<string> extensible2)
		{
			if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, extensible2, "__int__", out var value))
			{
				return value;
			}
			return __new__(null, extensible2.Value, 10);
		}
		if (PythonTypeOps.TryInvokeUnaryOperator(context, o, "__int__", out var value2) && !object.ReferenceEquals(value2, NotImplementedType.Value))
		{
			if (value2 is int || value2 is BigInteger || value2 is Extensible<int> || value2 is Extensible<BigInteger>)
			{
				return value2;
			}
			throw PythonOps.TypeError("__int__ returned non-Integral (type {0})", PythonTypeOps.GetOldName(value2));
		}
		if (PythonOps.TryGetBoundAttr(context, o, "__trunc__", out value2))
		{
			value2 = PythonOps.CallWithContext(context, value2);
			if (value2 is int || value2 is BigInteger || value2 is Extensible<int> || value2 is Extensible<BigInteger>)
			{
				return value2;
			}
			if (Converter.TryConvertToInt32(value2, out var result))
			{
				return result;
			}
			if (Converter.TryConvertToBigInteger(value2, out var result2))
			{
				return result2;
			}
			throw PythonOps.TypeError("__trunc__ returned non-Integral (type {0})", PythonTypeOps.GetOldName(value2));
		}
		if (o is OldInstance)
		{
			throw PythonOps.AttributeError("{0} instance has no attribute '__trunc__'", PythonTypeOps.GetOldName((OldInstance)o));
		}
		throw PythonOps.TypeError("int() argument must be a string or a number, not '{0}'", PythonTypeOps.GetName(o));
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, object o)
	{
		return __new__(context, TypeCache.Int32, o);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, Extensible<double> o)
	{
		PythonTypeOps.TryInvokeUnaryOperator(context, o, "__int__", out var value);
		if (cls == TypeCache.Int32)
		{
			return (int)value;
		}
		return cls.CreateInstance(context, value);
	}

	private static void ValidateType(PythonType cls)
	{
		if (cls == TypeCache.Boolean)
		{
			throw PythonOps.TypeError("int.__new__(bool) is not safe, use bool.__new__()");
		}
	}

	[StaticExtensionMethod]
	public static object __new__(PythonType cls, string s, int radix)
	{
		ValidateType(cls);
		if (radix == 16 || radix == 8 || radix == 2)
		{
			s = TrimRadix(s, radix);
		}
		return LiteralParser.ParseIntegerSign(s, radix);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, IList<byte> s)
	{
		if (!(s is IPythonObject o) || !PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, "__int__", out var value))
		{
			value = FastNew(context, s.MakeString());
		}
		if (cls == TypeCache.Int32)
		{
			return value;
		}
		ValidateType(cls);
		return cls.CreateInstance(context, value);
	}

	internal static string TrimRadix(string s, int radix)
	{
		for (int i = 0; i < s.Length; i++)
		{
			if (char.IsWhiteSpace(s[i]))
			{
				continue;
			}
			if (s[i] != '0' || i >= s.Length - 1)
			{
				break;
			}
			switch (radix)
			{
			case 16:
				if (s[i + 1] == 'x' || s[i + 1] == 'X')
				{
					s = s.Substring(i + 2);
				}
				break;
			case 8:
				if (s[i + 1] == 'o' || s[i + 1] == 'O')
				{
					s = s.Substring(i + 2);
				}
				break;
			case 2:
				if (s[i + 1] == 'b' || s[i + 1] == 'B')
				{
					s = s.Substring(i + 2);
				}
				break;
			}
			break;
		}
		return s;
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, object x)
	{
		object obj = FastNew(context, x);
		if (cls == TypeCache.Int32)
		{
			return obj;
		}
		ValidateType(cls);
		return cls.CreateInstance(context, obj);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls)
	{
		if (cls == TypeCache.Int32)
		{
			return 0;
		}
		return cls.CreateInstance(context);
	}

	[SpecialName]
	public static object FloorDivide(int x, int y)
	{
		if (y == -1 && x == int.MinValue)
		{
			return -(BigInteger)int.MinValue;
		}
		return ScriptingRuntimeHelpers.Int32ToObject(MathUtils.FloorDivideUnchecked(x, y));
	}

	[SpecialName]
	public static int Mod(int x, int y)
	{
		return MathUtils.FloorRemainder(x, y);
	}

	[SpecialName]
	public static object Power(int x, BigInteger power, BigInteger qmod)
	{
		return BigIntegerOps.Power(x, power, qmod);
	}

	[SpecialName]
	public static object Power(int x, double power, double qmod)
	{
		return NotImplementedType.Value;
	}

	[SpecialName]
	public static object Power(int x, int power, int? qmod)
	{
		if (!qmod.HasValue)
		{
			return Power(x, power);
		}
		int value = qmod.Value;
		if (power < 0)
		{
			throw PythonOps.TypeError("power", power, "power must be >= 0");
		}
		if (value == 0)
		{
			throw PythonOps.ZeroDivisionError();
		}
		long num = 1 % value;
		long num2 = x;
		while (power != 0)
		{
			if ((power & 1) != 0)
			{
				num = num * num2 % value;
			}
			num2 = num2 * num2 % value;
			power >>= 1;
		}
		if ((value < 0 && num > 0) || (value > 0 && num < 0))
		{
			num += value;
		}
		return (int)num;
	}

	[SpecialName]
	public static object Power(int x, int power)
	{
		if (power == 0)
		{
			return 1;
		}
		if (power < 0)
		{
			if (x == 0)
			{
				throw PythonOps.ZeroDivisionError("0.0 cannot be raised to a negative power");
			}
			return DoubleOps.Power(x, power);
		}
		int num = x;
		int num2 = 1;
		int y = power;
		checked
		{
			try
			{
				while (power != 0)
				{
					if ((power & 1) != 0)
					{
						num2 *= num;
					}
					if (power == 1)
					{
						break;
					}
					num *= num;
					power >>= 1;
				}
				return num2;
			}
			catch (OverflowException)
			{
				return BigIntegerOps.Power(x, y);
			}
		}
	}

	[SpecialName]
	public static object LeftShift(int x, int y)
	{
		if (y < 0)
		{
			throw PythonOps.ValueError("negative shift count");
		}
		if (y > 31 || (x > 0 && x > int.MaxValue >> y) || (x < 0 && x < int.MinValue >> y))
		{
			return Int64Ops.LeftShift(x, y);
		}
		return ScriptingRuntimeHelpers.Int32ToObject(x << y);
	}

	[SpecialName]
	public static int RightShift(int x, int y)
	{
		if (y < 0)
		{
			throw PythonOps.ValueError("negative shift count");
		}
		if (y > 31)
		{
			if (x < 0)
			{
				return -1;
			}
			return 0;
		}
		int num;
		if (x >= 0)
		{
			num = x >> y;
		}
		else
		{
			num = x + ((1 << y) - 1) >> y;
			if (x - (num << y) != 0)
			{
				num--;
			}
		}
		return num;
	}

	public static PythonTuple __divmod__(int x, int y)
	{
		return PythonTuple.MakeTuple(Divide(x, y), Mod(x, y));
	}

	[return: MaybeNotImplemented]
	public static object __divmod__(int x, object y)
	{
		return NotImplementedType.Value;
	}

	public static string __oct__(int x)
	{
		if (x == 0)
		{
			return "0";
		}
		if (x > 0)
		{
			return "0" + MathUtils.ToString(x, 8);
		}
		return "-0" + MathUtils.ToString(-x, 8);
	}

	public static string __hex__(int x)
	{
		if (x < 0)
		{
			return "-0x" + (-x).ToString("x");
		}
		return "0x" + x.ToString("x");
	}

	public static object __getnewargs__(CodeContext context, int self)
	{
		return PythonTuple.MakeTuple(__new__(context, TypeCache.Int32, self));
	}

	public static object __rdivmod__(int x, int y)
	{
		return __divmod__(y, x);
	}

	public static int __int__(int self)
	{
		return self;
	}

	public static int __index__(int self)
	{
		return self;
	}

	public static BigInteger __long__(int self)
	{
		return self;
	}

	public static double __float__(int self)
	{
		return self;
	}

	public static int __abs__(int self)
	{
		return Math.Abs(self);
	}

	public static object __coerce__(CodeContext context, int x, object o)
	{
		if (o is int)
		{
			return PythonTuple.MakeTuple(ScriptingRuntimeHelpers.Int32ToObject(x), o);
		}
		return NotImplementedType.Value;
	}

	public static string __format__(CodeContext context, int self, [NotNull] string formatSpec)
	{
		StringFormatSpec stringFormatSpec = StringFormatSpec.FromString(formatSpec);
		if (stringFormatSpec.Precision.HasValue)
		{
			throw PythonOps.ValueError("Precision not allowed in integer format specifier");
		}
		string text;
		switch (stringFormatSpec.Type)
		{
		case 'n':
		{
			CultureInfo numericCulture = PythonContext.GetContext(context).NumericCulture;
			if (numericCulture != CultureInfo.InvariantCulture)
			{
				text = self.ToString("N0", PythonContext.GetContext(context).NumericCulture);
				break;
			}
			goto case null;
		}
		case null:
		case 'd':
			text = ((!stringFormatSpec.ThousandsComma) ? self.ToString("D", CultureInfo.InvariantCulture) : self.ToString("#,0", CultureInfo.InvariantCulture));
			break;
		case '%':
			text = ((!stringFormatSpec.ThousandsComma) ? self.ToString("0.000000%", CultureInfo.InvariantCulture) : self.ToString("#,0.000000%", CultureInfo.InvariantCulture));
			break;
		case 'e':
			text = ((!stringFormatSpec.ThousandsComma) ? self.ToString("0.000000e+00", CultureInfo.InvariantCulture) : self.ToString("#,0.000000e+00", CultureInfo.InvariantCulture));
			break;
		case 'E':
			text = ((!stringFormatSpec.ThousandsComma) ? self.ToString("0.000000E+00", CultureInfo.InvariantCulture) : self.ToString("#,0.000000E+00", CultureInfo.InvariantCulture));
			break;
		case 'F':
		case 'f':
			text = ((!stringFormatSpec.ThousandsComma) ? self.ToString("#########0.000000", CultureInfo.InvariantCulture) : self.ToString("#,########0.000000", CultureInfo.InvariantCulture));
			break;
		case 'g':
			text = ((self < 1000000 && self > -1000000) ? ((!stringFormatSpec.ThousandsComma) ? self.ToString(CultureInfo.InvariantCulture) : self.ToString("#,0", CultureInfo.InvariantCulture)) : self.ToString("0.#####e+00", CultureInfo.InvariantCulture));
			break;
		case 'G':
			text = ((self < 1000000 && self > -1000000) ? ((!stringFormatSpec.ThousandsComma) ? self.ToString(CultureInfo.InvariantCulture) : self.ToString("#,0", CultureInfo.InvariantCulture)) : self.ToString("0.#####E+00", CultureInfo.InvariantCulture));
			break;
		case 'X':
			text = ToHex(self, lowercase: false);
			break;
		case 'x':
			text = ToHex(self, lowercase: true);
			break;
		case 'o':
			text = ToOctal(self, lowercase: true);
			break;
		case 'b':
			text = ToBinary(self, includeType: false);
			break;
		case 'c':
			if (((int?)stringFormatSpec.Sign).HasValue)
			{
				throw PythonOps.ValueError("Sign not allowed with integer format specifier 'c'");
			}
			if (self < 0 || self > 255)
			{
				throw PythonOps.OverflowError("%c arg not in range(0x10000)");
			}
			text = ScriptingRuntimeHelpers.CharToString((char)self);
			break;
		default:
			throw PythonOps.ValueError("Unknown format code '{0}'", stringFormatSpec.Type.ToString());
		}
		if (self < 0 && text[0] == '-')
		{
			text = text.Substring(1);
		}
		return stringFormatSpec.AlignNumericText(text, self == 0, self > 0);
	}

	public static string __repr__(int self)
	{
		return self.ToString(CultureInfo.InvariantCulture);
	}

	private static string ToHex(int self, bool lowercase)
	{
		if (self != int.MinValue)
		{
			int num = self;
			if (self < 0)
			{
				num = -self;
			}
			return num.ToString(lowercase ? "x" : "X", CultureInfo.InvariantCulture);
		}
		return "80000000";
	}

	private static string ToOctal(int self, bool lowercase)
	{
		switch (self)
		{
		case 0:
			return "0";
		default:
		{
			int num = self;
			if (self < 0)
			{
				num = -self;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int num2 = 30; num2 >= 0; num2 -= 3)
			{
				char c = (char)(48 + ((num >> num2) & 7));
				if (c != '0' || stringBuilder.Length > 0)
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}
		case int.MinValue:
			return "20000000000";
		}
	}

	internal static string ToBinary(int self)
	{
		if (self == int.MinValue)
		{
			return "-0b10000000000000000000000000000000";
		}
		string text = ToBinary(self, includeType: true);
		if (self < 0)
		{
			text = "-" + text;
		}
		return text;
	}

	private static string ToBinary(int self, bool includeType)
	{
		string text;
		switch (self)
		{
		case 0:
			text = "0";
			break;
		default:
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = self;
			if (self < 0)
			{
				num = -self;
			}
			for (int num2 = 31; num2 >= 0; num2--)
			{
				if ((num & (1 << num2)) != 0)
				{
					stringBuilder.Append('1');
				}
				else if (stringBuilder.Length != 0)
				{
					stringBuilder.Append('0');
				}
			}
			text = stringBuilder.ToString();
			break;
		}
		case int.MinValue:
			text = "10000000000000000000000000000000";
			break;
		}
		if (includeType)
		{
			text = "0b" + text;
		}
		return text;
	}

	[SpecialName]
	public static int Plus(int x)
	{
		return x;
	}

	[SpecialName]
	public static object Negate(int x)
	{
		if (x == int.MinValue)
		{
			return -(BigInteger)int.MinValue;
		}
		return -x;
	}

	[SpecialName]
	public static object Abs(int x)
	{
		if (x < 0)
		{
			if (x == int.MinValue)
			{
				return -(BigInteger)int.MinValue;
			}
			return -x;
		}
		return x;
	}

	[SpecialName]
	public static int OnesComplement(int x)
	{
		return ~x;
	}

	public static bool __nonzero__(int x)
	{
		return x != 0;
	}

	public static int __trunc__(int x)
	{
		return x;
	}

	public static int __hash__(int x)
	{
		return x;
	}

	[SpecialName]
	public static object Add(int x, int y)
	{
		long num = (long)x + (long)y;
		if (int.MinValue <= num && num <= int.MaxValue)
		{
			return ScriptingRuntimeHelpers.Int32ToObject((int)num);
		}
		return BigIntegerOps.Add(x, y);
	}

	[SpecialName]
	public static object Subtract(int x, int y)
	{
		long num = (long)x - (long)y;
		if (int.MinValue <= num && num <= int.MaxValue)
		{
			return ScriptingRuntimeHelpers.Int32ToObject((int)num);
		}
		return BigIntegerOps.Subtract(x, y);
	}

	[SpecialName]
	public static object Multiply(int x, int y)
	{
		long num = (long)x * (long)y;
		if (int.MinValue <= num && num <= int.MaxValue)
		{
			return ScriptingRuntimeHelpers.Int32ToObject((int)num);
		}
		return BigIntegerOps.Multiply(x, y);
	}

	[SpecialName]
	public static object Divide(int x, int y)
	{
		return FloorDivide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(int x, int y)
	{
		return DoubleOps.TrueDivide(x, y);
	}

	[SpecialName]
	public static object LeftShift(int x, [NotNull] BigInteger y)
	{
		return BigIntegerOps.LeftShift(x, y);
	}

	[SpecialName]
	public static int RightShift(int x, [NotNull] BigInteger y)
	{
		return (int)BigIntegerOps.RightShift(x, y);
	}

	[SpecialName]
	public static int BitwiseAnd(int x, int y)
	{
		return x & y;
	}

	[SpecialName]
	public static int BitwiseOr(int x, int y)
	{
		return x | y;
	}

	[SpecialName]
	public static int ExclusiveOr(int x, int y)
	{
		return x ^ y;
	}

	[SpecialName]
	public static int Compare(int x, int y)
	{
		if (x != y)
		{
			if (x <= y)
			{
				return -1;
			}
			return 1;
		}
		return 0;
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static sbyte ConvertToSByte(int x)
	{
		if (-128 <= x && x <= 127)
		{
			return (sbyte)x;
		}
		throw Converter.CannotConvertOverflow("SByte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static byte ConvertToByte(int x)
	{
		if (0 <= x && x <= 255)
		{
			return (byte)x;
		}
		throw Converter.CannotConvertOverflow("Byte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static short ConvertToInt16(int x)
	{
		if (-32768 <= x && x <= 32767)
		{
			return (short)x;
		}
		throw Converter.CannotConvertOverflow("Int16", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ushort ConvertToUInt16(int x)
	{
		if (0 <= x && x <= 65535)
		{
			return (ushort)x;
		}
		throw Converter.CannotConvertOverflow("UInt16", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static uint ConvertToUInt32(int x)
	{
		if (x >= 0)
		{
			return (uint)x;
		}
		throw Converter.CannotConvertOverflow("UInt32", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static long ConvertToInt64(int x)
	{
		return x;
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ulong ConvertToUInt64(int x)
	{
		if (x >= 0)
		{
			return (ulong)x;
		}
		throw Converter.CannotConvertOverflow("UInt64", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static float ConvertToSingle(int x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static double ConvertToDouble(int x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static int Getreal(int x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static int Getimag(int x)
	{
		return 0;
	}

	public static int conjugate(int x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static int Getnumerator(int x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static int Getdenominator(int x)
	{
		return 1;
	}

	public static int bit_length(int value)
	{
		return MathUtils.BitLength(value);
	}
}

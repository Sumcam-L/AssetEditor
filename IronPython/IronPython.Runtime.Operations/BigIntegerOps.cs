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

public static class BigIntegerOps
{
	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, string s, int radix)
	{
		if (radix == 16 || radix == 8 || radix == 2)
		{
			s = Int32Ops.TrimRadix(s, radix);
		}
		if (cls == TypeCache.BigInteger)
		{
			return ParseBigIntegerSign(s, radix);
		}
		BigInteger bigInteger = ParseBigIntegerSign(s, radix);
		return cls.CreateInstance(context, bigInteger);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, IList<byte> s)
	{
		if (!(s is IPythonObject o) || !PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, "__long__", out var value))
		{
			value = ParseBigIntegerSign(s.MakeString(), 10);
		}
		if (cls == TypeCache.BigInteger)
		{
			return value;
		}
		return cls.CreateInstance(context, value);
	}

	private static BigInteger ParseBigIntegerSign(string s, int radix)
	{
		try
		{
			return LiteralParser.ParseBigIntegerSign(s, radix);
		}
		catch (ArgumentException ex)
		{
			throw PythonOps.ValueError(ex.Message);
		}
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, object x)
	{
		if (x is string)
		{
			return ReturnObject(context, cls, ParseBigIntegerSign((string)x, 10));
		}
		if (x is Extensible<string> extensible)
		{
			if (PythonTypeOps.TryInvokeUnaryOperator(context, x, "__long__", out var value))
			{
				return ReturnObject(context, cls, (BigInteger)value);
			}
			return ReturnObject(context, cls, ParseBigIntegerSign(extensible.Value, 10));
		}
		if (x is double)
		{
			return ReturnObject(context, cls, DoubleOps.__long__((double)x));
		}
		if (x is int)
		{
			return ReturnObject(context, cls, (BigInteger)(int)x);
		}
		if (x is BigInteger)
		{
			return ReturnObject(context, cls, x);
		}
		if (x is Complex)
		{
			throw PythonOps.TypeError("can't convert complex to long; use long(abs(z))");
		}
		if (x is decimal)
		{
			return ReturnObject(context, cls, (BigInteger)(decimal)x);
		}
		if ((PythonTypeOps.TryInvokeUnaryOperator(context, x, "__long__", out var value2) && !object.ReferenceEquals(value2, NotImplementedType.Value)) || (x is OldInstance && PythonTypeOps.TryInvokeUnaryOperator(context, x, "__int__", out value2) && !object.ReferenceEquals(value2, NotImplementedType.Value)))
		{
			if (value2 is int || value2 is BigInteger || value2 is Extensible<int> || value2 is Extensible<BigInteger>)
			{
				return ReturnObject(context, cls, value2);
			}
			throw PythonOps.TypeError("__long__ returned non-long (type {0})", PythonTypeOps.GetOldName(value2));
		}
		if (PythonOps.TryGetBoundAttr(context, x, "__trunc__", out value2))
		{
			value2 = PythonOps.CallWithContext(context, value2);
			if (Converter.TryConvertToInt32(value2, out var result))
			{
				return ReturnObject(context, cls, (BigInteger)result);
			}
			if (Converter.TryConvertToBigInteger(value2, out var result2))
			{
				return ReturnObject(context, cls, result2);
			}
			throw PythonOps.TypeError("__trunc__ returned non-Integral (type {0})", PythonTypeOps.GetOldName(value2));
		}
		if (x is OldInstance)
		{
			throw PythonOps.AttributeError("{0} instance has no attribute '__trunc__'", ((OldInstance)x)._class.Name);
		}
		throw PythonOps.TypeError("long() argument must be a string or a number, not '{0}'", DynamicHelpers.GetPythonType(x).Name);
	}

	private static object ReturnObject(CodeContext context, PythonType cls, object value)
	{
		if (cls == TypeCache.BigInteger)
		{
			return value;
		}
		return cls.CreateInstance(context, value);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls)
	{
		if (cls == TypeCache.BigInteger)
		{
			return BigInteger.Zero;
		}
		return cls.CreateInstance(context, BigInteger.Zero);
	}

	[SpecialName]
	public static object Power(BigInteger x, object y, object z)
	{
		if (y is int)
		{
			return Power(x, (int)y, z);
		}
		if (y is long)
		{
			return Power(x, (long)y, z);
		}
		if (y is BigInteger)
		{
			return Power(x, (BigInteger)y, z);
		}
		return NotImplementedType.Value;
	}

	[SpecialName]
	public static object Power(BigInteger x, int y, object z)
	{
		if (z is int)
		{
			return Power(x, y, (int)z);
		}
		if (z is long)
		{
			return Power(x, y, (long)z);
		}
		if (z is BigInteger)
		{
			return Power(x, y, (BigInteger)z);
		}
		if (z == null)
		{
			return Power(x, y);
		}
		return NotImplementedType.Value;
	}

	[SpecialName]
	public static object Power(BigInteger x, BigInteger y, object z)
	{
		if (z is int)
		{
			return Power(x, y, (int)z);
		}
		if (z is long)
		{
			return Power(x, y, (long)z);
		}
		if (z is BigInteger)
		{
			return Power(x, y, (BigInteger)z);
		}
		if (z == null)
		{
			return Power(x, y);
		}
		return NotImplementedType.Value;
	}

	[SpecialName]
	public static object Power(BigInteger x, int y, BigInteger z)
	{
		if (y < 0)
		{
			throw PythonOps.TypeError("power", y, "power must be >= 0");
		}
		if (z == BigInteger.Zero)
		{
			throw PythonOps.ZeroDivisionError();
		}
		BigInteger bigInteger = x.ModPow(y, z);
		if ((z < BigInteger.Zero && bigInteger > BigInteger.Zero) || (z > BigInteger.Zero && bigInteger < BigInteger.Zero))
		{
			bigInteger += z;
		}
		return bigInteger;
	}

	[SpecialName]
	public static object Power(BigInteger x, BigInteger y, BigInteger z)
	{
		if (y < BigInteger.Zero)
		{
			throw PythonOps.TypeError("power", y, "power must be >= 0");
		}
		if (z == BigInteger.Zero)
		{
			throw PythonOps.ZeroDivisionError();
		}
		BigInteger bigInteger = x.ModPow(y, z);
		if ((z < BigInteger.Zero && bigInteger > BigInteger.Zero) || (z > BigInteger.Zero && bigInteger < BigInteger.Zero))
		{
			bigInteger += z;
		}
		return bigInteger;
	}

	[SpecialName]
	public static object Power([NotNull] BigInteger x, int y)
	{
		if (y < 0)
		{
			return DoubleOps.Power(x.ToFloat64(), y);
		}
		return x.Power(y);
	}

	[SpecialName]
	public static object Power([NotNull] BigInteger x, [NotNull] BigInteger y)
	{
		if (y.AsInt32(out var ret))
		{
			return Power(x, ret);
		}
		if (x == BigInteger.Zero)
		{
			if (y.Sign < 0)
			{
				throw PythonOps.ZeroDivisionError("0.0 cannot be raised to a negative power");
			}
			return BigInteger.Zero;
		}
		if (x == BigInteger.One)
		{
			return BigInteger.One;
		}
		throw PythonOps.ValueError("Number too big");
	}

	private static BigInteger DivMod(BigInteger x, BigInteger y, out BigInteger r)
	{
		BigInteger remainder;
		BigInteger bigInteger = BigInteger.DivRem(x, y, out remainder);
		if (x >= BigInteger.Zero)
		{
			if (y > BigInteger.Zero)
			{
				r = remainder;
				return bigInteger;
			}
			if (remainder == BigInteger.Zero)
			{
				r = remainder;
				return bigInteger;
			}
			r = remainder + y;
			return bigInteger - BigInteger.One;
		}
		if (y > BigInteger.Zero)
		{
			if (remainder == BigInteger.Zero)
			{
				r = remainder;
				return bigInteger;
			}
			r = remainder + y;
			return bigInteger - BigInteger.One;
		}
		r = remainder;
		return bigInteger;
	}

	[PythonHidden]
	public static BigInteger Add(BigInteger x, BigInteger y)
	{
		return x + y;
	}

	[PythonHidden]
	public static BigInteger Subtract(BigInteger x, BigInteger y)
	{
		return x - y;
	}

	[PythonHidden]
	public static BigInteger Multiply(BigInteger x, BigInteger y)
	{
		return x * y;
	}

	[SpecialName]
	public static BigInteger FloorDivide([NotNull] BigInteger x, [NotNull] BigInteger y)
	{
		return Divide(x, y);
	}

	[SpecialName]
	public static double TrueDivide([NotNull] BigInteger x, [NotNull] BigInteger y)
	{
		if (y == BigInteger.Zero)
		{
			throw new DivideByZeroException();
		}
		if (x.TryToFloat64(out var result) && y.TryToFloat64(out var result2))
		{
			return result / result2;
		}
		BigInteger remainder;
		BigInteger self = BigInteger.DivRem(x, y, out remainder);
		if (self.TryToFloat64(out result))
		{
			if (remainder != BigInteger.Zero)
			{
				BigInteger self2 = y / remainder;
				if (self2.TryToFloat64(out result2) && result2 != 0.0)
				{
					return result + 1.0 / result2;
				}
			}
			return result;
		}
		throw PythonOps.OverflowError("long/long too large for a float");
	}

	public static BigInteger operator /(BigInteger x, BigInteger y)
	{
		BigInteger r;
		return DivMod(x, y, out r);
	}

	public static BigInteger operator %(BigInteger x, BigInteger y)
	{
		DivMod(x, y, out var r);
		return r;
	}

	public static BigInteger operator <<(BigInteger x, int y)
	{
		if (y < 0)
		{
			throw PythonOps.ValueError("negative shift count");
		}
		return x << y;
	}

	public static BigInteger operator >>(BigInteger x, int y)
	{
		if (y < 0)
		{
			throw PythonOps.ValueError("negative shift count");
		}
		return x >> y;
	}

	public static BigInteger operator <<(BigInteger x, BigInteger y)
	{
		return x << (int)y;
	}

	public static BigInteger operator >>(BigInteger x, BigInteger y)
	{
		return x >> (int)y;
	}

	[SpecialName]
	public static PythonTuple DivMod(BigInteger x, BigInteger y)
	{
		BigInteger r;
		BigInteger bigInteger = DivMod(x, y, out r);
		return PythonTuple.MakeTuple(bigInteger, r);
	}

	public static object __abs__(BigInteger x)
	{
		return x.Abs();
	}

	public static bool __nonzero__(BigInteger x)
	{
		return !x.IsZero();
	}

	[SpecialName]
	public static object Negate(BigInteger x)
	{
		return -x;
	}

	public static object __pos__(BigInteger x)
	{
		return x;
	}

	public static object __int__(BigInteger x)
	{
		if (x.AsInt32(out var ret))
		{
			return ScriptingRuntimeHelpers.Int32ToObject(ret);
		}
		return x;
	}

	public static object __float__(BigInteger self)
	{
		return self.ToFloat64();
	}

	public static string __oct__(BigInteger x)
	{
		if (x == BigInteger.Zero)
		{
			return "0L";
		}
		if (x > 0L)
		{
			return "0" + x.ToString(8) + "L";
		}
		return "-0" + (-x).ToString(8) + "L";
	}

	public static string __hex__(BigInteger x)
	{
		if (x < 0L)
		{
			return "-0x" + (-x).ToString(16).ToLower() + "L";
		}
		return "0x" + x.ToString(16).ToLower() + "L";
	}

	public static object __getnewargs__(CodeContext context, BigInteger self)
	{
		return PythonTuple.MakeTuple(__new__(context, TypeCache.BigInteger, self));
	}

	[PythonHidden]
	public static BigInteger OnesComplement(BigInteger x)
	{
		return ~x;
	}

	internal static BigInteger FloorDivideImpl(BigInteger x, BigInteger y)
	{
		return FloorDivide(x, y);
	}

	[PythonHidden]
	public static BigInteger BitwiseAnd(BigInteger x, BigInteger y)
	{
		return x & y;
	}

	[PythonHidden]
	public static BigInteger BitwiseOr(BigInteger x, BigInteger y)
	{
		return x | y;
	}

	[PythonHidden]
	public static BigInteger ExclusiveOr(BigInteger x, BigInteger y)
	{
		return x ^ y;
	}

	[SpecialName]
	[PropertyMethod]
	public static BigInteger Getreal(BigInteger self)
	{
		return self;
	}

	[SpecialName]
	[PropertyMethod]
	public static BigInteger Getimag(BigInteger self)
	{
		return 0;
	}

	public static BigInteger conjugate(BigInteger self)
	{
		return self;
	}

	[SpecialName]
	[PropertyMethod]
	public static BigInteger Getnumerator(BigInteger self)
	{
		return self;
	}

	[SpecialName]
	[PropertyMethod]
	public static BigInteger Getdenominator(BigInteger self)
	{
		return 1;
	}

	public static int bit_length(BigInteger self)
	{
		return MathUtils.BitLength(self);
	}

	public static BigInteger __trunc__(BigInteger self)
	{
		return self;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static double ConvertToDouble(BigInteger self)
	{
		return self.ToFloat64();
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static int ConvertToInt32(BigInteger self)
	{
		if (self.AsInt32(out var ret))
		{
			return ret;
		}
		throw Converter.CannotConvertOverflow("int", self);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static Complex ConvertToComplex(BigInteger self)
	{
		return MathUtils.MakeReal(ConvertToDouble(self));
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static BigInteger ConvertToBigInteger(bool self)
	{
		if (!self)
		{
			return BigInteger.Zero;
		}
		return BigInteger.One;
	}

	[SpecialName]
	public static int Compare(BigInteger x, BigInteger y)
	{
		return x.CompareTo(y);
	}

	[SpecialName]
	public static int Compare(BigInteger x, int y)
	{
		if (x.AsInt32(out var ret))
		{
			if (ret != y)
			{
				if (ret <= y)
				{
					return -1;
				}
				return 1;
			}
			return 0;
		}
		return BigInteger.Compare(x, y);
	}

	[SpecialName]
	public static int Compare(BigInteger x, uint y)
	{
		if (x.AsUInt32(out var ret))
		{
			if (ret != y)
			{
				if (ret <= y)
				{
					return -1;
				}
				return 1;
			}
			return 0;
		}
		return BigInteger.Compare(x, y);
	}

	[SpecialName]
	public static int Compare(BigInteger x, double y)
	{
		return -DoubleOps.Compare(y, x);
	}

	[SpecialName]
	public static int Compare(BigInteger x, [NotNull] Extensible<double> y)
	{
		return -DoubleOps.Compare(y.Value, x);
	}

	[SpecialName]
	public static int Compare(BigInteger x, decimal y)
	{
		return DecimalOps.__cmp__(x, y);
	}

	[SpecialName]
	public static int Compare(BigInteger x, bool y)
	{
		return Compare(x, y ? 1 : 0);
	}

	public static BigInteger __long__(BigInteger self)
	{
		return self;
	}

	public static BigInteger __index__(BigInteger self)
	{
		return self;
	}

	public static int __hash__(BigInteger self)
	{
		if (self == -2147483648L)
		{
			return int.MinValue;
		}
		return self.GetHashCode();
	}

	public static string __repr__([NotNull] BigInteger self)
	{
		return self.ToString() + "L";
	}

	public static object __coerce__(CodeContext context, BigInteger self, object o)
	{
		if (Converter.TryConvertToBigInteger(o, out var result))
		{
			return PythonTuple.MakeTuple(self, result);
		}
		return NotImplementedType.Value;
	}

	[PythonHidden]
	public static float ToFloat(BigInteger self)
	{
		return (float)self.ToFloat64();
	}

	[PythonHidden]
	public static BigInteger Xor(BigInteger x, BigInteger y)
	{
		return x ^ y;
	}

	[PythonHidden]
	public static BigInteger Divide(BigInteger x, BigInteger y)
	{
		return x / y;
	}

	[PythonHidden]
	public static BigInteger Mod(BigInteger x, BigInteger y)
	{
		return x % y;
	}

	[PythonHidden]
	public static BigInteger LeftShift(BigInteger x, int y)
	{
		return x << y;
	}

	[PythonHidden]
	public static BigInteger RightShift(BigInteger x, int y)
	{
		return x >> y;
	}

	[PythonHidden]
	public static BigInteger LeftShift(BigInteger x, BigInteger y)
	{
		return x << y;
	}

	[PythonHidden]
	public static BigInteger RightShift(BigInteger x, BigInteger y)
	{
		return x >> y;
	}

	[PythonHidden]
	public static bool AsDecimal(BigInteger self, out decimal res)
	{
		if (self <= (BigInteger)decimal.MaxValue && self >= (BigInteger)decimal.MinValue)
		{
			res = (decimal)self;
			return true;
		}
		res = 0m;
		return false;
	}

	[PythonHidden]
	public static bool AsInt32(BigInteger self, out int res)
	{
		return self.AsInt32(out res);
	}

	[PythonHidden]
	public static bool AsInt64(BigInteger self, out long res)
	{
		return self.AsInt64(out res);
	}

	[CLSCompliant(false)]
	[PythonHidden]
	public static bool AsUInt32(BigInteger self, out uint res)
	{
		return self.AsUInt32(out res);
	}

	[PythonHidden]
	[CLSCompliant(false)]
	public static bool AsUInt64(BigInteger self, out ulong res)
	{
		return self.AsUInt64(out res);
	}

	[PythonHidden]
	public static int ToInt32(BigInteger self)
	{
		return (int)self;
	}

	[PythonHidden]
	public static long ToInt64(BigInteger self)
	{
		return (long)self;
	}

	[PythonHidden]
	[CLSCompliant(false)]
	public static uint ToUInt32(BigInteger self)
	{
		return (uint)self;
	}

	[CLSCompliant(false)]
	[PythonHidden]
	public static ulong ToUInt64(BigInteger self)
	{
		return (ulong)self;
	}

	[PythonHidden]
	public static bool ToBoolean(BigInteger self, IFormatProvider provider)
	{
		return !self.IsZero;
	}

	[PythonHidden]
	public static byte ToByte(BigInteger self, IFormatProvider provider)
	{
		return (byte)self;
	}

	[CLSCompliant(false)]
	[PythonHidden]
	public static sbyte ToSByte(BigInteger self, IFormatProvider provider)
	{
		return (sbyte)self;
	}

	[PythonHidden]
	public static char ToChar(BigInteger self, IFormatProvider provider)
	{
		if (self.AsInt32(out var ret) && ret <= 65535 && ret >= 0)
		{
			return (char)ret;
		}
		throw new OverflowException("big integer won't fit into char");
	}

	[PythonHidden]
	public static decimal ToDecimal(BigInteger self, IFormatProvider provider)
	{
		return (decimal)self;
	}

	[PythonHidden]
	public static double ToDouble(BigInteger self, IFormatProvider provider)
	{
		return ConvertToDouble(self);
	}

	[PythonHidden]
	public static float ToSingle(BigInteger self, IFormatProvider provider)
	{
		return ToFloat(self);
	}

	[PythonHidden]
	public static short ToInt16(BigInteger self, IFormatProvider provider)
	{
		return (short)self;
	}

	[PythonHidden]
	public static int ToInt32(BigInteger self, IFormatProvider provider)
	{
		return (int)self;
	}

	[PythonHidden]
	public static long ToInt64(BigInteger self, IFormatProvider provider)
	{
		return (long)self;
	}

	[CLSCompliant(false)]
	[PythonHidden]
	public static ushort ToUInt16(BigInteger self, IFormatProvider provider)
	{
		return (ushort)self;
	}

	[PythonHidden]
	[CLSCompliant(false)]
	public static uint ToUInt32(BigInteger self, IFormatProvider provider)
	{
		return (uint)self;
	}

	[CLSCompliant(false)]
	[PythonHidden]
	public static ulong ToUInt64(BigInteger self, IFormatProvider provider)
	{
		return (ulong)self;
	}

	[PythonHidden]
	public static object ToType(BigInteger self, Type conversionType, IFormatProvider provider)
	{
		if (conversionType == typeof(BigInteger))
		{
			return self;
		}
		throw new NotImplementedException();
	}

	[PythonHidden]
	public static TypeCode GetTypeCode(BigInteger self)
	{
		return TypeCode.Object;
	}

	[PythonHidden]
	public static BigInteger Square(BigInteger self)
	{
		return self * self;
	}

	[PythonHidden]
	public static bool IsNegative(BigInteger self)
	{
		return self.Sign < 0;
	}

	[PythonHidden]
	public static bool IsPositive(BigInteger self)
	{
		return self.Sign > 0;
	}

	[PythonHidden]
	public static int GetBitCount(BigInteger self)
	{
		return self.GetBitCount();
	}

	[PythonHidden]
	public static int GetByteCount(BigInteger self)
	{
		return self.GetByteCount();
	}

	[PythonHidden]
	public static BigInteger Create(byte[] v)
	{
		return new BigInteger(v);
	}

	[PythonHidden]
	public static BigInteger Create(int v)
	{
		return new BigInteger(v);
	}

	[PythonHidden]
	public static BigInteger Create(long v)
	{
		return new BigInteger(v);
	}

	[PythonHidden]
	[CLSCompliant(false)]
	public static BigInteger Create(uint v)
	{
		return new BigInteger(v);
	}

	[CLSCompliant(false)]
	[PythonHidden]
	public static BigInteger Create(ulong v)
	{
		return v;
	}

	[PythonHidden]
	public static BigInteger Create(decimal v)
	{
		return new BigInteger(v);
	}

	[PythonHidden]
	public static BigInteger Create(double v)
	{
		return new BigInteger(v);
	}

	[CLSCompliant(false)]
	[PythonHidden]
	public static uint[] GetWords(BigInteger self)
	{
		return self.GetWords();
	}

	[PythonHidden]
	[CLSCompliant(false)]
	public static uint GetWord(BigInteger self, int index)
	{
		return self.GetWord(index);
	}

	[PythonHidden]
	public static int GetWordCount(BigInteger self)
	{
		return self.GetWordCount();
	}

	public static string __format__(CodeContext context, BigInteger self, [NotNull] string formatSpec)
	{
		StringFormatSpec stringFormatSpec = StringFormatSpec.FromString(formatSpec);
		if (stringFormatSpec.Precision.HasValue)
		{
			throw PythonOps.ValueError("Precision not allowed in integer format specifier");
		}
		BigInteger bigInteger = self;
		if (self < 0L)
		{
			bigInteger = -self;
		}
		string text;
		switch (stringFormatSpec.Type)
		{
		case 'n':
		{
			CultureInfo numericCulture = PythonContext.GetContext(context).NumericCulture;
			if (numericCulture != CultureInfo.InvariantCulture)
			{
				text = ToCultureString(bigInteger, PythonContext.GetContext(context).NumericCulture);
				break;
			}
			goto case null;
		}
		case null:
		case 'd':
			text = ((!stringFormatSpec.ThousandsComma) ? bigInteger.ToString("D", CultureInfo.InvariantCulture) : bigInteger.ToString("#,0", CultureInfo.InvariantCulture));
			break;
		case '%':
			text = ((!stringFormatSpec.ThousandsComma) ? bigInteger.ToString("0.000000%", CultureInfo.InvariantCulture) : bigInteger.ToString("#,0.000000%", CultureInfo.InvariantCulture));
			break;
		case 'e':
			text = ((!stringFormatSpec.ThousandsComma) ? bigInteger.ToString("0.000000e+00", CultureInfo.InvariantCulture) : bigInteger.ToString("#,0.000000e+00", CultureInfo.InvariantCulture));
			break;
		case 'E':
			text = ((!stringFormatSpec.ThousandsComma) ? bigInteger.ToString("0.000000E+00", CultureInfo.InvariantCulture) : bigInteger.ToString("#,0.000000E+00", CultureInfo.InvariantCulture));
			break;
		case 'F':
		case 'f':
			text = ((!stringFormatSpec.ThousandsComma) ? bigInteger.ToString("#########0.000000", CultureInfo.InvariantCulture) : bigInteger.ToString("#,########0.000000", CultureInfo.InvariantCulture));
			break;
		case 'g':
			text = ((!(bigInteger >= 1000000L)) ? ((!stringFormatSpec.ThousandsComma) ? bigInteger.ToString(CultureInfo.InvariantCulture) : bigInteger.ToString("#,0", CultureInfo.InvariantCulture)) : bigInteger.ToString("0.#####e+00", CultureInfo.InvariantCulture));
			break;
		case 'G':
			text = ((!(bigInteger >= 1000000L)) ? ((!stringFormatSpec.ThousandsComma) ? bigInteger.ToString(CultureInfo.InvariantCulture) : bigInteger.ToString("#,0", CultureInfo.InvariantCulture)) : bigInteger.ToString("0.#####E+00", CultureInfo.InvariantCulture));
			break;
		case 'X':
			text = AbsToHex(bigInteger, lowercase: false);
			break;
		case 'x':
			text = AbsToHex(bigInteger, lowercase: true);
			break;
		case 'o':
			text = ToOctal(bigInteger, lowercase: true);
			break;
		case 'b':
			text = ToBinary(bigInteger, includeType: false, lowercase: true);
			break;
		case 'c':
		{
			if (((int?)stringFormatSpec.Sign).HasValue)
			{
				throw PythonOps.ValueError("Sign not allowed with integer format specifier 'c'");
			}
			if (!self.AsInt32(out var ret))
			{
				throw PythonOps.OverflowError("long int too large to convert to int");
			}
			if (ret < 0 || ret > 255)
			{
				throw PythonOps.OverflowError("%c arg not in range(0x10000)");
			}
			text = ScriptingRuntimeHelpers.CharToString((char)ret);
			break;
		}
		default:
			throw PythonOps.ValueError("Unknown format code '{0}'", stringFormatSpec.Type.ToString());
		}
		return stringFormatSpec.AlignNumericText(text, self.IsZero(), self.IsPositive());
	}

	internal static string AbsToHex(BigInteger val, bool lowercase)
	{
		return ToDigits(val, 16, lowercase);
	}

	private static string ToOctal(BigInteger val, bool lowercase)
	{
		return ToDigits(val, 8, lowercase);
	}

	internal static string ToBinary(BigInteger val)
	{
		string text = ToBinary(val.Abs(), includeType: true, lowercase: true);
		if (val.IsNegative())
		{
			text = "-" + text;
		}
		return text;
	}

	private static string ToBinary(BigInteger val, bool includeType, bool lowercase)
	{
		string text = ToDigits(val, 2, lowercase);
		if (includeType)
		{
			text = (lowercase ? "0b" : "0B") + text;
		}
		return text;
	}

	private static string ToCultureString(BigInteger val, CultureInfo ci)
	{
		string numberGroupSeparator = ci.NumberFormat.NumberGroupSeparator;
		int[] numberGroupSizes = ci.NumberFormat.NumberGroupSizes;
		string text = val.ToString();
		if (numberGroupSizes.Length > 0)
		{
			StringBuilder stringBuilder = new StringBuilder(text);
			int num = 0;
			int num2 = text.Length - 1;
			while (num2 > 0)
			{
				int num3 = numberGroupSizes[num];
				if (num3 == 0)
				{
					break;
				}
				num2 -= num3;
				if (num2 >= 0)
				{
					stringBuilder.Insert(num2 + 1, numberGroupSeparator);
				}
				if (num + 1 < numberGroupSizes.Length)
				{
					if (numberGroupSizes[num + 1] == 0)
					{
						break;
					}
					num++;
				}
			}
			text = stringBuilder.ToString();
		}
		return text;
	}

	private static string ToExponent(BigInteger self, bool lower, int minPrecision, int maxPrecision)
	{
		string text = self.ToString();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(text[0]);
		for (int i = 1; i < maxPrecision && i < text.Length; i++)
		{
			if (text[i] != '0' || i <= minPrecision)
			{
				if (stringBuilder.Length == 1)
				{
					stringBuilder.Append('.');
				}
				while (i > stringBuilder.Length - 1)
				{
					stringBuilder.Append('0');
				}
				if (i == maxPrecision - 1 && i != text.Length - 1 && text[i + 1] >= '5')
				{
					stringBuilder.Append((char)(text[i] + 1));
				}
				else
				{
					stringBuilder.Append(text[i]);
				}
			}
		}
		if (text.Length <= minPrecision)
		{
			if (stringBuilder.Length == 1)
			{
				stringBuilder.Append('.');
			}
			while (minPrecision >= stringBuilder.Length - 1)
			{
				stringBuilder.Append('0');
			}
		}
		stringBuilder.Append(lower ? "e+" : "E+");
		int num = text.Length - 1;
		if (num < 10)
		{
			stringBuilder.Append('0');
			stringBuilder.Append((char)(48 + num));
		}
		else
		{
			stringBuilder.Append(num.ToString());
		}
		return stringBuilder.ToString();
	}

	private static string ToDigits(BigInteger val, int radix, bool lower)
	{
		if (val.IsZero())
		{
			return "0";
		}
		StringBuilder stringBuilder = new StringBuilder();
		while (val != 0L)
		{
			int num = (int)(val % radix);
			if (num < 10)
			{
				stringBuilder.Append((char)(num + 48));
			}
			else if (lower)
			{
				stringBuilder.Append((char)(num - 10 + 97));
			}
			else
			{
				stringBuilder.Append((char)(num - 10 + 65));
			}
			val /= (BigInteger)radix;
		}
		StringBuilder stringBuilder2 = new StringBuilder(stringBuilder.Length);
		for (int num2 = stringBuilder.Length - 1; num2 >= 0; num2--)
		{
			stringBuilder2.Append(stringBuilder[num2]);
		}
		return stringBuilder2.ToString();
	}
}

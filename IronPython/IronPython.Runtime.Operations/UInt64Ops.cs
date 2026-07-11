using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class UInt64Ops
{
	[StaticExtensionMethod]
	public static object __new__(PythonType cls)
	{
		return __new__(cls, 0uL);
	}

	[StaticExtensionMethod]
	public static object __new__(PythonType cls, object value)
	{
		if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(ulong)))
		{
			throw PythonOps.TypeError("UInt64.__new__: first argument must be UInt64 type.");
		}
		if (value is IConvertible convertible)
		{
			switch (convertible.GetTypeCode())
			{
			case TypeCode.Byte:
				return (ulong)(byte)value;
			case TypeCode.SByte:
				return (ulong)(sbyte)value;
			case TypeCode.Int16:
				return (ulong)(short)value;
			case TypeCode.UInt16:
				return (ulong)(ushort)value;
			case TypeCode.Int32:
				return (ulong)(int)value;
			case TypeCode.UInt32:
				return (ulong)(uint)value;
			case TypeCode.Int64:
				return (ulong)(long)value;
			case TypeCode.UInt64:
				return (ulong)value;
			case TypeCode.Single:
				return (ulong)(float)value;
			case TypeCode.Double:
				return (ulong)(double)value;
			}
		}
		if (value is string)
		{
			return ulong.Parse((string)value);
		}
		if (value is BigInteger)
		{
			return (ulong)(BigInteger)value;
		}
		if (value is Extensible<BigInteger>)
		{
			return (ulong)((Extensible<BigInteger>)value).Value;
		}
		if (value is Extensible<double>)
		{
			return (ulong)((Extensible<double>)value).Value;
		}
		throw PythonOps.ValueError("invalid value for UInt64.__new__");
	}

	[SpecialName]
	public static ulong Plus(ulong x)
	{
		return x;
	}

	[SpecialName]
	public static object Negate(ulong x)
	{
		return BigIntegerOps.Negate(x);
	}

	[SpecialName]
	public static ulong Abs(ulong x)
	{
		return x;
	}

	[SpecialName]
	public static object OnesComplement(ulong x)
	{
		return BigIntegerOps.OnesComplement(x);
	}

	public static bool __nonzero__(ulong x)
	{
		return x != 0;
	}

	public static ulong __trunc__(ulong x)
	{
		return x;
	}

	public static int __hash__(ulong x)
	{
		int num = (int)x + (int)(x >> 32);
		if (x < 0)
		{
			return -num;
		}
		return num;
	}

	[SpecialName]
	public static object Add(ulong x, ulong y)
	{
		try
		{
			return checked(x + y);
		}
		catch (OverflowException)
		{
			return BigIntegerOps.Add(x, y);
		}
	}

	[SpecialName]
	public static object Add(ulong x, long y)
	{
		return BigIntegerOps.Add(x, y);
	}

	[SpecialName]
	public static object Add(long x, ulong y)
	{
		return BigIntegerOps.Add(x, y);
	}

	[SpecialName]
	public static object Subtract(ulong x, ulong y)
	{
		try
		{
			return checked(x - y);
		}
		catch (OverflowException)
		{
			return BigIntegerOps.Subtract(x, y);
		}
	}

	[SpecialName]
	public static object Subtract(ulong x, long y)
	{
		return BigIntegerOps.Subtract(x, y);
	}

	[SpecialName]
	public static object Subtract(long x, ulong y)
	{
		return BigIntegerOps.Subtract(x, y);
	}

	[SpecialName]
	public static object Multiply(ulong x, ulong y)
	{
		try
		{
			return checked(x * y);
		}
		catch (OverflowException)
		{
			return BigIntegerOps.Multiply(x, y);
		}
	}

	[SpecialName]
	public static object Multiply(ulong x, long y)
	{
		return BigIntegerOps.Multiply(x, y);
	}

	[SpecialName]
	public static object Multiply(long x, ulong y)
	{
		return BigIntegerOps.Multiply(x, y);
	}

	[SpecialName]
	public static object Divide(ulong x, ulong y)
	{
		return FloorDivide(x, y);
	}

	[SpecialName]
	public static object Divide(ulong x, long y)
	{
		return BigIntegerOps.Divide(x, y);
	}

	[SpecialName]
	public static object Divide(long x, ulong y)
	{
		return BigIntegerOps.Divide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(ulong x, ulong y)
	{
		return DoubleOps.TrueDivide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(ulong x, long y)
	{
		return BigIntegerOps.TrueDivide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(long x, ulong y)
	{
		return BigIntegerOps.TrueDivide(x, y);
	}

	[SpecialName]
	public static ulong FloorDivide(ulong x, ulong y)
	{
		return x / y;
	}

	[SpecialName]
	public static object FloorDivide(ulong x, long y)
	{
		return BigIntegerOps.FloorDivide(x, y);
	}

	[SpecialName]
	public static object FloorDivide(long x, ulong y)
	{
		return BigIntegerOps.FloorDivide(x, y);
	}

	[SpecialName]
	public static ulong Mod(ulong x, ulong y)
	{
		return x % y;
	}

	[SpecialName]
	public static BigInteger Mod(ulong x, long y)
	{
		return BigIntegerOps.Mod(x, y);
	}

	[SpecialName]
	public static BigInteger Mod(long x, ulong y)
	{
		return BigIntegerOps.Mod(x, y);
	}

	[SpecialName]
	public static object Power(ulong x, ulong y)
	{
		return BigIntegerOps.Power(x, y);
	}

	[SpecialName]
	public static object Power(ulong x, long y)
	{
		return BigIntegerOps.Power(x, y);
	}

	[SpecialName]
	public static object Power(long x, ulong y)
	{
		return BigIntegerOps.Power(x, y);
	}

	[SpecialName]
	public static object LeftShift(ulong x, [NotNull] BigInteger y)
	{
		return BigIntegerOps.LeftShift(x, y);
	}

	[SpecialName]
	public static ulong RightShift(ulong x, [NotNull] BigInteger y)
	{
		return (ulong)BigIntegerOps.RightShift(x, y);
	}

	[SpecialName]
	public static ulong BitwiseAnd(ulong x, ulong y)
	{
		return x & y;
	}

	[SpecialName]
	public static BigInteger BitwiseAnd(ulong x, long y)
	{
		return BigIntegerOps.BitwiseAnd(x, y);
	}

	[SpecialName]
	public static BigInteger BitwiseAnd(long x, ulong y)
	{
		return BigIntegerOps.BitwiseAnd(x, y);
	}

	[SpecialName]
	public static ulong BitwiseOr(ulong x, ulong y)
	{
		return x | y;
	}

	[SpecialName]
	public static BigInteger BitwiseOr(ulong x, long y)
	{
		return BigIntegerOps.BitwiseOr(x, y);
	}

	[SpecialName]
	public static BigInteger BitwiseOr(long x, ulong y)
	{
		return BigIntegerOps.BitwiseOr(x, y);
	}

	[SpecialName]
	public static ulong ExclusiveOr(ulong x, ulong y)
	{
		return x ^ y;
	}

	[SpecialName]
	public static BigInteger ExclusiveOr(ulong x, long y)
	{
		return BigIntegerOps.ExclusiveOr(x, y);
	}

	[SpecialName]
	public static BigInteger ExclusiveOr(long x, ulong y)
	{
		return BigIntegerOps.ExclusiveOr(x, y);
	}

	[SpecialName]
	public static int Compare(ulong x, ulong y)
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
	public static int Compare(ulong x, long y)
	{
		return BigIntegerOps.Compare((BigInteger)x, (BigInteger)y);
	}

	[SpecialName]
	public static int Compare(long x, ulong y)
	{
		return BigIntegerOps.Compare((BigInteger)x, (BigInteger)y);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static sbyte ConvertToSByte(ulong x)
	{
		if (x <= 127)
		{
			return (sbyte)x;
		}
		throw Converter.CannotConvertOverflow("SByte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static byte ConvertToByte(ulong x)
	{
		if (0 <= x && x <= 255)
		{
			return (byte)x;
		}
		throw Converter.CannotConvertOverflow("Byte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static short ConvertToInt16(ulong x)
	{
		if (x <= 32767)
		{
			return (short)x;
		}
		throw Converter.CannotConvertOverflow("Int16", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ushort ConvertToUInt16(ulong x)
	{
		if (0 <= x && x <= 65535)
		{
			return (ushort)x;
		}
		throw Converter.CannotConvertOverflow("UInt16", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static int ConvertToInt32(ulong x)
	{
		if (x <= int.MaxValue)
		{
			return (int)x;
		}
		throw Converter.CannotConvertOverflow("Int32", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static uint ConvertToUInt32(ulong x)
	{
		if (0 <= x && x <= uint.MaxValue)
		{
			return (uint)x;
		}
		throw Converter.CannotConvertOverflow("UInt32", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static long ConvertToInt64(ulong x)
	{
		if (x <= long.MaxValue)
		{
			return (long)x;
		}
		throw Converter.CannotConvertOverflow("Int64", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static float ConvertToSingle(ulong x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static double ConvertToDouble(ulong x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static ulong Getreal(ulong x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static ulong Getimag(ulong x)
	{
		return 0uL;
	}

	public static ulong conjugate(ulong x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static ulong Getnumerator(ulong x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static ulong Getdenominator(ulong x)
	{
		return 1uL;
	}

	public static string __hex__(ulong value)
	{
		return BigIntegerOps.__hex__(value);
	}

	public static int bit_length(ulong value)
	{
		return MathUtils.BitLengthUnsigned(value);
	}
}

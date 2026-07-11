using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class Int64Ops
{
	[StaticExtensionMethod]
	public static object __new__(PythonType cls)
	{
		return __new__(cls, 0L);
	}

	[StaticExtensionMethod]
	public static object __new__(PythonType cls, object value)
	{
		if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(long)))
		{
			throw PythonOps.TypeError("Int64.__new__: first argument must be Int64 type.");
		}
		if (value is IConvertible convertible)
		{
			switch (convertible.GetTypeCode())
			{
			case TypeCode.Byte:
				return (long)(byte)value;
			case TypeCode.SByte:
				return (long)(sbyte)value;
			case TypeCode.Int16:
				return (long)(short)value;
			case TypeCode.UInt16:
				return (long)(ushort)value;
			case TypeCode.Int32:
				return (long)(int)value;
			case TypeCode.UInt32:
				return (long)(uint)value;
			case TypeCode.Int64:
				return (long)value;
			case TypeCode.UInt64:
				return (long)(ulong)value;
			case TypeCode.Single:
				return (long)(float)value;
			case TypeCode.Double:
				return (long)(double)value;
			}
		}
		if (value is string)
		{
			return long.Parse((string)value);
		}
		if (value is BigInteger)
		{
			return (long)(BigInteger)value;
		}
		if (value is Extensible<BigInteger>)
		{
			return (long)((Extensible<BigInteger>)value).Value;
		}
		if (value is Extensible<double>)
		{
			return (long)((Extensible<double>)value).Value;
		}
		throw PythonOps.ValueError("invalid value for Int64.__new__");
	}

	[SpecialName]
	public static long Plus(long x)
	{
		return x;
	}

	[SpecialName]
	public static object Negate(long x)
	{
		if (x == long.MinValue)
		{
			return -(BigInteger)long.MinValue;
		}
		return -x;
	}

	[SpecialName]
	public static object Abs(long x)
	{
		if (x < 0)
		{
			if (x == long.MinValue)
			{
				return -(BigInteger)long.MinValue;
			}
			return -x;
		}
		return x;
	}

	[SpecialName]
	public static long OnesComplement(long x)
	{
		return ~x;
	}

	public static bool __nonzero__(long x)
	{
		return x != 0;
	}

	public static long __trunc__(long x)
	{
		return x;
	}

	public static int __hash__(long x)
	{
		long num = x;
		if (num < 0)
		{
			num *= -1;
		}
		int num2 = (int)num + (int)(num >> 32);
		if (x < 0)
		{
			return -num2;
		}
		return num2;
	}

	[SpecialName]
	public static object Add(long x, long y)
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
	public static object Subtract(long x, long y)
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
	public static object Multiply(long x, long y)
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
	public static object Divide(long x, long y)
	{
		return FloorDivide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(long x, long y)
	{
		return DoubleOps.TrueDivide(x, y);
	}

	[SpecialName]
	public static object FloorDivide(long x, long y)
	{
		if (y == -1 && x == long.MinValue)
		{
			return -(BigInteger)long.MinValue;
		}
		return MathUtils.FloorDivideUnchecked(x, y);
	}

	[SpecialName]
	public static long Mod(long x, long y)
	{
		return (long)BigIntegerOps.Mod(x, y);
	}

	[SpecialName]
	public static object Power(long x, long y)
	{
		return BigIntegerOps.Power(x, y);
	}

	[SpecialName]
	public static object LeftShift(long x, [NotNull] BigInteger y)
	{
		return BigIntegerOps.LeftShift(x, y);
	}

	[SpecialName]
	public static long RightShift(long x, [NotNull] BigInteger y)
	{
		return (long)BigIntegerOps.RightShift(x, y);
	}

	[SpecialName]
	public static long BitwiseAnd(long x, long y)
	{
		return x & y;
	}

	[SpecialName]
	public static long BitwiseOr(long x, long y)
	{
		return x | y;
	}

	[SpecialName]
	public static long ExclusiveOr(long x, long y)
	{
		return x ^ y;
	}

	[SpecialName]
	public static int Compare(long x, long y)
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
	public static sbyte ConvertToSByte(long x)
	{
		if (-128 <= x && x <= 127)
		{
			return (sbyte)x;
		}
		throw Converter.CannotConvertOverflow("SByte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static byte ConvertToByte(long x)
	{
		if (0 <= x && x <= 255)
		{
			return (byte)x;
		}
		throw Converter.CannotConvertOverflow("Byte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static short ConvertToInt16(long x)
	{
		if (-32768 <= x && x <= 32767)
		{
			return (short)x;
		}
		throw Converter.CannotConvertOverflow("Int16", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ushort ConvertToUInt16(long x)
	{
		if (0 <= x && x <= 65535)
		{
			return (ushort)x;
		}
		throw Converter.CannotConvertOverflow("UInt16", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static int ConvertToInt32(long x)
	{
		if (int.MinValue <= x && x <= int.MaxValue)
		{
			return (int)x;
		}
		throw Converter.CannotConvertOverflow("Int32", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static uint ConvertToUInt32(long x)
	{
		if (0 <= x && x <= uint.MaxValue)
		{
			return (uint)x;
		}
		throw Converter.CannotConvertOverflow("UInt32", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ulong ConvertToUInt64(long x)
	{
		if (x >= 0)
		{
			return (ulong)x;
		}
		throw Converter.CannotConvertOverflow("UInt64", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static float ConvertToSingle(long x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static double ConvertToDouble(long x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static long Getreal(long x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static long Getimag(long x)
	{
		return 0L;
	}

	public static long conjugate(long x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static long Getnumerator(long x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static long Getdenominator(long x)
	{
		return 1L;
	}

	public static string __hex__(long value)
	{
		return BigIntegerOps.__hex__(value);
	}

	public static int bit_length(long value)
	{
		return MathUtils.BitLength(value);
	}
}

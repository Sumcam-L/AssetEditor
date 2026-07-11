using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class Int16Ops
{
	[StaticExtensionMethod]
	public static object __new__(PythonType cls)
	{
		return __new__(cls, (short)0);
	}

	[StaticExtensionMethod]
	public static object __new__(PythonType cls, object value)
	{
		if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(short)))
		{
			throw PythonOps.TypeError("Int16.__new__: first argument must be Int16 type.");
		}
		if (value is IConvertible convertible)
		{
			switch (convertible.GetTypeCode())
			{
			case TypeCode.Byte:
				return (short)(byte)value;
			case TypeCode.SByte:
				return (short)(sbyte)value;
			case TypeCode.Int16:
				return (short)value;
			case TypeCode.UInt16:
				return (short)(ushort)value;
			case TypeCode.Int32:
				return (short)(int)value;
			case TypeCode.UInt32:
				return (short)(uint)value;
			case TypeCode.Int64:
				return (short)(long)value;
			case TypeCode.UInt64:
				return (short)(ulong)value;
			case TypeCode.Single:
				return (short)(float)value;
			case TypeCode.Double:
				return (short)(double)value;
			}
		}
		if (value is string)
		{
			return short.Parse((string)value);
		}
		if (value is BigInteger)
		{
			return (short)(BigInteger)value;
		}
		if (value is Extensible<BigInteger>)
		{
			return (short)((Extensible<BigInteger>)value).Value;
		}
		if (value is Extensible<double>)
		{
			return (short)((Extensible<double>)value).Value;
		}
		throw PythonOps.ValueError("invalid value for Int16.__new__");
	}

	[SpecialName]
	public static short Plus(short x)
	{
		return x;
	}

	[SpecialName]
	public static object Negate(short x)
	{
		if (x == short.MinValue)
		{
			return 32768;
		}
		return (short)(-x);
	}

	[SpecialName]
	public static object Abs(short x)
	{
		if (x < 0)
		{
			if (x == short.MinValue)
			{
				return 32768;
			}
			return (short)(-x);
		}
		return x;
	}

	[SpecialName]
	public static short OnesComplement(short x)
	{
		return (short)(~x);
	}

	public static bool __nonzero__(short x)
	{
		return x != 0;
	}

	public static short __trunc__(short x)
	{
		return x;
	}

	public static int __hash__(short x)
	{
		return x;
	}

	[SpecialName]
	public static object Add(short x, short y)
	{
		int num = x + y;
		if (-32768 <= num && num <= 32767)
		{
			return (short)num;
		}
		return num;
	}

	[SpecialName]
	public static object Subtract(short x, short y)
	{
		int num = x - y;
		if (-32768 <= num && num <= 32767)
		{
			return (short)num;
		}
		return num;
	}

	[SpecialName]
	public static object Multiply(short x, short y)
	{
		int num = x * y;
		if (-32768 <= num && num <= 32767)
		{
			return (short)num;
		}
		return num;
	}

	[SpecialName]
	public static object Divide(short x, short y)
	{
		return FloorDivide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(short x, short y)
	{
		return DoubleOps.TrueDivide(x, y);
	}

	[SpecialName]
	public static object FloorDivide(short x, short y)
	{
		if (y == -1 && x == short.MinValue)
		{
			return 32768;
		}
		return (short)MathUtils.FloorDivideUnchecked(x, y);
	}

	[SpecialName]
	public static short Mod(short x, short y)
	{
		return (short)Int32Ops.Mod(x, y);
	}

	[SpecialName]
	public static object Power(short x, short y)
	{
		return Int32Ops.Power(x, y);
	}

	[SpecialName]
	public static object LeftShift(short x, [NotNull] BigInteger y)
	{
		return BigIntegerOps.LeftShift(x, y);
	}

	[SpecialName]
	public static short RightShift(short x, [NotNull] BigInteger y)
	{
		return (short)BigIntegerOps.RightShift(x, y);
	}

	[SpecialName]
	public static object LeftShift(short x, int y)
	{
		return Int32Ops.LeftShift(x, y);
	}

	[SpecialName]
	public static short RightShift(short x, int y)
	{
		return (short)Int32Ops.RightShift(x, y);
	}

	[SpecialName]
	public static short BitwiseAnd(short x, short y)
	{
		return (short)(x & y);
	}

	[SpecialName]
	public static short BitwiseOr(short x, short y)
	{
		return (short)(x | y);
	}

	[SpecialName]
	public static short ExclusiveOr(short x, short y)
	{
		return (short)(x ^ y);
	}

	[SpecialName]
	public static int Compare(short x, short y)
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
	public static sbyte ConvertToSByte(short x)
	{
		if (-128 <= x && x <= 127)
		{
			return (sbyte)x;
		}
		throw Converter.CannotConvertOverflow("SByte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static byte ConvertToByte(short x)
	{
		if (0 <= x && x <= 255)
		{
			return (byte)x;
		}
		throw Converter.CannotConvertOverflow("Byte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ushort ConvertToUInt16(short x)
	{
		if (x >= 0)
		{
			return (ushort)x;
		}
		throw Converter.CannotConvertOverflow("UInt16", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static int ConvertToInt32(short x)
	{
		return x;
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static uint ConvertToUInt32(short x)
	{
		if (x >= 0)
		{
			return (uint)x;
		}
		throw Converter.CannotConvertOverflow("UInt32", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static long ConvertToInt64(short x)
	{
		return x;
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ulong ConvertToUInt64(short x)
	{
		if (x >= 0)
		{
			return (ulong)x;
		}
		throw Converter.CannotConvertOverflow("UInt64", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static float ConvertToSingle(short x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static double ConvertToDouble(short x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static short Getreal(short x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static short Getimag(short x)
	{
		return 0;
	}

	public static short conjugate(short x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static short Getnumerator(short x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static short Getdenominator(short x)
	{
		return 1;
	}

	public static string __hex__(short value)
	{
		return BigIntegerOps.__hex__(value);
	}

	public static int bit_length(short value)
	{
		return MathUtils.BitLength(value);
	}
}

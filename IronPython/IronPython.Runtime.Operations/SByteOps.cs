using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class SByteOps
{
	[StaticExtensionMethod]
	public static object __new__(PythonType cls)
	{
		return __new__(cls, (sbyte)0);
	}

	[StaticExtensionMethod]
	public static object __new__(PythonType cls, object value)
	{
		if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(sbyte)))
		{
			throw PythonOps.TypeError("SByte.__new__: first argument must be SByte type.");
		}
		if (value is IConvertible convertible)
		{
			switch (convertible.GetTypeCode())
			{
			case TypeCode.Byte:
				return (sbyte)(byte)value;
			case TypeCode.SByte:
				return (sbyte)value;
			case TypeCode.Int16:
				return (sbyte)(short)value;
			case TypeCode.UInt16:
				return (sbyte)(ushort)value;
			case TypeCode.Int32:
				return (sbyte)(int)value;
			case TypeCode.UInt32:
				return (sbyte)(uint)value;
			case TypeCode.Int64:
				return (sbyte)(long)value;
			case TypeCode.UInt64:
				return (sbyte)(ulong)value;
			case TypeCode.Single:
				return (sbyte)(float)value;
			case TypeCode.Double:
				return (sbyte)(double)value;
			}
		}
		if (value is string)
		{
			return sbyte.Parse((string)value);
		}
		if (value is BigInteger)
		{
			return (sbyte)(BigInteger)value;
		}
		if (value is Extensible<BigInteger>)
		{
			return (sbyte)((Extensible<BigInteger>)value).Value;
		}
		if (value is Extensible<double>)
		{
			return (sbyte)((Extensible<double>)value).Value;
		}
		throw PythonOps.ValueError("invalid value for SByte.__new__");
	}

	[SpecialName]
	public static sbyte Plus(sbyte x)
	{
		return x;
	}

	[SpecialName]
	public static object Negate(sbyte x)
	{
		if (x == sbyte.MinValue)
		{
			return 128;
		}
		return (sbyte)(-x);
	}

	[SpecialName]
	public static object Abs(sbyte x)
	{
		if (x < 0)
		{
			if (x == sbyte.MinValue)
			{
				return 128;
			}
			return (sbyte)(-x);
		}
		return x;
	}

	[SpecialName]
	public static sbyte OnesComplement(sbyte x)
	{
		return (sbyte)(~x);
	}

	public static bool __nonzero__(sbyte x)
	{
		return x != 0;
	}

	public static sbyte __trunc__(sbyte x)
	{
		return x;
	}

	public static int __hash__(sbyte x)
	{
		return x;
	}

	[SpecialName]
	public static object Add(sbyte x, sbyte y)
	{
		short num = (short)(x + y);
		if (-128 <= num && num <= 127)
		{
			return (sbyte)num;
		}
		return num;
	}

	[SpecialName]
	public static object Subtract(sbyte x, sbyte y)
	{
		short num = (short)(x - y);
		if (-128 <= num && num <= 127)
		{
			return (sbyte)num;
		}
		return num;
	}

	[SpecialName]
	public static object Multiply(sbyte x, sbyte y)
	{
		short num = (short)(x * y);
		if (-128 <= num && num <= 127)
		{
			return (sbyte)num;
		}
		return num;
	}

	[SpecialName]
	public static object Divide(sbyte x, sbyte y)
	{
		return FloorDivide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(sbyte x, sbyte y)
	{
		return DoubleOps.TrueDivide(x, y);
	}

	[SpecialName]
	public static object FloorDivide(sbyte x, sbyte y)
	{
		if (y == -1 && x == sbyte.MinValue)
		{
			return 128;
		}
		return (sbyte)MathUtils.FloorDivideUnchecked(x, y);
	}

	[SpecialName]
	public static sbyte Mod(sbyte x, sbyte y)
	{
		return (sbyte)Int32Ops.Mod(x, y);
	}

	[SpecialName]
	public static object Power(sbyte x, sbyte y)
	{
		return Int32Ops.Power(x, y);
	}

	[SpecialName]
	public static object LeftShift(sbyte x, [NotNull] BigInteger y)
	{
		return BigIntegerOps.LeftShift(x, y);
	}

	[SpecialName]
	public static sbyte RightShift(sbyte x, [NotNull] BigInteger y)
	{
		return (sbyte)BigIntegerOps.RightShift(x, y);
	}

	[SpecialName]
	public static object LeftShift(sbyte x, int y)
	{
		return Int32Ops.LeftShift(x, y);
	}

	[SpecialName]
	public static sbyte RightShift(sbyte x, int y)
	{
		return (sbyte)Int32Ops.RightShift(x, y);
	}

	[SpecialName]
	public static sbyte BitwiseAnd(sbyte x, sbyte y)
	{
		return (sbyte)(x & y);
	}

	[SpecialName]
	public static sbyte BitwiseOr(sbyte x, sbyte y)
	{
		return (sbyte)(x | y);
	}

	[SpecialName]
	public static sbyte ExclusiveOr(sbyte x, sbyte y)
	{
		return (sbyte)(x ^ y);
	}

	[SpecialName]
	public static int Compare(sbyte x, sbyte y)
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
	public static byte ConvertToByte(sbyte x)
	{
		if (x >= 0)
		{
			return (byte)x;
		}
		throw Converter.CannotConvertOverflow("Byte", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static short ConvertToInt16(sbyte x)
	{
		return x;
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ushort ConvertToUInt16(sbyte x)
	{
		if (x >= 0)
		{
			return (ushort)x;
		}
		throw Converter.CannotConvertOverflow("UInt16", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static int ConvertToInt32(sbyte x)
	{
		return x;
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static uint ConvertToUInt32(sbyte x)
	{
		if (x >= 0)
		{
			return (uint)x;
		}
		throw Converter.CannotConvertOverflow("UInt32", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static long ConvertToInt64(sbyte x)
	{
		return x;
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ulong ConvertToUInt64(sbyte x)
	{
		if (x >= 0)
		{
			return (ulong)x;
		}
		throw Converter.CannotConvertOverflow("UInt64", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static float ConvertToSingle(sbyte x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static double ConvertToDouble(sbyte x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static sbyte Getreal(sbyte x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static sbyte Getimag(sbyte x)
	{
		return 0;
	}

	public static sbyte conjugate(sbyte x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static sbyte Getnumerator(sbyte x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static sbyte Getdenominator(sbyte x)
	{
		return 1;
	}

	public static string __hex__(sbyte value)
	{
		return BigIntegerOps.__hex__(value);
	}

	public static int bit_length(sbyte value)
	{
		return MathUtils.BitLength(value);
	}
}

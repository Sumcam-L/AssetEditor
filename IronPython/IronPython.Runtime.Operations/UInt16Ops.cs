using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class UInt16Ops
{
	[StaticExtensionMethod]
	public static object __new__(PythonType cls)
	{
		return __new__(cls, (ushort)0);
	}

	[StaticExtensionMethod]
	public static object __new__(PythonType cls, object value)
	{
		if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(ushort)))
		{
			throw PythonOps.TypeError("UInt16.__new__: first argument must be UInt16 type.");
		}
		if (value is IConvertible convertible)
		{
			switch (convertible.GetTypeCode())
			{
			case TypeCode.Byte:
				return (ushort)(byte)value;
			case TypeCode.SByte:
				return (ushort)(sbyte)value;
			case TypeCode.Int16:
				return (ushort)(short)value;
			case TypeCode.UInt16:
				return (ushort)value;
			case TypeCode.Int32:
				return (ushort)(int)value;
			case TypeCode.UInt32:
				return (ushort)(uint)value;
			case TypeCode.Int64:
				return (ushort)(long)value;
			case TypeCode.UInt64:
				return (ushort)(ulong)value;
			case TypeCode.Single:
				return (ushort)(float)value;
			case TypeCode.Double:
				return (ushort)(double)value;
			}
		}
		if (value is string)
		{
			return ushort.Parse((string)value);
		}
		if (value is BigInteger)
		{
			return (ushort)(BigInteger)value;
		}
		if (value is Extensible<BigInteger>)
		{
			return (ushort)((Extensible<BigInteger>)value).Value;
		}
		if (value is Extensible<double>)
		{
			return (ushort)((Extensible<double>)value).Value;
		}
		throw PythonOps.ValueError("invalid value for UInt16.__new__");
	}

	[SpecialName]
	public static ushort Plus(ushort x)
	{
		return x;
	}

	[SpecialName]
	public static object Negate(ushort x)
	{
		return Int32Ops.Negate(x);
	}

	[SpecialName]
	public static ushort Abs(ushort x)
	{
		return x;
	}

	[SpecialName]
	public static object OnesComplement(ushort x)
	{
		return Int32Ops.OnesComplement(x);
	}

	public static bool __nonzero__(ushort x)
	{
		return x != 0;
	}

	public static ushort __trunc__(ushort x)
	{
		return x;
	}

	public static int __hash__(ushort x)
	{
		return x;
	}

	[SpecialName]
	public static object Add(ushort x, ushort y)
	{
		int num = x + y;
		if (0 <= num && num <= 65535)
		{
			return (ushort)num;
		}
		return num;
	}

	[SpecialName]
	public static object Add(ushort x, short y)
	{
		return Int32Ops.Add(x, y);
	}

	[SpecialName]
	public static object Add(short x, ushort y)
	{
		return Int32Ops.Add(x, y);
	}

	[SpecialName]
	public static object Subtract(ushort x, ushort y)
	{
		int num = x - y;
		if (0 <= num && num <= 65535)
		{
			return (ushort)num;
		}
		return num;
	}

	[SpecialName]
	public static object Subtract(ushort x, short y)
	{
		return Int32Ops.Subtract(x, y);
	}

	[SpecialName]
	public static object Subtract(short x, ushort y)
	{
		return Int32Ops.Subtract(x, y);
	}

	[SpecialName]
	public static object Multiply(ushort x, ushort y)
	{
		int num = x * y;
		if (0 <= num && num <= 65535)
		{
			return (ushort)num;
		}
		return num;
	}

	[SpecialName]
	public static object Multiply(ushort x, short y)
	{
		return Int32Ops.Multiply(x, y);
	}

	[SpecialName]
	public static object Multiply(short x, ushort y)
	{
		return Int32Ops.Multiply(x, y);
	}

	[SpecialName]
	public static object Divide(ushort x, ushort y)
	{
		return FloorDivide(x, y);
	}

	[SpecialName]
	public static object Divide(ushort x, short y)
	{
		return Int32Ops.Divide(x, y);
	}

	[SpecialName]
	public static object Divide(short x, ushort y)
	{
		return Int32Ops.Divide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(ushort x, ushort y)
	{
		return DoubleOps.TrueDivide((int)x, (int)y);
	}

	[SpecialName]
	public static double TrueDivide(ushort x, short y)
	{
		return Int32Ops.TrueDivide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(short x, ushort y)
	{
		return Int32Ops.TrueDivide(x, y);
	}

	[SpecialName]
	public static ushort FloorDivide(ushort x, ushort y)
	{
		return (ushort)(x / y);
	}

	[SpecialName]
	public static object FloorDivide(ushort x, short y)
	{
		return Int32Ops.FloorDivide(x, y);
	}

	[SpecialName]
	public static object FloorDivide(short x, ushort y)
	{
		return Int32Ops.FloorDivide(x, y);
	}

	[SpecialName]
	public static ushort Mod(ushort x, ushort y)
	{
		return (ushort)(x % y);
	}

	[SpecialName]
	public static int Mod(ushort x, short y)
	{
		return Int32Ops.Mod(x, y);
	}

	[SpecialName]
	public static int Mod(short x, ushort y)
	{
		return Int32Ops.Mod(x, y);
	}

	[SpecialName]
	public static object Power(ushort x, ushort y)
	{
		return Int32Ops.Power(x, y);
	}

	[SpecialName]
	public static object Power(ushort x, short y)
	{
		return Int32Ops.Power(x, y);
	}

	[SpecialName]
	public static object Power(short x, ushort y)
	{
		return Int32Ops.Power(x, y);
	}

	[SpecialName]
	public static object LeftShift(ushort x, [NotNull] BigInteger y)
	{
		return BigIntegerOps.LeftShift(x, y);
	}

	[SpecialName]
	public static ushort RightShift(ushort x, [NotNull] BigInteger y)
	{
		return (ushort)BigIntegerOps.RightShift(x, y);
	}

	[SpecialName]
	public static object LeftShift(ushort x, int y)
	{
		return Int32Ops.LeftShift(x, y);
	}

	[SpecialName]
	public static ushort RightShift(ushort x, int y)
	{
		return (ushort)Int32Ops.RightShift(x, y);
	}

	[SpecialName]
	public static ushort BitwiseAnd(ushort x, ushort y)
	{
		return (ushort)(x & y);
	}

	[SpecialName]
	public static int BitwiseAnd(ushort x, short y)
	{
		return Int32Ops.BitwiseAnd(x, y);
	}

	[SpecialName]
	public static int BitwiseAnd(short x, ushort y)
	{
		return Int32Ops.BitwiseAnd(x, y);
	}

	[SpecialName]
	public static ushort BitwiseOr(ushort x, ushort y)
	{
		return (ushort)(x | y);
	}

	[SpecialName]
	public static int BitwiseOr(ushort x, short y)
	{
		return Int32Ops.BitwiseOr(x, y);
	}

	[SpecialName]
	public static int BitwiseOr(short x, ushort y)
	{
		return Int32Ops.BitwiseOr(x, y);
	}

	[SpecialName]
	public static ushort ExclusiveOr(ushort x, ushort y)
	{
		return (ushort)(x ^ y);
	}

	[SpecialName]
	public static int ExclusiveOr(ushort x, short y)
	{
		return Int32Ops.ExclusiveOr(x, y);
	}

	[SpecialName]
	public static int ExclusiveOr(short x, ushort y)
	{
		return Int32Ops.ExclusiveOr(x, y);
	}

	[SpecialName]
	public static int Compare(ushort x, ushort y)
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
	public static int Compare(ushort x, short y)
	{
		return Int32Ops.Compare(x, y);
	}

	[SpecialName]
	public static int Compare(short x, ushort y)
	{
		return Int32Ops.Compare(x, y);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static sbyte ConvertToSByte(ushort x)
	{
		if (x <= 127)
		{
			return (sbyte)x;
		}
		throw Converter.CannotConvertOverflow("SByte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static byte ConvertToByte(ushort x)
	{
		if (0 <= x && x <= 255)
		{
			return (byte)x;
		}
		throw Converter.CannotConvertOverflow("Byte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static short ConvertToInt16(ushort x)
	{
		if (x <= 32767)
		{
			return (short)x;
		}
		throw Converter.CannotConvertOverflow("Int16", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static int ConvertToInt32(ushort x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static uint ConvertToUInt32(ushort x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static long ConvertToInt64(ushort x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static ulong ConvertToUInt64(ushort x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static float ConvertToSingle(ushort x)
	{
		return (int)x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static double ConvertToDouble(ushort x)
	{
		return (int)x;
	}

	[SpecialName]
	[PropertyMethod]
	public static ushort Getreal(ushort x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static ushort Getimag(ushort x)
	{
		return 0;
	}

	public static ushort conjugate(ushort x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static ushort Getnumerator(ushort x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static ushort Getdenominator(ushort x)
	{
		return 1;
	}

	public static string __hex__(ushort value)
	{
		return BigIntegerOps.__hex__(value);
	}

	public static int bit_length(ushort value)
	{
		return MathUtils.BitLength(value);
	}
}

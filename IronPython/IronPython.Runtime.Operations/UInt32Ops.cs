using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class UInt32Ops
{
	[StaticExtensionMethod]
	public static object __new__(PythonType cls)
	{
		return __new__(cls, 0u);
	}

	[StaticExtensionMethod]
	public static object __new__(PythonType cls, object value)
	{
		if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(uint)))
		{
			throw PythonOps.TypeError("UInt32.__new__: first argument must be UInt32 type.");
		}
		if (value is IConvertible convertible)
		{
			switch (convertible.GetTypeCode())
			{
			case TypeCode.Byte:
				return (uint)(byte)value;
			case TypeCode.SByte:
				return (uint)(sbyte)value;
			case TypeCode.Int16:
				return (uint)(short)value;
			case TypeCode.UInt16:
				return (uint)(ushort)value;
			case TypeCode.Int32:
				return (uint)(int)value;
			case TypeCode.UInt32:
				return (uint)value;
			case TypeCode.Int64:
				return (uint)(long)value;
			case TypeCode.UInt64:
				return (uint)(ulong)value;
			case TypeCode.Single:
				return (uint)(float)value;
			case TypeCode.Double:
				return (uint)(double)value;
			}
		}
		if (value is string)
		{
			return uint.Parse((string)value);
		}
		if (value is BigInteger)
		{
			return (uint)(BigInteger)value;
		}
		if (value is Extensible<BigInteger>)
		{
			return (uint)((Extensible<BigInteger>)value).Value;
		}
		if (value is Extensible<double>)
		{
			return (uint)((Extensible<double>)value).Value;
		}
		throw PythonOps.ValueError("invalid value for UInt32.__new__");
	}

	[SpecialName]
	public static uint Plus(uint x)
	{
		return x;
	}

	[SpecialName]
	public static object Negate(uint x)
	{
		return Int64Ops.Negate(x);
	}

	[SpecialName]
	public static uint Abs(uint x)
	{
		return x;
	}

	[SpecialName]
	public static object OnesComplement(uint x)
	{
		return Int64Ops.OnesComplement(x);
	}

	public static bool __nonzero__(uint x)
	{
		return x != 0;
	}

	public static uint __trunc__(uint x)
	{
		return x;
	}

	public static int __hash__(uint x)
	{
		return (int)x;
	}

	[SpecialName]
	public static object Add(uint x, uint y)
	{
		long num = (long)x + (long)y;
		if (0 <= num && num <= uint.MaxValue)
		{
			return (uint)num;
		}
		return num;
	}

	[SpecialName]
	public static object Add(uint x, int y)
	{
		return Int64Ops.Add(x, y);
	}

	[SpecialName]
	public static object Add(int x, uint y)
	{
		return Int64Ops.Add(x, y);
	}

	[SpecialName]
	public static object Subtract(uint x, uint y)
	{
		long num = (long)x - (long)y;
		if (0 <= num && num <= uint.MaxValue)
		{
			return (uint)num;
		}
		return num;
	}

	[SpecialName]
	public static object Subtract(uint x, int y)
	{
		return Int64Ops.Subtract(x, y);
	}

	[SpecialName]
	public static object Subtract(int x, uint y)
	{
		return Int64Ops.Subtract(x, y);
	}

	[SpecialName]
	public static object Multiply(uint x, uint y)
	{
		long num = (long)x * (long)y;
		if (0 <= num && num <= uint.MaxValue)
		{
			return (uint)num;
		}
		return num;
	}

	[SpecialName]
	public static object Multiply(uint x, int y)
	{
		return Int64Ops.Multiply(x, y);
	}

	[SpecialName]
	public static object Multiply(int x, uint y)
	{
		return Int64Ops.Multiply(x, y);
	}

	[SpecialName]
	public static object Divide(uint x, uint y)
	{
		return FloorDivide(x, y);
	}

	[SpecialName]
	public static object Divide(uint x, int y)
	{
		return Int64Ops.Divide(x, y);
	}

	[SpecialName]
	public static object Divide(int x, uint y)
	{
		return Int64Ops.Divide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(uint x, uint y)
	{
		return DoubleOps.TrueDivide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(uint x, int y)
	{
		return Int64Ops.TrueDivide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(int x, uint y)
	{
		return Int64Ops.TrueDivide(x, y);
	}

	[SpecialName]
	public static uint FloorDivide(uint x, uint y)
	{
		return x / y;
	}

	[SpecialName]
	public static object FloorDivide(uint x, int y)
	{
		return Int64Ops.FloorDivide(x, y);
	}

	[SpecialName]
	public static object FloorDivide(int x, uint y)
	{
		return Int64Ops.FloorDivide(x, y);
	}

	[SpecialName]
	public static uint Mod(uint x, uint y)
	{
		return x % y;
	}

	[SpecialName]
	public static long Mod(uint x, int y)
	{
		return Int64Ops.Mod(x, y);
	}

	[SpecialName]
	public static long Mod(int x, uint y)
	{
		return Int64Ops.Mod(x, y);
	}

	[SpecialName]
	public static object Power(uint x, uint y)
	{
		return Int32Ops.Power((int)x, (int)y);
	}

	[SpecialName]
	public static object Power(uint x, int y)
	{
		return Int64Ops.Power(x, y);
	}

	[SpecialName]
	public static object Power(int x, uint y)
	{
		return Int64Ops.Power(x, y);
	}

	[SpecialName]
	public static object LeftShift(uint x, [NotNull] BigInteger y)
	{
		return BigIntegerOps.LeftShift(x, y);
	}

	[SpecialName]
	public static uint RightShift(uint x, [NotNull] BigInteger y)
	{
		return (uint)BigIntegerOps.RightShift(x, y);
	}

	[SpecialName]
	public static uint BitwiseAnd(uint x, uint y)
	{
		return x & y;
	}

	[SpecialName]
	public static long BitwiseAnd(uint x, int y)
	{
		return Int64Ops.BitwiseAnd(x, y);
	}

	[SpecialName]
	public static long BitwiseAnd(int x, uint y)
	{
		return Int64Ops.BitwiseAnd(x, y);
	}

	[SpecialName]
	public static uint BitwiseOr(uint x, uint y)
	{
		return x | y;
	}

	[SpecialName]
	public static long BitwiseOr(uint x, int y)
	{
		return Int64Ops.BitwiseOr(x, y);
	}

	[SpecialName]
	public static long BitwiseOr(int x, uint y)
	{
		return Int64Ops.BitwiseOr(x, y);
	}

	[SpecialName]
	public static uint ExclusiveOr(uint x, uint y)
	{
		return x ^ y;
	}

	[SpecialName]
	public static long ExclusiveOr(uint x, int y)
	{
		return Int64Ops.ExclusiveOr(x, y);
	}

	[SpecialName]
	public static long ExclusiveOr(int x, uint y)
	{
		return Int64Ops.ExclusiveOr(x, y);
	}

	[SpecialName]
	public static int Compare(uint x, uint y)
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
	public static int Compare(uint x, int y)
	{
		return Int64Ops.Compare(x, y);
	}

	[SpecialName]
	public static int Compare(int x, uint y)
	{
		return Int64Ops.Compare(x, y);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static sbyte ConvertToSByte(uint x)
	{
		if (x <= 127)
		{
			return (sbyte)x;
		}
		throw Converter.CannotConvertOverflow("SByte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static byte ConvertToByte(uint x)
	{
		if (0 <= x && x <= 255)
		{
			return (byte)x;
		}
		throw Converter.CannotConvertOverflow("Byte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static short ConvertToInt16(uint x)
	{
		if (x <= 32767)
		{
			return (short)x;
		}
		throw Converter.CannotConvertOverflow("Int16", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ushort ConvertToUInt16(uint x)
	{
		if (0 <= x && x <= 65535)
		{
			return (ushort)x;
		}
		throw Converter.CannotConvertOverflow("UInt16", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static int ConvertToInt32(uint x)
	{
		if (x <= int.MaxValue)
		{
			return (int)x;
		}
		throw Converter.CannotConvertOverflow("Int32", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static long ConvertToInt64(uint x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static ulong ConvertToUInt64(uint x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static float ConvertToSingle(uint x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static double ConvertToDouble(uint x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static uint Getreal(uint x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static uint Getimag(uint x)
	{
		return 0u;
	}

	public static uint conjugate(uint x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static uint Getnumerator(uint x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static uint Getdenominator(uint x)
	{
		return 1u;
	}

	public static string __hex__(uint value)
	{
		return BigIntegerOps.__hex__(value);
	}

	public static int bit_length(uint value)
	{
		return MathUtils.BitLengthUnsigned(value);
	}
}

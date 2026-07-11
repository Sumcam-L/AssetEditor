using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class ByteOps
{
	internal static byte ToByteChecked(this int item)
	{
		try
		{
			return checked((byte)item);
		}
		catch (OverflowException)
		{
			throw PythonOps.ValueError("byte must be in range(0, 256)");
		}
	}

	internal static byte ToByteChecked(this BigInteger item)
	{
		if (item.AsInt32(out var ret))
		{
			return ret.ToByteChecked();
		}
		throw PythonOps.ValueError("byte must be in range(0, 256)");
	}

	internal static byte ToByteChecked(this double item)
	{
		try
		{
			return checked((byte)item);
		}
		catch (OverflowException)
		{
			throw PythonOps.ValueError("byte must be in range(0, 256)");
		}
	}

	internal static bool IsSign(this byte ch)
	{
		if (ch != 43)
		{
			return ch == 45;
		}
		return true;
	}

	internal static byte ToUpper(this byte p)
	{
		if (p >= 97 && p <= 122)
		{
			p -= 32;
		}
		return p;
	}

	internal static byte ToLower(this byte p)
	{
		if (p >= 65 && p <= 90)
		{
			p += 32;
		}
		return p;
	}

	internal static bool IsLower(this byte p)
	{
		if (p >= 97)
		{
			return p <= 122;
		}
		return false;
	}

	internal static bool IsUpper(this byte p)
	{
		if (p >= 65)
		{
			return p <= 90;
		}
		return false;
	}

	internal static bool IsDigit(this byte b)
	{
		if (b >= 48)
		{
			return b <= 57;
		}
		return false;
	}

	internal static bool IsLetter(this byte b)
	{
		if (!b.IsLower())
		{
			return b.IsUpper();
		}
		return true;
	}

	internal static bool IsWhiteSpace(this byte b)
	{
		if (b != 32 && b != 9 && b != 10 && b != 13 && b != 12)
		{
			return b == 11;
		}
		return true;
	}

	internal static void AppendJoin(object value, int index, List<byte> byteList)
	{
		if (value is IList<byte> collection)
		{
			byteList.AddRange(collection);
			return;
		}
		throw PythonOps.TypeError("sequence item {0}: expected bytes or byte array, {1} found", index.ToString(), PythonOps.GetPythonTypeName(value));
	}

	internal static IList<byte> CoerceBytes(object obj)
	{
		if (!(obj is IList<byte> result))
		{
			throw PythonOps.TypeError("expected string, got {0} Type", PythonTypeOps.GetName(obj));
		}
		return result;
	}

	internal static List<byte> GetBytes(ICollection bytes)
	{
		return GetBytes(bytes, GetByte);
	}

	internal static List<byte> GetBytes(ICollection bytes, Func<object, byte> conversion)
	{
		List<byte> list = new List<byte>(bytes.Count);
		foreach (object @byte in bytes)
		{
			list.Add(conversion(@byte));
		}
		return list;
	}

	internal static byte GetByteStringOk(object o)
	{
		string text;
		if (!object.ReferenceEquals(text = o as string, null))
		{
			if (text.Length == 1)
			{
				return ToByteChecked(text[0]);
			}
			throw PythonOps.TypeError("an integer or string of size 1 is required");
		}
		Extensible<string> extensible;
		if (!object.ReferenceEquals(extensible = o as Extensible<string>, null))
		{
			if (extensible.Value.Length == 1)
			{
				return ToByteChecked(extensible.Value[0]);
			}
			throw PythonOps.TypeError("an integer or string of size 1 is required");
		}
		return GetByteListOk(o);
	}

	internal static byte GetByteListOk(object o)
	{
		if (o is IList<byte> list)
		{
			if (list.Count == 1)
			{
				return list[0];
			}
			throw PythonOps.ValueError("an integer or string of size 1 is required");
		}
		return GetByte(o);
	}

	internal static byte GetByte(object o)
	{
		if (o is int)
		{
			return ((int)o).ToByteChecked();
		}
		if (o is BigInteger)
		{
			return ((BigInteger)o).ToByteChecked();
		}
		if (o is double)
		{
			return ((double)o).ToByteChecked();
		}
		if (o is Extensible<int> extensible)
		{
			return extensible.Value.ToByteChecked();
		}
		Extensible<BigInteger> extensible2;
		if (!object.ReferenceEquals(extensible2 = o as Extensible<BigInteger>, null))
		{
			return extensible2.Value.ToByteChecked();
		}
		Extensible<double> extensible3;
		if (!object.ReferenceEquals(extensible3 = o as Extensible<double>, null))
		{
			return extensible3.Value.ToByteChecked();
		}
		if (o is byte)
		{
			return (byte)o;
		}
		if (o is sbyte)
		{
			return ToByteChecked((sbyte)o);
		}
		if (o is char)
		{
			return ToByteChecked((char)o);
		}
		if (o is short)
		{
			return ToByteChecked((short)o);
		}
		if (o is ushort)
		{
			return ToByteChecked((ushort)o);
		}
		if (o is uint)
		{
			return ((BigInteger)(uint)o).ToByteChecked();
		}
		if (o is float)
		{
			return ToByteChecked((float)o);
		}
		if (Converter.TryConvertToIndex(o, out int index))
		{
			return index.ToByteChecked();
		}
		throw PythonOps.TypeError("an integer or string of size 1 is required");
	}

	[StaticExtensionMethod]
	public static object __new__(PythonType cls)
	{
		return __new__(cls, (byte)0);
	}

	[StaticExtensionMethod]
	public static object __new__(PythonType cls, object value)
	{
		if (cls != DynamicHelpers.GetPythonTypeFromType(typeof(byte)))
		{
			throw PythonOps.TypeError("Byte.__new__: first argument must be Byte type.");
		}
		if (value is IConvertible convertible)
		{
			switch (convertible.GetTypeCode())
			{
			case TypeCode.Byte:
				return (byte)value;
			case TypeCode.SByte:
				return (byte)(sbyte)value;
			case TypeCode.Int16:
				return (byte)(short)value;
			case TypeCode.UInt16:
				return (byte)(ushort)value;
			case TypeCode.Int32:
				return (byte)(int)value;
			case TypeCode.UInt32:
				return (byte)(uint)value;
			case TypeCode.Int64:
				return (byte)(long)value;
			case TypeCode.UInt64:
				return (byte)(ulong)value;
			case TypeCode.Single:
				return (byte)(float)value;
			case TypeCode.Double:
				return (byte)(double)value;
			}
		}
		if (value is string)
		{
			return byte.Parse((string)value);
		}
		if (value is BigInteger)
		{
			return (byte)(BigInteger)value;
		}
		if (value is Extensible<BigInteger>)
		{
			return (byte)((Extensible<BigInteger>)value).Value;
		}
		if (value is Extensible<double>)
		{
			return (byte)((Extensible<double>)value).Value;
		}
		throw PythonOps.ValueError("invalid value for Byte.__new__");
	}

	[SpecialName]
	public static byte Plus(byte x)
	{
		return x;
	}

	[SpecialName]
	public static object Negate(byte x)
	{
		return Int16Ops.Negate(x);
	}

	[SpecialName]
	public static byte Abs(byte x)
	{
		return x;
	}

	[SpecialName]
	public static object OnesComplement(byte x)
	{
		return Int16Ops.OnesComplement(x);
	}

	public static bool __nonzero__(byte x)
	{
		return x != 0;
	}

	public static byte __trunc__(byte x)
	{
		return x;
	}

	public static int __hash__(byte x)
	{
		return x;
	}

	[SpecialName]
	public static object Add(byte x, byte y)
	{
		short num = (short)(x + y);
		if (0 <= num && num <= 255)
		{
			return (byte)num;
		}
		return num;
	}

	[SpecialName]
	public static object Add(byte x, sbyte y)
	{
		return Int16Ops.Add(x, y);
	}

	[SpecialName]
	public static object Add(sbyte x, byte y)
	{
		return Int16Ops.Add(x, y);
	}

	[SpecialName]
	public static object Subtract(byte x, byte y)
	{
		short num = (short)(x - y);
		if (0 <= num && num <= 255)
		{
			return (byte)num;
		}
		return num;
	}

	[SpecialName]
	public static object Subtract(byte x, sbyte y)
	{
		return Int16Ops.Subtract(x, y);
	}

	[SpecialName]
	public static object Subtract(sbyte x, byte y)
	{
		return Int16Ops.Subtract(x, y);
	}

	[SpecialName]
	public static object Multiply(byte x, byte y)
	{
		short num = (short)(x * y);
		if (0 <= num && num <= 255)
		{
			return (byte)num;
		}
		return num;
	}

	[SpecialName]
	public static object Multiply(byte x, sbyte y)
	{
		return Int16Ops.Multiply(x, y);
	}

	[SpecialName]
	public static object Multiply(sbyte x, byte y)
	{
		return Int16Ops.Multiply(x, y);
	}

	[SpecialName]
	public static object Divide(byte x, byte y)
	{
		return FloorDivide(x, y);
	}

	[SpecialName]
	public static object Divide(byte x, sbyte y)
	{
		return Int16Ops.Divide(x, y);
	}

	[SpecialName]
	public static object Divide(sbyte x, byte y)
	{
		return Int16Ops.Divide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(byte x, byte y)
	{
		return DoubleOps.TrueDivide((int)x, (int)y);
	}

	[SpecialName]
	public static double TrueDivide(byte x, sbyte y)
	{
		return Int16Ops.TrueDivide(x, y);
	}

	[SpecialName]
	public static double TrueDivide(sbyte x, byte y)
	{
		return Int16Ops.TrueDivide(x, y);
	}

	[SpecialName]
	public static byte FloorDivide(byte x, byte y)
	{
		return (byte)(x / y);
	}

	[SpecialName]
	public static object FloorDivide(byte x, sbyte y)
	{
		return Int16Ops.FloorDivide(x, y);
	}

	[SpecialName]
	public static object FloorDivide(sbyte x, byte y)
	{
		return Int16Ops.FloorDivide(x, y);
	}

	[SpecialName]
	public static byte Mod(byte x, byte y)
	{
		return (byte)(x % y);
	}

	[SpecialName]
	public static short Mod(byte x, sbyte y)
	{
		return Int16Ops.Mod(x, y);
	}

	[SpecialName]
	public static short Mod(sbyte x, byte y)
	{
		return Int16Ops.Mod(x, y);
	}

	[SpecialName]
	public static object Power(byte x, byte y)
	{
		return Int32Ops.Power(x, y);
	}

	[SpecialName]
	public static object Power(byte x, sbyte y)
	{
		return Int16Ops.Power(x, y);
	}

	[SpecialName]
	public static object Power(sbyte x, byte y)
	{
		return Int16Ops.Power(x, y);
	}

	[SpecialName]
	public static object LeftShift(byte x, [NotNull] BigInteger y)
	{
		return BigIntegerOps.LeftShift(x, y);
	}

	[SpecialName]
	public static byte RightShift(byte x, [NotNull] BigInteger y)
	{
		return (byte)BigIntegerOps.RightShift(x, y);
	}

	[SpecialName]
	public static object LeftShift(byte x, int y)
	{
		return Int32Ops.LeftShift(x, y);
	}

	[SpecialName]
	public static byte RightShift(byte x, int y)
	{
		return (byte)Int32Ops.RightShift(x, y);
	}

	[SpecialName]
	public static byte BitwiseAnd(byte x, byte y)
	{
		return (byte)(x & y);
	}

	[SpecialName]
	public static short BitwiseAnd(byte x, sbyte y)
	{
		return Int16Ops.BitwiseAnd(x, y);
	}

	[SpecialName]
	public static short BitwiseAnd(sbyte x, byte y)
	{
		return Int16Ops.BitwiseAnd(x, y);
	}

	[SpecialName]
	public static byte BitwiseOr(byte x, byte y)
	{
		return (byte)(x | y);
	}

	[SpecialName]
	public static short BitwiseOr(byte x, sbyte y)
	{
		return Int16Ops.BitwiseOr(x, y);
	}

	[SpecialName]
	public static short BitwiseOr(sbyte x, byte y)
	{
		return Int16Ops.BitwiseOr(x, y);
	}

	[SpecialName]
	public static byte ExclusiveOr(byte x, byte y)
	{
		return (byte)(x ^ y);
	}

	[SpecialName]
	public static short ExclusiveOr(byte x, sbyte y)
	{
		return Int16Ops.ExclusiveOr(x, y);
	}

	[SpecialName]
	public static short ExclusiveOr(sbyte x, byte y)
	{
		return Int16Ops.ExclusiveOr(x, y);
	}

	[SpecialName]
	public static int Compare(byte x, byte y)
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
	public static int Compare(byte x, sbyte y)
	{
		return Int16Ops.Compare(x, y);
	}

	[SpecialName]
	public static int Compare(sbyte x, byte y)
	{
		return Int16Ops.Compare(x, y);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static sbyte ConvertToSByte(byte x)
	{
		if (x <= 127)
		{
			return (sbyte)x;
		}
		throw Converter.CannotConvertOverflow("SByte", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static short ConvertToInt16(byte x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static ushort ConvertToUInt16(byte x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static int ConvertToInt32(byte x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static uint ConvertToUInt32(byte x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static long ConvertToInt64(byte x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static ulong ConvertToUInt64(byte x)
	{
		return x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static float ConvertToSingle(byte x)
	{
		return (int)x;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static double ConvertToDouble(byte x)
	{
		return (int)x;
	}

	[SpecialName]
	[PropertyMethod]
	public static byte Getreal(byte x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static byte Getimag(byte x)
	{
		return 0;
	}

	public static byte conjugate(byte x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static byte Getnumerator(byte x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static byte Getdenominator(byte x)
	{
		return 1;
	}

	public static string __hex__(byte value)
	{
		return BigIntegerOps.__hex__(value);
	}

	public static int bit_length(byte value)
	{
		return MathUtils.BitLength(value);
	}
}

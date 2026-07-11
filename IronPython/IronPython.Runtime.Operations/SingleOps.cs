using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations;

public static class SingleOps
{
	[SpecialName]
	public static bool LessThan(float x, float y)
	{
		return x < y;
	}

	[SpecialName]
	public static bool LessThanOrEqual(float x, float y)
	{
		if (x == y)
		{
			return !float.IsNaN(x);
		}
		return x < y;
	}

	[SpecialName]
	public static bool GreaterThan(float x, float y)
	{
		return x > y;
	}

	[SpecialName]
	public static bool GreaterThanOrEqual(float x, float y)
	{
		if (x == y)
		{
			return !float.IsNaN(x);
		}
		return x > y;
	}

	[SpecialName]
	public static bool Equals(float x, float y)
	{
		if (x == y)
		{
			return !float.IsNaN(x);
		}
		return x == y;
	}

	[SpecialName]
	public static bool NotEquals(float x, float y)
	{
		return !Equals(x, y);
	}

	[SpecialName]
	public static float Mod(float x, float y)
	{
		return (float)DoubleOps.Mod(x, y);
	}

	[SpecialName]
	public static float Power(float x, float y)
	{
		return (float)DoubleOps.Power(x, y);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls)
	{
		if (cls == TypeCache.Single)
		{
			return 0f;
		}
		return cls.CreateInstance(context);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, object x)
	{
		if (cls != TypeCache.Single)
		{
			return cls.CreateInstance(context, x);
		}
		if (x is string)
		{
			return ParseFloat((string)x);
		}
		if (x is Extensible<string>)
		{
			return ParseFloat(((Extensible<string>)x).Value);
		}
		if (x is char)
		{
			return ParseFloat(ScriptingRuntimeHelpers.CharToString((char)x));
		}
		if (Converter.TryConvertToDouble(x, out var result))
		{
			return (float)result;
		}
		if (x is Complex)
		{
			throw PythonOps.TypeError("can't convert complex to Single; use abs(z)");
		}
		object obj = PythonOps.CallWithContext(context, PythonOps.GetBoundAttr(context, x, "__float__"));
		if (obj is double)
		{
			return (float)(double)obj;
		}
		throw PythonOps.TypeError("__float__ returned non-float (type %s)", DynamicHelpers.GetPythonType(obj));
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, IList<byte> s)
	{
		if (!(s is IPythonObject o) || !PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, "__float__", out var value))
		{
			value = ParseFloat(s.MakeString());
		}
		if (!(value is double))
		{
			throw PythonOps.TypeError("__float__ returned non-float (type %s)", DynamicHelpers.GetPythonType(value));
		}
		if (cls == TypeCache.Single)
		{
			return (float)value;
		}
		return cls.CreateInstance(context, (float)value);
	}

	private static object ParseFloat(string x)
	{
		try
		{
			return (float)LiteralParser.ParseFloat(x);
		}
		catch (FormatException)
		{
			throw PythonOps.ValueError("invalid literal for Single(): {0}", x);
		}
	}

	public static string __str__(CodeContext context, float x)
	{
		StringFormatter stringFormatter = new StringFormatter(context, "%.6g", x);
		stringFormatter._TrailingZeroAfterWholeFloat = true;
		return stringFormatter.Format();
	}

	public static string __repr__(CodeContext context, float self)
	{
		return __str__(context, self);
	}

	public static string __format__(CodeContext context, float self, [NotNull] string formatSpec)
	{
		return DoubleOps.__format__(context, self, formatSpec);
	}

	public static int __hash__(float x)
	{
		return DoubleOps.__hash__(x);
	}

	public static double __float__(float x)
	{
		return x;
	}

	[SpecialName]
	public static float Plus(float x)
	{
		return x;
	}

	[SpecialName]
	public static float Negate(float x)
	{
		return 0f - x;
	}

	[SpecialName]
	public static float Abs(float x)
	{
		return Math.Abs(x);
	}

	public static bool __nonzero__(float x)
	{
		return x != 0f;
	}

	public static object __trunc__(float x)
	{
		if (x >= 2.1474836E+09f || x <= -2.1474836E+09f)
		{
			return (BigInteger)x;
		}
		return (int)x;
	}

	[SpecialName]
	public static float Add(float x, float y)
	{
		return x + y;
	}

	[SpecialName]
	public static float Subtract(float x, float y)
	{
		return x - y;
	}

	[SpecialName]
	public static float Multiply(float x, float y)
	{
		return x * y;
	}

	[SpecialName]
	public static float Divide(float x, float y)
	{
		return TrueDivide(x, y);
	}

	[SpecialName]
	public static float TrueDivide(float x, float y)
	{
		if (y == 0f)
		{
			throw PythonOps.ZeroDivisionError();
		}
		return x / y;
	}

	[SpecialName]
	public static float FloorDivide(float x, float y)
	{
		if (y == 0f)
		{
			throw PythonOps.ZeroDivisionError();
		}
		return (float)Math.Floor(x / y);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static sbyte ConvertToSByte(float x)
	{
		if (-128f <= x && x <= 127f)
		{
			return (sbyte)x;
		}
		throw Converter.CannotConvertOverflow("SByte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static byte ConvertToByte(float x)
	{
		if (0f <= x && x <= 255f)
		{
			return (byte)x;
		}
		throw Converter.CannotConvertOverflow("Byte", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static short ConvertToInt16(float x)
	{
		if (-32768f <= x && x <= 32767f)
		{
			return (short)x;
		}
		throw Converter.CannotConvertOverflow("Int16", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ushort ConvertToUInt16(float x)
	{
		if (0f <= x && x <= 65535f)
		{
			return (ushort)x;
		}
		throw Converter.CannotConvertOverflow("UInt16", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static int ConvertToInt32(float x)
	{
		if (-2.1474836E+09f <= x && x <= 2.1474836E+09f)
		{
			return (int)x;
		}
		throw Converter.CannotConvertOverflow("Int32", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static uint ConvertToUInt32(float x)
	{
		if (0f <= x && x <= 4.2949673E+09f)
		{
			return (uint)x;
		}
		throw Converter.CannotConvertOverflow("UInt32", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static long ConvertToInt64(float x)
	{
		if (-9.223372E+18f <= x && x <= 9.223372E+18f)
		{
			return (long)x;
		}
		throw Converter.CannotConvertOverflow("Int64", x);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static ulong ConvertToUInt64(float x)
	{
		if (0f <= x && x <= 1.8446744E+19f)
		{
			return (ulong)x;
		}
		throw Converter.CannotConvertOverflow("UInt64", x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static double ConvertToDouble(float x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static float Getreal(float x)
	{
		return x;
	}

	[SpecialName]
	[PropertyMethod]
	public static float Getimag(float x)
	{
		return 0f;
	}

	public static float conjugate(float x)
	{
		return x;
	}
}

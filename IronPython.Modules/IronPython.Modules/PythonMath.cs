using System;
using System.Collections;
using System.Numerics;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonMath
{
	public const string __doc__ = "Provides common mathematical functions.";

	public const double pi = Math.PI;

	public const double e = Math.E;

	private const double degreesToRadians = Math.PI / 180.0;

	private const int Bias = 1022;

	public static double degrees(double radians)
	{
		return Check(radians, radians / (Math.PI / 180.0));
	}

	public static double radians(double degrees)
	{
		return Check(degrees, degrees * (Math.PI / 180.0));
	}

	public static double fmod(double v, double w)
	{
		return Check(v, w, v % w);
	}

	public static double fsum(IEnumerable e)
	{
		IEnumerator enumerator = e.GetEnumerator();
		double num = 0.0;
		while (enumerator.MoveNext())
		{
			num += Converter.ConvertToDouble(enumerator.Current);
		}
		return num;
	}

	public static PythonTuple frexp(double v)
	{
		if (double.IsInfinity(v) || double.IsNaN(v))
		{
			return PythonTuple.MakeTuple(v, 0.0);
		}
		int num = 0;
		double m = 0.0;
		if (v == 0.0)
		{
			m = 0.0;
			num = 0;
		}
		else
		{
			byte[] bytes = BitConverter.GetBytes(v);
			if (!BitConverter.IsLittleEndian)
			{
				throw new NotImplementedException();
			}
			DecomposeLe(bytes, out m, out num);
		}
		return PythonTuple.MakeTuple(m, num);
	}

	public static PythonTuple modf(double v)
	{
		if (double.IsInfinity(v))
		{
			return PythonTuple.MakeTuple(0.0, v);
		}
		double num = v % 1.0;
		v -= num;
		return PythonTuple.MakeTuple(num, v);
	}

	public static double ldexp(double v, BigInteger w)
	{
		if (v == 0.0 || double.IsInfinity(v))
		{
			return v;
		}
		return Check(v, v * Math.Pow(2.0, (double)w));
	}

	public static double hypot(double v, double w)
	{
		if (double.IsInfinity(v) || double.IsInfinity(w))
		{
			return double.PositiveInfinity;
		}
		return Check(v, w, MathUtils.Hypot(v, w));
	}

	public static double pow(double v, double exp)
	{
		if (v == 1.0 || exp == 0.0)
		{
			return 1.0;
		}
		if (double.IsNaN(v) || double.IsNaN(exp))
		{
			return double.NaN;
		}
		if (v == 0.0)
		{
			if (exp > 0.0)
			{
				return 0.0;
			}
			throw PythonOps.ValueError("math domain error");
		}
		if (double.IsPositiveInfinity(exp))
		{
			if (v > 1.0 || v < -1.0)
			{
				return double.PositiveInfinity;
			}
			if (v == -1.0)
			{
				return 1.0;
			}
			return 0.0;
		}
		if (double.IsNegativeInfinity(exp))
		{
			if (v > 1.0 || v < -1.0)
			{
				return 0.0;
			}
			if (v == -1.0)
			{
				return 1.0;
			}
			return double.PositiveInfinity;
		}
		return Check(v, exp, Math.Pow(v, exp));
	}

	public static double log(double v0)
	{
		if (v0 <= 0.0)
		{
			throw PythonOps.ValueError("math domain error");
		}
		return Check(v0, Math.Log(v0));
	}

	public static double log(double v0, double v1)
	{
		if (v0 <= 0.0 || v1 == 0.0)
		{
			throw PythonOps.ValueError("math domain error");
		}
		if (v1 == 1.0)
		{
			throw PythonOps.ZeroDivisionError("float division");
		}
		if (v1 == double.PositiveInfinity)
		{
			return 0.0;
		}
		return Check(Math.Log(v0, v1));
	}

	public static double log(BigInteger value)
	{
		if (value.Sign <= 0)
		{
			throw PythonOps.ValueError("math domain error");
		}
		return value.Log();
	}

	public static double log(object value)
	{
		if (Converter.TryConvertToDouble(value, out var result))
		{
			return log(result);
		}
		return log(Converter.ConvertToBigInteger(value));
	}

	public static double log(BigInteger value, double newBase)
	{
		if (newBase <= 0.0 || value <= 0L)
		{
			throw PythonOps.ValueError("math domain error");
		}
		if (newBase == 1.0)
		{
			throw PythonOps.ZeroDivisionError("float division");
		}
		if (newBase == double.PositiveInfinity)
		{
			return 0.0;
		}
		return Check(value.Log(newBase));
	}

	public static double log(object value, double newBase)
	{
		if (Converter.TryConvertToDouble(value, out var result))
		{
			return log(result, newBase);
		}
		return log(Converter.ConvertToBigInteger(value), newBase);
	}

	public static double log10(double v0)
	{
		if (v0 <= 0.0)
		{
			throw PythonOps.ValueError("math domain error");
		}
		return Check(v0, Math.Log10(v0));
	}

	public static double log10(BigInteger value)
	{
		if (value.Sign <= 0)
		{
			throw PythonOps.ValueError("math domain error");
		}
		return value.Log10();
	}

	public static double log10(object value)
	{
		if (Converter.TryConvertToDouble(value, out var result))
		{
			return log10(result);
		}
		return log10(Converter.ConvertToBigInteger(value));
	}

	public static double log1p(double v0)
	{
		if (double.IsPositiveInfinity(v0))
		{
			return double.PositiveInfinity;
		}
		double num = v0 + 1.0;
		if (num == 1.0)
		{
			return v0;
		}
		return log(num) * v0 / (num - 1.0);
	}

	public static double log1p(BigInteger value)
	{
		return log(value + BigInteger.One);
	}

	public static double log1p(object value)
	{
		if (Converter.TryConvertToDouble(value, out var result))
		{
			return log1p(result);
		}
		return log1p(Converter.ConvertToBigInteger(value));
	}

	public static double expm1(double v0)
	{
		return Check(v0, Math.Tanh(v0 / 2.0) * (Math.Exp(v0) + 1.0));
	}

	public static double asinh(double v0)
	{
		if (v0 == 0.0 || double.IsInfinity(v0))
		{
			return v0;
		}
		if (Math.Abs(v0) > 1.0)
		{
			return Math.Log(v0) + Math.Log(1.0 + MathUtils.Hypot(1.0, 1.0 / v0));
		}
		return Math.Log(v0 + MathUtils.Hypot(1.0, v0));
	}

	public static double asinh(object value)
	{
		if (Converter.TryConvertToDouble(value, out var result))
		{
			return asinh(result);
		}
		return asinh(Converter.ConvertToBigInteger(value));
	}

	public static double acosh(double v0)
	{
		if (v0 < 1.0)
		{
			throw PythonOps.ValueError("math domain error");
		}
		if (double.IsPositiveInfinity(v0))
		{
			return double.PositiveInfinity;
		}
		double num = Math.Sqrt(v0 + 1.0);
		return Math.Log(num) + Math.Log(v0 / num + Math.Sqrt(v0 - 1.0));
	}

	public static double acosh(object value)
	{
		if (Converter.TryConvertToDouble(value, out var result))
		{
			return acosh(result);
		}
		return acosh(Converter.ConvertToBigInteger(value));
	}

	public static double atanh(double v0)
	{
		if (v0 >= 1.0 || v0 <= -1.0)
		{
			throw PythonOps.ValueError("math domain error");
		}
		if (v0 == 0.0)
		{
			return v0;
		}
		return Math.Log((1.0 + v0) / (1.0 - v0)) * 0.5;
	}

	public static double atanh(BigInteger value)
	{
		if (value == 0L)
		{
			return 0.0;
		}
		throw PythonOps.ValueError("math domain error");
	}

	public static double atanh(object value)
	{
		if (Converter.TryConvertToDouble(value, out var result))
		{
			return atanh(result);
		}
		return atanh(Converter.ConvertToBigInteger(value));
	}

	public static double atan2(double v0, double v1)
	{
		if (double.IsNaN(v0) || double.IsNaN(v1))
		{
			return double.NaN;
		}
		if (double.IsInfinity(v0))
		{
			if (double.IsPositiveInfinity(v1))
			{
				return Math.PI / 4.0 * (double)Math.Sign(v0);
			}
			if (double.IsNegativeInfinity(v1))
			{
				return Math.PI * 3.0 / 4.0 * (double)Math.Sign(v0);
			}
			return Math.PI / 2.0 * (double)Math.Sign(v0);
		}
		if (double.IsInfinity(v1))
		{
			if (!(v1 > 0.0))
			{
				return Math.PI * (double)DoubleOps.Sign(v0);
			}
			return 0.0;
		}
		return Math.Atan2(v0, v1);
	}

	public static double erf(double v0)
	{
		return MathUtils.Erf(v0);
	}

	public static double erfc(double v0)
	{
		return MathUtils.ErfComplement(v0);
	}

	public static object factorial(double v0)
	{
		if (v0 % 1.0 != 0.0)
		{
			throw PythonOps.ValueError("factorial() only accepts integral values");
		}
		if (v0 < 0.0)
		{
			throw PythonOps.ValueError("factorial() not defined for negative values");
		}
		BigInteger bigInteger = 1;
		for (BigInteger bigInteger2 = (BigInteger)v0; bigInteger2 > BigInteger.One; bigInteger2 -= BigInteger.One)
		{
			bigInteger *= bigInteger2;
		}
		if (bigInteger > 2147483647L)
		{
			return bigInteger;
		}
		return (int)bigInteger;
	}

	public static object factorial(BigInteger value)
	{
		if (value < 0L)
		{
			throw PythonOps.ValueError("factorial() not defined for negative values");
		}
		BigInteger bigInteger = 1;
		for (BigInteger bigInteger2 = value; bigInteger2 > BigInteger.One; bigInteger2 -= BigInteger.One)
		{
			bigInteger *= bigInteger2;
		}
		if (bigInteger > 2147483647L)
		{
			return bigInteger;
		}
		return (int)bigInteger;
	}

	public static object factorial(object value)
	{
		if (Converter.TryConvertToDouble(value, out var result))
		{
			return factorial(result);
		}
		return factorial(Converter.ConvertToBigInteger(value));
	}

	public static double gamma(double v0)
	{
		return Check(v0, MathUtils.Gamma(v0));
	}

	public static double lgamma(double v0)
	{
		return Check(v0, MathUtils.LogGamma(v0));
	}

	public static object trunc(CodeContext context, object value)
	{
		if (PythonOps.TryGetBoundAttr(value, "__trunc__", out var ret))
		{
			return PythonOps.CallWithContext(context, ret);
		}
		throw PythonOps.AttributeError("__trunc__");
	}

	public static bool isinf(double v0)
	{
		return double.IsInfinity(v0);
	}

	public static bool isinf(BigInteger value)
	{
		return false;
	}

	public static bool isinf(object value)
	{
		if (Converter.TryConvertToDouble(value, out var result))
		{
			return isinf(result);
		}
		return false;
	}

	public static bool isnan(double v0)
	{
		return double.IsNaN(v0);
	}

	public static bool isnan(BigInteger value)
	{
		return false;
	}

	public static bool isnan(object value)
	{
		if (Converter.TryConvertToDouble(value, out var result))
		{
			return isnan(result);
		}
		return false;
	}

	public static double copysign(double x, double y)
	{
		return (double)DoubleOps.Sign(y) * Math.Abs(x);
	}

	public static double copysign(object x, object y)
	{
		if (!Converter.TryConvertToDouble(x, out var result) || !Converter.TryConvertToDouble(y, out var result2))
		{
			throw PythonOps.TypeError("TypeError: a float is required");
		}
		return (double)DoubleOps.Sign(result2) * Math.Abs(result);
	}

	private static void SetExponentLe(byte[] v, int exp)
	{
		exp += 1022;
		ushort num = LdExponentLe(v);
		ushort num2 = (ushort)((num & 0x800F) | (exp << 4));
		StExponentLe(v, num2);
	}

	private static int IntExponentLe(byte[] v)
	{
		ushort num = LdExponentLe(v);
		return ((num & 0x7FF0) >> 4) - 1022;
	}

	private static ushort LdExponentLe(byte[] v)
	{
		return (ushort)(v[6] | (v[7] << 8));
	}

	private static long LdMantissaLe(byte[] v)
	{
		int num = v[0] | (v[1] << 8) | (v[2] << 16) | (v[3] << 24);
		int num2 = v[4] | (v[5] << 8) | ((v[6] & 0xF) << 16);
		return num | num2;
	}

	private static void StExponentLe(byte[] v, ushort e)
	{
		v[6] = (byte)e;
		v[7] = (byte)(e >> 8);
	}

	private static bool IsDenormalizedLe(byte[] v)
	{
		ushort num = LdExponentLe(v);
		long num2 = LdMantissaLe(v);
		if ((num & 0x7FF0) == 0)
		{
			return num2 != 0;
		}
		return false;
	}

	private static void DecomposeLe(byte[] v, out double m, out int e)
	{
		if (IsDenormalizedLe(v))
		{
			m = BitConverter.ToDouble(v, 0);
			m *= Math.Pow(2.0, 1022.0);
			v = BitConverter.GetBytes(m);
			e = IntExponentLe(v) - 1022;
		}
		else
		{
			e = IntExponentLe(v);
		}
		SetExponentLe(v, 0);
		m = BitConverter.ToDouble(v, 0);
	}

	private static double Check(double v)
	{
		return PythonOps.CheckMath(v);
	}

	private static double Check(double input, double output)
	{
		return PythonOps.CheckMath(input, output);
	}

	private static double Check(double in0, double in1, double output)
	{
		return PythonOps.CheckMath(in0, in1, output);
	}

	public static double cos(double v0)
	{
		return Check(v0, Math.Cos(v0));
	}

	public static double sin(double v0)
	{
		return Check(v0, Math.Sin(v0));
	}

	public static double tan(double v0)
	{
		return Check(v0, Math.Tan(v0));
	}

	public static double cosh(double v0)
	{
		return Check(v0, Math.Cosh(v0));
	}

	public static double sinh(double v0)
	{
		return Check(v0, Math.Sinh(v0));
	}

	public static double tanh(double v0)
	{
		return Check(v0, Math.Tanh(v0));
	}

	public static double acos(double v0)
	{
		return Check(v0, Math.Acos(v0));
	}

	public static double asin(double v0)
	{
		return Check(v0, Math.Asin(v0));
	}

	public static double atan(double v0)
	{
		return Check(v0, Math.Atan(v0));
	}

	public static double floor(double v0)
	{
		return Check(v0, Math.Floor(v0));
	}

	public static double ceil(double v0)
	{
		return Check(v0, Math.Ceiling(v0));
	}

	public static double fabs(double v0)
	{
		return Check(v0, Math.Abs(v0));
	}

	public static double sqrt(double v0)
	{
		return Check(v0, Math.Sqrt(v0));
	}

	public static double exp(double v0)
	{
		return Check(v0, Math.Exp(v0));
	}
}

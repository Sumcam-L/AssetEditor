using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class ComplexOps
{
	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls)
	{
		if (cls == TypeCache.Complex)
		{
			return default(Complex);
		}
		return cls.CreateInstance(context);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, [DefaultParameterValue(null)] object real, [DefaultParameterValue(null)] object imag)
	{
		Complex self2;
		Complex self = (self2 = (self2 = default(Complex)));
		if (real == null && imag == null && cls == TypeCache.Complex)
		{
			throw PythonOps.TypeError("argument must be a string or a number");
		}
		if (imag != null)
		{
			if (real is string)
			{
				throw PythonOps.TypeError("complex() can't take second arg if first is a string");
			}
			if (imag is string)
			{
				throw PythonOps.TypeError("complex() second arg can't be a string");
			}
			self2 = Converter.ConvertToComplex(imag);
		}
		if (real != null)
		{
			if (real is string)
			{
				self = LiteralParser.ParseComplex((string)real);
			}
			else if (real is Extensible<string>)
			{
				self = LiteralParser.ParseComplex(((Extensible<string>)real).Value);
			}
			else if (real is Complex)
			{
				if (imag == null && cls == TypeCache.Complex)
				{
					return real;
				}
				self = (Complex)real;
			}
			else
			{
				self = Converter.ConvertToComplex(real);
			}
		}
		double num = self.Real - self2.Imaginary();
		double num2 = self.Imaginary() + self2.Real;
		if (cls == TypeCache.Complex)
		{
			return new Complex(num, num2);
		}
		return cls.CreateInstance(context, num, num2);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, double real)
	{
		if (cls == TypeCache.Complex)
		{
			return new Complex(real, 0.0);
		}
		return cls.CreateInstance(context, real, 0.0);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, double real, double imag)
	{
		if (cls == TypeCache.Complex)
		{
			return new Complex(real, imag);
		}
		return cls.CreateInstance(context, real, imag);
	}

	[SpecialName]
	[PropertyMethod]
	public static double Getreal(Complex self)
	{
		return self.Real;
	}

	[SpecialName]
	[PropertyMethod]
	public static double Getimag(Complex self)
	{
		return self.Imaginary();
	}

	[SpecialName]
	public static Complex Add(Complex x, Complex y)
	{
		return x + y;
	}

	[SpecialName]
	public static Complex Subtract(Complex x, Complex y)
	{
		return x - y;
	}

	[SpecialName]
	public static Complex Multiply(Complex x, Complex y)
	{
		return x * y;
	}

	[SpecialName]
	public static Complex Divide(Complex x, Complex y)
	{
		if (y.IsZero())
		{
			throw new DivideByZeroException("complex division by zero");
		}
		return x / y;
	}

	[SpecialName]
	public static Complex TrueDivide(Complex x, Complex y)
	{
		return Divide(x, y);
	}

	[SpecialName]
	public static Complex op_Power(Complex x, Complex y)
	{
		if (x.IsZero())
		{
			if (y.Real < 0.0 || y.Imaginary() != 0.0)
			{
				throw PythonOps.ZeroDivisionError("0.0 to a negative or complex power");
			}
			if (!y.IsZero())
			{
				return Complex.Zero;
			}
			return Complex.One;
		}
		if (y.Imaginary == 0.0)
		{
			int num = (int)y.Real;
			if (num >= 0 && y.Real == (double)num)
			{
				Complex one = Complex.One;
				if (num == 0)
				{
					return one;
				}
				Complex complex = x;
				while (num != 0)
				{
					if ((num & 1) != 0)
					{
						one *= complex;
					}
					complex *= complex;
					num >>= 1;
				}
				return one;
			}
		}
		return x.Pow(y);
	}

	[PythonHidden]
	public static Complex Power(Complex x, Complex y)
	{
		return op_Power(x, y);
	}

	[SpecialName]
	public static Complex FloorDivide(CodeContext context, Complex x, Complex y)
	{
		PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "complex divmod(), // and % are deprecated");
		return MathUtils.MakeReal(PythonOps.CheckMath(Math.Floor(Divide(x, y).Real)));
	}

	[SpecialName]
	public static Complex Mod(CodeContext context, Complex x, Complex y)
	{
		Complex complex = FloorDivide(context, x, y);
		return x - complex * y;
	}

	[SpecialName]
	public static PythonTuple DivMod(CodeContext context, Complex x, Complex y)
	{
		Complex complex = FloorDivide(context, x, y);
		return PythonTuple.MakeTuple(complex, x - complex * y);
	}

	public static int __hash__(Complex x)
	{
		if (x.Imaginary() == 0.0)
		{
			return DoubleOps.__hash__(x.Real);
		}
		return x.GetHashCode();
	}

	public static bool __nonzero__(Complex x)
	{
		return !x.IsZero();
	}

	public static Complex conjugate(Complex x)
	{
		return x.Conjugate();
	}

	public static object __getnewargs__(CodeContext context, Complex self)
	{
		return PythonTuple.MakeTuple(PythonOps.GetBoundAttr(context, self, "real"), PythonOps.GetBoundAttr(context, self, "imag"));
	}

	public static object __pos__(Complex x)
	{
		return x;
	}

	public static object __coerce__(Complex x, object y)
	{
		if (Converter.TryConvertToComplex(y, out var result))
		{
			if (double.IsInfinity(result.Real) && (y is BigInteger || y is Extensible<BigInteger>))
			{
				throw new OverflowException("long int too large to convert to float");
			}
			return PythonTuple.MakeTuple(x, result);
		}
		return NotImplementedType.Value;
	}

	public static string __str__(CodeContext context, Complex x)
	{
		if (x.Real != 0.0)
		{
			if (x.Imaginary() < 0.0 || DoubleOps.IsNegativeZero(x.Imaginary()))
			{
				return "(" + FormatComplexValue(context, x.Real) + FormatComplexValue(context, x.Imaginary()) + "j)";
			}
			return "(" + FormatComplexValue(context, x.Real) + "+" + FormatComplexValue(context, x.Imaginary()) + "j)";
		}
		return FormatComplexValue(context, x.Imaginary()) + "j";
	}

	public static string __repr__(CodeContext context, Complex x)
	{
		return __str__(context, x);
	}

	public static double __float__(Complex self)
	{
		throw PythonOps.TypeError("can't convert complex to float; use abs(z)");
	}

	public static int __int__(Complex self)
	{
		throw PythonOps.TypeError(" can't convert complex to int; use int(abs(z))");
	}

	public static BigInteger __long__(Complex self)
	{
		throw PythonOps.TypeError("can't convert complex to long; use long(abs(z))");
	}

	private static string FormatComplexValue(CodeContext context, double x)
	{
		StringFormatter stringFormatter = new StringFormatter(context, "%.6g", x);
		return stringFormatter.Format();
	}

	[SpecialName]
	public static double Abs(Complex x)
	{
		double num = x.Abs();
		if (double.IsInfinity(num) && !double.IsInfinity(x.Real) && !double.IsInfinity(x.Imaginary()))
		{
			throw PythonOps.OverflowError("absolute value too large");
		}
		return num;
	}

	[SpecialName]
	public static bool LessThan(Complex x, Complex y)
	{
		throw PythonOps.TypeError("complex is not an ordered type");
	}

	[SpecialName]
	public static bool LessThanOrEqual(Complex x, Complex y)
	{
		throw PythonOps.TypeError("complex is not an ordered type");
	}

	[SpecialName]
	public static bool GreaterThan(Complex x, Complex y)
	{
		throw PythonOps.TypeError("complex is not an ordered type");
	}

	[SpecialName]
	public static bool GreaterThanOrEqual(Complex x, Complex y)
	{
		throw PythonOps.TypeError("complex is not an ordered type");
	}
}

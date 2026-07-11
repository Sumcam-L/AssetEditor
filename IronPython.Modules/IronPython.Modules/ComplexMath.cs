using System;
using System.Numerics;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public class ComplexMath
{
	public const double pi = Math.PI;

	public const double e = Math.E;

	public const string __doc__ = "Provides access to functions for operating on complex numbers";

	public static Complex cos(object x)
	{
		Complex complexNum = GetComplexNum(x);
		if (double.IsNaN(complexNum.Imaginary()))
		{
			return new Complex(double.NaN, double.NaN);
		}
		if (double.IsInfinity(complexNum.Real))
		{
			throw PythonOps.ValueError("math domain error");
		}
		double real = Math.Cos(complexNum.Real) * Math.Cosh(complexNum.Imaginary());
		double imaginary = 0.0 - Math.Sin(complexNum.Real) * Math.Sinh(complexNum.Imaginary());
		return new Complex(real, imaginary);
	}

	public static Complex sin(object x)
	{
		Complex complexNum = GetComplexNum(x);
		if (double.IsNaN(complexNum.Imaginary()))
		{
			return new Complex(double.NaN, double.NaN);
		}
		if (double.IsInfinity(complexNum.Real))
		{
			throw PythonOps.ValueError("math domain error");
		}
		double real = Math.Sin(complexNum.Real) * Math.Cosh(complexNum.Imaginary());
		double imaginary = Math.Cos(complexNum.Real) * Math.Sinh(complexNum.Imaginary());
		return new Complex(real, imaginary);
	}

	public static Complex tan(object x)
	{
		Complex complexNum = GetComplexNum(x);
		if (double.IsPositiveInfinity(complexNum.Imaginary()))
		{
			return Complex.ImaginaryOne;
		}
		if (double.IsNegativeInfinity(complexNum.Imaginary()))
		{
			return new Complex(0.0, -1.0);
		}
		return sin(complexNum) / cos(complexNum);
	}

	public static Complex cosh(object x)
	{
		Complex complexNum = GetComplexNum(x);
		if (double.IsNaN(complexNum.Real))
		{
			return new Complex(double.NaN, double.NaN);
		}
		if (double.IsInfinity(complexNum.Imaginary()))
		{
			throw PythonOps.ValueError("math domain error");
		}
		double real = Math.Cosh(complexNum.Real) * Math.Cos(complexNum.Imaginary());
		double imaginary = Math.Sinh(complexNum.Real) * Math.Sin(complexNum.Imaginary());
		return new Complex(real, imaginary);
	}

	public static Complex sinh(object x)
	{
		Complex complexNum = GetComplexNum(x);
		if (double.IsNaN(complexNum.Real))
		{
			return new Complex(double.NaN, double.NaN);
		}
		if (double.IsInfinity(complexNum.Imaginary()))
		{
			throw PythonOps.ValueError("math domain error");
		}
		double real = Math.Sinh(complexNum.Real) * Math.Cos(complexNum.Imaginary());
		double imaginary = Math.Cosh(complexNum.Real) * Math.Sin(complexNum.Imaginary());
		return new Complex(real, imaginary);
	}

	public static Complex tanh(object x)
	{
		Complex complexNum = GetComplexNum(x);
		if (double.IsPositiveInfinity(complexNum.Real))
		{
			return Complex.One;
		}
		if (double.IsNegativeInfinity(complexNum.Real))
		{
			return new Complex(-1.0, 0.0);
		}
		return sinh(complexNum) / cosh(complexNum);
	}

	public static Complex acos(object x)
	{
		Complex complexNum = GetComplexNum(x);
		double num = MathUtils.Hypot(complexNum.Real + 1.0, complexNum.Imaginary());
		double num2 = MathUtils.Hypot(complexNum.Real - 1.0, complexNum.Imaginary());
		double num3 = 0.5 * (num + num2);
		double real = Math.Acos(0.5 * (num - num2));
		double num4 = Math.Log(num3 + Math.Sqrt(num3 + 1.0) * Math.Sqrt(num3 - 1.0));
		return new Complex(real, (complexNum.Imaginary() >= 0.0) ? num4 : (0.0 - num4));
	}

	public static Complex asin(object x)
	{
		Complex complexNum = GetComplexNum(x);
		double num = MathUtils.Hypot(complexNum.Real + 1.0, complexNum.Imaginary());
		double num2 = MathUtils.Hypot(complexNum.Real - 1.0, complexNum.Imaginary());
		double num3 = 0.5 * (num + num2);
		double real = Math.Asin(0.5 * (num - num2));
		double num4 = Math.Log(num3 + Math.Sqrt(num3 + 1.0) * Math.Sqrt(num3 - 1.0));
		return new Complex(real, (complexNum.Imaginary() >= 0.0) ? num4 : (0.0 - num4));
	}

	public static Complex atan(object x)
	{
		Complex complexNum = GetComplexNum(x);
		Complex imaginaryOne = Complex.ImaginaryOne;
		return imaginaryOne * 0.5 * (log(imaginaryOne + complexNum) - log(imaginaryOne - complexNum));
	}

	public static Complex acosh(object x)
	{
		Complex complexNum = GetComplexNum(x);
		return log(complexNum + sqrt(complexNum + 1) * sqrt(complexNum - 1));
	}

	public static Complex asinh(object x)
	{
		Complex complexNum = GetComplexNum(x);
		if (complexNum.IsZero())
		{
			return MathUtils.MakeImaginary(complexNum.Imaginary());
		}
		Complex complex = 1 / complexNum;
		return log(complexNum) + log(1 + sqrt(complex * complex + 1));
	}

	public static Complex atanh(object x)
	{
		Complex complexNum = GetComplexNum(x);
		return (log(1 + complexNum) - log(1 - complexNum)) * 0.5;
	}

	public static Complex log(object x)
	{
		Complex complexNum = GetComplexNum(x);
		if (complexNum.IsZero())
		{
			throw PythonOps.ValueError("math domain error");
		}
		double d = complexNum.Abs();
		double angle = GetAngle(complexNum);
		return new Complex(Math.Log(d), angle);
	}

	public static Complex log(object x, object logBase)
	{
		return log(x) / log(logBase);
	}

	public static Complex log10(object x)
	{
		return log(x, 10);
	}

	public static Complex exp(object x)
	{
		Complex complexNum = GetComplexNum(x);
		if (complexNum.Imaginary() == 0.0)
		{
			if (double.IsPositiveInfinity(complexNum.Real))
			{
				return new Complex(double.PositiveInfinity, 0.0);
			}
			double num = Math.Exp(complexNum.Real);
			if (double.IsInfinity(num))
			{
				throw PythonOps.OverflowError("math range error");
			}
			return new Complex(num, 0.0);
		}
		if (double.IsNegativeInfinity(complexNum.Real))
		{
			return Complex.Zero;
		}
		if (double.IsNaN(complexNum.Real))
		{
			return new Complex(double.NaN, double.NaN);
		}
		if (double.IsNaN(complexNum.Imaginary()))
		{
			return new Complex(double.IsInfinity(complexNum.Real) ? double.PositiveInfinity : double.NaN, double.NaN);
		}
		if (double.IsInfinity(complexNum.Imaginary()))
		{
			throw PythonOps.ValueError("math domain error");
		}
		double num2 = Math.Cos(complexNum.Imaginary());
		double num3 = ((num2 > 0.0) ? Math.Exp(complexNum.Real + Math.Log(num2)) : ((!(num2 < 0.0)) ? 0.0 : (0.0 - Math.Exp(complexNum.Real + Math.Log(0.0 - num2)))));
		double num4 = Math.Sin(complexNum.Imaginary());
		double num5 = ((num4 > 0.0) ? Math.Exp(complexNum.Real + Math.Log(num4)) : ((!(num4 < 0.0)) ? 0.0 : (0.0 - Math.Exp(complexNum.Real + Math.Log(0.0 - num4)))));
		if ((double.IsInfinity(num3) || double.IsInfinity(num5)) && !double.IsInfinity(complexNum.Real))
		{
			throw PythonOps.OverflowError("math range error");
		}
		return new Complex(num3, num5);
	}

	public static Complex sqrt(object x)
	{
		Complex complexNum = GetComplexNum(x);
		if (complexNum.Imaginary() == 0.0)
		{
			if (complexNum.Real >= 0.0)
			{
				return MathUtils.MakeReal(Math.Sqrt(complexNum.Real));
			}
			return MathUtils.MakeImaginary(Math.Sqrt(0.0 - complexNum.Real));
		}
		double num = complexNum.Abs() + complexNum.Real;
		double real = Math.Sqrt(0.5 * num);
		double imaginary = complexNum.Imaginary() / Math.Sqrt(2.0 * num);
		return new Complex(real, imaginary);
	}

	public static double phase(object x)
	{
		Complex complexNum = GetComplexNum(x);
		return GetAngle(complexNum);
	}

	public static PythonTuple polar(object x)
	{
		Complex complexNum = GetComplexNum(x);
		double[] array = new double[2]
		{
			complexNum.Abs(),
			GetAngle(complexNum)
		};
		if (double.IsInfinity(array[0]) && !IsInfinity(complexNum))
		{
			throw PythonOps.OverflowError("math range error");
		}
		return new PythonTuple(array);
	}

	public static Complex rect(double r, double theta)
	{
		if (r == 0.0)
		{
			return Complex.Zero;
		}
		if (theta == 0.0)
		{
			return new Complex(r, 0.0);
		}
		if (double.IsNaN(r))
		{
			return new Complex(double.NaN, double.NaN);
		}
		if (double.IsNaN(theta))
		{
			return new Complex(double.IsInfinity(r) ? double.PositiveInfinity : double.NaN, double.NaN);
		}
		if (double.IsInfinity(theta))
		{
			throw PythonOps.ValueError("math domain error");
		}
		return new Complex(r * Math.Cos(theta), r * Math.Sin(theta));
	}

	public static bool isinf(object x)
	{
		Complex complexNum = GetComplexNum(x);
		return IsInfinity(complexNum);
	}

	public static bool isnan(object x)
	{
		Complex complexNum = GetComplexNum(x);
		return IsNaN(complexNum);
	}

	private static bool IsInfinity(Complex num)
	{
		if (!double.IsInfinity(num.Real))
		{
			return double.IsInfinity(num.Imaginary());
		}
		return true;
	}

	private static bool IsNaN(Complex num)
	{
		if (!double.IsNaN(num.Real))
		{
			return double.IsNaN(num.Imaginary());
		}
		return true;
	}

	private static double GetAngle(Complex num)
	{
		if (IsNaN(num))
		{
			return double.NaN;
		}
		if (double.IsPositiveInfinity(num.Real))
		{
			if (double.IsPositiveInfinity(num.Imaginary()))
			{
				return Math.PI / 4.0;
			}
			if (double.IsNegativeInfinity(num.Imaginary()))
			{
				return -Math.PI / 4.0;
			}
			return 0.0;
		}
		if (double.IsNegativeInfinity(num.Real))
		{
			if (double.IsPositiveInfinity(num.Imaginary()))
			{
				return Math.PI * 3.0 / 4.0;
			}
			if (double.IsNegativeInfinity(num.Imaginary()))
			{
				return Math.PI * -3.0 / 4.0;
			}
			return (double)DoubleOps.Sign(num.Imaginary()) * Math.PI;
		}
		if (num.Real == 0.0)
		{
			if (num.Imaginary() != 0.0)
			{
				return Math.PI / 2.0 * (double)Math.Sign(num.Imaginary());
			}
			return (DoubleOps.IsPositiveZero(num.Real) ? 0.0 : Math.PI) * (double)DoubleOps.Sign(num.Imaginary());
		}
		return Math.Atan2(num.Imaginary(), num.Real);
	}

	private static Complex GetComplexNum(object num)
	{
		if (num != null)
		{
			return Converter.ConvertToComplex(num);
		}
		throw new NullReferenceException("The input was null");
	}
}

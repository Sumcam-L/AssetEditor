using System;
using System.Windows;
using System.Windows.Media;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf;

public static class MathUtil
{
	public static Matrix GetTransform(Point translation, double scale)
	{
		Matrix result = default(Matrix);
		result.Translate(translation.X, translation.Y);
		result.Scale(scale, scale);
		return result;
	}

	public static Matrix GetTransform(Point translation, double xScale, double yScale)
	{
		Matrix result = default(Matrix);
		result.Translate(translation.X, translation.Y);
		result.Scale(xScale, yScale);
		return result;
	}

	public static Point Transform(Matrix matrix, Point p)
	{
		return matrix.Transform(p);
	}

	public static double Transform(Matrix matrix, double x)
	{
		return matrix.Transform(new Point(x, 0.0)).X;
	}

	public static double TransformVector(Matrix matrix, double x)
	{
		return matrix.Transform(new Vector(x, 0.0)).X;
	}

	public static Point InverseTransform(Matrix matrix, Point p)
	{
		if (matrix.HasInverse)
		{
			matrix.Invert();
		}
		return matrix.Transform(p);
	}

	public static Rect Transform(Matrix matrix, Rect r)
	{
		Point[] array = new Point[2]
		{
			new Point(r.Left, r.Top),
			new Point(r.Right, r.Bottom)
		};
		matrix.Transform(array);
		return new Rect(array[0].X, array[0].Y, array[1].X - array[0].X, array[1].Y - array[0].Y);
	}

	public static Rect InverseTransform(Matrix matrix, Rect r)
	{
		if (matrix.HasInverse)
		{
			matrix.Invert();
		}
		Point[] array = new Point[2]
		{
			new Point(r.Left, r.Top),
			new Point(r.Right, r.Bottom)
		};
		matrix.Transform(array);
		return new Rect(array[0].X, array[0].Y, array[1].X - array[0].X, array[1].Y - array[0].Y);
	}

	public static double RoundToDoublePrecision(double value, int digits)
	{
		double num = Math.Abs(value);
		if (num >= 1.0)
		{
			digits -= (int)Math.Ceiling(Math.Log10(num));
		}
		if (digits >= 0)
		{
			value = Math.Round(value, digits);
		}
		return value;
	}

	public static double CalculateDistance(Rect startRect, Keys arrow, Rect targetRect)
	{
		double num;
		double num2;
		double num3;
		double num4;
		double num5;
		double num6;
		double num7;
		switch (arrow)
		{
		case Keys.Up:
			num = startRect.Left;
			num2 = startRect.Right;
			num3 = 0.0 - startRect.Top;
			num4 = targetRect.Left;
			num5 = targetRect.Right;
			num6 = 0.0 - targetRect.Bottom;
			num7 = 0.0 - targetRect.Top;
			break;
		case Keys.Right:
			num = startRect.Top;
			num2 = startRect.Bottom;
			num3 = startRect.Right;
			num4 = targetRect.Top;
			num5 = targetRect.Bottom;
			num6 = targetRect.Left;
			num7 = targetRect.Right;
			break;
		case Keys.Down:
			num = startRect.Left;
			num2 = startRect.Right;
			num3 = startRect.Bottom;
			num4 = targetRect.Left;
			num5 = targetRect.Right;
			num6 = targetRect.Top;
			num7 = targetRect.Bottom;
			break;
		case Keys.Left:
			num = startRect.Top;
			num2 = startRect.Bottom;
			num3 = 0.0 - startRect.Left;
			num4 = targetRect.Top;
			num5 = targetRect.Bottom;
			num6 = 0.0 - targetRect.Right;
			num7 = 0.0 - targetRect.Right;
			break;
		default:
			throw new ArgumentException("'arrow' must be a single arrow key");
		}
		if (num3 > num7)
		{
			return double.MaxValue;
		}
		double num8 = num7 - num3;
		if (num5 < num - num8 || num4 > num2 + num8)
		{
			return double.MaxValue;
		}
		double num9 = num6 - num3;
		if (num5 < num)
		{
			double num10 = num - num5;
			return num10 * num10 + num9 * num9;
		}
		if (num4 > num2)
		{
			double num11 = num4 - num2;
			return num11 * num11 + num9 * num9;
		}
		return num9 * num9;
	}
}

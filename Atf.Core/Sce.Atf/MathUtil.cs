using System;
using System.Collections.Generic;

namespace Sce.Atf;

public static class MathUtil
{
	private static readonly byte[] s_logBase2Table;

	public static int LogBase2(int bitField)
	{
		int num = bitField >> 16;
		int num2 = bitField >> 24;
		int num3 = bitField >> 8;
		if (num > 0)
		{
			if (num2 > 0)
			{
				return 24 + s_logBase2Table[num2];
			}
			return 16 + s_logBase2Table[num & 0xFF];
		}
		if (num3 > 0)
		{
			return 8 + s_logBase2Table[num3];
		}
		return s_logBase2Table[bitField];
	}

	public static bool OnlyOneBitSet(int bitField)
	{
		return bitField != 0 && (bitField & (bitField - 1)) == 0;
	}

	public static bool AreApproxEqual(float[] v1, float[] v2, double error)
	{
		if (v1.Length != v2.Length)
		{
			throw new ArgumentException("Incompatible arrays");
		}
		for (int i = 0; i < v1.Length; i++)
		{
			if (!AreApproxEqual(v1[i], v2[i], error))
			{
				return false;
			}
		}
		return true;
	}

	public static bool AreApproxEqual(double[] v1, double[] v2, double error)
	{
		if (v1.Length != v2.Length)
		{
			throw new ArgumentException("Incompatible arrays");
		}
		for (int i = 0; i < v1.Length; i++)
		{
			if (!AreApproxEqual(v1[i], v2[i], error))
			{
				return false;
			}
		}
		return true;
	}

	public static bool AreApproxEqual(double x, double y, double error)
	{
		double num = Math.Abs(x - y);
		double num2 = Math.Max(Math.Abs(x), Math.Abs(y));
		return num <= error * num2;
	}

	public static double Snap(double x, double step)
	{
		if (step < 0.0)
		{
			throw new ArgumentException("step can't be negative");
		}
		double num = x;
		if (Math.Abs(num) * 1.1125369292536007E-308 < step)
		{
			num /= step;
			num = Math.Floor(num + 0.5);
			num *= step;
		}
		return num;
	}

	public static int Snap(int x, int step)
	{
		if (step < 0)
		{
			throw new ArgumentException("step can't be negative");
		}
		if (step == 0)
		{
			return x;
		}
		int num = ((x < 0) ? (x - step / 2) : (x + step / 2));
		return num - num % step;
	}

	public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
	{
		if (Comparer<T>.Default.Compare(value, min) < 0)
		{
			return min;
		}
		if (Comparer<T>.Default.Compare(value, max) > 0)
		{
			return max;
		}
		return value;
	}

	public static T Max<T>(T x, T y) where T : IComparable<T>
	{
		return (Comparer<T>.Default.Compare(x, y) > 0) ? x : y;
	}

	public static T Min<T>(T x, T y) where T : IComparable<T>
	{
		return (Comparer<T>.Default.Compare(x, y) < 0) ? x : y;
	}

	public static int Wrap(int value, int min, int max)
	{
		int num = ((value < min) ? ((value - min + 1) / (max - min) - 1) : ((value - min) / (max - min)));
		value -= num * (max - min);
		return value;
	}

	public static double Wrap(double value, double min, double max)
	{
		if (value > max)
		{
			return Wrap(min + (value - max), min, max);
		}
		if (value < min)
		{
			return Wrap(max - (min - value), min, max);
		}
		return value;
	}

	public static float Interp(float value, float min, float max)
	{
		return min + value * (max - min);
	}

	public static float ReverseInterp(float value, float min, float max)
	{
		return (value - min) / (max - min);
	}

	public static float CatmullRomInterp(float p1, float p2, float p3, float p4, float t)
	{
		float num = t * t;
		float num2 = t * num;
		float num3 = 2f * p2 + (p3 - p1) * t + (2f * p1 - 5f * p2 + 4f * p3 - p4) * num + (3f * p2 - p1 + p4 - 3f * p3) * num2;
		return num3 * 0.5f;
	}

	public static float HermiteInterp(float p1, float tan1, float p2, float tan2, float t)
	{
		float num = t * t;
		float num2 = num * t;
		return (2f * num2 - 3f * num + 1f) * p1 + (num2 - 2f * num + t) * tan1 + (-2f * num2 + 3f * num) * p2 + (num2 - num) * tan2;
	}

	public static float CubicSplineInterp(float p1, float p2, float t)
	{
		float num = t * t * (3f - 2f * t);
		return p1 + num * (p2 - p1);
	}

	public static float Remap(float value, float min, float max, float newMin, float newMax)
	{
		return Interp(ReverseInterp(value, min, max), newMin, newMax);
	}

	public static double Closest(double value, double cmp1, double cmp2)
	{
		if (Math.Abs(cmp1 - value) <= Math.Abs(cmp2 - value))
		{
			return cmp1;
		}
		return cmp2;
	}

	public static double Remainder(double x, double y)
	{
		return x - y * Math.Floor(x / y);
	}

	static MathUtil()
	{
		s_logBase2Table = new byte[256];
		s_logBase2Table[0] = 0;
		s_logBase2Table[1] = 0;
		for (int i = 2; i < 256; i++)
		{
			s_logBase2Table[i] = (byte)(1 + s_logBase2Table[i / 2]);
		}
	}
}

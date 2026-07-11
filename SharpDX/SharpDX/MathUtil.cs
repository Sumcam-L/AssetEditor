using System;

namespace SharpDX;

public static class MathUtil
{
	public const float ZeroTolerance = 1E-06f;

	public const float Pi = (float)Math.PI;

	public const float TwoPi = (float)Math.PI * 2f;

	public const float PiOverTwo = (float)Math.PI / 2f;

	public const float PiOverFour = (float)Math.PI / 4f;

	public unsafe static bool NearEqual(float a, float b)
	{
		if (IsZero(a - b))
		{
			return true;
		}
		int num = *(int*)(&a);
		int num2 = *(int*)(&b);
		if (num < 0 != num2 < 0)
		{
			return false;
		}
		int num3 = Math.Abs(num - num2);
		return num3 <= 4;
	}

	public static bool IsZero(float a)
	{
		return Math.Abs(a) < 1E-06f;
	}

	public static bool IsOne(float a)
	{
		return IsZero(a - 1f);
	}

	public static bool WithinEpsilon(float a, float b, float epsilon)
	{
		float num = a - b;
		if (0f - epsilon <= num)
		{
			return num <= epsilon;
		}
		return false;
	}

	public static float RevolutionsToDegrees(float revolution)
	{
		return revolution * 360f;
	}

	public static float RevolutionsToRadians(float revolution)
	{
		return revolution * ((float)Math.PI * 2f);
	}

	public static float RevolutionsToGradians(float revolution)
	{
		return revolution * 400f;
	}

	public static float DegreesToRevolutions(float degree)
	{
		return degree / 360f;
	}

	public static float DegreesToRadians(float degree)
	{
		return degree * ((float)Math.PI / 180f);
	}

	public static float RadiansToRevolutions(float radian)
	{
		return radian / ((float)Math.PI * 2f);
	}

	public static float RadiansToGradians(float radian)
	{
		return radian * (200f / (float)Math.PI);
	}

	public static float GradiansToRevolutions(float gradian)
	{
		return gradian / 400f;
	}

	public static float GradiansToDegrees(float gradian)
	{
		return gradian * 0.9f;
	}

	public static float GradiansToRadians(float gradian)
	{
		return gradian * ((float)Math.PI / 200f);
	}

	public static float RadiansToDegrees(float radian)
	{
		return radian * (180f / (float)Math.PI);
	}

	public static float Clamp(float value, float min, float max)
	{
		if (!(value < min))
		{
			if (!(value > max))
			{
				return value;
			}
			return max;
		}
		return min;
	}

	public static int Clamp(int value, int min, int max)
	{
		if (value >= min)
		{
			if (value <= max)
			{
				return value;
			}
			return max;
		}
		return min;
	}

	public static double Lerp(double from, double to, double amount)
	{
		return (1.0 - amount) * from + amount * to;
	}

	public static float Lerp(float from, float to, float amount)
	{
		return (1f - amount) * from + amount * to;
	}

	public static byte Lerp(byte from, byte to, float amount)
	{
		return (byte)Lerp((int)from, (int)to, amount);
	}

	public static float SmoothStep(float amount)
	{
		if (!(amount <= 0f))
		{
			if (!(amount >= 1f))
			{
				return amount * amount * (3f - 2f * amount);
			}
			return 1f;
		}
		return 0f;
	}

	public static float SmootherStep(float amount)
	{
		if (!(amount <= 0f))
		{
			if (!(amount >= 1f))
			{
				return amount * amount * amount * (amount * (amount * 6f - 15f) + 10f);
			}
			return 1f;
		}
		return 0f;
	}

	public static float Mod(float value, float modulo)
	{
		if (modulo == 0f)
		{
			return value;
		}
		return value % modulo;
	}

	public static float Mod2PI(float value)
	{
		return Mod(value, (float)Math.PI * 2f);
	}

	public static int Wrap(int value, int min, int max)
	{
		if (min > max)
		{
			throw new ArgumentException($"min {min} should be less than or equal to max {max}", "min");
		}
		int num = max - min + 1;
		if (value < min)
		{
			value += num * ((min - value) / num + 1);
		}
		return min + (value - min) % num;
	}

	public static float Wrap(float value, float min, float max)
	{
		if (NearEqual(min, max))
		{
			return min;
		}
		double num = min;
		double num2 = max;
		double num3 = value;
		if (num > num2)
		{
			throw new ArgumentException($"min {min} should be less than or equal to max {max}", "min");
		}
		double num4 = num2 - num;
		return (float)(num + (num3 - num) - num4 * Math.Floor((num3 - num) / num4));
	}

	public static float Gauss(float amplitude, float x, float y, float radX, float radY, float sigmaX, float sigmaY)
	{
		return (float)Gauss((double)amplitude, (double)x, (double)y, (double)radX, (double)radY, (double)sigmaX, (double)sigmaY);
	}

	public static double Gauss(double amplitude, double x, double y, double radX, double radY, double sigmaX, double sigmaY)
	{
		return amplitude * Math.E - (Math.Pow(x - radX / 2.0, 2.0) / (2.0 * Math.Pow(sigmaX, 2.0)) + Math.Pow(y - radY / 2.0, 2.0) / (2.0 * Math.Pow(sigmaY, 2.0)));
	}
}

using System;

namespace SharpDX;

public static class RandomUtil
{
	public static float NextFloat(this Random random, float min, float max)
	{
		return MathUtil.Lerp(min, max, (float)random.NextDouble());
	}

	public static double NextDouble(this Random random, double min, double max)
	{
		return MathUtil.Lerp(min, max, random.NextDouble());
	}

	public static long NextLong(this Random random)
	{
		byte[] array = new byte[8];
		random.NextBytes(array);
		return BitConverter.ToInt64(array, 0);
	}

	public static long NextLong(this Random random, long min, long max)
	{
		byte[] array = new byte[8];
		random.NextBytes(array);
		long num = BitConverter.ToInt64(array, 0);
		return Math.Abs(num % (max - min + 1)) + min;
	}

	public static Vector2 NextVector2(this Random random, Vector2 min, Vector2 max)
	{
		return new Vector2(random.NextFloat(min.X, max.X), random.NextFloat(min.Y, max.Y));
	}

	public static Vector3 NextVector3(this Random random, Vector3 min, Vector3 max)
	{
		return new Vector3(random.NextFloat(min.X, max.X), random.NextFloat(min.Y, max.Y), random.NextFloat(min.Z, max.Z));
	}

	public static Vector4 NextVector4(this Random random, Vector4 min, Vector4 max)
	{
		return new Vector4(random.NextFloat(min.X, max.X), random.NextFloat(min.Y, max.Y), random.NextFloat(min.Z, max.Z), random.NextFloat(min.W, max.W));
	}

	public static Color NextColor(this Random random)
	{
		return new Color(random.NextFloat(0f, 1f), random.NextFloat(0f, 1f), random.NextFloat(0f, 1f), 1f);
	}

	public static Color NextColor(this Random random, float minBrightness, float maxBrightness)
	{
		return new Color(random.NextFloat(minBrightness, maxBrightness), random.NextFloat(minBrightness, maxBrightness), random.NextFloat(minBrightness, maxBrightness), 1f);
	}

	public static Color NextColor(this Random random, float minBrightness, float maxBrightness, float alpha)
	{
		return new Color(random.NextFloat(minBrightness, maxBrightness), random.NextFloat(minBrightness, maxBrightness), random.NextFloat(minBrightness, maxBrightness), alpha);
	}

	public static Color NextColor(this Random random, float minBrightness, float maxBrightness, float minAlpha, float maxAlpha)
	{
		return new Color(random.NextFloat(minBrightness, maxBrightness), random.NextFloat(minBrightness, maxBrightness), random.NextFloat(minBrightness, maxBrightness), random.NextFloat(minAlpha, maxAlpha));
	}

	public static Point NextPoint(this Random random, Point min, Point max)
	{
		return new Point(random.Next(min.X, max.X), random.Next(min.Y, max.Y));
	}

	public static TimeSpan NextTime(this Random random, TimeSpan min, TimeSpan max)
	{
		return TimeSpan.FromTicks(random.NextLong(min.Ticks, max.Ticks));
	}
}

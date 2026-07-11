using System;

namespace Sce.Atf.VectorMath;

public struct Ray2F : IFormattable
{
	public Vec2F Origin;

	public Vec2F Direction;

	public Ray2F(Vec2F origin, Vec2F direction)
	{
		Origin = origin;
		Direction = direction;
	}

	public static bool Intersect(Ray2F r1, Ray2F r2, ref double t1, ref double t2)
	{
		double num = r2.Direction.Y * r1.Direction.X - r2.Direction.X * r1.Direction.Y;
		if (Math.Abs(num) < 9.999999974752427E-07)
		{
			return false;
		}
		Vec2F vec2F = Vec2F.Sub(r1.Origin, r2.Origin);
		t1 = (double)(r2.Direction.X * vec2F.Y - r2.Direction.Y * vec2F.X) / num;
		t2 = (double)(r1.Direction.X * vec2F.Y - r1.Direction.Y * vec2F.X) / num;
		return true;
	}

	public static bool Intersect(Ray2F ray, Box2F box, ref Vec2F intersection)
	{
		SlabIntersect(ray.Direction.X, ray.Origin.X, box.Min.X, box.Max.X, out var tmin, out var tmax);
		SlabIntersect(ray.Direction.Y, ray.Origin.Y, box.Min.Y, box.Max.Y, out var tmin2, out var tmax2);
		if (tmin > tmax2 || tmin2 > tmax)
		{
			return false;
		}
		tmin = Math.Max(tmin, tmin2);
		tmax = Math.Min(tmax, tmax2);
		if (tmin > 0f)
		{
			intersection = ray.Origin + ray.Direction * tmin;
			return true;
		}
		return false;
	}

	private static void SlabIntersect(float direction, float origin, float min, float max, out float tmin, out float tmax)
	{
		float num = 1f / direction;
		if (num >= 0f)
		{
			tmin = (min - origin) * num;
			tmax = (max - origin) * num;
		}
		else
		{
			tmin = (max - origin) * num;
			tmax = (min - origin) * num;
		}
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		string numberListSeparator = StringUtil.GetNumberListSeparator(formatProvider);
		if (format == null)
		{
			format = "R";
		}
		return string.Format("{0}{4} {1}{4} {2}{4} {3}", ((double)Origin.X).ToString(format, formatProvider), ((double)Origin.Y).ToString(format, formatProvider), ((double)Direction.X).ToString(format, formatProvider), ((double)Direction.Y).ToString(format, formatProvider), numberListSeparator);
	}
}

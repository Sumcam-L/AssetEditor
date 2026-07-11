using System;

namespace Sce.Atf.VectorMath;

public struct CircleF : IFormattable
{
	public Vec2F Center;

	public float Radius;

	public CircleF(Vec2F center, float radius)
	{
		Center = center;
		Radius = radius;
	}

	public CircleF(Vec2F p1, Vec2F p2, Vec2F p3)
	{
		Vec2F vec2F = Vec2F.Add(p1, p2);
		vec2F *= 0.5f;
		Vec2F origin = Vec2F.Add(p3, p2);
		origin *= 0.5f;
		Vec2F perp = Vec2F.Sub(p2, p1).Perp;
		Vec2F perp2 = Vec2F.Sub(p3, p2).Perp;
		double t = 0.0;
		double t2 = 0.0;
		if (Ray2F.Intersect(new Ray2F(vec2F, perp), new Ray2F(origin, perp2), ref t, ref t2))
		{
			Center = vec2F + perp * (float)t;
			Radius = Vec2F.Distance(Center, p1);
		}
		else
		{
			Center = new Vec2F(float.PositiveInfinity, float.PositiveInfinity);
			Radius = float.PositiveInfinity;
		}
	}

	public bool Contains(Vec2F p)
	{
		return Vec2F.Sub(p, Center).LengthSquared < Radius * Radius;
	}

	public static bool Project(Vec2F p, CircleF c, ref Vec2F projection)
	{
		Vec2F v = Vec2F.Sub(p, c.Center);
		float length = v.Length;
		bool result = false;
		if (length > 1E-06f * c.Radius)
		{
			result = true;
			float s = c.Radius / length;
			projection = Vec2F.Add(c.Center, Vec2F.Mul(v, s));
		}
		return result;
	}

	public static bool Intersect(CircleF c1, CircleF c2, ref Vec2F p1, ref Vec2F p2)
	{
		Vec2F v = c2.Center - c1.Center;
		double num = v.Length;
		if (num < 1E-06 || num > (double)(c1.Radius + c2.Radius))
		{
			return false;
		}
		v *= (float)(1.0 / num);
		Vec2F perp = v.Perp;
		double num2 = (num * num + (double)(c1.Radius * c1.Radius) - (double)(c2.Radius * c2.Radius)) / ((double)(2f * c1.Radius) * num);
		double num3 = Math.Sqrt(1.0 - num2 * num2);
		Vec2F vec2F = Vec2F.Mul(v, (float)((double)c1.Radius * num2));
		Vec2F vec2F2 = Vec2F.Mul(perp, (float)((double)c1.Radius * num3));
		p1 = c1.Center + vec2F + vec2F2;
		p2 = c1.Center + vec2F - vec2F2;
		return true;
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
		return string.Format("{0}{3} {1}{3} {2}", ((double)Center.X).ToString(format, formatProvider), ((double)Center.Y).ToString(format, formatProvider), ((double)Radius).ToString(format, formatProvider), numberListSeparator);
	}
}

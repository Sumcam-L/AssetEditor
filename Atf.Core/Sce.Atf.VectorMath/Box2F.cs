using System;

namespace Sce.Atf.VectorMath;

public class Box2F : IFormattable
{
	public Vec2F Min;

	public Vec2F Max;

	private bool m_empty = true;

	public bool IsEmpty => Min == Max;

	public Vec2F Centroid => Vec2F.Mul(Min + Max, 0.5f);

	public Box2F()
	{
		m_empty = true;
	}

	public Box2F(Vec2F min, Vec2F max)
	{
		Min = min;
		Max = max;
		m_empty = false;
	}

	public Box2F Extend(Vec2F p)
	{
		if (m_empty)
		{
			Min = (Max = p);
			m_empty = false;
		}
		else
		{
			Min.X = Math.Min(Min.X, p.X);
			Min.Y = Math.Min(Min.Y, p.Y);
			Max.X = Math.Max(Max.X, p.X);
			Max.Y = Math.Max(Max.Y, p.Y);
		}
		return this;
	}

	public Box2F Extend(float[] v)
	{
		if (v.Length >= 2)
		{
			if (m_empty)
			{
				Max.X = (Min.X = v[0]);
				Max.Y = (Min.Y = v[1]);
			}
			for (int i = 0; i < v.Length; i += 3)
			{
				Min.X = Math.Min(Min.X, v[i]);
				Min.Y = Math.Min(Min.Y, v[i + 1]);
				Max.X = Math.Max(Max.X, v[i]);
				Max.Y = Math.Max(Max.Y, v[i + 1]);
			}
		}
		return this;
	}

	public Box2F Extend(CircleF circle)
	{
		float radius = circle.Radius;
		Extend(circle.Center + new Vec2F(radius, radius));
		Extend(circle.Center - new Vec2F(radius, radius));
		return this;
	}

	public Box2F Extend(Box2F other)
	{
		if (!other.IsEmpty)
		{
			Extend(other.Min);
			Extend(other.Max);
		}
		return this;
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
		return string.Format("{0}{4} {1}{4} {2}{4} {3}", Min.X.ToString(format, formatProvider), Min.Y.ToString(format, formatProvider), Max.X.ToString(format, formatProvider), Max.Y.ToString(format, formatProvider), numberListSeparator);
	}
}

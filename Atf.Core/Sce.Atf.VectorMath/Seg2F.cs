using System;

namespace Sce.Atf.VectorMath;

public class Seg2F : IFormattable
{
	public Vec2F P1;

	public Vec2F P2;

	public const float DegenerateLength = 1E-05f;

	public Seg2F(Vec2F p1, Vec2F p2)
	{
		P1 = p1;
		P2 = p2;
	}

	public static Vec2F Project(Seg2F seg, Vec2F p)
	{
		Vec2F result = default(Vec2F);
		Vec2F vec2F = seg.P2 - seg.P1;
		Vec2F v = p - seg.P1;
		float num = Vec2F.Dot(vec2F, vec2F);
		if (num < 9.9999994E-11f)
		{
			return seg.P1;
		}
		float num2 = Vec2F.Dot(vec2F, v);
		if (num2 < 0f)
		{
			return seg.P1;
		}
		if (num2 > num)
		{
			return seg.P2;
		}
		double num3 = num2 / num;
		result.X = (float)((double)seg.P1.X + num3 * (double)vec2F.X);
		result.Y = (float)((double)seg.P1.Y + num3 * (double)vec2F.Y);
		return result;
	}

	public static float DistanceToSegment(Seg2F seg, Vec2F p)
	{
		Vec2F v = Project(seg, p);
		return Vec2F.Distance(p, v);
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
		return string.Format("{0}{4} {1}{4} {2}{4} {3}", P1.X.ToString(format, formatProvider), P1.Y.ToString(format, formatProvider), P2.X.ToString(format, formatProvider), P2.Y.ToString(format, formatProvider), numberListSeparator);
	}
}

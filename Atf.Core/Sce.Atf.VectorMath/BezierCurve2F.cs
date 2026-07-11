using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.VectorMath;

public class BezierCurve2F : IFormattable
{
	public Vec2F P1;

	public Vec2F P2;

	public Vec2F P3;

	public Vec2F P4;

	public float Flatness
	{
		get
		{
			Seg2F seg = new Seg2F(P1, P4);
			float val = Seg2F.DistanceToSegment(seg, P2);
			float val2 = Seg2F.DistanceToSegment(seg, P3);
			return Math.Max(val, val2);
		}
	}

	public BezierCurve2F(PointF p1, PointF p2, PointF p3, PointF p4)
		: this(new Vec2F(p1.X, p1.Y), new Vec2F(p2.X, p2.Y), new Vec2F(p3.X, p3.Y), new Vec2F(p4.X, p4.Y))
	{
	}

	public BezierCurve2F(Vec2F p1, Vec2F p2, Vec2F p3, Vec2F p4)
	{
		P1 = p1;
		P2 = p2;
		P3 = p3;
		P4 = p4;
	}

	public Vec2F Evaluate(float t)
	{
		float num = 1f - t;
		Vec2F vec2F = P1 * num + P2 * t;
		Vec2F vec2F2 = P2 * num + P3 * t;
		Vec2F vec2F3 = P3 * num + P4 * t;
		Vec2F vec2F4 = vec2F * num + vec2F2 * t;
		Vec2F vec2F5 = vec2F2 * num + vec2F3 * t;
		return vec2F4 * num + vec2F5 * t;
	}

	public void Subdivide(float t, out BezierCurve2F left, out BezierCurve2F right)
	{
		float num = 1f - t;
		Vec2F vec2F = P1 * num + P2 * t;
		Vec2F vec2F2 = P2 * num + P3 * t;
		Vec2F vec2F3 = P3 * num + P4 * t;
		Vec2F vec2F4 = vec2F * num + vec2F2 * t;
		Vec2F vec2F5 = vec2F2 * num + vec2F3 * t;
		Vec2F vec2F6 = vec2F4 * num + vec2F5 * t;
		left = new BezierCurve2F(P1, vec2F, vec2F4, vec2F6);
		right = new BezierCurve2F(vec2F6, vec2F5, vec2F3, P4);
	}

	public static bool Pick(BezierCurve2F curve, Vec2F p, float tolerance, ref Vec2F hitPoint)
	{
		Queue<BezierCurve2F> queue = new Queue<BezierCurve2F>();
		queue.Enqueue(curve);
		float num = float.MaxValue;
		Vec2F vec2F = default(Vec2F);
		while (queue.Count > 0)
		{
			BezierCurve2F bezierCurve2F = queue.Dequeue();
			Seg2F seg = new Seg2F(bezierCurve2F.P1, bezierCurve2F.P4);
			Vec2F vec2F2 = Seg2F.Project(seg, p);
			float num2 = Vec2F.Distance(p, vec2F2);
			float flatness = bezierCurve2F.Flatness;
			if (num2 - flatness > tolerance)
			{
				continue;
			}
			if (flatness <= tolerance)
			{
				if (num2 < num)
				{
					num = num2;
					vec2F = vec2F2;
				}
			}
			else
			{
				bezierCurve2F.Subdivide(0.5f, out var left, out var right);
				queue.Enqueue(left);
				queue.Enqueue(right);
			}
		}
		if (num < tolerance)
		{
			hitPoint = vec2F;
			return true;
		}
		return false;
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
		return string.Format("{0}{8} {1}{8} {2}{8} {3}{8} {4}{8} {5}{8} {6}{8} {7}", P1.X.ToString(format, formatProvider), P1.Y.ToString(format, formatProvider), P2.X.ToString(format, formatProvider), P2.Y.ToString(format, formatProvider), P3.X.ToString(format, formatProvider), P3.Y.ToString(format, formatProvider), P4.X.ToString(format, formatProvider), P4.Y.ToString(format, formatProvider), numberListSeparator);
	}
}

using System;

namespace Sce.Atf.VectorMath;

public struct AngleAxisF : IEquatable<AngleAxisF>, IFormattable
{
	public float Angle;

	public Vec3F Axis;

	private static double EPS = 1E-06;

	public AngleAxisF(float angle, Vec3F axis)
	{
		Angle = angle;
		Axis.X = axis.X;
		Axis.Y = axis.Y;
		Axis.Z = axis.Z;
		Axis.Normalize();
	}

	public void Set(Matrix3F m)
	{
		double num = m.M32 - m.M23;
		double num2 = m.M13 - m.M31;
		double num3 = m.M21 - m.M12;
		double num4 = num * num + num2 * num2 + num3 * num3;
		if (num4 > EPS)
		{
			num4 = Math.Sqrt(num4);
			double y = 0.5 * num4;
			double x = 0.5 * ((double)(m.M11 + m.M22 + m.M33) - 1.0);
			Angle = (float)Math.Atan2(y, x);
			double num5 = 1.0 / num4;
			Axis.X = (float)(num * num5);
			Axis.Y = (float)(num2 * num5);
			Axis.Z = (float)(num3 * num5);
		}
		else
		{
			Axis.X = 0f;
			Axis.Y = 1f;
			Axis.Z = 0f;
			Angle = 0f;
		}
	}

	public void Set(QuatF q)
	{
		double num = q.X * q.X + q.Y * q.Y + q.Z * q.Z;
		if (num > EPS)
		{
			num = Math.Sqrt(num);
			double num2 = 1.0 / num;
			Axis.X = (float)((double)q.X * num2);
			Axis.Y = (float)((double)q.Y * num2);
			Axis.Z = (float)((double)q.Z * num2);
			Angle = (float)(2.0 * Math.Atan2(num, q.W));
		}
		else
		{
			Axis.X = 0f;
			Axis.Y = 1f;
			Axis.Z = 0f;
			Angle = 0f;
		}
	}

	public bool Equals(AngleAxisF a)
	{
		return Angle == a.Angle && Axis.Equals(a.Axis);
	}

	public bool Equals(AngleAxisF a, double eps)
	{
		return ((double)Math.Abs(Angle - a.Angle) < eps && Axis.Equals(a.Axis, eps)) || ((double)Math.Abs(Angle + a.Angle) < eps && Axis.Equals(a.Axis * -1f, eps));
	}

	public override bool Equals(object obj)
	{
		if (obj is AngleAxisF a)
		{
			return Equals(a);
		}
		return false;
	}

	public override int GetHashCode()
	{
		long num = 1L;
		num = 31 * num + Angle.GetHashCode();
		num = 31 * num + Axis.GetHashCode();
		return (int)(num ^ (num >> 32));
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
		return string.Format("{1}{0} {2}{0} {3}{0} {4}", numberListSeparator, Angle.ToString(format, formatProvider), Axis.X.ToString(format, formatProvider), Axis.Y.ToString(format, formatProvider), Axis.Z.ToString(format, formatProvider));
	}
}

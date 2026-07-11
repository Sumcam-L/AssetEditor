using System;

namespace Sce.Atf.VectorMath;

public struct QuatF : IEquatable<QuatF>, IFormattable
{
	public float X;

	public float Y;

	public float Z;

	public float W;

	public static readonly QuatF Identity;

	private const double EPS = 1E-06;

	private const double EPS2 = 1E-30;

	public QuatF Conjugate => new QuatF(0f - X, 0f - Y, 0f - Z, W);

	public QuatF Inverse
	{
		get
		{
			double num = 1.0 / (double)(X * X + Y * Y + Z * Z + W * W);
			return new QuatF((float)((double)(0f - X) * num), (float)((double)(0f - Y) * num), (float)((double)(0f - Z) * num), (float)((double)W * num));
		}
	}

	public QuatF(float x, float y, float z, float w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public QuatF(float[] coords)
	{
		if (coords.Length < 4)
		{
			throw new ArgumentException("not enough coords");
		}
		X = coords[0];
		Y = coords[1];
		Z = coords[2];
		W = coords[3];
	}

	public QuatF(Vec3F v)
	{
		X = v.X;
		Y = v.Y;
		Z = v.Z;
		W = 1f;
	}

	public QuatF(Vec3F v, float w)
	{
		X = v.X;
		Y = v.Y;
		Z = v.Z;
		W = w;
	}

	public QuatF(Vec4F v)
	{
		X = v.X;
		Y = v.Y;
		Z = v.Z;
		W = v.W;
	}

	public static QuatF Mul(QuatF q1, QuatF q2)
	{
		float x = q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y + q1.W * q2.X;
		float y = (0f - q1.X) * q2.Z + q1.Y * q2.W + q1.Z * q2.X + q1.W * q2.Y;
		float z = q1.X * q2.Y - q1.Y * q2.X + q1.Z * q2.W + q1.W * q2.Z;
		float w = (0f - q1.X) * q2.X - q1.Y * q2.Y - q1.Z * q2.Z + q1.W * q2.W;
		return new QuatF(x, y, z, w);
	}

	public static QuatF Normalize(QuatF q)
	{
		double num = q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z;
		float x;
		float y;
		float z;
		float w;
		if (num > 1E-06)
		{
			num = 1.0 / Math.Sqrt(num);
			x = (float)(num * (double)q.X);
			y = (float)(num * (double)q.Y);
			z = (float)(num * (double)q.Z);
			w = (float)(num * (double)q.W);
		}
		else
		{
			x = (y = (z = (w = 0f)));
		}
		return new QuatF(x, y, z, w);
	}

	public void Set(Matrix4F m)
	{
		double num = 0.25 * (double)(m.M11 + m.M22 + m.M33 + 1f);
		if (num >= 0.0)
		{
			if (num >= 1E-30)
			{
				double num2 = Math.Sqrt(num);
				W = (float)num2;
				num = 0.25 / num2;
				X = (float)((double)(m.M32 - m.M23) * num);
				Y = (float)((double)(m.M13 - m.M31) * num);
				Z = (float)((double)(m.M21 - m.M12) * num);
				return;
			}
			W = 0f;
			num = -0.5 * (double)(m.M22 + m.M33);
			if (num >= 0.0)
			{
				if (num >= 1E-30)
				{
					double num3 = Math.Sqrt(num);
					X = (float)num3;
					num = 0.5 / num3;
					Y = (float)((double)m.M21 * num);
					Z = (float)((double)m.M31 * num);
					return;
				}
				X = 0f;
				num = 0.5 * (double)(1f - m.M33);
				if (num >= 1E-30)
				{
					double num4 = Math.Sqrt(num);
					Y = (float)num4;
					Z = (float)((double)m.M32 / (2.0 * num4));
				}
				Y = 0f;
				Z = 1f;
			}
			else
			{
				X = 0f;
				Y = 0f;
				Z = 1f;
			}
		}
		else
		{
			W = 0f;
			X = 0f;
			Y = 0f;
			Z = 1f;
		}
	}

	public static QuatF FromAxisAngle(Vec3F axis, float angle)
	{
		QuatF result = default(QuatF);
		double num = Math.Sqrt(axis.X * axis.X + axis.Y * axis.Y + axis.Z * axis.Z);
		if (num < 1E-06)
		{
			result = new QuatF(0f, 0f, 0f, 0f);
		}
		else
		{
			double num2 = Math.Sin((double)angle / 2.0);
			double num3 = num2 / num;
			result.W = (float)Math.Cos((double)angle / 2.0);
			result.X = (float)((double)axis.X * num3);
			result.Y = (float)((double)axis.Y * num3);
			result.Z = (float)((double)axis.Z * num3);
		}
		return result;
	}

	public void Set(AngleAxisF a)
	{
		double num = Math.Sqrt(a.Axis.X * a.Axis.X + a.Axis.Y * a.Axis.Y + a.Axis.Z * a.Axis.Z);
		if (num < 1E-06)
		{
			X = (Y = (Z = (W = 0f)));
			return;
		}
		double num2 = Math.Sin((double)a.Angle / 2.0);
		double num3 = num2 / num;
		W = (float)Math.Cos((double)a.Angle / 2.0);
		X = (float)((double)a.Axis.X * num3);
		Y = (float)((double)a.Axis.Y * num3);
		Z = (float)((double)a.Axis.Z * num3);
	}

	public static QuatF Slerp(QuatF q1, QuatF q2, float t)
	{
		double num = q2.X * q1.X + q2.Y * q1.Y + q2.Z * q1.Z + q2.W * q1.W;
		if (num < 0.0)
		{
			q1.X = 0f - q1.X;
		}
		q1.Y = 0f - q1.Y;
		q1.Z = 0f - q1.Z;
		q1.W = 0f - q1.W;
		double num4;
		double num5;
		if (1.0 - Math.Abs(num) > 1E-06)
		{
			double num2 = Math.Acos(num);
			double num3 = Math.Sin(num2);
			num4 = Math.Sin((1.0 - (double)t) * num2) / num3;
			num5 = Math.Sin((double)t * num2) / num3;
		}
		else
		{
			num4 = 1f - t;
			num5 = t;
		}
		return new QuatF((float)(num4 * (double)q1.X + num5 * (double)q2.X), (float)(num4 * (double)q1.Y + num5 * (double)q2.Y), (float)(num4 * (double)q1.Z + num5 * (double)q2.Z), (float)(num4 * (double)q1.W + num5 * (double)q2.W));
	}

	public bool Equals(QuatF q)
	{
		return X == q.X && Y == q.Y && Z == q.Z && W == q.W;
	}

	public bool Equals(QuatF q, double eps)
	{
		return (double)Math.Abs(X - q.X) < eps && (double)Math.Abs(Y - q.Y) < eps && (double)Math.Abs(Z - q.Z) < eps && (double)Math.Abs(W - q.W) < eps;
	}

	public float[] ToArray()
	{
		return new float[4] { X, Y, Z, W };
	}

	public static QuatF operator *(QuatF q1, QuatF q2)
	{
		return Mul(q1, q2);
	}

	public override bool Equals(object obj)
	{
		if (obj is QuatF quatF)
		{
			return object.Equals(this, quatF);
		}
		return false;
	}

	public override int GetHashCode()
	{
		long num = 1L;
		num = 31 * num + W.GetHashCode();
		num = 31 * num + X.GetHashCode();
		num = 31 * num + Y.GetHashCode();
		num = 31 * num + Z.GetHashCode();
		return (int)(num ^ (num >> 32));
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		string numberListSeparator = StringUtil.GetNumberListSeparator(formatProvider);
		return string.Format("{1}{0} {2}{0} {3}{0} {4}", numberListSeparator, ((double)X).ToString(format, formatProvider), ((double)Y).ToString(format, formatProvider), ((double)Z).ToString(format, formatProvider), ((double)W).ToString(format, formatProvider));
	}

	static QuatF()
	{
		Identity = new QuatF(0f, 0f, 0f, 1f);
	}
}

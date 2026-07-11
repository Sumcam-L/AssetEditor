using System;
using System.Drawing;

namespace Sce.Atf.VectorMath;

public struct Vec2F : IEquatable<Vec2F>, IFormattable
{
	public float X;

	public float Y;

	public static readonly Vec2F XAxis;

	public static readonly Vec2F YAxis;

	public float Length => (float)Math.Sqrt(X * X + Y * Y);

	public float LengthSquared => X * X + Y * Y;

	public Vec2F Perp => new Vec2F(0f - Y, X);

	public float this[int i]
	{
		get
		{
			return i switch
			{
				0 => X, 
				1 => Y, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		set
		{
			switch (i)
			{
			case 0:
				X = value;
				break;
			case 1:
				Y = value;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public Vec2F(float x, float y)
	{
		X = x;
		Y = y;
	}

	public Vec2F(float[] coords)
	{
		if (coords.Length < 2)
		{
			throw new ArgumentException("not enough coords");
		}
		X = coords[0];
		Y = coords[1];
	}

	public Vec2F(PointF p)
	{
		X = p.X;
		Y = p.Y;
	}

	public static implicit operator PointF(Vec2F v)
	{
		return new PointF(v.X, v.Y);
	}

	public void Set(float x, float y)
	{
		X = x;
		Y = y;
	}

	public void Set(float[] coords)
	{
		if (coords.Length < 2)
		{
			throw new ArgumentException("not enough coords");
		}
		X = coords[0];
		Y = coords[1];
	}

	public void Set(Vec2F v)
	{
		X = v.X;
		Y = v.Y;
	}

	public static Vec2F Neg(Vec2F v)
	{
		return new Vec2F(0f - v.X, 0f - v.Y);
	}

	public void Neg()
	{
		Set(0f - X, 0f - Y);
	}

	public static Vec2F Add(Vec2F v1, Vec2F v2)
	{
		return new Vec2F(v1.X + v2.X, v1.Y + v2.Y);
	}

	public static Vec2F Sub(Vec2F v1, Vec2F v2)
	{
		return new Vec2F(v1.X - v2.X, v1.Y - v2.Y);
	}

	public static Vec2F Mul(Vec2F v1, Vec2F v2)
	{
		return new Vec2F(v1.X * v2.X, v1.Y * v2.Y);
	}

	public static Vec2F Mul(Vec2F v, float s)
	{
		return new Vec2F(v.X * s, v.Y * s);
	}

	public static Vec2F Lerp(Vec2F v1, Vec2F v2, float t)
	{
		return new Vec2F(v1.X + (v2.X - v1.X) * t, v1.Y + (v2.Y - v1.Y) * t);
	}

	public static Vec2F ClampMin(Vec2F v, float min)
	{
		return new Vec2F(Math.Max(v.X, min), Math.Max(v.Y, min));
	}

	public static Vec2F ClampMax(Vec2F v, float max)
	{
		return new Vec2F(Math.Min(v.X, max), Math.Min(v.Y, max));
	}

	public static Vec2F Clamp(Vec2F v, float min, float max)
	{
		return new Vec2F(Math.Min(Math.Max(v.X, min), max), Math.Min(Math.Max(v.Y, min), max));
	}

	public static Vec2F Abs(Vec2F v)
	{
		return new Vec2F(Math.Abs(v.X), Math.Abs(v.Y));
	}

	public static Vec2F Normalize(Vec2F v)
	{
		float length = v.Length;
		if (length < 1E-06f)
		{
			return v;
		}
		float num = 1f / length;
		return new Vec2F(v.X * num, v.Y * num);
	}

	public void Normalize()
	{
		Set(Normalize(this));
	}

	public static float PerpDot(Vec2F u, Vec2F v)
	{
		return (0f - u.Y) * v.X + u.X * v.Y;
	}

	public static float Dot(Vec2F u, Vec2F v)
	{
		return u.X * v.X + u.Y * v.Y;
	}

	public static float Distance(Vec2F v1, Vec2F v2)
	{
		return Sub(v1, v2).Length;
	}

	public static float Angle(Vec2F v1, Vec2F v2)
	{
		double val = (double)Dot(v1, v2) / Math.Sqrt(Dot(v1, v1) * Dot(v2, v2));
		val = Math.Max(val, -1.0);
		val = Math.Min(val, 1.0);
		return (float)Math.Acos(val);
	}

	public bool Equals(Vec2F v)
	{
		return X == v.X && Y == v.Y;
	}

	public bool Equals(Vec2F v, double eps)
	{
		return (double)Math.Abs(X - v.X) < eps && (double)Math.Abs(Y - v.Y) < eps;
	}

	public float[] ToArray()
	{
		return new float[2] { X, Y };
	}

	public static Vec2F operator -(Vec2F v)
	{
		return Neg(v);
	}

	public static Vec2F operator +(Vec2F v1, Vec2F v2)
	{
		return Add(v1, v2);
	}

	public static Vec2F operator -(Vec2F v1, Vec2F v2)
	{
		return Sub(v1, v2);
	}

	public static Vec2F operator *(Vec2F v, float s)
	{
		return Mul(v, s);
	}

	public static Vec2F operator *(float s, Vec2F v)
	{
		return Mul(v, s);
	}

	public static Vec2F operator /(Vec2F v, float s)
	{
		return Mul(v, 1f / s);
	}

	public static bool operator ==(Vec2F v1, Vec2F v2)
	{
		return v1.Equals(v2);
	}

	public static bool operator !=(Vec2F v1, Vec2F v2)
	{
		return !v1.Equals(v2);
	}

	public override bool Equals(object obj)
	{
		if (obj is Vec2F v)
		{
			return Equals(v);
		}
		return false;
	}

	public override int GetHashCode()
	{
		long num = 1L;
		num = 31 * num + X.GetHashCode();
		num = 31 * num + Y.GetHashCode();
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
		return string.Format("{0}{2} {1}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), numberListSeparator);
	}

	static Vec2F()
	{
		XAxis = new Vec2F(1f, 0f);
		YAxis = new Vec2F(0f, 1f);
	}
}

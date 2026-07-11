using System;

namespace Sce.Atf.VectorMath;

public struct Vec4F : IEquatable<Vec4F>, IFormattable
{
	public float X;

	public float Y;

	public float Z;

	public float W;

	public static readonly Vec4F XAxis;

	public static readonly Vec4F YAxis;

	public static readonly Vec4F ZAxis;

	public static readonly Vec4F WAxis;

	public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

	public float LengthSquared => X * X + Y * Y + Z * Z + W * W;

	public float this[int i]
	{
		get
		{
			return i switch
			{
				0 => X, 
				1 => Y, 
				2 => Z, 
				3 => W, 
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
			case 2:
				Z = value;
				break;
			case 3:
				W = value;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public Vec4F(float x, float y, float z, float w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public Vec4F(float[] coords)
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

	public Vec4F(Vec3F v)
	{
		X = v.X;
		Y = v.Y;
		Z = v.Z;
		W = 1f;
	}

	public Vec4F(Vec4F v)
	{
		X = v.X;
		Y = v.Y;
		Z = v.Z;
		W = v.W;
	}

	public void Set(float x, float y, float z, float w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public void Set(float[] coords)
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

	public void Set(Vec4F v)
	{
		X = v.X;
		Y = v.Y;
		Z = v.Z;
		W = v.W;
	}

	public static Vec4F Neg(Vec4F v)
	{
		return new Vec4F(0f - v.X, 0f - v.Y, 0f - v.Z, 0f - v.W);
	}

	public void Neg()
	{
		Set(0f - X, 0f - Y, 0f - Z, 0f - W);
	}

	public float[] ToArray()
	{
		return new float[4] { X, Y, Z, W };
	}

	public static Vec4F Add(Vec4F v1, Vec4F v2)
	{
		return new Vec4F(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z, v1.W + v2.W);
	}

	public static Vec4F Sub(Vec4F v1, Vec4F v2)
	{
		return new Vec4F(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z, v1.W - v2.W);
	}

	public static Vec4F Mul(Vec4F v1, Vec4F v2)
	{
		return new Vec4F(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z, v1.W * v2.W);
	}

	public static Vec4F Mul(Vec4F v, float s)
	{
		return new Vec4F(v.X * s, v.Y * s, v.Z * s, v.W * s);
	}

	public static Vec4F Lerp(Vec4F v1, Vec4F v2, float t)
	{
		return new Vec4F(v1.X + (v2.X - v1.X) * t, v1.Y + (v2.Y - v1.Y) * t, v1.Z + (v2.Z - v1.Z) * t, v1.W + (v2.W - v1.W) * t);
	}

	public static Vec4F ClampMin(Vec4F v, float min)
	{
		return new Vec4F(Math.Max(v.X, min), Math.Max(v.Y, min), Math.Max(v.Z, min), Math.Max(v.W, min));
	}

	public static Vec4F ClampMax(Vec4F v, float max)
	{
		return new Vec4F(Math.Min(v.X, max), Math.Min(v.Y, max), Math.Min(v.Z, max), Math.Min(v.W, max));
	}

	public static Vec4F Clamp(Vec4F v, float min, float max)
	{
		return new Vec4F(Math.Min(Math.Max(v.X, min), max), Math.Min(Math.Max(v.Y, min), max), Math.Min(Math.Max(v.Z, min), max), Math.Min(Math.Max(v.W, min), max));
	}

	public static Vec4F Abs(Vec4F v)
	{
		return new Vec4F(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z), Math.Abs(v.W));
	}

	public static Vec4F Normalize(Vec4F v)
	{
		float length = v.Length;
		if (length < 1E-06f)
		{
			return v;
		}
		float num = 1f / length;
		return new Vec4F(v.X * num, v.Y * num, v.Z * num, v.W * num);
	}

	public void Normalize()
	{
		Set(Normalize(this));
	}

	public static float Dot(Vec4F u, Vec4F v)
	{
		return u.X * v.X + u.Y * v.Y + u.Z * v.Z + u.W * v.W;
	}

	public static float Distance(Vec4F v1, Vec4F v2)
	{
		return Sub(v1, v2).Length;
	}

	public static float Angle(Vec4F v1, Vec4F v2)
	{
		double val = (double)Dot(v1, v2) / Math.Sqrt(Dot(v1, v1) * Dot(v2, v2));
		val = Math.Max(val, -1.0);
		val = Math.Min(val, 1.0);
		return (float)Math.Acos(val);
	}

	public bool Equals(Vec4F v)
	{
		return X == v.X && Y == v.Y && Z == v.Z && W == v.W;
	}

	public bool Equals(Vec4F v, double eps)
	{
		return (double)Math.Abs(X - v.X) < eps && (double)Math.Abs(Y - v.Y) < eps && (double)Math.Abs(Z - v.Z) < eps && (double)Math.Abs(W - v.W) < eps;
	}

	public static Vec4F operator -(Vec4F v)
	{
		return Neg(v);
	}

	public static Vec4F operator +(Vec4F v1, Vec4F v2)
	{
		return Add(v1, v2);
	}

	public static Vec4F operator -(Vec4F v1, Vec4F v2)
	{
		return Sub(v1, v2);
	}

	public static Vec4F operator *(Vec4F v, float s)
	{
		return Mul(v, s);
	}

	public static Vec4F operator *(float s, Vec4F v)
	{
		return Mul(v, s);
	}

	public static Vec4F operator /(Vec4F v, float s)
	{
		return Mul(v, 1f / s);
	}

	public static bool operator ==(Vec4F v1, Vec4F v2)
	{
		return v1.Equals(v2);
	}

	public static bool operator !=(Vec4F v1, Vec4F v2)
	{
		return !v1.Equals(v2);
	}

	public override bool Equals(object obj)
	{
		if (obj is Vec4F v)
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
		num = 31 * num + Z.GetHashCode();
		num = 31 * num + W.GetHashCode();
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
		return string.Format("{1}{0} {2}{0} {3}{0} {4}", numberListSeparator, X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
	}

	static Vec4F()
	{
		XAxis = new Vec4F(1f, 0f, 0f, 0f);
		YAxis = new Vec4F(0f, 1f, 0f, 0f);
		ZAxis = new Vec4F(0f, 0f, 1f, 0f);
		WAxis = new Vec4F(0f, 0f, 0f, 1f);
	}
}

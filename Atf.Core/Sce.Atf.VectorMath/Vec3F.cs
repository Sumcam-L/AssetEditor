using System;
using System.Globalization;

namespace Sce.Atf.VectorMath;

public struct Vec3F : IEquatable<Vec3F>, IFormattable
{
	public float X;

	public float Y;

	public float Z;

	public static readonly Vec3F XAxis;

	public static readonly Vec3F YAxis;

	public static readonly Vec3F ZAxis;

	public static readonly Vec3F ZeroVector;

	public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

	public float LengthSquared => X * X + Y * Y + Z * Z;

	public float this[int i]
	{
		get
		{
			return i switch
			{
				0 => X, 
				1 => Y, 
				2 => Z, 
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
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public Vec3F(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public Vec3F(float[] coords)
	{
		X = coords[0];
		Y = coords[1];
		Z = coords[2];
	}

	public Vec3F(Vec3F v)
	{
		X = v.X;
		Y = v.Y;
		Z = v.Z;
	}

	public Vec3F(Vec2F v)
	{
		X = v.X;
		Y = v.Y;
		Z = 1f;
	}

	public void Set(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public void Set(float[] coords)
	{
		X = coords[0];
		Y = coords[1];
		Z = coords[2];
	}

	public void Set(Vec3F v)
	{
		X = v.X;
		Y = v.Y;
		Z = v.Z;
	}

	public static Vec3F Neg(Vec3F v)
	{
		return new Vec3F(0f - v.X, 0f - v.Y, 0f - v.Z);
	}

	public void Neg()
	{
		Set(0f - X, 0f - Y, 0f - Z);
	}

	public static Vec3F Add(Vec3F v1, Vec3F v2)
	{
		return new Vec3F(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
	}

	public static Vec3F Sub(Vec3F v1, Vec3F v2)
	{
		return new Vec3F(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
	}

	public static Vec3F Mul(Vec3F v1, Vec3F v2)
	{
		return new Vec3F(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
	}

	public static Vec3F Mul(Vec3F v, float s)
	{
		return new Vec3F(v.X * s, v.Y * s, v.Z * s);
	}

	public static Vec3F Div(Vec3F v1, Vec3F v2)
	{
		return new Vec3F(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
	}

	public static Vec3F Lerp(Vec3F v1, Vec3F v2, float t)
	{
		return new Vec3F(v1.X + (v2.X - v1.X) * t, v1.Y + (v2.Y - v1.Y) * t, v1.Z + (v2.Z - v1.Z) * t);
	}

	public static Vec3F ClampMin(Vec3F v, float min)
	{
		return new Vec3F(Math.Max(v.X, min), Math.Max(v.Y, min), Math.Max(v.Z, min));
	}

	public static Vec3F ClampMax(Vec3F v, float max)
	{
		return new Vec3F(Math.Min(v.X, max), Math.Min(v.Y, max), Math.Min(v.Z, max));
	}

	public static Vec3F Clamp(Vec3F v, float min, float max)
	{
		return new Vec3F(Math.Min(Math.Max(v.X, min), max), Math.Min(Math.Max(v.Y, min), max), Math.Min(Math.Max(v.Z, min), max));
	}

	public static Vec3F Abs(Vec3F v)
	{
		return new Vec3F(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z));
	}

	public static Vec3F Normalize(Vec3F v)
	{
		float length = v.Length;
		if (length < 1E-06f)
		{
			return v;
		}
		float num = 1f / length;
		return new Vec3F(v.X * num, v.Y * num, v.Z * num);
	}

	public void Normalize()
	{
		Set(Normalize(this));
	}

	public static float Dot(Vec3F u, Vec3F v)
	{
		return u.X * v.X + u.Y * v.Y + u.Z * v.Z;
	}

	public static float Distance(Vec3F v1, Vec3F v2)
	{
		return Sub(v1, v2).Length;
	}

	public static float Angle(Vec3F v1, Vec3F v2)
	{
		double val = (double)Dot(v1, v2) / Math.Sqrt(Dot(v1, v1) * Dot(v2, v2));
		val = Math.Max(val, -1.0);
		val = Math.Min(val, 1.0);
		return (float)Math.Acos(val);
	}

	public static Vec3F Cross(Vec3F v1, Vec3F v2)
	{
		return new Vec3F(v1.Y * v2.Z - v1.Z * v2.Y, v2.X * v1.Z - v2.Z * v1.X, v1.X * v2.Y - v1.Y * v2.X);
	}

	public bool Equals(Vec3F v)
	{
		return X == v.X && Y == v.Y && Z == v.Z;
	}

	public bool Equals(Vec3F v, double eps)
	{
		return (double)Math.Abs(X - v.X) < eps && (double)Math.Abs(Y - v.Y) < eps && (double)Math.Abs(Z - v.Z) < eps;
	}

	public float[] ToArray()
	{
		return new float[3] { X, Y, Z };
	}

	public static Vec3F Parse(string s)
	{
		string listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
		string[] array = s.Split(new string[1] { listSeparator }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 3)
		{
			throw new FormatException();
		}
		Vec3F result = default(Vec3F);
		for (int i = 0; i < 3; i++)
		{
			result[i] = float.Parse(array[i]);
		}
		return result;
	}

	public static Vec3F operator -(Vec3F v)
	{
		return Neg(v);
	}

	public static Vec3F operator +(Vec3F v1, Vec3F v2)
	{
		return Add(v1, v2);
	}

	public static Vec3F operator -(Vec3F v1, Vec3F v2)
	{
		return Sub(v1, v2);
	}

	public static Vec3F operator *(Vec3F v, float s)
	{
		return Mul(v, s);
	}

	public static Vec3F operator *(float s, Vec3F v)
	{
		return Mul(v, s);
	}

	public static Vec3F operator *(Vec3F v1, Vec3F v2)
	{
		return Mul(v1, v2);
	}

	public static Vec3F operator /(Vec3F v, float s)
	{
		return Mul(v, 1f / s);
	}

	public static Vec3F operator /(Vec3F v1, Vec3F v2)
	{
		return Div(v1, v2);
	}

	public static bool operator ==(Vec3F v1, Vec3F v2)
	{
		return v1.Equals(v2);
	}

	public static bool operator !=(Vec3F v1, Vec3F v2)
	{
		return !v1.Equals(v2);
	}

	public override bool Equals(object obj)
	{
		if (obj is Vec3F v)
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
		return string.Format("{1}{0} {2}{0} {3}", numberListSeparator, ((double)X).ToString(format, formatProvider), ((double)Y).ToString(format, formatProvider), ((double)Z).ToString(format, formatProvider));
	}

	static Vec3F()
	{
		XAxis = new Vec3F(1f, 0f, 0f);
		YAxis = new Vec3F(0f, 1f, 0f);
		ZAxis = new Vec3F(0f, 0f, 1f);
		ZeroVector = new Vec3F(0f, 0f, 0f);
	}
}

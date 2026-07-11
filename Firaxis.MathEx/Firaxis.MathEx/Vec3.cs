using System;

namespace Firaxis.MathEx;

public struct Vec3
{
	public float X;

	public float Y;

	public float Z;

	private static Vec3 zero;

	private static Vec3 one;

	public static Vec3 Zero => zero;

	public static Vec3 One => one;

	public float LengthSquared => X * X + Y * Y + Z * Z;

	public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

	public float this[int i]
	{
		get
		{
			return i switch
			{
				0 => X, 
				1 => Y, 
				2 => Z, 
				_ => 0f, 
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
			}
		}
	}

	public Vec3(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public Vec3(float[] afComponents)
	{
		X = afComponents[0];
		Y = afComponents[1];
		Z = afComponents[2];
	}

	public Vec3(Vec2 v)
	{
		X = v.X;
		Y = v.Y;
		Z = 0f;
	}

	public override bool Equals(object obj)
	{
		if (obj is Vec3)
		{
			return Equals((Vec3)obj);
		}
		return false;
	}

	public override string ToString()
	{
		return $"{X}, {Y}, {Z}";
	}

	public bool Equals(Vec3 other)
	{
		return X == other.X && Y == other.Y && Z == other.Z;
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
	}

	public static Vec3 operator -(Vec3 value)
	{
		return new Vec3(0f - value.X, 0f - value.Y, 0f - value.Z);
	}

	public static Vec3 operator -(Vec3 value1, Vec3 value2)
	{
		return new Vec3(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z);
	}

	public static bool operator !=(Vec3 value1, Vec3 value2)
	{
		return value1.X != value2.X || value1.Y != value2.Y || value1.Z != value2.Z;
	}

	public static Vec3 operator *(float scaleFactor, Vec3 value)
	{
		return new Vec3(value.X * scaleFactor, value.Y * scaleFactor, value.Z * scaleFactor);
	}

	public static Vec3 operator *(Vec3 value, float scaleFactor)
	{
		return new Vec3(value.X * scaleFactor, value.Y * scaleFactor, value.Z * scaleFactor);
	}

	public static Vec3 operator *(Vec3 value1, Vec3 value2)
	{
		return new Vec3(value1.X * value2.X, value1.Y * value2.Y, value1.Z * value2.Z);
	}

	public static Vec3 operator /(Vec3 value, float divider)
	{
		float num = 1f / divider;
		Vec3 result = default(Vec3);
		result.X = value.X * num;
		result.Y = value.Y * num;
		result.Z = value.Z * num;
		return result;
	}

	public static Vec3 operator /(Vec3 value1, Vec3 value2)
	{
		return new Vec3(value1.X / value2.X, value1.Y / value2.Y, value1.Z / value2.Z);
	}

	public static Vec3 operator +(Vec3 value1, Vec3 value2)
	{
		return new Vec3(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z);
	}

	public static bool operator ==(Vec3 value1, Vec3 value2)
	{
		return value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z;
	}

	public static Vec3 Lerp(Vec3 v0, Vec3 v1, float t)
	{
		return v0 + (v1 - v0) * t;
	}

	public float Dot(Vec3 v)
	{
		return X * v.X + Y * v.Y + Z * v.Z;
	}

	public Vec3 Cross(Vec3 v)
	{
		return new Vec3(Y * v.Z - Z * v.Y, 0f - (X * v.Z - Z * v.X), X * v.Y - Y * v.X);
	}

	static Vec3()
	{
		zero = default(Vec3);
		one = new Vec3(1f, 1f, 1f);
	}
}

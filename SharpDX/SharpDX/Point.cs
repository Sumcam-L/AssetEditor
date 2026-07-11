using System;
using SharpDX.Serialization;

namespace SharpDX;

public struct Point : IEquatable<Point>, IDataSerializable
{
	public static readonly Point Zero = new Point(0, 0);

	public int X;

	public int Y;

	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}

	public bool Equals(Point other)
	{
		if (other.X == X)
		{
			return other.Y == Y;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (obj.GetType() != typeof(Point))
		{
			return false;
		}
		return Equals((Point)obj);
	}

	public override int GetHashCode()
	{
		return (X * 397) ^ Y;
	}

	public static bool operator ==(Point left, Point right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Point left, Point right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return $"({X},{Y})";
	}

	public static explicit operator Point(Vector2 value)
	{
		return new Point((int)value.X, (int)value.Y);
	}

	public static implicit operator Vector2(Point value)
	{
		return new Vector2(value.X, value.Y);
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(X);
			serializer.Writer.Write(Y);
		}
		else
		{
			X = serializer.Reader.ReadInt32();
			Y = serializer.Reader.ReadInt32();
		}
	}
}

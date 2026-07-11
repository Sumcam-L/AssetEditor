using System.Drawing;

namespace Firaxis.MathEx;

public struct Point2
{
	public int X;

	public int Y;

	private static Point2 empty;

	public static Point2 Empty => empty;

	public Point ToPoint => new Point(X, Y);

	public Vec2 ToVec2 => new Vec2(X, Y);

	public Point2(int x, int y)
	{
		X = x;
		Y = y;
	}

	public Point2(Point point)
	{
		X = point.X;
		Y = point.Y;
	}

	public override string ToString()
	{
		return $"{X}, {Y}";
	}

	public void Set(int x, int y)
	{
		X = x;
		Y = y;
	}

	public void Set(Point point)
	{
		X = point.X;
		Y = point.Y;
	}

	public override bool Equals(object obj)
	{
		if (obj is Point2)
		{
			return Equals((Point2)obj);
		}
		return false;
	}

	public bool Equals(Point2 other)
	{
		return X == other.X && Y == other.Y;
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() + Y.GetHashCode();
	}

	public static Point2 operator -(Point2 value)
	{
		return new Point2(-value.X, -value.Y);
	}

	public static Point2 operator -(Point2 value1, Point2 value2)
	{
		return new Point2(value1.X - value2.X, value1.Y - value2.Y);
	}

	public static bool operator !=(Point2 value1, Point2 value2)
	{
		return value1.X != value2.X || value1.Y != value2.Y;
	}

	public static Point2 operator *(int scaleFactor, Point2 value)
	{
		return new Point2(value.X * scaleFactor, value.Y * scaleFactor);
	}

	public static Point2 operator *(Point2 value, int scaleFactor)
	{
		return new Point2(value.X * scaleFactor, value.Y * scaleFactor);
	}

	public static Point2 operator *(Point2 value1, Point2 value2)
	{
		return new Point2(value1.X * value2.X, value1.Y * value2.Y);
	}

	public static Point2 operator /(Point2 value, int divider)
	{
		float num = 1f / (float)divider;
		Point2 result = default(Point2);
		result.X = (int)((float)value.X * num);
		result.Y = (int)((float)value.Y * num);
		return result;
	}

	public static Point2 operator /(Point2 value1, Point2 value2)
	{
		return new Point2(value1.X / value2.X, value1.Y / value2.Y);
	}

	public static Point2 operator +(Point2 value1, Point2 value2)
	{
		return new Point2(value1.X + value2.X, value1.Y + value2.Y);
	}

	public static bool operator ==(Point2 value1, Point2 value2)
	{
		return value1.X == value2.X && value1.Y == value2.Y;
	}

	static Point2()
	{
		empty = default(Point2);
	}
}

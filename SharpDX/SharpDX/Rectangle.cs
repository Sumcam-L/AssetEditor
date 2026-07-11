using System;
using System.Globalization;
using SharpDX.Serialization;

namespace SharpDX;

public struct Rectangle : IEquatable<Rectangle>, IDataSerializable
{
	private int _left;

	private int _top;

	private int _right;

	private int _bottom;

	public static readonly Rectangle Empty;

	public int Left
	{
		get
		{
			return _left;
		}
		set
		{
			_left = value;
		}
	}

	public int Top
	{
		get
		{
			return _top;
		}
		set
		{
			_top = value;
		}
	}

	public int Right
	{
		get
		{
			return _right;
		}
		set
		{
			_right = value;
		}
	}

	public int Bottom
	{
		get
		{
			return _bottom;
		}
		set
		{
			_bottom = value;
		}
	}

	public int X
	{
		get
		{
			return _left;
		}
		set
		{
			_right = value + Width;
			_left = value;
		}
	}

	public int Y
	{
		get
		{
			return _top;
		}
		set
		{
			_bottom = value + Height;
			_top = value;
		}
	}

	public int Width
	{
		get
		{
			return _right - _left;
		}
		set
		{
			_right = _left + value;
		}
	}

	public int Height
	{
		get
		{
			return _bottom - _top;
		}
		set
		{
			_bottom = _top + value;
		}
	}

	public Point Location
	{
		get
		{
			return new Point(X, Y);
		}
		set
		{
			X = value.X;
			Y = value.Y;
		}
	}

	public Point Center => new Point(X + Width / 2, Y + Height / 2);

	public bool IsEmpty
	{
		get
		{
			if (Width == 0 && Height == 0 && X == 0)
			{
				return Y == 0;
			}
			return false;
		}
	}

	public Size2 Size
	{
		get
		{
			return new Size2(Width, Height);
		}
		set
		{
			Width = value.Width;
			Height = value.Height;
		}
	}

	public Point TopLeft => new Point(_left, _top);

	public Point TopRight => new Point(_right, _top);

	public Point BottomLeft => new Point(_left, _bottom);

	public Point BottomRight => new Point(_right, _bottom);

	static Rectangle()
	{
		Empty = default(Rectangle);
	}

	public Rectangle(int x, int y, int width, int height)
	{
		_left = x;
		_top = y;
		_right = x + width;
		_bottom = y + height;
	}

	public void Offset(Point amount)
	{
		Offset(amount.X, amount.Y);
	}

	public void Offset(int offsetX, int offsetY)
	{
		X += offsetX;
		Y += offsetY;
	}

	public void Inflate(int horizontalAmount, int verticalAmount)
	{
		X -= horizontalAmount;
		Y -= verticalAmount;
		Width += horizontalAmount * 2;
		Height += verticalAmount * 2;
	}

	public bool Contains(int x, int y)
	{
		if (X <= x && x < Right && Y <= y)
		{
			return y < Bottom;
		}
		return false;
	}

	public bool Contains(Point value)
	{
		Contains(ref value, out var result);
		return result;
	}

	public void Contains(ref Point value, out bool result)
	{
		result = X <= value.X && value.X < Right && Y <= value.Y && value.Y < Bottom;
	}

	public bool Contains(Rectangle value)
	{
		Contains(ref value, out var result);
		return result;
	}

	public void Contains(ref Rectangle value, out bool result)
	{
		result = X <= value.X && value.Right <= Right && Y <= value.Y && value.Bottom <= Bottom;
	}

	public bool Contains(float x, float y)
	{
		if (x >= (float)_left && x <= (float)_right && y >= (float)_top)
		{
			return y <= (float)_bottom;
		}
		return false;
	}

	public bool Contains(Vector2 vector2D)
	{
		return Contains(vector2D.X, vector2D.Y);
	}

	public bool Intersects(Rectangle value)
	{
		Intersects(ref value, out var result);
		return result;
	}

	public void Intersects(ref Rectangle value, out bool result)
	{
		result = value.X < Right && X < value.Right && value.Y < Bottom && Y < value.Bottom;
	}

	public static Rectangle Intersect(Rectangle value1, Rectangle value2)
	{
		Intersect(ref value1, ref value2, out var result);
		return result;
	}

	public static void Intersect(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
	{
		int num = ((value1.X > value2.X) ? value1.X : value2.X);
		int num2 = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
		int num3 = ((value1.Right < value2.Right) ? value1.Right : value2.Right);
		int num4 = ((value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom);
		if (num3 > num && num4 > num2)
		{
			result = new Rectangle(num, num2, num3 - num, num4 - num2);
		}
		else
		{
			result = Empty;
		}
	}

	public static Rectangle Union(Rectangle value1, Rectangle value2)
	{
		Union(ref value1, ref value2, out var result);
		return result;
	}

	public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
	{
		int num = Math.Min(value1.Left, value2.Left);
		int num2 = Math.Max(value1.Right, value2.Right);
		int num3 = Math.Min(value1.Top, value2.Top);
		int num4 = Math.Max(value1.Bottom, value2.Bottom);
		result = new Rectangle(num, num3, num2 - num, num4 - num3);
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (obj.GetType() != typeof(Rectangle))
		{
			return false;
		}
		return Equals((Rectangle)obj);
	}

	public bool Equals(Rectangle other)
	{
		if (other._left == _left && other._top == _top && other._right == _right)
		{
			return other._bottom == _bottom;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int left = _left;
		left = (left * 397) ^ _top;
		left = (left * 397) ^ _right;
		return (left * 397) ^ _bottom;
	}

	public static bool operator ==(Rectangle left, Rectangle right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Rectangle left, Rectangle right)
	{
		return !(left == right);
	}

	public static implicit operator RectangleF(Rectangle value)
	{
		return new RectangleF(value.X, value.Y, value.Width, value.Height);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "X:{0} Y:{1} Width:{2} Height:{3}", X, Y, Width, Height);
	}

	internal void MakeXYAndWidthHeight()
	{
		_right -= _left;
		_bottom -= _top;
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(_left);
			serializer.Writer.Write(_top);
			serializer.Writer.Write(_right);
			serializer.Writer.Write(_bottom);
		}
		else
		{
			_left = serializer.Reader.ReadInt32();
			_top = serializer.Reader.ReadInt32();
			_right = serializer.Reader.ReadInt32();
			_bottom = serializer.Reader.ReadInt32();
		}
	}
}

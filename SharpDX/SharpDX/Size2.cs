using System;
using SharpDX.Serialization;

namespace SharpDX;

public struct Size2 : IEquatable<Size2>, IDataSerializable
{
	public static readonly Size2 Zero = new Size2(0, 0);

	public static readonly Size2 Empty = Zero;

	public int Width;

	public int Height;

	public Size2(int width, int height)
	{
		Width = width;
		Height = height;
	}

	public bool Equals(Size2 other)
	{
		if (other.Width == Width)
		{
			return other.Height == Height;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (obj.GetType() != typeof(Size2))
		{
			return false;
		}
		return Equals((Size2)obj);
	}

	public override int GetHashCode()
	{
		return (Width * 397) ^ Height;
	}

	public static bool operator ==(Size2 left, Size2 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Size2 left, Size2 right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return $"({Width},{Height})";
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(Width);
			serializer.Writer.Write(Height);
		}
		else
		{
			Width = serializer.Reader.ReadInt32();
			Height = serializer.Reader.ReadInt32();
		}
	}
}

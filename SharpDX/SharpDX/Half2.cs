using System;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[DynamicSerializer("TKH2")]
public struct Half2 : IEquatable<Half2>, IDataSerializable
{
	public Half X;

	public Half Y;

	public Half2(Half x, Half y)
	{
		X = x;
		Y = y;
	}

	public Half2(float x, float y)
	{
		X = new Half(x);
		Y = new Half(y);
	}

	public Half2(ushort x, ushort y)
	{
		X = new Half(x);
		Y = new Half(y);
	}

	public Half2(Half value)
	{
		X = value;
		Y = value;
	}

	public Half2(float value)
	{
		X = new Half(value);
		Y = new Half(value);
	}

	public static implicit operator Half2(Vector2 value)
	{
		return new Half2(value.X, value.Y);
	}

	public static implicit operator Vector2(Half2 value)
	{
		return new Vector2(value.X, value.Y);
	}

	public static bool operator ==(Half2 left, Half2 right)
	{
		return Equals(ref left, ref right);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public static bool operator !=(Half2 left, Half2 right)
	{
		return !Equals(ref left, ref right);
	}

	public override int GetHashCode()
	{
		return (X.GetHashCode() * 397) ^ Y.GetHashCode();
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(X.RawValue);
			serializer.Writer.Write(Y.RawValue);
		}
		else
		{
			X.RawValue = serializer.Reader.ReadUInt16();
			Y.RawValue = serializer.Reader.ReadUInt16();
		}
	}

	public static bool Equals(ref Half2 value1, ref Half2 value2)
	{
		if (value1.X == value2.X)
		{
			return value1.Y == value2.Y;
		}
		return false;
	}

	public bool Equals(Half2 other)
	{
		if (X == other.X)
		{
			return Y == other.Y;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(obj.GetType(), typeof(Half2)))
		{
			return false;
		}
		return Equals((Half2)obj);
	}
}

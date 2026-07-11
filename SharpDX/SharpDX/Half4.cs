using System;
using SharpDX.Serialization;

namespace SharpDX;

[DynamicSerializer("TKH4")]
public struct Half4 : IEquatable<Half4>, IDataSerializable
{
	public Half X;

	public Half Y;

	public Half Z;

	public Half W;

	public Half4(Half x, Half y, Half z, Half w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public Half4(float x, float y, float z, float w)
	{
		X = new Half(x);
		Y = new Half(y);
		Z = new Half(z);
		W = new Half(w);
	}

	public Half4(ushort x, ushort y, ushort z, ushort w)
	{
		X = new Half(x);
		Y = new Half(y);
		Z = new Half(z);
		W = new Half(w);
	}

	public Half4(Half value)
	{
		X = value;
		Y = value;
		Z = value;
		W = value;
	}

	public static implicit operator Half4(Vector4 value)
	{
		return new Half4(value.X, value.Y, value.Z, value.W);
	}

	public static implicit operator Vector4(Half4 value)
	{
		return new Vector4(value.X, value.Y, value.Z, value.W);
	}

	public static explicit operator Half4(Vector3 value)
	{
		return new Half4(value.X, value.Y, value.Z, 0f);
	}

	public static explicit operator Vector3(Half4 value)
	{
		return new Vector3(value.X, value.Y, value.Z);
	}

	public static explicit operator Half4(Vector2 value)
	{
		return new Half4(value.X, value.Y, 0f, 0f);
	}

	public static explicit operator Vector2(Half4 value)
	{
		return new Vector2(value.X, value.Y);
	}

	public static bool operator ==(Half4 left, Half4 right)
	{
		return Equals(ref left, ref right);
	}

	public static bool operator !=(Half4 left, Half4 right)
	{
		return !Equals(ref left, ref right);
	}

	public override int GetHashCode()
	{
		int hashCode = X.GetHashCode();
		hashCode = (hashCode * 397) ^ Y.GetHashCode();
		hashCode = (hashCode * 397) ^ Z.GetHashCode();
		return (hashCode * 397) ^ W.GetHashCode();
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(X.RawValue);
			serializer.Writer.Write(Y.RawValue);
			serializer.Writer.Write(Z.RawValue);
			serializer.Writer.Write(W.RawValue);
		}
		else
		{
			X.RawValue = serializer.Reader.ReadUInt16();
			Y.RawValue = serializer.Reader.ReadUInt16();
			Z.RawValue = serializer.Reader.ReadUInt16();
			W.RawValue = serializer.Reader.ReadUInt16();
		}
	}

	public static bool Equals(ref Half4 value1, ref Half4 value2)
	{
		if (value1.X == value2.X && value1.Y == value2.Y)
		{
			if (value1.Z == value2.Z)
			{
				return value1.W == value2.W;
			}
			return false;
		}
		return false;
	}

	public bool Equals(Half4 other)
	{
		if (X == other.X && Y == other.Y)
		{
			if (Z == other.Z)
			{
				return W == other.W;
			}
			return false;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(obj.GetType(), typeof(Half4)))
		{
			return false;
		}
		return Equals((Half4)obj);
	}
}

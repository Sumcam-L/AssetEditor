using System;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Size = 4)]
[DynamicSerializer("TKB1")]
public struct Bool : IEquatable<Bool>, IDataSerializable
{
	private int boolValue;

	public Bool(bool boolValue)
	{
		this.boolValue = (boolValue ? 1 : 0);
	}

	public bool Equals(Bool other)
	{
		return boolValue == other.boolValue;
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (obj is Bool)
		{
			return Equals((Bool)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return boolValue;
	}

	public static bool operator ==(Bool left, Bool right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Bool left, Bool right)
	{
		return !left.Equals(right);
	}

	public static implicit operator bool(Bool booleanValue)
	{
		return booleanValue.boolValue != 0;
	}

	public static implicit operator Bool(bool boolValue)
	{
		return new Bool(boolValue);
	}

	public override string ToString()
	{
		return $"{boolValue != 0}";
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(boolValue);
		}
		else
		{
			boolValue = serializer.Reader.ReadInt32();
		}
	}
}

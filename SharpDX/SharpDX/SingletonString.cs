using System;
using SharpDX.Serialization;

namespace SharpDX;

public struct SingletonString : IEquatable<SingletonString>, IDataSerializable
{
	private int hashCode;

	private string text;

	public SingletonString(string text)
	{
		this = default(SingletonString);
		this.text = string.Intern(text);
		hashCode = text?.GetHashCode() ?? 0;
	}

	public bool Equals(SingletonString other)
	{
		if (hashCode == other.hashCode)
		{
			return object.ReferenceEquals(text, other.text);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (obj is SingletonString)
		{
			return Equals((SingletonString)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return hashCode;
	}

	public void Serialize(BinarySerializer serializer)
	{
		serializer.Serialize(ref text, SerializeFlags.Nullable);
		if (serializer.Mode == SerializerMode.Read)
		{
			hashCode = ((text != null) ? text.GetHashCode() : 0);
		}
	}

	public static bool operator ==(SingletonString left, SingletonString right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SingletonString left, SingletonString right)
	{
		return !left.Equals(right);
	}

	public static bool operator ==(SingletonString left, string right)
	{
		return string.Equals(left.text, right);
	}

	public static bool operator !=(SingletonString left, string right)
	{
		return !string.Equals(left.text, right);
	}

	public static implicit operator string(SingletonString value)
	{
		return value.text;
	}

	public static explicit operator SingletonString(string value)
	{
		return new SingletonString(value);
	}

	public override string ToString()
	{
		return text;
	}
}

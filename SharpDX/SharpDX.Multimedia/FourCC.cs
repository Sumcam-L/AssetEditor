using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX.Multimedia;

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct FourCC : IEquatable<FourCC>, IDataSerializable
{
	public static readonly FourCC Empty = new FourCC(0);

	private uint value;

	public FourCC(string fourCC)
	{
		if (fourCC.Length != 4)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid length for FourCC(\"{0}\". Must be be 4 characters long ", new object[1] { fourCC }), "fourCC");
		}
		value = ((uint)fourCC[3] << 24) | ((uint)fourCC[2] << 16) | ((uint)fourCC[1] << 8) | fourCC[0];
	}

	public FourCC(char byte1, char byte2, char byte3, char byte4)
	{
		value = ((uint)byte4 << 24) | ((uint)byte3 << 16) | ((uint)byte2 << 8) | byte1;
	}

	public FourCC(uint fourCC)
	{
		value = fourCC;
	}

	public FourCC(int fourCC)
	{
		value = (uint)fourCC;
	}

	public static implicit operator uint(FourCC d)
	{
		return d.value;
	}

	public static implicit operator int(FourCC d)
	{
		return (int)d.value;
	}

	public static implicit operator FourCC(uint d)
	{
		return new FourCC(d);
	}

	public static implicit operator FourCC(int d)
	{
		return new FourCC(d);
	}

	public static implicit operator string(FourCC d)
	{
		return d.ToString();
	}

	public static implicit operator FourCC(string d)
	{
		return new FourCC(d);
	}

	public override string ToString()
	{
		return $"{new string(new char[4]
		{
			(char)(value & 0xFF),
			(char)((value >> 8) & 0xFF),
			(char)((value >> 16) & 0xFF),
			(char)((value >> 24) & 0xFF)
		})}";
	}

	public bool Equals(FourCC other)
	{
		return value == other.value;
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (obj is FourCC)
		{
			return Equals((FourCC)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)value;
	}

	public void Serialize(BinarySerializer serializer)
	{
		serializer.Serialize(ref value);
	}

	public static bool operator ==(FourCC left, FourCC right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(FourCC left, FourCC right)
	{
		return !left.Equals(right);
	}
}

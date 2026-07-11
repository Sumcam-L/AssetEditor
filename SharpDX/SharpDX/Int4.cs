using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DynamicSerializer("TKI4")]
public struct Int4 : IEquatable<Int4>, IFormattable, IDataSerializable
{
	public static readonly int SizeInBytes;

	public static readonly Int4 Zero;

	public static readonly Int4 UnitX;

	public static readonly Int4 UnitY;

	public static readonly Int4 UnitZ;

	public static readonly Int4 UnitW;

	public static readonly Int4 One;

	public int X;

	public int Y;

	public int Z;

	public int W;

	public int this[int index]
	{
		get
		{
			return index switch
			{
				0 => X, 
				1 => Y, 
				2 => Z, 
				3 => W, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Int4 run from 0 to 3, inclusive."), 
			};
		}
		set
		{
			switch (index)
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
			case 3:
				W = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for Int4 run from 0 to 3, inclusive.");
			}
		}
	}

	public Int4(int value)
	{
		X = value;
		Y = value;
		Z = value;
		W = value;
	}

	public Int4(int x, int y, int z, int w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public Int4(int[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 4)
		{
			throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for Int4.");
		}
		X = values[0];
		Y = values[1];
		Z = values[2];
		W = values[3];
	}

	public int[] ToArray()
	{
		return new int[4] { X, Y, Z, W };
	}

	public static void Add(ref Int4 left, ref Int4 right, out Int4 result)
	{
		result = new Int4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
	}

	public static Int4 Add(Int4 left, Int4 right)
	{
		return new Int4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
	}

	public static void Subtract(ref Int4 left, ref Int4 right, out Int4 result)
	{
		result = new Int4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
	}

	public static Int4 Subtract(Int4 left, Int4 right)
	{
		return new Int4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
	}

	public static void Multiply(ref Int4 value, int scale, out Int4 result)
	{
		result = new Int4(value.X * scale, value.Y * scale, value.Z * scale, value.W * scale);
	}

	public static Int4 Multiply(Int4 value, int scale)
	{
		return new Int4(value.X * scale, value.Y * scale, value.Z * scale, value.W * scale);
	}

	public static void Modulate(ref Int4 left, ref Int4 right, out Int4 result)
	{
		result = new Int4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
	}

	public static Int4 Modulate(Int4 left, Int4 right)
	{
		return new Int4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
	}

	public static void Divide(ref Int4 value, int scale, out Int4 result)
	{
		result = new Int4(value.X / scale, value.Y / scale, value.Z / scale, value.W / scale);
	}

	public static Int4 Divide(Int4 value, int scale)
	{
		return new Int4(value.X / scale, value.Y / scale, value.Z / scale, value.W / scale);
	}

	public static void Negate(ref Int4 value, out Int4 result)
	{
		result = new Int4(-value.X, -value.Y, -value.Z, -value.W);
	}

	public static Int4 Negate(Int4 value)
	{
		return new Int4(-value.X, -value.Y, -value.Z, -value.W);
	}

	public static void Clamp(ref Int4 value, ref Int4 min, ref Int4 max, out Int4 result)
	{
		int x = value.X;
		x = ((x > max.X) ? max.X : x);
		x = ((x < min.X) ? min.X : x);
		int y = value.Y;
		y = ((y > max.Y) ? max.Y : y);
		y = ((y < min.Y) ? min.Y : y);
		int z = value.Z;
		z = ((z > max.Z) ? max.Z : z);
		z = ((z < min.Z) ? min.Z : z);
		int w = value.W;
		w = ((w > max.W) ? max.W : w);
		w = ((w < min.W) ? min.W : w);
		result = new Int4(x, y, z, w);
	}

	public static Int4 Clamp(Int4 value, Int4 min, Int4 max)
	{
		Clamp(ref value, ref min, ref max, out var result);
		return result;
	}

	public static void Max(ref Int4 left, ref Int4 right, out Int4 result)
	{
		result.X = ((left.X > right.X) ? left.X : right.X);
		result.Y = ((left.Y > right.Y) ? left.Y : right.Y);
		result.Z = ((left.Z > right.Z) ? left.Z : right.Z);
		result.W = ((left.W > right.W) ? left.W : right.W);
	}

	public static Int4 Max(Int4 left, Int4 right)
	{
		Max(ref left, ref right, out var result);
		return result;
	}

	public static void Min(ref Int4 left, ref Int4 right, out Int4 result)
	{
		result.X = ((left.X < right.X) ? left.X : right.X);
		result.Y = ((left.Y < right.Y) ? left.Y : right.Y);
		result.Z = ((left.Z < right.Z) ? left.Z : right.Z);
		result.W = ((left.W < right.W) ? left.W : right.W);
	}

	public static Int4 Min(Int4 left, Int4 right)
	{
		Min(ref left, ref right, out var result);
		return result;
	}

	public static Int4 operator +(Int4 left, Int4 right)
	{
		return new Int4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
	}

	public static Int4 operator +(Int4 value)
	{
		return value;
	}

	public static Int4 operator -(Int4 left, Int4 right)
	{
		return new Int4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
	}

	public static Int4 operator -(Int4 value)
	{
		return new Int4(-value.X, -value.Y, -value.Z, -value.W);
	}

	public static Int4 operator *(int scale, Int4 value)
	{
		return new Int4(value.X * scale, value.Y * scale, value.Z * scale, value.W * scale);
	}

	public static Int4 operator *(Int4 value, int scale)
	{
		return new Int4(value.X * scale, value.Y * scale, value.Z * scale, value.W * scale);
	}

	public static Int4 operator /(Int4 value, int scale)
	{
		return new Int4(value.X / scale, value.Y / scale, value.Z / scale, value.W / scale);
	}

	public static bool operator ==(Int4 left, Int4 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Int4 left, Int4 right)
	{
		return !left.Equals(right);
	}

	public static explicit operator Vector2(Int4 value)
	{
		return new Vector2(value.X, value.Y);
	}

	public static explicit operator Vector3(Int4 value)
	{
		return new Vector3(value.X, value.Y, value.Z);
	}

	public static explicit operator Vector4(Int4 value)
	{
		return new Vector4(value.X, value.Y, value.Z, value.W);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
	}

	public string ToString(string format)
	{
		if (format == null)
		{
			return ToString();
		}
		return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture), W.ToString(format, CultureInfo.CurrentCulture));
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			ToString(formatProvider);
		}
		return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
	}

	public override int GetHashCode()
	{
		int x = X;
		x = (x * 397) ^ Y;
		x = (x * 397) ^ Z;
		return (x * 397) ^ W;
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(X);
			serializer.Writer.Write(Y);
			serializer.Writer.Write(Z);
			serializer.Writer.Write(W);
		}
		else
		{
			X = serializer.Reader.ReadInt32();
			Y = serializer.Reader.ReadInt32();
			Z = serializer.Reader.ReadInt32();
			W = serializer.Reader.ReadInt32();
		}
	}

	public bool Equals(Int4 other)
	{
		if (other.X == X && other.Y == Y && other.Z == Z)
		{
			return other.W == W;
		}
		return false;
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(value.GetType(), typeof(Int4)))
		{
			return false;
		}
		return Equals((Int4)value);
	}

	public static implicit operator Int4(int[] input)
	{
		return new Int4(input);
	}

	public static implicit operator int[](Int4 input)
	{
		return input.ToArray();
	}

	static Int4()
	{
		SizeInBytes = Marshal.SizeOf(typeof(Int4));
		Zero = default(Int4);
		UnitX = new Int4(1, 0, 0, 0);
		UnitY = new Int4(0, 1, 0, 0);
		UnitZ = new Int4(0, 0, 1, 0);
		UnitW = new Int4(0, 0, 0, 1);
		One = new Int4(1, 1, 1, 1);
	}
}

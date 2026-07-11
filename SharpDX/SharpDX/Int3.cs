using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DynamicSerializer("TKI3")]
public struct Int3 : IEquatable<Int3>, IFormattable, IDataSerializable
{
	public static readonly int SizeInBytes;

	public static readonly Int3 Zero;

	public static readonly Int3 UnitX;

	public static readonly Int3 UnitY;

	public static readonly Int3 UnitZ;

	public static readonly Int3 One;

	public int X;

	public int Y;

	public int Z;

	public int this[int index]
	{
		get
		{
			return index switch
			{
				0 => X, 
				1 => Y, 
				2 => Z, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Int3 run from 0 to 2, inclusive."), 
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
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for Int3 run from 0 to 2, inclusive.");
			}
		}
	}

	public Int3(int value)
	{
		X = value;
		Y = value;
		Z = value;
	}

	public Int3(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public Int3(int[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 3)
		{
			throw new ArgumentOutOfRangeException("values", "There must be three and only three input values for Int3.");
		}
		X = values[0];
		Y = values[1];
		Z = values[2];
	}

	public int[] ToArray()
	{
		return new int[3] { X, Y, Z };
	}

	public static void Add(ref Int3 left, ref Int3 right, out Int3 result)
	{
		result = new Int3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
	}

	public static Int3 Add(Int3 left, Int3 right)
	{
		return new Int3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
	}

	public static void Subtract(ref Int3 left, ref Int3 right, out Int3 result)
	{
		result = new Int3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
	}

	public static Int3 Subtract(Int3 left, Int3 right)
	{
		return new Int3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
	}

	public static void Multiply(ref Int3 value, int scale, out Int3 result)
	{
		result = new Int3(value.X * scale, value.Y * scale, value.Z * scale);
	}

	public static Int3 Multiply(Int3 value, int scale)
	{
		return new Int3(value.X * scale, value.Y * scale, value.Z * scale);
	}

	public static void Modulate(ref Int3 left, ref Int3 right, out Int3 result)
	{
		result = new Int3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
	}

	public static Int3 Modulate(Int3 left, Int3 right)
	{
		return new Int3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
	}

	public static void Divide(ref Int3 value, int scale, out Int3 result)
	{
		result = new Int3(value.X / scale, value.Y / scale, value.Z / scale);
	}

	public static Int3 Divide(Int3 value, int scale)
	{
		return new Int3(value.X / scale, value.Y / scale, value.Z / scale);
	}

	public static void Negate(ref Int3 value, out Int3 result)
	{
		result = new Int3(-value.X, -value.Y, -value.Z);
	}

	public static Int3 Negate(Int3 value)
	{
		return new Int3(-value.X, -value.Y, -value.Z);
	}

	public static void Clamp(ref Int3 value, ref Int3 min, ref Int3 max, out Int3 result)
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
		result = new Int3(x, y, z);
	}

	public static Int3 Clamp(Int3 value, Int3 min, Int3 max)
	{
		Clamp(ref value, ref min, ref max, out var result);
		return result;
	}

	public static void Max(ref Int3 left, ref Int3 right, out Int3 result)
	{
		result.X = ((left.X > right.X) ? left.X : right.X);
		result.Y = ((left.Y > right.Y) ? left.Y : right.Y);
		result.Z = ((left.Z > right.Z) ? left.Z : right.Z);
	}

	public static Int3 Max(Int3 left, Int3 right)
	{
		Max(ref left, ref right, out var result);
		return result;
	}

	public static void Min(ref Int3 left, ref Int3 right, out Int3 result)
	{
		result.X = ((left.X < right.X) ? left.X : right.X);
		result.Y = ((left.Y < right.Y) ? left.Y : right.Y);
		result.Z = ((left.Z < right.Z) ? left.Z : right.Z);
	}

	public static Int3 Min(Int3 left, Int3 right)
	{
		Min(ref left, ref right, out var result);
		return result;
	}

	public static Int3 operator +(Int3 left, Int3 right)
	{
		return new Int3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
	}

	public static Int3 operator +(Int3 value)
	{
		return value;
	}

	public static Int3 operator -(Int3 left, Int3 right)
	{
		return new Int3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
	}

	public static Int3 operator -(Int3 value)
	{
		return new Int3(-value.X, -value.Y, -value.Z);
	}

	public static Int3 operator *(int scale, Int3 value)
	{
		return new Int3(value.X * scale, value.Y * scale, value.Z * scale);
	}

	public static Int3 operator *(Int3 value, int scale)
	{
		return new Int3(value.X * scale, value.Y * scale, value.Z * scale);
	}

	public static Int3 operator /(Int3 value, int scale)
	{
		return new Int3(value.X / scale, value.Y / scale, value.Z / scale);
	}

	public static bool operator ==(Int3 left, Int3 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Int3 left, Int3 right)
	{
		return !left.Equals(right);
	}

	public static explicit operator Vector2(Int3 value)
	{
		return new Vector2(value.X, value.Y);
	}

	public static explicit operator Vector3(Int3 value)
	{
		return new Vector3(value.X, value.Y, value.Z);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", new object[3] { X, Y, Z });
	}

	public string ToString(string format)
	{
		if (format == null)
		{
			return ToString();
		}
		return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", new object[3]
		{
			X.ToString(format, CultureInfo.CurrentCulture),
			Y.ToString(format, CultureInfo.CurrentCulture),
			Z.ToString(format, CultureInfo.CurrentCulture)
		});
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", new object[3] { X, Y, Z });
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			ToString(formatProvider);
		}
		return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", new object[3]
		{
			X.ToString(format, formatProvider),
			Y.ToString(format, formatProvider),
			Z.ToString(format, formatProvider)
		});
	}

	public override int GetHashCode()
	{
		int x = X;
		x = (x * 397) ^ Y;
		return (x * 397) ^ Z;
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(X);
			serializer.Writer.Write(Y);
			serializer.Writer.Write(Z);
		}
		else
		{
			X = serializer.Reader.ReadInt32();
			Y = serializer.Reader.ReadInt32();
			Z = serializer.Reader.ReadInt32();
		}
	}

	public bool Equals(Int3 other)
	{
		if (other.X == X && other.Y == Y)
		{
			return other.Z == Z;
		}
		return false;
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(value.GetType(), typeof(Int3)))
		{
			return false;
		}
		return Equals((Int3)value);
	}

	public static implicit operator Int3(int[] input)
	{
		return new Int3(input);
	}

	public static implicit operator int[](Int3 input)
	{
		return input.ToArray();
	}

	static Int3()
	{
		SizeInBytes = Marshal.SizeOf(typeof(Int3));
		Zero = default(Int3);
		UnitX = new Int3(1, 0, 0);
		UnitY = new Int3(0, 1, 0);
		UnitZ = new Int3(0, 0, 1);
		One = new Int3(1, 1, 1);
	}
}

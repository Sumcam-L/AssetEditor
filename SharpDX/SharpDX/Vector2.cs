using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DynamicSerializer("TKV2")]
public struct Vector2 : IEquatable<Vector2>, IFormattable, IDataSerializable
{
	public static readonly int SizeInBytes;

	public static readonly Vector2 Zero;

	public static readonly Vector2 UnitX;

	public static readonly Vector2 UnitY;

	public static readonly Vector2 One;

	public float X;

	public float Y;

	public bool IsNormalized => MathUtil.IsOne(X * X + Y * Y);

	public bool IsZero
	{
		get
		{
			if (X == 0f)
			{
				return Y == 0f;
			}
			return false;
		}
	}

	public float this[int index]
	{
		get
		{
			return index switch
			{
				0 => X, 
				1 => Y, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Vector2 run from 0 to 1, inclusive."), 
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
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2 run from 0 to 1, inclusive.");
			}
		}
	}

	public Vector2(float value)
	{
		X = value;
		Y = value;
	}

	public Vector2(float x, float y)
	{
		X = x;
		Y = y;
	}

	public Vector2(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 2)
		{
			throw new ArgumentOutOfRangeException("values", "There must be two and only two input values for Vector2.");
		}
		X = values[0];
		Y = values[1];
	}

	public float Length()
	{
		return (float)Math.Sqrt(X * X + Y * Y);
	}

	public float LengthSquared()
	{
		return X * X + Y * Y;
	}

	public void Normalize()
	{
		float num = Length();
		if (!MathUtil.IsZero(num))
		{
			float num2 = 1f / num;
			X *= num2;
			Y *= num2;
		}
	}

	public float[] ToArray()
	{
		return new float[2] { X, Y };
	}

	public static void Add(ref Vector2 left, ref Vector2 right, out Vector2 result)
	{
		result = new Vector2(left.X + right.X, left.Y + right.Y);
	}

	public static Vector2 Add(Vector2 left, Vector2 right)
	{
		return new Vector2(left.X + right.X, left.Y + right.Y);
	}

	public static void Add(ref Vector2 left, ref float right, out Vector2 result)
	{
		result = new Vector2(left.X + right, left.Y + right);
	}

	public static Vector2 Add(Vector2 left, float right)
	{
		return new Vector2(left.X + right, left.Y + right);
	}

	public static void Subtract(ref Vector2 left, ref Vector2 right, out Vector2 result)
	{
		result = new Vector2(left.X - right.X, left.Y - right.Y);
	}

	public static Vector2 Subtract(Vector2 left, Vector2 right)
	{
		return new Vector2(left.X - right.X, left.Y - right.Y);
	}

	public static void Subtract(ref Vector2 left, ref float right, out Vector2 result)
	{
		result = new Vector2(left.X - right, left.Y - right);
	}

	public static Vector2 Subtract(Vector2 left, float right)
	{
		return new Vector2(left.X - right, left.Y - right);
	}

	public static void Subtract(ref float left, ref Vector2 right, out Vector2 result)
	{
		result = new Vector2(left - right.X, left - right.Y);
	}

	public static Vector2 Subtract(float left, Vector2 right)
	{
		return new Vector2(left - right.X, left - right.Y);
	}

	public static void Multiply(ref Vector2 value, float scale, out Vector2 result)
	{
		result = new Vector2(value.X * scale, value.Y * scale);
	}

	public static Vector2 Multiply(Vector2 value, float scale)
	{
		return new Vector2(value.X * scale, value.Y * scale);
	}

	public static void Multiply(ref Vector2 left, ref Vector2 right, out Vector2 result)
	{
		result = new Vector2(left.X * right.X, left.Y * right.Y);
	}

	public static Vector2 Multiply(Vector2 left, Vector2 right)
	{
		return new Vector2(left.X * right.X, left.Y * right.Y);
	}

	public static void Divide(ref Vector2 value, float scale, out Vector2 result)
	{
		result = new Vector2(value.X / scale, value.Y / scale);
	}

	public static Vector2 Divide(Vector2 value, float scale)
	{
		return new Vector2(value.X / scale, value.Y / scale);
	}

	public static void Divide(float scale, ref Vector2 value, out Vector2 result)
	{
		result = new Vector2(scale / value.X, scale / value.Y);
	}

	public static Vector2 Divide(float scale, Vector2 value)
	{
		return new Vector2(scale / value.X, scale / value.Y);
	}

	public static void Negate(ref Vector2 value, out Vector2 result)
	{
		result = new Vector2(0f - value.X, 0f - value.Y);
	}

	public static Vector2 Negate(Vector2 value)
	{
		return new Vector2(0f - value.X, 0f - value.Y);
	}

	public static void Barycentric(ref Vector2 value1, ref Vector2 value2, ref Vector2 value3, float amount1, float amount2, out Vector2 result)
	{
		result = new Vector2(value1.X + amount1 * (value2.X - value1.X) + amount2 * (value3.X - value1.X), value1.Y + amount1 * (value2.Y - value1.Y) + amount2 * (value3.Y - value1.Y));
	}

	public static Vector2 Barycentric(Vector2 value1, Vector2 value2, Vector2 value3, float amount1, float amount2)
	{
		Barycentric(ref value1, ref value2, ref value3, amount1, amount2, out var result);
		return result;
	}

	public static void Clamp(ref Vector2 value, ref Vector2 min, ref Vector2 max, out Vector2 result)
	{
		float x = value.X;
		x = ((x > max.X) ? max.X : x);
		x = ((x < min.X) ? min.X : x);
		float y = value.Y;
		y = ((y > max.Y) ? max.Y : y);
		y = ((y < min.Y) ? min.Y : y);
		result = new Vector2(x, y);
	}

	public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
	{
		Clamp(ref value, ref min, ref max, out var result);
		return result;
	}

	public void Saturate()
	{
		X = ((X < 0f) ? 0f : ((X > 1f) ? 1f : X));
		Y = ((Y < 0f) ? 0f : ((Y > 1f) ? 1f : Y));
	}

	public static void Distance(ref Vector2 value1, ref Vector2 value2, out float result)
	{
		float num = value1.X - value2.X;
		float num2 = value1.Y - value2.Y;
		result = (float)Math.Sqrt(num * num + num2 * num2);
	}

	public static float Distance(Vector2 value1, Vector2 value2)
	{
		float num = value1.X - value2.X;
		float num2 = value1.Y - value2.Y;
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	public static void DistanceSquared(ref Vector2 value1, ref Vector2 value2, out float result)
	{
		float num = value1.X - value2.X;
		float num2 = value1.Y - value2.Y;
		result = num * num + num2 * num2;
	}

	public static float DistanceSquared(Vector2 value1, Vector2 value2)
	{
		float num = value1.X - value2.X;
		float num2 = value1.Y - value2.Y;
		return num * num + num2 * num2;
	}

	public static void Dot(ref Vector2 left, ref Vector2 right, out float result)
	{
		result = left.X * right.X + left.Y * right.Y;
	}

	public static float Dot(Vector2 left, Vector2 right)
	{
		return left.X * right.X + left.Y * right.Y;
	}

	public static void Normalize(ref Vector2 value, out Vector2 result)
	{
		result = value;
		result.Normalize();
	}

	public static Vector2 Normalize(Vector2 value)
	{
		value.Normalize();
		return value;
	}

	public static void Lerp(ref Vector2 start, ref Vector2 end, float amount, out Vector2 result)
	{
		result.X = MathUtil.Lerp(start.X, end.X, amount);
		result.Y = MathUtil.Lerp(start.Y, end.Y, amount);
	}

	public static Vector2 Lerp(Vector2 start, Vector2 end, float amount)
	{
		Lerp(ref start, ref end, amount, out var result);
		return result;
	}

	public static void SmoothStep(ref Vector2 start, ref Vector2 end, float amount, out Vector2 result)
	{
		amount = MathUtil.SmoothStep(amount);
		Lerp(ref start, ref end, amount, out result);
	}

	public static Vector2 SmoothStep(Vector2 start, Vector2 end, float amount)
	{
		SmoothStep(ref start, ref end, amount, out var result);
		return result;
	}

	public static void Hermite(ref Vector2 value1, ref Vector2 tangent1, ref Vector2 value2, ref Vector2 tangent2, float amount, out Vector2 result)
	{
		float num = amount * amount;
		float num2 = amount * num;
		float num3 = 2f * num2 - 3f * num + 1f;
		float num4 = -2f * num2 + 3f * num;
		float num5 = num2 - 2f * num + amount;
		float num6 = num2 - num;
		result.X = value1.X * num3 + value2.X * num4 + tangent1.X * num5 + tangent2.X * num6;
		result.Y = value1.Y * num3 + value2.Y * num4 + tangent1.Y * num5 + tangent2.Y * num6;
	}

	public static Vector2 Hermite(Vector2 value1, Vector2 tangent1, Vector2 value2, Vector2 tangent2, float amount)
	{
		Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out var result);
		return result;
	}

	public static void CatmullRom(ref Vector2 value1, ref Vector2 value2, ref Vector2 value3, ref Vector2 value4, float amount, out Vector2 result)
	{
		float num = amount * amount;
		float num2 = amount * num;
		result.X = 0.5f * (2f * value2.X + (0f - value1.X + value3.X) * amount + (2f * value1.X - 5f * value2.X + 4f * value3.X - value4.X) * num + (0f - value1.X + 3f * value2.X - 3f * value3.X + value4.X) * num2);
		result.Y = 0.5f * (2f * value2.Y + (0f - value1.Y + value3.Y) * amount + (2f * value1.Y - 5f * value2.Y + 4f * value3.Y - value4.Y) * num + (0f - value1.Y + 3f * value2.Y - 3f * value3.Y + value4.Y) * num2);
	}

	public static Vector2 CatmullRom(Vector2 value1, Vector2 value2, Vector2 value3, Vector2 value4, float amount)
	{
		CatmullRom(ref value1, ref value2, ref value3, ref value4, amount, out var result);
		return result;
	}

	public static void Max(ref Vector2 left, ref Vector2 right, out Vector2 result)
	{
		result.X = ((left.X > right.X) ? left.X : right.X);
		result.Y = ((left.Y > right.Y) ? left.Y : right.Y);
	}

	public static Vector2 Max(Vector2 left, Vector2 right)
	{
		Max(ref left, ref right, out var result);
		return result;
	}

	public static void Min(ref Vector2 left, ref Vector2 right, out Vector2 result)
	{
		result.X = ((left.X < right.X) ? left.X : right.X);
		result.Y = ((left.Y < right.Y) ? left.Y : right.Y);
	}

	public static Vector2 Min(Vector2 left, Vector2 right)
	{
		Min(ref left, ref right, out var result);
		return result;
	}

	public static void Reflect(ref Vector2 vector, ref Vector2 normal, out Vector2 result)
	{
		float num = vector.X * normal.X + vector.Y * normal.Y;
		result.X = vector.X - 2f * num * normal.X;
		result.Y = vector.Y - 2f * num * normal.Y;
	}

	public static Vector2 Reflect(Vector2 vector, Vector2 normal)
	{
		Reflect(ref vector, ref normal, out var result);
		return result;
	}

	public static void Orthogonalize(Vector2[] destination, params Vector2[] source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (destination.Length < source.Length)
		{
			throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");
		}
		for (int i = 0; i < source.Length; i++)
		{
			Vector2 vector = source[i];
			for (int j = 0; j < i; j++)
			{
				vector -= Dot(destination[j], vector) / Dot(destination[j], destination[j]) * destination[j];
			}
			destination[i] = vector;
		}
	}

	public static void Orthonormalize(Vector2[] destination, params Vector2[] source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (destination.Length < source.Length)
		{
			throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");
		}
		for (int i = 0; i < source.Length; i++)
		{
			Vector2 vector = source[i];
			for (int j = 0; j < i; j++)
			{
				vector -= Dot(destination[j], vector) * destination[j];
			}
			vector.Normalize();
			destination[i] = vector;
		}
	}

	public static void Transform(ref Vector2 vector, ref Quaternion rotation, out Vector2 result)
	{
		float num = rotation.X + rotation.X;
		float num2 = rotation.Y + rotation.Y;
		float num3 = rotation.Z + rotation.Z;
		float num4 = rotation.W * num3;
		float num5 = rotation.X * num;
		float num6 = rotation.X * num2;
		float num7 = rotation.Y * num2;
		float num8 = rotation.Z * num3;
		result = new Vector2(vector.X * (1f - num7 - num8) + vector.Y * (num6 - num4), vector.X * (num6 + num4) + vector.Y * (1f - num5 - num8));
	}

	public static Vector2 Transform(Vector2 vector, Quaternion rotation)
	{
		Transform(ref vector, ref rotation, out var result);
		return result;
	}

	public static void Transform(Vector2[] source, ref Quaternion rotation, Vector2[] destination)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (destination.Length < source.Length)
		{
			throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");
		}
		float num = rotation.X + rotation.X;
		float num2 = rotation.Y + rotation.Y;
		float num3 = rotation.Z + rotation.Z;
		float num4 = rotation.W * num3;
		float num5 = rotation.X * num;
		float num6 = rotation.X * num2;
		float num7 = rotation.Y * num2;
		float num8 = rotation.Z * num3;
		float num9 = 1f - num7 - num8;
		float num10 = num6 - num4;
		float num11 = num6 + num4;
		float num12 = 1f - num5 - num8;
		for (int i = 0; i < source.Length; i++)
		{
			ref Vector2 reference = ref destination[i];
			reference = new Vector2(source[i].X * num9 + source[i].Y * num10, source[i].X * num11 + source[i].Y * num12);
		}
	}

	public static void Transform(ref Vector2 vector, ref Matrix transform, out Vector4 result)
	{
		result = new Vector4(vector.X * transform.M11 + vector.Y * transform.M21 + transform.M41, vector.X * transform.M12 + vector.Y * transform.M22 + transform.M42, vector.X * transform.M13 + vector.Y * transform.M23 + transform.M43, vector.X * transform.M14 + vector.Y * transform.M24 + transform.M44);
	}

	public static Vector4 Transform(Vector2 vector, Matrix transform)
	{
		Transform(ref vector, ref transform, out var result);
		return result;
	}

	public static void Transform(Vector2[] source, ref Matrix transform, Vector4[] destination)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (destination.Length < source.Length)
		{
			throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");
		}
		for (int i = 0; i < source.Length; i++)
		{
			Transform(ref source[i], ref transform, out destination[i]);
		}
	}

	public static void TransformCoordinate(ref Vector2 coordinate, ref Matrix transform, out Vector2 result)
	{
		Vector4 vector = new Vector4
		{
			X = coordinate.X * transform.M11 + coordinate.Y * transform.M21 + transform.M41,
			Y = coordinate.X * transform.M12 + coordinate.Y * transform.M22 + transform.M42,
			Z = coordinate.X * transform.M13 + coordinate.Y * transform.M23 + transform.M43,
			W = 1f / (coordinate.X * transform.M14 + coordinate.Y * transform.M24 + transform.M44)
		};
		result = new Vector2(vector.X * vector.W, vector.Y * vector.W);
	}

	public static Vector2 TransformCoordinate(Vector2 coordinate, Matrix transform)
	{
		TransformCoordinate(ref coordinate, ref transform, out var result);
		return result;
	}

	public static void TransformCoordinate(Vector2[] source, ref Matrix transform, Vector2[] destination)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (destination.Length < source.Length)
		{
			throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");
		}
		for (int i = 0; i < source.Length; i++)
		{
			TransformCoordinate(ref source[i], ref transform, out destination[i]);
		}
	}

	public static void TransformNormal(ref Vector2 normal, ref Matrix transform, out Vector2 result)
	{
		result = new Vector2(normal.X * transform.M11 + normal.Y * transform.M21, normal.X * transform.M12 + normal.Y * transform.M22);
	}

	public static Vector2 TransformNormal(Vector2 normal, Matrix transform)
	{
		TransformNormal(ref normal, ref transform, out var result);
		return result;
	}

	public static void TransformNormal(Vector2[] source, ref Matrix transform, Vector2[] destination)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (destination.Length < source.Length)
		{
			throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");
		}
		for (int i = 0; i < source.Length; i++)
		{
			TransformNormal(ref source[i], ref transform, out destination[i]);
		}
	}

	public static Vector2 operator +(Vector2 left, Vector2 right)
	{
		return new Vector2(left.X + right.X, left.Y + right.Y);
	}

	public static Vector2 operator *(Vector2 left, Vector2 right)
	{
		return new Vector2(left.X * right.X, left.Y * right.Y);
	}

	public static Vector2 operator +(Vector2 value)
	{
		return value;
	}

	public static Vector2 operator -(Vector2 left, Vector2 right)
	{
		return new Vector2(left.X - right.X, left.Y - right.Y);
	}

	public static Vector2 operator -(Vector2 value)
	{
		return new Vector2(0f - value.X, 0f - value.Y);
	}

	public static Vector2 operator *(float scale, Vector2 value)
	{
		return new Vector2(value.X * scale, value.Y * scale);
	}

	public static Vector2 operator *(Vector2 value, float scale)
	{
		return new Vector2(value.X * scale, value.Y * scale);
	}

	public static Vector2 operator /(Vector2 value, float scale)
	{
		return new Vector2(value.X / scale, value.Y / scale);
	}

	public static Vector2 operator /(float scale, Vector2 value)
	{
		return new Vector2(scale / value.X, scale / value.Y);
	}

	public static Vector2 operator /(Vector2 value, Vector2 scale)
	{
		return new Vector2(value.X / scale.X, value.Y / scale.Y);
	}

	public static Vector2 operator +(Vector2 value, float scalar)
	{
		return new Vector2(value.X + scalar, value.Y + scalar);
	}

	public static Vector2 operator +(float scalar, Vector2 value)
	{
		return new Vector2(scalar + value.X, scalar + value.Y);
	}

	public static Vector2 operator -(Vector2 value, float scalar)
	{
		return new Vector2(value.X - scalar, value.Y - scalar);
	}

	public static Vector2 operator -(float scalar, Vector2 value)
	{
		return new Vector2(scalar - value.X, scalar - value.Y);
	}

	public static bool operator ==(Vector2 left, Vector2 right)
	{
		return left.Equals(ref right);
	}

	public static bool operator !=(Vector2 left, Vector2 right)
	{
		return !left.Equals(ref right);
	}

	public static explicit operator Vector3(Vector2 value)
	{
		return new Vector3(value, 0f);
	}

	public static explicit operator Vector4(Vector2 value)
	{
		return new Vector4(value, 0f, 0f);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", new object[2] { X, Y });
	}

	public string ToString(string format)
	{
		if (format == null)
		{
			return ToString();
		}
		return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", new object[2]
		{
			X.ToString(format, CultureInfo.CurrentCulture),
			Y.ToString(format, CultureInfo.CurrentCulture)
		});
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "X:{0} Y:{1}", new object[2] { X, Y });
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			ToString(formatProvider);
		}
		return string.Format(formatProvider, "X:{0} Y:{1}", new object[2]
		{
			X.ToString(format, formatProvider),
			Y.ToString(format, formatProvider)
		});
	}

	public override int GetHashCode()
	{
		return (X.GetHashCode() * 397) ^ Y.GetHashCode();
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(X);
			serializer.Writer.Write(Y);
		}
		else
		{
			X = serializer.Reader.ReadSingle();
			Y = serializer.Reader.ReadSingle();
		}
	}

	public bool Equals(ref Vector2 other)
	{
		if (MathUtil.NearEqual(other.X, X))
		{
			return MathUtil.NearEqual(other.Y, Y);
		}
		return false;
	}

	public bool Equals(Vector2 other)
	{
		return Equals(ref other);
	}

	public override bool Equals(object value)
	{
		if (!(value is Vector2 other))
		{
			return false;
		}
		return Equals(ref other);
	}

	static Vector2()
	{
		SizeInBytes = Marshal.SizeOf(typeof(Vector2));
		Zero = default(Vector2);
		UnitX = new Vector2(1f, 0f);
		UnitY = new Vector2(0f, 1f);
		One = new Vector2(1f, 1f);
	}
}

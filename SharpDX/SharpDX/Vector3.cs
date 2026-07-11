using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DynamicSerializer("TKV3")]
public struct Vector3 : IEquatable<Vector3>, IFormattable, IDataSerializable
{
	public static readonly int SizeInBytes;

	public static readonly Vector3 Zero;

	public static readonly Vector3 UnitX;

	public static readonly Vector3 UnitY;

	public static readonly Vector3 UnitZ;

	public static readonly Vector3 One;

	public static readonly Vector3 Up;

	public static readonly Vector3 Down;

	public static readonly Vector3 Left;

	public static readonly Vector3 Right;

	public static readonly Vector3 ForwardRH;

	public static readonly Vector3 ForwardLH;

	public static readonly Vector3 BackwardRH;

	public static readonly Vector3 BackwardLH;

	public float X;

	public float Y;

	public float Z;

	public bool IsNormalized => MathUtil.IsOne(X * X + Y * Y + Z * Z);

	public bool IsZero
	{
		get
		{
			if (X == 0f && Y == 0f)
			{
				return Z == 0f;
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
				2 => Z, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Vector3 run from 0 to 2, inclusive."), 
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
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3 run from 0 to 2, inclusive.");
			}
		}
	}

	public Vector3(float value)
	{
		X = value;
		Y = value;
		Z = value;
	}

	public Vector3(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public Vector3(Vector2 value, float z)
	{
		X = value.X;
		Y = value.Y;
		Z = z;
	}

	public Vector3(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 3)
		{
			throw new ArgumentOutOfRangeException("values", "There must be three and only three input values for Vector3.");
		}
		X = values[0];
		Y = values[1];
		Z = values[2];
	}

	public float Length()
	{
		return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
	}

	public float LengthSquared()
	{
		return X * X + Y * Y + Z * Z;
	}

	public void Normalize()
	{
		float num = Length();
		if (!MathUtil.IsZero(num))
		{
			float num2 = 1f / num;
			X *= num2;
			Y *= num2;
			Z *= num2;
		}
	}

	public float[] ToArray()
	{
		return new float[3] { X, Y, Z };
	}

	public static void Add(ref Vector3 left, ref Vector3 right, out Vector3 result)
	{
		result = new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
	}

	public static Vector3 Add(Vector3 left, Vector3 right)
	{
		return new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
	}

	public static void Add(ref Vector3 left, ref float right, out Vector3 result)
	{
		result = new Vector3(left.X + right, left.Y + right, left.Z + right);
	}

	public static Vector3 Add(Vector3 left, float right)
	{
		return new Vector3(left.X + right, left.Y + right, left.Z + right);
	}

	public static void Subtract(ref Vector3 left, ref Vector3 right, out Vector3 result)
	{
		result = new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
	}

	public static Vector3 Subtract(Vector3 left, Vector3 right)
	{
		return new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
	}

	public static void Subtract(ref Vector3 left, ref float right, out Vector3 result)
	{
		result = new Vector3(left.X - right, left.Y - right, left.Z - right);
	}

	public static Vector3 Subtract(Vector3 left, float right)
	{
		return new Vector3(left.X - right, left.Y - right, left.Z - right);
	}

	public static void Subtract(ref float left, ref Vector3 right, out Vector3 result)
	{
		result = new Vector3(left - right.X, left - right.Y, left - right.Z);
	}

	public static Vector3 Subtract(float left, Vector3 right)
	{
		return new Vector3(left - right.X, left - right.Y, left - right.Z);
	}

	public static void Multiply(ref Vector3 value, float scale, out Vector3 result)
	{
		result = new Vector3(value.X * scale, value.Y * scale, value.Z * scale);
	}

	public static Vector3 Multiply(Vector3 value, float scale)
	{
		return new Vector3(value.X * scale, value.Y * scale, value.Z * scale);
	}

	public static void Multiply(ref Vector3 left, ref Vector3 right, out Vector3 result)
	{
		result = new Vector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
	}

	public static Vector3 Multiply(Vector3 left, Vector3 right)
	{
		return new Vector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
	}

	public static void Divide(ref Vector3 value, float scale, out Vector3 result)
	{
		result = new Vector3(value.X / scale, value.Y / scale, value.Z / scale);
	}

	public static Vector3 Divide(Vector3 value, float scale)
	{
		return new Vector3(value.X / scale, value.Y / scale, value.Z / scale);
	}

	public static void Divide(float scale, ref Vector3 value, out Vector3 result)
	{
		result = new Vector3(scale / value.X, scale / value.Y, scale / value.Z);
	}

	public static Vector3 Divide(float scale, Vector3 value)
	{
		return new Vector3(scale / value.X, scale / value.Y, scale / value.Z);
	}

	public static void Negate(ref Vector3 value, out Vector3 result)
	{
		result = new Vector3(0f - value.X, 0f - value.Y, 0f - value.Z);
	}

	public static Vector3 Negate(Vector3 value)
	{
		return new Vector3(0f - value.X, 0f - value.Y, 0f - value.Z);
	}

	public static void Barycentric(ref Vector3 value1, ref Vector3 value2, ref Vector3 value3, float amount1, float amount2, out Vector3 result)
	{
		result = new Vector3(value1.X + amount1 * (value2.X - value1.X) + amount2 * (value3.X - value1.X), value1.Y + amount1 * (value2.Y - value1.Y) + amount2 * (value3.Y - value1.Y), value1.Z + amount1 * (value2.Z - value1.Z) + amount2 * (value3.Z - value1.Z));
	}

	public static Vector3 Barycentric(Vector3 value1, Vector3 value2, Vector3 value3, float amount1, float amount2)
	{
		Barycentric(ref value1, ref value2, ref value3, amount1, amount2, out var result);
		return result;
	}

	public static void Clamp(ref Vector3 value, ref Vector3 min, ref Vector3 max, out Vector3 result)
	{
		float x = value.X;
		x = ((x > max.X) ? max.X : x);
		x = ((x < min.X) ? min.X : x);
		float y = value.Y;
		y = ((y > max.Y) ? max.Y : y);
		y = ((y < min.Y) ? min.Y : y);
		float z = value.Z;
		z = ((z > max.Z) ? max.Z : z);
		z = ((z < min.Z) ? min.Z : z);
		result = new Vector3(x, y, z);
	}

	public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
	{
		Clamp(ref value, ref min, ref max, out var result);
		return result;
	}

	public static void Cross(ref Vector3 left, ref Vector3 right, out Vector3 result)
	{
		result = new Vector3(left.Y * right.Z - left.Z * right.Y, left.Z * right.X - left.X * right.Z, left.X * right.Y - left.Y * right.X);
	}

	public static Vector3 Cross(Vector3 left, Vector3 right)
	{
		Cross(ref left, ref right, out var result);
		return result;
	}

	public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result)
	{
		float num = value1.X - value2.X;
		float num2 = value1.Y - value2.Y;
		float num3 = value1.Z - value2.Z;
		result = (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	public static float Distance(Vector3 value1, Vector3 value2)
	{
		float num = value1.X - value2.X;
		float num2 = value1.Y - value2.Y;
		float num3 = value1.Z - value2.Z;
		return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	public static void DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result)
	{
		float num = value1.X - value2.X;
		float num2 = value1.Y - value2.Y;
		float num3 = value1.Z - value2.Z;
		result = num * num + num2 * num2 + num3 * num3;
	}

	public static float DistanceSquared(Vector3 value1, Vector3 value2)
	{
		float num = value1.X - value2.X;
		float num2 = value1.Y - value2.Y;
		float num3 = value1.Z - value2.Z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public static bool NearEqual(Vector3 left, Vector3 right, Vector3 epsilon)
	{
		return NearEqual(ref left, ref right, ref epsilon);
	}

	public static bool NearEqual(ref Vector3 left, ref Vector3 right, ref Vector3 epsilon)
	{
		if (MathUtil.WithinEpsilon(left.X, right.X, epsilon.X) && MathUtil.WithinEpsilon(left.Y, right.Y, epsilon.Y))
		{
			return MathUtil.WithinEpsilon(left.Z, right.Z, epsilon.Z);
		}
		return false;
	}

	public static void Dot(ref Vector3 left, ref Vector3 right, out float result)
	{
		result = left.X * right.X + left.Y * right.Y + left.Z * right.Z;
	}

	public static float Dot(Vector3 left, Vector3 right)
	{
		return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
	}

	public static void Normalize(ref Vector3 value, out Vector3 result)
	{
		result = value;
		result.Normalize();
	}

	public static Vector3 Normalize(Vector3 value)
	{
		value.Normalize();
		return value;
	}

	public static void Lerp(ref Vector3 start, ref Vector3 end, float amount, out Vector3 result)
	{
		result.X = MathUtil.Lerp(start.X, end.X, amount);
		result.Y = MathUtil.Lerp(start.Y, end.Y, amount);
		result.Z = MathUtil.Lerp(start.Z, end.Z, amount);
	}

	public static Vector3 Lerp(Vector3 start, Vector3 end, float amount)
	{
		Lerp(ref start, ref end, amount, out var result);
		return result;
	}

	public static void SmoothStep(ref Vector3 start, ref Vector3 end, float amount, out Vector3 result)
	{
		amount = MathUtil.SmoothStep(amount);
		Lerp(ref start, ref end, amount, out result);
	}

	public static Vector3 SmoothStep(Vector3 start, Vector3 end, float amount)
	{
		SmoothStep(ref start, ref end, amount, out var result);
		return result;
	}

	public static void Hermite(ref Vector3 value1, ref Vector3 tangent1, ref Vector3 value2, ref Vector3 tangent2, float amount, out Vector3 result)
	{
		float num = amount * amount;
		float num2 = amount * num;
		float num3 = 2f * num2 - 3f * num + 1f;
		float num4 = -2f * num2 + 3f * num;
		float num5 = num2 - 2f * num + amount;
		float num6 = num2 - num;
		result.X = value1.X * num3 + value2.X * num4 + tangent1.X * num5 + tangent2.X * num6;
		result.Y = value1.Y * num3 + value2.Y * num4 + tangent1.Y * num5 + tangent2.Y * num6;
		result.Z = value1.Z * num3 + value2.Z * num4 + tangent1.Z * num5 + tangent2.Z * num6;
	}

	public static Vector3 Hermite(Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, float amount)
	{
		Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out var result);
		return result;
	}

	public static void CatmullRom(ref Vector3 value1, ref Vector3 value2, ref Vector3 value3, ref Vector3 value4, float amount, out Vector3 result)
	{
		float num = amount * amount;
		float num2 = amount * num;
		result.X = 0.5f * (2f * value2.X + (0f - value1.X + value3.X) * amount + (2f * value1.X - 5f * value2.X + 4f * value3.X - value4.X) * num + (0f - value1.X + 3f * value2.X - 3f * value3.X + value4.X) * num2);
		result.Y = 0.5f * (2f * value2.Y + (0f - value1.Y + value3.Y) * amount + (2f * value1.Y - 5f * value2.Y + 4f * value3.Y - value4.Y) * num + (0f - value1.Y + 3f * value2.Y - 3f * value3.Y + value4.Y) * num2);
		result.Z = 0.5f * (2f * value2.Z + (0f - value1.Z + value3.Z) * amount + (2f * value1.Z - 5f * value2.Z + 4f * value3.Z - value4.Z) * num + (0f - value1.Z + 3f * value2.Z - 3f * value3.Z + value4.Z) * num2);
	}

	public static Vector3 CatmullRom(Vector3 value1, Vector3 value2, Vector3 value3, Vector3 value4, float amount)
	{
		CatmullRom(ref value1, ref value2, ref value3, ref value4, amount, out var result);
		return result;
	}

	public static void Max(ref Vector3 left, ref Vector3 right, out Vector3 result)
	{
		result.X = ((left.X > right.X) ? left.X : right.X);
		result.Y = ((left.Y > right.Y) ? left.Y : right.Y);
		result.Z = ((left.Z > right.Z) ? left.Z : right.Z);
	}

	public static Vector3 Max(Vector3 left, Vector3 right)
	{
		Max(ref left, ref right, out var result);
		return result;
	}

	public static void Min(ref Vector3 left, ref Vector3 right, out Vector3 result)
	{
		result.X = ((left.X < right.X) ? left.X : right.X);
		result.Y = ((left.Y < right.Y) ? left.Y : right.Y);
		result.Z = ((left.Z < right.Z) ? left.Z : right.Z);
	}

	public static Vector3 Min(Vector3 left, Vector3 right)
	{
		Min(ref left, ref right, out var result);
		return result;
	}

	public static void Project(ref Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, ref Matrix worldViewProjection, out Vector3 result)
	{
		Vector3 result2 = default(Vector3);
		TransformCoordinate(ref vector, ref worldViewProjection, out result2);
		result = new Vector3((1f + result2.X) * 0.5f * width + x, (1f - result2.Y) * 0.5f * height + y, result2.Z * (maxZ - minZ) + minZ);
	}

	public static Vector3 Project(Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, Matrix worldViewProjection)
	{
		Project(ref vector, x, y, width, height, minZ, maxZ, ref worldViewProjection, out var result);
		return result;
	}

	public static void Unproject(ref Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, ref Matrix worldViewProjection, out Vector3 result)
	{
		Vector3 coordinate = default(Vector3);
		Matrix result2 = default(Matrix);
		Matrix.Invert(ref worldViewProjection, out result2);
		coordinate.X = (vector.X - x) / width * 2f - 1f;
		coordinate.Y = 0f - ((vector.Y - y) / height * 2f - 1f);
		coordinate.Z = (vector.Z - minZ) / (maxZ - minZ);
		TransformCoordinate(ref coordinate, ref result2, out result);
	}

	public static Vector3 Unproject(Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, Matrix worldViewProjection)
	{
		Unproject(ref vector, x, y, width, height, minZ, maxZ, ref worldViewProjection, out var result);
		return result;
	}

	public static void Reflect(ref Vector3 vector, ref Vector3 normal, out Vector3 result)
	{
		float num = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
		result.X = vector.X - 2f * num * normal.X;
		result.Y = vector.Y - 2f * num * normal.Y;
		result.Z = vector.Z - 2f * num * normal.Z;
	}

	public static Vector3 Reflect(Vector3 vector, Vector3 normal)
	{
		Reflect(ref vector, ref normal, out var result);
		return result;
	}

	public static void Orthogonalize(Vector3[] destination, params Vector3[] source)
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
			Vector3 vector = source[i];
			for (int j = 0; j < i; j++)
			{
				vector -= Dot(destination[j], vector) / Dot(destination[j], destination[j]) * destination[j];
			}
			destination[i] = vector;
		}
	}

	public static void Orthonormalize(Vector3[] destination, params Vector3[] source)
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
			Vector3 vector = source[i];
			for (int j = 0; j < i; j++)
			{
				vector -= Dot(destination[j], vector) * destination[j];
			}
			vector.Normalize();
			destination[i] = vector;
		}
	}

	public static void Transform(ref Vector3 vector, ref Quaternion rotation, out Vector3 result)
	{
		float num = rotation.X + rotation.X;
		float num2 = rotation.Y + rotation.Y;
		float num3 = rotation.Z + rotation.Z;
		float num4 = rotation.W * num;
		float num5 = rotation.W * num2;
		float num6 = rotation.W * num3;
		float num7 = rotation.X * num;
		float num8 = rotation.X * num2;
		float num9 = rotation.X * num3;
		float num10 = rotation.Y * num2;
		float num11 = rotation.Y * num3;
		float num12 = rotation.Z * num3;
		result = new Vector3(vector.X * (1f - num10 - num12) + vector.Y * (num8 - num6) + vector.Z * (num9 + num5), vector.X * (num8 + num6) + vector.Y * (1f - num7 - num12) + vector.Z * (num11 - num4), vector.X * (num9 - num5) + vector.Y * (num11 + num4) + vector.Z * (1f - num7 - num10));
	}

	public static Vector3 Transform(Vector3 vector, Quaternion rotation)
	{
		Transform(ref vector, ref rotation, out var result);
		return result;
	}

	public static void Transform(Vector3[] source, ref Quaternion rotation, Vector3[] destination)
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
		float num4 = rotation.W * num;
		float num5 = rotation.W * num2;
		float num6 = rotation.W * num3;
		float num7 = rotation.X * num;
		float num8 = rotation.X * num2;
		float num9 = rotation.X * num3;
		float num10 = rotation.Y * num2;
		float num11 = rotation.Y * num3;
		float num12 = rotation.Z * num3;
		float num13 = 1f - num10 - num12;
		float num14 = num8 - num6;
		float num15 = num9 + num5;
		float num16 = num8 + num6;
		float num17 = 1f - num7 - num12;
		float num18 = num11 - num4;
		float num19 = num9 - num5;
		float num20 = num11 + num4;
		float num21 = 1f - num7 - num10;
		for (int i = 0; i < source.Length; i++)
		{
			ref Vector3 reference = ref destination[i];
			reference = new Vector3(source[i].X * num13 + source[i].Y * num14 + source[i].Z * num15, source[i].X * num16 + source[i].Y * num17 + source[i].Z * num18, source[i].X * num19 + source[i].Y * num20 + source[i].Z * num21);
		}
	}

	public static void Transform(ref Vector3 vector, ref Matrix3x3 transform, out Vector3 result)
	{
		result = new Vector3(vector.X * transform.M11 + vector.Y * transform.M21 + vector.Z * transform.M31, vector.X * transform.M12 + vector.Y * transform.M22 + vector.Z * transform.M32, vector.X * transform.M13 + vector.Y * transform.M23 + vector.Z * transform.M33);
	}

	public static Vector3 Transform(Vector3 vector, Matrix3x3 transform)
	{
		Transform(ref vector, ref transform, out var result);
		return result;
	}

	public static void Transform(ref Vector3 vector, ref Matrix transform, out Vector3 result)
	{
		Transform(ref vector, ref transform, out Vector4 result2);
		result = (Vector3)result2;
	}

	public static void Transform(ref Vector3 vector, ref Matrix transform, out Vector4 result)
	{
		result = new Vector4(vector.X * transform.M11 + vector.Y * transform.M21 + vector.Z * transform.M31 + transform.M41, vector.X * transform.M12 + vector.Y * transform.M22 + vector.Z * transform.M32 + transform.M42, vector.X * transform.M13 + vector.Y * transform.M23 + vector.Z * transform.M33 + transform.M43, vector.X * transform.M14 + vector.Y * transform.M24 + vector.Z * transform.M34 + transform.M44);
	}

	public static Vector4 Transform(Vector3 vector, Matrix transform)
	{
		Transform(ref vector, ref transform, out Vector4 result);
		return result;
	}

	public static void Transform(Vector3[] source, ref Matrix transform, Vector4[] destination)
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

	public static void TransformCoordinate(ref Vector3 coordinate, ref Matrix transform, out Vector3 result)
	{
		Vector4 vector = new Vector4
		{
			X = coordinate.X * transform.M11 + coordinate.Y * transform.M21 + coordinate.Z * transform.M31 + transform.M41,
			Y = coordinate.X * transform.M12 + coordinate.Y * transform.M22 + coordinate.Z * transform.M32 + transform.M42,
			Z = coordinate.X * transform.M13 + coordinate.Y * transform.M23 + coordinate.Z * transform.M33 + transform.M43,
			W = 1f / (coordinate.X * transform.M14 + coordinate.Y * transform.M24 + coordinate.Z * transform.M34 + transform.M44)
		};
		result = new Vector3(vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W);
	}

	public static Vector3 TransformCoordinate(Vector3 coordinate, Matrix transform)
	{
		TransformCoordinate(ref coordinate, ref transform, out var result);
		return result;
	}

	public static void TransformCoordinate(Vector3[] source, ref Matrix transform, Vector3[] destination)
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

	public static void TransformNormal(ref Vector3 normal, ref Matrix transform, out Vector3 result)
	{
		result = new Vector3(normal.X * transform.M11 + normal.Y * transform.M21 + normal.Z * transform.M31, normal.X * transform.M12 + normal.Y * transform.M22 + normal.Z * transform.M32, normal.X * transform.M13 + normal.Y * transform.M23 + normal.Z * transform.M33);
	}

	public static Vector3 TransformNormal(Vector3 normal, Matrix transform)
	{
		TransformNormal(ref normal, ref transform, out var result);
		return result;
	}

	public static void TransformNormal(Vector3[] source, ref Matrix transform, Vector3[] destination)
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

	public static Vector3 operator +(Vector3 left, Vector3 right)
	{
		return new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
	}

	public static Vector3 operator *(Vector3 left, Vector3 right)
	{
		return new Vector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
	}

	public static Vector3 operator +(Vector3 value)
	{
		return value;
	}

	public static Vector3 operator -(Vector3 left, Vector3 right)
	{
		return new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
	}

	public static Vector3 operator -(Vector3 value)
	{
		return new Vector3(0f - value.X, 0f - value.Y, 0f - value.Z);
	}

	public static Vector3 operator *(float scale, Vector3 value)
	{
		return new Vector3(value.X * scale, value.Y * scale, value.Z * scale);
	}

	public static Vector3 operator *(Vector3 value, float scale)
	{
		return new Vector3(value.X * scale, value.Y * scale, value.Z * scale);
	}

	public static Vector3 operator /(Vector3 value, float scale)
	{
		return new Vector3(value.X / scale, value.Y / scale, value.Z / scale);
	}

	public static Vector3 operator /(float scale, Vector3 value)
	{
		return new Vector3(scale / value.X, scale / value.Y, scale / value.Z);
	}

	public static Vector3 operator /(Vector3 value, Vector3 scale)
	{
		return new Vector3(value.X / scale.X, value.Y / scale.Y, value.Z / scale.Z);
	}

	public static Vector3 operator +(Vector3 value, float scalar)
	{
		return new Vector3(value.X + scalar, value.Y + scalar, value.Z + scalar);
	}

	public static Vector3 operator +(float scalar, Vector3 value)
	{
		return new Vector3(scalar + value.X, scalar + value.Y, scalar + value.Z);
	}

	public static Vector3 operator -(Vector3 value, float scalar)
	{
		return new Vector3(value.X - scalar, value.Y - scalar, value.Z - scalar);
	}

	public static Vector3 operator -(float scalar, Vector3 value)
	{
		return new Vector3(scalar - value.X, scalar - value.Y, scalar - value.Z);
	}

	public static bool operator ==(Vector3 left, Vector3 right)
	{
		return left.Equals(ref right);
	}

	public static bool operator !=(Vector3 left, Vector3 right)
	{
		return !left.Equals(ref right);
	}

	public static explicit operator Vector2(Vector3 value)
	{
		return new Vector2(value.X, value.Y);
	}

	public static explicit operator Vector4(Vector3 value)
	{
		return new Vector4(value, 0f);
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
			return ToString(formatProvider);
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
		int hashCode = X.GetHashCode();
		hashCode = (hashCode * 397) ^ Y.GetHashCode();
		return (hashCode * 397) ^ Z.GetHashCode();
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
			X = serializer.Reader.ReadSingle();
			Y = serializer.Reader.ReadSingle();
			Z = serializer.Reader.ReadSingle();
		}
	}

	public bool Equals(ref Vector3 other)
	{
		if (MathUtil.NearEqual(other.X, X) && MathUtil.NearEqual(other.Y, Y))
		{
			return MathUtil.NearEqual(other.Z, Z);
		}
		return false;
	}

	public bool Equals(Vector3 other)
	{
		return Equals(ref other);
	}

	public override bool Equals(object value)
	{
		if (!(value is Vector3 other))
		{
			return false;
		}
		return Equals(ref other);
	}

	static Vector3()
	{
		SizeInBytes = Marshal.SizeOf(typeof(Vector3));
		Zero = default(Vector3);
		UnitX = new Vector3(1f, 0f, 0f);
		UnitY = new Vector3(0f, 1f, 0f);
		UnitZ = new Vector3(0f, 0f, 1f);
		One = new Vector3(1f, 1f, 1f);
		Up = new Vector3(0f, 1f, 0f);
		Down = new Vector3(0f, -1f, 0f);
		Left = new Vector3(-1f, 0f, 0f);
		Right = new Vector3(1f, 0f, 0f);
		ForwardRH = new Vector3(0f, 0f, -1f);
		ForwardLH = new Vector3(0f, 0f, 1f);
		BackwardRH = new Vector3(0f, 0f, 1f);
		BackwardLH = new Vector3(0f, 0f, -1f);
	}
}

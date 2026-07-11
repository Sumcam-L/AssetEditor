using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Quaternion : IEquatable<Quaternion>, IFormattable, IDataSerializable
{
	public static readonly int SizeInBytes;

	public static readonly Quaternion Zero;

	public static readonly Quaternion One;

	public static readonly Quaternion Identity;

	public float X;

	public float Y;

	public float Z;

	public float W;

	public bool IsIdentity => Equals(Identity);

	public bool IsNormalized => MathUtil.IsOne(X * X + Y * Y + Z * Z + W * W);

	public float Angle
	{
		get
		{
			float a = X * X + Y * Y + Z * Z;
			if (MathUtil.IsZero(a))
			{
				return 0f;
			}
			return (float)(2.0 * Math.Acos(MathUtil.Clamp(W, -1f, 1f)));
		}
	}

	public Vector3 Axis
	{
		get
		{
			float num = X * X + Y * Y + Z * Z;
			if (MathUtil.IsZero(num))
			{
				return Vector3.UnitX;
			}
			float num2 = 1f / (float)Math.Sqrt(num);
			return new Vector3(X * num2, Y * num2, Z * num2);
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
				3 => W, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Quaternion run from 0 to 3, inclusive."), 
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
				throw new ArgumentOutOfRangeException("index", "Indices for Quaternion run from 0 to 3, inclusive.");
			}
		}
	}

	public Quaternion(float value)
	{
		X = value;
		Y = value;
		Z = value;
		W = value;
	}

	public Quaternion(Vector4 value)
	{
		X = value.X;
		Y = value.Y;
		Z = value.Z;
		W = value.W;
	}

	public Quaternion(Vector3 value, float w)
	{
		X = value.X;
		Y = value.Y;
		Z = value.Z;
		W = w;
	}

	public Quaternion(Vector2 value, float z, float w)
	{
		X = value.X;
		Y = value.Y;
		Z = z;
		W = w;
	}

	public Quaternion(float x, float y, float z, float w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public Quaternion(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 4)
		{
			throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for Quaternion.");
		}
		X = values[0];
		Y = values[1];
		Z = values[2];
		W = values[3];
	}

	public void Conjugate()
	{
		X = 0f - X;
		Y = 0f - Y;
		Z = 0f - Z;
	}

	public void Invert()
	{
		float num = LengthSquared();
		if (!MathUtil.IsZero(num))
		{
			num = 1f / num;
			X = (0f - X) * num;
			Y = (0f - Y) * num;
			Z = (0f - Z) * num;
			W *= num;
		}
	}

	public float Length()
	{
		return (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
	}

	public float LengthSquared()
	{
		return X * X + Y * Y + Z * Z + W * W;
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
			W *= num2;
		}
	}

	public float[] ToArray()
	{
		return new float[4] { X, Y, Z, W };
	}

	public static void Add(ref Quaternion left, ref Quaternion right, out Quaternion result)
	{
		result.X = left.X + right.X;
		result.Y = left.Y + right.Y;
		result.Z = left.Z + right.Z;
		result.W = left.W + right.W;
	}

	public static Quaternion Add(Quaternion left, Quaternion right)
	{
		Add(ref left, ref right, out var result);
		return result;
	}

	public static void Subtract(ref Quaternion left, ref Quaternion right, out Quaternion result)
	{
		result.X = left.X - right.X;
		result.Y = left.Y - right.Y;
		result.Z = left.Z - right.Z;
		result.W = left.W - right.W;
	}

	public static Quaternion Subtract(Quaternion left, Quaternion right)
	{
		Subtract(ref left, ref right, out var result);
		return result;
	}

	public static void Multiply(ref Quaternion value, float scale, out Quaternion result)
	{
		result.X = value.X * scale;
		result.Y = value.Y * scale;
		result.Z = value.Z * scale;
		result.W = value.W * scale;
	}

	public static Quaternion Multiply(Quaternion value, float scale)
	{
		Multiply(ref value, scale, out var result);
		return result;
	}

	public static void Multiply(ref Quaternion left, ref Quaternion right, out Quaternion result)
	{
		float x = left.X;
		float y = left.Y;
		float z = left.Z;
		float w = left.W;
		float x2 = right.X;
		float y2 = right.Y;
		float z2 = right.Z;
		float w2 = right.W;
		float num = y * z2 - z * y2;
		float num2 = z * x2 - x * z2;
		float num3 = x * y2 - y * x2;
		float num4 = x * x2 + y * y2 + z * z2;
		result.X = x * w2 + x2 * w + num;
		result.Y = y * w2 + y2 * w + num2;
		result.Z = z * w2 + z2 * w + num3;
		result.W = w * w2 - num4;
	}

	public static Quaternion Multiply(Quaternion left, Quaternion right)
	{
		Multiply(ref left, ref right, out var result);
		return result;
	}

	public static void Negate(ref Quaternion value, out Quaternion result)
	{
		result.X = 0f - value.X;
		result.Y = 0f - value.Y;
		result.Z = 0f - value.Z;
		result.W = 0f - value.W;
	}

	public static Quaternion Negate(Quaternion value)
	{
		Negate(ref value, out var result);
		return result;
	}

	public static void Barycentric(ref Quaternion value1, ref Quaternion value2, ref Quaternion value3, float amount1, float amount2, out Quaternion result)
	{
		Slerp(ref value1, ref value2, amount1 + amount2, out var result2);
		Slerp(ref value1, ref value3, amount1 + amount2, out var result3);
		Slerp(ref result2, ref result3, amount2 / (amount1 + amount2), out result);
	}

	public static Quaternion Barycentric(Quaternion value1, Quaternion value2, Quaternion value3, float amount1, float amount2)
	{
		Barycentric(ref value1, ref value2, ref value3, amount1, amount2, out var result);
		return result;
	}

	public static void Conjugate(ref Quaternion value, out Quaternion result)
	{
		result.X = 0f - value.X;
		result.Y = 0f - value.Y;
		result.Z = 0f - value.Z;
		result.W = value.W;
	}

	public static Quaternion Conjugate(Quaternion value)
	{
		Conjugate(ref value, out var result);
		return result;
	}

	public static void Dot(ref Quaternion left, ref Quaternion right, out float result)
	{
		result = left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
	}

	public static float Dot(Quaternion left, Quaternion right)
	{
		return left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
	}

	public static void Exponential(ref Quaternion value, out Quaternion result)
	{
		float num = (float)Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
		float num2 = (float)Math.Sin(num);
		if (!MathUtil.IsZero(num2))
		{
			float num3 = num2 / num;
			result.X = num3 * value.X;
			result.Y = num3 * value.Y;
			result.Z = num3 * value.Z;
		}
		else
		{
			result = value;
		}
		result.W = (float)Math.Cos(num);
	}

	public static Quaternion Exponential(Quaternion value)
	{
		Exponential(ref value, out var result);
		return result;
	}

	public static void Invert(ref Quaternion value, out Quaternion result)
	{
		result = value;
		result.Invert();
	}

	public static Quaternion Invert(Quaternion value)
	{
		Invert(ref value, out var result);
		return result;
	}

	public static void Lerp(ref Quaternion start, ref Quaternion end, float amount, out Quaternion result)
	{
		float num = 1f - amount;
		if (Dot(start, end) >= 0f)
		{
			result.X = num * start.X + amount * end.X;
			result.Y = num * start.Y + amount * end.Y;
			result.Z = num * start.Z + amount * end.Z;
			result.W = num * start.W + amount * end.W;
		}
		else
		{
			result.X = num * start.X - amount * end.X;
			result.Y = num * start.Y - amount * end.Y;
			result.Z = num * start.Z - amount * end.Z;
			result.W = num * start.W - amount * end.W;
		}
		result.Normalize();
	}

	public static Quaternion Lerp(Quaternion start, Quaternion end, float amount)
	{
		Lerp(ref start, ref end, amount, out var result);
		return result;
	}

	public static void Logarithm(ref Quaternion value, out Quaternion result)
	{
		if ((double)Math.Abs(value.W) < 1.0)
		{
			float num = (float)Math.Acos(value.W);
			float num2 = (float)Math.Sin(num);
			if (!MathUtil.IsZero(num2))
			{
				float num3 = num / num2;
				result.X = value.X * num3;
				result.Y = value.Y * num3;
				result.Z = value.Z * num3;
			}
			else
			{
				result = value;
			}
		}
		else
		{
			result = value;
		}
		result.W = 0f;
	}

	public static Quaternion Logarithm(Quaternion value)
	{
		Logarithm(ref value, out var result);
		return result;
	}

	public static void Normalize(ref Quaternion value, out Quaternion result)
	{
		Quaternion quaternion = value;
		result = quaternion;
		result.Normalize();
	}

	public static Quaternion Normalize(Quaternion value)
	{
		value.Normalize();
		return value;
	}

	public static void RotationAxis(ref Vector3 axis, float angle, out Quaternion result)
	{
		Vector3.Normalize(ref axis, out var result2);
		float num = angle * 0.5f;
		float num2 = (float)Math.Sin(num);
		float w = (float)Math.Cos(num);
		result.X = result2.X * num2;
		result.Y = result2.Y * num2;
		result.Z = result2.Z * num2;
		result.W = w;
	}

	public static Quaternion RotationAxis(Vector3 axis, float angle)
	{
		RotationAxis(ref axis, angle, out var result);
		return result;
	}

	public static void RotationMatrix(ref Matrix matrix, out Quaternion result)
	{
		float num = matrix.M11 + matrix.M22 + matrix.M33;
		if (num > 0f)
		{
			float num2 = (float)Math.Sqrt(num + 1f);
			result.W = num2 * 0.5f;
			num2 = 0.5f / num2;
			result.X = (matrix.M23 - matrix.M32) * num2;
			result.Y = (matrix.M31 - matrix.M13) * num2;
			result.Z = (matrix.M12 - matrix.M21) * num2;
		}
		else if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
		{
			float num2 = (float)Math.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33);
			float num3 = 0.5f / num2;
			result.X = 0.5f * num2;
			result.Y = (matrix.M12 + matrix.M21) * num3;
			result.Z = (matrix.M13 + matrix.M31) * num3;
			result.W = (matrix.M23 - matrix.M32) * num3;
		}
		else if (matrix.M22 > matrix.M33)
		{
			float num2 = (float)Math.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33);
			float num3 = 0.5f / num2;
			result.X = (matrix.M21 + matrix.M12) * num3;
			result.Y = 0.5f * num2;
			result.Z = (matrix.M32 + matrix.M23) * num3;
			result.W = (matrix.M31 - matrix.M13) * num3;
		}
		else
		{
			float num2 = (float)Math.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22);
			float num3 = 0.5f / num2;
			result.X = (matrix.M31 + matrix.M13) * num3;
			result.Y = (matrix.M32 + matrix.M23) * num3;
			result.Z = 0.5f * num2;
			result.W = (matrix.M12 - matrix.M21) * num3;
		}
	}

	public static void RotationMatrix(ref Matrix3x3 matrix, out Quaternion result)
	{
		float num = matrix.M11 + matrix.M22 + matrix.M33;
		if (num > 0f)
		{
			float num2 = (float)Math.Sqrt(num + 1f);
			result.W = num2 * 0.5f;
			num2 = 0.5f / num2;
			result.X = (matrix.M23 - matrix.M32) * num2;
			result.Y = (matrix.M31 - matrix.M13) * num2;
			result.Z = (matrix.M12 - matrix.M21) * num2;
		}
		else if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
		{
			float num2 = (float)Math.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33);
			float num3 = 0.5f / num2;
			result.X = 0.5f * num2;
			result.Y = (matrix.M12 + matrix.M21) * num3;
			result.Z = (matrix.M13 + matrix.M31) * num3;
			result.W = (matrix.M23 - matrix.M32) * num3;
		}
		else if (matrix.M22 > matrix.M33)
		{
			float num2 = (float)Math.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33);
			float num3 = 0.5f / num2;
			result.X = (matrix.M21 + matrix.M12) * num3;
			result.Y = 0.5f * num2;
			result.Z = (matrix.M32 + matrix.M23) * num3;
			result.W = (matrix.M31 - matrix.M13) * num3;
		}
		else
		{
			float num2 = (float)Math.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22);
			float num3 = 0.5f / num2;
			result.X = (matrix.M31 + matrix.M13) * num3;
			result.Y = (matrix.M32 + matrix.M23) * num3;
			result.Z = 0.5f * num2;
			result.W = (matrix.M12 - matrix.M21) * num3;
		}
	}

	public static void RotationLookAt(ref Vector3 forward, ref Vector3 up, ref Vector3 right, out Quaternion result)
	{
		forward.Normalize();
		up.Normalize();
		right.Normalize();
		Matrix3x3 matrix = default(Matrix3x3);
		matrix.M11 = right.X;
		matrix.M12 = right.Y;
		matrix.M13 = right.Z;
		matrix.M21 = up.X;
		matrix.M22 = up.Y;
		matrix.M23 = up.Z;
		matrix.M31 = forward.X;
		matrix.M32 = forward.Y;
		matrix.M33 = forward.Z;
		RotationMatrix(ref matrix, out result);
	}

	public static Quaternion RotationLookAt(Vector3 forward, Vector3 up, Vector3 right)
	{
		RotationLookAt(ref forward, ref up, ref right, out var result);
		return result;
	}

	public static void RotationLookAtLH(ref Vector3 forward, ref Vector3 up, out Quaternion result)
	{
		Vector3.Cross(ref up, ref forward, out var result2);
		RotationLookAt(ref forward, ref up, ref result2, out result);
	}

	public static Quaternion RotationLookAtLH(Vector3 forward, Vector3 up)
	{
		RotationLookAtLH(ref forward, ref up, out var result);
		return result;
	}

	public static void RotationLookAtRH(ref Vector3 forward, ref Vector3 up, out Quaternion result)
	{
		Vector3.Cross(ref forward, ref up, out var result2);
		RotationLookAt(ref forward, ref up, ref result2, out result);
	}

	public static Quaternion RotationLookAtRH(Vector3 forward, Vector3 up)
	{
		RotationLookAtRH(ref forward, ref up, out var result);
		return result;
	}

	public static void BillboardLH(ref Vector3 objectPosition, ref Vector3 cameraPosition, ref Vector3 cameraUpVector, ref Vector3 cameraForwardVector, out Quaternion result)
	{
		Vector3 right = objectPosition - cameraPosition;
		float a = right.LengthSquared();
		if (MathUtil.IsZero(a))
		{
			right = -cameraForwardVector;
		}
		Vector3.Cross(ref cameraUpVector, ref right, out var result2);
		Vector3.Cross(ref right, ref result2, out var result3);
		RotationLookAt(ref right, ref result3, ref result2, out result);
	}

	public static Quaternion BillboardLH(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3 cameraForwardVector)
	{
		BillboardLH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector, out var result);
		return result;
	}

	public static void BillboardRH(ref Vector3 objectPosition, ref Vector3 cameraPosition, ref Vector3 cameraUpVector, ref Vector3 cameraForwardVector, out Quaternion result)
	{
		Vector3 right = cameraPosition - objectPosition;
		float a = right.LengthSquared();
		if (MathUtil.IsZero(a))
		{
			right = cameraForwardVector;
		}
		Vector3.Cross(ref cameraUpVector, ref right, out var result2);
		Vector3.Cross(ref right, ref result2, out var result3);
		RotationLookAt(ref right, ref result3, ref result2, out result);
	}

	public static Quaternion BillboardRH(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3 cameraForwardVector)
	{
		BillboardRH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector, out var result);
		return result;
	}

	public static Quaternion RotationMatrix(Matrix matrix)
	{
		RotationMatrix(ref matrix, out var result);
		return result;
	}

	public static void RotationYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result)
	{
		float num = roll * 0.5f;
		float num2 = pitch * 0.5f;
		float num3 = yaw * 0.5f;
		float num4 = (float)Math.Sin(num);
		float num5 = (float)Math.Cos(num);
		float num6 = (float)Math.Sin(num2);
		float num7 = (float)Math.Cos(num2);
		float num8 = (float)Math.Sin(num3);
		float num9 = (float)Math.Cos(num3);
		result.X = num9 * num6 * num5 + num8 * num7 * num4;
		result.Y = num8 * num7 * num5 - num9 * num6 * num4;
		result.Z = num9 * num7 * num4 - num8 * num6 * num5;
		result.W = num9 * num7 * num5 + num8 * num6 * num4;
	}

	public static Quaternion RotationYawPitchRoll(float yaw, float pitch, float roll)
	{
		RotationYawPitchRoll(yaw, pitch, roll, out var result);
		return result;
	}

	public static void Slerp(ref Quaternion start, ref Quaternion end, float amount, out Quaternion result)
	{
		float value = Dot(start, end);
		float num;
		float num2;
		if (Math.Abs(value) > 0.999999f)
		{
			num = 1f - amount;
			num2 = amount * (float)Math.Sign(value);
		}
		else
		{
			float num3 = (float)Math.Acos(Math.Abs(value));
			float num4 = (float)(1.0 / Math.Sin(num3));
			num = (float)Math.Sin((1f - amount) * num3) * num4;
			num2 = (float)Math.Sin(amount * num3) * num4 * (float)Math.Sign(value);
		}
		result.X = num * start.X + num2 * end.X;
		result.Y = num * start.Y + num2 * end.Y;
		result.Z = num * start.Z + num2 * end.Z;
		result.W = num * start.W + num2 * end.W;
	}

	public static Quaternion Slerp(Quaternion start, Quaternion end, float amount)
	{
		Slerp(ref start, ref end, amount, out var result);
		return result;
	}

	public static void Squad(ref Quaternion value1, ref Quaternion value2, ref Quaternion value3, ref Quaternion value4, float amount, out Quaternion result)
	{
		Slerp(ref value1, ref value4, amount, out var result2);
		Slerp(ref value2, ref value3, amount, out var result3);
		Slerp(ref result2, ref result3, 2f * amount * (1f - amount), out result);
	}

	public static Quaternion Squad(Quaternion value1, Quaternion value2, Quaternion value3, Quaternion value4, float amount)
	{
		Squad(ref value1, ref value2, ref value3, ref value4, amount, out var result);
		return result;
	}

	public static Quaternion[] SquadSetup(Quaternion value1, Quaternion value2, Quaternion value3, Quaternion value4)
	{
		Quaternion quaternion = (((value1 + value2).LengthSquared() < (value1 - value2).LengthSquared()) ? (-value1) : value1);
		Quaternion value5 = (((value2 + value3).LengthSquared() < (value2 - value3).LengthSquared()) ? (-value3) : value3);
		Quaternion quaternion2 = (((value3 + value4).LengthSquared() < (value3 - value4).LengthSquared()) ? (-value4) : value4);
		Quaternion value6 = value2;
		Exponential(ref value6, out var result);
		Exponential(ref value5, out var result2);
		return new Quaternion[3]
		{
			value6 * Exponential(-0.25f * (Logarithm(result * value5) + Logarithm(result * quaternion))),
			value5 * Exponential(-0.25f * (Logarithm(result2 * quaternion2) + Logarithm(result2 * value6))),
			value5
		};
	}

	public static Quaternion operator +(Quaternion left, Quaternion right)
	{
		Add(ref left, ref right, out var result);
		return result;
	}

	public static Quaternion operator -(Quaternion left, Quaternion right)
	{
		Subtract(ref left, ref right, out var result);
		return result;
	}

	public static Quaternion operator -(Quaternion value)
	{
		Negate(ref value, out var result);
		return result;
	}

	public static Quaternion operator *(float scale, Quaternion value)
	{
		Multiply(ref value, scale, out var result);
		return result;
	}

	public static Quaternion operator *(Quaternion value, float scale)
	{
		Multiply(ref value, scale, out var result);
		return result;
	}

	public static Quaternion operator *(Quaternion left, Quaternion right)
	{
		Multiply(ref left, ref right, out var result);
		return result;
	}

	public static bool operator ==(Quaternion left, Quaternion right)
	{
		return left.Equals(ref right);
	}

	public static bool operator !=(Quaternion left, Quaternion right)
	{
		return !left.Equals(ref right);
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
			return ToString(formatProvider);
		}
		return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
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
			serializer.Writer.Write(X);
			serializer.Writer.Write(Y);
			serializer.Writer.Write(Z);
			serializer.Writer.Write(W);
		}
		else
		{
			X = serializer.Reader.ReadSingle();
			Y = serializer.Reader.ReadSingle();
			Z = serializer.Reader.ReadSingle();
			W = serializer.Reader.ReadSingle();
		}
	}

	public bool Equals(ref Quaternion other)
	{
		if (MathUtil.NearEqual(other.X, X) && MathUtil.NearEqual(other.Y, Y) && MathUtil.NearEqual(other.Z, Z))
		{
			return MathUtil.NearEqual(other.W, W);
		}
		return false;
	}

	public bool Equals(Quaternion other)
	{
		return Equals(ref other);
	}

	public override bool Equals(object value)
	{
		if (!(value is Quaternion other))
		{
			return false;
		}
		return Equals(ref other);
	}

	static Quaternion()
	{
		SizeInBytes = Marshal.SizeOf(typeof(Quaternion));
		Zero = default(Quaternion);
		One = new Quaternion(1f, 1f, 1f, 1f);
		Identity = new Quaternion(0f, 0f, 0f, 1f);
	}
}

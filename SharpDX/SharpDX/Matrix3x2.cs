using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Matrix3x2 : IDataSerializable
{
	public static readonly Matrix3x2 Identity;

	public float M11;

	public float M12;

	public float M21;

	public float M22;

	public float M31;

	public float M32;

	public Vector2 Row1
	{
		get
		{
			return new Vector2(M11, M12);
		}
		set
		{
			M11 = value.X;
			M12 = value.Y;
		}
	}

	public Vector2 Row2
	{
		get
		{
			return new Vector2(M21, M22);
		}
		set
		{
			M21 = value.X;
			M22 = value.Y;
		}
	}

	public Vector2 Row3
	{
		get
		{
			return new Vector2(M31, M32);
		}
		set
		{
			M31 = value.X;
			M32 = value.Y;
		}
	}

	public Vector3 Column1
	{
		get
		{
			return new Vector3(M11, M21, M31);
		}
		set
		{
			M11 = value.X;
			M21 = value.Y;
			M31 = value.Z;
		}
	}

	public Vector3 Column2
	{
		get
		{
			return new Vector3(M12, M22, M32);
		}
		set
		{
			M12 = value.X;
			M22 = value.Y;
			M32 = value.Z;
		}
	}

	public Vector2 TranslationVector
	{
		get
		{
			return new Vector2(M31, M32);
		}
		set
		{
			M31 = value.X;
			M32 = value.Y;
		}
	}

	public Vector2 ScaleVector
	{
		get
		{
			return new Vector2(M11, M22);
		}
		set
		{
			M11 = value.X;
			M22 = value.Y;
		}
	}

	public bool IsIdentity => Equals(Identity);

	public float this[int index]
	{
		get
		{
			return index switch
			{
				0 => M11, 
				1 => M12, 
				2 => M21, 
				3 => M22, 
				4 => M31, 
				5 => M32, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Matrix3x2 run from 0 to 5, inclusive."), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				M11 = value;
				break;
			case 1:
				M12 = value;
				break;
			case 2:
				M21 = value;
				break;
			case 3:
				M22 = value;
				break;
			case 4:
				M31 = value;
				break;
			case 5:
				M32 = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for Matrix3x2 run from 0 to 5, inclusive.");
			}
		}
	}

	public float this[int row, int column]
	{
		get
		{
			if (row < 0 || row > 2)
			{
				throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 2, inclusive.");
			}
			if (column < 0 || column > 1)
			{
				throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 1, inclusive.");
			}
			return this[row * 2 + column];
		}
		set
		{
			if (row < 0 || row > 2)
			{
				throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 2, inclusive.");
			}
			if (column < 0 || column > 1)
			{
				throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 1, inclusive.");
			}
			this[row * 2 + column] = value;
		}
	}

	public Matrix3x2(float value)
	{
		M11 = (M12 = (M21 = (M22 = (M31 = (M32 = value)))));
	}

	public Matrix3x2(float M11, float M12, float M21, float M22, float M31, float M32)
	{
		this.M11 = M11;
		this.M12 = M12;
		this.M21 = M21;
		this.M22 = M22;
		this.M31 = M31;
		this.M32 = M32;
	}

	public Matrix3x2(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 6)
		{
			throw new ArgumentOutOfRangeException("values", "There must be six input values for Matrix3x2.");
		}
		M11 = values[0];
		M12 = values[1];
		M21 = values[2];
		M22 = values[3];
		M31 = values[4];
		M32 = values[5];
	}

	public float[] ToArray()
	{
		return new float[6] { M11, M12, M21, M22, M31, M32 };
	}

	public static void Add(ref Matrix3x2 left, ref Matrix3x2 right, out Matrix3x2 result)
	{
		result.M11 = left.M11 + right.M11;
		result.M12 = left.M12 + right.M12;
		result.M21 = left.M21 + right.M21;
		result.M22 = left.M22 + right.M22;
		result.M31 = left.M31 + right.M31;
		result.M32 = left.M32 + right.M32;
	}

	public static Matrix3x2 Add(Matrix3x2 left, Matrix3x2 right)
	{
		Add(ref left, ref right, out var result);
		return result;
	}

	public static void Subtract(ref Matrix3x2 left, ref Matrix3x2 right, out Matrix3x2 result)
	{
		result.M11 = left.M11 - right.M11;
		result.M12 = left.M12 - right.M12;
		result.M21 = left.M21 - right.M21;
		result.M22 = left.M22 - right.M22;
		result.M31 = left.M31 - right.M31;
		result.M32 = left.M32 - right.M32;
	}

	public static Matrix3x2 Subtract(Matrix3x2 left, Matrix3x2 right)
	{
		Subtract(ref left, ref right, out var result);
		return result;
	}

	public static void Multiply(ref Matrix3x2 left, float right, out Matrix3x2 result)
	{
		result.M11 = left.M11 * right;
		result.M12 = left.M12 * right;
		result.M21 = left.M21 * right;
		result.M22 = left.M22 * right;
		result.M31 = left.M31 * right;
		result.M32 = left.M32 * right;
	}

	public static Matrix3x2 Multiply(Matrix3x2 left, float right)
	{
		Multiply(ref left, right, out var result);
		return result;
	}

	public static void Multiply(ref Matrix3x2 left, ref Matrix3x2 right, out Matrix3x2 result)
	{
		result = default(Matrix3x2);
		result.M11 = left.M11 * right.M11 + left.M12 * right.M21;
		result.M12 = left.M11 * right.M12 + left.M12 * right.M22;
		result.M21 = left.M21 * right.M11 + left.M22 * right.M21;
		result.M22 = left.M21 * right.M12 + left.M22 * right.M22;
		result.M31 = left.M31 * right.M11 + left.M32 * right.M21 + right.M31;
		result.M32 = left.M31 * right.M12 + left.M32 * right.M22 + right.M32;
	}

	public static Matrix3x2 Multiply(Matrix3x2 left, Matrix3x2 right)
	{
		Multiply(ref left, ref right, out var result);
		return result;
	}

	public static void Divide(ref Matrix3x2 left, float right, out Matrix3x2 result)
	{
		float num = 1f / right;
		result.M11 = left.M11 * num;
		result.M12 = left.M12 * num;
		result.M21 = left.M21 * num;
		result.M22 = left.M22 * num;
		result.M31 = left.M31 * num;
		result.M32 = left.M32 * num;
	}

	public static void Divide(ref Matrix3x2 left, ref Matrix3x2 right, out Matrix3x2 result)
	{
		result.M11 = left.M11 / right.M11;
		result.M12 = left.M12 / right.M12;
		result.M21 = left.M21 / right.M21;
		result.M22 = left.M22 / right.M22;
		result.M31 = left.M31 / right.M31;
		result.M32 = left.M32 / right.M32;
	}

	public static void Negate(ref Matrix3x2 value, out Matrix3x2 result)
	{
		result.M11 = 0f - value.M11;
		result.M12 = 0f - value.M12;
		result.M21 = 0f - value.M21;
		result.M22 = 0f - value.M22;
		result.M31 = 0f - value.M31;
		result.M32 = 0f - value.M32;
	}

	public static Matrix3x2 Negate(Matrix3x2 value)
	{
		Negate(ref value, out var result);
		return result;
	}

	public static void Lerp(ref Matrix3x2 start, ref Matrix3x2 end, float amount, out Matrix3x2 result)
	{
		result.M11 = MathUtil.Lerp(start.M11, end.M11, amount);
		result.M12 = MathUtil.Lerp(start.M12, end.M12, amount);
		result.M21 = MathUtil.Lerp(start.M21, end.M21, amount);
		result.M22 = MathUtil.Lerp(start.M22, end.M22, amount);
		result.M31 = MathUtil.Lerp(start.M31, end.M31, amount);
		result.M32 = MathUtil.Lerp(start.M32, end.M32, amount);
	}

	public static Matrix3x2 Lerp(Matrix3x2 start, Matrix3x2 end, float amount)
	{
		Lerp(ref start, ref end, amount, out var result);
		return result;
	}

	public static void SmoothStep(ref Matrix3x2 start, ref Matrix3x2 end, float amount, out Matrix3x2 result)
	{
		amount = MathUtil.SmoothStep(amount);
		Lerp(ref start, ref end, amount, out result);
	}

	public static Matrix3x2 SmoothStep(Matrix3x2 start, Matrix3x2 end, float amount)
	{
		SmoothStep(ref start, ref end, amount, out var result);
		return result;
	}

	public static void Scaling(ref Vector2 scale, out Matrix3x2 result)
	{
		Scaling(scale.X, scale.Y, out result);
	}

	public static Matrix3x2 Scaling(Vector2 scale)
	{
		Scaling(ref scale, out var result);
		return result;
	}

	public static void Scaling(float x, float y, out Matrix3x2 result)
	{
		result = Identity;
		result.M11 = x;
		result.M22 = y;
	}

	public static Matrix3x2 Scaling(float x, float y)
	{
		Scaling(x, y, out var result);
		return result;
	}

	public static void Scaling(float scale, out Matrix3x2 result)
	{
		result = Identity;
		result.M11 = (result.M22 = scale);
	}

	public static Matrix3x2 Scaling(float scale)
	{
		Scaling(scale, out var result);
		return result;
	}

	public static Matrix3x2 Scaling(float x, float y, Vector2 center)
	{
		Matrix3x2 result = default(Matrix3x2);
		result.M11 = x;
		result.M12 = 0f;
		result.M21 = 0f;
		result.M22 = y;
		result.M31 = center.X - x * center.X;
		result.M32 = center.Y - y * center.Y;
		return result;
	}

	public static void Scaling(float x, float y, ref Vector2 center, out Matrix3x2 result)
	{
		Matrix3x2 matrix3x = default(Matrix3x2);
		matrix3x.M11 = x;
		matrix3x.M12 = 0f;
		matrix3x.M21 = 0f;
		matrix3x.M22 = y;
		matrix3x.M31 = center.X - x * center.X;
		matrix3x.M32 = center.Y - y * center.Y;
		result = matrix3x;
	}

	public float Determinant()
	{
		return M11 * M22 - M12 * M21;
	}

	public static void Rotation(float angle, out Matrix3x2 result)
	{
		float num = (float)Math.Cos(angle);
		float num2 = (float)Math.Sin(angle);
		result = Identity;
		result.M11 = num;
		result.M12 = num2;
		result.M21 = 0f - num2;
		result.M22 = num;
	}

	public static Matrix3x2 Rotation(float angle)
	{
		Rotation(angle, out var result);
		return result;
	}

	public static Matrix3x2 Rotation(float angle, Vector2 center)
	{
		Rotation(angle, center, out var result);
		return result;
	}

	public static void Rotation(float angle, Vector2 center, out Matrix3x2 result)
	{
		result = Translation(-center) * Rotation(angle) * Translation(center);
	}

	public static void Transformation(float xScale, float yScale, float angle, float xOffset, float yOffset, out Matrix3x2 result)
	{
		result = Scaling(xScale, yScale) * Rotation(angle) * Translation(xOffset, yOffset);
	}

	public static Matrix3x2 Transformation(float xScale, float yScale, float angle, float xOffset, float yOffset)
	{
		Transformation(xScale, yScale, angle, xOffset, yOffset, out var result);
		return result;
	}

	public static void Translation(ref Vector2 value, out Matrix3x2 result)
	{
		Translation(value.X, value.Y, out result);
	}

	public static Matrix3x2 Translation(Vector2 value)
	{
		Translation(ref value, out var result);
		return result;
	}

	public static void Translation(float x, float y, out Matrix3x2 result)
	{
		result = Identity;
		result.M31 = x;
		result.M32 = y;
	}

	public static Matrix3x2 Translation(float x, float y)
	{
		Translation(x, y, out var result);
		return result;
	}

	public static Vector2 TransformPoint(Matrix3x2 matrix, Vector2 point)
	{
		Vector2 result = default(Vector2);
		result.X = point.X * matrix.M11 + point.Y * matrix.M21 + matrix.M31;
		result.Y = point.X * matrix.M12 + point.Y * matrix.M22 + matrix.M32;
		return result;
	}

	public static void TransformPoint(ref Matrix3x2 matrix, ref Vector2 point, out Vector2 result)
	{
		Vector2 vector = default(Vector2);
		vector.X = point.X * matrix.M11 + point.Y * matrix.M21 + matrix.M31;
		vector.Y = point.X * matrix.M12 + point.Y * matrix.M22 + matrix.M32;
		result = vector;
	}

	public void Invert()
	{
		Invert(ref this, out this);
	}

	public static Matrix3x2 Invert(Matrix3x2 value)
	{
		Invert(ref value, out var result);
		return result;
	}

	public static Matrix3x2 Skew(float angleX, float angleY)
	{
		Skew(angleX, angleY, out var result);
		return result;
	}

	public static void Skew(float angleX, float angleY, out Matrix3x2 result)
	{
		result = Matrix.Identity;
		result.M12 = (float)Math.Tan(angleX);
		result.M21 = (float)Math.Tan(angleY);
	}

	public static void Invert(ref Matrix3x2 value, out Matrix3x2 result)
	{
		float num = value.Determinant();
		if (MathUtil.IsZero(num))
		{
			result = Identity;
			return;
		}
		float num2 = 1f / num;
		float m = value.M31;
		float m2 = value.M32;
		result = new Matrix3x2(value.M22 * num2, (0f - value.M12) * num2, (0f - value.M21) * num2, value.M11 * num2, (value.M21 * m2 - m * value.M22) * num2, (m * value.M12 - value.M11 * m2) * num2);
	}

	public static Matrix3x2 operator +(Matrix3x2 left, Matrix3x2 right)
	{
		Add(ref left, ref right, out var result);
		return result;
	}

	public static Matrix3x2 operator +(Matrix3x2 value)
	{
		return value;
	}

	public static Matrix3x2 operator -(Matrix3x2 left, Matrix3x2 right)
	{
		Subtract(ref left, ref right, out var result);
		return result;
	}

	public static Matrix3x2 operator -(Matrix3x2 value)
	{
		Negate(ref value, out var result);
		return result;
	}

	public static Matrix3x2 operator *(float left, Matrix3x2 right)
	{
		Multiply(ref right, left, out var result);
		return result;
	}

	public static Matrix3x2 operator *(Matrix3x2 left, float right)
	{
		Multiply(ref left, right, out var result);
		return result;
	}

	public static Matrix3x2 operator *(Matrix3x2 left, Matrix3x2 right)
	{
		Multiply(ref left, ref right, out var result);
		return result;
	}

	public static Matrix3x2 operator /(Matrix3x2 left, float right)
	{
		Divide(ref left, right, out var result);
		return result;
	}

	public static Matrix3x2 operator /(Matrix3x2 left, Matrix3x2 right)
	{
		Divide(ref left, ref right, out var result);
		return result;
	}

	public static bool operator ==(Matrix3x2 left, Matrix3x2 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Matrix3x2 left, Matrix3x2 right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "[M11:{0} M12:{1}] [M21:{2} M22:{3}] [M31:{4} M32:{5}]", M11, M12, M21, M22, M31, M32);
	}

	public string ToString(string format)
	{
		if (format == null)
		{
			return ToString();
		}
		return string.Format(format, CultureInfo.CurrentCulture, "[M11:{0} M12:{1}] [M21:{2} M22:{3}] [M31:{4} M32:{5}]", M11.ToString(format, CultureInfo.CurrentCulture), M12.ToString(format, CultureInfo.CurrentCulture), M21.ToString(format, CultureInfo.CurrentCulture), M22.ToString(format, CultureInfo.CurrentCulture), M31.ToString(format, CultureInfo.CurrentCulture), M32.ToString(format, CultureInfo.CurrentCulture));
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "[M11:{0} M12:{1}] [M21:{2} M22:{3}] [M31:{4} M32:{5}]", M11.ToString(formatProvider), M12.ToString(formatProvider), M21.ToString(formatProvider), M22.ToString(formatProvider), M31.ToString(formatProvider), M32.ToString(formatProvider));
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			return ToString(formatProvider);
		}
		return string.Format(format, formatProvider, "[M11:{0} M12:{1}] [M21:{2} M22:{3}] [M31:{4} M32:{5}]", M11.ToString(format, formatProvider), M12.ToString(format, formatProvider), M21.ToString(format, formatProvider), M22.ToString(format, formatProvider), M31.ToString(format, formatProvider), M32.ToString(format, formatProvider));
	}

	public override int GetHashCode()
	{
		int hashCode = M11.GetHashCode();
		hashCode = (hashCode * 397) ^ M12.GetHashCode();
		hashCode = (hashCode * 397) ^ M21.GetHashCode();
		hashCode = (hashCode * 397) ^ M22.GetHashCode();
		hashCode = (hashCode * 397) ^ M31.GetHashCode();
		return (hashCode * 397) ^ M32.GetHashCode();
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(M11);
			serializer.Writer.Write(M12);
			serializer.Writer.Write(M21);
			serializer.Writer.Write(M22);
			serializer.Writer.Write(M31);
			serializer.Writer.Write(M32);
		}
		else
		{
			M11 = serializer.Reader.ReadSingle();
			M12 = serializer.Reader.ReadSingle();
			M21 = serializer.Reader.ReadSingle();
			M22 = serializer.Reader.ReadSingle();
			M31 = serializer.Reader.ReadSingle();
			M32 = serializer.Reader.ReadSingle();
		}
	}

	public bool Equals(Matrix3x2 other)
	{
		if (MathUtil.NearEqual(other.M11, M11) && MathUtil.NearEqual(other.M12, M12) && MathUtil.NearEqual(other.M21, M21) && MathUtil.NearEqual(other.M22, M22) && MathUtil.NearEqual(other.M31, M31))
		{
			return MathUtil.NearEqual(other.M32, M32);
		}
		return false;
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(value.GetType(), typeof(Matrix3x2)))
		{
			return false;
		}
		return Equals((Matrix3x2)value);
	}

	public static implicit operator Matrix3x2(Matrix matrix)
	{
		return new Matrix3x2
		{
			M11 = matrix.M11,
			M12 = matrix.M12,
			M21 = matrix.M21,
			M22 = matrix.M22,
			M31 = matrix.M41,
			M32 = matrix.M42
		};
	}

	static Matrix3x2()
	{
		Identity = new Matrix3x2(1f, 0f, 0f, 1f, 0f, 0f);
	}
}

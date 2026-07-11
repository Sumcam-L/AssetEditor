using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DynamicSerializer("TKM3")]
public struct Matrix3x3 : IEquatable<Matrix3x3>, IFormattable, IDataSerializable
{
	public static readonly int SizeInBytes;

	public static readonly Matrix3x3 Zero;

	public static readonly Matrix3x3 Identity;

	public float M11;

	public float M12;

	public float M13;

	public float M21;

	public float M22;

	public float M23;

	public float M31;

	public float M32;

	public float M33;

	public Vector3 Row1
	{
		get
		{
			return new Vector3(M11, M12, M13);
		}
		set
		{
			M11 = value.X;
			M12 = value.Y;
			M13 = value.Z;
		}
	}

	public Vector3 Row2
	{
		get
		{
			return new Vector3(M21, M22, M23);
		}
		set
		{
			M21 = value.X;
			M22 = value.Y;
			M23 = value.Z;
		}
	}

	public Vector3 Row3
	{
		get
		{
			return new Vector3(M31, M32, M33);
		}
		set
		{
			M31 = value.X;
			M32 = value.Y;
			M33 = value.Z;
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

	public Vector3 Column3
	{
		get
		{
			return new Vector3(M13, M23, M33);
		}
		set
		{
			M13 = value.X;
			M23 = value.Y;
			M33 = value.Z;
		}
	}

	public Vector3 ScaleVector
	{
		get
		{
			return new Vector3(M11, M22, M33);
		}
		set
		{
			M11 = value.X;
			M22 = value.Y;
			M33 = value.Z;
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
				2 => M13, 
				4 => M21, 
				5 => M22, 
				6 => M23, 
				8 => M31, 
				9 => M32, 
				10 => M33, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Matrix3x3 run from 0 to 8, inclusive."), 
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
				M13 = value;
				break;
			case 4:
				M21 = value;
				break;
			case 5:
				M22 = value;
				break;
			case 6:
				M23 = value;
				break;
			case 8:
				M31 = value;
				break;
			case 9:
				M32 = value;
				break;
			case 10:
				M33 = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for Matrix3x3 run from 0 to 8, inclusive.");
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
			if (column < 0 || column > 2)
			{
				throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 2, inclusive.");
			}
			return this[row * 3 + column];
		}
		set
		{
			if (row < 0 || row > 2)
			{
				throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 2, inclusive.");
			}
			if (column < 0 || column > 2)
			{
				throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 2, inclusive.");
			}
			this[row * 3 + column] = value;
		}
	}

	public Matrix3x3(float value)
	{
		M11 = (M12 = (M13 = (M21 = (M22 = (M23 = (M31 = (M32 = (M33 = value))))))));
	}

	public Matrix3x3(float M11, float M12, float M13, float M21, float M22, float M23, float M31, float M32, float M33)
	{
		this.M11 = M11;
		this.M12 = M12;
		this.M13 = M13;
		this.M21 = M21;
		this.M22 = M22;
		this.M23 = M23;
		this.M31 = M31;
		this.M32 = M32;
		this.M33 = M33;
	}

	public Matrix3x3(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 9)
		{
			throw new ArgumentOutOfRangeException("values", "There must be sixteen and only sixteen input values for Matrix3x3.");
		}
		M11 = values[0];
		M12 = values[1];
		M13 = values[2];
		M21 = values[3];
		M22 = values[4];
		M23 = values[5];
		M31 = values[6];
		M32 = values[7];
		M33 = values[8];
	}

	public float Determinant()
	{
		return M11 * M22 * M33 + M12 * M23 * M31 + M13 * M21 * M32 - M13 * M22 * M31 - M12 * M21 * M33 - M11 * M23 * M32;
	}

	public void Invert()
	{
		Invert(ref this, out this);
	}

	public void Transpose()
	{
		Transpose(ref this, out this);
	}

	public void Orthogonalize()
	{
		Orthogonalize(ref this, out this);
	}

	public void Orthonormalize()
	{
		Orthonormalize(ref this, out this);
	}

	public void DecomposeQR(out Matrix3x3 Q, out Matrix3x3 R)
	{
		Matrix3x3 value = this;
		value.Transpose();
		Orthonormalize(ref value, out Q);
		Q.Transpose();
		R = default(Matrix3x3);
		R.M11 = Vector3.Dot(Q.Column1, Column1);
		R.M12 = Vector3.Dot(Q.Column1, Column2);
		R.M13 = Vector3.Dot(Q.Column1, Column3);
		R.M22 = Vector3.Dot(Q.Column2, Column2);
		R.M23 = Vector3.Dot(Q.Column2, Column3);
		R.M33 = Vector3.Dot(Q.Column3, Column3);
	}

	public void DecomposeLQ(out Matrix3x3 L, out Matrix3x3 Q)
	{
		Orthonormalize(ref this, out Q);
		L = default(Matrix3x3);
		L.M11 = Vector3.Dot(Q.Row1, Row1);
		L.M21 = Vector3.Dot(Q.Row1, Row2);
		L.M22 = Vector3.Dot(Q.Row2, Row2);
		L.M31 = Vector3.Dot(Q.Row1, Row3);
		L.M32 = Vector3.Dot(Q.Row2, Row3);
		L.M33 = Vector3.Dot(Q.Row3, Row3);
	}

	public bool Decompose(out Vector3 scale, out Quaternion rotation)
	{
		scale.X = (float)Math.Sqrt(M11 * M11 + M12 * M12 + M13 * M13);
		scale.Y = (float)Math.Sqrt(M21 * M21 + M22 * M22 + M23 * M23);
		scale.Z = (float)Math.Sqrt(M31 * M31 + M32 * M32 + M33 * M33);
		if (MathUtil.IsZero(scale.X) || MathUtil.IsZero(scale.Y) || MathUtil.IsZero(scale.Z))
		{
			rotation = Quaternion.Identity;
			return false;
		}
		Matrix3x3 matrix = new Matrix3x3
		{
			M11 = M11 / scale.X,
			M12 = M12 / scale.X,
			M13 = M13 / scale.X,
			M21 = M21 / scale.Y,
			M22 = M22 / scale.Y,
			M23 = M23 / scale.Y,
			M31 = M31 / scale.Z,
			M32 = M32 / scale.Z,
			M33 = M33 / scale.Z
		};
		Quaternion.RotationMatrix(ref matrix, out rotation);
		return true;
	}

	public bool DecomposeUniformScale(out float scale, out Quaternion rotation)
	{
		scale = (float)Math.Sqrt(M11 * M11 + M12 * M12 + M13 * M13);
		float num = 1f / scale;
		if (Math.Abs(scale) < 1E-06f)
		{
			rotation = Quaternion.Identity;
			return false;
		}
		Matrix3x3 matrix = new Matrix3x3
		{
			M11 = M11 * num,
			M12 = M12 * num,
			M13 = M13 * num,
			M21 = M21 * num,
			M22 = M22 * num,
			M23 = M23 * num,
			M31 = M31 * num,
			M32 = M32 * num,
			M33 = M33 * num
		};
		Quaternion.RotationMatrix(ref matrix, out rotation);
		return true;
	}

	public void ExchangeRows(int firstRow, int secondRow)
	{
		if (firstRow < 0)
		{
			throw new ArgumentOutOfRangeException("firstRow", "The parameter firstRow must be greater than or equal to zero.");
		}
		if (firstRow > 2)
		{
			throw new ArgumentOutOfRangeException("firstRow", "The parameter firstRow must be less than or equal to two.");
		}
		if (secondRow < 0)
		{
			throw new ArgumentOutOfRangeException("secondRow", "The parameter secondRow must be greater than or equal to zero.");
		}
		if (secondRow > 2)
		{
			throw new ArgumentOutOfRangeException("secondRow", "The parameter secondRow must be less than or equal to two.");
		}
		if (firstRow != secondRow)
		{
			float value = this[secondRow, 0];
			float value2 = this[secondRow, 1];
			float value3 = this[secondRow, 2];
			this[secondRow, 0] = this[firstRow, 0];
			this[secondRow, 1] = this[firstRow, 1];
			this[secondRow, 2] = this[firstRow, 2];
			this[firstRow, 0] = value;
			this[firstRow, 1] = value2;
			this[firstRow, 2] = value3;
		}
	}

	public void ExchangeColumns(int firstColumn, int secondColumn)
	{
		if (firstColumn < 0)
		{
			throw new ArgumentOutOfRangeException("firstColumn", "The parameter firstColumn must be greater than or equal to zero.");
		}
		if (firstColumn > 2)
		{
			throw new ArgumentOutOfRangeException("firstColumn", "The parameter firstColumn must be less than or equal to two.");
		}
		if (secondColumn < 0)
		{
			throw new ArgumentOutOfRangeException("secondColumn", "The parameter secondColumn must be greater than or equal to zero.");
		}
		if (secondColumn > 2)
		{
			throw new ArgumentOutOfRangeException("secondColumn", "The parameter secondColumn must be less than or equal to two.");
		}
		if (firstColumn != secondColumn)
		{
			float value = this[0, secondColumn];
			float value2 = this[1, secondColumn];
			float value3 = this[2, secondColumn];
			this[0, secondColumn] = this[0, firstColumn];
			this[1, secondColumn] = this[1, firstColumn];
			this[2, secondColumn] = this[2, firstColumn];
			this[0, firstColumn] = value;
			this[1, firstColumn] = value2;
			this[2, firstColumn] = value3;
		}
	}

	public float[] ToArray()
	{
		return new float[9] { M11, M12, M13, M21, M22, M23, M31, M32, M33 };
	}

	public static void Add(ref Matrix3x3 left, ref Matrix3x3 right, out Matrix3x3 result)
	{
		result.M11 = left.M11 + right.M11;
		result.M12 = left.M12 + right.M12;
		result.M13 = left.M13 + right.M13;
		result.M21 = left.M21 + right.M21;
		result.M22 = left.M22 + right.M22;
		result.M23 = left.M23 + right.M23;
		result.M31 = left.M31 + right.M31;
		result.M32 = left.M32 + right.M32;
		result.M33 = left.M33 + right.M33;
	}

	public static Matrix3x3 Add(Matrix3x3 left, Matrix3x3 right)
	{
		Add(ref left, ref right, out var result);
		return result;
	}

	public static void Subtract(ref Matrix3x3 left, ref Matrix3x3 right, out Matrix3x3 result)
	{
		result.M11 = left.M11 - right.M11;
		result.M12 = left.M12 - right.M12;
		result.M13 = left.M13 - right.M13;
		result.M21 = left.M21 - right.M21;
		result.M22 = left.M22 - right.M22;
		result.M23 = left.M23 - right.M23;
		result.M31 = left.M31 - right.M31;
		result.M32 = left.M32 - right.M32;
		result.M33 = left.M33 - right.M33;
	}

	public static Matrix3x3 Subtract(Matrix3x3 left, Matrix3x3 right)
	{
		Subtract(ref left, ref right, out var result);
		return result;
	}

	public static void Multiply(ref Matrix3x3 left, float right, out Matrix3x3 result)
	{
		result.M11 = left.M11 * right;
		result.M12 = left.M12 * right;
		result.M13 = left.M13 * right;
		result.M21 = left.M21 * right;
		result.M22 = left.M22 * right;
		result.M23 = left.M23 * right;
		result.M31 = left.M31 * right;
		result.M32 = left.M32 * right;
		result.M33 = left.M33 * right;
	}

	public static Matrix3x3 Multiply(Matrix3x3 left, float right)
	{
		Multiply(ref left, right, out var result);
		return result;
	}

	public static void Multiply(ref Matrix3x3 left, ref Matrix3x3 right, out Matrix3x3 result)
	{
		result = new Matrix3x3
		{
			M11 = left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31,
			M12 = left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32,
			M13 = left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33,
			M21 = left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31,
			M22 = left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32,
			M23 = left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33,
			M31 = left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31,
			M32 = left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32,
			M33 = left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33
		};
	}

	public static Matrix3x3 Multiply(Matrix3x3 left, Matrix3x3 right)
	{
		Multiply(ref left, ref right, out var result);
		return result;
	}

	public static void Divide(ref Matrix3x3 left, float right, out Matrix3x3 result)
	{
		float num = 1f / right;
		result.M11 = left.M11 * num;
		result.M12 = left.M12 * num;
		result.M13 = left.M13 * num;
		result.M21 = left.M21 * num;
		result.M22 = left.M22 * num;
		result.M23 = left.M23 * num;
		result.M31 = left.M31 * num;
		result.M32 = left.M32 * num;
		result.M33 = left.M33 * num;
	}

	public static Matrix3x3 Divide(Matrix3x3 left, float right)
	{
		Divide(ref left, right, out var result);
		return result;
	}

	public static void Divide(ref Matrix3x3 left, ref Matrix3x3 right, out Matrix3x3 result)
	{
		result.M11 = left.M11 / right.M11;
		result.M12 = left.M12 / right.M12;
		result.M13 = left.M13 / right.M13;
		result.M21 = left.M21 / right.M21;
		result.M22 = left.M22 / right.M22;
		result.M23 = left.M23 / right.M23;
		result.M31 = left.M31 / right.M31;
		result.M32 = left.M32 / right.M32;
		result.M33 = left.M33 / right.M33;
	}

	public static Matrix3x3 Divide(Matrix3x3 left, Matrix3x3 right)
	{
		Divide(ref left, ref right, out var result);
		return result;
	}

	public static void Exponent(ref Matrix3x3 value, int exponent, out Matrix3x3 result)
	{
		if (exponent < 0)
		{
			throw new ArgumentOutOfRangeException("exponent", "The exponent can not be negative.");
		}
		switch (exponent)
		{
		case 0:
			result = Identity;
			return;
		case 1:
			result = value;
			return;
		}
		Matrix3x3 identity = Identity;
		Matrix3x3 matrix3x = value;
		while (true)
		{
			if ((exponent & 1) != 0)
			{
				identity *= matrix3x;
			}
			exponent /= 2;
			if (exponent <= 0)
			{
				break;
			}
			matrix3x *= matrix3x;
		}
		result = identity;
	}

	public static Matrix3x3 Exponent(Matrix3x3 value, int exponent)
	{
		Exponent(ref value, exponent, out var result);
		return result;
	}

	public static void Negate(ref Matrix3x3 value, out Matrix3x3 result)
	{
		result.M11 = 0f - value.M11;
		result.M12 = 0f - value.M12;
		result.M13 = 0f - value.M13;
		result.M21 = 0f - value.M21;
		result.M22 = 0f - value.M22;
		result.M23 = 0f - value.M23;
		result.M31 = 0f - value.M31;
		result.M32 = 0f - value.M32;
		result.M33 = 0f - value.M33;
	}

	public static Matrix3x3 Negate(Matrix3x3 value)
	{
		Negate(ref value, out var result);
		return result;
	}

	public static void Lerp(ref Matrix3x3 start, ref Matrix3x3 end, float amount, out Matrix3x3 result)
	{
		result.M11 = MathUtil.Lerp(start.M11, end.M11, amount);
		result.M12 = MathUtil.Lerp(start.M12, end.M12, amount);
		result.M13 = MathUtil.Lerp(start.M13, end.M13, amount);
		result.M21 = MathUtil.Lerp(start.M21, end.M21, amount);
		result.M22 = MathUtil.Lerp(start.M22, end.M22, amount);
		result.M23 = MathUtil.Lerp(start.M23, end.M23, amount);
		result.M31 = MathUtil.Lerp(start.M31, end.M31, amount);
		result.M32 = MathUtil.Lerp(start.M32, end.M32, amount);
		result.M33 = MathUtil.Lerp(start.M33, end.M33, amount);
	}

	public static Matrix3x3 Lerp(Matrix3x3 start, Matrix3x3 end, float amount)
	{
		Lerp(ref start, ref end, amount, out var result);
		return result;
	}

	public static void SmoothStep(ref Matrix3x3 start, ref Matrix3x3 end, float amount, out Matrix3x3 result)
	{
		amount = MathUtil.SmoothStep(amount);
		Lerp(ref start, ref end, amount, out result);
	}

	public static Matrix3x3 SmoothStep(Matrix3x3 start, Matrix3x3 end, float amount)
	{
		SmoothStep(ref start, ref end, amount, out var result);
		return result;
	}

	public static void Transpose(ref Matrix3x3 value, out Matrix3x3 result)
	{
		result = new Matrix3x3
		{
			M11 = value.M11,
			M12 = value.M21,
			M13 = value.M31,
			M21 = value.M12,
			M22 = value.M22,
			M23 = value.M32,
			M31 = value.M13,
			M32 = value.M23,
			M33 = value.M33
		};
	}

	public static void TransposeByRef(ref Matrix3x3 value, ref Matrix3x3 result)
	{
		result.M11 = value.M11;
		result.M12 = value.M21;
		result.M13 = value.M31;
		result.M21 = value.M12;
		result.M22 = value.M22;
		result.M23 = value.M32;
		result.M31 = value.M13;
		result.M32 = value.M23;
		result.M33 = value.M33;
	}

	public static Matrix3x3 Transpose(Matrix3x3 value)
	{
		Transpose(ref value, out var result);
		return result;
	}

	public static void Invert(ref Matrix3x3 value, out Matrix3x3 result)
	{
		float num = value.M22 * value.M33 + value.M23 * (0f - value.M32);
		float num2 = value.M21 * value.M33 + value.M23 * (0f - value.M31);
		float num3 = value.M21 * value.M32 + value.M22 * (0f - value.M31);
		float num4 = value.M11 * num - value.M12 * num2 + value.M13 * num3;
		if (Math.Abs(num4) == 0f)
		{
			result = Zero;
			return;
		}
		num4 = 1f / num4;
		float num5 = value.M12 * value.M33 + value.M13 * (0f - value.M32);
		float num6 = value.M11 * value.M33 + value.M13 * (0f - value.M31);
		float num7 = value.M11 * value.M32 + value.M12 * (0f - value.M31);
		float num8 = value.M12 * value.M23 - value.M13 * value.M22;
		float num9 = value.M11 * value.M23 - value.M13 * value.M21;
		float num10 = value.M11 * value.M22 - value.M12 * value.M21;
		result.M11 = num * num4;
		result.M12 = (0f - num5) * num4;
		result.M13 = num8 * num4;
		result.M21 = (0f - num2) * num4;
		result.M22 = num6 * num4;
		result.M23 = (0f - num9) * num4;
		result.M31 = num3 * num4;
		result.M32 = (0f - num7) * num4;
		result.M33 = num10 * num4;
	}

	public static Matrix3x3 Invert(Matrix3x3 value)
	{
		value.Invert();
		return value;
	}

	public static void Orthogonalize(ref Matrix3x3 value, out Matrix3x3 result)
	{
		result = value;
		result.Row2 -= Vector3.Dot(result.Row1, result.Row2) / Vector3.Dot(result.Row1, result.Row1) * result.Row1;
		result.Row3 -= Vector3.Dot(result.Row1, result.Row3) / Vector3.Dot(result.Row1, result.Row1) * result.Row1;
		result.Row3 -= Vector3.Dot(result.Row2, result.Row3) / Vector3.Dot(result.Row2, result.Row2) * result.Row2;
	}

	public static Matrix3x3 Orthogonalize(Matrix3x3 value)
	{
		Orthogonalize(ref value, out var result);
		return result;
	}

	public static void Orthonormalize(ref Matrix3x3 value, out Matrix3x3 result)
	{
		result = value;
		result.Row1 = Vector3.Normalize(result.Row1);
		result.Row2 -= Vector3.Dot(result.Row1, result.Row2) * result.Row1;
		result.Row2 = Vector3.Normalize(result.Row2);
		result.Row3 -= Vector3.Dot(result.Row1, result.Row3) * result.Row1;
		result.Row3 -= Vector3.Dot(result.Row2, result.Row3) * result.Row2;
		result.Row3 = Vector3.Normalize(result.Row3);
	}

	public static Matrix3x3 Orthonormalize(Matrix3x3 value)
	{
		Orthonormalize(ref value, out var result);
		return result;
	}

	public static void UpperTriangularForm(ref Matrix3x3 value, out Matrix3x3 result)
	{
		result = value;
		int num = 0;
		int num2 = 3;
		int num3 = 3;
		for (int i = 0; i < num2; i++)
		{
			if (num3 <= num)
			{
				break;
			}
			int j = i;
			while (MathUtil.IsZero(result[j, num]))
			{
				j++;
				if (j == num2)
				{
					j = i;
					num++;
					if (num == num3)
					{
						return;
					}
				}
			}
			if (j != i)
			{
				result.ExchangeRows(j, i);
			}
			float num4 = 1f / result[i, num];
			for (; j < num2; j++)
			{
				if (j != i)
				{
					result[j, 0] -= result[i, 0] * num4 * result[j, num];
					result[j, 1] -= result[i, 1] * num4 * result[j, num];
					result[j, 2] -= result[i, 2] * num4 * result[j, num];
				}
			}
			num++;
		}
	}

	public static Matrix3x3 UpperTriangularForm(Matrix3x3 value)
	{
		UpperTriangularForm(ref value, out var result);
		return result;
	}

	public static void LowerTriangularForm(ref Matrix3x3 value, out Matrix3x3 result)
	{
		Matrix3x3 value2 = value;
		Transpose(ref value2, out result);
		int num = 0;
		int num2 = 3;
		int num3 = 3;
		for (int i = 0; i < num2; i++)
		{
			if (num3 <= num)
			{
				return;
			}
			int j = i;
			while (MathUtil.IsZero(result[j, num]))
			{
				j++;
				if (j == num2)
				{
					j = i;
					num++;
					if (num == num3)
					{
						return;
					}
				}
			}
			if (j != i)
			{
				result.ExchangeRows(j, i);
			}
			float num4 = 1f / result[i, num];
			for (; j < num2; j++)
			{
				if (j != i)
				{
					result[j, 0] -= result[i, 0] * num4 * result[j, num];
					result[j, 1] -= result[i, 1] * num4 * result[j, num];
					result[j, 2] -= result[i, 2] * num4 * result[j, num];
				}
			}
			num++;
		}
		Transpose(ref result, out result);
	}

	public static Matrix3x3 LowerTriangularForm(Matrix3x3 value)
	{
		LowerTriangularForm(ref value, out var result);
		return result;
	}

	public static void RowEchelonForm(ref Matrix3x3 value, out Matrix3x3 result)
	{
		result = value;
		int num = 0;
		int num2 = 3;
		int num3 = 3;
		for (int i = 0; i < num2; i++)
		{
			if (num3 <= num)
			{
				break;
			}
			int j = i;
			while (MathUtil.IsZero(result[j, num]))
			{
				j++;
				if (j == num2)
				{
					j = i;
					num++;
					if (num == num3)
					{
						return;
					}
				}
			}
			if (j != i)
			{
				result.ExchangeRows(j, i);
			}
			float num4 = 1f / result[i, num];
			result[i, 0] *= num4;
			result[i, 1] *= num4;
			result[i, 2] *= num4;
			for (; j < num2; j++)
			{
				if (j != i)
				{
					result[j, 0] -= result[i, 0] * result[j, num];
					result[j, 1] -= result[i, 1] * result[j, num];
					result[j, 2] -= result[i, 2] * result[j, num];
				}
			}
			num++;
		}
	}

	public static Matrix3x3 RowEchelonForm(Matrix3x3 value)
	{
		RowEchelonForm(ref value, out var result);
		return result;
	}

	public static void Billboard(ref Vector3 objectPosition, ref Vector3 cameraPosition, ref Vector3 cameraUpVector, ref Vector3 cameraForwardVector, out Matrix3x3 result)
	{
		Vector3 right = objectPosition - cameraPosition;
		float num = right.LengthSquared();
		if (MathUtil.IsZero(num))
		{
			right = -cameraForwardVector;
		}
		else
		{
			right *= (float)(1.0 / Math.Sqrt(num));
		}
		Vector3.Cross(ref cameraUpVector, ref right, out var result2);
		result2.Normalize();
		Vector3.Cross(ref right, ref result2, out var result3);
		result.M11 = result2.X;
		result.M12 = result2.Y;
		result.M13 = result2.Z;
		result.M21 = result3.X;
		result.M22 = result3.Y;
		result.M23 = result3.Z;
		result.M31 = right.X;
		result.M32 = right.Y;
		result.M33 = right.Z;
	}

	public static Matrix3x3 Billboard(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3 cameraForwardVector)
	{
		Billboard(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector, out var result);
		return result;
	}

	public static void LookAtLH(ref Vector3 eye, ref Vector3 target, ref Vector3 up, out Matrix3x3 result)
	{
		Vector3.Subtract(ref target, ref eye, out var result2);
		result2.Normalize();
		Vector3.Cross(ref up, ref result2, out var result3);
		result3.Normalize();
		Vector3.Cross(ref result2, ref result3, out var result4);
		result = Identity;
		result.M11 = result3.X;
		result.M21 = result3.Y;
		result.M31 = result3.Z;
		result.M12 = result4.X;
		result.M22 = result4.Y;
		result.M32 = result4.Z;
		result.M13 = result2.X;
		result.M23 = result2.Y;
		result.M33 = result2.Z;
	}

	public static Matrix3x3 LookAtLH(Vector3 eye, Vector3 target, Vector3 up)
	{
		LookAtLH(ref eye, ref target, ref up, out var result);
		return result;
	}

	public static void LookAtRH(ref Vector3 eye, ref Vector3 target, ref Vector3 up, out Matrix3x3 result)
	{
		Vector3.Subtract(ref eye, ref target, out var result2);
		result2.Normalize();
		Vector3.Cross(ref up, ref result2, out var result3);
		result3.Normalize();
		Vector3.Cross(ref result2, ref result3, out var result4);
		result = Identity;
		result.M11 = result3.X;
		result.M21 = result3.Y;
		result.M31 = result3.Z;
		result.M12 = result4.X;
		result.M22 = result4.Y;
		result.M32 = result4.Z;
		result.M13 = result2.X;
		result.M23 = result2.Y;
		result.M33 = result2.Z;
	}

	public static Matrix3x3 LookAtRH(Vector3 eye, Vector3 target, Vector3 up)
	{
		LookAtRH(ref eye, ref target, ref up, out var result);
		return result;
	}

	public static void Reflection(ref Plane plane, out Matrix3x3 result)
	{
		float x = plane.Normal.X;
		float y = plane.Normal.Y;
		float z = plane.Normal.Z;
		float num = -2f * x;
		float num2 = -2f * y;
		float num3 = -2f * z;
		result.M11 = num * x + 1f;
		result.M12 = num2 * x;
		result.M13 = num3 * x;
		result.M21 = num * y;
		result.M22 = num2 * y + 1f;
		result.M23 = num3 * y;
		result.M31 = num * z;
		result.M32 = num2 * z;
		result.M33 = num3 * z + 1f;
	}

	public static Matrix3x3 Reflection(Plane plane)
	{
		Reflection(ref plane, out var result);
		return result;
	}

	public static void Shadow(ref Vector4 light, ref Plane plane, out Matrix3x3 result)
	{
		float num = plane.Normal.X * light.X + plane.Normal.Y * light.Y + plane.Normal.Z * light.Z + plane.D * light.W;
		float num2 = 0f - plane.Normal.X;
		float num3 = 0f - plane.Normal.Y;
		float num4 = 0f - plane.Normal.Z;
		result.M11 = num2 * light.X + num;
		result.M21 = num3 * light.X;
		result.M31 = num4 * light.X;
		result.M12 = num2 * light.Y;
		result.M22 = num3 * light.Y + num;
		result.M32 = num4 * light.Y;
		result.M13 = num2 * light.Z;
		result.M23 = num3 * light.Z;
		result.M33 = num4 * light.Z + num;
	}

	public static Matrix3x3 Shadow(Vector4 light, Plane plane)
	{
		Shadow(ref light, ref plane, out var result);
		return result;
	}

	public static void Scaling(ref Vector3 scale, out Matrix3x3 result)
	{
		Scaling(scale.X, scale.Y, scale.Z, out result);
	}

	public static Matrix3x3 Scaling(Vector3 scale)
	{
		Scaling(ref scale, out var result);
		return result;
	}

	public static void Scaling(float x, float y, float z, out Matrix3x3 result)
	{
		result = Identity;
		result.M11 = x;
		result.M22 = y;
		result.M33 = z;
	}

	public static Matrix3x3 Scaling(float x, float y, float z)
	{
		Scaling(x, y, z, out var result);
		return result;
	}

	public static void Scaling(float scale, out Matrix3x3 result)
	{
		result = Identity;
		result.M11 = (result.M22 = (result.M33 = scale));
	}

	public static Matrix3x3 Scaling(float scale)
	{
		Scaling(scale, out var result);
		return result;
	}

	public static void RotationX(float angle, out Matrix3x3 result)
	{
		float num = (float)Math.Cos(angle);
		float num2 = (float)Math.Sin(angle);
		result = Identity;
		result.M22 = num;
		result.M23 = num2;
		result.M32 = 0f - num2;
		result.M33 = num;
	}

	public static Matrix3x3 RotationX(float angle)
	{
		RotationX(angle, out var result);
		return result;
	}

	public static void RotationY(float angle, out Matrix3x3 result)
	{
		float num = (float)Math.Cos(angle);
		float num2 = (float)Math.Sin(angle);
		result = Identity;
		result.M11 = num;
		result.M13 = 0f - num2;
		result.M31 = num2;
		result.M33 = num;
	}

	public static Matrix3x3 RotationY(float angle)
	{
		RotationY(angle, out var result);
		return result;
	}

	public static void RotationZ(float angle, out Matrix3x3 result)
	{
		float num = (float)Math.Cos(angle);
		float num2 = (float)Math.Sin(angle);
		result = Identity;
		result.M11 = num;
		result.M12 = num2;
		result.M21 = 0f - num2;
		result.M22 = num;
	}

	public static Matrix3x3 RotationZ(float angle)
	{
		RotationZ(angle, out var result);
		return result;
	}

	public static void RotationAxis(ref Vector3 axis, float angle, out Matrix3x3 result)
	{
		float x = axis.X;
		float y = axis.Y;
		float z = axis.Z;
		float num = (float)Math.Cos(angle);
		float num2 = (float)Math.Sin(angle);
		float num3 = x * x;
		float num4 = y * y;
		float num5 = z * z;
		float num6 = x * y;
		float num7 = x * z;
		float num8 = y * z;
		result = Identity;
		result.M11 = num3 + num * (1f - num3);
		result.M12 = num6 - num * num6 + num2 * z;
		result.M13 = num7 - num * num7 - num2 * y;
		result.M21 = num6 - num * num6 - num2 * z;
		result.M22 = num4 + num * (1f - num4);
		result.M23 = num8 - num * num8 + num2 * x;
		result.M31 = num7 - num * num7 + num2 * y;
		result.M32 = num8 - num * num8 - num2 * x;
		result.M33 = num5 + num * (1f - num5);
	}

	public static Matrix3x3 RotationAxis(Vector3 axis, float angle)
	{
		RotationAxis(ref axis, angle, out var result);
		return result;
	}

	public static void RotationQuaternion(ref Quaternion rotation, out Matrix3x3 result)
	{
		float num = rotation.X * rotation.X;
		float num2 = rotation.Y * rotation.Y;
		float num3 = rotation.Z * rotation.Z;
		float num4 = rotation.X * rotation.Y;
		float num5 = rotation.Z * rotation.W;
		float num6 = rotation.Z * rotation.X;
		float num7 = rotation.Y * rotation.W;
		float num8 = rotation.Y * rotation.Z;
		float num9 = rotation.X * rotation.W;
		result = Identity;
		result.M11 = 1f - 2f * (num2 + num3);
		result.M12 = 2f * (num4 + num5);
		result.M13 = 2f * (num6 - num7);
		result.M21 = 2f * (num4 - num5);
		result.M22 = 1f - 2f * (num3 + num);
		result.M23 = 2f * (num8 + num9);
		result.M31 = 2f * (num6 + num7);
		result.M32 = 2f * (num8 - num9);
		result.M33 = 1f - 2f * (num2 + num);
	}

	public static Matrix3x3 RotationQuaternion(Quaternion rotation)
	{
		RotationQuaternion(ref rotation, out var result);
		return result;
	}

	public static void RotationYawPitchRoll(float yaw, float pitch, float roll, out Matrix3x3 result)
	{
		Quaternion result2 = default(Quaternion);
		Quaternion.RotationYawPitchRoll(yaw, pitch, roll, out result2);
		RotationQuaternion(ref result2, out result);
	}

	public static Matrix3x3 RotationYawPitchRoll(float yaw, float pitch, float roll)
	{
		RotationYawPitchRoll(yaw, pitch, roll, out var result);
		return result;
	}

	public static Matrix3x3 operator +(Matrix3x3 left, Matrix3x3 right)
	{
		Add(ref left, ref right, out var result);
		return result;
	}

	public static Matrix3x3 operator +(Matrix3x3 value)
	{
		return value;
	}

	public static Matrix3x3 operator -(Matrix3x3 left, Matrix3x3 right)
	{
		Subtract(ref left, ref right, out var result);
		return result;
	}

	public static Matrix3x3 operator -(Matrix3x3 value)
	{
		Negate(ref value, out var result);
		return result;
	}

	public static Matrix3x3 operator *(float left, Matrix3x3 right)
	{
		Multiply(ref right, left, out var result);
		return result;
	}

	public static Matrix3x3 operator *(Matrix3x3 left, float right)
	{
		Multiply(ref left, right, out var result);
		return result;
	}

	public static Matrix3x3 operator *(Matrix3x3 left, Matrix3x3 right)
	{
		Multiply(ref left, ref right, out var result);
		return result;
	}

	public static Matrix3x3 operator /(Matrix3x3 left, float right)
	{
		Divide(ref left, right, out var result);
		return result;
	}

	public static Matrix3x3 operator /(Matrix3x3 left, Matrix3x3 right)
	{
		Divide(ref left, ref right, out var result);
		return result;
	}

	public static bool operator ==(Matrix3x3 left, Matrix3x3 right)
	{
		return left.Equals(ref right);
	}

	public static bool operator !=(Matrix3x3 left, Matrix3x3 right)
	{
		return !left.Equals(ref right);
	}

	public static explicit operator Matrix(Matrix3x3 Value)
	{
		return new Matrix(Value.M11, Value.M12, Value.M13, 0f, Value.M21, Value.M22, Value.M23, 0f, Value.M31, Value.M32, Value.M33, 0f, 0f, 0f, 0f, 1f);
	}

	public static explicit operator Matrix3x3(Matrix Value)
	{
		return new Matrix3x3(Value.M11, Value.M12, Value.M13, Value.M21, Value.M22, Value.M23, Value.M31, Value.M32, Value.M33);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "[M11:{0} M12:{1} M13:{2}] [M21:{3} M22:{4} M23:{5}] [M31:{6} M32:{7} M33:{8}]", M11, M12, M13, M21, M22, M23, M31, M32, M33);
	}

	public string ToString(string format)
	{
		if (format == null)
		{
			return ToString();
		}
		return string.Format(format, CultureInfo.CurrentCulture, "[M11:{0} M12:{1} M13:{2}] [M21:{3} M22:{4} M23:{5}] [M31:{6} M32:{7} M33:{8}]", M11.ToString(format, CultureInfo.CurrentCulture), M12.ToString(format, CultureInfo.CurrentCulture), M13.ToString(format, CultureInfo.CurrentCulture), M21.ToString(format, CultureInfo.CurrentCulture), M22.ToString(format, CultureInfo.CurrentCulture), M23.ToString(format, CultureInfo.CurrentCulture), M31.ToString(format, CultureInfo.CurrentCulture), M32.ToString(format, CultureInfo.CurrentCulture), M33.ToString(format, CultureInfo.CurrentCulture));
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "[M11:{0} M12:{1} M13:{2}] [M21:{3} M22:{4} M23:{5}] [M31:{6} M32:{7} M33:{8}]", M11.ToString(formatProvider), M12.ToString(formatProvider), M13.ToString(formatProvider), M21.ToString(formatProvider), M22.ToString(formatProvider), M23.ToString(formatProvider), M31.ToString(formatProvider), M32.ToString(formatProvider), M33.ToString(formatProvider));
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			return ToString(formatProvider);
		}
		return string.Format(format, formatProvider, "[M11:{0} M12:{1} M13:{2}] [M21:{3} M22:{4} M23:{5}] [M31:{6} M32:{7} M33:{8}]", M11.ToString(format, formatProvider), M12.ToString(format, formatProvider), M13.ToString(format, formatProvider), M21.ToString(format, formatProvider), M22.ToString(format, formatProvider), M23.ToString(format, formatProvider), M31.ToString(format, formatProvider), M32.ToString(format, formatProvider), M33.ToString(format, formatProvider));
	}

	public override int GetHashCode()
	{
		int hashCode = M11.GetHashCode();
		hashCode = (hashCode * 397) ^ M12.GetHashCode();
		hashCode = (hashCode * 397) ^ M13.GetHashCode();
		hashCode = (hashCode * 397) ^ M21.GetHashCode();
		hashCode = (hashCode * 397) ^ M22.GetHashCode();
		hashCode = (hashCode * 397) ^ M23.GetHashCode();
		hashCode = (hashCode * 397) ^ M31.GetHashCode();
		hashCode = (hashCode * 397) ^ M32.GetHashCode();
		return (hashCode * 397) ^ M33.GetHashCode();
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(M11);
			serializer.Writer.Write(M12);
			serializer.Writer.Write(M13);
			serializer.Writer.Write(M21);
			serializer.Writer.Write(M22);
			serializer.Writer.Write(M23);
			serializer.Writer.Write(M31);
			serializer.Writer.Write(M32);
			serializer.Writer.Write(M33);
		}
		else
		{
			M11 = serializer.Reader.ReadSingle();
			M12 = serializer.Reader.ReadSingle();
			M13 = serializer.Reader.ReadSingle();
			M21 = serializer.Reader.ReadSingle();
			M22 = serializer.Reader.ReadSingle();
			M23 = serializer.Reader.ReadSingle();
			M31 = serializer.Reader.ReadSingle();
			M32 = serializer.Reader.ReadSingle();
			M33 = serializer.Reader.ReadSingle();
		}
	}

	public bool Equals(ref Matrix3x3 other)
	{
		if (MathUtil.NearEqual(other.M11, M11) && MathUtil.NearEqual(other.M12, M12) && MathUtil.NearEqual(other.M13, M13) && MathUtil.NearEqual(other.M21, M21) && MathUtil.NearEqual(other.M22, M22) && MathUtil.NearEqual(other.M23, M23) && MathUtil.NearEqual(other.M31, M31) && MathUtil.NearEqual(other.M32, M32))
		{
			return MathUtil.NearEqual(other.M33, M33);
		}
		return false;
	}

	public bool Equals(Matrix3x3 other)
	{
		return Equals(ref other);
	}

	public static bool Equals(ref Matrix3x3 a, ref Matrix3x3 b)
	{
		if (MathUtil.NearEqual(a.M11, b.M11) && MathUtil.NearEqual(a.M12, b.M12) && MathUtil.NearEqual(a.M13, b.M13) && MathUtil.NearEqual(a.M21, b.M21) && MathUtil.NearEqual(a.M22, b.M22) && MathUtil.NearEqual(a.M23, b.M23) && MathUtil.NearEqual(a.M31, b.M31) && MathUtil.NearEqual(a.M32, b.M32))
		{
			return MathUtil.NearEqual(a.M33, b.M33);
		}
		return false;
	}

	public override bool Equals(object value)
	{
		if (!(value is Matrix3x3 other))
		{
			return false;
		}
		return Equals(ref other);
	}

	static Matrix3x3()
	{
		SizeInBytes = Marshal.SizeOf(typeof(Matrix3x3));
		Zero = default(Matrix3x3);
		Identity = new Matrix3x3
		{
			M11 = 1f,
			M22 = 1f,
			M33 = 1f
		};
	}
}

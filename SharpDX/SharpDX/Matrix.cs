using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DynamicSerializer("TKMX")]
public struct Matrix : IEquatable<Matrix>, IFormattable, IDataSerializable
{
	public static readonly int SizeInBytes;

	public static readonly Matrix Zero;

	public static readonly Matrix Identity;

	public float M11;

	public float M12;

	public float M13;

	public float M14;

	public float M21;

	public float M22;

	public float M23;

	public float M24;

	public float M31;

	public float M32;

	public float M33;

	public float M34;

	public float M41;

	public float M42;

	public float M43;

	public float M44;

	public Vector3 Up
	{
		get
		{
			Vector3 result = default(Vector3);
			result.X = M21;
			result.Y = M22;
			result.Z = M23;
			return result;
		}
		set
		{
			M21 = value.X;
			M22 = value.Y;
			M23 = value.Z;
		}
	}

	public Vector3 Down
	{
		get
		{
			Vector3 result = default(Vector3);
			result.X = 0f - M21;
			result.Y = 0f - M22;
			result.Z = 0f - M23;
			return result;
		}
		set
		{
			M21 = 0f - value.X;
			M22 = 0f - value.Y;
			M23 = 0f - value.Z;
		}
	}

	public Vector3 Right
	{
		get
		{
			Vector3 result = default(Vector3);
			result.X = M11;
			result.Y = M12;
			result.Z = M13;
			return result;
		}
		set
		{
			M11 = value.X;
			M12 = value.Y;
			M13 = value.Z;
		}
	}

	public Vector3 Left
	{
		get
		{
			Vector3 result = default(Vector3);
			result.X = 0f - M11;
			result.Y = 0f - M12;
			result.Z = 0f - M13;
			return result;
		}
		set
		{
			M11 = 0f - value.X;
			M12 = 0f - value.Y;
			M13 = 0f - value.Z;
		}
	}

	public Vector3 Forward
	{
		get
		{
			Vector3 result = default(Vector3);
			result.X = 0f - M31;
			result.Y = 0f - M32;
			result.Z = 0f - M33;
			return result;
		}
		set
		{
			M31 = 0f - value.X;
			M32 = 0f - value.Y;
			M33 = 0f - value.Z;
		}
	}

	public Vector3 Backward
	{
		get
		{
			Vector3 result = default(Vector3);
			result.X = M31;
			result.Y = M32;
			result.Z = M33;
			return result;
		}
		set
		{
			M31 = value.X;
			M32 = value.Y;
			M33 = value.Z;
		}
	}

	public Vector4 Row1
	{
		get
		{
			return new Vector4(M11, M12, M13, M14);
		}
		set
		{
			M11 = value.X;
			M12 = value.Y;
			M13 = value.Z;
			M14 = value.W;
		}
	}

	public Vector4 Row2
	{
		get
		{
			return new Vector4(M21, M22, M23, M24);
		}
		set
		{
			M21 = value.X;
			M22 = value.Y;
			M23 = value.Z;
			M24 = value.W;
		}
	}

	public Vector4 Row3
	{
		get
		{
			return new Vector4(M31, M32, M33, M34);
		}
		set
		{
			M31 = value.X;
			M32 = value.Y;
			M33 = value.Z;
			M34 = value.W;
		}
	}

	public Vector4 Row4
	{
		get
		{
			return new Vector4(M41, M42, M43, M44);
		}
		set
		{
			M41 = value.X;
			M42 = value.Y;
			M43 = value.Z;
			M44 = value.W;
		}
	}

	public Vector4 Column1
	{
		get
		{
			return new Vector4(M11, M21, M31, M41);
		}
		set
		{
			M11 = value.X;
			M21 = value.Y;
			M31 = value.Z;
			M41 = value.W;
		}
	}

	public Vector4 Column2
	{
		get
		{
			return new Vector4(M12, M22, M32, M42);
		}
		set
		{
			M12 = value.X;
			M22 = value.Y;
			M32 = value.Z;
			M42 = value.W;
		}
	}

	public Vector4 Column3
	{
		get
		{
			return new Vector4(M13, M23, M33, M43);
		}
		set
		{
			M13 = value.X;
			M23 = value.Y;
			M33 = value.Z;
			M43 = value.W;
		}
	}

	public Vector4 Column4
	{
		get
		{
			return new Vector4(M14, M24, M34, M44);
		}
		set
		{
			M14 = value.X;
			M24 = value.Y;
			M34 = value.Z;
			M44 = value.W;
		}
	}

	public Vector3 TranslationVector
	{
		get
		{
			return new Vector3(M41, M42, M43);
		}
		set
		{
			M41 = value.X;
			M42 = value.Y;
			M43 = value.Z;
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
				3 => M14, 
				4 => M21, 
				5 => M22, 
				6 => M23, 
				7 => M24, 
				8 => M31, 
				9 => M32, 
				10 => M33, 
				11 => M34, 
				12 => M41, 
				13 => M42, 
				14 => M43, 
				15 => M44, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Matrix run from 0 to 15, inclusive."), 
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
			case 3:
				M14 = value;
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
			case 7:
				M24 = value;
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
			case 11:
				M34 = value;
				break;
			case 12:
				M41 = value;
				break;
			case 13:
				M42 = value;
				break;
			case 14:
				M43 = value;
				break;
			case 15:
				M44 = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for Matrix run from 0 to 15, inclusive.");
			}
		}
	}

	public float this[int row, int column]
	{
		get
		{
			if (row < 0 || row > 3)
			{
				throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 3, inclusive.");
			}
			if (column < 0 || column > 3)
			{
				throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 3, inclusive.");
			}
			return this[row * 4 + column];
		}
		set
		{
			if (row < 0 || row > 3)
			{
				throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 3, inclusive.");
			}
			if (column < 0 || column > 3)
			{
				throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 3, inclusive.");
			}
			this[row * 4 + column] = value;
		}
	}

	public Matrix(float value)
	{
		M11 = (M12 = (M13 = (M14 = (M21 = (M22 = (M23 = (M24 = (M31 = (M32 = (M33 = (M34 = (M41 = (M42 = (M43 = (M44 = value)))))))))))))));
	}

	public Matrix(float M11, float M12, float M13, float M14, float M21, float M22, float M23, float M24, float M31, float M32, float M33, float M34, float M41, float M42, float M43, float M44)
	{
		this.M11 = M11;
		this.M12 = M12;
		this.M13 = M13;
		this.M14 = M14;
		this.M21 = M21;
		this.M22 = M22;
		this.M23 = M23;
		this.M24 = M24;
		this.M31 = M31;
		this.M32 = M32;
		this.M33 = M33;
		this.M34 = M34;
		this.M41 = M41;
		this.M42 = M42;
		this.M43 = M43;
		this.M44 = M44;
	}

	public Matrix(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 16)
		{
			throw new ArgumentOutOfRangeException("values", "There must be sixteen and only sixteen input values for Matrix.");
		}
		M11 = values[0];
		M12 = values[1];
		M13 = values[2];
		M14 = values[3];
		M21 = values[4];
		M22 = values[5];
		M23 = values[6];
		M24 = values[7];
		M31 = values[8];
		M32 = values[9];
		M33 = values[10];
		M34 = values[11];
		M41 = values[12];
		M42 = values[13];
		M43 = values[14];
		M44 = values[15];
	}

	public float Determinant()
	{
		float num = M33 * M44 - M34 * M43;
		float num2 = M32 * M44 - M34 * M42;
		float num3 = M32 * M43 - M33 * M42;
		float num4 = M31 * M44 - M34 * M41;
		float num5 = M31 * M43 - M33 * M41;
		float num6 = M31 * M42 - M32 * M41;
		return M11 * (M22 * num - M23 * num2 + M24 * num3) - M12 * (M21 * num - M23 * num4 + M24 * num5) + M13 * (M21 * num2 - M22 * num4 + M24 * num6) - M14 * (M21 * num3 - M22 * num5 + M23 * num6);
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

	public void DecomposeQR(out Matrix Q, out Matrix R)
	{
		Matrix value = this;
		value.Transpose();
		Orthonormalize(ref value, out Q);
		Q.Transpose();
		R = default(Matrix);
		R.M11 = Vector4.Dot(Q.Column1, Column1);
		R.M12 = Vector4.Dot(Q.Column1, Column2);
		R.M13 = Vector4.Dot(Q.Column1, Column3);
		R.M14 = Vector4.Dot(Q.Column1, Column4);
		R.M22 = Vector4.Dot(Q.Column2, Column2);
		R.M23 = Vector4.Dot(Q.Column2, Column3);
		R.M24 = Vector4.Dot(Q.Column2, Column4);
		R.M33 = Vector4.Dot(Q.Column3, Column3);
		R.M34 = Vector4.Dot(Q.Column3, Column4);
		R.M44 = Vector4.Dot(Q.Column4, Column4);
	}

	public void DecomposeLQ(out Matrix L, out Matrix Q)
	{
		Orthonormalize(ref this, out Q);
		L = default(Matrix);
		L.M11 = Vector4.Dot(Q.Row1, Row1);
		L.M21 = Vector4.Dot(Q.Row1, Row2);
		L.M22 = Vector4.Dot(Q.Row2, Row2);
		L.M31 = Vector4.Dot(Q.Row1, Row3);
		L.M32 = Vector4.Dot(Q.Row2, Row3);
		L.M33 = Vector4.Dot(Q.Row3, Row3);
		L.M41 = Vector4.Dot(Q.Row1, Row4);
		L.M42 = Vector4.Dot(Q.Row2, Row4);
		L.M43 = Vector4.Dot(Q.Row3, Row4);
		L.M44 = Vector4.Dot(Q.Row4, Row4);
	}

	public bool Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
	{
		translation.X = M41;
		translation.Y = M42;
		translation.Z = M43;
		scale.X = (float)Math.Sqrt(M11 * M11 + M12 * M12 + M13 * M13);
		scale.Y = (float)Math.Sqrt(M21 * M21 + M22 * M22 + M23 * M23);
		scale.Z = (float)Math.Sqrt(M31 * M31 + M32 * M32 + M33 * M33);
		if (MathUtil.IsZero(scale.X) || MathUtil.IsZero(scale.Y) || MathUtil.IsZero(scale.Z))
		{
			rotation = Quaternion.Identity;
			return false;
		}
		Matrix matrix = new Matrix
		{
			M11 = M11 / scale.X,
			M12 = M12 / scale.X,
			M13 = M13 / scale.X,
			M21 = M21 / scale.Y,
			M22 = M22 / scale.Y,
			M23 = M23 / scale.Y,
			M31 = M31 / scale.Z,
			M32 = M32 / scale.Z,
			M33 = M33 / scale.Z,
			M44 = 1f
		};
		Quaternion.RotationMatrix(ref matrix, out rotation);
		return true;
	}

	public bool DecomposeUniformScale(out float scale, out Quaternion rotation, out Vector3 translation)
	{
		translation.X = M41;
		translation.Y = M42;
		translation.Z = M43;
		scale = (float)Math.Sqrt(M11 * M11 + M12 * M12 + M13 * M13);
		float num = 1f / scale;
		if (Math.Abs(scale) < 1E-06f)
		{
			rotation = Quaternion.Identity;
			return false;
		}
		Matrix matrix = new Matrix
		{
			M11 = M11 * num,
			M12 = M12 * num,
			M13 = M13 * num,
			M21 = M21 * num,
			M22 = M22 * num,
			M23 = M23 * num,
			M31 = M31 * num,
			M32 = M32 * num,
			M33 = M33 * num,
			M44 = 1f
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
		if (firstRow > 3)
		{
			throw new ArgumentOutOfRangeException("firstRow", "The parameter firstRow must be less than or equal to three.");
		}
		if (secondRow < 0)
		{
			throw new ArgumentOutOfRangeException("secondRow", "The parameter secondRow must be greater than or equal to zero.");
		}
		if (secondRow > 3)
		{
			throw new ArgumentOutOfRangeException("secondRow", "The parameter secondRow must be less than or equal to three.");
		}
		if (firstRow != secondRow)
		{
			float value = this[secondRow, 0];
			float value2 = this[secondRow, 1];
			float value3 = this[secondRow, 2];
			float value4 = this[secondRow, 3];
			this[secondRow, 0] = this[firstRow, 0];
			this[secondRow, 1] = this[firstRow, 1];
			this[secondRow, 2] = this[firstRow, 2];
			this[secondRow, 3] = this[firstRow, 3];
			this[firstRow, 0] = value;
			this[firstRow, 1] = value2;
			this[firstRow, 2] = value3;
			this[firstRow, 3] = value4;
		}
	}

	public void ExchangeColumns(int firstColumn, int secondColumn)
	{
		if (firstColumn < 0)
		{
			throw new ArgumentOutOfRangeException("firstColumn", "The parameter firstColumn must be greater than or equal to zero.");
		}
		if (firstColumn > 3)
		{
			throw new ArgumentOutOfRangeException("firstColumn", "The parameter firstColumn must be less than or equal to three.");
		}
		if (secondColumn < 0)
		{
			throw new ArgumentOutOfRangeException("secondColumn", "The parameter secondColumn must be greater than or equal to zero.");
		}
		if (secondColumn > 3)
		{
			throw new ArgumentOutOfRangeException("secondColumn", "The parameter secondColumn must be less than or equal to three.");
		}
		if (firstColumn != secondColumn)
		{
			float value = this[0, secondColumn];
			float value2 = this[1, secondColumn];
			float value3 = this[2, secondColumn];
			float value4 = this[3, secondColumn];
			this[0, secondColumn] = this[0, firstColumn];
			this[1, secondColumn] = this[1, firstColumn];
			this[2, secondColumn] = this[2, firstColumn];
			this[3, secondColumn] = this[3, firstColumn];
			this[0, firstColumn] = value;
			this[1, firstColumn] = value2;
			this[2, firstColumn] = value3;
			this[3, firstColumn] = value4;
		}
	}

	public float[] ToArray()
	{
		return new float[16]
		{
			M11, M12, M13, M14, M21, M22, M23, M24, M31, M32,
			M33, M34, M41, M42, M43, M44
		};
	}

	public static void Add(ref Matrix left, ref Matrix right, out Matrix result)
	{
		result.M11 = left.M11 + right.M11;
		result.M12 = left.M12 + right.M12;
		result.M13 = left.M13 + right.M13;
		result.M14 = left.M14 + right.M14;
		result.M21 = left.M21 + right.M21;
		result.M22 = left.M22 + right.M22;
		result.M23 = left.M23 + right.M23;
		result.M24 = left.M24 + right.M24;
		result.M31 = left.M31 + right.M31;
		result.M32 = left.M32 + right.M32;
		result.M33 = left.M33 + right.M33;
		result.M34 = left.M34 + right.M34;
		result.M41 = left.M41 + right.M41;
		result.M42 = left.M42 + right.M42;
		result.M43 = left.M43 + right.M43;
		result.M44 = left.M44 + right.M44;
	}

	public static Matrix Add(Matrix left, Matrix right)
	{
		Add(ref left, ref right, out var result);
		return result;
	}

	public static void Subtract(ref Matrix left, ref Matrix right, out Matrix result)
	{
		result.M11 = left.M11 - right.M11;
		result.M12 = left.M12 - right.M12;
		result.M13 = left.M13 - right.M13;
		result.M14 = left.M14 - right.M14;
		result.M21 = left.M21 - right.M21;
		result.M22 = left.M22 - right.M22;
		result.M23 = left.M23 - right.M23;
		result.M24 = left.M24 - right.M24;
		result.M31 = left.M31 - right.M31;
		result.M32 = left.M32 - right.M32;
		result.M33 = left.M33 - right.M33;
		result.M34 = left.M34 - right.M34;
		result.M41 = left.M41 - right.M41;
		result.M42 = left.M42 - right.M42;
		result.M43 = left.M43 - right.M43;
		result.M44 = left.M44 - right.M44;
	}

	public static Matrix Subtract(Matrix left, Matrix right)
	{
		Subtract(ref left, ref right, out var result);
		return result;
	}

	public static void Multiply(ref Matrix left, float right, out Matrix result)
	{
		result.M11 = left.M11 * right;
		result.M12 = left.M12 * right;
		result.M13 = left.M13 * right;
		result.M14 = left.M14 * right;
		result.M21 = left.M21 * right;
		result.M22 = left.M22 * right;
		result.M23 = left.M23 * right;
		result.M24 = left.M24 * right;
		result.M31 = left.M31 * right;
		result.M32 = left.M32 * right;
		result.M33 = left.M33 * right;
		result.M34 = left.M34 * right;
		result.M41 = left.M41 * right;
		result.M42 = left.M42 * right;
		result.M43 = left.M43 * right;
		result.M44 = left.M44 * right;
	}

	public static Matrix Multiply(Matrix left, float right)
	{
		Multiply(ref left, right, out var result);
		return result;
	}

	public static void Multiply(ref Matrix left, ref Matrix right, out Matrix result)
	{
		result = new Matrix
		{
			M11 = left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41,
			M12 = left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42,
			M13 = left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43,
			M14 = left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44,
			M21 = left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41,
			M22 = left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42,
			M23 = left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43,
			M24 = left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44,
			M31 = left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41,
			M32 = left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42,
			M33 = left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43,
			M34 = left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44,
			M41 = left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41,
			M42 = left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42,
			M43 = left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43,
			M44 = left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44
		};
	}

	public static Matrix Multiply(Matrix left, Matrix right)
	{
		Multiply(ref left, ref right, out var result);
		return result;
	}

	public static void Divide(ref Matrix left, float right, out Matrix result)
	{
		float num = 1f / right;
		result.M11 = left.M11 * num;
		result.M12 = left.M12 * num;
		result.M13 = left.M13 * num;
		result.M14 = left.M14 * num;
		result.M21 = left.M21 * num;
		result.M22 = left.M22 * num;
		result.M23 = left.M23 * num;
		result.M24 = left.M24 * num;
		result.M31 = left.M31 * num;
		result.M32 = left.M32 * num;
		result.M33 = left.M33 * num;
		result.M34 = left.M34 * num;
		result.M41 = left.M41 * num;
		result.M42 = left.M42 * num;
		result.M43 = left.M43 * num;
		result.M44 = left.M44 * num;
	}

	public static Matrix Divide(Matrix left, float right)
	{
		Divide(ref left, right, out var result);
		return result;
	}

	public static void Divide(ref Matrix left, ref Matrix right, out Matrix result)
	{
		result.M11 = left.M11 / right.M11;
		result.M12 = left.M12 / right.M12;
		result.M13 = left.M13 / right.M13;
		result.M14 = left.M14 / right.M14;
		result.M21 = left.M21 / right.M21;
		result.M22 = left.M22 / right.M22;
		result.M23 = left.M23 / right.M23;
		result.M24 = left.M24 / right.M24;
		result.M31 = left.M31 / right.M31;
		result.M32 = left.M32 / right.M32;
		result.M33 = left.M33 / right.M33;
		result.M34 = left.M34 / right.M34;
		result.M41 = left.M41 / right.M41;
		result.M42 = left.M42 / right.M42;
		result.M43 = left.M43 / right.M43;
		result.M44 = left.M44 / right.M44;
	}

	public static Matrix Divide(Matrix left, Matrix right)
	{
		Divide(ref left, ref right, out var result);
		return result;
	}

	public static void Exponent(ref Matrix value, int exponent, out Matrix result)
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
		Matrix identity = Identity;
		Matrix matrix = value;
		while (true)
		{
			if ((exponent & 1) != 0)
			{
				identity *= matrix;
			}
			exponent /= 2;
			if (exponent <= 0)
			{
				break;
			}
			matrix *= matrix;
		}
		result = identity;
	}

	public static Matrix Exponent(Matrix value, int exponent)
	{
		Exponent(ref value, exponent, out var result);
		return result;
	}

	public static void Negate(ref Matrix value, out Matrix result)
	{
		result.M11 = 0f - value.M11;
		result.M12 = 0f - value.M12;
		result.M13 = 0f - value.M13;
		result.M14 = 0f - value.M14;
		result.M21 = 0f - value.M21;
		result.M22 = 0f - value.M22;
		result.M23 = 0f - value.M23;
		result.M24 = 0f - value.M24;
		result.M31 = 0f - value.M31;
		result.M32 = 0f - value.M32;
		result.M33 = 0f - value.M33;
		result.M34 = 0f - value.M34;
		result.M41 = 0f - value.M41;
		result.M42 = 0f - value.M42;
		result.M43 = 0f - value.M43;
		result.M44 = 0f - value.M44;
	}

	public static Matrix Negate(Matrix value)
	{
		Negate(ref value, out var result);
		return result;
	}

	public static void Lerp(ref Matrix start, ref Matrix end, float amount, out Matrix result)
	{
		result.M11 = MathUtil.Lerp(start.M11, end.M11, amount);
		result.M12 = MathUtil.Lerp(start.M12, end.M12, amount);
		result.M13 = MathUtil.Lerp(start.M13, end.M13, amount);
		result.M14 = MathUtil.Lerp(start.M14, end.M14, amount);
		result.M21 = MathUtil.Lerp(start.M21, end.M21, amount);
		result.M22 = MathUtil.Lerp(start.M22, end.M22, amount);
		result.M23 = MathUtil.Lerp(start.M23, end.M23, amount);
		result.M24 = MathUtil.Lerp(start.M24, end.M24, amount);
		result.M31 = MathUtil.Lerp(start.M31, end.M31, amount);
		result.M32 = MathUtil.Lerp(start.M32, end.M32, amount);
		result.M33 = MathUtil.Lerp(start.M33, end.M33, amount);
		result.M34 = MathUtil.Lerp(start.M34, end.M34, amount);
		result.M41 = MathUtil.Lerp(start.M41, end.M41, amount);
		result.M42 = MathUtil.Lerp(start.M42, end.M42, amount);
		result.M43 = MathUtil.Lerp(start.M43, end.M43, amount);
		result.M44 = MathUtil.Lerp(start.M44, end.M44, amount);
	}

	public static Matrix Lerp(Matrix start, Matrix end, float amount)
	{
		Lerp(ref start, ref end, amount, out var result);
		return result;
	}

	public static void SmoothStep(ref Matrix start, ref Matrix end, float amount, out Matrix result)
	{
		amount = MathUtil.SmoothStep(amount);
		Lerp(ref start, ref end, amount, out result);
	}

	public static Matrix SmoothStep(Matrix start, Matrix end, float amount)
	{
		SmoothStep(ref start, ref end, amount, out var result);
		return result;
	}

	public static void Transpose(ref Matrix value, out Matrix result)
	{
		result = new Matrix
		{
			M11 = value.M11,
			M12 = value.M21,
			M13 = value.M31,
			M14 = value.M41,
			M21 = value.M12,
			M22 = value.M22,
			M23 = value.M32,
			M24 = value.M42,
			M31 = value.M13,
			M32 = value.M23,
			M33 = value.M33,
			M34 = value.M43,
			M41 = value.M14,
			M42 = value.M24,
			M43 = value.M34,
			M44 = value.M44
		};
	}

	public static void TransposeByRef(ref Matrix value, ref Matrix result)
	{
		result.M11 = value.M11;
		result.M12 = value.M21;
		result.M13 = value.M31;
		result.M14 = value.M41;
		result.M21 = value.M12;
		result.M22 = value.M22;
		result.M23 = value.M32;
		result.M24 = value.M42;
		result.M31 = value.M13;
		result.M32 = value.M23;
		result.M33 = value.M33;
		result.M34 = value.M43;
		result.M41 = value.M14;
		result.M42 = value.M24;
		result.M43 = value.M34;
		result.M44 = value.M44;
	}

	public static Matrix Transpose(Matrix value)
	{
		Transpose(ref value, out var result);
		return result;
	}

	public static void Invert(ref Matrix value, out Matrix result)
	{
		float num = value.M31 * value.M42 - value.M32 * value.M41;
		float num2 = value.M31 * value.M43 - value.M33 * value.M41;
		float num3 = value.M34 * value.M41 - value.M31 * value.M44;
		float num4 = value.M32 * value.M43 - value.M33 * value.M42;
		float num5 = value.M34 * value.M42 - value.M32 * value.M44;
		float num6 = value.M33 * value.M44 - value.M34 * value.M43;
		float num7 = value.M22 * num6 + value.M23 * num5 + value.M24 * num4;
		float num8 = value.M21 * num6 + value.M23 * num3 + value.M24 * num2;
		float num9 = value.M21 * (0f - num5) + value.M22 * num3 + value.M24 * num;
		float num10 = value.M21 * num4 + value.M22 * (0f - num2) + value.M23 * num;
		float num11 = value.M11 * num7 - value.M12 * num8 + value.M13 * num9 - value.M14 * num10;
		if (Math.Abs(num11) == 0f)
		{
			result = Zero;
			return;
		}
		num11 = 1f / num11;
		float num12 = value.M11 * value.M22 - value.M12 * value.M21;
		float num13 = value.M11 * value.M23 - value.M13 * value.M21;
		float num14 = value.M14 * value.M21 - value.M11 * value.M24;
		float num15 = value.M12 * value.M23 - value.M13 * value.M22;
		float num16 = value.M14 * value.M22 - value.M12 * value.M24;
		float num17 = value.M13 * value.M24 - value.M14 * value.M23;
		float num18 = value.M12 * num6 + value.M13 * num5 + value.M14 * num4;
		float num19 = value.M11 * num6 + value.M13 * num3 + value.M14 * num2;
		float num20 = value.M11 * (0f - num5) + value.M12 * num3 + value.M14 * num;
		float num21 = value.M11 * num4 + value.M12 * (0f - num2) + value.M13 * num;
		float num22 = value.M42 * num17 + value.M43 * num16 + value.M44 * num15;
		float num23 = value.M41 * num17 + value.M43 * num14 + value.M44 * num13;
		float num24 = value.M41 * (0f - num16) + value.M42 * num14 + value.M44 * num12;
		float num25 = value.M41 * num15 + value.M42 * (0f - num13) + value.M43 * num12;
		float num26 = value.M32 * num17 + value.M33 * num16 + value.M34 * num15;
		float num27 = value.M31 * num17 + value.M33 * num14 + value.M34 * num13;
		float num28 = value.M31 * (0f - num16) + value.M32 * num14 + value.M34 * num12;
		float num29 = value.M31 * num15 + value.M32 * (0f - num13) + value.M33 * num12;
		result.M11 = num7 * num11;
		result.M12 = (0f - num18) * num11;
		result.M13 = num22 * num11;
		result.M14 = (0f - num26) * num11;
		result.M21 = (0f - num8) * num11;
		result.M22 = num19 * num11;
		result.M23 = (0f - num23) * num11;
		result.M24 = num27 * num11;
		result.M31 = num9 * num11;
		result.M32 = (0f - num20) * num11;
		result.M33 = num24 * num11;
		result.M34 = (0f - num28) * num11;
		result.M41 = (0f - num10) * num11;
		result.M42 = num21 * num11;
		result.M43 = (0f - num25) * num11;
		result.M44 = num29 * num11;
	}

	public static Matrix Invert(Matrix value)
	{
		value.Invert();
		return value;
	}

	public static void Orthogonalize(ref Matrix value, out Matrix result)
	{
		result = value;
		result.Row2 -= Vector4.Dot(result.Row1, result.Row2) / Vector4.Dot(result.Row1, result.Row1) * result.Row1;
		result.Row3 -= Vector4.Dot(result.Row1, result.Row3) / Vector4.Dot(result.Row1, result.Row1) * result.Row1;
		result.Row3 -= Vector4.Dot(result.Row2, result.Row3) / Vector4.Dot(result.Row2, result.Row2) * result.Row2;
		result.Row4 -= Vector4.Dot(result.Row1, result.Row4) / Vector4.Dot(result.Row1, result.Row1) * result.Row1;
		result.Row4 -= Vector4.Dot(result.Row2, result.Row4) / Vector4.Dot(result.Row2, result.Row2) * result.Row2;
		result.Row4 -= Vector4.Dot(result.Row3, result.Row4) / Vector4.Dot(result.Row3, result.Row3) * result.Row3;
	}

	public static Matrix Orthogonalize(Matrix value)
	{
		Orthogonalize(ref value, out var result);
		return result;
	}

	public static void Orthonormalize(ref Matrix value, out Matrix result)
	{
		result = value;
		result.Row1 = Vector4.Normalize(result.Row1);
		result.Row2 -= Vector4.Dot(result.Row1, result.Row2) * result.Row1;
		result.Row2 = Vector4.Normalize(result.Row2);
		result.Row3 -= Vector4.Dot(result.Row1, result.Row3) * result.Row1;
		result.Row3 -= Vector4.Dot(result.Row2, result.Row3) * result.Row2;
		result.Row3 = Vector4.Normalize(result.Row3);
		result.Row4 -= Vector4.Dot(result.Row1, result.Row4) * result.Row1;
		result.Row4 -= Vector4.Dot(result.Row2, result.Row4) * result.Row2;
		result.Row4 -= Vector4.Dot(result.Row3, result.Row4) * result.Row3;
		result.Row4 = Vector4.Normalize(result.Row4);
	}

	public static Matrix Orthonormalize(Matrix value)
	{
		Orthonormalize(ref value, out var result);
		return result;
	}

	public static void UpperTriangularForm(ref Matrix value, out Matrix result)
	{
		result = value;
		int num = 0;
		int num2 = 4;
		int num3 = 4;
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
					result[j, 3] -= result[i, 3] * num4 * result[j, num];
				}
			}
			num++;
		}
	}

	public static Matrix UpperTriangularForm(Matrix value)
	{
		UpperTriangularForm(ref value, out var result);
		return result;
	}

	public static void LowerTriangularForm(ref Matrix value, out Matrix result)
	{
		Matrix value2 = value;
		Transpose(ref value2, out result);
		int num = 0;
		int num2 = 4;
		int num3 = 4;
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
					result[j, 3] -= result[i, 3] * num4 * result[j, num];
				}
			}
			num++;
		}
		Transpose(ref result, out result);
	}

	public static Matrix LowerTriangularForm(Matrix value)
	{
		LowerTriangularForm(ref value, out var result);
		return result;
	}

	public static void RowEchelonForm(ref Matrix value, out Matrix result)
	{
		result = value;
		int num = 0;
		int num2 = 4;
		int num3 = 4;
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
			result[i, 3] *= num4;
			for (; j < num2; j++)
			{
				if (j != i)
				{
					result[j, 0] -= result[i, 0] * result[j, num];
					result[j, 1] -= result[i, 1] * result[j, num];
					result[j, 2] -= result[i, 2] * result[j, num];
					result[j, 3] -= result[i, 3] * result[j, num];
				}
			}
			num++;
		}
	}

	public static Matrix RowEchelonForm(Matrix value)
	{
		RowEchelonForm(ref value, out var result);
		return result;
	}

	public static void ReducedRowEchelonForm(ref Matrix value, ref Vector4 augment, out Matrix result, out Vector4 augmentResult)
	{
		float[,] array = new float[4, 5]
		{
			{
				value[0, 0],
				value[0, 1],
				value[0, 2],
				value[0, 3],
				augment[0]
			},
			{
				value[1, 0],
				value[1, 1],
				value[1, 2],
				value[1, 3],
				augment[1]
			},
			{
				value[2, 0],
				value[2, 1],
				value[2, 2],
				value[2, 3],
				augment[2]
			},
			{
				value[3, 0],
				value[3, 1],
				value[3, 2],
				value[3, 3],
				augment[3]
			}
		};
		int num = 0;
		int num2 = 4;
		int num3 = 5;
		for (int i = 0; i < num2; i++)
		{
			if (num3 <= num)
			{
				break;
			}
			int num4 = i;
			while (array[num4, num] == 0f)
			{
				num4++;
				if (num4 == num2)
				{
					num4 = i;
					num++;
					if (num3 == num)
					{
						break;
					}
				}
			}
			for (int j = 0; j < num3; j++)
			{
				float num5 = array[i, j];
				array[i, j] = array[num4, j];
				array[num4, j] = num5;
			}
			float num6 = array[i, num];
			for (int k = 0; k < num3; k++)
			{
				array[i, k] /= num6;
			}
			for (int l = 0; l < num2; l++)
			{
				if (l != i)
				{
					float num7 = array[l, num];
					for (int m = 0; m < num3; m++)
					{
						array[l, m] -= num7 * array[i, m];
					}
				}
			}
			num++;
		}
		result.M11 = array[0, 0];
		result.M12 = array[0, 1];
		result.M13 = array[0, 2];
		result.M14 = array[0, 3];
		result.M21 = array[1, 0];
		result.M22 = array[1, 1];
		result.M23 = array[1, 2];
		result.M24 = array[1, 3];
		result.M31 = array[2, 0];
		result.M32 = array[2, 1];
		result.M33 = array[2, 2];
		result.M34 = array[2, 3];
		result.M41 = array[3, 0];
		result.M42 = array[3, 1];
		result.M43 = array[3, 2];
		result.M44 = array[3, 3];
		augmentResult.X = array[0, 4];
		augmentResult.Y = array[1, 4];
		augmentResult.Z = array[2, 4];
		augmentResult.W = array[3, 4];
	}

	public static void BillboardLH(ref Vector3 objectPosition, ref Vector3 cameraPosition, ref Vector3 cameraUpVector, ref Vector3 cameraForwardVector, out Matrix result)
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
		result.M14 = 0f;
		result.M21 = result3.X;
		result.M22 = result3.Y;
		result.M23 = result3.Z;
		result.M24 = 0f;
		result.M31 = right.X;
		result.M32 = right.Y;
		result.M33 = right.Z;
		result.M34 = 0f;
		result.M41 = objectPosition.X;
		result.M42 = objectPosition.Y;
		result.M43 = objectPosition.Z;
		result.M44 = 1f;
	}

	public static Matrix BillboardLH(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3 cameraForwardVector)
	{
		BillboardLH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector, out var result);
		return result;
	}

	public static void BillboardRH(ref Vector3 objectPosition, ref Vector3 cameraPosition, ref Vector3 cameraUpVector, ref Vector3 cameraForwardVector, out Matrix result)
	{
		Vector3 right = cameraPosition - objectPosition;
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
		result.M14 = 0f;
		result.M21 = result3.X;
		result.M22 = result3.Y;
		result.M23 = result3.Z;
		result.M24 = 0f;
		result.M31 = right.X;
		result.M32 = right.Y;
		result.M33 = right.Z;
		result.M34 = 0f;
		result.M41 = objectPosition.X;
		result.M42 = objectPosition.Y;
		result.M43 = objectPosition.Z;
		result.M44 = 1f;
	}

	public static Matrix BillboardRH(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3 cameraForwardVector)
	{
		BillboardRH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector, out var result);
		return result;
	}

	public static void LookAtLH(ref Vector3 eye, ref Vector3 target, ref Vector3 up, out Matrix result)
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
		Vector3.Dot(ref result3, ref eye, out result.M41);
		Vector3.Dot(ref result4, ref eye, out result.M42);
		Vector3.Dot(ref result2, ref eye, out result.M43);
		result.M41 = 0f - result.M41;
		result.M42 = 0f - result.M42;
		result.M43 = 0f - result.M43;
	}

	public static Matrix LookAtLH(Vector3 eye, Vector3 target, Vector3 up)
	{
		LookAtLH(ref eye, ref target, ref up, out var result);
		return result;
	}

	public static void LookAtRH(ref Vector3 eye, ref Vector3 target, ref Vector3 up, out Matrix result)
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
		Vector3.Dot(ref result3, ref eye, out result.M41);
		Vector3.Dot(ref result4, ref eye, out result.M42);
		Vector3.Dot(ref result2, ref eye, out result.M43);
		result.M41 = 0f - result.M41;
		result.M42 = 0f - result.M42;
		result.M43 = 0f - result.M43;
	}

	public static Matrix LookAtRH(Vector3 eye, Vector3 target, Vector3 up)
	{
		LookAtRH(ref eye, ref target, ref up, out var result);
		return result;
	}

	public static void OrthoLH(float width, float height, float znear, float zfar, out Matrix result)
	{
		float num = width * 0.5f;
		float num2 = height * 0.5f;
		OrthoOffCenterLH(0f - num, num, 0f - num2, num2, znear, zfar, out result);
	}

	public static Matrix OrthoLH(float width, float height, float znear, float zfar)
	{
		OrthoLH(width, height, znear, zfar, out var result);
		return result;
	}

	public static void OrthoRH(float width, float height, float znear, float zfar, out Matrix result)
	{
		float num = width * 0.5f;
		float num2 = height * 0.5f;
		OrthoOffCenterRH(0f - num, num, 0f - num2, num2, znear, zfar, out result);
	}

	public static Matrix OrthoRH(float width, float height, float znear, float zfar)
	{
		OrthoRH(width, height, znear, zfar, out var result);
		return result;
	}

	public static void OrthoOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix result)
	{
		float num = 1f / (zfar - znear);
		result = Identity;
		result.M11 = 2f / (right - left);
		result.M22 = 2f / (top - bottom);
		result.M33 = num;
		result.M41 = (left + right) / (left - right);
		result.M42 = (top + bottom) / (bottom - top);
		result.M43 = (0f - znear) * num;
	}

	public static Matrix OrthoOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar)
	{
		OrthoOffCenterLH(left, right, bottom, top, znear, zfar, out var result);
		return result;
	}

	public static void OrthoOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix result)
	{
		OrthoOffCenterLH(left, right, bottom, top, znear, zfar, out result);
		result.M33 *= -1f;
	}

	public static Matrix OrthoOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar)
	{
		OrthoOffCenterRH(left, right, bottom, top, znear, zfar, out var result);
		return result;
	}

	public static void PerspectiveLH(float width, float height, float znear, float zfar, out Matrix result)
	{
		float num = width * 0.5f;
		float num2 = height * 0.5f;
		PerspectiveOffCenterLH(0f - num, num, 0f - num2, num2, znear, zfar, out result);
	}

	public static Matrix PerspectiveLH(float width, float height, float znear, float zfar)
	{
		PerspectiveLH(width, height, znear, zfar, out var result);
		return result;
	}

	public static void PerspectiveRH(float width, float height, float znear, float zfar, out Matrix result)
	{
		float num = width * 0.5f;
		float num2 = height * 0.5f;
		PerspectiveOffCenterRH(0f - num, num, 0f - num2, num2, znear, zfar, out result);
	}

	public static Matrix PerspectiveRH(float width, float height, float znear, float zfar)
	{
		PerspectiveRH(width, height, znear, zfar, out var result);
		return result;
	}

	public static void PerspectiveFovLH(float fov, float aspect, float znear, float zfar, out Matrix result)
	{
		float num = (float)(1.0 / Math.Tan(fov * 0.5f));
		float num2 = num / aspect;
		float num3 = znear / num2;
		float num4 = znear / num;
		PerspectiveOffCenterLH(0f - num3, num3, 0f - num4, num4, znear, zfar, out result);
	}

	public static Matrix PerspectiveFovLH(float fov, float aspect, float znear, float zfar)
	{
		PerspectiveFovLH(fov, aspect, znear, zfar, out var result);
		return result;
	}

	public static void PerspectiveFovRH(float fov, float aspect, float znear, float zfar, out Matrix result)
	{
		float num = (float)(1.0 / Math.Tan(fov * 0.5f));
		float num2 = num / aspect;
		float num3 = znear / num2;
		float num4 = znear / num;
		PerspectiveOffCenterRH(0f - num3, num3, 0f - num4, num4, znear, zfar, out result);
	}

	public static Matrix PerspectiveFovRH(float fov, float aspect, float znear, float zfar)
	{
		PerspectiveFovRH(fov, aspect, znear, zfar, out var result);
		return result;
	}

	public static void PerspectiveOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix result)
	{
		float num = zfar / (zfar - znear);
		result = default(Matrix);
		result.M11 = 2f * znear / (right - left);
		result.M22 = 2f * znear / (top - bottom);
		result.M31 = (left + right) / (left - right);
		result.M32 = (top + bottom) / (bottom - top);
		result.M33 = num;
		result.M34 = 1f;
		result.M43 = (0f - znear) * num;
	}

	public static Matrix PerspectiveOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar)
	{
		PerspectiveOffCenterLH(left, right, bottom, top, znear, zfar, out var result);
		return result;
	}

	public static void PerspectiveOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix result)
	{
		PerspectiveOffCenterLH(left, right, bottom, top, znear, zfar, out result);
		result.M31 *= -1f;
		result.M32 *= -1f;
		result.M33 *= -1f;
		result.M34 *= -1f;
	}

	public static Matrix PerspectiveOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar)
	{
		PerspectiveOffCenterRH(left, right, bottom, top, znear, zfar, out var result);
		return result;
	}

	public static void Reflection(ref Plane plane, out Matrix result)
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
		result.M14 = 0f;
		result.M21 = num * y;
		result.M22 = num2 * y + 1f;
		result.M23 = num3 * y;
		result.M24 = 0f;
		result.M31 = num * z;
		result.M32 = num2 * z;
		result.M33 = num3 * z + 1f;
		result.M34 = 0f;
		result.M41 = num * plane.D;
		result.M42 = num2 * plane.D;
		result.M43 = num3 * plane.D;
		result.M44 = 1f;
	}

	public static Matrix Reflection(Plane plane)
	{
		Reflection(ref plane, out var result);
		return result;
	}

	public static void Shadow(ref Vector4 light, ref Plane plane, out Matrix result)
	{
		float num = plane.Normal.X * light.X + plane.Normal.Y * light.Y + plane.Normal.Z * light.Z + plane.D * light.W;
		float num2 = 0f - plane.Normal.X;
		float num3 = 0f - plane.Normal.Y;
		float num4 = 0f - plane.Normal.Z;
		float num5 = 0f - plane.D;
		result.M11 = num2 * light.X + num;
		result.M21 = num3 * light.X;
		result.M31 = num4 * light.X;
		result.M41 = num5 * light.X;
		result.M12 = num2 * light.Y;
		result.M22 = num3 * light.Y + num;
		result.M32 = num4 * light.Y;
		result.M42 = num5 * light.Y;
		result.M13 = num2 * light.Z;
		result.M23 = num3 * light.Z;
		result.M33 = num4 * light.Z + num;
		result.M43 = num5 * light.Z;
		result.M14 = num2 * light.W;
		result.M24 = num3 * light.W;
		result.M34 = num4 * light.W;
		result.M44 = num5 * light.W + num;
	}

	public static Matrix Shadow(Vector4 light, Plane plane)
	{
		Shadow(ref light, ref plane, out var result);
		return result;
	}

	public static void Scaling(ref Vector3 scale, out Matrix result)
	{
		Scaling(scale.X, scale.Y, scale.Z, out result);
	}

	public static Matrix Scaling(Vector3 scale)
	{
		Scaling(ref scale, out var result);
		return result;
	}

	public static void Scaling(float x, float y, float z, out Matrix result)
	{
		result = Identity;
		result.M11 = x;
		result.M22 = y;
		result.M33 = z;
	}

	public static Matrix Scaling(float x, float y, float z)
	{
		Scaling(x, y, z, out var result);
		return result;
	}

	public static void Scaling(float scale, out Matrix result)
	{
		result = Identity;
		result.M11 = (result.M22 = (result.M33 = scale));
	}

	public static Matrix Scaling(float scale)
	{
		Scaling(scale, out var result);
		return result;
	}

	public static void RotationX(float angle, out Matrix result)
	{
		float num = (float)Math.Cos(angle);
		float num2 = (float)Math.Sin(angle);
		result = Identity;
		result.M22 = num;
		result.M23 = num2;
		result.M32 = 0f - num2;
		result.M33 = num;
	}

	public static Matrix RotationX(float angle)
	{
		RotationX(angle, out var result);
		return result;
	}

	public static void RotationY(float angle, out Matrix result)
	{
		float num = (float)Math.Cos(angle);
		float num2 = (float)Math.Sin(angle);
		result = Identity;
		result.M11 = num;
		result.M13 = 0f - num2;
		result.M31 = num2;
		result.M33 = num;
	}

	public static Matrix RotationY(float angle)
	{
		RotationY(angle, out var result);
		return result;
	}

	public static void RotationZ(float angle, out Matrix result)
	{
		float num = (float)Math.Cos(angle);
		float num2 = (float)Math.Sin(angle);
		result = Identity;
		result.M11 = num;
		result.M12 = num2;
		result.M21 = 0f - num2;
		result.M22 = num;
	}

	public static Matrix RotationZ(float angle)
	{
		RotationZ(angle, out var result);
		return result;
	}

	public static void RotationAxis(ref Vector3 axis, float angle, out Matrix result)
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

	public static Matrix RotationAxis(Vector3 axis, float angle)
	{
		RotationAxis(ref axis, angle, out var result);
		return result;
	}

	public static void RotationQuaternion(ref Quaternion rotation, out Matrix result)
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

	public static Matrix RotationQuaternion(Quaternion rotation)
	{
		RotationQuaternion(ref rotation, out var result);
		return result;
	}

	public static void RotationYawPitchRoll(float yaw, float pitch, float roll, out Matrix result)
	{
		Quaternion result2 = default(Quaternion);
		Quaternion.RotationYawPitchRoll(yaw, pitch, roll, out result2);
		RotationQuaternion(ref result2, out result);
	}

	public static Matrix RotationYawPitchRoll(float yaw, float pitch, float roll)
	{
		RotationYawPitchRoll(yaw, pitch, roll, out var result);
		return result;
	}

	public static void Translation(ref Vector3 value, out Matrix result)
	{
		Translation(value.X, value.Y, value.Z, out result);
	}

	public static Matrix Translation(Vector3 value)
	{
		Translation(ref value, out var result);
		return result;
	}

	public static void Translation(float x, float y, float z, out Matrix result)
	{
		result = Identity;
		result.M41 = x;
		result.M42 = y;
		result.M43 = z;
	}

	public static Matrix Translation(float x, float y, float z)
	{
		Translation(x, y, z, out var result);
		return result;
	}

	public static void Skew(float angle, ref Vector3 rotationVec, ref Vector3 transVec, out Matrix matrix)
	{
		float num = 1E-06f;
		Vector3 right = rotationVec;
		Vector3 right2 = Vector3.Normalize(transVec);
		Vector3.Dot(ref rotationVec, ref right2, out var result);
		right += result * right2;
		Vector3.Dot(ref rotationVec, ref right, out var result2);
		float num2 = (float)Math.Cos(angle);
		float num3 = (float)Math.Sin(angle);
		float num4 = result2 * num2 - result * num3;
		float num5 = result2 * num3 + result * num2;
		if (num4 < num)
		{
			throw new ArgumentException("illegal skew angle");
		}
		float num6 = num5 / num4 - result / result2;
		matrix = Identity;
		matrix.M11 = num6 * right2[0] * right[0] + 1f;
		matrix.M12 = num6 * right2[0] * right[1];
		matrix.M13 = num6 * right2[0] * right[2];
		matrix.M21 = num6 * right2[1] * right[0];
		matrix.M22 = num6 * right2[1] * right[1] + 1f;
		matrix.M23 = num6 * right2[1] * right[2];
		matrix.M31 = num6 * right2[2] * right[0];
		matrix.M32 = num6 * right2[2] * right[1];
		matrix.M33 = num6 * right2[2] * right[2] + 1f;
	}

	public static void AffineTransformation(float scaling, ref Quaternion rotation, ref Vector3 translation, out Matrix result)
	{
		result = Scaling(scaling) * RotationQuaternion(rotation) * Translation(translation);
	}

	public static Matrix AffineTransformation(float scaling, Quaternion rotation, Vector3 translation)
	{
		AffineTransformation(scaling, ref rotation, ref translation, out var result);
		return result;
	}

	public static void AffineTransformation(float scaling, ref Vector3 rotationCenter, ref Quaternion rotation, ref Vector3 translation, out Matrix result)
	{
		result = Scaling(scaling) * Translation(-rotationCenter) * RotationQuaternion(rotation) * Translation(rotationCenter) * Translation(translation);
	}

	public static Matrix AffineTransformation(float scaling, Vector3 rotationCenter, Quaternion rotation, Vector3 translation)
	{
		AffineTransformation(scaling, ref rotationCenter, ref rotation, ref translation, out var result);
		return result;
	}

	public static void AffineTransformation2D(float scaling, float rotation, ref Vector2 translation, out Matrix result)
	{
		result = Scaling(scaling, scaling, 1f) * RotationZ(rotation) * Translation((Vector3)translation);
	}

	public static Matrix AffineTransformation2D(float scaling, float rotation, Vector2 translation)
	{
		AffineTransformation2D(scaling, rotation, ref translation, out var result);
		return result;
	}

	public static void AffineTransformation2D(float scaling, ref Vector2 rotationCenter, float rotation, ref Vector2 translation, out Matrix result)
	{
		result = Scaling(scaling, scaling, 1f) * Translation((Vector3)(-rotationCenter)) * RotationZ(rotation) * Translation((Vector3)rotationCenter) * Translation((Vector3)translation);
	}

	public static Matrix AffineTransformation2D(float scaling, Vector2 rotationCenter, float rotation, Vector2 translation)
	{
		AffineTransformation2D(scaling, ref rotationCenter, rotation, ref translation, out var result);
		return result;
	}

	public static void Transformation(ref Vector3 scalingCenter, ref Quaternion scalingRotation, ref Vector3 scaling, ref Vector3 rotationCenter, ref Quaternion rotation, ref Vector3 translation, out Matrix result)
	{
		Matrix matrix = RotationQuaternion(scalingRotation);
		result = Translation(-scalingCenter) * Transpose(matrix) * Scaling(scaling) * matrix * Translation(scalingCenter) * Translation(-rotationCenter) * RotationQuaternion(rotation) * Translation(rotationCenter) * Translation(translation);
	}

	public static Matrix Transformation(Vector3 scalingCenter, Quaternion scalingRotation, Vector3 scaling, Vector3 rotationCenter, Quaternion rotation, Vector3 translation)
	{
		Transformation(ref scalingCenter, ref scalingRotation, ref scaling, ref rotationCenter, ref rotation, ref translation, out var result);
		return result;
	}

	public static void Transformation2D(ref Vector2 scalingCenter, float scalingRotation, ref Vector2 scaling, ref Vector2 rotationCenter, float rotation, ref Vector2 translation, out Matrix result)
	{
		result = Translation((Vector3)(-scalingCenter)) * RotationZ(0f - scalingRotation) * Scaling((Vector3)scaling) * RotationZ(scalingRotation) * Translation((Vector3)scalingCenter) * Translation((Vector3)(-rotationCenter)) * RotationZ(rotation) * Translation((Vector3)rotationCenter) * Translation((Vector3)translation);
		result.M33 = 1f;
		result.M44 = 1f;
	}

	public static Matrix Transformation2D(Vector2 scalingCenter, float scalingRotation, Vector2 scaling, Vector2 rotationCenter, float rotation, Vector2 translation)
	{
		Transformation2D(ref scalingCenter, scalingRotation, ref scaling, ref rotationCenter, rotation, ref translation, out var result);
		return result;
	}

	public static Matrix operator +(Matrix left, Matrix right)
	{
		Add(ref left, ref right, out var result);
		return result;
	}

	public static Matrix operator +(Matrix value)
	{
		return value;
	}

	public static Matrix operator -(Matrix left, Matrix right)
	{
		Subtract(ref left, ref right, out var result);
		return result;
	}

	public static Matrix operator -(Matrix value)
	{
		Negate(ref value, out var result);
		return result;
	}

	public static Matrix operator *(float left, Matrix right)
	{
		Multiply(ref right, left, out var result);
		return result;
	}

	public static Matrix operator *(Matrix left, float right)
	{
		Multiply(ref left, right, out var result);
		return result;
	}

	public static Matrix operator *(Matrix left, Matrix right)
	{
		Multiply(ref left, ref right, out var result);
		return result;
	}

	public static Matrix operator /(Matrix left, float right)
	{
		Divide(ref left, right, out var result);
		return result;
	}

	public static Matrix operator /(Matrix left, Matrix right)
	{
		Divide(ref left, ref right, out var result);
		return result;
	}

	public static bool operator ==(Matrix left, Matrix right)
	{
		return left.Equals(ref right);
	}

	public static bool operator !=(Matrix left, Matrix right)
	{
		return !left.Equals(ref right);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M23:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}]", M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44);
	}

	public string ToString(string format)
	{
		if (format == null)
		{
			return ToString();
		}
		return string.Format(format, CultureInfo.CurrentCulture, "[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M23:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}]", M11.ToString(format, CultureInfo.CurrentCulture), M12.ToString(format, CultureInfo.CurrentCulture), M13.ToString(format, CultureInfo.CurrentCulture), M14.ToString(format, CultureInfo.CurrentCulture), M21.ToString(format, CultureInfo.CurrentCulture), M22.ToString(format, CultureInfo.CurrentCulture), M23.ToString(format, CultureInfo.CurrentCulture), M24.ToString(format, CultureInfo.CurrentCulture), M31.ToString(format, CultureInfo.CurrentCulture), M32.ToString(format, CultureInfo.CurrentCulture), M33.ToString(format, CultureInfo.CurrentCulture), M34.ToString(format, CultureInfo.CurrentCulture), M41.ToString(format, CultureInfo.CurrentCulture), M42.ToString(format, CultureInfo.CurrentCulture), M43.ToString(format, CultureInfo.CurrentCulture), M44.ToString(format, CultureInfo.CurrentCulture));
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M23:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}]", M11.ToString(formatProvider), M12.ToString(formatProvider), M13.ToString(formatProvider), M14.ToString(formatProvider), M21.ToString(formatProvider), M22.ToString(formatProvider), M23.ToString(formatProvider), M24.ToString(formatProvider), M31.ToString(formatProvider), M32.ToString(formatProvider), M33.ToString(formatProvider), M34.ToString(formatProvider), M41.ToString(formatProvider), M42.ToString(formatProvider), M43.ToString(formatProvider), M44.ToString(formatProvider));
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			return ToString(formatProvider);
		}
		return string.Format(format, formatProvider, "[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M23:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}]", M11.ToString(format, formatProvider), M12.ToString(format, formatProvider), M13.ToString(format, formatProvider), M14.ToString(format, formatProvider), M21.ToString(format, formatProvider), M22.ToString(format, formatProvider), M23.ToString(format, formatProvider), M24.ToString(format, formatProvider), M31.ToString(format, formatProvider), M32.ToString(format, formatProvider), M33.ToString(format, formatProvider), M34.ToString(format, formatProvider), M41.ToString(format, formatProvider), M42.ToString(format, formatProvider), M43.ToString(format, formatProvider), M44.ToString(format, formatProvider));
	}

	public override int GetHashCode()
	{
		int hashCode = M11.GetHashCode();
		hashCode = (hashCode * 397) ^ M12.GetHashCode();
		hashCode = (hashCode * 397) ^ M13.GetHashCode();
		hashCode = (hashCode * 397) ^ M14.GetHashCode();
		hashCode = (hashCode * 397) ^ M21.GetHashCode();
		hashCode = (hashCode * 397) ^ M22.GetHashCode();
		hashCode = (hashCode * 397) ^ M23.GetHashCode();
		hashCode = (hashCode * 397) ^ M24.GetHashCode();
		hashCode = (hashCode * 397) ^ M31.GetHashCode();
		hashCode = (hashCode * 397) ^ M32.GetHashCode();
		hashCode = (hashCode * 397) ^ M33.GetHashCode();
		hashCode = (hashCode * 397) ^ M34.GetHashCode();
		hashCode = (hashCode * 397) ^ M41.GetHashCode();
		hashCode = (hashCode * 397) ^ M42.GetHashCode();
		hashCode = (hashCode * 397) ^ M43.GetHashCode();
		return (hashCode * 397) ^ M44.GetHashCode();
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(M11);
			serializer.Writer.Write(M12);
			serializer.Writer.Write(M13);
			serializer.Writer.Write(M14);
			serializer.Writer.Write(M21);
			serializer.Writer.Write(M22);
			serializer.Writer.Write(M23);
			serializer.Writer.Write(M24);
			serializer.Writer.Write(M31);
			serializer.Writer.Write(M32);
			serializer.Writer.Write(M33);
			serializer.Writer.Write(M34);
			serializer.Writer.Write(M41);
			serializer.Writer.Write(M42);
			serializer.Writer.Write(M43);
			serializer.Writer.Write(M44);
		}
		else
		{
			M11 = serializer.Reader.ReadSingle();
			M12 = serializer.Reader.ReadSingle();
			M13 = serializer.Reader.ReadSingle();
			M14 = serializer.Reader.ReadSingle();
			M21 = serializer.Reader.ReadSingle();
			M22 = serializer.Reader.ReadSingle();
			M23 = serializer.Reader.ReadSingle();
			M24 = serializer.Reader.ReadSingle();
			M31 = serializer.Reader.ReadSingle();
			M32 = serializer.Reader.ReadSingle();
			M33 = serializer.Reader.ReadSingle();
			M34 = serializer.Reader.ReadSingle();
			M41 = serializer.Reader.ReadSingle();
			M42 = serializer.Reader.ReadSingle();
			M43 = serializer.Reader.ReadSingle();
			M44 = serializer.Reader.ReadSingle();
		}
	}

	public bool Equals(ref Matrix other)
	{
		if (MathUtil.NearEqual(other.M11, M11) && MathUtil.NearEqual(other.M12, M12) && MathUtil.NearEqual(other.M13, M13) && MathUtil.NearEqual(other.M14, M14) && MathUtil.NearEqual(other.M21, M21) && MathUtil.NearEqual(other.M22, M22) && MathUtil.NearEqual(other.M23, M23) && MathUtil.NearEqual(other.M24, M24) && MathUtil.NearEqual(other.M31, M31) && MathUtil.NearEqual(other.M32, M32) && MathUtil.NearEqual(other.M33, M33) && MathUtil.NearEqual(other.M34, M34) && MathUtil.NearEqual(other.M41, M41) && MathUtil.NearEqual(other.M42, M42) && MathUtil.NearEqual(other.M43, M43))
		{
			return MathUtil.NearEqual(other.M44, M44);
		}
		return false;
	}

	public bool Equals(Matrix other)
	{
		return Equals(ref other);
	}

	public override bool Equals(object value)
	{
		if (!(value is Matrix other))
		{
			return false;
		}
		return Equals(ref other);
	}

	static Matrix()
	{
		SizeInBytes = Marshal.SizeOf(typeof(Matrix));
		Zero = default(Matrix);
		Identity = new Matrix
		{
			M11 = 1f,
			M22 = 1f,
			M33 = 1f,
			M44 = 1f
		};
	}
}

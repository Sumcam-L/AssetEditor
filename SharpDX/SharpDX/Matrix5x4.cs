using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Matrix5x4 : IEquatable<Matrix5x4>, IFormattable, IDataSerializable
{
	public static readonly int SizeInBytes;

	public static readonly Matrix5x4 Zero;

	public static readonly Matrix5x4 Identity;

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

	public float M51;

	public float M52;

	public float M53;

	public float M54;

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

	public Vector4 Row5
	{
		get
		{
			return new Vector4(M51, M52, M53, M54);
		}
		set
		{
			M51 = value.X;
			M52 = value.Y;
			M53 = value.Z;
			M54 = value.W;
		}
	}

	public Vector4 TranslationVector
	{
		get
		{
			return new Vector4(M51, M52, M53, M54);
		}
		set
		{
			M51 = value.X;
			M52 = value.Y;
			M53 = value.Z;
			M54 = value.W;
		}
	}

	public Vector4 ScaleVector
	{
		get
		{
			return new Vector4(M11, M22, M33, M44);
		}
		set
		{
			M11 = value.X;
			M22 = value.Y;
			M33 = value.Z;
			M44 = value.W;
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
				16 => M51, 
				17 => M52, 
				18 => M53, 
				19 => M54, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Matrix5x4 run from 0 to 19, inclusive."), 
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
			case 16:
				M51 = value;
				break;
			case 17:
				M52 = value;
				break;
			case 18:
				M53 = value;
				break;
			case 19:
				M54 = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for Matrix5x4 run from 0 to 19, inclusive.");
			}
		}
	}

	public float this[int row, int column]
	{
		get
		{
			if (row < 0 || row > 4)
			{
				throw new ArgumentOutOfRangeException("row", "Rows for matrices run from 0 to 4, inclusive.");
			}
			if (column < 0 || column > 3)
			{
				throw new ArgumentOutOfRangeException("column", "Columns for matrices run from 0 to 3, inclusive.");
			}
			return this[row * 4 + column];
		}
		set
		{
			if (row < 0 || row > 4)
			{
				throw new ArgumentOutOfRangeException("row", "Rows for matrices run from 0 to 4, inclusive.");
			}
			if (column < 0 || column > 3)
			{
				throw new ArgumentOutOfRangeException("column", "Columns for matrices run from 0 to 3, inclusive.");
			}
			this[row * 4 + column] = value;
		}
	}

	public Matrix5x4(float value)
	{
		M11 = (M12 = (M13 = (M14 = (M21 = (M22 = (M23 = (M24 = (M31 = (M32 = (M33 = (M34 = (M41 = (M42 = (M43 = (M44 = (M51 = (M52 = (M53 = (M54 = value)))))))))))))))))));
	}

	public Matrix5x4(float M11, float M12, float M13, float M14, float M21, float M22, float M23, float M24, float M31, float M32, float M33, float M34, float M41, float M42, float M43, float M44, float M51, float M52, float M53, float M54)
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
		this.M51 = M51;
		this.M52 = M52;
		this.M53 = M53;
		this.M54 = M54;
	}

	public Matrix5x4(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 20)
		{
			throw new ArgumentOutOfRangeException("values", "There must be 20 input values for Matrix5x4.");
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
		M51 = values[16];
		M52 = values[17];
		M53 = values[18];
		M54 = values[19];
	}

	public static void Add(ref Matrix5x4 left, ref Matrix5x4 right, out Matrix5x4 result)
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
		result.M51 = left.M51 + right.M51;
		result.M52 = left.M52 + right.M52;
		result.M53 = left.M53 + right.M53;
		result.M54 = left.M54 + right.M54;
	}

	public static Matrix5x4 Add(Matrix5x4 left, Matrix5x4 right)
	{
		Add(ref left, ref right, out var result);
		return result;
	}

	public static void Subtract(ref Matrix5x4 left, ref Matrix5x4 right, out Matrix5x4 result)
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
		result.M51 = left.M51 - right.M51;
		result.M52 = left.M52 - right.M52;
		result.M53 = left.M53 - right.M53;
		result.M54 = left.M54 - right.M54;
	}

	public static Matrix5x4 Subtract(Matrix5x4 left, Matrix5x4 right)
	{
		Subtract(ref left, ref right, out var result);
		return result;
	}

	public static void Multiply(ref Matrix5x4 left, float right, out Matrix5x4 result)
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
		result.M51 = left.M51 * right;
		result.M52 = left.M52 * right;
		result.M53 = left.M53 * right;
		result.M54 = left.M54 * right;
	}

	public static void Divide(ref Matrix5x4 left, float right, out Matrix5x4 result)
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
		result.M51 = left.M51 * num;
		result.M52 = left.M52 * num;
		result.M53 = left.M53 * num;
		result.M54 = left.M54 * num;
	}

	public static void Negate(ref Matrix5x4 value, out Matrix5x4 result)
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
		result.M51 = 0f - value.M51;
		result.M52 = 0f - value.M52;
		result.M53 = 0f - value.M53;
		result.M54 = 0f - value.M54;
	}

	public static Matrix5x4 Negate(Matrix5x4 value)
	{
		Negate(ref value, out var result);
		return result;
	}

	public static void Lerp(ref Matrix5x4 start, ref Matrix5x4 end, float amount, out Matrix5x4 result)
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
		result.M51 = MathUtil.Lerp(start.M51, end.M51, amount);
		result.M52 = MathUtil.Lerp(start.M52, end.M52, amount);
		result.M53 = MathUtil.Lerp(start.M53, end.M53, amount);
		result.M54 = MathUtil.Lerp(start.M54, end.M54, amount);
	}

	public static Matrix5x4 Lerp(Matrix5x4 start, Matrix5x4 end, float amount)
	{
		Lerp(ref start, ref end, amount, out var result);
		return result;
	}

	public static void SmoothStep(ref Matrix5x4 start, ref Matrix5x4 end, float amount, out Matrix5x4 result)
	{
		amount = MathUtil.SmoothStep(amount);
		Lerp(ref start, ref end, amount, out result);
	}

	public static Matrix5x4 SmoothStep(Matrix5x4 start, Matrix5x4 end, float amount)
	{
		SmoothStep(ref start, ref end, amount, out var result);
		return result;
	}

	public static void Scaling(ref Vector4 scale, out Matrix5x4 result)
	{
		Scaling(scale.X, scale.Y, scale.Z, scale.W, out result);
	}

	public static Matrix5x4 Scaling(Vector4 scale)
	{
		Scaling(ref scale, out var result);
		return result;
	}

	public static void Scaling(float x, float y, float z, float w, out Matrix5x4 result)
	{
		result = Identity;
		result.M11 = x;
		result.M22 = y;
		result.M33 = z;
		result.M44 = w;
	}

	public static Matrix5x4 Scaling(float x, float y, float z, float w)
	{
		Scaling(x, y, z, w, out var result);
		return result;
	}

	public static void Scaling(float scale, out Matrix5x4 result)
	{
		result = Identity;
		result.M11 = (result.M22 = (result.M33 = (result.M44 = scale)));
	}

	public static Matrix5x4 Scaling(float scale)
	{
		Scaling(scale, out var result);
		return result;
	}

	public static void Translation(ref Vector4 value, out Matrix5x4 result)
	{
		Translation(value.X, value.Y, value.Z, value.W, out result);
	}

	public static Matrix5x4 Translation(Vector4 value)
	{
		Translation(ref value, out var result);
		return result;
	}

	public static void Translation(float x, float y, float z, float w, out Matrix5x4 result)
	{
		result = Identity;
		result.M51 = x;
		result.M52 = y;
		result.M53 = z;
		result.M54 = w;
	}

	public static Matrix5x4 Translation(float x, float y, float z, float w)
	{
		Translation(x, y, z, w, out var result);
		return result;
	}

	public static Matrix5x4 operator +(Matrix5x4 left, Matrix5x4 right)
	{
		Add(ref left, ref right, out var result);
		return result;
	}

	public static Matrix5x4 operator +(Matrix5x4 value)
	{
		return value;
	}

	public static Matrix5x4 operator -(Matrix5x4 left, Matrix5x4 right)
	{
		Subtract(ref left, ref right, out var result);
		return result;
	}

	public static Matrix5x4 operator -(Matrix5x4 value)
	{
		Negate(ref value, out var result);
		return result;
	}

	public static Matrix5x4 operator *(float left, Matrix5x4 right)
	{
		Multiply(ref right, left, out var result);
		return result;
	}

	public static Matrix5x4 operator *(Matrix5x4 left, float right)
	{
		Multiply(ref left, right, out var result);
		return result;
	}

	public static Matrix5x4 operator /(Matrix5x4 left, float right)
	{
		Divide(ref left, right, out var result);
		return result;
	}

	public static bool operator ==(Matrix5x4 left, Matrix5x4 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Matrix5x4 left, Matrix5x4 right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M3:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}] [M51:{16} M52:{17} M53:{18} M54:{19}]", M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44, M51, M52, M53, M54);
	}

	public string ToString(string format)
	{
		if (format == null)
		{
			return ToString();
		}
		return string.Format(format, CultureInfo.CurrentCulture, "[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M3:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}] [M51:{16} M52:{17} M53:{18} M54:{19}]", M11.ToString(format, CultureInfo.CurrentCulture), M12.ToString(format, CultureInfo.CurrentCulture), M13.ToString(format, CultureInfo.CurrentCulture), M14.ToString(format, CultureInfo.CurrentCulture), M21.ToString(format, CultureInfo.CurrentCulture), M22.ToString(format, CultureInfo.CurrentCulture), M23.ToString(format, CultureInfo.CurrentCulture), M24.ToString(format, CultureInfo.CurrentCulture), M31.ToString(format, CultureInfo.CurrentCulture), M32.ToString(format, CultureInfo.CurrentCulture), M33.ToString(format, CultureInfo.CurrentCulture), M34.ToString(format, CultureInfo.CurrentCulture), M41.ToString(format, CultureInfo.CurrentCulture), M42.ToString(format, CultureInfo.CurrentCulture), M43.ToString(format, CultureInfo.CurrentCulture), M44.ToString(format, CultureInfo.CurrentCulture), M51.ToString(format, CultureInfo.CurrentCulture), M52.ToString(format, CultureInfo.CurrentCulture), M53.ToString(format, CultureInfo.CurrentCulture), M54.ToString(format, CultureInfo.CurrentCulture));
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M3:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}] [M51:{16} M52:{17} M53:{18} M54:{19}]", M11.ToString(formatProvider), M12.ToString(formatProvider), M13.ToString(formatProvider), M14.ToString(formatProvider), M21.ToString(formatProvider), M22.ToString(formatProvider), M23.ToString(formatProvider), M24.ToString(formatProvider), M31.ToString(formatProvider), M32.ToString(formatProvider), M33.ToString(formatProvider), M34.ToString(formatProvider), M41.ToString(formatProvider), M42.ToString(formatProvider), M43.ToString(formatProvider), M44.ToString(formatProvider), M51.ToString(formatProvider), M52.ToString(formatProvider), M53.ToString(formatProvider), M54.ToString(formatProvider));
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			return ToString(formatProvider);
		}
		return string.Format(format, formatProvider, "[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M3:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}] [M51:{16} M52:{17} M53:{18} M54:{19}]", M11.ToString(format, formatProvider), M12.ToString(format, formatProvider), M13.ToString(format, formatProvider), M14.ToString(format, formatProvider), M21.ToString(format, formatProvider), M22.ToString(format, formatProvider), M23.ToString(format, formatProvider), M24.ToString(format, formatProvider), M31.ToString(format, formatProvider), M32.ToString(format, formatProvider), M33.ToString(format, formatProvider), M34.ToString(format, formatProvider), M41.ToString(format, formatProvider), M42.ToString(format, formatProvider), M43.ToString(format, formatProvider), M44.ToString(format, formatProvider), M51.ToString(format, formatProvider), M52.ToString(format, formatProvider), M53.ToString(format, formatProvider), M54.ToString(format, formatProvider));
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
		hashCode = (hashCode * 397) ^ M44.GetHashCode();
		hashCode = (hashCode * 397) ^ M51.GetHashCode();
		hashCode = (hashCode * 397) ^ M52.GetHashCode();
		hashCode = (hashCode * 397) ^ M53.GetHashCode();
		return (hashCode * 397) ^ M54.GetHashCode();
	}

	public bool Equals(Matrix5x4 other)
	{
		if (MathUtil.NearEqual(other.M11, M11) && MathUtil.NearEqual(other.M12, M12) && MathUtil.NearEqual(other.M13, M13) && MathUtil.NearEqual(other.M14, M14) && MathUtil.NearEqual(other.M21, M21) && MathUtil.NearEqual(other.M22, M22) && MathUtil.NearEqual(other.M23, M23) && MathUtil.NearEqual(other.M24, M24) && MathUtil.NearEqual(other.M31, M31) && MathUtil.NearEqual(other.M32, M32) && MathUtil.NearEqual(other.M33, M33) && MathUtil.NearEqual(other.M34, M34) && MathUtil.NearEqual(other.M41, M41) && MathUtil.NearEqual(other.M42, M42) && MathUtil.NearEqual(other.M43, M43) && MathUtil.NearEqual(other.M44, M44) && MathUtil.NearEqual(other.M51, M51) && MathUtil.NearEqual(other.M52, M52) && MathUtil.NearEqual(other.M53, M53))
		{
			return MathUtil.NearEqual(other.M54, M54);
		}
		return false;
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(value.GetType(), typeof(Matrix5x4)))
		{
			return false;
		}
		return Equals((Matrix5x4)value);
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
			serializer.Writer.Write(M51);
			serializer.Writer.Write(M52);
			serializer.Writer.Write(M53);
			serializer.Writer.Write(M54);
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
			M51 = serializer.Reader.ReadSingle();
			M52 = serializer.Reader.ReadSingle();
			M53 = serializer.Reader.ReadSingle();
			M54 = serializer.Reader.ReadSingle();
		}
	}

	static Matrix5x4()
	{
		SizeInBytes = Marshal.SizeOf(typeof(Matrix5x4));
		Zero = default(Matrix5x4);
		Identity = new Matrix5x4
		{
			M11 = 1f,
			M22 = 1f,
			M33 = 1f,
			M44 = 1f,
			M54 = 0f
		};
	}
}

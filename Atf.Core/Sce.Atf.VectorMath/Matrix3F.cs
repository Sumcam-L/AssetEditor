using System;

namespace Sce.Atf.VectorMath;

public class Matrix3F : IEquatable<Matrix3F>, IFormattable
{
	public float M11;

	public float M12;

	public float M13;

	public float M21;

	public float M22;

	public float M23;

	public float M31;

	public float M32;

	public float M33;

	public static readonly Matrix3F Identity = new Matrix3F();

	public static readonly Matrix3F Zero = new Matrix3F(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

	private static readonly Matrix3F s_rotationTemp = new Matrix3F();

	private const double EPS = 1E-06;

	public double Determinant => M11 * (M22 * M33 - M23 * M32) + M12 * (M23 * M31 - M21 * M33) + M13 * (M21 * M32 - M22 * M31);

	public float this[int i]
	{
		get
		{
			return i switch
			{
				0 => M11, 
				1 => M12, 
				2 => M13, 
				3 => M21, 
				4 => M22, 
				5 => M23, 
				6 => M31, 
				7 => M32, 
				8 => M33, 
				_ => throw new IndexOutOfRangeException(), 
			};
		}
		set
		{
			switch (i)
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
				M21 = value;
				break;
			case 4:
				M22 = value;
				break;
			case 5:
				M23 = value;
				break;
			case 6:
				M31 = value;
				break;
			case 7:
				M32 = value;
				break;
			case 8:
				M33 = value;
				break;
			default:
				throw new IndexOutOfRangeException();
			}
		}
	}

	public float this[int i, int j]
	{
		get
		{
			return this[i * 3 + j];
		}
		set
		{
			this[i * 3 + j] = value;
		}
	}

	public Vec3F XAxis
	{
		get
		{
			return new Vec3F(M11, M12, M13);
		}
		set
		{
			M11 = value.X;
			M12 = value.Y;
			M13 = value.Z;
		}
	}

	public Vec3F YAxis
	{
		get
		{
			return new Vec3F(M21, M22, M23);
		}
		set
		{
			M21 = value.X;
			M22 = value.Y;
			M23 = value.Z;
		}
	}

	public Vec3F ZAxis
	{
		get
		{
			return new Vec3F(M31, M32, M33);
		}
		set
		{
			M31 = value.X;
			M32 = value.Y;
			M33 = value.Z;
		}
	}

	public Matrix3F()
	{
		M11 = 1f;
		M12 = 0f;
		M13 = 0f;
		M21 = 0f;
		M22 = 1f;
		M23 = 0f;
		M31 = 0f;
		M32 = 0f;
		M33 = 1f;
	}

	public Matrix3F(float[] m)
	{
		M11 = m[0];
		M12 = m[1];
		M13 = m[2];
		M21 = m[3];
		M22 = m[4];
		M23 = m[5];
		M31 = m[6];
		M32 = m[7];
		M33 = m[8];
	}

	public Matrix3F(Matrix3F m)
	{
		M11 = m.M11;
		M12 = m.M12;
		M13 = m.M13;
		M21 = m.M21;
		M22 = m.M22;
		M23 = m.M23;
		M31 = m.M31;
		M32 = m.M32;
		M33 = m.M33;
	}

	public Matrix3F(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
	{
		M11 = m00;
		M12 = m01;
		M13 = m02;
		M21 = m10;
		M22 = m11;
		M23 = m12;
		M31 = m20;
		M32 = m21;
		M33 = m22;
	}

	public void CopyTo(float[] array, int index)
	{
		array[index++] = M11;
		array[index++] = M12;
		array[index++] = M13;
		array[index++] = M21;
		array[index++] = M22;
		array[index++] = M23;
		array[index++] = M31;
		array[index++] = M32;
		array[index++] = M33;
	}

	public float[] ToArray()
	{
		return new float[9] { M11, M12, M13, M21, M22, M23, M31, M32, M33 };
	}

	public void Set(Matrix3F m)
	{
		M11 = m.M11;
		M12 = m.M12;
		M13 = m.M13;
		M21 = m.M21;
		M22 = m.M22;
		M23 = m.M23;
		M31 = m.M31;
		M32 = m.M32;
		M33 = m.M33;
	}

	public void Set(QuatF q)
	{
		M11 = (float)(1.0 - 2.0 * (double)q.Y * (double)q.Y - 2.0 * (double)q.Z * (double)q.Z);
		M21 = (float)(2.0 * (double)(q.X * q.Y + q.W * q.Z));
		M31 = (float)(2.0 * (double)(q.X * q.Z - q.W * q.Y));
		M12 = (float)(2.0 * (double)(q.X * q.Y - q.W * q.Z));
		M22 = (float)(1.0 - 2.0 * (double)q.X * (double)q.X - 2.0 * (double)q.Z * (double)q.Z);
		M32 = (float)(2.0 * (double)(q.Y * q.Z + q.W * q.X));
		M13 = (float)(2.0 * (double)(q.X * q.Z + q.W * q.Y));
		M23 = (float)(2.0 * (double)(q.Y * q.Z - q.W * q.X));
		M33 = (float)(1.0 - 2.0 * (double)q.X * (double)q.X - 2.0 * (double)q.Y * (double)q.Y);
	}

	public void Set(AngleAxisF a)
	{
		double num = Math.Sqrt(a.Axis.X * a.Axis.X + a.Axis.Y * a.Axis.Y + a.Axis.Z * a.Axis.Z);
		if (num < 1E-06)
		{
			Set(Identity);
			return;
		}
		double num2 = 1.0 / num;
		double num3 = (double)a.Axis.X * num2;
		double num4 = (double)a.Axis.Y * num2;
		double num5 = (double)a.Axis.Z * num2;
		double num6 = Math.Sin(a.Angle);
		double num7 = Math.Cos(a.Angle);
		double num8 = 1.0 - num7;
		double num9 = a.Axis.X * a.Axis.Z;
		double num10 = a.Axis.X * a.Axis.Y;
		double num11 = a.Axis.Y * a.Axis.Z;
		M11 = (float)(num8 * num3 * num3 + num7);
		M12 = (float)(num8 * num10 - num6 * num5);
		M13 = (float)(num8 * num9 + num6 * num4);
		M21 = (float)(num8 * num10 + num6 * num5);
		M22 = (float)(num8 * num4 * num4 + num7);
		M23 = (float)(num8 * num11 - num6 * num3);
		M31 = (float)(num8 * num9 - num6 * num4);
		M32 = (float)(num8 * num11 + num6 * num3);
		M33 = (float)(num8 * num5 * num5 + num7);
	}

	public void UniformScale(double scale)
	{
		M11 = (float)scale;
		M12 = 0f;
		M13 = 0f;
		M21 = 0f;
		M22 = (float)scale;
		M23 = 0f;
		M31 = 0f;
		M32 = 0f;
		M33 = (float)scale;
	}

	public void Scale(Vec3F scale)
	{
		M12 = (M13 = (M21 = (M23 = (M31 = (M32 = 0f)))));
		M11 = scale.X;
		M22 = scale.Y;
		M33 = scale.Z;
	}

	public Vec3F GetScale()
	{
		return new Vec3F(XAxis.Length, YAxis.Length, ZAxis.Length);
	}

	public void Rotation(Vec3F rotation)
	{
		RotX(rotation.X);
		s_rotationTemp.RotY(rotation.Y);
		Mul(this, s_rotationTemp);
		s_rotationTemp.RotZ(rotation.Z);
		Mul(this, s_rotationTemp);
	}

	public void RotX(double angle)
	{
		double num = Math.Sin(angle);
		double num2 = Math.Cos(angle);
		M11 = 1f;
		M12 = 0f;
		M13 = 0f;
		M21 = 0f;
		M22 = (float)num2;
		M23 = (float)num;
		M31 = 0f;
		M32 = (float)(0.0 - num);
		M33 = (float)num2;
	}

	public void RotY(double angle)
	{
		double num = Math.Sin(angle);
		double num2 = Math.Cos(angle);
		M11 = (float)num2;
		M12 = 0f;
		M13 = (float)(0.0 - num);
		M21 = 0f;
		M22 = 1f;
		M23 = 0f;
		M31 = (float)num;
		M32 = 0f;
		M33 = (float)num2;
	}

	public void RotZ(double angle)
	{
		double num = Math.Sin(angle);
		double num2 = Math.Cos(angle);
		M11 = (float)num2;
		M12 = (float)num;
		M13 = 0f;
		M21 = (float)(0.0 - num);
		M22 = (float)num2;
		M23 = 0f;
		M31 = 0f;
		M32 = 0f;
		M33 = 1f;
	}

	public void Mul(Matrix3F m, float scale)
	{
		M11 = scale * m.M11;
		M12 = scale * m.M12;
		M13 = scale * m.M13;
		M21 = scale * m.M21;
		M22 = scale * m.M22;
		M23 = scale * m.M23;
		M31 = scale * m.M31;
		M32 = scale * m.M32;
		M33 = scale * m.M33;
	}

	public void Mul(Matrix3F m1, Matrix3F m2)
	{
		float m3 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31;
		float m4 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32;
		float m5 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33;
		float m6 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31;
		float m7 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32;
		float m8 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33;
		float m9 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31;
		float m10 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32;
		float m11 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33;
		M11 = m3;
		M12 = m4;
		M13 = m5;
		M21 = m6;
		M22 = m7;
		M23 = m8;
		M31 = m9;
		M32 = m10;
		M33 = m11;
	}

	public void Invert(Matrix3F m)
	{
		float m2 = (0f - m.M23) * m.M32 + m.M22 * m.M33;
		float m3 = m.M13 * m.M32 - m.M12 * m.M33;
		float m4 = (0f - m.M13) * m.M22 + m.M12 * m.M23;
		float m5 = m.M23 * m.M31 - m.M21 * m.M33;
		float m6 = (0f - m.M13) * m.M31 + m.M11 * m.M33;
		float m7 = m.M13 * m.M21 - m.M11 * m.M23;
		float m8 = (0f - m.M22) * m.M31 + m.M21 * m.M32;
		float m9 = m.M12 * m.M31 - m.M11 * m.M32;
		float m10 = (0f - m.M12) * m.M21 + m.M11 * m.M22;
		float scale = (float)(1.0 / m.Determinant);
		M11 = m2;
		M12 = m3;
		M13 = m4;
		M21 = m5;
		M22 = m6;
		M23 = m7;
		M31 = m8;
		M32 = m9;
		M33 = m10;
		Mul(this, scale);
	}

	public void Transpose(Matrix3F m)
	{
		float m2 = m.M21;
		M21 = m.M12;
		M12 = m2;
		m2 = m.M31;
		M31 = m.M13;
		M13 = m2;
		m2 = m.M32;
		M32 = m.M23;
		M23 = m2;
	}

	public void Normalize(Matrix3F m)
	{
		double num = 1.0 / Math.Sqrt(m.M11 * m.M11 + m.M21 * m.M21 + m.M31 * m.M31);
		M11 = (float)((double)m.M11 * num);
		M21 = (float)((double)m.M21 * num);
		M31 = (float)((double)m.M31 * num);
		num = 1.0 / Math.Sqrt(m.M12 * m.M12 + m.M22 * m.M22 + m.M32 * m.M32);
		M12 = (float)((double)m.M12 * num);
		M22 = (float)((double)m.M22 * num);
		M32 = (float)((double)m.M32 * num);
		M13 = M21 * M32 - M22 * M31;
		M23 = M12 * M31 - M11 * M32;
		M33 = M11 * M22 - M12 * M21;
	}

	public void Negate(Matrix3F m)
	{
		M11 = 0f - m.M11;
		M12 = 0f - m.M12;
		M13 = 0f - m.M13;
		M21 = 0f - m.M21;
		M22 = 0f - m.M22;
		M23 = 0f - m.M23;
		M31 = 0f - m.M31;
		M32 = 0f - m.M32;
		M33 = 0f - m.M33;
	}

	public void Transform(Vec3F v, out Vec3F result)
	{
		float x = M11 * v.X + M12 * v.Y + M13 * v.Z;
		float y = M21 * v.X + M22 * v.Y + M23 * v.Z;
		float z = M31 * v.X + M32 * v.Y + M33 * v.Z;
		result.X = x;
		result.Y = y;
		result.Z = z;
	}

	public void GetEulerAngles(out float x, out float y, out float z)
	{
		Vec3F vec3F = new Vec3F(M11 * M11, M12 * M12, M13 * M13);
		float num = (float)Math.Sqrt(vec3F.X + vec3F.Y);
		if (num > 1E-06f)
		{
			x = (float)Math.Atan2(M23, M33);
			y = (float)Math.Atan2(0f - M13, num);
			z = (float)Math.Atan2(M12, M11);
		}
		else
		{
			x = (float)Math.Atan2(0f - M33, M22);
			y = (float)Math.Atan2(0f - M13, num);
			z = 0f;
		}
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		string numberListSeparator = StringUtil.GetNumberListSeparator(formatProvider);
		if (format == null)
		{
			format = "R";
		}
		return string.Format("{0}{9} {1}{9} {2}{9} {3}{9} {4}{9} {5}{9} {6}{9} {7}{9} {8}", M11.ToString(format, formatProvider), M12.ToString(format, formatProvider), M13.ToString(format, formatProvider), M21.ToString(format, formatProvider), M22.ToString(format, formatProvider), M23.ToString(format, formatProvider), M31.ToString(format, formatProvider), M32.ToString(format, formatProvider), M33.ToString(format, formatProvider), numberListSeparator);
	}

	public bool Equals(Matrix3F m)
	{
		return M11 == m.M11 && M12 == m.M12 && M13 == m.M13 && M21 == m.M21 && M22 == m.M22 && M23 == m.M23 && M31 == m.M31 && M32 == m.M32 && M33 == m.M33;
	}

	public override bool Equals(object obj)
	{
		if (obj is Matrix3F)
		{
			Matrix3F m = (Matrix3F)obj;
			return Equals(m);
		}
		return false;
	}

	public bool EpsilonEquals(Matrix3F m, double eps)
	{
		return (double)Math.Abs(M11 - m.M11) < eps && (double)Math.Abs(M12 - m.M12) < eps && (double)Math.Abs(M13 - m.M13) < eps && (double)Math.Abs(M21 - m.M21) < eps && (double)Math.Abs(M22 - m.M22) < eps && (double)Math.Abs(M23 - m.M23) < eps && (double)Math.Abs(M31 - m.M31) < eps && (double)Math.Abs(M32 - m.M32) < eps && (double)Math.Abs(M33 - m.M33) < eps;
	}

	public override int GetHashCode()
	{
		long num = 1L;
		num = 31 * num + M11.GetHashCode();
		num = 31 * num + M12.GetHashCode();
		num = 31 * num + M13.GetHashCode();
		num = 31 * num + M21.GetHashCode();
		num = 31 * num + M22.GetHashCode();
		num = 31 * num + M23.GetHashCode();
		num = 31 * num + M31.GetHashCode();
		num = 31 * num + M32.GetHashCode();
		num = 31 * num + M33.GetHashCode();
		return (int)(num ^ (num >> 32));
	}
}

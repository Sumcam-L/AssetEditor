using System;

namespace Sce.Atf.VectorMath;

public class Matrix4F : IEquatable<Matrix4F>, IFormattable
{
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

	private static readonly Matrix4F s_rotationTemp = new Matrix4F();

	private static readonly Matrix4F s_mulTemp = new Matrix4F();

	private const double EPS = 1E-06;

	public static Matrix4F Identity => new Matrix4F();

	public static Matrix4F Zero => new Matrix4F(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

	public float this[int i]
	{
		get
		{
			return i switch
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
				throw new IndexOutOfRangeException();
			}
		}
	}

	public float this[int i, int j]
	{
		get
		{
			return this[i * 4 + j];
		}
		set
		{
			this[i * 4 + j] = value;
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

	public Vec3F Translation
	{
		get
		{
			return new Vec3F(M41, M42, M43);
		}
		set
		{
			M41 = value.X;
			M42 = value.Y;
			M43 = value.Z;
		}
	}

	public float XTranslation
	{
		get
		{
			return M41;
		}
		set
		{
			M41 = value;
		}
	}

	public float YTranslation
	{
		get
		{
			return M42;
		}
		set
		{
			M42 = value;
		}
	}

	public float ZTranslation
	{
		get
		{
			return M43;
		}
		set
		{
			M43 = value;
		}
	}

	public double Determinant => M14 * M23 * M32 * M41 - M13 * M24 * M32 * M41 - M14 * M22 * M33 * M41 + M12 * M24 * M33 * M41 + M13 * M22 * M34 * M41 - M12 * M23 * M34 * M41 - M14 * M23 * M31 * M42 + M13 * M24 * M31 * M42 + M14 * M21 * M33 * M42 - M11 * M24 * M33 * M42 - M13 * M21 * M34 * M42 + M11 * M23 * M34 * M42 + M14 * M22 * M31 * M43 - M12 * M24 * M31 * M43 - M14 * M21 * M32 * M43 + M11 * M24 * M32 * M43 + M12 * M21 * M34 * M43 - M11 * M22 * M34 * M43 - M13 * M22 * M31 * M44 + M12 * M23 * M31 * M44 + M13 * M21 * M32 * M44 - M11 * M23 * M32 * M44 - M12 * M21 * M33 * M44 + M11 * M22 * M33 * M44;

	public Matrix4F()
	{
		M11 = 1f;
		M12 = 0f;
		M13 = 0f;
		M14 = 0f;
		M21 = 0f;
		M22 = 1f;
		M23 = 0f;
		M24 = 0f;
		M31 = 0f;
		M32 = 0f;
		M33 = 1f;
		M34 = 0f;
		M41 = 0f;
		M42 = 0f;
		M43 = 0f;
		M44 = 1f;
	}

	public Matrix4F(float[] m)
	{
		Set(m);
	}

	public Matrix4F(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
	{
		M11 = m11;
		M12 = m12;
		M13 = m13;
		M14 = m14;
		M21 = m21;
		M22 = m22;
		M23 = m23;
		M24 = m24;
		M31 = m31;
		M32 = m32;
		M33 = m33;
		M34 = m34;
		M41 = m41;
		M42 = m42;
		M43 = m43;
		M44 = m44;
	}

	public Matrix4F(Vec3F translation)
	{
		Set(translation);
	}

	public Matrix4F(Matrix3F m)
	{
		Set(m);
	}

	public Matrix4F(Matrix4F m)
	{
		Set(m);
	}

	public Matrix4F(QuatF q)
	{
		Set(q);
	}

	public Matrix4F(AngleAxisF angleAxis)
	{
		Set(angleAxis);
	}

	public void Set(Vec3F translation)
	{
		M11 = (M22 = (M33 = (M44 = 1f)));
		M12 = (M13 = (M14 = (M21 = (M23 = (M24 = (M31 = (M32 = (M34 = 0f))))))));
		M41 = translation.X;
		M42 = translation.Y;
		M43 = translation.Z;
	}

	public void Set(Matrix3F m)
	{
		M11 = m.M11;
		M12 = m.M12;
		M13 = m.M13;
		M14 = 0f;
		M21 = m.M21;
		M22 = m.M22;
		M23 = m.M23;
		M24 = 0f;
		M31 = m.M31;
		M32 = m.M32;
		M33 = m.M33;
		M34 = 0f;
		M41 = 0f;
		M42 = 0f;
		M43 = 0f;
		M44 = 1f;
	}

	public void Set(Matrix4F m)
	{
		M11 = m.M11;
		M12 = m.M12;
		M13 = m.M13;
		M14 = m.M14;
		M21 = m.M21;
		M22 = m.M22;
		M23 = m.M23;
		M24 = m.M24;
		M31 = m.M31;
		M32 = m.M32;
		M33 = m.M33;
		M34 = m.M34;
		M41 = m.M41;
		M42 = m.M42;
		M43 = m.M43;
		M44 = m.M44;
	}

	public void Set(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
	{
		M11 = m11;
		M12 = m12;
		M13 = m13;
		M14 = m14;
		M21 = m21;
		M22 = m22;
		M23 = m23;
		M24 = m24;
		M31 = m31;
		M32 = m32;
		M33 = m33;
		M34 = m34;
		M41 = m41;
		M42 = m42;
		M43 = m43;
		M44 = m44;
	}

	public void Set(float[] m)
	{
		M11 = m[0];
		M12 = m[1];
		M13 = m[2];
		M14 = m[3];
		M21 = m[4];
		M22 = m[5];
		M23 = m[6];
		M24 = m[7];
		M31 = m[8];
		M32 = m[9];
		M33 = m[10];
		M34 = m[11];
		M41 = m[12];
		M42 = m[13];
		M43 = m[14];
		M44 = m[15];
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
		M14 = (M24 = (M34 = (M41 = (M42 = (M43 = 0f)))));
		M44 = 1f;
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
		M14 = (M24 = (M34 = (M41 = (M42 = (M43 = 0f)))));
		M44 = 1f;
	}

	public void CopyTo(float[] array, int index)
	{
		array[index++] = M11;
		array[index++] = M12;
		array[index++] = M13;
		array[index++] = M14;
		array[index++] = M21;
		array[index++] = M22;
		array[index++] = M23;
		array[index++] = M24;
		array[index++] = M31;
		array[index++] = M32;
		array[index++] = M33;
		array[index++] = M34;
		array[index++] = M41;
		array[index++] = M42;
		array[index++] = M43;
		array[index] = M44;
	}

	public float[] ToArray()
	{
		return new float[16]
		{
			M11, M12, M13, M14, M21, M22, M23, M24, M31, M32,
			M33, M34, M41, M42, M43, M44
		};
	}

	public static Matrix4F CreateTranslation(Vec3F translation)
	{
		Matrix4F matrix4F = new Matrix4F();
		matrix4F.Set(translation);
		return matrix4F;
	}

	public void Scale(float s)
	{
		M12 = (M13 = (M14 = (M21 = (M23 = (M24 = (M31 = (M32 = (M34 = (M41 = (M42 = (M43 = 0f)))))))))));
		M44 = 1f;
		M11 = s;
		M22 = s;
		M33 = s;
	}

	public void Scale(float x, float y, float z)
	{
		M12 = (M13 = (M14 = (M21 = (M23 = (M24 = (M31 = (M32 = (M34 = (M41 = (M42 = (M43 = 0f)))))))))));
		M44 = 1f;
		M11 = x;
		M22 = y;
		M33 = z;
	}

	public void Scale(Vec3F scale)
	{
		M12 = (M13 = (M14 = (M21 = (M23 = (M24 = (M31 = (M32 = (M34 = (M41 = (M42 = (M43 = 0f)))))))))));
		M44 = 1f;
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
		M14 = (M24 = (M34 = (M41 = (M42 = (M43 = 0f)))));
		M44 = 1f;
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
		M14 = (M24 = (M34 = (M41 = (M42 = (M43 = 0f)))));
		M44 = 1f;
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
		M14 = (M24 = (M34 = (M41 = (M42 = (M43 = 0f)))));
		M44 = 1f;
	}

	public static Matrix4F RotAxisRH(Vec3F axis, double angle)
	{
		float num = (float)Math.Cos(angle);
		float num2 = (float)Math.Sin(angle);
		float num3 = 1f - num;
		Matrix4F matrix4F = new Matrix4F();
		float x = axis.X;
		float y = axis.Y;
		float z = axis.Z;
		matrix4F.Set(num + num3 * x * x, num3 * x * y + num2 * z, num3 * x * z - num2 * y, 0f, num3 * x * y - num2 * z, num + num3 * y * y, num3 * y * z + num2 * x, 0f, num3 * x * z + num2 * y, num3 * y * z - num2 * x, num + num3 * z * z, 0f, 0f, 0f, 0f, 1f);
		return matrix4F;
	}

	public void GetRotation(Matrix3F result)
	{
		result.M11 = M11;
		result.M12 = M12;
		result.M13 = M13;
		result.M21 = M21;
		result.M22 = M22;
		result.M23 = M23;
		result.M31 = M31;
		result.M32 = M32;
		result.M33 = M33;
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

	public void Mul(Matrix4F m, float scale)
	{
		M11 = scale * m.M11;
		M12 = scale * m.M12;
		M13 = scale * m.M13;
		M14 = scale * m.M14;
		M21 = scale * m.M21;
		M22 = scale * m.M22;
		M23 = scale * m.M23;
		M24 = scale * m.M24;
		M31 = scale * m.M31;
		M32 = scale * m.M32;
		M33 = scale * m.M33;
		M34 = scale * m.M34;
		M41 = scale * m.M41;
		M42 = scale * m.M42;
		M43 = scale * m.M43;
		M44 = scale * m.M44;
	}

	public void Mul(Matrix4F m1, Matrix4F m2)
	{
		if (m1 == this)
		{
			s_mulTemp.Set(m1);
			m1 = s_mulTemp;
			if (m2 == this)
			{
				m2 = s_mulTemp;
			}
		}
		else if (m2 == this)
		{
			s_mulTemp.Set(m2);
			m2 = s_mulTemp;
		}
		M11 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31 + m1.M14 * m2.M41;
		M12 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32 + m1.M14 * m2.M42;
		M13 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33 + m1.M14 * m2.M43;
		M14 = m1.M11 * m2.M14 + m1.M12 * m2.M24 + m1.M13 * m2.M34 + m1.M14 * m2.M44;
		M21 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31 + m1.M24 * m2.M41;
		M22 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32 + m1.M24 * m2.M42;
		M23 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33 + m1.M24 * m2.M43;
		M24 = m1.M21 * m2.M14 + m1.M22 * m2.M24 + m1.M23 * m2.M34 + m1.M24 * m2.M44;
		M31 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31 + m1.M34 * m2.M41;
		M32 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32 + m1.M34 * m2.M42;
		M33 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33 + m1.M34 * m2.M43;
		M34 = m1.M31 * m2.M14 + m1.M32 * m2.M24 + m1.M33 * m2.M34 + m1.M34 * m2.M44;
		M41 = m1.M41 * m2.M11 + m1.M42 * m2.M21 + m1.M43 * m2.M31 + m1.M44 * m2.M41;
		M42 = m1.M41 * m2.M12 + m1.M42 * m2.M22 + m1.M43 * m2.M32 + m1.M44 * m2.M42;
		M43 = m1.M41 * m2.M13 + m1.M42 * m2.M23 + m1.M43 * m2.M33 + m1.M44 * m2.M43;
		M44 = m1.M41 * m2.M14 + m1.M42 * m2.M24 + m1.M43 * m2.M34 + m1.M44 * m2.M44;
	}

	public void Invert(Matrix4F m)
	{
		float m2 = m.M23 * m.M34 * m.M42 - m.M24 * m.M33 * m.M42 + m.M24 * m.M32 * m.M43 - m.M22 * m.M34 * m.M43 - m.M23 * m.M32 * m.M44 + m.M22 * m.M33 * m.M44;
		float m3 = m.M14 * m.M33 * m.M42 - m.M13 * m.M34 * m.M42 - m.M14 * m.M32 * m.M43 + m.M12 * m.M34 * m.M43 + m.M13 * m.M32 * m.M44 - m.M12 * m.M33 * m.M44;
		float m4 = m.M13 * m.M24 * m.M42 - m.M14 * m.M23 * m.M42 + m.M14 * m.M22 * m.M43 - m.M12 * m.M24 * m.M43 - m.M13 * m.M22 * m.M44 + m.M12 * m.M23 * m.M44;
		float m5 = m.M14 * m.M23 * m.M32 - m.M13 * m.M24 * m.M32 - m.M14 * m.M22 * m.M33 + m.M12 * m.M24 * m.M33 + m.M13 * m.M22 * m.M34 - m.M12 * m.M23 * m.M34;
		float m6 = m.M24 * m.M33 * m.M41 - m.M23 * m.M34 * m.M41 - m.M24 * m.M31 * m.M43 + m.M21 * m.M34 * m.M43 + m.M23 * m.M31 * m.M44 - m.M21 * m.M33 * m.M44;
		float m7 = m.M13 * m.M34 * m.M41 - m.M14 * m.M33 * m.M41 + m.M14 * m.M31 * m.M43 - m.M11 * m.M34 * m.M43 - m.M13 * m.M31 * m.M44 + m.M11 * m.M33 * m.M44;
		float m8 = m.M14 * m.M23 * m.M41 - m.M13 * m.M24 * m.M41 - m.M14 * m.M21 * m.M43 + m.M11 * m.M24 * m.M43 + m.M13 * m.M21 * m.M44 - m.M11 * m.M23 * m.M44;
		float m9 = m.M13 * m.M24 * m.M31 - m.M14 * m.M23 * m.M31 + m.M14 * m.M21 * m.M33 - m.M11 * m.M24 * m.M33 - m.M13 * m.M21 * m.M34 + m.M11 * m.M23 * m.M34;
		float m10 = m.M22 * m.M34 * m.M41 - m.M24 * m.M32 * m.M41 + m.M24 * m.M31 * m.M42 - m.M21 * m.M34 * m.M42 - m.M22 * m.M31 * m.M44 + m.M21 * m.M32 * m.M44;
		float m11 = m.M14 * m.M32 * m.M41 - m.M12 * m.M34 * m.M41 - m.M14 * m.M31 * m.M42 + m.M11 * m.M34 * m.M42 + m.M12 * m.M31 * m.M44 - m.M11 * m.M32 * m.M44;
		float m12 = m.M12 * m.M24 * m.M41 - m.M14 * m.M22 * m.M41 + m.M14 * m.M21 * m.M42 - m.M11 * m.M24 * m.M42 - m.M12 * m.M21 * m.M44 + m.M11 * m.M22 * m.M44;
		float m13 = m.M14 * m.M22 * m.M31 - m.M12 * m.M24 * m.M31 - m.M14 * m.M21 * m.M32 + m.M11 * m.M24 * m.M32 + m.M12 * m.M21 * m.M34 - m.M11 * m.M22 * m.M34;
		float m14 = m.M23 * m.M32 * m.M41 - m.M22 * m.M33 * m.M41 - m.M23 * m.M31 * m.M42 + m.M21 * m.M33 * m.M42 + m.M22 * m.M31 * m.M43 - m.M21 * m.M32 * m.M43;
		float m15 = m.M12 * m.M33 * m.M41 - m.M13 * m.M32 * m.M41 + m.M13 * m.M31 * m.M42 - m.M11 * m.M33 * m.M42 - m.M12 * m.M31 * m.M43 + m.M11 * m.M32 * m.M43;
		float m16 = m.M13 * m.M22 * m.M41 - m.M12 * m.M23 * m.M41 - m.M13 * m.M21 * m.M42 + m.M11 * m.M23 * m.M42 + m.M12 * m.M21 * m.M43 - m.M11 * m.M22 * m.M43;
		float m17 = m.M12 * m.M23 * m.M31 - m.M13 * m.M22 * m.M31 + m.M13 * m.M21 * m.M32 - m.M11 * m.M23 * m.M32 - m.M12 * m.M21 * m.M33 + m.M11 * m.M22 * m.M33;
		float scale = (float)(1.0 / m.Determinant);
		M11 = m2;
		M12 = m3;
		M13 = m4;
		M14 = m5;
		M21 = m6;
		M22 = m7;
		M23 = m8;
		M24 = m9;
		M31 = m10;
		M32 = m11;
		M33 = m12;
		M34 = m13;
		M41 = m14;
		M42 = m15;
		M43 = m16;
		M44 = m17;
		Mul(this, scale);
	}

	public void Transpose(Matrix4F m)
	{
		float m2 = m.M11;
		float m3 = m.M21;
		float m4 = m.M31;
		float m5 = m.M41;
		float m6 = m.M12;
		float m7 = m.M22;
		float m8 = m.M32;
		float m9 = m.M42;
		float m10 = m.M13;
		float m11 = m.M23;
		float m12 = m.M33;
		float m13 = m.M43;
		float m14 = m.M14;
		float m15 = m.M24;
		float m16 = m.M34;
		float m17 = m.M44;
		M11 = m2;
		M12 = m3;
		M13 = m4;
		M14 = m5;
		M21 = m6;
		M22 = m7;
		M23 = m8;
		M24 = m9;
		M31 = m10;
		M32 = m11;
		M33 = m12;
		M34 = m13;
		M41 = m14;
		M42 = m15;
		M43 = m16;
		M44 = m17;
	}

	public void Normalize(Matrix4F m)
	{
		double num = 1.0 / Math.Sqrt(m.M11 * m.M11 + m.M12 * m.M12 + m.M13 * m.M13);
		M11 = (float)((double)m.M11 * num);
		M12 = (float)((double)m.M12 * num);
		M13 = (float)((double)m.M13 * num);
		num = 1.0 / Math.Sqrt(m.M21 * m.M21 + m.M22 * m.M22 + m.M23 * m.M23);
		M21 = (float)((double)m.M21 * num);
		M22 = (float)((double)m.M22 * num);
		M23 = (float)((double)m.M23 * num);
		num = 1.0 / Math.Sqrt(m.M31 * m.M31 + m.M32 * m.M32 + m.M33 * m.M33);
		M31 = (float)((double)m.M31 * num);
		M32 = (float)((double)m.M32 * num);
		M33 = (float)((double)m.M33 * num);
		M41 = m.M41;
		M42 = m.M42;
		M43 = m.M43;
	}

	public void OrthoNormalize(Matrix4F m)
	{
		Vec3F vec3F = new Vec3F(m.M11, m.M12, m.M13);
		Vec3F v = Vec3F.Cross(v2: new Vec3F(m.M21, m.M22, m.M23), v1: vec3F);
		Vec3F vec3F2 = Vec3F.Cross(v, vec3F);
		vec3F /= vec3F.Length;
		vec3F2 /= vec3F2.Length;
		v /= v.Length;
		M11 = vec3F.X;
		M12 = vec3F.Y;
		M13 = vec3F.Z;
		M21 = vec3F2.X;
		M22 = vec3F2.Y;
		M23 = vec3F2.Z;
		M31 = v.X;
		M32 = v.Y;
		M33 = v.Z;
		M41 = m.M41;
		M42 = m.M42;
		M43 = m.M43;
	}

	public void Transform(Vec3F v, out Vec3F result)
	{
		result.X = M11 * v.X + M21 * v.Y + M31 * v.Z + M41;
		result.Y = M12 * v.X + M22 * v.Y + M32 * v.Z + M42;
		result.Z = M13 * v.X + M23 * v.Y + M33 * v.Z + M43;
	}

	public void Transform(ref Vec3F v)
	{
		float x = M11 * v.X + M21 * v.Y + M31 * v.Z + M41;
		float y = M12 * v.X + M22 * v.Y + M32 * v.Z + M42;
		float z = M13 * v.X + M23 * v.Y + M33 * v.Z + M43;
		v.X = x;
		v.Y = y;
		v.Z = z;
	}

	public void TransformVector(Vec3F v, out Vec3F result)
	{
		result.X = M11 * v.X + M21 * v.Y + M31 * v.Z;
		result.Y = M12 * v.X + M22 * v.Y + M32 * v.Z;
		result.Z = M13 * v.X + M23 * v.Y + M33 * v.Z;
	}

	public void TransformNormal(Vec3F n, out Vec3F result)
	{
		Matrix4F matrix4F = new Matrix4F(this);
		matrix4F.Invert(matrix4F);
		matrix4F.Transpose(matrix4F);
		TransformNormal(n, matrix4F, out result);
	}

	public void TransformNormal(Vec3F n, Matrix4F transposeOfInverse, out Vec3F result)
	{
		transposeOfInverse.Transform(n, out result);
	}

	public void Transform(Vec4F v, out Vec4F result)
	{
		result.X = M11 * v.X + M21 * v.Y + M31 * v.Z + M41 * v.W;
		result.Y = M12 * v.X + M22 * v.Y + M32 * v.Z + M42 * v.W;
		result.Z = M13 * v.X + M23 * v.Y + M33 * v.Z + M43 * v.W;
		result.W = M14 * v.X + M24 * v.Y + M34 * v.Z + M44 * v.W;
	}

	public void Transform(Plane3F p, out Plane3F result)
	{
		Vec3F result2 = p.Normal;
		Vec3F result3 = p.PointOnPlane();
		TransformNormal(result2, out result2);
		Transform(result3, out result3);
		result2.Normalize();
		result = new Plane3F(result2, result3);
	}

	public void Transform(Plane3F p, Matrix4F transposeOfInverse, out Plane3F result)
	{
		Vec3F result2 = p.Normal;
		Vec3F result3 = p.PointOnPlane();
		TransformNormal(result2, transposeOfInverse, out result2);
		Transform(result3, out result3);
		result2.Normalize();
		result = new Plane3F(result2, result3);
	}

	public override string ToString()
	{
		string text = StringUtil.GetNumberListSeparator(null) + " ";
		return M11 + text + M12 + text + M13 + text + M14 + Environment.NewLine + M21 + text + M22 + text + M23 + text + M24 + Environment.NewLine + M31 + text + M32 + text + M33 + text + M34 + Environment.NewLine + M41 + text + M42 + text + M43 + text + M44 + Environment.NewLine + Environment.NewLine;
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		string numberListSeparator = StringUtil.GetNumberListSeparator(formatProvider);
		if (format == null)
		{
			format = "R";
		}
		return string.Format("{0}{16} {1}{16} {2}{16} {3}{16} {4}{16} {5}{16} {6}{16} {7}{16} {8}{16} {9}{16} {10}{16} {11}{16} {12}{16} {13}{16} {14}{16} {15}", M11.ToString(format, formatProvider), M12.ToString(format, formatProvider), M13.ToString(format, formatProvider), M14.ToString(format, formatProvider), M21.ToString(format, formatProvider), M22.ToString(format, formatProvider), M23.ToString(format, formatProvider), M24.ToString(format, formatProvider), M31.ToString(format, formatProvider), M32.ToString(format, formatProvider), M33.ToString(format, formatProvider), M34.ToString(format, formatProvider), M41.ToString(format, formatProvider), M42.ToString(format, formatProvider), M43.ToString(format, formatProvider), M44.ToString(format, formatProvider), numberListSeparator);
	}

	public bool Equals(Matrix4F m)
	{
		return M11 == m.M11 && M12 == m.M12 && M13 == m.M13 && M14 == m.M14 && M21 == m.M21 && M22 == m.M22 && M23 == m.M23 && M24 == m.M24 && M31 == m.M31 && M32 == m.M32 && M33 == m.M33 && M34 == m.M34 && M41 == m.M41 && M42 == m.M42 && M43 == m.M43 && M44 == m.M44;
	}

	public override bool Equals(object obj)
	{
		if (obj is Matrix4F)
		{
			Matrix4F m = (Matrix4F)obj;
			return Equals(m);
		}
		return false;
	}

	public bool EpsilonEquals(Matrix4F m, double eps)
	{
		return (double)Math.Abs(M11 - m.M11) < eps && (double)Math.Abs(M12 - m.M12) < eps && (double)Math.Abs(M13 - m.M13) < eps && (double)Math.Abs(M14 - m.M14) < eps && (double)Math.Abs(M21 - m.M21) < eps && (double)Math.Abs(M22 - m.M22) < eps && (double)Math.Abs(M23 - m.M23) < eps && (double)Math.Abs(M24 - m.M24) < eps && (double)Math.Abs(M31 - m.M31) < eps && (double)Math.Abs(M32 - m.M32) < eps && (double)Math.Abs(M33 - m.M33) < eps && (double)Math.Abs(M34 - m.M34) < eps && (double)Math.Abs(M41 - m.M41) < eps && (double)Math.Abs(M42 - m.M42) < eps && (double)Math.Abs(M43 - m.M43) < eps && (double)Math.Abs(M44 - m.M44) < eps;
	}

	public override int GetHashCode()
	{
		long num = 1L;
		num = 31 * num + M11.GetHashCode();
		num = 31 * num + M12.GetHashCode();
		num = 31 * num + M13.GetHashCode();
		num = 31 * num + M14.GetHashCode();
		num = 31 * num + M21.GetHashCode();
		num = 31 * num + M22.GetHashCode();
		num = 31 * num + M23.GetHashCode();
		num = 31 * num + M24.GetHashCode();
		num = 31 * num + M31.GetHashCode();
		num = 31 * num + M32.GetHashCode();
		num = 31 * num + M33.GetHashCode();
		num = 31 * num + M34.GetHashCode();
		num = 31 * num + M41.GetHashCode();
		num = 31 * num + M42.GetHashCode();
		num = 31 * num + M43.GetHashCode();
		num = 31 * num + M44.GetHashCode();
		return (int)(num ^ (num >> 32));
	}

	public static void Multiply(Matrix4F m1, Matrix4F m2, Matrix4F result)
	{
		float m3 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31 + m1.M14 * m2.M41;
		float m4 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32 + m1.M14 * m2.M42;
		float m5 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33 + m1.M14 * m2.M43;
		float m6 = m1.M11 * m2.M14 + m1.M12 * m2.M24 + m1.M13 * m2.M34 + m1.M14 * m2.M44;
		float m7 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31 + m1.M24 * m2.M41;
		float m8 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32 + m1.M24 * m2.M42;
		float m9 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33 + m1.M24 * m2.M43;
		float m10 = m1.M21 * m2.M14 + m1.M22 * m2.M24 + m1.M23 * m2.M34 + m1.M24 * m2.M44;
		float m11 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31 + m1.M34 * m2.M41;
		float m12 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32 + m1.M34 * m2.M42;
		float m13 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33 + m1.M34 * m2.M43;
		float m14 = m1.M31 * m2.M14 + m1.M32 * m2.M24 + m1.M33 * m2.M34 + m1.M34 * m2.M44;
		float m15 = m1.M41 * m2.M11 + m1.M42 * m2.M21 + m1.M43 * m2.M31 + m1.M44 * m2.M41;
		float m16 = m1.M41 * m2.M12 + m1.M42 * m2.M22 + m1.M43 * m2.M32 + m1.M44 * m2.M42;
		float m17 = m1.M41 * m2.M13 + m1.M42 * m2.M23 + m1.M43 * m2.M33 + m1.M44 * m2.M43;
		float m18 = m1.M41 * m2.M14 + m1.M42 * m2.M24 + m1.M43 * m2.M34 + m1.M44 * m2.M44;
		result.M11 = m3;
		result.M12 = m4;
		result.M13 = m5;
		result.M14 = m6;
		result.M21 = m7;
		result.M22 = m8;
		result.M23 = m9;
		result.M24 = m10;
		result.M31 = m11;
		result.M32 = m12;
		result.M33 = m13;
		result.M34 = m14;
		result.M41 = m15;
		result.M42 = m16;
		result.M43 = m17;
		result.M44 = m18;
	}

	public static Matrix4F Multiply(Matrix4F m1, Matrix4F m2)
	{
		Matrix4F matrix4F = new Matrix4F();
		matrix4F.M11 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31 + m1.M14 * m2.M41;
		matrix4F.M12 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32 + m1.M14 * m2.M42;
		matrix4F.M13 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33 + m1.M14 * m2.M43;
		matrix4F.M14 = m1.M11 * m2.M14 + m1.M12 * m2.M24 + m1.M13 * m2.M34 + m1.M14 * m2.M44;
		matrix4F.M21 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31 + m1.M24 * m2.M41;
		matrix4F.M22 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32 + m1.M24 * m2.M42;
		matrix4F.M23 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33 + m1.M24 * m2.M43;
		matrix4F.M24 = m1.M21 * m2.M14 + m1.M22 * m2.M24 + m1.M23 * m2.M34 + m1.M24 * m2.M44;
		matrix4F.M31 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31 + m1.M34 * m2.M41;
		matrix4F.M32 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32 + m1.M34 * m2.M42;
		matrix4F.M33 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33 + m1.M34 * m2.M43;
		matrix4F.M34 = m1.M31 * m2.M14 + m1.M32 * m2.M24 + m1.M33 * m2.M34 + m1.M34 * m2.M44;
		matrix4F.M41 = m1.M41 * m2.M11 + m1.M42 * m2.M21 + m1.M43 * m2.M31 + m1.M44 * m2.M41;
		matrix4F.M42 = m1.M41 * m2.M12 + m1.M42 * m2.M22 + m1.M43 * m2.M32 + m1.M44 * m2.M42;
		matrix4F.M43 = m1.M41 * m2.M13 + m1.M42 * m2.M23 + m1.M43 * m2.M33 + m1.M44 * m2.M43;
		matrix4F.M44 = m1.M41 * m2.M14 + m1.M42 * m2.M24 + m1.M43 * m2.M34 + m1.M44 * m2.M44;
		return matrix4F;
	}

	public static Matrix4F operator *(Matrix4F m1, Matrix4F m2)
	{
		return Multiply(m1, m2);
	}
}

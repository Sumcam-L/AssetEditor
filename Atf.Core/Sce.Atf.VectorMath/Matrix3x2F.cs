using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sce.Atf.VectorMath;

public struct Matrix3x2F : IEquatable<Matrix3x2F>
{
	public float M11;

	public float M12;

	public float M21;

	public float M22;

	public float DX;

	public float DY;

	public static Matrix3x2F Identity => new Matrix3x2F
	{
		M11 = 1f,
		M22 = 1f
	};

	public bool IsIdentity => M11 == 1f && M12 == 0f && M21 == 0f && M22 == 1f && DX == 0f && DY == 0f;

	public static void Multiply(ref Matrix3x2F left, ref Matrix3x2F right, out Matrix3x2F result)
	{
		Matrix3x2F matrix3x2F = default(Matrix3x2F);
		float m = left.M12;
		float m2 = right.M21;
		float m3 = left.M11;
		float m4 = right.M11;
		matrix3x2F.M11 = m4 * m3 + m2 * m;
		float m5 = right.M22;
		float m6 = right.M12;
		matrix3x2F.M12 = m6 * m3 + m5 * m;
		float m7 = left.M22;
		float m8 = left.M21;
		matrix3x2F.M21 = m7 * m2 + m8 * m4;
		matrix3x2F.M22 = m8 * m6 + m7 * m5;
		float dY = left.DY;
		float dX = left.DX;
		matrix3x2F.DX = right.DX + (dX * m4 + dY * m2);
		matrix3x2F.DY = right.DY + (dX * m6 + dY * m5);
		result = matrix3x2F;
	}

	public static Matrix3x2F Multiply(Matrix3x2F left, Matrix3x2F right)
	{
		Matrix3x2F result = default(Matrix3x2F);
		float m = right.M21;
		float m2 = left.M12;
		float m3 = left.M11;
		float m4 = right.M11;
		result.M11 = m4 * m3 + m2 * m;
		float m5 = right.M22;
		float m6 = right.M12;
		result.M12 = m6 * m3 + m5 * m2;
		float m7 = left.M22;
		float m8 = left.M21;
		result.M21 = m7 * m + m8 * m4;
		result.M22 = m8 * m6 + m7 * m5;
		float dY = left.DY;
		float dX = left.DX;
		result.DX = right.DX + (dX * m4 + dY * m);
		result.DY = right.DY + (dX * m6 + dY * m5);
		return result;
	}

	public static void CreateRotation(float angle, out Matrix3x2F result)
	{
		Matrix3x2F matrix3x2F = default(Matrix3x2F);
		double num = (double)angle * Math.PI / 180.0;
		float num2 = (float)Math.Cos(num);
		float num3 = (float)Math.Sin(num);
		matrix3x2F.M11 = num2;
		matrix3x2F.M12 = num3;
		matrix3x2F.M21 = 0f - num3;
		matrix3x2F.M22 = num2;
		result = matrix3x2F;
	}

	public static Matrix3x2F CreateRotation(float angle)
	{
		Matrix3x2F result = default(Matrix3x2F);
		double num = (double)angle * Math.PI / 180.0;
		float num2 = (float)Math.Cos(num);
		float num3 = (float)Math.Sin(num);
		result.M11 = num2;
		result.M12 = num3;
		result.M21 = 0f - num3;
		result.M22 = num2;
		return result;
	}

	public static void CreateScale(float x, float y, out Matrix3x2F result)
	{
		result = new Matrix3x2F
		{
			M11 = x,
			M22 = y
		};
	}

	public static Matrix3x2F CreateScale(float x, float y)
	{
		return new Matrix3x2F
		{
			M11 = x,
			M22 = y
		};
	}

	public static void CreateTranslation(float x, float y, out Matrix3x2F result)
	{
		result = new Matrix3x2F
		{
			M11 = 1f,
			M22 = 1f,
			DX = x,
			DY = y
		};
	}

	public static Matrix3x2F CreateTranslation(float x, float y)
	{
		return new Matrix3x2F
		{
			M11 = 1f,
			M22 = 1f,
			DX = x,
			DY = y
		};
	}

	public static RectangleF Transform(Matrix3x2F mat, RectangleF r)
	{
		return new RectangleF
		{
			X = r.X * mat.M11 + r.Y * mat.M21 + mat.DX,
			Y = r.X * mat.M12 + r.Y * mat.M22 + mat.DY,
			Width = r.Width * mat.M11 + r.Height * mat.M21,
			Height = r.Width * mat.M12 + r.Height * mat.M22
		};
	}

	public static void TransformPoint(ref Matrix3x2F mat, ref PointF point, ref PointF result)
	{
		result.X = point.X * mat.M11 + point.Y * mat.M21 + mat.DX;
		result.Y = point.X * mat.M12 + point.Y * mat.M22 + mat.DY;
	}

	public static PointF TransformPoint(Matrix3x2F mat, PointF point)
	{
		return new PointF
		{
			X = point.X * mat.M11 + point.Y * mat.M21 + mat.DX,
			Y = point.X * mat.M12 + point.Y * mat.M22 + mat.DY
		};
	}

	public static PointF TransformVector(Matrix3x2F mat, PointF point)
	{
		return new PointF
		{
			X = point.X * mat.M11 + point.Y * mat.M21,
			Y = point.X * mat.M12 + point.Y * mat.M22
		};
	}

	public float Determinant()
	{
		return M22 * M11 - M21 * M12;
	}

	public void Invert()
	{
		float m = M22;
		float num = 0f - M12;
		float num2 = 0f - M21;
		float m2 = M11;
		float num3 = M21 * DY - M22 * DX;
		float num4 = M12 * DX - M11 * DY;
		float num5 = 1f / Determinant();
		M11 = m * num5;
		M12 = num * num5;
		M21 = num2 * num5;
		M22 = m2 * num5;
		DX = num3 * num5;
		DY = num4 * num5;
	}

	public static Matrix3x2F Invert(Matrix3x2F m)
	{
		float m2 = m.M22;
		float num = 0f - m.M12;
		float num2 = 0f - m.M21;
		float m3 = m.M11;
		float num3 = m.M21 * m.DY - m.M22 * m.DX;
		float num4 = m.M12 * m.DX - m.M11 * m.DY;
		float num5 = 1f / m.Determinant();
		return new Matrix3x2F
		{
			M11 = m2 * num5,
			M12 = num * num5,
			M21 = num2 * num5,
			M22 = m3 * num5,
			DX = num3 * num5,
			DY = num4 * num5
		};
	}

	public static Matrix3x2F operator *(Matrix3x2F left, Matrix3x2F right)
	{
		Matrix3x2F result = default(Matrix3x2F);
		float m = right.M21;
		float m2 = left.M12;
		float m3 = left.M11;
		float m4 = right.M11;
		result.M11 = m4 * m3 + m2 * m;
		float m5 = right.M22;
		float m6 = right.M12;
		result.M12 = m6 * m3 + m5 * m2;
		float m7 = left.M22;
		float m8 = left.M21;
		result.M21 = m7 * m + m8 * m4;
		result.M22 = m8 * m6 + m7 * m5;
		float dY = left.DY;
		float dX = left.DX;
		result.DX = right.DX + (dX * m4 + dY * m);
		result.DY = right.DY + (dX * m6 + dY * m5);
		return result;
	}

	public static implicit operator Matrix3x2F(Matrix matrix)
	{
		Matrix3x2F result = default(Matrix3x2F);
		float[] elements = matrix.Elements;
		result.M11 = elements[0];
		result.M12 = elements[1];
		result.M21 = elements[2];
		result.M22 = elements[3];
		result.DX = elements[4];
		result.DY = elements[5];
		return result;
	}

	public override int GetHashCode()
	{
		return M11.GetHashCode() + M12.GetHashCode() + M21.GetHashCode() + M22.GetHashCode() + DX.GetHashCode() + DY.GetHashCode();
	}

	public static bool Equals(ref Matrix3x2F value1, ref Matrix3x2F value2)
	{
		return value1.M11 == value2.M11 && value1.M12 == value2.M12 && value1.M21 == value2.M21 && value1.M22 == value2.M22 && value1.DX == value2.DX && value1.DY == value2.DY;
	}

	public bool Equals(Matrix3x2F other)
	{
		return M11 == other.M11 && M12 == other.M12 && M21 == other.M21 && M22 == other.M22 && DX == other.DX && DY == other.DY;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((Matrix3x2F)obj);
	}
}

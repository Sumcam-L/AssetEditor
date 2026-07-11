using System;

namespace Sce.Atf.VectorMath;

public struct EulerAngles3F : IFormattable
{
	private Vec3F m_Angles;

	private EulerAngleOrder m_Order;

	public Vec3F Angles
	{
		get
		{
			return m_Angles;
		}
		set
		{
			m_Angles = value;
		}
	}

	public EulerAngleOrder RotOrder
	{
		get
		{
			return m_Order;
		}
		set
		{
			m_Order = value;
		}
	}

	public EulerAngles3F(Vec3F angles, EulerAngleOrder order)
	{
		m_Angles = angles;
		m_Order = order;
	}

	public EulerAngles3F(float[] angles, EulerAngleOrder order)
	{
		m_Angles = new Vec3F(angles);
		m_Order = order;
	}

	public Vec3F CalculateOrderedAngles()
	{
		Matrix3F matrix3F = CalculateMatrix();
		matrix3F.GetEulerAngles(out var x, out var y, out var z);
		return new Vec3F(x, y, z);
	}

	public Matrix3F CalculateMatrix()
	{
		Matrix3F matrix3F = new Matrix3F();
		Matrix3F matrix3F2 = new Matrix3F();
		switch (m_Order)
		{
		case EulerAngleOrder.XYZ:
			if (m_Angles.X != 0f)
			{
				matrix3F2.RotX(m_Angles.X);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			if (m_Angles.Y != 0f)
			{
				matrix3F2.RotY(m_Angles.Y);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			if (m_Angles.Z != 0f)
			{
				matrix3F2.RotZ(m_Angles.Z);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			break;
		case EulerAngleOrder.XZY:
			if (m_Angles.X != 0f)
			{
				matrix3F2.RotX(m_Angles.X);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			if (m_Angles.Z != 0f)
			{
				matrix3F2.RotZ(m_Angles.Z);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			if (m_Angles.Y != 0f)
			{
				matrix3F2.RotY(m_Angles.Y);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			break;
		case EulerAngleOrder.YXZ:
			if (m_Angles.Y != 0f)
			{
				matrix3F2.RotY(m_Angles.Y);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			if (m_Angles.X != 0f)
			{
				matrix3F2.RotX(m_Angles.X);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			if (m_Angles.Z != 0f)
			{
				matrix3F2.RotZ(m_Angles.Z);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			break;
		case EulerAngleOrder.YZX:
			if (m_Angles.Y != 0f)
			{
				matrix3F2.RotY(m_Angles.Y);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			if (m_Angles.Z != 0f)
			{
				matrix3F2.RotZ(m_Angles.Z);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			if (m_Angles.X != 0f)
			{
				matrix3F2.RotX(m_Angles.X);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			break;
		case EulerAngleOrder.ZXY:
			if (m_Angles.Z != 0f)
			{
				matrix3F2.RotZ(m_Angles.Z);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			if (m_Angles.X != 0f)
			{
				matrix3F2.RotX(m_Angles.X);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			if (m_Angles.Y != 0f)
			{
				matrix3F2.RotY(m_Angles.Y);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			break;
		case EulerAngleOrder.ZYX:
			if (m_Angles.Z != 0f)
			{
				matrix3F2.RotZ(m_Angles.Z);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			if (m_Angles.Y != 0f)
			{
				matrix3F2.RotY(m_Angles.Y);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			if (m_Angles.X != 0f)
			{
				matrix3F2.RotX(m_Angles.X);
				matrix3F.Mul(matrix3F, matrix3F2);
			}
			break;
		default:
			throw new Exception("Illegal euler rotation order ");
		}
		return matrix3F;
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
		return string.Format("{0}{4} {1}{4} {2}{4} {3}", Angles.X.ToString(format, formatProvider), Angles.Y.ToString(format, formatProvider), Angles.Z.ToString(format, formatProvider), RotOrder, numberListSeparator);
	}
}

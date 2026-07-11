using System.Drawing;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing;

public struct PointD
{
	public double X;

	public double Y;

	public PointD(double x, double y)
	{
		X = x;
		Y = y;
	}

	public static implicit operator PointD(Vec2F v)
	{
		return new PointD
		{
			X = v.X,
			Y = v.Y
		};
	}

	public static implicit operator PointD(PointF v)
	{
		return new PointD
		{
			X = v.X,
			Y = v.Y
		};
	}

	public static explicit operator Vec2F(PointD p)
	{
		return new Vec2F
		{
			X = (float)p.X,
			Y = (float)p.Y
		};
	}

	public static explicit operator PointF(PointD p)
	{
		return new PointF
		{
			X = (float)p.X,
			Y = (float)p.Y
		};
	}
}

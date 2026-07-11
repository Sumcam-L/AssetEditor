using System.Drawing;
using Sce.Atf.VectorMath;
using SharpDX;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D;

internal static class D2dToSharpDX
{
	internal static Vector2 ToSharpDX(this PointF point)
	{
		return new Vector2(point.X, point.Y);
	}

	internal static Vector2 ToSharpDX(this Vec2F point)
	{
		return new Vector2(point.X, point.Y);
	}

	internal static Size2F ToSharpDX(this SizeF point)
	{
		return new Size2F(point.Width, point.Height);
	}

	internal static SharpDX.RectangleF ToSharpDX(this System.Drawing.RectangleF rect)
	{
		return new SharpDX.RectangleF(rect.Left, rect.Top, rect.Width, rect.Height);
	}

	internal static RoundedRectangle ToSharpDX(this D2dRoundedRect rec)
	{
		return new RoundedRectangle
		{
			RadiusX = rec.RadiusX,
			RadiusY = rec.RadiusY,
			Rect = rec.Rect.ToSharpDX()
		};
	}

	internal static Ellipse ToSharpDX(this D2dEllipse ellipse)
	{
		return new Ellipse
		{
			Point = ellipse.Center.ToSharpDX(),
			RadiusX = ellipse.RadiusX,
			RadiusY = ellipse.RadiusY
		};
	}

	internal static BezierSegment ToSharpDX(this D2dBezierSegment seg)
	{
		return new BezierSegment
		{
			Point1 = seg.Point1.ToSharpDX(),
			Point2 = seg.Point2.ToSharpDX(),
			Point3 = seg.Point3.ToSharpDX()
		};
	}

	internal static ArcSegment ToSharpDX(this D2dArcSegment arc)
	{
		return new ArcSegment
		{
			Point = arc.Point.ToSharpDX(),
			Size = arc.Size.ToSharpDX(),
			RotationAngle = arc.RotationAngle,
			ArcSize = (ArcSize)arc.ArcSize,
			SweepDirection = (SweepDirection)arc.SweepDirection
		};
	}
}

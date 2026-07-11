using System.Drawing;

namespace Sce.Atf.Direct2D;

public struct D2dArcSegment
{
	public PointF Point;

	public SizeF Size;

	public float RotationAngle;

	public D2dSweepDirection SweepDirection;

	public D2dArcSize ArcSize;
}

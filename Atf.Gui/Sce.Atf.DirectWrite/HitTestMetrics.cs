using System.Drawing;

namespace Sce.Atf.DirectWrite;

public struct HitTestMetrics
{
	public int TextPosition;

	public int Length;

	public bool IsInside;

	public bool IsTrailingHit;

	public PointF Point;

	public float Height;

	public float Width;

	public float Top;
}

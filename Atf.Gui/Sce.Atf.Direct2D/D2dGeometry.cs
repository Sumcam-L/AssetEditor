using System.Drawing;
using SharpDX;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D;

public class D2dGeometry : D2dResource
{
	private PathGeometry m_geometry;

	internal PathGeometry NativeGeometry => m_geometry;

	public int FigureCount => m_geometry.FigureCount;

	public int SegmentCount => m_geometry.SegmentCount;

	public System.Drawing.RectangleF Bounds
	{
		get
		{
			SharpDX.RectangleF bounds = m_geometry.GetBounds();
			return new System.Drawing.RectangleF(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
		}
	}

	public D2dGeometry()
	{
		m_geometry = new PathGeometry(D2dFactory.NativeFactory);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && m_geometry != null)
		{
			m_geometry.Dispose();
			m_geometry = null;
		}
		base.Dispose(disposing);
	}

	public D2dGeometrySink Open()
	{
		return new D2dGeometrySink(m_geometry.Open());
	}

	public bool FillContainsPoint(PointF point)
	{
		return m_geometry.FillContainsPoint(point.ToSharpDX());
	}

	public bool StrokeContainsPoint(PointF point, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		return m_geometry.StrokeContainsPoint(point.ToSharpDX(), strokeWidth, strokeStyle?.NativeStrokeStyle);
	}
}

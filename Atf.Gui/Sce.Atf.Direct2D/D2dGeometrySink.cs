using System.Drawing;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D;

public sealed class D2dGeometrySink : D2dResource
{
	private GeometrySink m_sink;

	internal D2dGeometrySink(GeometrySink sink)
	{
		m_sink = sink;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && m_sink != null)
		{
			m_sink.Dispose();
			m_sink = null;
		}
		base.Dispose(disposing);
	}

	public void BeginFigure(PointF startPoint, D2dFigureBegin figureBegin)
	{
		m_sink.BeginFigure(startPoint.ToSharpDX(), (FigureBegin)figureBegin);
	}

	public void EndFigure(D2dFigureEnd figureEnd)
	{
		m_sink.EndFigure((FigureEnd)figureEnd);
	}

	public void AddLine(PointF point)
	{
		m_sink.AddLine(point.ToSharpDX());
	}

	public void AddBeziers(params D2dBezierSegment[] beziers)
	{
		foreach (D2dBezierSegment seg in beziers)
		{
			m_sink.AddBezier(seg.ToSharpDX());
		}
	}

	public void AddLines(params PointF[] points)
	{
		foreach (PointF point in points)
		{
			m_sink.AddLine(point.ToSharpDX());
		}
	}

	public void AddArc(D2dArcSegment arc)
	{
		m_sink.AddArc(arc.ToSharpDX());
	}

	public void Close()
	{
		m_sink.Close();
	}
}

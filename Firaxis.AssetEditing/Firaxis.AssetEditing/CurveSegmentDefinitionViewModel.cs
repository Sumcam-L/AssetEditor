using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public abstract class CurveSegmentDefinitionViewModel
{
	private protected CurveEditingContext Owner { get; set; }

	public CurveAdapter Curve { get; private set; }

	public CurveSegmentDefinitionAdapter CurveSegmentDefinition
	{
		get
		{
			if (Curve == null)
			{
				return null;
			}
			if (SegmentDefinitionIndex >= Curve.CurveSegments.Count)
			{
				return null;
			}
			return Curve.CurveSegments[SegmentDefinitionIndex];
		}
	}

	public int SegmentDefinitionIndex { get; private set; }

	public abstract Pen SegmentPen { get; }

	public CurveSegmentDefinitionViewModel(CurveAdapter curve, int segmentDefIndex, CurveEditingContext owner)
	{
		Curve = curve;
		SegmentDefinitionIndex = segmentDefIndex;
		Owner = owner;
	}

	public abstract void PaintCurveLines(Func<PointF, PointF> curveSpaceToDrawSpace, Graphics graphics);

	public void PaintControlPoints(Func<PointF, PointF> curveSpaceToDrawSpace, Graphics graphics, ISelectionContext selectionContext)
	{
		foreach (CurveControlPointViewModel controlPoint in GetControlPoints())
		{
			PointF point = curveSpaceToDrawSpace(controlPoint.Location);
			Point point2 = PointFToPoint(point);
			Size size = (selectionContext.SelectionContains(controlPoint) ? Owner.SelectedControlPointSize : Owner.ControlPointSize);
			graphics.FillEllipse(rect: new Rectangle(new Point(point2.X - size.Width / 2, point2.Y - size.Height / 2), size), brush: Brushes.Black);
		}
	}

	public void PaintEndLines(Func<PointF, PointF> curveSpaceToDrawSpace, Graphics graphics, float topY, float bottomY)
	{
		IEnumerable<CurveControlPointViewModel> controlPoints = GetControlPoints();
		if (controlPoints.Any())
		{
			using (Pen pen = new Pen(Color.Black))
			{
				pen.DashStyle = DashStyle.Dash;
				float x = controlPoints.First().Location.X;
				float x2 = controlPoints.Last().Location.X;
				graphics.DrawLine(pen, curveSpaceToDrawSpace(new PointF(x, bottomY)), curveSpaceToDrawSpace(new PointF(x, topY)));
				graphics.DrawLine(pen, curveSpaceToDrawSpace(new PointF(x2, bottomY)), curveSpaceToDrawSpace(new PointF(x2, topY)));
			}
		}
	}

	private Point PointFToPoint(PointF point)
	{
		return new Point(Convert.ToInt32(point.X), Convert.ToInt32(point.Y));
	}

	public abstract IEnumerable<CurveControlPointViewModel> GetControlPoints();

	public bool StartsWith(CurveControlPointViewModel controlPoint)
	{
		return GetControlPoints().FirstOrDefault() == controlPoint;
	}

	protected float GetMinX()
	{
		if (SegmentDefinitionIndex == 0)
		{
			return 0f;
		}
		return Curve.CurveSegments[SegmentDefinitionIndex - 1].StartingPoint;
	}

	protected float GetMaxX()
	{
		if (SegmentDefinitionIndex == 0)
		{
			return 0f;
		}
		if (SegmentDefinitionIndex >= Curve.CurveSegments.Count - 1)
		{
			return 1f;
		}
		return Curve.CurveSegments[SegmentDefinitionIndex + 1].StartingPoint;
	}

	protected float GetMinY()
	{
		if (!Owner.ClampDomain)
		{
			return float.MinValue;
		}
		return Owner.MinY;
	}

	protected float GetMaxY()
	{
		if (!Owner.ClampDomain)
		{
			return float.MaxValue;
		}
		return Owner.MaxY;
	}
}

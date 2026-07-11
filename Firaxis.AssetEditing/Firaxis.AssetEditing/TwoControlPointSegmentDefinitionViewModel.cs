using System;
using System.Collections.Generic;
using System.Drawing;

namespace Firaxis.AssetEditing;

public abstract class TwoControlPointSegmentDefinitionViewModel : CurveSegmentDefinitionViewModel
{
	private CurveControlPointViewModel FirstControlPoint { get; set; }

	private CurveControlPointViewModel LastControlPoint { get; set; }

	protected abstract float FirstYValue { get; set; }

	protected abstract float LastYValue { get; set; }

	public TwoControlPointSegmentDefinitionViewModel(CurveAdapter curve, int segmentDefIndex, CurveEditingContext owner)
		: base(curve, segmentDefIndex, owner)
	{
		FirstControlPoint = new CurveControlPointViewModel(GetFirstValue, SetFirstValue, ScrubFirstValue);
		LastControlPoint = new CurveControlPointViewModel(GetLastValue, SetLastValue, ScrubLastValue);
	}

	protected float GetLastValueMinX()
	{
		return base.CurveSegmentDefinition.StartingPoint;
	}

	protected float GetLastValueMaxX()
	{
		int num = base.SegmentDefinitionIndex + 2;
		if (num < base.Curve.CurveSegments.Count)
		{
			return base.Curve.CurveSegments[num].StartingPoint;
		}
		return 1f;
	}

	public override void PaintCurveLines(Func<PointF, PointF> curveSpaceToDrawSpace, Graphics graphics)
	{
		PointF pt = curveSpaceToDrawSpace(FirstControlPoint.Location);
		PointF pt2 = curveSpaceToDrawSpace(LastControlPoint.Location);
		graphics.DrawLine(SegmentPen, pt, pt2);
	}

	public override IEnumerable<CurveControlPointViewModel> GetControlPoints()
	{
		yield return FirstControlPoint;
		yield return LastControlPoint;
	}

	private PointF GetFirstValue()
	{
		CurveSegmentDefinitionAdapter curveSegmentDefinition = base.CurveSegmentDefinition;
		if (curveSegmentDefinition == null)
		{
			return PointF.Empty;
		}
		return new PointF(curveSegmentDefinition.StartingPoint, FirstYValue);
	}

	private PointF GetLastValue()
	{
		float x = 1f;
		if (base.SegmentDefinitionIndex < base.Curve.CurveSegments.Count - 1)
		{
			x = base.Curve.CurveSegments[base.SegmentDefinitionIndex + 1].StartingPoint;
		}
		return new PointF(x, LastYValue);
	}

	private PointF ScrubFirstValue(PointF inputValue)
	{
		PointF result = inputValue;
		result.X = Math.Max(result.X, GetMinX());
		result.X = Math.Min(result.X, GetMaxX());
		result.Y = Math.Max(result.Y, GetMinY());
		result.Y = Math.Min(result.Y, GetMaxY());
		return result;
	}

	private PointF ScrubLastValue(PointF inputValue)
	{
		PointF result = inputValue;
		result.X = Math.Max(result.X, GetLastValueMinX());
		result.X = Math.Min(result.X, GetLastValueMaxX());
		result.Y = Math.Max(result.Y, GetMinY());
		result.Y = Math.Min(result.Y, GetMaxY());
		return result;
	}

	private void SetFirstValue(PointF value)
	{
		if (base.CurveSegmentDefinition != null)
		{
			base.CurveSegmentDefinition.StartingPoint = value.X;
			FirstYValue = value.Y;
		}
	}

	private void SetLastValue(PointF value)
	{
		if (base.SegmentDefinitionIndex < base.Curve.CurveSegments.Count - 1)
		{
			base.Curve.CurveSegments[base.SegmentDefinitionIndex + 1].StartingPoint = value.X;
		}
		LastYValue = value.Y;
	}
}

using System.Drawing;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class LinearCurveSegmentDefinitionViewModel : TwoControlPointSegmentDefinitionViewModel
{
	public override Pen SegmentPen => Pens.Black;

	private LinearCurveSegmentAdapter LinearCurveSegment => base.CurveSegmentDefinition?.Curve.As<LinearCurveSegmentAdapter>();

	protected override float FirstYValue
	{
		get
		{
			return LinearCurveSegment.FirstValue;
		}
		set
		{
			LinearCurveSegment.FirstValue = value;
		}
	}

	protected override float LastYValue
	{
		get
		{
			return LinearCurveSegment.LastValue;
		}
		set
		{
			LinearCurveSegment.LastValue = value;
		}
	}

	public LinearCurveSegmentDefinitionViewModel(CurveAdapter curve, int segmentDefIndex, CurveEditingContext owner)
		: base(curve, segmentDefIndex, owner)
	{
	}
}

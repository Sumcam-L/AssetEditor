using System.Drawing;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class ConstantCurveSegmentDefinitionViewModel : TwoControlPointSegmentDefinitionViewModel
{
	public override Pen SegmentPen => Pens.Red;

	private ConstantCurveSegmentAdapter ConstantCurveSegment => base.CurveSegmentDefinition?.Curve.As<ConstantCurveSegmentAdapter>();

	protected override float FirstYValue
	{
		get
		{
			return ConstantCurveSegment.ConstantValue;
		}
		set
		{
			ConstantCurveSegment.ConstantValue = value;
		}
	}

	protected override float LastYValue
	{
		get
		{
			return ConstantCurveSegment.ConstantValue;
		}
		set
		{
			ConstantCurveSegment.ConstantValue = value;
		}
	}

	public ConstantCurveSegmentDefinitionViewModel(CurveAdapter curve, int segmentDefIndex, CurveEditingContext owner)
		: base(curve, segmentDefIndex, owner)
	{
	}
}

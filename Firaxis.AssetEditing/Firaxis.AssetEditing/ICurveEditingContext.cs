using System.Drawing;

namespace Firaxis.AssetEditing;

public interface ICurveEditingContext
{
	PointF MenuCurvePosition { get; set; }

	void NudgeSelectedPointsDown();

	void NudgeSelectedPointsLeft();

	void NudgeSelectedPointsRight();

	void NudgeSelectedPointsUp();

	void SelectNextPoint();

	void SelectPreviousPoint();

	void RemoveSelectedCurveSegments();

	void InsertConstantCurveSegmentDefinition(float segmentBegin, float value);

	void InsertLinearCurveSegmentDefinition(float segmentBegin, float firstValue);
}

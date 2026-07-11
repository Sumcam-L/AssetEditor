using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface ICurve : ICurveSegment
{
	IEnumerable<ICurveSegmentDefinition> CurveSegments { get; }

	bool IsEmpty { get; }

	void AddCurveSegment(ICurveSegmentDefinition Curve);

	void RemoveCurveSegment(ICurveSegmentDefinition Curve);

	void ClearCurveSegments();
}

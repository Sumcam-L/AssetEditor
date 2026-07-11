using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface ICurveClass : IClassEntity, ICloudEntity, INameProvider, IVersionedData
{
	IEnumerable<CurveSegmentType> AllowedCurveTypes { get; }

	void AllowCurveType(CurveSegmentType type);

	bool IsCurveTypeAllowed(CurveSegmentType type);

	void ClearAllowedCurveTypes();
}

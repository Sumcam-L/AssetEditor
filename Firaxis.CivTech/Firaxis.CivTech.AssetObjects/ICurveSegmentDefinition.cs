using System;

namespace Firaxis.CivTech.AssetObjects;

public interface ICurveSegmentDefinition : IAssemblyInstance, IDisposable, IComparable<ICurveSegmentDefinition>
{
	float StartingPoint { get; set; }

	ICurveSegment Curve { get; }
}
public interface ICurveSegmentDefinition<T> : ICurveSegmentDefinition, IAssemblyInstance, IDisposable, IComparable<ICurveSegmentDefinition> where T : ICurveSegment
{
	T TypedCurve { get; }
}

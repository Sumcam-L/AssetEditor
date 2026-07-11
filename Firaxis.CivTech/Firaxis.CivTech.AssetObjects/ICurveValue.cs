namespace Firaxis.CivTech.AssetObjects;

public interface ICurveValue : IValue, ICurveSegment
{
	ICurve ParameterValue { get; set; }
}

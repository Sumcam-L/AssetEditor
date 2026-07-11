namespace Firaxis.CivTech.AssetObjects;

public interface IConstantCurveSegment : ICurveSegment
{
	float ConstantValue { get; set; }
}

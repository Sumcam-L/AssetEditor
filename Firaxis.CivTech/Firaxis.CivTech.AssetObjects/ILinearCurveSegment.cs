namespace Firaxis.CivTech.AssetObjects;

public interface ILinearCurveSegment : ICurveSegment
{
	float FirstValue { get; set; }

	float LastValue { get; set; }
}

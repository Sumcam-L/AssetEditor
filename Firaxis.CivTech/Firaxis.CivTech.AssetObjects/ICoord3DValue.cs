namespace Firaxis.CivTech.AssetObjects;

public interface ICoord3DValue : IValue
{
	Point3F ParameterValue { get; set; }
}

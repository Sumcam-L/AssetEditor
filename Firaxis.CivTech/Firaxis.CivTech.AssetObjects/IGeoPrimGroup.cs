namespace Firaxis.CivTech.AssetObjects;

public interface IGeoPrimGroup
{
	string Name { get; }

	uint NumFirstPrim { get; }

	uint NumPrims { get; }

	IGeometryInstance Geo { get; }
}

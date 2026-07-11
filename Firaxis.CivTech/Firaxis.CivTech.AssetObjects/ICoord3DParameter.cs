namespace Firaxis.CivTech.AssetObjects;

public interface ICoord3DParameter : IParameter
{
	float DefaultX { get; set; }

	float DefaultY { get; set; }

	float DefaultZ { get; set; }
}

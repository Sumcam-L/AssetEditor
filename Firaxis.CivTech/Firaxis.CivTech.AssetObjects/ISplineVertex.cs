namespace Firaxis.CivTech.AssetObjects;

public interface ISplineVertex
{
	Point3F Position { get; set; }

	bool SharpCorner { get; set; }
}

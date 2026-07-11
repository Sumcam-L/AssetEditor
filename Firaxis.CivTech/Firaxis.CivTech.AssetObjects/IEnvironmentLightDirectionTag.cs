namespace Firaxis.CivTech.AssetObjects;

public interface IEnvironmentLightDirectionTag
{
	float X { get; }

	float Y { get; }

	float Z { get; }

	float Diameter { get; set; }

	float AngularFalloff { get; set; }

	IFloatVector3 Intensity { get; }

	bool CastsShadows { get; set; }

	string Name { get; set; }

	void SetDirection(float x, float y, float z);

	void SetIntensity(float x, float y, float z);
}

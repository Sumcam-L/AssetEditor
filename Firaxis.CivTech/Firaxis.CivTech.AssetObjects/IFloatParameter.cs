namespace Firaxis.CivTech.AssetObjects;

public interface IFloatParameter : IParameter
{
	float Min { get; }

	float Max { get; }

	float Default { get; set; }

	void SetRange(float fMin, float fMax);
}

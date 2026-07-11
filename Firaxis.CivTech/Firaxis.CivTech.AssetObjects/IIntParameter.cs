namespace Firaxis.CivTech.AssetObjects;

public interface IIntParameter : IParameter
{
	int Min { get; }

	int Max { get; }

	int Default { get; set; }

	void SetRange(int iMin, int iMax);
}

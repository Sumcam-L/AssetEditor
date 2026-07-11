namespace Firaxis.CivTech.AssetObjects;

public interface IArtDefRefValue : IValue
{
	string ElementName { get; set; }

	string RootCollectionName { get; set; }

	string ArtDefPath { get; set; }

	string TemplateName { get; set; }

	bool IsCollectionLocked { get; set; }

	void SetFromInfo(ArtDefReferenceInfo info);
}

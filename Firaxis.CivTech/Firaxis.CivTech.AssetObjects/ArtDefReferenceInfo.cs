namespace Firaxis.CivTech.AssetObjects;

public struct ArtDefReferenceInfo
{
	public string elementName;

	public string collectionName;

	public string artDefPath;

	public string templateName;

	public bool isCollectionLocked;

	public ArtDefReferenceInfo(IArtDefRefValue value)
	{
		elementName = value.ElementName;
		collectionName = value.RootCollectionName;
		artDefPath = value.ArtDefPath;
		templateName = value.TemplateName;
		isCollectionLocked = value.IsCollectionLocked;
	}
}

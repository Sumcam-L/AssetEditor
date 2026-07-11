using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IArtDefRefParameter : IParameter
{
	bool IsNullAllowed { get; set; }

	string DefaultElementName { get; set; }

	string DefaultCollectionName { get; set; }

	string DefaultArtDefPath { get; set; }

	string DefaultTemplateName { get; set; }

	bool CollectionIsLocked { get; set; }

	IEnumerable<string> AllowedCollectionNames { get; }

	IEnumerable<string> AllowedArtDefTemplateNames { get; }

	void AddAllowedCollection(string collectionName);

	void RemoveAllowedCollection(string collectionName);

	void ClearAllowedCollections();

	void AddAllowedArtDefTemplate(string artDefTemplateName);

	void RemoveAllowedArtDefTemplate(string artDefTemplateName);

	void ClearAllowedArtDefTemplates();
}

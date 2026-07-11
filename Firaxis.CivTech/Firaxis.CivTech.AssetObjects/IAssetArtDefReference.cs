using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IAssetArtDefReference
{
	string TemplateName { get; set; }

	string CollectionName { get; set; }

	IEnumerable<string> Tags { get; }

	string FlattenTagsToString();

	void SetTagsFromString(string tags);

	void AddTag(string tag);

	void RemoveTag(string tag);

	void ClearTags();
}

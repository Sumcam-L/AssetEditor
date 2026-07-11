using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface ICloudEntity : INameProvider, IVersionedData
{
	bool IsClassEntity { get; }

	string Description { get; set; }

	IEnumerable<string> Tags { get; }

	string FlattenTagsToString();

	void SetTagsFromString(string tags);

	void AddTag(string tag);

	void RemoveTag(string tag);

	void ClearTags();
}

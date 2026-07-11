using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IArtDefCollection
{
	string CollectionName { get; set; }

	bool ReplaceMergedCollectionElements { get; set; }

	IEnumerable<IArtDefElement> Elements { get; }

	IArtDefElement AddElement(string name);

	void RemoveElement(string name);
}

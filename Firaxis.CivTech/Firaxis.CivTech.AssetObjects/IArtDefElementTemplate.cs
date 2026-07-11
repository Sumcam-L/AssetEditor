using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IArtDefElementTemplate
{
	string Name { get; set; }

	string Description { get; set; }

	string CustomEditor { get; set; }

	bool HasCustomEditor { get; }

	IParameterSet Parameters { get; }

	bool AppendMergedParameterCollections { get; set; }

	bool ReplaceMergedCollectionElements { get; set; }

	IEnumerable<IArtDefElementTemplate> Children { get; }

	IArtDefElementTemplate AddChild(string name);

	void RemoveChild(string name);
}

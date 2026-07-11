using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IArtDefElement
{
	string Name { get; set; }

	IValueSet Fields { get; }

	bool AppendMergedParameterCollections { get; set; }

	IEnumerable<IArtDefCollection> Children { get; }

	IArtDefCollection AddCollection(string name, IArtDefElementTemplate template);

	void RemoveCollection(string name);

	void UpdateCollectionsFromTemplate(IArtDefElementTemplate artDefTmpl);

	string SerializeToXML();

	bool DeserializeFromXML(string xml);
}

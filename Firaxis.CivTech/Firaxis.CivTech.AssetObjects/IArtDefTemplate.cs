using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IArtDefTemplate
{
	string Name { get; set; }

	string Description { get; set; }

	IEnumerable<IArtDefElementTemplate> Collections { get; }

	IArtDefElementTemplate Add(string name);

	void Remove(string name);
}

using System.Collections.Generic;

namespace Firaxis.CivTech;

public interface IEntityCacheData
{
	string Name { get; }

	string Class { get; }

	string Project { get; }

	IEnumerable<string> Tags { get; }
}

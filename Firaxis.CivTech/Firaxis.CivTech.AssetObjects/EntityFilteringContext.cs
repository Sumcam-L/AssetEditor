using System.Collections.Generic;
using System.Linq;

namespace Firaxis.CivTech.AssetObjects;

public class EntityFilteringContext : IEntityFilteringContext
{
	public ICollection<IEntityFilterDefinition> FilterDefinitions { get; private set; } = new List<IEntityFilterDefinition>();

	public IEntityFilterSet CreateFilterSet()
	{
		return new EntityFilterSet(FilterDefinitions.Select((IEntityFilterDefinition def) => def.CreateFilter()).ToArray());
	}
}

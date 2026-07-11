using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IEntityFilteringContext
{
	ICollection<IEntityFilterDefinition> FilterDefinitions { get; }

	IEntityFilterSet CreateFilterSet();
}

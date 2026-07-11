using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IEntityFilterSet
{
	IEnumerable<IEntityFilter> Filters { get; }

	IEnumerable<EntityID> FilterEntities(IEnumerable<EntityID> entities);
}

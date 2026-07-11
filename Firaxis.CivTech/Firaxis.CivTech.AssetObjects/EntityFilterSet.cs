using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Firaxis.CivTech.AssetObjects;

public class EntityFilterSet : IEntityFilterSet
{
	public IEnumerable<IEntityFilter> Filters { get; private set; }

	public EntityFilterSet(IEnumerable<IEntityFilter> filters)
	{
		Filters = filters;
	}

	public IEnumerable<EntityID> FilterEntities(IEnumerable<EntityID> entities)
	{
		ConcurrentBag<EntityID> filteredEntities = new ConcurrentBag<EntityID>();
		IEnumerable<IEntityFilter> sortedFilters = Filters.OrderBy((IEntityFilter x) => x.GetRanking()).ToArray();
		Parallel.ForEach(entities, delegate(EntityID entityID)
		{
			if (sortedFilters.All((IEntityFilter filter) => filter.PassesFilter(entityID)))
			{
				filteredEntities.Add(entityID);
			}
		});
		return filteredEntities;
	}
}

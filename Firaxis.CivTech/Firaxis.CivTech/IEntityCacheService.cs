using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech;

public interface IEntityCacheService
{
	void HandleProjectChange();

	IQueryService GetDependents(string entityName, InstanceType entityType);

	IEnumerable<string> FindFilesByType(InstanceType type);

	IEnumerable<EntityID> GetAllEntities();

	IEnumerable<string> GetAllTags();

	IEnumerable<IEntityCacheData> GetCacheData(EntityID entityID);

	IEnumerable<IEntityCacheData> GetCacheData(IEnumerable<EntityID> entityIDs);
}

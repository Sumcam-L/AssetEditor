using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IChangedEntityProvider
{
	IEnumerable<EntityID> GetChangedEntities();
}

using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IEntityContainerEntity : IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
	IEnumerable<string> GetContainedEntityNames(InstanceType entityType);

	bool AddEntity(IInstanceEntity entity, InstanceType type);

	bool RemoveEntity(IInstanceEntity entity, InstanceType type);
}

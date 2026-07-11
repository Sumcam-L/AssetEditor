using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IEnvironmentLightInstance : ILightInstance, IImportedEntity, IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
	IEnumerable<IEnvironmentLightDirectionTag> DirectionTags { get; }

	IEnvironmentLightDirectionTag AddDirectionTag(float x, float y, float z);

	void RemoveDirectionTag(IEnvironmentLightDirectionTag tag);

	void ClearDirectionTags();
}

using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IAnimationInstance : IImportedEntity, IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
	float Duration { get; set; }
}

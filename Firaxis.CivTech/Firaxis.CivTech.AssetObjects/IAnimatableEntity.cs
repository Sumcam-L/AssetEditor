using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IAnimatableEntity : IAnimatable, IEntityContainerEntity, IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
}

using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IMaterialInstance : IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
}

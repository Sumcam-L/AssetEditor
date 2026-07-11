using System;

namespace Firaxis.CivTech.AssetObjects;

public interface ILightInstance : IImportedEntity, IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
}

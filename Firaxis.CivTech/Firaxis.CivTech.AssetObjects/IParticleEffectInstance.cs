using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IParticleEffectInstance : IImportedEntity, IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
}

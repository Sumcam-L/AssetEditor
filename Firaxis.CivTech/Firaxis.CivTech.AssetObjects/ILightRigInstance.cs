using System;

namespace Firaxis.CivTech.AssetObjects;

public interface ILightRigInstance : IAnimatableEntity, IAnimatable, IEntityContainerEntity, IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
	ILightReferenceCollection LightReferences { get; }
}

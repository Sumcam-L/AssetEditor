using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IAnalyticLightInstance : ILightInstance, IImportedEntity, IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
}

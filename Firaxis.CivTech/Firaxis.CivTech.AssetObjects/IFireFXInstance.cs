using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IFireFXInstance : IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
	IFireFXInstanceData InstanceData { get; }

	void HandleClassChange();
}

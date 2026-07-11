using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IBehaviorInstance : IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>, IBehaviorDataProvider, IAnimatable
{
	IEnumerable<string> ReferenceGeometries { get; }

	void AddReferenceGeometry(string geoName);

	void RemoveReferenceGeometry(string geoName);

	IEnumerable<EntityID> GetDependentAssets();
}

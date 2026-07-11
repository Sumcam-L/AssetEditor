using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IGeometryInstance : IImportedEntity, IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
	IEnumerable<IGeoMesh> GeometryMeshes { get; }

	IEnumerable<string> BoneNames { get; }

	string ModelName { get; }

	IEnumerable<Lod> Lods { get; }

	bool HasBone(string bone);
}

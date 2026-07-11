using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IGeometryInstanceBuildable : IGeometryInstance, IImportedEntity, IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
	IGeoMesh AddGeometryMesh(string meshName);

	void AddBone(string bone);

	void SetModelName(string name);

	void ClearGeometryMeshes();

	void ClearBones();

	void AddLod(Lod lod);
}

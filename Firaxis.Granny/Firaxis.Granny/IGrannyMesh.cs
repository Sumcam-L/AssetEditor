using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.Granny;

public interface IGrannyMesh : IDisposable
{
	string Name { get; set; }

	int VertexCount { get; }

	int IndexCount { get; }

	MeshBounds BoundingBox { get; }

	List<IGrannyMaterial> MaterialBindings { get; }

	List<IGrannyTriMaterialGroup> TriangleMaterialGroups { get; }

	List<string> BoneBindings { get; }

	bool AddMaterialBinding(IGrannyMaterial kMaterial);

	bool RemoveMaterialBinding(IGrannyMaterial kMaterial);
}

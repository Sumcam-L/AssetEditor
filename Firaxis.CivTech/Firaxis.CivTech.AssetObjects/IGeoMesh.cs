using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IGeoMesh
{
	string Name { get; }

	uint VertexCount { get; set; }

	uint PrimitiveCount { get; set; }

	uint BoundBoneCount { get; set; }

	IEnumerable<IGeoPrimGroup> GeoPrimGroups { get; }

	IGeometryInstance Geo { get; }

	IGeoPrimGroup AddGeoPrimGroup(string name, uint numFirstPrim, uint numPrims);
}

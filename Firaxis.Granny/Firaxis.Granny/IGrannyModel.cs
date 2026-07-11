using System.Collections.Generic;

namespace Firaxis.Granny;

public interface IGrannyModel : IGrannyReferenceStorage
{
	string Name { get; set; }

	IGrannySkeleton Skeleton { get; }

	IGrannyTransform InitialPlacement { get; }

	List<IGrannyMesh> MeshBindings { get; }

	List<IGrannyLod> Lods { get; }

	void RemoveMeshBinding(IGrannyMesh kMesh);
}

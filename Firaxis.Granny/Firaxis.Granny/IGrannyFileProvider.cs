using Firaxis.Collections;

namespace Firaxis.Granny;

public interface IGrannyFileProvider
{
	ListEvent<IGrannyFile> Files { get; }

	ListEvent<IGrannyAnimation> Animations { get; }

	ListEvent<IGrannyMesh> Meshes { get; }

	ListEvent<IGrannyModel> Models { get; }
}

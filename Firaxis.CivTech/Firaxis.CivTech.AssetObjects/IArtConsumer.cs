using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IArtConsumer
{
	string ConsumerName { get; set; }

	IEnumerable<string> RelativeArtDefPaths { get; set; }

	IEnumerable<string> ReferencedLibraries { get; set; }

	bool LoadsLibraries { get; set; }

	void AddArtDefPath(string path);

	bool RemoveArtDefPath(string path);

	void ClearRelativeArtDefs();

	void AddLibrary(string libraryName);

	bool RemoveLibrary(string libraryName);

	void ClearLibraries();
}

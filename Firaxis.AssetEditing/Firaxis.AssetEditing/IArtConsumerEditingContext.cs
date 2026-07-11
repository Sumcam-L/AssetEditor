using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public interface IArtConsumerEditingContext
{
	DomNode DomNode { get; }

	string ConsumerName { get; set; }

	bool LoadsLibraries { get; set; }

	IEnumerable<RelativePathAdapter> RelativePaths { get; }

	IEnumerable<LibraryReferenceAdapter> LibraryReferences { get; }

	string[] LibraryNames { get; }

	bool IsValidName(string consumerName);

	void AddRelativeArtDefPath();

	void RemoveRelativeArtDefPaths(IEnumerable<string> paths);

	void AddLibraryReferences(string libraryName);

	void RemoveLibraryReferences(IEnumerable<string> libraryNames);
}

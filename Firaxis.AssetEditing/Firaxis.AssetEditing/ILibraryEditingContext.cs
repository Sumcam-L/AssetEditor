using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public interface ILibraryEditingContext
{
	DomNode DomNode { get; }

	string LibraryName { get; set; }

	IEnumerable<RelativePathAdapter> RelativePaths { get; }

	bool IsNewLibrary { get; }

	IEnumerable<string> GetReferencingConsumerNames();

	bool IsValidName(string libraryName);

	void AddRelativePath();

	void RemoveRelativePaths(IEnumerable<string> paths);
}

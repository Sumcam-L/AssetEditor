using System.Collections.Generic;

namespace Firaxis.AssetEditing;

public interface IRelativePathHost
{
	string Name { get; }

	HostType Type { get; }

	IEnumerable<string> RelativePaths { get; }

	void AddRelativePath(string path);

	void RemoveRelativePath(string path);
}

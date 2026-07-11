using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IGameLibrary
{
	string LibraryName { get; set; }

	IEnumerable<string> RelativePackagePaths { get; }

	bool AddPath(string path);

	bool RemovePath(string path);

	void ClearPaths();
}

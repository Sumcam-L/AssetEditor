using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetEditing;

public class GameLibraryPathHost : IRelativePathHost
{
	private readonly IGameLibrary _library;

	public string Name => _library.LibraryName;

	public HostType Type => HostType.GameLibrary;

	public IEnumerable<string> RelativePaths => _library.RelativePackagePaths;

	public GameLibraryPathHost(IGameLibrary library)
	{
		_library = library;
	}

	public void AddRelativePath(string path)
	{
		_library.AddPath(path);
	}

	public void RemoveRelativePath(string path)
	{
		_library.RemovePath(path);
	}
}

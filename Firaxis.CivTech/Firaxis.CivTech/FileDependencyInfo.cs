namespace Firaxis.CivTech;

public class FileDependencyInfo
{
	public long Timestamp = 0L;

	public uint Changelist = 0u;

	public IFileDependencyCatalog<string> Dependencies = new FileDependencyCatalog<string>();

	public IFileDependencyCatalog<string> Dependants = new FileDependencyCatalog<string>();

	public IFileCatalog<string> Files = new FileCatelog<string>();
}

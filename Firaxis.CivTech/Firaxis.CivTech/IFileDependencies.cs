namespace Firaxis.CivTech;

public interface IFileDependencies<TKey>
{
	long Timestamp { get; set; }

	uint Changelist { get; set; }

	IFileDependencyCatalog<TKey> Dependencies { get; }

	IFileDependencyCatalog<TKey> Dependants { get; }

	IFileCatalog<TKey> Files { get; }
}

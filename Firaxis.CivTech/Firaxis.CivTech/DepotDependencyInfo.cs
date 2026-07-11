namespace Firaxis.CivTech;

public class DepotDependencyInfo
{
	public long Timestamp = 0L;

	public uint Changelist = 0u;

	public IDependencyCatalog Dependencies = new DependencyCatalog();

	public IDependencyCatalog Dependants = new DependencyCatalog();

	public IDepotCatalog Files = new FileDepotCatelog();
}

namespace Firaxis.CivTech;

public interface IDatabaseDependencies
{
	long Timestamp { get; set; }

	uint Changelist { get; set; }

	IDependencyCatalog Dependencies { get; }

	IDependencyCatalog Dependants { get; }

	IDepotCatalog Files { get; }
}

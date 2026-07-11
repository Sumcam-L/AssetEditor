namespace Firaxis.VersionControl;

public interface IVersionControlDepot
{
	string Name { get; }

	DepotType Type { get; }
}

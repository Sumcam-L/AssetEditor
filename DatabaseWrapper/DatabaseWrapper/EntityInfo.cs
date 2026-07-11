using Firaxis.CivTech.AssetObjects;

namespace DatabaseWrapper;

public struct EntityInfo
{
	public readonly EntityID ID;

	public readonly string DepotPath;

	public EntityInfo(EntityID id, string depotPath)
	{
		ID = id;
		DepotPath = depotPath;
	}
}

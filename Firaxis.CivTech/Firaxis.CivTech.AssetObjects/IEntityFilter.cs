namespace Firaxis.CivTech.AssetObjects;

public interface IEntityFilter
{
	bool PassesFilter(EntityID entity);

	int GetRanking();
}

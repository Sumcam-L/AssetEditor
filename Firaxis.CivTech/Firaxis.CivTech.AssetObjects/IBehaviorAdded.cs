namespace Firaxis.CivTech.AssetObjects;

public interface IBehaviorAdded : IEntityChangedEvent
{
	string BehaviorName { get; set; }
}

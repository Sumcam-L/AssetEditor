namespace Firaxis.CivTech.AssetObjects;

public interface IBehaviorRemoved : IEntityChangedEvent
{
	string BehaviorName { get; set; }
}

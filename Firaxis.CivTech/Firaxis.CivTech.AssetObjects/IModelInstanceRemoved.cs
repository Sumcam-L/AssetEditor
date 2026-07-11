namespace Firaxis.CivTech.AssetObjects;

public interface IModelInstanceRemoved : IEntityChangedEvent
{
	string ModelName { get; set; }
}

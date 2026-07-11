namespace Firaxis.CivTech.AssetObjects;

public interface IModelInstanceChanged : IEntityChangedEvent
{
	string ModelName { get; set; }

	string GeoName { get; set; }

	IModelInstance Model { get; set; }
}

namespace Firaxis.CivTech.AssetObjects;

public interface ILightTagDirectionRemoved : IEntityChangedEvent
{
	string LightTagDirectionName { get; set; }
}

namespace Firaxis.CivTech.AssetObjects;

public interface ILightTagDirectionChanged : IEntityChangedEvent
{
	IEnvironmentLightDirectionTag LightTag { get; set; }
}

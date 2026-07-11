namespace Firaxis.CivTech.AssetObjects;

public interface IAssetDSGChanged : IEntityChangedEvent
{
	string DSGName { get; set; }
}

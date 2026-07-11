namespace Firaxis.CivTech.AssetObjects;

public interface IAssetTimelineRemoved : IEntityChangedEvent
{
	string TimelineName { get; set; }
}

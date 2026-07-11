namespace Firaxis.CivTech.AssetObjects;

public interface IAssetTimelineChanged : IEntityChangedEvent
{
	ITimeline ChangedTimeline { get; set; }
}

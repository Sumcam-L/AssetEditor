namespace Firaxis.CivTech.AssetObjects;

public interface IAssetTimelineSetChanged : IEntityChangedEvent
{
	void SlotRemoved(string slotName);

	void SlotChanged(string slotName, string timelineName);
}

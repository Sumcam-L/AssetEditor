namespace Firaxis.CivTech.AssetObjects;

public interface IAssetAnimationSetChanged : IEntityChangedEvent
{
	void SlotRemoved(string slotName);

	void SlotChanged(string slotName, string animName);
}

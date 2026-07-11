namespace Firaxis.CivTech.AssetObjects;

public static class TriggerExtensions
{
	public static bool IsAssetTrigger(this ITrigger trigger)
	{
		return trigger.Type == TriggerType.TT_ASSET_VFX;
	}
}

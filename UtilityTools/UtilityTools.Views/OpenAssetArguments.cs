using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.Views;

public class OpenAssetArguments
{
	public IInstanceEntity Entity { get; set; }

	public string PreviewModuleName { get; set; }

	public int SlotID { get; set; }
}

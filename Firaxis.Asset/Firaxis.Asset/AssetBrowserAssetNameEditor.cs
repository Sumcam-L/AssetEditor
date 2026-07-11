using Firaxis.AssetBrowser;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.Asset;

public class AssetBrowserAssetNameEditor : AssetBrowserNameEditor
{
	public AssetBrowserAssetNameEditor()
		: base(CivTechRegistry.CivTechService, CivTechRegistry.EntityFilteringService, new InstanceType[1])
	{
	}
}

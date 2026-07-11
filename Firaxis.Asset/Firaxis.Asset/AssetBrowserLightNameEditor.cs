using Firaxis.AssetBrowser;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.Asset;

public class AssetBrowserLightNameEditor : AssetBrowserNameEditor
{
	public AssetBrowserLightNameEditor()
		: base(CivTechRegistry.CivTechService, CivTechRegistry.EntityFilteringService, new InstanceType[1] { InstanceType.IT_ANALYTIC_LIGHT })
	{
	}
}

using Firaxis.AssetCloudFramework;
using Firaxis.CivTech;

namespace Firaxis.ATF;

public class BasicAssetCloudSettingService : IAssetCloudSettingService
{
	public IAssetCloudSettings AssetCloudSettings { get; private set; }

	public BasicAssetCloudSettingService()
	{
		AssetCloudSettings = new AssetCloudSettings();
		AssetCloudSettings.UseLocalConfig = false;
		AssetCloudSettings.UseLocalToolHost = false;
	}
}

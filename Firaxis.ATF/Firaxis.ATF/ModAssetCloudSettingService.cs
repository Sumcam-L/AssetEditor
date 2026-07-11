using System.IO;
using Firaxis.AssetCloudFramework;
using Firaxis.CivTech;

namespace Firaxis.ATF;

public class ModAssetCloudSettingService : IAssetCloudSettingService
{
	public IAssetCloudSettings AssetCloudSettings { get; private set; }

	public ModAssetCloudSettingService(string gameFolder)
	{
		AssetCloudSettings = new AssetCloudSettings();
		AssetCloudSettings.UseLocalConfig = false;
		AssetCloudSettings.UseLocalToolHost = false;
		AssetCloudSettings.AssetCooker = Path.Combine(gameFolder, "Civ6AssetCooker_Win64_null_FinalRelease.exe");
		AssetCloudSettings.ToolHostLocal = (AssetCloudSettings.ToolHost = Path.Combine(gameFolder, "Civ6ToolHost_Win64_DX11_Release.dll"));
		AssetCloudSettings.ShowAssertions = false;
		AssetCloudSettings.ModTools = true;
		AssetCloudSettings.ActiveConfigPath = Path.Combine(gameFolder, "civ6.cfg");
	}
}

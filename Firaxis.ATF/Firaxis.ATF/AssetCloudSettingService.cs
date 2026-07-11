using System.ComponentModel.Composition;
using Firaxis.AssetCloudFramework;
using Firaxis.CivTech;
using Firaxis.CivTech.Properties;
using Firaxis.Error;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IAssetCloudSettingService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AssetCloudSettingService : IAssetCloudSettingService
{
	private AssetCloudSettings m_settings;

	public virtual IAssetCloudSettings AssetCloudSettings => m_settings;

	[ImportingConstructor]
	public AssetCloudSettingService()
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			LoadAssetCloudSettings();
			ValidateExecutableSettings(m_settings);
		}
	}

	public AssetCloudSettingService(bool useLocalToolHost, bool useLocalConfig, string localCfgPath)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			LoadAssetCloudSettings();
			AssetCloudSettings.UseLocalToolHost = useLocalToolHost;
			AssetCloudSettings.UseLocalConfig = useLocalConfig;
			AssetCloudSettings.ProjectConfigLocal = localCfgPath;
		}
	}

	private void LoadAssetCloudSettings()
	{
		m_settings = new AssetCloudSettings(Firaxis.CivTech.Properties.Resources.ModTools);
		Outputs.WriteLine(OutputMessageType.Info, "Loading toolchain settings...");
		if (!m_settings.Load())
		{
			Outputs.WriteLine(OutputMessageType.Info, "Using default toolchain settings...");
			m_settings.Save();
		}
	}

	private void ValidateExecutableSettings(AssetCloudSettings settings)
	{
		Outputs.WriteLine(OutputMessageType.Info, "Validating installed components...");
		string configName = "Civ6";
		if (!settings.ValidateAndUpdateReleaseToolHost(configName))
		{
			string text = "The released tool host could not be found! The content creation tools will not function until you reinstall the asset cloud installer.";
			if (MessageBoxes.Show(text, "Asset Cloud", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Cancel)
			{
				ExceptionLogger.FatalExit(-1, text, "Asset Cloud");
			}
		}
		if (!settings.ValidateAndUpdateReleaseAssetCooker(configName))
		{
			string text2 = "The released cooker could not be found! Cooking via the asset cloud tray icon will not be available until you reinstall the asset cloud installer.";
			if (MessageBoxes.Show(text2, "Asset Cloud", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Cancel)
			{
				ExceptionLogger.FatalExit(-1, text2, "Asset Cloud");
			}
		}
		if (settings.UseLocalToolHost && !settings.ValidateUserLocalToolHost())
		{
			string text3 = "The tool host local override could not be found! Reverting to released tool components.";
			if (MessageBoxes.Show(text3, "Asset Cloud", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Cancel)
			{
				ExceptionLogger.FatalExit(-1, text3, "Asset Cloud");
			}
			settings.UseLocalToolHost = false;
		}
	}
}

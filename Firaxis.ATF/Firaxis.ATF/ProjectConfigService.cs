using System.ComponentModel.Composition;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Microsoft.Win32;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(IProjectConfigService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ProjectConfigService : IProjectConfigService
{
	private IAssetCloudSettings AssetCloudSettings { get; set; }

	private CivTechContext CivTechContext { get; set; }

	private ICommonConfigsRootProvider CommonConfigsRootProvider { get; set; }

	private ProjectInfo BaseGameInfo { get; set; }

	public virtual IProjectConfig Config { get; private set; }

	[ImportingConstructor]
	public ProjectConfigService(IAssetCloudSettingService acss, ICommonConfigsRootProvider bgvcs, IProjectSelectionService pss)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			AssetCloudSettings = acss.AssetCloudSettings;
			CommonConfigsRootProvider = bgvcs;
			if (!pss.Projects.ContainsProject("Civ6"))
			{
				throw new ProjectConfigProjectPathsException("Civ6");
			}
			BaseGameInfo = pss.Projects["Civ6"];
			CivTechContext = Context.EnsureCreated<CivTechContext>();
			LoadConfig();
		}
	}

	public virtual string GetConfigLocation(IAssetCloudSettings settings, IProjectRootProvider cfgProvider, ProjectInfo projectInfo)
	{
		string result = string.Empty;
		if (!settings.ModTools)
		{
			if (!settings.UseLocalConfig)
			{
				if (!string.IsNullOrEmpty(CommonConfigsRootProvider.ConfigPath) && File.Exists(CommonConfigsRootProvider.ConfigPath))
				{
					result = CommonConfigsRootProvider.ConfigPath;
				}
				else
				{
					if (string.IsNullOrEmpty(projectInfo.Config))
					{
						return null;
					}
					result = Path.Combine(cfgProvider.WorkspaceRoot, "Civ6", "pantry", "Civ6.cfg");
				}
			}
			else
			{
				result = settings.ProjectConfigLocal;
			}
		}
		else
		{
			RegistryKey toolsRegistryKey = settings.GetToolsRegistryKey("Civ6", "");
			if (toolsRegistryKey != null)
			{
				result = toolsRegistryKey.GetValue("ProjectConfig") as string;
			}
		}
		return result;
	}

	public virtual void LoadConfig()
	{
		Config = LoadConfig(AssetCloudSettings, CommonConfigsRootProvider, BaseGameInfo);
	}

	public virtual void LoadConfig(string xmlText)
	{
		Config = LoadConfigInternal(xmlText);
	}

	private IProjectConfig LoadConfig(IAssetCloudSettings settings, ICommonConfigsRootProvider cfgProvider, ProjectInfo projectInfo)
	{
		string configLocation = GetConfigLocation(settings, cfgProvider, projectInfo);
		if (!File.Exists(configLocation))
		{
			throw new ProjectConfigFilePathException(projectInfo.Name, configLocation);
		}
		IProjectConfig projectConfig = CivTechContext.CreateInstance<IProjectConfig>();
		if (!projectConfig.DeserializeFromFile(configLocation))
		{
			throw new ProjectConfigFilePathException(projectInfo.Name, configLocation);
		}
		Outputs.WriteLine(OutputMessageType.Info, "Using project config located at \"{0}\"", configLocation);
		return projectConfig;
	}

	private IProjectConfig LoadConfigInternal(string xmlText)
	{
		IProjectConfig projectConfig = CivTechContext.CreateInstance<IProjectConfig>();
		if (!projectConfig.DeserializeFromXML(xmlText))
		{
			throw new FileNotFoundException("Failed to load project config file from text!");
		}
		return projectConfig;
	}
}

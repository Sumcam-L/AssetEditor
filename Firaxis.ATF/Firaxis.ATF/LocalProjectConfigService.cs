using System.ComponentModel.Composition;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.ATF;

[Export(typeof(IProjectConfigService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class LocalProjectConfigService : IProjectConfigService
{
	private readonly string ProjectConfigFilePath;

	private IProjectRootProvider RootProvider { get; set; }

	public IProjectConfig Config { get; private set; }

	[ImportingConstructor]
	public LocalProjectConfigService(ICommonConfigsRootProvider rootProvider)
	{
		ProjectConfigFilePath = Path.Combine(rootProvider.WorkspaceRoot, "Civ6", "pantry", "Civ6.cfg");
		Config = Context.EnsureCreated<CivTechContext>().CreateInstance<IProjectConfig>();
		LoadConfig();
	}

	public string GetConfigLocation(IAssetCloudSettings settings, IProjectRootProvider cfgProvider, ProjectInfo projectInfo)
	{
		return ProjectConfigFilePath;
	}

	public void LoadConfig()
	{
		if (!File.Exists(ProjectConfigFilePath))
		{
			throw new ProjectConfigFilePathException("offline", ProjectConfigFilePath);
		}
		if (!Config.DeserializeFromFile(ProjectConfigFilePath))
		{
			throw new ProjectConfigFilePathException("offfline", ProjectConfigFilePath);
		}
	}

	public void LoadConfig(string xmlText)
	{
		if (!Config.DeserializeFromXML(xmlText))
		{
			throw new ProjectConfigException("offfline");
		}
	}
}

using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.ATF;

public class BasicProjectConfigService : IProjectConfigService
{
	private CivTechContext CivTechContext { get; set; }

	private string ProjectConfigPath { get; set; }

	public IProjectConfig Config { get; private set; }

	public BasicProjectConfigService(string projCfgPath)
	{
		CivTechContext = Context.EnsureCreated<CivTechContext>();
		ProjectConfigPath = projCfgPath;
		LoadConfig();
	}

	public string GetConfigLocation(IAssetCloudSettings settings, IProjectRootProvider cfgProvider, ProjectInfo projectInfo)
	{
		return ProjectConfigPath;
	}

	public void LoadConfig()
	{
		Config = CivTechContext.CreateInstance<IProjectConfig>();
		if (!Config.DeserializeFromFile(ProjectConfigPath))
		{
			throw new FileNotFoundException("Failed to find project config file!", ProjectConfigPath);
		}
	}

	public void LoadConfig(string xmlText)
	{
		Config = CivTechContext.CreateInstance<IProjectConfig>();
		if (!Config.DeserializeFromXML(xmlText))
		{
			throw new FileNotFoundException("Failed to serialize project config file from xml!");
		}
	}
}

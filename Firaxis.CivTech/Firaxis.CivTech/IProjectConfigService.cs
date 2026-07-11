using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech;

public interface IProjectConfigService
{
	IProjectConfig Config { get; }

	string GetConfigLocation(IAssetCloudSettings settings, IProjectRootProvider cfgProvider, ProjectInfo projectInfo);

	void LoadConfig();

	void LoadConfig(string xmlText);
}

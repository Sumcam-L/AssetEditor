namespace Firaxis.CivTech;

public interface ICommonConfigsRootProvider : IProjectRootProvider
{
	ToolsBuildType BuildType { get; }

	string ConfigPath { get; }

	string EnvironmentPath { get; }
}

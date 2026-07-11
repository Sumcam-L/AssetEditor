namespace Firaxis.CivTech;

public class ProjectConfigEnvironmentLoadException : ProjectConfigException
{
	private string m_assetCloudPath;

	public override string Message => $"Failed to load json from AssetCloud.env at location \"{m_assetCloudPath}\" for project \"{ProjectName}\"";

	public ProjectConfigEnvironmentLoadException(string projectName, string envPath)
		: base(projectName)
	{
		m_assetCloudPath = envPath;
	}
}

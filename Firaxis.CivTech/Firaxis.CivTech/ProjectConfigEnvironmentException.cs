namespace Firaxis.CivTech;

public class ProjectConfigEnvironmentException : ProjectConfigException
{
	private string m_localAssetCloudPath;

	private string m_sharedAssetCloudPath;

	public override string Message => $"Failed to find AssetCloud.env in both the local \"{m_localAssetCloudPath}\" and shared \"{m_sharedAssetCloudPath}\" locations for project \"{ProjectName}\"";

	public ProjectConfigEnvironmentException(string projectName, string localEnvPath, string sharedlEnvPath)
		: base(projectName)
	{
		m_localAssetCloudPath = localEnvPath;
		m_sharedAssetCloudPath = sharedlEnvPath;
	}
}

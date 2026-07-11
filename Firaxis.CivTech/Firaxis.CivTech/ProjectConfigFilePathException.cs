namespace Firaxis.CivTech;

public class ProjectConfigFilePathException : ProjectConfigException
{
	private readonly string ConfigPath;

	public override string Message => $"Failed to find project config file \"{ConfigPath}\" for project \"{ProjectName}\" !";

	public ProjectConfigFilePathException(string projectName, string configPath)
		: base(projectName)
	{
		ConfigPath = configPath;
	}
}

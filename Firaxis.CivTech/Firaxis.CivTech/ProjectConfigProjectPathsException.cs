namespace Firaxis.CivTech;

public class ProjectConfigProjectPathsException : ProjectConfigException
{
	public override string Message => $"Failed to find project path settings for project \"{ProjectName}\"";

	public ProjectConfigProjectPathsException(string projectName)
		: base(projectName)
	{
	}
}

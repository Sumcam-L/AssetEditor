namespace Firaxis.CivTech;

public class ProjectConfigVersionControlException : ProjectConfigException
{
	public override string Message => $"Failed to find version control settings for project \"{ProjectName}\"";

	public ProjectConfigVersionControlException(string projectName)
		: base(projectName)
	{
	}
}

using System;

namespace Firaxis.CivTech;

public class ProjectConfigException : Exception
{
	public readonly string ProjectName;

	public ProjectConfigException(string projectName)
	{
		ProjectName = projectName;
	}
}

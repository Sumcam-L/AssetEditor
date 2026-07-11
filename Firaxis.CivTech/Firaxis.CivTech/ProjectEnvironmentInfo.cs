using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Firaxis.CivTech;

public class ProjectEnvironmentInfo
{
	public IDictionary<string, ProjectInfo> Projects = new ConcurrentDictionary<string, ProjectInfo>();
}

using System.Collections.Generic;

namespace Firaxis.CivTech;

public interface IProjectInfoMap
{
	IEnumerable<ProjectInfo> ProjectInfos { get; }

	IEnumerable<string> ProjectNames { get; }

	ProjectInfo this[string name] { get; }

	bool AddProject(ProjectInfo project);

	bool AddAlternateKey(string primaryKey, string altKey);

	void RemoveProject(string name);

	bool ContainsProject(string name);

	bool GetProject(string name, ref ProjectInfo project);
}

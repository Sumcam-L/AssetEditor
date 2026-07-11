using System;
using System.Collections.Generic;

namespace Firaxis.CivTech;

public interface IProjectMap
{
	IEnumerable<ProjectEnvironment> Projects { get; }

	IEnumerable<string> ProjectNames { get; }

	ProjectEnvironment this[string name] { get; }

	ProjectEnvironment this[Guid id] { get; }

	bool AddProject(ProjectEnvironment project);

	bool GetProject(string name, ref ProjectEnvironment project);

	bool GetProject(Guid id, ref ProjectEnvironment project);

	bool ContainsProject(string name);

	bool ContainsProject(Guid id);

	void RemoveProject(string name);

	void RemoveProject(Guid id);
}

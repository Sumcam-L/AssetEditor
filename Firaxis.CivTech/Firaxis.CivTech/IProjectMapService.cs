using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech;

public interface IProjectMapService
{
	IVirtualPantry LayeredPantry { get; }

	ProjectEnvironment PrimaryProject { get; }

	IProjectMap ActiveProjectMap { get; }

	IProjectMap AllProjectsMap { get; }

	IEnumerable<string> GetActivePantryPaths();

	IEnumerable<string> GetProjectPantryPaths(ProjectEnvironment project);

	IEnumerable<string> GetProjectPantryPaths(string project);

	IEnumerable<ProjectPaths> GetActiveProjectPaths();

	IEnumerable<ProjectEnvironment> GetProjectAndDependencies(ProjectEnvironment project);

	void HandleProjectChange();

	string GetProjectNameFromUri(Uri uri);

	bool IsFromActiveProject(Uri uri);

	bool IsFromPrimaryProject(Uri uri);

	bool IsFromActiveProjectOrDependencies(Uri uri);

	bool IsFromProjectDependencies(Uri uri);

	bool IsFromActiveProject(EntityID entity);

	bool IsFromActiveProjectOrDependencies(EntityID entity);

	bool IsFromProjectDependencies(EntityID entity);
}

using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech;

public interface ICivTechService : IDisposable
{
	string AnyProject { get; }

	IProjectMapService ProjectMapService { get; }

	CivTechContext CivTechContext { get; }

	IToolHostLoaderService ToolHostLoader { get; }

	IProjectMap AllProjectsMap { get; }

	IProjectMap ActiveProjectMap { get; }

	ProjectEnvironment PrimaryProject { get; }

	IAssetCloudSettings AssetCloudSettings { get; }

	IEnumerable<string> GetActivePantryPaths();

	IEnumerable<string> GetActiveProjects();

	string GetBaseGamePantryPath();

	bool IsFromActiveProject(Uri uri);

	bool IsFromActiveProjectOrDependencies(Uri uri);

	bool IsFromProjectDependencies(Uri uri);

	bool IsFromModDependencies(Uri uri);

	bool IsFromPrimaryModProject(Uri uri);

	bool IsFromActiveProject(EntityID entity);

	bool IsFromActiveProjectOrDependencies(EntityID entity);

	bool IsFromProjectDependencies(EntityID entity);

	string GetBrowserDataPath();

	IWorkspaceDependencyRegistry GetWorkspaceDependencyRegistry(Uri uri);
}

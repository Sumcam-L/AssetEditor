using System;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;

namespace Firaxis.CivTech;

public static class ProjectMapServiceExtensions
{
	public static ResultCode GetEntityPath(this IProjectMapService projMapSvc, IInstanceEntity entity, out string entityPath)
	{
		entityPath = entity.GetXMLPath();
		if (!string.IsNullOrEmpty(entityPath))
		{
			return ResultCode.Success;
		}
		entityPath = projMapSvc.LayeredPantry.GetPrimaryPantryPath(entity.Name, entity.Type);
		return ResultCode.Success;
	}

	public static ResultCode GetEntityPath(this IProjectMapService projMapSvc, string entityName, InstanceType entityType, out string entityPath)
	{
		entityPath = projMapSvc.LayeredPantry.GetPantryPath(entityName, entityType);
		if (string.IsNullOrEmpty(entityPath))
		{
			entityPath = projMapSvc.LayeredPantry.GetPrimaryPantryPath(entityName, entityType);
		}
		return ResultCode.Success;
	}

	public static ResultCode GetProjectFromDepot(this IProjectMapService projMapSvc, string depotPath, ref ProjectEnvironment project)
	{
		if (string.IsNullOrEmpty(depotPath))
		{
			project = projMapSvc.PrimaryProject;
			return new ResultCode("Failed to get project from depot path because depot path was empty");
		}
		string text = string.Empty;
		foreach (ProjectEnvironment item in projMapSvc.ActiveProjectMap.Projects.Reverse())
		{
			text = item.VersionControl?.GetLocalPath(depotPath);
			if (!string.IsNullOrEmpty(text))
			{
				break;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			project = projMapSvc.PrimaryProject;
			return new ResultCode("Failed to get local path from depot path \"{0}\"", depotPath);
		}
		return projMapSvc.GetEntityProject(text, ref project);
	}

	public static ResultCode GetEntityProject(this IProjectMapService projMapSvc, IInstanceEntity entity, ref ProjectEnvironment project)
	{
		string xMLPath = entity.GetXMLPath();
		return projMapSvc.GetEntityProject(xMLPath, ref project);
	}

	public static ResultCode GetEntityProject(this IProjectMapService projMapSvc, string entityName, InstanceType entityType, ref ProjectEnvironment project)
	{
		string entityPath = string.Empty;
		ResultCode resultCode = projMapSvc.GetEntityPath(entityName, entityType, out entityPath);
		if ((bool)resultCode)
		{
			resultCode = projMapSvc.GetEntityProject(entityPath, ref project);
		}
		return resultCode;
	}

	public static ResultCode GetEntityProject(this IProjectMapService projMapSvc, string entityPath, ref ProjectEnvironment project)
	{
		if (string.IsNullOrEmpty(entityPath))
		{
			project = projMapSvc.PrimaryProject;
			return ResultCode.Success;
		}
		string projectName = projMapSvc.GetProjectName(new Uri(entityPath));
		if (string.IsNullOrEmpty(projectName))
		{
			return new ResultCode("Failed to determine project for entity with a path of \"{0}\"", entityPath);
		}
		if (!projMapSvc.ActiveProjectMap.ContainsProject(projectName))
		{
			return new ResultCode("Project \"{0}\" identified for entity with a path of \"{1}\" is not part of active project dependencies", projectName, entityPath);
		}
		project = projMapSvc.ActiveProjectMap[projectName];
		return ResultCode.Success;
	}

	public static string GetProjectName(this IProjectMapService projMapSvc, Uri uri)
	{
		return projMapSvc.GetProjectNameFromUri(uri);
	}

	public static string GetProjectName(this IProjectMapService projMapSvc, IInstanceEntity entity)
	{
		string result = projMapSvc.PrimaryProject.Name;
		try
		{
			result = projMapSvc.GetProjectNameFromUri(new Uri(entity.GetXMLPath()));
		}
		catch (Exception exObj)
		{
			BugSubmitter.SilentException(exObj);
		}
		return result;
	}
}

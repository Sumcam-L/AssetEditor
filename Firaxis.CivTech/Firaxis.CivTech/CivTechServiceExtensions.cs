using System;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;

namespace Firaxis.CivTech;

public static class CivTechServiceExtensions
{
	public static string GetProjectName(this ICivTechService civTechSvc, Uri uri)
	{
		return civTechSvc.ProjectMapService.GetProjectNameFromUri(uri);
	}

	public static string GetProjectName(this ICivTechService civTechSvc, IInstanceEntity entity)
	{
		string result = civTechSvc.ProjectMapService.PrimaryProject.Name;
		try
		{
			result = civTechSvc.ProjectMapService.GetProjectNameFromUri(new Uri(entity.GetXMLPath()));
		}
		catch (Exception exObj)
		{
			BugSubmitter.SilentException(exObj);
		}
		return result;
	}

	public static string GetEntityPath(this ICivTechService civTechSvc, IInstanceEntity entity)
	{
		civTechSvc.ProjectMapService.GetEntityPath(entity, out var entityPath);
		return entityPath;
	}

	public static string GetEntityPath(this ICivTechService civTechSvc, string entityName, InstanceType entityType)
	{
		civTechSvc.ProjectMapService.GetEntityPath(entityName, entityType, out var entityPath);
		return entityPath;
	}

	public static ResultCode GetEntityPath(this ICivTechService civTechSvc, IInstanceEntity entity, out string entityPath)
	{
		return civTechSvc.ProjectMapService.GetEntityPath(entity, out entityPath);
	}

	public static ResultCode GetEntityPath(this ICivTechService civTechSvc, string entityName, InstanceType entityType, out string entityPath)
	{
		return civTechSvc.ProjectMapService.GetEntityPath(entityName, entityType, out entityPath);
	}

	public static ResultCode GetEntityProject(this ICivTechService civTechSvc, IInstanceEntity entity, ref ProjectEnvironment project)
	{
		return civTechSvc.ProjectMapService.GetEntityProject(entity, ref project);
	}

	public static ResultCode GetEntityProject(this ICivTechService civTechSvc, string entityName, InstanceType entityType, ref ProjectEnvironment project)
	{
		return civTechSvc.ProjectMapService.GetEntityProject(entityName, entityType, ref project);
	}

	public static ResultCode GetEntityProject(this ICivTechService civTechSvc, string entityPath, ref ProjectEnvironment project)
	{
		return civTechSvc.ProjectMapService.GetEntityProject(entityPath, ref project);
	}
}

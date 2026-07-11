using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;

namespace Firaxis.ATF;

public static class CompositeDocumentUriExtensions
{
	public static IEnumerable<IInstanceEntity> GetDependentEntities(ICivTechService civTechSvc, string entityName, InstanceType entityType, IInstanceSet instanceSet)
	{
		IInstanceEntity entity = instanceSet.FindByNameAndType(entityName, entityType);
		if (entity != null)
		{
			yield break;
		}
		entity = instanceSet.LoadEntityByName(entityName, entityType);
		if (entity == null)
		{
			yield break;
		}
		yield return entity;
		IEnumerable<IInstanceEntity> entities = entity.CookParameters.GetEntities(civTechSvc, instanceSet);
		foreach (IInstanceEntity item in entities)
		{
			yield return item;
		}
	}

	public static IEnumerable<IInstanceEntity> GetEntities(this IValueSet valueSet, ICivTechService cvTechSvc, IInstanceSet instanceSet)
	{
		List<IInstanceEntity> list = new List<IInstanceEntity>();
		foreach (IObjectValue assignedObjectValue in GetAssignedObjectValues(valueSet))
		{
			string boundObjectName = assignedObjectValue.GetBoundObjectName();
			InstanceType boundObjectType = assignedObjectValue.GetBoundObjectType();
			list.AddRange(GetDependentEntities(cvTechSvc, boundObjectName, boundObjectType, instanceSet));
		}
		return list;
	}

	public static IEnumerable<Uri> GetGeneratedEntityUris(ICivTechService civTechSvc, string entityName, InstanceType entityType, IInstanceSet instanceSet)
	{
		IInstanceEntity entity = instanceSet.FindByNameAndType(entityName, entityType);
		if (entity != null)
		{
			yield break;
		}
		yield return new Uri(civTechSvc.GetEntityPath(entityName, entityType));
		entity = instanceSet.LoadEntityByName(entityName, entityType);
		if (entity == null)
		{
			yield break;
		}
		IClassEntity insCls = civTechSvc.PrimaryProject.Config.Classes.FindForInstance(entity);
		if (insCls == null)
		{
			BugSubmitter.SilentReport($"Failed to GetGeneratedEntityUris for entity of name \"{entityName}\" and type {entityType} because class \"{entity.ClassName}\" could not be found in the project config. @summary Failed to GetGeneratedEntityUris because of missing entity class @assign bwhitman");
			yield break;
		}
		foreach (IInstanceDataFile dataFile in entity.DataFiles.Where((IInstanceDataFile df) => !string.IsNullOrEmpty(df.RelativePath)))
		{
			IClassDataFile classDataFile = insCls.DataFiles.FirstOrDefault((IClassDataFile dfc) => dfc.ID == dataFile.ID);
			if (classDataFile == null)
			{
				Outputs.WriteLine(OutputMessageType.Info, "Stripping data file with ID \"{0}\" that no longer exists in the class entity \"{1}\" configuration.", dataFile.ID, insCls.Name);
			}
			else if (!classDataFile.IsGenerated)
			{
				yield return new Uri(entity.GetDataFilePath(dataFile.RelativePath));
			}
		}
		IEnumerable<Uri> generatedObjectValuesUris = entity.CookParameters.GetGeneratedObjectValuesUris(civTechSvc, instanceSet);
		foreach (Uri item in generatedObjectValuesUris)
		{
			yield return item;
		}
	}

	public static IEnumerable<Uri> GetGeneratedObjectValuesUris(this IValueSet valueSet, ICivTechService civTechSvc, IInstanceSet instanceSet)
	{
		List<Uri> list = new List<Uri>();
		foreach (IObjectValue assignedObjectValue in GetAssignedObjectValues(valueSet))
		{
			string boundObjectName = assignedObjectValue.GetBoundObjectName();
			InstanceType boundObjectType = assignedObjectValue.GetBoundObjectType();
			list.AddRange(GetGeneratedEntityUris(civTechSvc, boundObjectName, boundObjectType, instanceSet));
		}
		return list;
	}

	public static IEnumerable<Uri> GetSourceEntityUris(ICivTechService civTechSvc, string entityName, InstanceType entityType, IInstanceSet instanceSet)
	{
		IInstanceEntity entity = instanceSet.FindByNameAndType(entityName, entityType);
		if (entity != null)
		{
			yield break;
		}
		_ = civTechSvc.PrimaryProject.Paths.GamePantry;
		entity = instanceSet.LoadEntityByName(entityName, entityType);
		if (entity == null)
		{
			yield break;
		}
		if (entity is IImportedEntity importedEntity)
		{
			yield return new Uri(civTechSvc.PrimaryProject.VersionControl.GetLocalPath(importedEntity.SourceFilePath));
		}
		IEnumerable<Uri> sourceObjectValuesUris = entity.CookParameters.GetSourceObjectValuesUris(civTechSvc, instanceSet);
		foreach (Uri item in sourceObjectValuesUris)
		{
			yield return item;
		}
	}

	public static IEnumerable<Uri> GetSourceObjectValuesUris(this IValueSet valueSet, ICivTechService civTechSvc, IInstanceSet instanceSet)
	{
		List<Uri> list = new List<Uri>();
		_ = civTechSvc.PrimaryProject.Paths.GamePantry;
		foreach (IObjectValue assignedObjectValue in GetAssignedObjectValues(valueSet))
		{
			string boundObjectName = assignedObjectValue.GetBoundObjectName();
			InstanceType boundObjectType = assignedObjectValue.GetBoundObjectType();
			list.AddRange(GetSourceEntityUris(civTechSvc, boundObjectName, boundObjectType, instanceSet));
		}
		return list;
	}

	private static IEnumerable<IObjectValue> GetAssignedObjectValues(IValueSet valueSet)
	{
		return from val in valueSet.ItemsOfType<IObjectValue>()
			where !string.IsNullOrEmpty(val.GetBoundObjectName())
			select val;
	}
}

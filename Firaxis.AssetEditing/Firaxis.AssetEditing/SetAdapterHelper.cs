using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Sce.Atf;
using UtilityTools.Helpers;

namespace Firaxis.AssetEditing;

public static class SetAdapterHelper
{
	public static IEnumerable<EntityID> GetSelectedEntityIDs(IEnumerable<string> entityNames, InstanceType entityType)
	{
		ICollection<EntityID> collection = new HashSet<EntityID>();
		foreach (string entityName in entityNames)
		{
			EntityID item = new EntityID(entityName, entityType);
			collection.Add(item);
		}
		return collection;
	}

	public static void ImportSelectedEntities(IEnumerable<EntityID> entitiesToReimport, BaseEntityPropertyContext entityContext, bool recurseIntoChildren)
	{
		if (entitiesToReimport.Any())
		{
			BugSubmitter.SilentAssert(entityContext.InTransaction, "The context importing entities needs to be in a transaction!");
			IImportService importService = entityContext.ImportService;
			AssetBrowserFileCommands assetDocumentCommands = entityContext.AssetDocumentCommands;
			IDocumentRegistryMediator documentRegistryMediator = entityContext.DocumentRegistryMediator;
			BatchEntitySourceControlService sourceControl = entityContext.SourceControl;
			new EntityImporter(entityContext.CivTechService, importService, assetDocumentCommands, documentRegistryMediator, sourceControl, entitiesToReimport, recurseIntoChildren).Import();
		}
	}

	public static IEnumerable<EntityID> LaunchMiniImporter(ICivTechService civTechSvc, IFileWatcherService fileWatchSvc, IInstanceSet entitySet, IEntityCacheService entityCacheService, IEnumerable<string> allowedClasses, InstanceType entityType)
	{
		Predicate<IImportedEntity> canExport = (IImportedEntity ent) => true;
		IEnumerable<EntityID> enumerable = DialogHelper.LaunchMiniImporter(civTechSvc, fileWatchSvc, allowedClasses, entityType, entityCacheService, global::DatabaseWrapper.DatabaseWrapper.ImportEntities, canExport);
		if (enumerable.Any())
		{
			StringBuilder stringBuilder = new StringBuilder("The following files have been opened in Perforce:");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			foreach (EntityID item in enumerable)
			{
				IInstanceEntity instanceEntity = entitySet.LoadEntityIfUnique(item.Name, item.Type);
				IClassEntity classEntity = civTechSvc.PrimaryProject.Config.Classes.FindForInstance(instanceEntity);
				if (classEntity == null)
				{
					Outputs.WriteLine(OutputMessageType.Info, "Skipping launch mini-importer for instance entity of type {0} and class \"{1}\" because that class no longer exists in the project config.", instanceEntity.Type.ToString(), instanceEntity.ClassName);
					continue;
				}
				stringBuilder.AppendLine(civTechSvc.GetEntityPath(instanceEntity.Name, instanceEntity.Type));
				foreach (IInstanceDataFile df in instanceEntity.DataFiles)
				{
					IClassDataFile classDataFile = classEntity.DataFiles.FirstOrDefault((IClassDataFile dfc) => dfc.ID == df.ID);
					if (classDataFile == null)
					{
						Outputs.WriteLine(OutputMessageType.Info, "Stripping data file with ID \"{0}\" that no longer exists in the class entity \"{1}\" configuration.", df.ID, classEntity.Name);
					}
					else if (classDataFile.IsGenerated)
					{
						Outputs.WriteLine(OutputMessageType.Info, "Skipping launch mini-importer for generated data file with ID \"{0}\".", df.ID);
					}
					else
					{
						stringBuilder.AppendLine(instanceEntity.GetDataFilePath(df.RelativePath));
					}
				}
				stringBuilder.AppendLine();
			}
			Outputs.WriteLine(OutputMessageType.Info, stringBuilder.ToString());
		}
		return enumerable;
	}

	public static IEnumerable<IImportedEntity> LaunchSourceClassAssociationView(ICivTechService civTechSvc, IInstanceSet entitySet, IEnumerable<SourceFileModel> sources, IParameterSet parameterSet, IValueSet valueSet, InstanceType entityType)
	{
		Predicate<IImportedEntity> canExport = (IImportedEntity ent) => true;
		IEnumerable<IImportedEntity> enumerable = DialogHelper.LaunchSourceClassAssociationView(civTechSvc, sources, parameterSet, valueSet, entityType, global::DatabaseWrapper.DatabaseWrapper.ImportEntities, canExport, entitySet);
		if (enumerable.Any())
		{
			StringBuilder stringBuilder = new StringBuilder("The following files have been opened in Perforce:");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			foreach (IImportedEntity item in enumerable)
			{
				IClassEntity classEntity = civTechSvc.PrimaryProject.Config.Classes.FindForInstance(item);
				if (classEntity == null)
				{
					Outputs.WriteLine(OutputMessageType.Info, "Skipping launch source class associate view for instance entity of type {0} and class \"{1}\" because that class no longer exists in the project config.", item.Type.ToString(), item.ClassName);
					continue;
				}
				stringBuilder.AppendLine(civTechSvc.GetEntityPath(item.Name, item.Type));
				foreach (IInstanceDataFile df in item.DataFiles)
				{
					IClassDataFile classDataFile = classEntity.DataFiles.FirstOrDefault((IClassDataFile dfc) => dfc.ID == df.ID);
					if (classDataFile == null)
					{
						Outputs.WriteLine(OutputMessageType.Info, "Stripping data file with ID \"{0}\" that no longer exists in the class entity \"{1}\" configuration.", df.ID, classEntity.Name);
					}
					else if (classDataFile.IsGenerated)
					{
						Outputs.WriteLine(OutputMessageType.Info, "Skipping launch source class associate view for generated data file with ID \"{0}\".", df.ID);
					}
					else
					{
						stringBuilder.AppendLine(item.GetDataFilePath(df.RelativePath));
					}
				}
				stringBuilder.AppendLine();
			}
		}
		return enumerable;
	}
}

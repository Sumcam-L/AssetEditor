using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Firaxis.CivTech.TextureExport;
using Firaxis.ContentExporters;
using Firaxis.Error;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace DatabaseWrapper;

public class DatabaseWrapper
{
	private static string TEMP_CACHE_PATH;

	static DatabaseWrapper()
	{
		TEMP_CACHE_PATH = Path.Combine(Path.GetTempPath(), "CivTechImportCache");
	}

	public static string GetImportName(string sourceFilePath, string sourceObjectName)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
		if (string.IsNullOrEmpty(sourceObjectName) || sourceObjectName.Equals("loose_layers_only"))
		{
			return fileNameWithoutExtension;
		}
		string text = fileNameWithoutExtension + "_" + sourceObjectName;
		return text.Replace(':', '_');
	}

	public static ImportOperationResult ImportEntity(ICivTechService civTechSvc, string projectName, IImportedEntity entity)
	{
		IEnumerable<ImportOperationResult> source = ImportEntities(civTechSvc, projectName, new IImportedEntity[1] { entity });
		return source.Any() ? source.First() : new ImportOperationResult(entity);
	}

	public static ImportOperationResult FastImportNewEntity(ICivTechService civTechSvc, string projectName, string sourceObjectName, InstanceType newEntityType, string className, string sourceFilePath, IInstanceSet instances, out string finalEntityName)
	{
		string importName = GetImportName(sourceFilePath, sourceObjectName);
		IImportedEntity importedEntity = CreateInstanceEntity(newEntityType, importName, instances) as IImportedEntity;
		importedEntity.ClassName = className;
		importedEntity.SourceFilePath = sourceFilePath;
		importedEntity.SourceObjectName = sourceObjectName;
		if (newEntityType == InstanceType.IT_TEXTURE)
		{
			ITextureInstance textureInstance = importedEntity as ITextureInstance;
			AssignTextureClassDefaults(projectName, textureInstance);
		}
		int num = 1;
		string name = importedEntity.Name;
		while (!IsEntityNameAvailable(projectName, importedEntity))
		{
			importedEntity.Name = $"{name}_{num++:D2}";
		}
		finalEntityName = importedEntity.Name;
		return ImportEntity(civTechSvc, projectName, importedEntity);
	}

	public static ImportOperationResult FastImportNewEntity(ICivTechService civTechSvc, string projectName, string sourceObjectName, InstanceType newEntityType, string className, string sourceFilePath, IInstanceSet instances, string entityName)
	{
		IImportedEntity importedEntity = CreateInstanceEntity(newEntityType, entityName, instances) as IImportedEntity;
		importedEntity.ClassName = className;
		importedEntity.SourceFilePath = sourceFilePath;
		importedEntity.SourceObjectName = sourceObjectName;
		if (newEntityType == InstanceType.IT_TEXTURE)
		{
			ITextureInstance textureInstance = importedEntity as ITextureInstance;
			AssignTextureClassDefaults(projectName, textureInstance);
		}
		return ImportEntity(civTechSvc, projectName, importedEntity);
	}

	public static void AssignTextureClassDefaults(string projectName, ITextureInstance textureInstance)
	{
		ITextureExportOptions exportOptions = GetTextureClass(projectName, textureInstance.ClassName).ExportOptions;
		textureInstance.ExportSettings.AssignFromTextureClass(GetTextureClass(projectName, textureInstance.ClassName));
		textureInstance.ExportSettings.SupportScale = exportOptions.DefaultMipSupportScale;
		textureInstance.ExportSettings.FilterType = exportOptions.DefaultMipFilter;
		switch (exportOptions.ExportTextureType)
		{
		case TextureType.TEX_2D:
			textureInstance.ExportSettings.ExportMode = ExportMode.Texture2D;
			break;
		case TextureType.TEX_3D:
			textureInstance.ExportSettings.ExportMode = ExportMode.Texture3D;
			break;
		case TextureType.TEX_3D_COLORKEY:
			textureInstance.ExportSettings.ExportMode = ExportMode.ColorKey;
			break;
		case TextureType.TEX_CUBE:
			textureInstance.ExportSettings.ExportMode = ExportMode.CubeMap;
			break;
		default:
			textureInstance.ExportSettings.ExportMode = ExportMode.Texture2D;
			break;
		}
	}

	private static void ValidateEntityNames(IDictionary<IImportedEntity, ImportOperationResult> entities)
	{
		foreach (IImportedEntity validEntity in entities.GetValidEntities())
		{
			if (!StaticMethods.IsEntityNameValid(validEntity))
			{
				entities[validEntity].Result = new ResultCode("Entity name ({0}) is invalid", validEntity.Name);
			}
		}
	}

	private static void TrimEntitiesWithoutSource(IDictionary<IImportedEntity, ImportOperationResult> entities)
	{
		foreach (IImportedEntity validEntity in entities.GetValidEntities())
		{
			if (string.IsNullOrEmpty(validEntity.SourceFilePath))
			{
				entities[validEntity].Result = new ResultCode("Entity {0} does not have a valid source file.  Invalid source file path: {1}", validEntity.Name, validEntity.SourceFilePath);
			}
		}
	}

	private static IEnumerable<string> BuildEntityFileSyncList(IDictionary<IImportedEntity, ImportOperationResult> entities)
	{
		IList<string> list = new List<string>();
		foreach (KeyValuePair<IImportedEntity, ImportOperationResult> item in entities.Where((KeyValuePair<IImportedEntity, ImportOperationResult> ent) => ent.Value.Result))
		{
			ProjectEnvironment project = null;
			ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(item.Value.Entity, ref project);
			if (!entityProject)
			{
				item.Value.Result = entityProject;
				continue;
			}
			IVersionControlService versionControl = project.VersionControl;
			string xMLPath = item.Value.Entity.GetXMLPath();
			if (versionControl.IsVersionControlled(xMLPath) && !versionControl.IsCurrentVersion(xMLPath))
			{
				string localPath = versionControl.GetLocalPath(xMLPath);
				list.Add(localPath);
			}
		}
		return list;
	}

	private static IEnumerable<string> BuildSourceFileSyncList(IDictionary<IImportedEntity, ImportOperationResult> entities)
	{
		IList<string> list = new List<string>();
		foreach (KeyValuePair<IImportedEntity, ImportOperationResult> item in entities.Where((KeyValuePair<IImportedEntity, ImportOperationResult> ent) => ent.Value.Result))
		{
			IImportedEntity entity = item.Value.Entity;
			ProjectEnvironment project = null;
			ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(entity, ref project);
			if (!entityProject)
			{
				item.Value.Result = entityProject;
				continue;
			}
			IVersionControlService versionControl = project.VersionControl;
			if (versionControl.IsVersionControlled(entity.SourceFilePath) && !versionControl.IsCurrentVersion(entity.SourceFilePath))
			{
				string localPath = versionControl.GetLocalPath(entity.SourceFilePath);
				list.Add(localPath);
			}
		}
		return list;
	}

	private static void FilterOutOfDateEntities(IDictionary<IImportedEntity, ImportOperationResult> entities)
	{
		foreach (KeyValuePair<IImportedEntity, ImportOperationResult> item in entities.Where((KeyValuePair<IImportedEntity, ImportOperationResult> ent) => ent.Value.Result))
		{
			IImportedEntity entity = item.Value.Entity;
			ProjectEnvironment project = null;
			ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(entity, ref project);
			if (!entityProject)
			{
				item.Value.Result = entityProject;
				continue;
			}
			string xMLPath = entity.GetXMLPath();
			if (project.VersionControl.IsVersionControlled(xMLPath))
			{
				if (!project.VersionControl.IsCurrentVersion(xMLPath) && !project.VersionControl.IsAddingBackDeletedFile(xMLPath))
				{
					entities[entity].Result = new ResultCode("Entity ({0}) is out of date.  Sync your pantry before reimporting it!", entity.Name);
				}
				else if (project.VersionControl.IsMarkedForDelete(xMLPath))
				{
					entities[entity].Result = new ResultCode("Entity ({0}) is marked for delete.  revert the entity before reimporting it!", entity.Name);
				}
			}
		}
	}

	private static void BuildTypesAndExtensions(IEnumerable<IImportedEntity> entities, IList<Tuple<string, InstanceType>> entityTypeExtensionCollection, bool usePerforce = true)
	{
		foreach (IImportedEntity entity2 in entities)
		{
			ProjectEnvironment project = null;
			ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(entity2, ref project);
			if (!entityProject)
			{
				Outputs.WriteLine(OutputMessageType.Error, entityProject.Message);
				continue;
			}
			InstanceType entityType = entity2.Type;
			string extension = Path.GetExtension(entity2.SourceFilePath);
			Tuple<string, InstanceType> tuple = entityTypeExtensionCollection.FirstOrDefault((Tuple<string, InstanceType> pair) => pair.Item1 == extension && pair.Item2 == entityType);
			if (tuple == null)
			{
				entityTypeExtensionCollection.Add(Tuple.Create(extension, entityType));
			}
			EntityID entity = new EntityID(entity2);
			CreateInstanceEntitySubdirectories(project.Name, entity);
		}
	}

	private static void PerformPreExportValidation(IDictionary<IImportedEntity, ImportOperationResult> entities, IContentExporter exporter, bool usePerforce = true)
	{
		IEnumerable<IImportedEntity> enumerable = entities.GetValidEntities().Where(delegate(IImportedEntity ent)
		{
			string extension = Path.GetExtension(ent.SourceFilePath);
			return exporter.SupportedFileTypes.Contains(extension, StringComparer.InvariantCultureIgnoreCase) && exporter.SupportedInstanceTypes.Contains(ent.Type);
		});
		foreach (IImportedEntity item in enumerable)
		{
			ProjectEnvironment project = null;
			ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(item, ref project);
			if (!entityProject)
			{
				entities[item].Result = entityProject;
				continue;
			}
			IClassEntity entityClass = GetClass(project.Name, item);
			ResultCode resultCode = exporter.ValidateClass(item, entityClass);
			if ((bool)resultCode)
			{
				if (usePerforce)
				{
					string errMsg = string.Empty;
					if (!OpenIfRequired(item, out errMsg))
					{
						entities[item].Result = new ResultCode("Entity {0} could not be opened for edit.\n{1}", item.Name, errMsg);
					}
				}
			}
			else
			{
				entities[item].Result = resultCode;
			}
		}
	}

	private static void CacheEntityFiles(IDictionary<IImportedEntity, ImportOperationResult> entities, IContentExporter exporter)
	{
		if (!Directory.Exists(TEMP_CACHE_PATH))
		{
			Directory.CreateDirectory(TEMP_CACHE_PATH);
		}
		IEnumerable<IImportedEntity> enumerable = entities.GetValidEntities().Where(delegate(IImportedEntity ent)
		{
			string extension = Path.GetExtension(ent.SourceFilePath);
			return exporter.SupportedFileTypes.Contains(extension, StringComparer.InvariantCultureIgnoreCase) && exporter.SupportedInstanceTypes.Contains(ent.Type);
		});
		foreach (IImportedEntity item in enumerable)
		{
			try
			{
				string xMLPath = item.GetXMLPath();
				string text = Path.Combine(TEMP_CACHE_PATH, item.Name + item.XMLExtension);
				if (!TryCopyFile(xMLPath, text))
				{
					Context.EnsureCreated<CivTechContext>().CivTechLogger.AddLogItem(LogEventType.Error, "Exporter", $"Couldn't cache the entity \"{item.Name}\", failed to copy entity file from \"{xMLPath}\" to \"{text}\"!");
				}
				foreach (IInstanceDataFile dataFile in item.DataFiles)
				{
					string text2 = Path.Combine(Path.GetDirectoryName(item.GetXMLPath()), dataFile.RelativePath);
					string text3 = Path.Combine(TEMP_CACHE_PATH, dataFile.RelativePath);
					if (!TryCopyFile(text2, text3))
					{
						Context.EnsureCreated<CivTechContext>().CivTechLogger.AddLogItem(LogEventType.Error, "Exporter", $"Couldn't cache the entity \"{item.Name}\", failed to copy data file from \"{text2}\" to \"{text3}\"!");
					}
				}
			}
			catch (Exception ex)
			{
				Context.EnsureCreated<CivTechContext>().CivTechLogger.AddLogItem(LogEventType.Error, "Exporter", $"Couldn't cache the entity: {item.Name}. " + ex.Message);
			}
		}
	}

	private static void ClearEntityCache()
	{
		if (Directory.Exists(TEMP_CACHE_PATH))
		{
			Directory.EnumerateFiles(TEMP_CACHE_PATH).ForEach(delegate(string filePath)
			{
				TryDeleteFile(filePath);
			});
		}
	}

	private static bool RecoverCachedEntity(IImportedEntity entity)
	{
		try
		{
			string text = Path.Combine(TEMP_CACHE_PATH, entity.Name + entity.XMLExtension);
			string xMLPath = entity.GetXMLPath();
			if (!TryMoveFile(text, xMLPath))
			{
				Context.EnsureCreated<CivTechContext>().CivTechLogger.AddLogItem(LogEventType.Error, "Exporter", $"Couldn't recover the entity \"{entity.Name}\" from the cache, failed to move entity file from \"{text}\" to \"{xMLPath}\"!");
				return false;
			}
			foreach (IInstanceDataFile dataFile in entity.DataFiles)
			{
				string text2 = Path.Combine(TEMP_CACHE_PATH, dataFile.RelativePath);
				string text3 = Path.Combine(Path.GetDirectoryName(xMLPath), dataFile.RelativePath);
				if (!TryMoveFile(text2, text3))
				{
					Context.EnsureCreated<CivTechContext>().CivTechLogger.AddLogItem(LogEventType.Error, "Exporter", $"Couldn't recover the entity \"{entity.Name}\" from the cache, failed to move data file from \"{text2}\" to \"{text3}\"!");
					return false;
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Context.EnsureCreated<CivTechContext>().CivTechLogger.AddLogItem(LogEventType.Error, "Exporter", $"Couldn't recover the entity: {entity.Name} from the cache. " + ex.Message);
			return false;
		}
	}

	private static bool TryDeleteFile(string filePath)
	{
		if (File.Exists(filePath))
		{
			if (!TryFileOperation(delegate
			{
				File.SetAttributes(filePath, FileAttributes.Normal);
			}))
			{
				return false;
			}
			if (!TryFileOperation(delegate
			{
				File.Delete(filePath);
			}))
			{
				return false;
			}
		}
		return true;
	}

	private static bool TryMoveFile(string srcFilePath, string destFilePath)
	{
		if (File.Exists(destFilePath) && !TryFileOperation(delegate
		{
			File.SetAttributes(destFilePath, FileAttributes.Normal);
		}))
		{
			return false;
		}
		if (!File.Exists(srcFilePath))
		{
			return false;
		}
		if (!TryFileOperation(delegate
		{
			File.SetAttributes(srcFilePath, FileAttributes.Normal);
		}))
		{
			return false;
		}
		if (!TryFileOperation(delegate
		{
			File.Copy(srcFilePath, destFilePath, overwrite: true);
		}))
		{
			return false;
		}
		if (!TryFileOperation(delegate
		{
			File.Delete(srcFilePath);
		}))
		{
			return false;
		}
		return true;
	}

	private static bool TryCopyFile(string srcFilePath, string destFilePath)
	{
		if (File.Exists(destFilePath) && !TryFileOperation(delegate
		{
			File.SetAttributes(destFilePath, FileAttributes.Normal);
		}))
		{
			return false;
		}
		if (!File.Exists(srcFilePath))
		{
			return false;
		}
		if (!TryFileOperation(delegate
		{
			File.Copy(srcFilePath, destFilePath, overwrite: true);
		}))
		{
			return false;
		}
		return true;
	}

	private static bool TryFileOperation(Action fileOperation)
	{
		int num = 5;
		int num2 = 32;
		do
		{
			try
			{
				fileOperation();
				return true;
			}
			catch (IOException ex)
			{
				if (Marshal.GetHRForException(ex) != -2147024864)
				{
					BugSubmitter.SilentException(ex);
					return false;
				}
			}
			Thread.Sleep(num2);
			num2 <<= 1;
		}
		while (num-- >= 0);
		return false;
	}

	private static void PerformExport(ICivTechService civTechSvc, IDictionary<IImportedEntity, ImportOperationResult> entities, IContentExporter exporter)
	{
		Stack<Tuple<IImportedEntity, string>> stack = new Stack<Tuple<IImportedEntity, string>>(entities.Count);
		IList<Tuple<ImportOperationResult, IClassEntity>> list = new List<Tuple<ImportOperationResult, IClassEntity>>();
		IEnumerable<IImportedEntity> enumerable = entities.GetValidEntities().Where(delegate(IImportedEntity ent)
		{
			string extension = Path.GetExtension(ent.SourceFilePath);
			return exporter.SupportedFileTypes.Contains(extension, StringComparer.InvariantCultureIgnoreCase) && exporter.SupportedInstanceTypes.Contains(ent.Type);
		});
		foreach (IImportedEntity item2 in enumerable)
		{
			stack.Push(Tuple.Create(item2, item2.SourceFilePath));
			ProjectEnvironment project = null;
			ResultCode entityProject = civTechSvc.GetEntityProject(item2, ref project);
			if ((bool)entityProject)
			{
				item2.SourceFilePath = project.VersionControl.GetLocalPath(item2.SourceFilePath);
				IClassEntity item = GetClass(project.Name, item2);
				list.Add(Tuple.Create(entities[item2], item));
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Error, entityProject.Message);
			}
		}
		exporter.Export(civTechSvc, list);
		while (stack.Count > 0)
		{
			Tuple<IImportedEntity, string> tuple = stack.Pop();
			tuple.Item1.SourceFilePath = tuple.Item2;
		}
	}

	private static bool DataFileExists(string baseDataFilePath, IInstanceDataFile dataFile, ref string fullDataFilePath)
	{
		fullDataFilePath = Path.Combine(baseDataFilePath, dataFile.RelativePath);
		return File.Exists(fullDataFilePath);
	}

	private static void RecoverFailedExports(IDictionary<IImportedEntity, ImportOperationResult> entities)
	{
		string empty = string.Empty;
		List<IImportedEntity> list = new List<IImportedEntity>();
		foreach (KeyValuePair<IImportedEntity, ImportOperationResult> entity in entities)
		{
			if (!entity.Value.Result)
			{
				IImportedEntity key = entity.Key;
				if (!RecoverCachedEntity(key))
				{
					string msg = entities[key].Result.Message + "\n" + $"The entity ({key.Name}) could not be reverted to the cached version.";
					entities[key].Result = new ResultCode(msg);
				}
			}
		}
	}

	private static void EnsureEntitiesExported(IDictionary<IImportedEntity, ImportOperationResult> entities, DateTime sentinelTime, IContentExporter exporter, bool usePerforce = true)
	{
		string empty = string.Empty;
		IEnumerable<IImportedEntity> enumerable = entities.GetValidEntities().Where(delegate(IImportedEntity ent)
		{
			string extension = Path.GetExtension(ent.SourceFilePath);
			return exporter.SupportedFileTypes.Contains(extension, StringComparer.InvariantCultureIgnoreCase) && exporter.SupportedInstanceTypes.Contains(ent.Type);
		});
		foreach (IImportedEntity item in enumerable)
		{
			string xMLPath = item.GetXMLPath();
			string directoryName = Path.GetDirectoryName(xMLPath);
			ICollection<string> collection = new List<string>();
			if (item.DataFiles.Any())
			{
				ProjectEnvironment project = null;
				ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(item, ref project);
				if (!entityProject)
				{
					entities[item].Result = entityProject;
					continue;
				}
				IVersionControlService versionControl = project.VersionControl;
				foreach (IInstanceDataFile dataFile in item.DataFiles)
				{
					string fullDataFilePath = string.Empty;
					if (DataFileExists(directoryName, dataFile, ref fullDataFilePath))
					{
						FileInfo fileInfo = new FileInfo(fullDataFilePath);
						if (!(fileInfo.LastWriteTimeUtc < sentinelTime.ToUniversalTime()))
						{
							continue;
						}
						IImportedEntity entity = entities[item].Entity;
						if (collection.Count == 0)
						{
							string localPath = versionControl.GetLocalPath(item.SourceFilePath);
							collection = exporter.GetSourceObjectNames(localPath, entity.Type).ToList();
						}
						if (!string.IsNullOrEmpty(entity.SourceObjectName))
						{
							if (collection.Count == 0)
							{
								string msg = $"I couldn't get the source object list from {entity.SourceFilePath}.  Ensure that all of the latest plug-ins are installed.";
								entities[item].Result = new ResultCode(msg);
							}
							else if (!collection.Contains(entity.SourceObjectName))
							{
								string msg2 = $"\nThe Source Object '{entity.SourceObjectName}' doesn't exist in the source file: {entity.SourceFilePath}\n\nPlease make sure the object exists in the source file and try again";
								entities[item].Result = new ResultCode(msg2);
							}
							else
							{
								string msg3 = $"The data file ({fullDataFilePath}) failed to export.  Is the file checked out in Perforce?";
								entities[item].Result = new ResultCode(msg3);
							}
						}
					}
					else
					{
						string msg4 = $"The data file located at ({fullDataFilePath}) does not exist on disk.  This means that the export-step failed.";
						entities[item].Result = new ResultCode(msg4);
					}
				}
			}
			else
			{
				entities[item].Result = new ResultCode("The entity ({0}) does not have any data files assigned to it as a result of the export-step failing.", item);
			}
			if (!entities[item].Result && !RecoverCachedEntity(item))
			{
				string msg5 = entities[item].Result.Message + "\n" + $"The entity ({item.Name}) could not be reverted to the cached version.";
				entities[item].Result = new ResultCode(msg5);
			}
		}
	}

	private static void ValidateExportedEntities(IDictionary<IImportedEntity, ImportOperationResult> entities, IContentExporter exporter, bool usePerforce = true)
	{
		string empty = string.Empty;
		IEnumerable<IImportedEntity> enumerable = entities.GetValidEntities().Where(delegate(IImportedEntity ent)
		{
			string extension = Path.GetExtension(ent.SourceFilePath);
			return exporter.SupportedFileTypes.Contains(extension, StringComparer.InvariantCultureIgnoreCase) && exporter.SupportedInstanceTypes.Contains(ent.Type);
		});
		foreach (IImportedEntity item in enumerable)
		{
			ProjectEnvironment project = null;
			ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(item, ref project);
			if (!entityProject)
			{
				entities[item].Result = entityProject;
				continue;
			}
			IClassEntity entityClass = GetClass(project.Name, item);
			ResultCode resultCode = exporter.Validate(item, entityClass, project.Paths.GamePantry);
			if ((bool)resultCode)
			{
				continue;
			}
			entities[item].Result = resultCode;
			if (usePerforce && DoesEntityExist(item))
			{
				if (!RecoverCachedEntity(item))
				{
					string msg = entities[item].Result.Message + "\n" + $"The entity ({item.Name}) could not be reverted to the cached version.";
					entities[item].Result = new ResultCode(msg);
				}
				else
				{
					exporter.RebuildExportedEntity(CivTechRegistry.CivTechService, item, entityClass);
				}
			}
		}
	}

	public static IEnumerable<ImportOperationResult> ImportEntities(ICivTechService civTechSvc, string projectName, IEnumerable<IImportedEntity> entities)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		IDictionary<string, IEnumerable<string>> assetParents = GetAssetParents(entities.OfType<IGeometryInstance>());
		ImportOperationResultCollection importOperationResultCollection = PerformExport(civTechSvc, entities, usePerforce: true);
		UploadEntities(civTechSvc, importOperationResultCollection);
		stopwatch.Stop();
		FixupAssetModelInstances(civTechSvc, importOperationResultCollection.GetValidEntities().OfType<IGeometryInstance>(), assetParents, Enumerable.Empty<string>());
		Context.EnsureCreated<CivTechContext>().CivTechLogger.AddLogItem(LogEventType.Info, "Importer", $"Importing {entities.Count()} entities took {(double)stopwatch.ElapsedMilliseconds / 1000.0:0.00} seconds");
		return importOperationResultCollection;
	}

	public static IEnumerable<ImportOperationResult> ImportEntities(ICivTechService civTechSvc, IEnumerable<IImportedEntity> entities, IEnumerable<string> ignoreAssetNames)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		IDictionary<string, IEnumerable<string>> assetParents = GetAssetParents(entities.OfType<IGeometryInstance>());
		ImportOperationResultCollection importOperationResultCollection = PerformExport(civTechSvc, entities, usePerforce: true);
		UploadEntities(civTechSvc, importOperationResultCollection);
		stopwatch.Stop();
		FixupAssetModelInstances(civTechSvc, importOperationResultCollection.GetValidEntities().OfType<IGeometryInstance>(), assetParents, ignoreAssetNames);
		Context.EnsureCreated<CivTechContext>().CivTechLogger.AddLogItem(LogEventType.Info, "Importer", $"Importing {entities.Count()} entities took {(double)stopwatch.ElapsedMilliseconds / 1000.0:0.00} seconds");
		return importOperationResultCollection;
	}

	private static IDictionary<string, IEnumerable<string>> GetAssetParents(IEnumerable<IGeometryInstance> geometries)
	{
		IDictionary<string, IEnumerable<string>> dictionary = new Dictionary<string, IEnumerable<string>>();
		foreach (IGeometryInstance geometry in geometries)
		{
			IQueryService dependents = CivTechRegistry.EntityCacheService.GetDependents(geometry.Name, geometry.Type);
			dictionary[geometry.Name] = dependents.InstanceItems[InstanceType.IT_ASSET].ToArray();
		}
		return dictionary;
	}

	public static IEnumerable<ImportOperationResult> RenderEntityFromSource(IEnumerable<IImportedEntity> entities, string outputFolder)
	{
		ImportOperationResultCollection importOperationResultCollection = new ImportOperationResultCollection(entities);
		ValidateEntityNames(importOperationResultCollection);
		FilterOutOfDateEntities(importOperationResultCollection);
		TrimEntitiesWithoutSource(importOperationResultCollection);
		List<Tuple<string, InstanceType>> entityTypeExtensionCollection = new List<Tuple<string, InstanceType>>();
		BuildTypesAndExtensions(importOperationResultCollection.GetValidEntities(), entityTypeExtensionCollection);
		foreach (IImportedEntity entity in entities)
		{
			ProjectEnvironment project = null;
			ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(entity, ref project);
			if (!entityProject)
			{
				importOperationResultCollection[entity].Result = entityProject;
				continue;
			}
			IContentCreationTool contentCreationTool = ExporterService.GetContentCreationTool(entity);
			contentCreationTool.RenderSourceObjectFromFile(project.VersionControl.GetLocalPath(entity.SourceFilePath), entity.SourceObjectName, outputFolder);
		}
		return importOperationResultCollection;
	}

	private static void FixupAssetModelInstances(ICivTechService civTechSvc, IEnumerable<IGeometryInstance> geometries, IDictionary<string, IEnumerable<string>> geometryAssetParents, IEnumerable<string> ignoreAssetNames)
	{
		if (!geometries.Any() || geometryAssetParents.Count == 0)
		{
			return;
		}
		ICollection<string> collection = new SortedSet<string>();
		IDictionary<string, IInstanceSet> dictionary = new Dictionary<string, IInstanceSet>();
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>();
		foreach (IGeometryInstance geometry in geometries)
		{
			ProjectEnvironment project = null;
			ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(geometry, ref project);
			if (!entityProject)
			{
				Outputs.WriteLine(OutputMessageType.Error, entityProject.Message);
				continue;
			}
			IInstanceSet value = null;
			if (!dictionary.TryGetValue(project.Name, out value))
			{
				IInstanceSet instanceSet = (dictionary[project.Name] = civTechContext.CreateInstance<IInstanceSet>(new object[1] { civTechSvc.ProjectMapService.GetProjectPantryPaths(project) }));
				value = instanceSet;
			}
			if (!geometryAssetParents.TryGetValue(geometry.Name, out var value2))
			{
				continue;
			}
			foreach (string item in value2)
			{
				if (!ignoreAssetNames.Contains(item))
				{
					IAssetInstance assetInstance = value.LoadByName<IAssetInstance>(item);
					if (assetInstance != null)
					{
						collection.Add(item);
						StaticMethods.ReconcileAssetGeometrySet(assetInstance, geometry, civTechContext, civTechSvc);
					}
				}
			}
		}
		if (ShouldReconcileAssets(collection))
		{
			string errMsg = string.Empty;
			UploadEntities(civTechSvc, dictionary.SelectMany((KeyValuePair<string, IInstanceSet> kvp) => kvp.Value.Items), out errMsg);
			if (HasError(errMsg))
			{
				MessageBoxes.Show(errMsg, "Errors Occurred!", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		dictionary.ForEach(delegate(KeyValuePair<string, IInstanceSet> pi)
		{
			pi.Value.Clear();
			pi.Value.Dispose();
		});
		dictionary.Clear();
	}

	private static bool ShouldReconcileAssets(IEnumerable<string> assetNames)
	{
		if (!assetNames.Any())
		{
			return false;
		}
		string message = string.Format("The following assets have been affected by the geometry import.  Would you like me to try and update them for you automatically?  NOTE:  Don't do this if the assets are open in Asset Editor.\n\n{0}", "\t" + string.Join("\n\t", assetNames));
		return MessageBoxes.Show(message, "Update affected assets?", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes;
	}

	private static bool HasError(string errorString)
	{
		return !string.IsNullOrEmpty(errorString);
	}

	public static ImportOperationResult ExportEntity(ICivTechService civTechSvc, string projectName, IImportedEntity entity)
	{
		IEnumerable<ImportOperationResult> source = ExportEntities(civTechSvc, projectName, new IImportedEntity[1] { entity });
		return source.Any() ? source.First() : new ImportOperationResult(entity);
	}

	public static IEnumerable<ImportOperationResult> ExportEntities(ICivTechService civTechSvc, string projectName, IEnumerable<IImportedEntity> entities)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		ImportOperationResultCollection result = PerformExport(civTechSvc, entities, usePerforce: true);
		stopwatch.Stop();
		Context.EnsureCreated<CivTechContext>().CivTechLogger.AddLogItem(LogEventType.Info, "Exporter", $"Exporting {entities.Count()} entities took {(double)stopwatch.ElapsedMilliseconds / 1000.0:0.00} seconds");
		return result;
	}

	private static ImportOperationResultCollection PerformExport(ICivTechService civTechSvc, IEnumerable<IImportedEntity> entities, bool usePerforce)
	{
		DateTime now = DateTime.Now;
		ImportOperationResultCollection importOperationResultCollection = new ImportOperationResultCollection(entities);
		ValidateEntityNames(importOperationResultCollection);
		FilterOutOfDateEntities(importOperationResultCollection);
		TrimEntitiesWithoutSource(importOperationResultCollection);
		SyncOutOfDateEntityFiles(importOperationResultCollection);
		SyncOutOfDateSourceFiles(importOperationResultCollection);
		TrimEntitiesWithOutOfDateSources(importOperationResultCollection);
		List<Tuple<string, InstanceType>> list = new List<Tuple<string, InstanceType>>();
		BuildTypesAndExtensions(importOperationResultCollection.GetValidEntities(), list);
		foreach (Tuple<string, InstanceType> item in list)
		{
			IContentExporter exporter = ExporterService.GetExporter(item.Item1, item.Item2);
			BugSubmitter.SilentAssert(exporter != null, "No exporter could be found for the file type {0}.  Entities of this file type were not exported/imported. @summary No exporter could be found for type @assign bwhitman", item.Item1);
			PerformPreExportValidation(importOperationResultCollection, exporter, usePerforce);
			ClearEntityCache();
			CacheEntityFiles(importOperationResultCollection, exporter);
			PerformExport(civTechSvc, importOperationResultCollection, exporter);
			RecoverFailedExports(importOperationResultCollection);
			EnsureEntitiesExported(importOperationResultCollection, now, exporter, usePerforce);
			ValidateExportedEntities(importOperationResultCollection, exporter, usePerforce);
			ClearEntityCache();
		}
		return importOperationResultCollection;
	}

	private static void SyncOutOfDateEntityFiles(IDictionary<IImportedEntity, ImportOperationResult> entities)
	{
		IEnumerable<string> enumerable = BuildEntityFileSyncList(entities);
		if (enumerable.Any())
		{
			string message = string.Format("The following pantry files are out of date.  Sync them to continue with the export?\n\n{0}", string.Join("\t\n", enumerable));
			MessageBoxResult messageBoxResult = MessageBoxes.Show(message, "Sync Pantry Files", MessageBoxButton.YesNo, MessageBoxImage.Information);
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				IList<string> errMsg = new List<string>();
				CivTechRegistry.CivTechService.PrimaryProject.VersionControl.GetLatest(enumerable, errMsg);
			}
		}
	}

	private static void SyncOutOfDateSourceFiles(IDictionary<IImportedEntity, ImportOperationResult> entities)
	{
		IEnumerable<string> enumerable = BuildSourceFileSyncList(entities);
		if (enumerable.Any())
		{
			string message = string.Format("The following source files are out of date.  Sync them to continue with the export?\n\n{0}", string.Join("\t\n", enumerable));
			MessageBoxResult messageBoxResult = MessageBoxes.Show(message, "Sync Source Files", MessageBoxButton.YesNo, MessageBoxImage.Information);
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				IList<string> errMsg = new List<string>();
				CivTechRegistry.CivTechService.PrimaryProject.VersionControl.GetLatest(enumerable, errMsg);
			}
		}
	}

	private static void TrimEntitiesWithOutOfDateSources(IDictionary<IImportedEntity, ImportOperationResult> entities)
	{
		foreach (IImportedEntity validEntity in entities.GetValidEntities())
		{
			ProjectEnvironment project = null;
			ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(validEntity, ref project);
			if (!entityProject)
			{
				entities[validEntity].Result = entityProject;
			}
			else if (project.VersionControl.IsVersionControlled(validEntity.SourceFilePath))
			{
				if (!project.VersionControl.IsCurrentVersion(validEntity.SourceFilePath) && !project.VersionControl.IsAddingBackDeletedFile(validEntity.SourceFilePath))
				{
					entities[validEntity].Result = new ResultCode("Entity ({0})'s source file ({1}) is out of date.  Sync the source file before reimporting it!", validEntity.Name, validEntity.SourceFilePath);
				}
				else if (project.VersionControl.IsMarkedForDelete(validEntity.SourceFilePath))
				{
					entities[validEntity].Result = new ResultCode("Entity ({0})'s source file ({1}) is marked for delete.  Revert the source file before reimporting it!", validEntity.Name, validEntity.SourceFilePath);
				}
			}
		}
	}

	public static void CreateInstanceEntitySubdirectories(string projectName, EntityID entity)
	{
		CreateInstanceEntitySubdirectories(projectName, GetInstanceDirectory(projectName, entity.Type), entity.Name);
	}

	public static void CreateInstanceEntitySubdirectories(string projectName, string exportDir, IInstanceEntity entity)
	{
		CreateInstanceEntitySubdirectories(projectName, exportDir, entity.Name);
	}

	public static void CreateInstanceEntitySubdirectories(string projectName, string exportDir, string entityName)
	{
		IEnumerable<string> entitySubdirectories = StaticMethods.GetEntitySubdirectories(entityName);
		string text = exportDir;
		foreach (string item in entitySubdirectories)
		{
			text = Path.Combine(text, item);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
		}
	}

	private static IInstanceEntity CreateInstanceEntity(InstanceType type, string name, IInstanceSet instances)
	{
		return instances.CreateEntityByName(name, type);
	}

	public static bool DeserializeInstanceEntity(IInstanceEntity entity)
	{
		string dataFile = GetDataFile(entity);
		if (string.IsNullOrEmpty(dataFile))
		{
			return false;
		}
		string xmlText = string.Empty;
		try
		{
			if (!File.Exists(dataFile))
			{
				return false;
			}
			using StreamReader streamReader = new StreamReader(dataFile);
			xmlText = streamReader.ReadToEnd();
			streamReader.Close();
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (IOException)
		{
			return false;
		}
		return entity.DeserializeFromXML(xmlText);
	}

	public static bool DoesEntityExist(IInstanceEntity entity)
	{
		string xMLPath = entity.GetXMLPath();
		if (string.IsNullOrEmpty(xMLPath))
		{
			return false;
		}
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(entity, ref project);
		if (!entityProject)
		{
			return false;
		}
		return project.VersionControl.IsVersionControlled(xMLPath);
	}

	public static bool DoesEntityExist(InstanceType eType, string name)
	{
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(name, eType, ref project);
		if (!entityProject)
		{
			return false;
		}
		string entityPath = string.Empty;
		ResultCode entityPath2 = CivTechRegistry.CivTechService.GetEntityPath(name, eType, out entityPath);
		if (!entityPath2)
		{
			return false;
		}
		return project.VersionControl.IsVersionControlled(entityPath);
	}

	public static IEnumerable<string> GetAllowedClasses(IEntityContainerClass containerClass, IInstanceEntity entity)
	{
		return GetAllowedClasses(containerClass, entity.Type);
	}

	public static IEnumerable<string> GetAllowedClasses(IEntityContainerClass containerClass, InstanceType entityType)
	{
		List<string> list = new List<string>();
		if (containerClass == null)
		{
			return list;
		}
		list.AddRange(containerClass.GetAllowedClasses(entityType));
		return list;
	}

	public static IAnalyticLightClass GetAnalyticLightClass(string projectName, string className)
	{
		return GetClasses<IAnalyticLightClass>(projectName).FirstOrDefault((IAnalyticLightClass cls) => cls.Name == className);
	}

	public static IEnumerable<IAnalyticLightClass> GetAnalyticLightClasses(string projectName)
	{
		return GetClasses<IAnalyticLightClass>(projectName);
	}

	public static IAnimationClass GetAnimationClass(string projectName, string className)
	{
		return GetClasses<IAnimationClass>(projectName).FirstOrDefault((IAnimationClass cls) => cls.Name == className);
	}

	public static IEnumerable<IAnimationClass> GetAnimationClasses(string projectName)
	{
		return GetClasses<IAnimationClass>(projectName);
	}

	public static IAssetClass GetAssetClass(string projectName, string className)
	{
		return GetClasses<IAssetClass>(projectName).FirstOrDefault((IAssetClass cls) => cls.Name == className);
	}

	public static IEnumerable<IAssetClass> GetAssetClasses(string projectName)
	{
		return GetClasses<IAssetClass>(projectName);
	}

	public static IBehaviorClass GetBehaviorClass(string projectName, string className)
	{
		return GetClasses<IBehaviorClass>(projectName).FirstOrDefault((IBehaviorClass cls) => cls.Name == className);
	}

	public static IEnumerable<IBehaviorClass> GetBehaviorClasses(string projectName)
	{
		return GetClasses<IBehaviorClass>(projectName);
	}

	public static IClassEntity GetClass(string projectName, InstanceType type, string className)
	{
		if (!CivTechRegistry.CivTechService.AllProjectsMap.ContainsProject(projectName))
		{
			return null;
		}
		IEnumerable<IClassEntity> items = CivTechRegistry.CivTechService.AllProjectsMap[projectName].Config.Classes.Items;
		IEnumerable<IClassEntity> source = items.Where((IClassEntity wc) => wc.InstanceTypeEnum == type && wc.Name == className);
		return source.FirstOrDefault();
	}

	public static IClassEntity GetClass(string projectName, IInstanceEntity entity)
	{
		return GetClass(projectName, entity.Type, entity.ClassName);
	}

	public static IEnumerable<IClassEntity> GetClasses(string projectName, InstanceType entityType)
	{
		if (!CivTechRegistry.CivTechService.AllProjectsMap.ContainsProject(projectName))
		{
			return Enumerable.Empty<IClassEntity>();
		}
		IEnumerable<IClassEntity> items = CivTechRegistry.CivTechService.AllProjectsMap[projectName].Config.Classes.Items;
		IEnumerable<IClassEntity> source = items.Where((IClassEntity wc) => wc.InstanceTypeEnum == entityType);
		return source.ToArray();
	}

	public static IEnumerable<T> GetClasses<T>(string projectName) where T : IClassEntity
	{
		if (!CivTechRegistry.CivTechService.AllProjectsMap.ContainsProject(projectName))
		{
			return Enumerable.Empty<T>();
		}
		return CivTechRegistry.CivTechService.AllProjectsMap[projectName].Config.Classes.Items.OfType<T>().ToArray();
	}

	public static IEnumerable<string> GetClassNames<T>(string projectName) where T : IClassEntity
	{
		if (!CivTechRegistry.CivTechService.AllProjectsMap.ContainsProject(projectName))
		{
			return Enumerable.Empty<string>();
		}
		return (from cls in CivTechRegistry.CivTechService.AllProjectsMap[projectName].Config.Classes.Items.OfType<T>()
			select cls.Name).ToArray();
	}

	public static IEnumerable<string> GetClassNames(string projectName)
	{
		if (!CivTechRegistry.CivTechService.AllProjectsMap.ContainsProject(projectName))
		{
			return Enumerable.Empty<string>();
		}
		return CivTechRegistry.CivTechService.AllProjectsMap[projectName].Config.Classes.Items.Select((IClassEntity cls) => cls.Name).ToArray();
	}

	public static IEnumerable<string> GetClassNames(string projectName, InstanceType entityType)
	{
		if (!CivTechRegistry.CivTechService.AllProjectsMap.ContainsProject(projectName))
		{
			return Enumerable.Empty<string>();
		}
		return (from sc in CivTechRegistry.CivTechService.AllProjectsMap[projectName].Config.Classes.Items
			where sc.InstanceTypeEnum == entityType
			select sc.Name).ToArray();
	}

	public static string GetDataFile(IInstanceEntity entity)
	{
		return GetDataFile(entity.Type, entity.Name);
	}

	public static string GetDataFile(InstanceType type, string name)
	{
		return CivTechRegistry.CivTechService.GetEntityPath(name, type);
	}

	public static string GetDefaultClassName(string projectName, IEntityContainerEntity parentEntity, IInstanceEntity entity)
	{
		IEntityContainerClass containerClass = GetClass(projectName, parentEntity) as IEntityContainerClass;
		return GetDefaultClassName(projectName, containerClass, entity);
	}

	public static string GetDefaultClassName(string projectName, IEntityContainerClass containerClass, IInstanceEntity entity)
	{
		string result = string.Empty;
		if (containerClass == null)
		{
			return result;
		}
		if (entity.Type == InstanceType.IT_MATERIAL)
		{
			IEnumerable<IMaterialClass> materialClasses = GetMaterialClasses(projectName);
			foreach (IMaterialClass item in materialClasses)
			{
				if (item.Name == containerClass.Name)
				{
					result = item.Name;
				}
			}
		}
		else
		{
			IEnumerable<string> allowedClasses = containerClass.GetAllowedClasses(entity.Type);
			if (allowedClasses.Count() == 1)
			{
				result = allowedClasses.First();
			}
		}
		return result;
	}

	public static IDSGClass GetDSGClass(string projectName, string className)
	{
		return GetClasses<IDSGClass>(projectName).FirstOrDefault((IDSGClass cls) => cls.Name == className);
	}

	public static IEnumerable<IDSGClass> GetDSGClasses(string projectName)
	{
		return GetClasses<IDSGClass>(projectName);
	}

	public static IEnumerable<IInstanceEntity> GetEntityDependencies(IInstanceEntity entity, IInstanceSet instances)
	{
		List<IInstanceEntity> list = new List<IInstanceEntity>();
		if (entity == null || instances == null)
		{
			return list;
		}
		foreach (IValue item in entity.CookParameters.Items)
		{
			if (item is IObjectValue objectValue && !string.IsNullOrEmpty(objectValue.GetBoundObjectName()))
			{
				IInstanceEntity instanceEntity = instances.LoadEntityByName(objectValue.GetBoundObjectName(), objectValue.GetBoundObjectType());
				if (instanceEntity != null)
				{
					list.Add(instanceEntity);
					list.AddRange(GetEntityDependencies(instanceEntity, instances));
				}
			}
		}
		if (entity.Type == InstanceType.IT_ASSET)
		{
			IAssetInstance assetInstance = entity as IAssetInstance;
			List<IInstanceEntity> list2 = new List<IInstanceEntity>();
			foreach (string geometry in assetInstance.GetGeometries())
			{
				IInstanceEntity instanceEntity2 = instances.LoadEntityByName(geometry, InstanceType.IT_GEOMETRY);
				if (instanceEntity2 != null)
				{
					list2.Add(instanceEntity2);
				}
			}
			foreach (string animation in assetInstance.GetAnimations())
			{
				IInstanceEntity instanceEntity3 = instances.LoadEntityByName(animation, InstanceType.IT_ANIMATION);
				if (instanceEntity3 != null)
				{
					list2.Add(instanceEntity3);
				}
			}
			foreach (string material in assetInstance.GetMaterials())
			{
				IInstanceEntity instanceEntity4 = instances.LoadEntityByName(material, InstanceType.IT_MATERIAL);
				if (instanceEntity4 != null)
				{
					list2.Add(instanceEntity4);
				}
			}
			list.AddRange(list2);
			foreach (IInstanceEntity item2 in list2)
			{
				list.AddRange(GetEntityDependencies(item2, instances));
			}
		}
		return list;
	}

	public static IEnvironmentLightClass GetEnvironmentLightClass(string projectName, string className)
	{
		return GetClasses<IEnvironmentLightClass>(projectName).FirstOrDefault((IEnvironmentLightClass cls) => cls.Name == className);
	}

	public static IEnumerable<IEnvironmentLightClass> GetEnvironmentLightClasses(string projectName)
	{
		return GetClasses<IEnvironmentLightClass>(projectName);
	}

	public static IGeometryClass GetGeometryClass(string projectName, string className)
	{
		return GetClasses<IGeometryClass>(projectName).FirstOrDefault((IGeometryClass cls) => cls.Name == className);
	}

	public static IEnumerable<IGeometryClass> GetGeometryClasses(string projectName)
	{
		return GetClasses<IGeometryClass>(projectName);
	}

	public static IEnumerable<IImportedEntity> GetImportableEntities(ICivTechService civTechSvc, IAssetInstance asset, IInstanceSet instances, bool recursive)
	{
		ICollection<IImportedEntity> collection = new HashSet<IImportedEntity>(asset.GetImportableEntities(instances, recursive));
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(asset, ref project);
		if (!entityProject)
		{
			return Enumerable.Empty<IImportedEntity>();
		}
		IEnumerable<IAssetInstance> referencedAssets = asset.GetReferencedAssets(project.Config.Classes, instances, recursive);
		foreach (IAssetInstance item in referencedAssets)
		{
			IEnumerable<IImportedEntity> importableEntities = item.GetImportableEntities(instances, recursive);
			foreach (IImportedEntity item2 in importableEntities)
			{
				collection.Add(item2);
			}
		}
		return collection;
	}

	public static IEnumerable<IImportedEntity> GetImportableEntities(ICivTechService civTechSvc, IBehaviorInstance behavior, IInstanceSet instances, bool recursive)
	{
		ICollection<IImportedEntity> collection = new HashSet<IImportedEntity>(behavior.GetImportableEntities(instances, recursive));
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(behavior, ref project);
		if (!entityProject)
		{
			return Enumerable.Empty<IImportedEntity>();
		}
		IEnumerable<IAssetInstance> referencedAssets = behavior.GetReferencedAssets(project.Config.Classes, instances, recursive);
		foreach (IAssetInstance item in referencedAssets)
		{
			IEnumerable<IImportedEntity> importableEntities = item.GetImportableEntities(instances, recursive);
			foreach (IImportedEntity item2 in importableEntities)
			{
				collection.Add(item2);
			}
		}
		return collection;
	}

	public static IEnumerable<IImportedEntity> GetImportableEntities(ICivTechService civTechSvc, IInstanceEntity entity, IInstanceSet instances, bool recursive)
	{
		IAssetInstance assetInstance = entity as IAssetInstance;
		IBehaviorInstance behaviorInstance = entity as IBehaviorInstance;
		if (assetInstance != null)
		{
			return GetImportableEntities(civTechSvc, assetInstance, instances, recursive);
		}
		if (behaviorInstance != null)
		{
			return GetImportableEntities(civTechSvc, behaviorInstance, instances, recursive);
		}
		return entity.GetImportableEntities(instances, recursive);
	}

	public static IEnumerable<IImportedEntity> GetImportableEntities(ICivTechService civTechSvc, IValueSet values, IInstanceSet instances, bool recursive)
	{
		return GetImportableEntities(civTechSvc, values.Items, instances, recursive);
	}

	public static IEnumerable<IImportedEntity> GetImportableEntities(ICivTechService civTechSvc, IEnumerable<IValue> values, IInstanceSet instances, bool recursive)
	{
		List<IImportedEntity> list = new List<IImportedEntity>();
		foreach (IValue value in values)
		{
			IObjectValue objectValue = value as IObjectValue;
			IObjectCollectionValue objectCollectionValue = value as IObjectCollectionValue;
			if (objectValue != null)
			{
				if (string.IsNullOrEmpty(objectValue.GetBoundObjectName()))
				{
					continue;
				}
				IInstanceEntity instanceEntity = instances.LoadEntityByName(objectValue.GetBoundObjectName(), objectValue.GetBoundObjectType());
				if (instanceEntity != null)
				{
					if (StaticMethods.IsImportableType(objectValue.GetBoundObjectType()))
					{
						list.Add(instanceEntity as IImportedEntity);
					}
					if (recursive)
					{
						list.AddRange(GetImportableEntities(civTechSvc, instanceEntity.CookParameters.Items, instances, recursive));
					}
				}
			}
			else if (objectCollectionValue != null)
			{
				list.AddRange(GetImportableEntities(civTechSvc, objectCollectionValue.Items, instances, recursive));
			}
		}
		return list;
	}

	public static IEnumerable<string> GetImportableEntityNames(IValueSet values, IInstanceSet instances, bool recursive)
	{
		return GetImportableEntityNames(values.Items, instances, recursive);
	}

	public static IEnumerable<string> GetImportableEntityNames(IEnumerable<IValue> values, IInstanceSet instances, bool recursive)
	{
		List<string> list = new List<string>();
		foreach (IValue value in values)
		{
			IObjectValue objectValue = value as IObjectValue;
			IObjectCollectionValue objectCollectionValue = value as IObjectCollectionValue;
			if (objectValue != null)
			{
				if (string.IsNullOrEmpty(objectValue.GetBoundObjectName()))
				{
					continue;
				}
				IInstanceEntity instanceEntity = instances.LoadEntityByName(objectValue.GetBoundObjectName(), objectValue.GetBoundObjectType());
				if (instanceEntity != null)
				{
					list.Add(instanceEntity.Name);
					if (recursive)
					{
						list.AddRange(GetImportableEntityNames(instanceEntity.CookParameters, instances, recursive));
					}
				}
			}
			else if (objectCollectionValue != null)
			{
				list.AddRange(GetImportableEntityNames(objectCollectionValue.Items, instances, recursive));
			}
		}
		return list;
	}

	public static string GetInstanceDirectory(string projectName, InstanceType type)
	{
		ProjectEnvironment projectEnvironment = CivTechRegistry.CivTechService.AllProjectsMap[projectName];
		return StaticMethods.PantryRootForInstanceType(projectEnvironment.Paths.GamePantry, type).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
	}

	public static string GetInstanceDirectory(string projectName, IInstanceEntity entity)
	{
		return GetInstanceDirectory(projectName, entity.Type);
	}

	public static ILightRigClass GetLightRigClass(string projectName, string className)
	{
		return GetClasses<ILightRigClass>(projectName).FirstOrDefault((ILightRigClass lrc) => lrc.Name == className);
	}

	public static IEnumerable<ILightRigClass> GetLightRigClasses(string projectName)
	{
		return GetClasses<ILightRigClass>(projectName);
	}

	public static IMaterialClass GetMaterialClass(string projectName, string className)
	{
		return GetClasses<IMaterialClass>(projectName).FirstOrDefault((IMaterialClass lrc) => lrc.Name == className);
	}

	public static IEnumerable<IMaterialClass> GetMaterialClasses(string projectName)
	{
		return GetClasses<IMaterialClass>(projectName);
	}

	public static IParticleEffectClass GetParticleEffectClass(string projectName, string className)
	{
		IClassEntity classEntity = GetClass(projectName, InstanceType.IT_PARTICLE_EFFECT, className);
		return (classEntity == null) ? null : ((IParticleEffectClass)classEntity);
	}

	public static IEnumerable<IParticleEffectClass> GetParticleEffectClasses(string projectName)
	{
		return GetClasses<IParticleEffectClass>(projectName);
	}

	public static ITextureClass GetTextureClass(string projectName, string className)
	{
		return GetClasses<ITextureClass>(projectName).FirstOrDefault((ITextureClass cls) => cls.Name == className);
	}

	public static IEnumerable<ITextureClass> GetTextureClasses(string projectName)
	{
		return GetClasses<ITextureClass>(projectName);
	}

	public static bool IsEntityNameAvailable(string projectName, IInstanceEntity entity)
	{
		return IsEntityNameAvailable(projectName, entity.Type, entity.Name);
	}

	public static bool IsEntityNameAvailable(string projectName, InstanceType eType, string name)
	{
		if (DoesEntityExist(eType, name))
		{
			return false;
		}
		string text = GetInstanceDirectory(projectName, eType);
		if (!text.EndsWith(Path.DirectorySeparatorChar.ToString()) && !text.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
		{
			text += Path.DirectorySeparatorChar;
		}
		string text2 = text + name + EnumToStringConverter.GetExtensionFromType(eType);
		text2 = text2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		return !FileExistsInSourceControl(text2);
	}

	public static IEnumerable<IInstanceEntity> LoadAllInstanceEntities(string projectName, InstanceType entityType, IInstanceSet instances)
	{
		List<IInstanceEntity> list = new List<IInstanceEntity>();
		IQueryService queryService = CivTechRegistry.EntityQueryService.FindFilesByName(new string[1] { projectName }, string.Empty, null, new InstanceType[1] { entityType });
		foreach (string item in queryService.InstanceItems[entityType])
		{
			IInstanceEntity instanceEntity = instances.LoadEntityByName(item, entityType);
			if (instanceEntity != null)
			{
				list.Add(instanceEntity);
			}
		}
		return list;
	}

	public static IArtDef LoadArtDef(Uri path, IProjectConfig projCfg)
	{
		IArtDef artDef = Context.EnsureCreated<CivTechContext>().CreateInstance<IArtDef>(new object[1] { projCfg });
		if (!artDef.DeserializeFromFile(path.LocalPath))
		{
			artDef.Dispose();
			return null;
		}
		return artDef;
	}

	public static IInstanceSet LoadEntities(IVirtualPantry virtPan, IEnumerable<Tuple<string, InstanceType>> entities)
	{
		IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { CivTechRegistry.CivTechService.GetActivePantryPaths() });
		instanceSet.LoadEntities(virtPan, entities);
		return instanceSet;
	}

	public static IInstanceSet LoadEntities(IVirtualPantry virtPan, IEnumerable<KeyValuePair<string, InstanceType>> entities)
	{
		IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { CivTechRegistry.CivTechService.GetActivePantryPaths() });
		instanceSet.LoadEntities(entities);
		return instanceSet;
	}

	public static IEnumerable<IClassEntity> LoadEntityClasses(string projectName, InstanceType entityType, IEntityContainerClass assetClass)
	{
		List<IClassEntity> list = new List<IClassEntity>();
		IEnumerable<string> allowedClasses = GetAllowedClasses(assetClass, entityType);
		if (allowedClasses.Any())
		{
			foreach (string item in allowedClasses)
			{
				IClassEntity classEntity = GetClass(projectName, entityType, item);
				if (classEntity != null)
				{
					list.Add(classEntity);
				}
			}
		}
		return list;
	}

	public static IXLP LoadXLP(Uri path)
	{
		IXLP iXLP = Context.EnsureCreated<CivTechContext>().CreateInstance<IXLP>();
		if (!iXLP.DeserializeFromFile(path.LocalPath))
		{
			iXLP.Dispose();
			return null;
		}
		return iXLP;
	}

	public static bool AllFilesWriteable(IEnumerable<string> paths, out string errMsg)
	{
		bool result = true;
		errMsg = string.Empty;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string path in paths)
		{
			if (File.Exists(path))
			{
				FileInfo fileInfo = new FileInfo(path);
				if (fileInfo.IsReadOnly)
				{
					result = false;
				}
			}
			else if (FileExistsInSourceControl(path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)))
			{
				result = false;
				stringBuilder.AppendFormat("Path {0} exists in Perforce but not on disk, please sync your database and try again.\r\n", path);
			}
		}
		errMsg = stringBuilder.ToString();
		return result;
	}

	public static bool FileExistsInSourceControl(string entityPath)
	{
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(entityPath, ref project);
		if (!entityProject)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Failed to test for entity existence in source control\n\n{0}", entityProject.Message);
			return false;
		}
		return project.VersionControl.IsVersionControlled(entityPath);
	}

	public static string GetLocalPathFromDepotPath(string path)
	{
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(path, ref project);
		if (!entityProject)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Failed to get local path from depot path for entity\n\n{0}", entityProject.Message);
			return path;
		}
		return project.VersionControl.GetLocalPath(path);
	}

	public static bool OpenForAdd(IInstanceEntity entity, out string errMsg)
	{
		if (!EnsureDataFilesPopulated(entity, out errMsg))
		{
			return false;
		}
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(entity, ref project);
		if (!entityProject)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Failed to OpenForAdd entity \"{0}\"\n\n", entity.Name);
			stringBuilder.Append(entityProject.Message);
			errMsg = stringBuilder.ToString();
			return false;
		}
		IEnumerable<string> entityPaths = entity.GetEntityPaths();
		IList<string> list = new List<string>();
		if (!project.VersionControl.AddFiles(entityPaths, list))
		{
			errMsg = string.Join("\n", list);
			return false;
		}
		errMsg = string.Empty;
		return true;
	}

	public static bool OpenForEdit(IInstanceEntity entity, out string errMsg)
	{
		if (!EnsureDataFilesPopulated(entity, out errMsg))
		{
			return false;
		}
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(entity, ref project);
		if (!entityProject)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Failed to OpenForEdit entity \"{0}\"\n\n", entity.Name);
			stringBuilder.Append(entityProject.Message);
			errMsg = stringBuilder.ToString();
			return false;
		}
		IEnumerable<string> entityPaths = entity.GetEntityPaths();
		IList<string> list = new List<string>();
		if (!project.VersionControl.EditFiles(entityPaths, list))
		{
			errMsg = string.Join("\n", list);
			return false;
		}
		errMsg = string.Empty;
		return true;
	}

	public static bool OpenForEditOrAdd(string fullPath, out string errMsg)
	{
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(fullPath, ref project);
		if (!entityProject)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Failed to OpenForEditOrAdd entity at path \"{0}\"\n\n", fullPath);
			stringBuilder.Append(entityProject.Message);
			errMsg = stringBuilder.ToString();
			return false;
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		IVersionControlService versionControl = project.VersionControl;
		if (!OpenForEditOrAdd(versionControl, fullPath, stringBuilder2))
		{
			errMsg = stringBuilder2.ToString();
			return false;
		}
		errMsg = string.Empty;
		return true;
	}

	public static bool OpenIfRequired(IInstanceEntity entity, out string errMsg)
	{
		bool flag = true;
		if (!EnsureDataFilesPopulated(entity, out errMsg))
		{
			return false;
		}
		string entityPath = string.Empty;
		ResultCode entityPath2 = CivTechRegistry.CivTechService.GetEntityPath(entity, out entityPath);
		if (!entityPath2)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Failed to OpenIfRequired entity \"{0}\"\n\n", entity.Name);
			stringBuilder.Append(entityPath2.Message);
			errMsg = stringBuilder.ToString();
			return false;
		}
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(entity, ref project);
		if (!entityProject)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.AppendFormat("Failed to OpenIfRequired entity \"{0}\"\n\n", entity.Name);
			stringBuilder2.Append(entityProject.Message);
			errMsg = stringBuilder2.ToString();
			return false;
		}
		StringBuilder stringBuilder3 = new StringBuilder();
		flag &= OpenForEditOrAdd(project.VersionControl, entityPath, stringBuilder3);
		string directoryName = Path.GetDirectoryName(entityPath);
		foreach (IInstanceDataFile dataFile in entity.DataFiles)
		{
			string fullPath = Path.Combine(directoryName, dataFile.RelativePath);
			flag &= OpenForEditOrAdd(project.VersionControl, fullPath, stringBuilder3);
		}
		errMsg = stringBuilder3.ToString();
		return flag;
	}

	public static bool RevertEntity(IInstanceEntity entity, out string errMsg)
	{
		if (!EnsureDataFilesPopulated(entity, out errMsg))
		{
			return false;
		}
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(entity, ref project);
		if (!entityProject)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Failed to RevertEntity entity \"{0}\"\n\n", entity.Name);
			stringBuilder.Append(entityProject.Message);
			errMsg = stringBuilder.ToString();
			return false;
		}
		IEnumerable<string> entityPaths = entity.GetEntityPaths();
		IList<string> list = new List<string>();
		if (!project.VersionControl.RevertFiles(entityPaths, list))
		{
			errMsg = string.Join("\n", list);
			return false;
		}
		errMsg = string.Empty;
		return true;
	}

	public static bool RevertEntityIfOpen(IInstanceEntity entity, out string errMsg)
	{
		return RevertEntity(entity, out errMsg);
	}

	public static bool DeleteFile(string filePath, out string errMsg)
	{
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(filePath, ref project);
		if (!entityProject)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Failed to DeleteFile entity at path \"{0}\"\n\n", filePath);
			stringBuilder.Append(entityProject.Message);
			errMsg = stringBuilder.ToString();
			return false;
		}
		IVersionControlService versionControl = project.VersionControl;
		versionControl.RevertFile(filePath, out errMsg);
		if (versionControl.IsVersionControlled(filePath))
		{
			if (versionControl.DeleteFile(filePath, out errMsg))
			{
				errMsg = $"Document {filePath} could not be deleted.\n{errMsg}\n" + errMsg;
				return false;
			}
		}
		else
		{
			try
			{
				File.Delete(filePath);
			}
			catch (Exception ex)
			{
				errMsg = $"Document {filePath} could not be deleted.\n{errMsg}";
				BugSubmitter.SilentReport($"Couldn't delete the files: {filePath} \n {ex.Message} @assign agould @summary Couldn't delete the requested file from disk");
				return false;
			}
		}
		return !File.Exists(filePath);
	}

	private static bool EnsureDataFilesPopulated(IInstanceEntity entity, out string errMsg)
	{
		ProjectEnvironment project = null;
		ResultCode entityProject = CivTechRegistry.CivTechService.GetEntityProject(entity, ref project);
		if (!entityProject)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Failed to populated data files\n\n");
			stringBuilder.Append(entityProject.Message);
			errMsg = stringBuilder.ToString();
			return false;
		}
		IClassEntity classEntity = project.Config.Classes.FindForInstance(entity);
		if (classEntity == null)
		{
			errMsg = $"Failed to find asset class for entity \"{entity.Name}\"";
			return false;
		}
		IEnumerable<string> first = classEntity.DataFiles.Select((IClassDataFile df) => df.ID);
		IEnumerable<string> second = entity.DataFiles.Select((IInstanceDataFile df) => df.ID);
		if (first.Except(second).Any())
		{
			entity.PopulateDataFiles(classEntity);
		}
		errMsg = string.Empty;
		return true;
	}

	private static bool OpenForEditOrAdd(IVersionControlService vcs, string fullPath, StringBuilder errStrBldr)
	{
		string errMsg;
		if (!vcs.IsVersionControlled(fullPath) || vcs.IsMarkedForDelete(fullPath))
		{
			if (!vcs.AddFile(fullPath, out errMsg))
			{
				errStrBldr.Append(errMsg);
				return false;
			}
		}
		else if (!vcs.IsEditible(fullPath) && !vcs.EditFile(fullPath, out errMsg))
		{
			errStrBldr.Append(errMsg);
			return false;
		}
		return true;
	}

	private static bool OpenForRename(IVersionControlService vcs, string oldFullPath, string newFullPath, StringBuilder errStrBldr)
	{
		string errMsg;
		if (!vcs.IsVersionControlled(oldFullPath) || vcs.IsMarkedForDelete(oldFullPath))
		{
			if (!vcs.AddFile(newFullPath, out errMsg))
			{
				errStrBldr.Append(errMsg);
				return false;
			}
		}
		else
		{
			if (vcs.IsEditible(oldFullPath) && !vcs.RevertFile(oldFullPath, out errMsg))
			{
				errStrBldr.Append(errMsg);
				return false;
			}
			if (!vcs.DeleteFile(oldFullPath, out errMsg))
			{
				errStrBldr.Append(errMsg);
				if (!vcs.RevertFile(oldFullPath, out errMsg))
				{
					errStrBldr.Append(errMsg);
				}
				return false;
			}
			if (!vcs.AddFile(newFullPath, out errMsg))
			{
				errStrBldr.Append(errMsg);
				return false;
			}
		}
		return true;
	}

	public static IEnumerable<string> BuildDefaultTagList(IInstanceEntity entity)
	{
		List<string> list = new List<string>();
		list.Add(entity.ClassName);
		if (entity.ClassName.Contains('_'))
		{
			list.AddRange(entity.ClassName.Split('_'));
		}
		return list;
	}

	public static IEnumerable<string> BuildDefaultTagList(IImportedEntity entity)
	{
		List<string> list = new List<string>(BuildDefaultTagList((IInstanceEntity)entity));
		string path = entity.SourceFilePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		list.Add(Path.GetFileNameWithoutExtension(path));
		list.Add(entity.SourceObjectName);
		return list;
	}

	public static bool SerializeEntity(ICivTechService civTechSvc, IInstanceEntity entity, out string errMsg)
	{
		string entityPath = string.Empty;
		ResultCode entityPath2 = civTechSvc.GetEntityPath(entity, out entityPath);
		if (!entityPath2)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Failed to SerializeEntity entity \"{0}\"\n\n", entity.Name);
			stringBuilder.Append(entityPath2.Message);
			errMsg = stringBuilder.ToString();
			return false;
		}
		entityPath = entityPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		if (File.Exists(entityPath) && (File.GetAttributes(entityPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
		{
			errMsg = $"Failed to SerializeEntity entity \"{entity.Name}\", file \"{entityPath}\" exists and is read-only. Did the check out fail?";
			return false;
		}
		errMsg = string.Empty;
		return entity.SerializeIntoFile(entityPath);
	}

	public static bool UploadEntities(ICivTechService civTechSvc, IEnumerable<IInstanceEntity> entities, out string errMsg)
	{
		bool result = true;
		errMsg = string.Empty;
		foreach (IInstanceEntity entity in entities)
		{
			string errMsg2 = string.Empty;
			if (!UploadEntity(civTechSvc, entity, out errMsg2))
			{
				errMsg += errMsg2;
				result = false;
			}
		}
		return result;
	}

	public static bool UploadEntity(ICivTechService civTechSvc, IInstanceEntity entity, out string errMsg)
	{
		entity.Name = entity.Name.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		if (entity is IImportedEntity entity2)
		{
			return UploadImportedEntity(civTechSvc, entity2, out errMsg);
		}
		return UploadInstanceEntity(civTechSvc, entity, out errMsg);
	}

	private static void UploadEntities(ICivTechService civTechSvc, IDictionary<IImportedEntity, ImportOperationResult> entities)
	{
		foreach (IImportedEntity validEntity in entities.GetValidEntities())
		{
			validEntity.Name = validEntity.Name.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (!UploadEntity(civTechSvc, validEntity, out var errMsg))
			{
				entities[validEntity].Result = new ResultCode($"This entity failed during the upload step.  Reason: {errMsg}");
			}
		}
	}

	private static bool UploadImportedEntity(ICivTechService civTechSvc, IImportedEntity entity, out string errMsg)
	{
		if (!UploadInstanceEntity(civTechSvc, entity, out errMsg))
		{
			return false;
		}
		entity.UpdateImportedTime();
		errMsg = string.Empty;
		return true;
	}

	private static bool UploadInstanceEntity(ICivTechService civTechSvc, IInstanceEntity entity, out string errMsg)
	{
		IEnumerable<string> first = BuildDefaultTagList(entity);
		IEnumerable<string> enumerable = first.Except(entity.Tags);
		foreach (string item in enumerable)
		{
			entity.AddTag(item);
		}
		if (!OpenIfRequired(entity, out errMsg))
		{
			return false;
		}
		if (!SerializeEntity(civTechSvc, entity, out errMsg))
		{
			return false;
		}
		errMsg = string.Empty;
		return true;
	}
}

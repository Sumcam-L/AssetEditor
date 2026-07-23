using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace DatabaseWrapper;

public class WorkspaceDependencyUpdater : WorkspaceDependencyBase
{
	private IDepotCatalog m_addedFiles = new FileDepotCatelog();

	private ISet<string> m_deletedFiles = new HashSet<string>(new PathComparer());

	private ISet<DepotFileInfo> m_invalidFiles = new HashSet<DepotFileInfo>();

	private ISet<DepotFileInfo> m_modifiedFiles = new HashSet<DepotFileInfo>();

	public WorkspaceDependencyUpdater(IProjectMapService projMapSvc, IProjectConfigService projCfgSvc, string targetProject)
		: base(projMapSvc, projCfgSvc, targetProject, testFilesExist: false)
	{
	}

	public void AddDependency(Uri fileUri, IDatabaseDependencies dbDeps)
	{
		string localPath = fileUri.LocalPath;
		string depotRootedPath = GetDepotRootedPath(localPath);
		using IInstanceSet instSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.PantryRoots });
		UpdateDependencyData(instSet, dbDeps, depotRootedPath, ContainerCrawlOptions.DoEverything);
	}

	public void RemoveDependency(Uri fileUri, IDatabaseDependencies dbDeps)
	{
		string localPath = fileUri.LocalPath;
		string depotRootedPath = GetDepotRootedPath(localPath);
		if (dbDeps.Files.ContainsKey(depotRootedPath))
		{
			dbDeps.Files.Remove(depotRootedPath);
			dbDeps.Dependencies.RemoveKey(depotRootedPath);
		}
	}

	public override bool UpdateDependencies(IDatabaseDependencies workspaceDependencies)
	{
		var updTimer = Stopwatch.StartNew();
		using (new ScopedStopwatch(base.TargetProject + ": Evaluating environment took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			Directory.EnumerateFiles(base.PrimaryPantryRoot, "*", SearchOption.AllDirectories).ForEach(delegate(string file)
			{
				AddToFileCatalogIfNew(workspaceDependencies.Files, file);
			});
			workspaceDependencies.Files.ParallelForEachValue(delegate(DepotFileInfo value)
			{
				if (HasBeenDeleted(value))
				{
					AddToDeletedFiles(value);
				}
				else if (HasBeenModified(value))
				{
					AddToModifiedFiles(value);
				}
				else if (HasInvalidEntityInfo(value))
				{
					AddToInvalidFileInfo(value);
				}
			});
		}
		PaintTimingLog.Write("Startup: deps-eval-env elapsed={0}ms", updTimer.ElapsedMilliseconds);
		Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "{0}: Discovered {1} new files, {2} modified files, {3} deleted files, and {4} invalid file infos", base.TargetProject, m_addedFiles.Count, m_modifiedFiles.Count, m_deletedFiles.Count, m_invalidFiles.Count);
		IInstanceSet entitySet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.PantryRoots });
		try
		{
			using (new ScopedStopwatch(base.TargetProject + ": Updating dependency and file info took {0} seconds", delegate(string str)
			{
				Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
			}))
			{
				m_deletedFiles.ForEach(delegate(string filePath)
				{
					MarkAsDangling(workspaceDependencies, filePath);
				});
				m_addedFiles.ForEachValue(delegate(DepotFileInfo item)
				{
					if (item.Type == 2)
					{
						UpdateEntityFileInfo(entitySet, workspaceDependencies, item.Filename);
					}
					else
					{
						UpdateDepotFileInfo(workspaceDependencies, item);
					}
				});
				m_modifiedFiles.ForEach(delegate(DepotFileInfo item)
				{
					if (item.Type == 2)
					{
						UpdateEntityFileInfo(entitySet, workspaceDependencies, item.Filename);
					}
					else
					{
						UpdateDepotFileInfo(workspaceDependencies, item);
					}
				});
				m_addedFiles.ForEachValue(delegate(DepotFileInfo item)
				{
					if (HasDependencies(item))
					{
						UpdateDependencyData(entitySet, workspaceDependencies, item, ContainerCrawlOptions.DoEverything);
					}
				});
				m_modifiedFiles.ForEach(delegate(DepotFileInfo item)
				{
					if (HasDependencies(item))
					{
						UpdateDependencyData(entitySet, workspaceDependencies, item, ContainerCrawlOptions.DoUpdate);
					}
				});
			}
			PaintTimingLog.Write("Startup: deps-update-info elapsed={0}ms", updTimer.ElapsedMilliseconds);
			using (new ScopedStopwatch(base.TargetProject + ": Repairing entity classes for " + m_invalidFiles.Count + " entities took {0} seconds", delegate(string str)
			{
				Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
			}))
			{
				m_invalidFiles.ForEach(delegate(DepotFileInfo item)
				{
					UpdateEntityFileInfo(entitySet, workspaceDependencies, item.Filename);
				});
			}
			PaintTimingLog.Write("Startup: deps-repair elapsed={0}ms", updTimer.ElapsedMilliseconds);
		}
		finally
		{
			if (entitySet != null)
			{
				entitySet.Dispose();
			}
		}
		return true;
	}

	private void AddToDeletedFiles(DepotFileInfo info)
	{
		string fullPath = GetFullPath(info.Filename);
		lock (m_deletedFiles)
		{
			if (!m_deletedFiles.Contains(fullPath))
			{
				m_deletedFiles.Add(fullPath);
			}
		}
	}

	private void AddToInvalidFileInfo(DepotFileInfo info)
	{
		lock (m_invalidFiles)
		{
			if (!m_invalidFiles.Contains(info))
			{
				m_invalidFiles.Add(info);
			}
		}
	}

	private void AddToModifiedFiles(DepotFileInfo info)
	{
		lock (m_modifiedFiles)
		{
			if (!m_modifiedFiles.Contains(info))
			{
				m_modifiedFiles.Add(info);
			}
		}
	}

	private void AddToFileCatalogIfNew(IDepotCatalog fileDepot, string filePath)
	{
		string depotPath = string.Empty;
		string localPath = filePath;
		GetPaths(filePath, out localPath, out depotPath);
		if (!fileDepot.ContainsKey(depotPath))
		{
			FileType fileType = GetFileType(filePath);
			InstanceType type = InstanceType.IT_INVALID;
			StaticMethods.GetInstanceType(filePath, out type);
			AddFileIfUnique(filePath, fileDepot, fileType, FileStatus.Unknown, type, string.Empty, out var depotRootedPath);
			BugSubmitter.SilentAssert(depotRootedPath == depotPath, "Depot rooted path generation failed for \"{0}\" @summary Depot rooted path generation failed @assign bwhitman", filePath);
			AddFileIfUnique(filePath, m_addedFiles, fileType, FileStatus.Unknown, type, string.Empty, out depotRootedPath);
		}
	}

	private bool ExistsInPantryDepInfo(IDatabaseDependencies workspaceDependencies, string filePath)
	{
		string depotRootedPath = GetDepotRootedPath(filePath);
		return workspaceDependencies.Dependencies.ContainsKey(depotRootedPath);
	}

	private bool HasBeenDeleted(DepotFileInfo info)
	{
		string fullPath = GetFullPath(info.Filename);
		return !File.Exists(fullPath);
	}

	private bool HasInvalidEntityInfo(DepotFileInfo info)
	{
		if (info.Type == 2)
		{
			return string.IsNullOrEmpty(info.EntityClass);
		}
		if (info.Type == 5)
		{
			InstanceType type = InstanceType.IT_INVALID;
			return StaticMethods.GetInstanceType(info.Filename, out type) && info.EntityType != (int)type;
		}
		return false;
	}

	private void MarkAsDangling(IDatabaseDependencies workspaceDependencies, string fullPath)
	{
		GetPaths(fullPath, out var localPath, out var depotPath);
		string fileKey = (string.IsNullOrEmpty(depotPath) ? localPath : depotPath);
		if (workspaceDependencies.Files.TryGetValue(fileKey, out var info))
		{
			info.Status = 1;
			workspaceDependencies.Files.AddOrUpdate(fileKey, info);
			RemoveDependencyData(workspaceDependencies, info);
		}
	}

	private void RemoveDependencyData(IDatabaseDependencies workspaceDependencies, DepotFileInfo item)
	{
		workspaceDependencies.Dependencies.RemoveKey(item.Filename);
	}

	private void UpdateDependencyData(IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, DepotFileInfo item, ContainerCrawlOptions crawlOptions)
	{
		UpdateDependencyData(instSet, workspaceDependencies, item.Filename, crawlOptions);
	}

	private void UpdateDependencyData(IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, string depotRootedFilename, ContainerCrawlOptions crawlOptions)
	{
		string fullPath = GetFullPath(depotRootedFilename);
		if (StaticMethods.GetInstanceNameAndType(base.ProjectMapService, fullPath, out var instanceName, out var type))
		{
			IInstanceEntity instanceEntity = EnsureEntityLoaded(instanceName, type, fullPath, instSet);
			if (instanceEntity != null)
			{
				int dependencyCount = 0;
				AddEntityDependencies(instanceEntity, depotRootedFilename, instSet, workspaceDependencies, crawlOptions, ref dependencyCount);
				AddRootFileIfNew(fullPath, type, instanceEntity.ClassName, FileStatus.Normal, workspaceDependencies);
				AddEntityStatsAndTags(instanceEntity, workspaceDependencies, depotRootedFilename);
			}
			else
			{
				BugSubmitter.SilentReport($"Failed to deserialize entity dependency in UpdateDependencyData for entity \"{fullPath}\" with active project \"{base.TargetProject}\" @summary Failed to deserialize entity dependency in UpdateDependencyData @assign bwhitman");
				AddRootFileIfNew(fullPath, type, string.Empty, FileStatus.FailedToLoad, workspaceDependencies);
			}
		}
		else
		{
			string text = Path.GetExtension(fullPath).ToLower();
			if (text == ".artdef")
			{
				AddArtDefDependencies(fullPath, instSet, workspaceDependencies, crawlOptions);
			}
			else if (text == ".xlp")
			{
				int dependencyCount2 = 0;
				AddXLPDependencies(fullPath, instSet, workspaceDependencies, crawlOptions, ref dependencyCount2);
			}
		}
	}

	private void UpdateDepotFileInfo(IDatabaseDependencies workspaceDependencies, DepotFileInfo item)
	{
		string fullPath = GetFullPath(item.Filename);
		BugSubmitter.SilentAssert(item.Type != 2, "Attempted to UpdateDepotFileInfo for \"{0}\" ({1}) as if it were a non-entity @summary Entity file info must be updated via UpdateEntityFileInfo @assign bwhitman", item.Filename, fullPath);
		DepotFileInfo info = new DepotFileInfo
		{
			Status = ((!File.Exists(fullPath)) ? 1 : 0),
			Filename = item.Filename,
			Tags = item.Tags,
			Stats = item.Stats,
			Type = item.Type,
			EntityType = item.EntityType,
			EntityClass = item.EntityClass
		};
		if (info.Status == 0)
		{
			FileInfo fileInfo = new FileInfo(fullPath);
			info.Filesize = fileInfo.Length;
			info.Timestamp = fileInfo.LastWriteTimeUtc.ToFileTimeUtc();
		}
		else
		{
			info.Filesize = 0L;
			info.Timestamp = 0L;
		}
		workspaceDependencies.Files.AddOrUpdate(info.Filename, info);
	}

	private void UpdateEntityFileInfo(IInstanceSet entitySet, IDatabaseDependencies workspaceDependencies, string rootedPath)
	{
		if (workspaceDependencies.Files.TryGetValue(rootedPath, out var info))
		{
			string fullPath = GetFullPath(rootedPath);
			BugSubmitter.SilentAssert(info.Type == 2, "Attempted to UpdateEntityFileInfo for \"{0}\" ({1}) as if it were an entity @summary Non-entity file info must be updated via UpdateDepotFileInfo @assign bwhitman", rootedPath, fullPath);
			InstanceType entityType = (InstanceType)info.EntityType;
			IInstanceEntity instanceEntity = LoadEntity(entityType, fullPath, entitySet);
			if (instanceEntity != null)
			{
				info.Status = ((!File.Exists(fullPath)) ? 1 : 0);
				info.Filename = rootedPath;
				info.Tags = new List<string>(instanceEntity.Tags);
				info.EntityClass = instanceEntity.ClassName;
				info.EntityType = (int)instanceEntity.Type;
				info.Type = 2;
				if (info.Status == 0)
				{
					FileInfo fileInfo = new FileInfo(fullPath);
					info.Filesize = fileInfo.Length;
					info.Timestamp = fileInfo.LastWriteTimeUtc.ToFileTimeUtc();
				}
				else
				{
					info.Filesize = 0L;
					info.Timestamp = 0L;
				}
				workspaceDependencies.Files.AddOrUpdate(rootedPath, info);
			}
			else if (!File.Exists(fullPath))
			{
				Outputs.Write(OutputMessageType.Error, "Removing dependency information after failed to update entity info for file \"{0}\" of type {1}", rootedPath, entityType);
				workspaceDependencies.Files.Remove(rootedPath);
			}
		}
		else
		{
			Outputs.Write(OutputMessageType.Error, "Failed to find file in file catalog during update of entity info for file \"{0}\"", rootedPath);
		}
	}
}

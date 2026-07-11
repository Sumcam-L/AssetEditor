using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Firaxis.Error;
using Firaxis.Utility;
using Sce.Atf;

namespace DatabaseWrapper;

public abstract class WorkspaceDependencyBase : IWorkspaceMapper
{
	[Flags]
	public enum ContainerCrawlOptions
	{
		None = 0,
		AddToDependencyCatalog = 1,
		AddToFileCatalog = 2,
		ChaseDependenciesForExistingFiles = 4,
		UpdateExistingDependencyInfo = 8,
		DoBuild = 7,
		DoUpdate = 0xB,
		DoEverything = 0xF
	}

	private const int kMaxEntityLoadRetries = 5;

	private string m_artDefRoot;

	private IList<string> m_depotRoots = new List<string>();

	private IList<string> m_pantryRoots = new List<string>();

	private IList<string> m_artDevRoots = new List<string>();

	private string[] m_srcExts = new string[7] { ".tga", ".max", ".ma", ".mb", ".dds", ".psd", ".peb" };

	private string m_workspaceRoot;

	private string m_xlpRoot;

	protected bool TestFilesExist { get; private set; }

	protected string ArtDefRoot => m_artDefRoot;

	protected IEnumerable<string> PantryRoots => m_pantryRoots;

	protected IEnumerable<string> ArtDevRoots => m_artDevRoots;

	protected string PrimaryPantryRoot => m_pantryRoots[0];

	protected string PrimaryArtDevRoot => m_artDevRoots[0];

	protected IProjectConfigService ProjectConfigService { get; private set; }

	protected IProjectMapService ProjectMapService { get; private set; }

	protected string XLPRoot => m_xlpRoot;

	protected string TargetProject { get; private set; }

	protected IVirtualPantry LayeredPantry { get; private set; }

	public abstract bool UpdateDependencies(IDatabaseDependencies workspaceDependencies);

	public WorkspaceDependencyBase(IProjectMapService projMapSvc, IProjectConfigService projCfgSvc, string targetProject, bool testFilesExist)
	{
		TargetProject = targetProject;
		ProjectMapService = projMapSvc;
		ProjectConfigService = projCfgSvc;
		TestFilesExist = testFilesExist;
		ProjectEnvironment project = null;
		if (!projMapSvc.AllProjectsMap.GetProject(targetProject, ref project))
		{
			return;
		}
		m_artDefRoot = EnsureTrailingSlash(project.Paths.ArtDefRoot);
		m_xlpRoot = EnsureTrailingSlash(project.Paths.XLPRoot);
		m_workspaceRoot = EnsureTrailingSlash(project.VersionControl.WorkspaceRoot).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		foreach (ProjectEnvironment projectAndDependency in ProjectMapService.GetProjectAndDependencies(project))
		{
			string text = EnsureTrailingSlash(projectAndDependency.VersionControl.WorkspaceRoot).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string text2 = EnsureTrailingSlash(projectAndDependency.Paths.GamePantry).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string item = EnsureTrailingSlash(projectAndDependency.Paths.ArtDev).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			m_depotRoots.Add(text);
			m_pantryRoots.Add(text2);
			m_artDevRoots.Add(item);
			BugSubmitter.SilentAssert(text2.StartsWith(text, StringComparison.InvariantCultureIgnoreCase), "For active project \"{0}\" and dependency project \"{1}\" the pantry root \"{2}\" is not a sub directory of depot Root \"{3}\" @summary Pantry root is not a subdirectory of depot root @assign bwhitman", targetProject, projectAndDependency.Name, text2, text);
		}
		IEnumerable<string> projectPantryPaths = ProjectMapService.GetProjectPantryPaths(project);
		LayeredPantry = Context.EnsureCreated<CivTechContext>().CreateInstance<IVirtualPantry>(new object[1] { projectPantryPaths });
	}

	public bool GetFileInfo(IDatabaseDependencies workspaceDependencies, Uri item, ref DepotFileInfo info)
	{
		if (!GetPaths(item.LocalPath, out var _, out var depotPath))
		{
			return false;
		}
		if (!workspaceDependencies.Files.TryGetValue(depotPath, out info))
		{
			return false;
		}
		return true;
	}

	public FileType GetFileType(IDatabaseDependencies workspaceDependencies, Uri item)
	{
		if (!GetPaths(item.LocalPath, out var _, out var depotPath))
		{
			if (Path.IsPathRooted(item.LocalPath))
			{
				return GetFileType(item.LocalPath);
			}
			return FileType.Unknown;
		}
		if (workspaceDependencies.Files.TryGetValue(depotPath, out var info))
		{
			return (FileType)info.Type;
		}
		return GetFileType(item.LocalPath);
	}

	public string GetFullPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return string.Empty;
		}
		if (IsDepotPath(path) && (bool)GetLocalPathFromPerforcePath(path, out var filePath))
		{
			return filePath;
		}
		if (Path.IsPathRooted(path))
		{
			if (path[0] != Path.AltDirectorySeparatorChar && path[0] != Path.DirectorySeparatorChar)
			{
				return path;
			}
			return Path.Combine(m_depotRoots[0], path.Substring(1));
		}
		return Path.Combine(m_depotRoots[0], path);
	}

	public bool VerifyAndSaveDatabase(IDatabaseDependencies database, string outFilePath)
	{
		DatabaseDependencies databaseDependencies = new DatabaseDependencies();
		databaseDependencies.Changelist = database.Changelist;
		databaseDependencies.Timestamp = database.Timestamp;
		SortedSet<string> filePaths = new SortedSet<string>(database.Dependencies.Keys, StringComparer.CurrentCultureIgnoreCase);
		database.Files.ForEachKey(delegate(string key)
		{
			filePaths.Add(key);
		});
		using (IInstanceSet temporaryEntitySet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { ProjectMapService.GetActivePantryPaths() }))
		{
			foreach (string item in filePaths)
			{
				if (database.Files.TryGetValue(item, out var info) && info.Status == 0)
				{
					VerifyDepotFile(database, item, temporaryEntitySet, ref info);
					databaseDependencies.Files.AddOrUpdate(item, info);
					databaseDependencies.Dependencies.AddChildren(item, database.Dependencies[item]);
				}
			}
		}
		return databaseDependencies.Save(outFilePath);
	}

	public void VerifyDatabase(IDatabaseDependencies database)
	{
		using IInstanceSet temporaryEntitySet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { ProjectMapService.GetActivePantryPaths() });
		SortedSet<string> filePaths = new SortedSet<string>(database.Dependencies.Keys, StringComparer.CurrentCultureIgnoreCase);
		database.Files.ForEachKey(delegate(string key)
		{
			filePaths.Add(key);
		});
		foreach (string item in filePaths)
		{
			if (database.Files.TryGetValue(item, out var info) && info.Status == 0)
			{
				VerifyDepotFile(database, item, temporaryEntitySet, ref info);
				database.Files.AddOrUpdate(item, info);
			}
		}
	}

	protected void AddArtDefDependencies(string inputPath, IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions)
	{
		string fullPath = GetFullPath(inputPath);
		string depotRootedPath = GetDepotRootedPath(inputPath);
		bool flag = crawlOptions.HasFlag(ContainerCrawlOptions.ChaseDependenciesForExistingFiles);
		if (!workspaceDependencies.Files.ContainsKey(depotRootedPath) && crawlOptions.HasFlag(ContainerCrawlOptions.AddToFileCatalog))
		{
			flag |= AddRootFileIfNew(fullPath, FileType.ArtDef, FileStatus.Unknown, workspaceDependencies);
		}
		if (!flag)
		{
			return;
		}
		using (new ScopedStopwatch(TargetProject + ": Processing ArtDef \"" + depotRootedPath + "\" took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			ProcessArtDefDependencies(fullPath, depotRootedPath, instSet, workspaceDependencies, crawlOptions);
		}
	}

	protected void AddXLPDependencies(string inputPath, IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions, ref int dependencyCount)
	{
		string fullPath = GetFullPath(inputPath);
		string depotRootedPath = GetDepotRootedPath(inputPath);
		bool flag = crawlOptions.HasFlag(ContainerCrawlOptions.ChaseDependenciesForExistingFiles);
		if (!workspaceDependencies.Files.ContainsKey(depotRootedPath) && crawlOptions.HasFlag(ContainerCrawlOptions.AddToFileCatalog))
		{
			flag |= AddRootFileIfNew(fullPath, FileType.XLP, FileStatus.Unknown, workspaceDependencies);
		}
		if (!flag)
		{
			return;
		}
		using (new ScopedStopwatch(TargetProject + ": Processing XLP \"" + depotRootedPath + "\" took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			ProcessXLPDependencies(fullPath, depotRootedPath, instSet, workspaceDependencies, crawlOptions, ref dependencyCount);
		}
	}

	protected bool AddFileIfUnique(string filePath, IDepotCatalog fileList, FileType ft, FileStatus fs, InstanceType it, string entClass, out string depotRootedPath)
	{
		bool flag = false;
		DepotFileInfo info = CreateFileInfo(filePath, ft, fs, it, entClass);
		if (!fileList.ContainsKey(info.Filename))
		{
			depotRootedPath = GetDepotRootedPath(info.Filename);
			flag = true;
		}
		else
		{
			depotRootedPath = string.Empty;
			fileList.TryGetValue(info.Filename, out info);
			info.EntityType = (int)it;
			if (!string.IsNullOrEmpty(entClass))
			{
				info.EntityClass = entClass;
			}
			flag = HasBeenModified(info) || (info.EntityClass == null && info.Type == 2);
		}
		fileList.AddOrUpdate(info.Filename, info);
		return flag;
	}

	protected bool AddRootFileIfNew(string filePath, InstanceType it, string entClass, FileStatus fs, IDatabaseDependencies workspaceDependencies)
	{
		string depotRootedPath;
		return AddFileIfUnique(filePath, workspaceDependencies.Files, FileType.Entity, fs, it, entClass, out depotRootedPath);
	}

	protected bool AddRootFileIfNew(string filePath, FileType ft, FileStatus fs, IDatabaseDependencies workspaceDependencies)
	{
		string depotRootedPath;
		return AddFileIfUnique(filePath, workspaceDependencies.Files, ft, fs, InstanceType.IT_INVALID, string.Empty, out depotRootedPath);
	}

	protected bool BeginsWithPantryRoot(string filePath)
	{
		foreach (string pantryRoot in m_pantryRoots)
		{
			if (PathCompareHelper.StartsWith(filePath, pantryRoot, bIgnoreCase: true))
			{
				return true;
			}
		}
		return false;
	}

	protected IInstanceEntity EnsureEntityLoaded(string name, InstanceType insType, string insPath, IInstanceSet instSet)
	{
		return instSet.LoadEntityByPath(insPath, insType);
	}

	public virtual string GetDepotRootedPath(string filePath)
	{
		BugSubmitter.SilentAssert(m_artDevRoots.Count == m_pantryRoots.Count, "Pantry roots has {0} elements and and ArtDev roots has {1} elements @summary Pantry roots and ArtDev roots lists are out of sync @assign bwhitman", m_pantryRoots.Count, m_artDevRoots.Count);
		for (int i = 0; i < m_pantryRoots.Count; i++)
		{
			string text = m_pantryRoots[i];
			string text2 = m_artDevRoots[i];
			if (PathCompareHelper.StartsWith(filePath, text, bIgnoreCase: true))
			{
				string text3 = m_depotRoots[i];
				BugSubmitter.SilentAssert(PathCompareHelper.StartsWith(text, text3, bIgnoreCase: true), "When processing \"{0}\" at index {1} pantry root \"{2}\" is not a sub directory of depot Root \"{3}\" @summary Pantry root is not a subdirectory of depot root @assign bwhitman", filePath, i, text, text3);
				return filePath.Substring(text3.Length).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			}
			if (PathCompareHelper.StartsWith(filePath, text2, bIgnoreCase: true))
			{
				string text4 = m_depotRoots[i];
				BugSubmitter.SilentAssert(PathCompareHelper.StartsWith(text2, text4, bIgnoreCase: true), "When processing \"{0}\" at index {1} artdev root \"{2}\" is not a sub directory of depot Root \"{3}\" @summary Pantry root is not a subdirectory of depot root @assign bwhitman", filePath, i, text2, text4);
				return filePath.Substring(text4.Length).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			}
		}
		return GetWorkspaceRootedPath(filePath);
	}

	protected FileType GetFileType(string filePath)
	{
		if (IsEntity(filePath))
		{
			return FileType.Entity;
		}
		if (IsArtDef(filePath))
		{
			return FileType.ArtDef;
		}
		if (IsXLP(filePath))
		{
			return FileType.XLP;
		}
		if (IsDataFile(filePath))
		{
			return FileType.DataFile;
		}
		if (IsSourceFile(filePath))
		{
			return FileType.SourceFile;
		}
		return FileType.Unknown;
	}

	protected bool GetPaths(string filePath, out string localPath, out string depotPath)
	{
		if (IsDepotPath(filePath))
		{
			GetLocalPathFromPerforcePath(filePath, out localPath);
			depotPath = GetDepotRootedPath(localPath);
		}
		else
		{
			localPath = filePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			depotPath = GetDepotRootedPath(localPath);
		}
		return PathCompareHelper.EndsWith(localPath, depotPath, bIgnoreCase: true);
	}

	protected bool HasBeenModified(DepotFileInfo info)
	{
		string fullPath = GetFullPath(info.Filename);
		try
		{
			if (Path.IsPathRooted(fullPath) && File.Exists(fullPath))
			{
				DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(fullPath);
				DateTime a = DateTime.FromFileTimeUtc(info.Timestamp);
				return !DateTimeEqual(a, lastWriteTimeUtc);
			}
		}
		catch (IOException)
		{
		}
		return true;
	}

	protected bool HasDependencies(DepotFileInfo item)
	{
		string fullPath = GetFullPath(item.Filename);
		return IsEntity(fullPath) || IsXLP(fullPath) || IsArtDef(fullPath);
	}

	protected IInstanceEntity LoadEntity(InstanceType insType, string insPath, IInstanceSet instSet)
	{
		if (string.IsNullOrEmpty(insPath))
		{
			return null;
		}
		int num = 5;
		int num2 = 32;
		IInstanceEntity instanceEntity = instSet.LoadEntityByPath(insPath, insType);
		while (instanceEntity == null && --num >= 0)
		{
			Thread.Sleep(num2);
			num2 <<= 1;
			instanceEntity = instSet.LoadEntityByPath(insPath, insType);
		}
		if (instanceEntity == null)
		{
			BugSubmitter.SilentAssert(!File.Exists(insPath), "Failed to DeserializeFromFile for type {0} with path {1} for project {2} @summary Could not create entity instance deserialize from file @assign bwhitman", insType, insPath, TargetProject);
		}
		else
		{
			BugSubmitter.SilentAssert(num == 5, "Succeeded in DeserializeFromFile for type {0} with path {1} after {2} retries for project {3} @summary Succeeded in DeserializeFromFile after retries @assign bwhitman", insType, insPath, num, TargetProject);
		}
		return instanceEntity;
	}

	private void ProcessArtDefDependencies(string artDefPath, string depotRootedArtDefPath, IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions)
	{
		if (string.IsNullOrEmpty(artDefPath))
		{
			return;
		}
		float num = TimedOperation.Do(delegate
		{
			Uri uri = new Uri(artDefPath);
			using IArtDef artDef = LoadArtDef(uri, ProjectConfigService.Config);
			if (artDef == null)
			{
				BugSubmitter.SilentReport($"Failed to deserialize ArtDef in ProcessArtDefDependencies for ArtDef \"{artDefPath}\" with active project \"{TargetProject}\" @summary Failed to deserialize ArtDef in ProcessArtDefDependencies @assign bwhitman");
				AddRootFileIfNew(artDefPath, FileType.ArtDef, FileStatus.FailedToLoad, workspaceDependencies);
			}
			else
			{
				AddRootFileIfNew(artDefPath, FileType.ArtDef, FileStatus.Normal, workspaceDependencies);
				Action<IValue> act = CreateValueVisitor(uri, depotRootedArtDefPath, instSet, workspaceDependencies, crawlOptions);
				artDef.VisitAllValues(act);
			}
		});
		Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "Processing \"{0}\" took {1} secs", artDefPath, num);
	}

	private void ProcessEntityDependencies(InstanceType insType, string insPath, string entityDepotRootedPath, IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions, ref int dependencyCount)
	{
		if (instSet.FindByPath(insPath) != null)
		{
			return;
		}
		int locDepCnt = ++dependencyCount;
		float num = TimedOperation.Do(delegate
		{
			if (!workspaceDependencies.Dependencies.ContainsKey(entityDepotRootedPath) || crawlOptions.HasFlag(ContainerCrawlOptions.UpdateExistingDependencyInfo))
			{
				IInstanceEntity instanceEntity = LoadEntity(insType, insPath, instSet);
				if (instanceEntity == null)
				{
					if (!File.Exists(insPath))
					{
						Outputs.WriteLine(OutputMessageType.Error, OutputMessageVerbosity.Normal, "File not found while loading entity dependency \"{0}\"", insPath);
					}
					else
					{
						BugSubmitter.SilentReport($"Failed to deserialize entity dependency in ProcessEntityDependencies for entity \"{insPath}\" with active project \"{TargetProject}\" @summary Failed to deserialize entity dependency in ProcessEntityDependencies @assign bwhitman");
					}
					AddRootFileIfNew(insPath, insType, string.Empty, FileStatus.FailedToLoad, workspaceDependencies);
				}
				else
				{
					AddEntityDependencies(instanceEntity, entityDepotRootedPath, instSet, workspaceDependencies, crawlOptions, ref locDepCnt);
					AddEntityStatsAndTags(instanceEntity, workspaceDependencies, entityDepotRootedPath);
				}
			}
		});
		dependencyCount = locDepCnt;
		Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "Processing \"{0}\" took {1} secs", insPath, num);
	}

	private void AddEntityToEntityDependency(string parentDepotRootedPath, InstanceType childInstanceType, string childFullPath, IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions, ref int dependencyCount)
	{
		if (!string.IsNullOrEmpty(childFullPath))
		{
			string depotRootedPath = GetDepotRootedPath(childFullPath);
			bool flag = crawlOptions.HasFlag(ContainerCrawlOptions.ChaseDependenciesForExistingFiles);
			if (!workspaceDependencies.Files.ContainsKey(depotRootedPath) && crawlOptions.HasFlag(ContainerCrawlOptions.AddToFileCatalog))
			{
				flag |= AddRootFileIfNew(depotRootedPath, childInstanceType, string.Empty, FileStatus.Unknown, workspaceDependencies);
			}
			if (crawlOptions.HasFlag(ContainerCrawlOptions.AddToDependencyCatalog))
			{
				workspaceDependencies.Dependencies.AddIfUnique(parentDepotRootedPath, depotRootedPath);
			}
			dependencyCount++;
		}
	}

	protected void AddEntityDependencies(InstanceType insType, string inputPath, IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions, ref int dependencyCount)
	{
		string fullPath = GetFullPath(inputPath);
		string depotRootedPath = GetDepotRootedPath(inputPath);
		bool flag = crawlOptions.HasFlag(ContainerCrawlOptions.ChaseDependenciesForExistingFiles);
		if (!workspaceDependencies.Files.ContainsKey(depotRootedPath) && crawlOptions.HasFlag(ContainerCrawlOptions.AddToFileCatalog))
		{
			flag |= AddRootFileIfNew(fullPath, insType, string.Empty, FileStatus.Unknown, workspaceDependencies);
		}
		if (!flag)
		{
			return;
		}
		using (new ScopedStopwatch(TargetProject + ": Processing Entity \"" + depotRootedPath + "\" took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			ProcessEntityDependencies(insType, fullPath, depotRootedPath, instSet, workspaceDependencies, crawlOptions, ref dependencyCount);
		}
	}

	protected void AddEntityDependencies(IInstanceEntity entity, string entityDepotRootedPath, IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions, ref int dependencyCount)
	{
		AddEntitySourceFiles(entity, entityDepotRootedPath, workspaceDependencies, ref dependencyCount);
		AddEntityDataFiles(entity, entityDepotRootedPath, workspaceDependencies, ref dependencyCount);
		AddEntityDependenciesRecurse(entity, entityDepotRootedPath, instSet, workspaceDependencies, crawlOptions, ref dependencyCount);
	}

	protected void AddEntityDependenciesRecurse(IInstanceEntity entity, string entityDepotRootedPath, IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions, ref int dependencyCount)
	{
		int localProcCount = 0;
		entity.CrawlCooktimeDependencies(delegate(InstanceType insType, string insName)
		{
			AddEntityToEntityDependency(entityDepotRootedPath, insType, LayeredPantry.GetPantryPath(insName, insType), instSet, workspaceDependencies, crawlOptions, ref localProcCount);
		});
		entity.CrawlEntityTimelineDependencies(TriggerType.TT_ASSET_VFX, InstanceType.IT_ASSET, delegate(InstanceType insType, string insName)
		{
			AddEntityToEntityDependency(entityDepotRootedPath, insType, LayeredPantry.GetPantryPath(insName, insType), instSet, workspaceDependencies, crawlOptions, ref localProcCount);
		});
		entity.CrawlEntityTimelineDependencies(TriggerType.TT_LIGHT, InstanceType.IT_ANALYTIC_LIGHT, delegate(InstanceType insType, string insName)
		{
			AddEntityToEntityDependency(entityDepotRootedPath, insType, LayeredPantry.GetPantryPath(insName, insType), instSet, workspaceDependencies, crawlOptions, ref localProcCount);
		});
		entity.CrawlAttachmentDependencies(ProjectConfigService.Config, delegate(InstanceType insType, string insName)
		{
			AddEntityToEntityDependency(entityDepotRootedPath, insType, LayeredPantry.GetPantryPath(insName, insType), instSet, workspaceDependencies, crawlOptions, ref localProcCount);
		});
		if (entity is IAssetInstance asset)
		{
			asset.CrawlSplineDependencies(ProjectConfigService.Config, delegate(InstanceType insType, string insName)
			{
				AddEntityToEntityDependency(entityDepotRootedPath, insType, LayeredPantry.GetPantryPath(insName, insType), instSet, workspaceDependencies, crawlOptions, ref localProcCount);
			});
		}
		dependencyCount += localProcCount;
	}

	protected void AddEntityStatsAndTags(IInstanceEntity entity, IDatabaseDependencies workspaceDependencies, string depotRootedPath)
	{
		BugSubmitter.SilentAssert(workspaceDependencies.Files.ContainsKey(depotRootedPath), "Tried to add entity stats and tags to a non-existent entity at path \"{0}\" while in project \"{1}\" @summary Tried to add entity stats and tags to a non-existent entity @assign bwhitman", depotRootedPath, TargetProject);
		if (workspaceDependencies.Files.TryGetValue(depotRootedPath, out var info))
		{
			if (info.Stats == null)
			{
				info.Stats = new ConcurrentDictionary<string, int>();
			}
			if (info.Tags == null)
			{
				info.Tags = new List<string>();
			}
			entity.PublishStats(info.Stats);
			info.Tags = new List<string>(entity.Tags);
			workspaceDependencies.Files.AddOrUpdate(depotRootedPath, info);
		}
	}

	private void AddEntityDataFiles(IInstanceEntity entity, string entityDepotRootedPath, IDatabaseDependencies workspaceDependencies, ref int dependencyCount)
	{
		IEnumerable<Uri> dataFileURIs = entity.GetDataFileURIs();
		foreach (Uri item in dataFileURIs)
		{
			BugSubmitter.SilentAssert(Path.GetDirectoryName(item.LocalPath) == Path.GetDirectoryName(entity.GetXMLPath()), "Failed to AddEntityDataFiles for entity \"{0}\" of type \"{1}\".\n\n\tData file: \"{2}\"\n\tEntity File: \"{3}\" @summary Data file path and entity path are not the same during AddEntityDataFiles @assign bwhitman", entity.Name, entity.Type, item.LocalPath, entity.GetXMLPath());
			string depotRootedPath = GetDepotRootedPath(item.LocalPath);
			dependencyCount++;
			if (!workspaceDependencies.Files.ContainsKey(depotRootedPath))
			{
				AddRootFileIfNew(item.LocalPath, FileType.DataFile, FileStatus.Unknown, workspaceDependencies);
			}
			workspaceDependencies.Dependencies.AddIfUnique(entityDepotRootedPath, depotRootedPath);
		}
	}

	private void AddEntitySourceFiles(IInstanceEntity entity, string entityDepotRootedPath, IDatabaseDependencies workspaceDependencies, ref int dependencyCount)
	{
		if (entity is IImportedEntity importedEntity && importedEntity.SourceFilePath.Length != 0)
		{
			dependencyCount++;
			string depotPath = string.Empty;
			string localPath = string.Empty;
			bool paths = GetPaths(importedEntity.SourceFilePath, out localPath, out depotPath);
			BugSubmitter.SilentAssert(paths, "Failed to AddEntitySourceFiles for entity \"{0}\" of type \"{1}\".\n\n\tSource file: \"{2}\"\n\tLocalPath: \"{3}\"\n\tDepotPath: \"{4}\" @summary Failed to get local and depot paths during AddEntitySourceFiles @assign bwhitman", entity.Name, entity.Type, importedEntity.SourceFilePath, localPath, depotPath);
			if (!workspaceDependencies.Files.ContainsKey(depotPath))
			{
				AddRootFileIfNew(localPath, FileType.SourceFile, FileStatus.Unknown, workspaceDependencies);
			}
			workspaceDependencies.Dependencies.AddIfUnique(entityDepotRootedPath, depotPath);
		}
	}

	private void ProcessXLPDependencies(string xlpPath, string depotRootedXLPPath, IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions, ref int dependencyCount)
	{
		if (string.IsNullOrEmpty(xlpPath))
		{
			return;
		}
		int locDepCnt = ++dependencyCount;
		float num = TimedOperation.Do(delegate
		{
			Uri path = new Uri(xlpPath);
			IXLP xlp = LoadXLP(path);
			try
			{
				if (xlp == null)
				{
					BugSubmitter.SilentReport($"Failed to deserialize XLP in ProcessXLPDependencies for XLP \"{xlpPath}\" with active project \"{TargetProject}\" @summary Failed to deserialize XLP in ProcessXLPDependencies @assign bwhitman");
					AddRootFileIfNew(xlpPath, FileType.XLP, FileStatus.FailedToLoad, workspaceDependencies);
				}
				else
				{
					AddRootFileIfNew(xlpPath, FileType.XLP, FileStatus.Normal, workspaceDependencies);
					IXLPClass iXLPClass = ProjectConfigService.Config.XLPClasses.Items.FirstOrDefault((IXLPClass xc) => xc.Name == xlp.ClassName);
					if (iXLPClass != null)
					{
						using (new ScopedStopwatch(TargetProject + ": Processing XLP \"" + depotRootedXLPPath + "\" containing " + xlp.XLPEntries.Count + " entries tooks {0} seconds", delegate(string str)
						{
							Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
						}))
						{
							foreach (IXLPEntry xLPEntry in xlp.XLPEntries)
							{
								AddXLPEntryDependencies(xLPEntry, iXLPClass, depotRootedXLPPath, instSet, workspaceDependencies, crawlOptions, ref locDepCnt);
							}
							return;
						}
					}
				}
			}
			finally
			{
				if (xlp != null)
				{
					xlp.Dispose();
				}
			}
		});
		dependencyCount = locDepCnt;
		Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "Processing \"{0}\" took {1} secs", xlpPath, num);
	}

	private void AddXLPEntryDependencies(IXLPEntry xlpEnt, IXLPClass xlpCls, string depotRootedXLPPath, IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions, ref int dependencyCount)
	{
		string pantryPath = LayeredPantry.GetPantryPath(xlpEnt.ObjectName, xlpCls.InstanceType);
		if (string.IsNullOrEmpty(pantryPath))
		{
			EntityID entityID = new EntityID(xlpEnt.ObjectName, xlpCls.InstanceType);
			Outputs.WriteLine(OutputMessageType.Error, OutputMessageVerbosity.Verbose, "Unable to load entity {0}.", entityID.ToString());
			return;
		}
		string depotRootedPath = GetDepotRootedPath(pantryPath);
		bool flag = crawlOptions.HasFlag(ContainerCrawlOptions.ChaseDependenciesForExistingFiles);
		if (!workspaceDependencies.Files.ContainsKey(depotRootedPath) && crawlOptions.HasFlag(ContainerCrawlOptions.AddToFileCatalog))
		{
			flag |= AddRootFileIfNew(pantryPath, xlpCls.InstanceType, string.Empty, FileStatus.Unknown, workspaceDependencies);
		}
		if (crawlOptions.HasFlag(ContainerCrawlOptions.AddToDependencyCatalog))
		{
			workspaceDependencies.Dependencies.AddIfUnique(depotRootedXLPPath, depotRootedPath);
		}
		if (flag)
		{
			ProcessEntityDependencies(xlpCls.InstanceType, pantryPath, depotRootedPath, instSet, workspaceDependencies, crawlOptions, ref dependencyCount);
		}
	}

	private DepotFileInfo CreateFileInfo(string filePath, FileType ft, FileStatus fs, InstanceType it, string entClass)
	{
		string depotPath = string.Empty;
		string localPath = filePath;
		GetPaths(filePath, out localPath, out depotPath);
		DepotFileInfo result = new DepotFileInfo
		{
			Status = (int)fs,
			Filename = (string.IsNullOrEmpty(depotPath) ? localPath : depotPath),
			Type = (int)ft,
			EntityType = (int)it,
			EntityClass = entClass,
			Tags = new List<string>(),
			Stats = new Dictionary<string, int>()
		};
		if (result.Status == 5)
		{
			result.Status = ((!File.Exists(localPath)) ? 1 : 0);
		}
		if (result.Status == 0)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(localPath);
				result.Filesize = fileInfo.Length;
				result.Timestamp = fileInfo.LastWriteTimeUtc.ToFileTimeUtc();
			}
			catch (IOException)
			{
				result.Filesize = 0L;
				result.Timestamp = 0L;
			}
		}
		else
		{
			result.Filesize = 0L;
			result.Timestamp = 0L;
		}
		return result;
	}

	private Action<IValue> CreateValueVisitor(Uri fullArtDefPath, string depotRootedArtDefPath, IInstanceSet instSet, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions)
	{
		Action<IValue> valueVisitor = null;
		valueVisitor = delegate(IValue value)
		{
			ICollectionValue collectionValue = value as ICollectionValue;
			IBLPEntryValue iBLPEntryValue = value as IBLPEntryValue;
			IArtDefRefValue artDefRefValue = value as IArtDefRefValue;
			if (collectionValue != null)
			{
				VisitCollectionValue(collectionValue, valueVisitor);
			}
			else if (iBLPEntryValue != null)
			{
				VisitBLPEntryValue(fullArtDefPath, depotRootedArtDefPath, instSet, iBLPEntryValue, workspaceDependencies, crawlOptions);
			}
			else if (artDefRefValue != null)
			{
				VisitArtDefRefValue(fullArtDefPath, depotRootedArtDefPath, instSet, artDefRefValue, workspaceDependencies, crawlOptions);
			}
		};
		return valueVisitor;
	}

	private static bool DateTimeEqual(DateTime a, DateTime b)
	{
		return a.Year == b.Year && a.Month == b.Month && a.Day == b.Day && a.Hour == b.Hour && a.Minute == b.Minute && a.Second == b.Second;
	}

	private string EnsureTrailingSlash(string inPath)
	{
		char c = inPath[inPath.Length - 1];
		if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
		{
			return inPath;
		}
		return inPath + Path.DirectorySeparatorChar;
	}

	private ResultCode GetLocalPathFromPerforcePath(string perforcePath, out string filePath)
	{
		ProjectEnvironment project = null;
		ResultCode projectFromDepot = ProjectMapService.GetProjectFromDepot(perforcePath, ref project);
		if (!projectFromDepot)
		{
			filePath = perforcePath;
			return projectFromDepot;
		}
		filePath = project.VersionControl.GetLocalPath(perforcePath);
		return ResultCode.Success;
	}

	public virtual string GetWorkspaceRootedPath(string filePath)
	{
		string text = filePath.TrimStart(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		if (string.IsNullOrEmpty(m_workspaceRoot))
		{
			return filePath;
		}
		if (!PathCompareHelper.StartsWith(text, m_workspaceRoot, bIgnoreCase: true))
		{
			return filePath;
		}
		return text.Substring(m_workspaceRoot.Length).TrimStart(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
	}

	private bool IsArtDef(string filePath)
	{
		return PathCompareHelper.EndsWith(filePath, ".artdef", bIgnoreCase: true) && PathCompareHelper.StartsWith(filePath, ArtDefRoot, bIgnoreCase: true);
	}

	private bool IsDataFile(string filePath)
	{
		if (!BeginsWithPantryRoot(filePath))
		{
			return false;
		}
		string ext = Path.GetExtension(filePath);
		return ProjectConfigService.Config.Classes.Items.Any((IClassEntity classEntity) => classEntity.DataFiles.Any((IClassDataFile dataFile) => dataFile.Extension.Equals(ext, StringComparison.CurrentCultureIgnoreCase)));
	}

	private bool IsDepotPath(string filePath)
	{
		return !string.IsNullOrEmpty(filePath) && filePath.Length >= 2 && filePath[0] == '/' && filePath[1] == '/';
	}

	private bool IsEntity(string filePath)
	{
		InstanceType type;
		return BeginsWithPantryRoot(filePath) && StaticMethods.GetInstanceType(filePath, out type);
	}

	private bool IsSourceFile(string filePath)
	{
		string extension = Path.GetExtension(filePath);
		return m_srcExts.Contains(extension, StringComparer.CurrentCultureIgnoreCase);
	}

	private bool IsXLP(string filePath)
	{
		return PathCompareHelper.EndsWith(filePath, ".xlp", bIgnoreCase: true) && PathCompareHelper.StartsWith(filePath, XLPRoot, bIgnoreCase: true);
	}

	private T CreateThreadSafe<T>(params object[] ctorParams) where T : IAssemblyInstance
	{
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>();
		lock (civTechContext)
		{
			return civTechContext.CreateInstance<T>(ctorParams);
		}
	}

	private bool LoadThreadSafe<T>(Uri fileUri, T fileObj) where T : ISerializable, IAssemblyInstance
	{
		lock ((object)fileObj)
		{
			string localPath = fileUri.LocalPath;
			if (!fileObj.DeserializeFromFile(localPath))
			{
				fileObj.Dispose();
				return false;
			}
		}
		return true;
	}

	private IArtDef LoadArtDef(Uri path, IProjectConfig projCfg)
	{
		IArtDef artDef = CreateThreadSafe<IArtDef>(new object[1] { projCfg });
		if (!LoadThreadSafe(path, artDef))
		{
			return null;
		}
		return artDef;
	}

	private IXLP LoadXLP(Uri path)
	{
		IXLP iXLP = CreateThreadSafe<IXLP>(new object[0]);
		if (!LoadThreadSafe(path, iXLP))
		{
			return null;
		}
		return iXLP;
	}

	private void VerifyDepotFile(IDatabaseDependencies database, string depotFilePath, IInstanceSet temporaryEntitySet, ref DepotFileInfo depotFile)
	{
		string fullPath = GetFullPath(depotFilePath);
		if (string.IsNullOrEmpty(fullPath))
		{
			return;
		}
		try
		{
			string localPath = new Uri(fullPath).LocalPath;
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, OutputMessageVerbosity.Verbose, "Invalid URI for fullPath \"{0}\" generated from depotPath \"{1}\". Error=\"{2}\"", fullPath, depotFilePath, ex.Message);
			return;
		}
		DateTime a = DateTime.FromFileTimeUtc(depotFile.Timestamp);
		DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(fullPath);
		if (!DateTimeEqual(a, lastWriteTimeUtc))
		{
			long timestamp = lastWriteTimeUtc.ToFileTimeUtc();
			depotFile.Timestamp = timestamp;
		}
		if (depotFile.Type != 2)
		{
			return;
		}
		EntityID entityIDFromPath = StaticMethods.GetEntityIDFromPath(ProjectMapService, fullPath);
		if (StaticMethods.GetInstanceType(fullPath, out var type))
		{
			depotFile.EntityType = (int)type;
			if (string.IsNullOrEmpty(depotFile.EntityClass))
			{
				IInstanceEntity instanceEntity = temporaryEntitySet.LoadEntityIfUnique(entityIDFromPath.Name, type);
				if (instanceEntity != null)
				{
					depotFile.EntityClass = instanceEntity.ClassName;
				}
			}
		}
		else
		{
			FileType fileType = GetFileType(fullPath);
			depotFile.Type = (int)fileType;
		}
	}

	private void VisitArtDefRefValue(Uri fullArtDefPath, string depotRootedArtDefPath, IInstanceSet instSet, IArtDefRefValue artDefRefValue, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions)
	{
		if (string.IsNullOrEmpty(artDefRefValue.ArtDefPath) || string.IsNullOrEmpty(artDefRefValue.ElementName))
		{
			return;
		}
		string artDefPantryPath = LayeredPantry.GetArtDefPantryPath(artDefRefValue.ArtDefPath);
		if (string.IsNullOrEmpty(artDefPantryPath))
		{
			BugSubmitter.SilentReport($"Failed to get fully qualified path for ArtDefRef \"{artDefRefValue.ParameterName}\" in \"{depotRootedArtDefPath}\" for project {TargetProject} pointing to \"{artDefRefValue.ArtDefPath}\":\"{artDefRefValue.ElementName}\" @summary failed to process dependencies for ArtDefRef @assign bwhitman");
			return;
		}
		if (!artDefPantryPath.EndsWith(".artdef", StringComparison.CurrentCultureIgnoreCase))
		{
			BugSubmitter.SilentReport($"Failed to get valid fully qualified path for ArtDefRef \"{artDefRefValue.ParameterName}\" in \"{depotRootedArtDefPath}\" for project {TargetProject} pointing to \"{artDefRefValue.ArtDefPath}\":\"{artDefRefValue.ElementName}\", Actual computed path was \"{artDefPantryPath}\" @summary failed to process dependencies for ArtDefRef @assign bwhitman");
			return;
		}
		string depotRootedPath = GetDepotRootedPath(artDefPantryPath);
		if (string.IsNullOrEmpty(depotRootedPath))
		{
			BugSubmitter.SilentReport($"Failed to get depot rooted path from \"{artDefPantryPath}\" for ArtDefRef \"{artDefRefValue.ParameterName}\" in \"{depotRootedArtDefPath}\" for project {TargetProject} pointing to \"{artDefRefValue.ArtDefPath}\":\"{artDefRefValue.ElementName}\" @summary failed to process dependencies for ArtDefRef @assign bwhitman");
		}
		else if (!(depotRootedArtDefPath == depotRootedPath))
		{
			if (!workspaceDependencies.Files.ContainsKey(depotRootedPath) && ProjectMapService.IsFromActiveProject(fullArtDefPath))
			{
				ProcessArtDefDependencies(artDefPantryPath, depotRootedPath, instSet, workspaceDependencies, crawlOptions);
			}
			if (crawlOptions.HasFlag(ContainerCrawlOptions.AddToDependencyCatalog))
			{
				workspaceDependencies.Dependencies.AddIfUnique(depotRootedArtDefPath, depotRootedPath);
			}
		}
	}

	private void VisitBLPEntryValue(Uri fullArtDefPath, string depotRootedArtDefPath, IInstanceSet instSet, IBLPEntryValue blpEntryValue, IDatabaseDependencies workspaceDependencies, ContainerCrawlOptions crawlOptions)
	{
		if (string.IsNullOrEmpty(blpEntryValue.EntryName) || string.IsNullOrEmpty(blpEntryValue.XLPPath))
		{
			return;
		}
		string xLPPantryPath = LayeredPantry.GetXLPPantryPath(blpEntryValue.XLPPath);
		string depotRootedPath = GetDepotRootedPath(xLPPantryPath);
		if (string.IsNullOrEmpty(xLPPantryPath))
		{
			Outputs.WriteLine(OutputMessageType.Error, "In ArtDef \"{0}\" failed to find BLP reference \"{1}\" in the layered pantry", fullArtDefPath.LocalPath, blpEntryValue.XLPPath);
			return;
		}
		if (string.IsNullOrEmpty(depotRootedPath))
		{
			Outputs.WriteLine(OutputMessageType.Error, "In ArtDef \"{0}\" failed to determine workspace root for BLP reference \"{1}\" with a full path of \"{2}\"", fullArtDefPath.LocalPath, blpEntryValue.XLPPath, xLPPantryPath);
			return;
		}
		int dependencyCount = 0;
		if (!workspaceDependencies.Files.ContainsKey(depotRootedPath) && ProjectMapService.IsFromActiveProject(fullArtDefPath))
		{
			AddXLPDependencies(xLPPantryPath, instSet, workspaceDependencies, crawlOptions, ref dependencyCount);
		}
		if (crawlOptions.HasFlag(ContainerCrawlOptions.AddToDependencyCatalog))
		{
			workspaceDependencies.Dependencies.AddIfUnique(depotRootedArtDefPath, depotRootedPath);
		}
	}

	private void VisitCollectionValue(ICollectionValue collectionValue, Action<IValue> valueVisitor)
	{
		foreach (IValue item in collectionValue.Items)
		{
			valueVisitor(item);
		}
	}
}

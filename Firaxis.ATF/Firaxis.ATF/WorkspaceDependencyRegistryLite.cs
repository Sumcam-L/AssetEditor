using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DatabaseWrapper;
using Firaxis.CivTech;
using Firaxis.Threading;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.ATF;

public class WorkspaceDependencyRegistryLite : IWorkspaceDependencyRegistry
{
	private ReaderWriterLockSlim m_depedencyRegistryLock = new ReaderWriterLockSlim();

	private IDatabaseDependencies m_dependencies = new DatabaseDependencies();

	private string m_workspaceRoot;

	private IList<string> m_depotRoots = new List<string>();

	private IList<string> m_pantryRoots = new List<string>();

	private ProjectEnvironment m_projectEnvironment;

	private IProjectMapService m_projectMapService;

	private IProjectConfigService m_projectConfigService;

	private IVersionControlService m_versionControlService;

	private WorkspaceDependencyBase m_dependencyUpdater;

	private string TargetProject { get; set; } = "Uninitialized";

	public WorkspaceDependencyRegistryLite(IProjectMapService projMapSvc, IProjectConfigService projCfgSvc, IVersionControlService verCtlSvc, WorkspaceDependencyBase depUpdater)
	{
		m_projectMapService = projMapSvc;
		m_projectConfigService = projCfgSvc;
		m_versionControlService = verCtlSvc;
		m_dependencyUpdater = depUpdater;
	}

	public void Initialize(string targetProject)
	{
		TargetProject = targetProject;
		m_projectEnvironment = m_projectMapService.AllProjectsMap[targetProject];
		m_workspaceRoot = EnsureTrailingSlash(m_projectEnvironment.VersionControl.WorkspaceRoot).ToLower().Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		InitializeDependencyInfo(targetProject);
		SetupPantryRoots();
	}

	public DateTime GetLastChangeTime(Uri fileUri)
	{
		ISet<string> seenPaths = new HashSet<string>(new PathComparer());
		return GetLastChangeTimeImpl(fileUri, seenPaths);
	}

	public DependencyTree GetDependentTree(Uri item)
	{
		return GetDependentTree(item, null);
	}

	public bool DependsOn(Uri entityThatDependsOn, Uri entityThatIsDependedOn)
	{
		using (new ScopedReaderLock(m_depedencyRegistryLock))
		{
			return LocklessGetUris(entityThatDependsOn, m_dependencies.Dependencies).Any((Uri u) => PathCompareHelper.Equals(u.LocalPath, entityThatIsDependedOn.LocalPath, bIgnoreCase: true));
		}
	}

	public IEnumerable<Uri> GetDependencies(Uri fileUri)
	{
		using (new ScopedReaderLock(m_depedencyRegistryLock))
		{
			return LocklessGetUris(fileUri, m_dependencies.Dependencies);
		}
	}

	public IEnumerable<Uri> GetDependents(Uri fileUri)
	{
		using (new ScopedReaderLock(m_depedencyRegistryLock))
		{
			return LocklessGetUris(fileUri, m_dependencies.Dependants);
		}
	}

	public bool HasFile(Uri item)
	{
		using (new ScopedReaderLock(m_depedencyRegistryLock))
		{
			string depotRootedPath = GetDepotRootedPath(item.LocalPath);
			return m_dependencies.Files.ContainsKey(depotRootedPath);
		}
	}

	public IEnumerable<Uri> GetFiles()
	{
		IList<Uri> uris = new List<Uri>();
		using (new ScopedReaderLock(m_depedencyRegistryLock))
		{
			m_dependencies.Files.ForEachValue(delegate(DepotFileInfo fileInfo)
			{
				if (!string.IsNullOrEmpty(fileInfo.Filename) && Uri.TryCreate(m_dependencyUpdater.GetFullPath(fileInfo.Filename), UriKind.Absolute, out var result))
				{
					uris.Add(result);
				}
			});
		}
		return uris;
	}

	public FileType GetFileType(Uri item)
	{
		using (new ScopedReaderLock(m_depedencyRegistryLock))
		{
			return m_dependencyUpdater.GetFileType(m_dependencies, item);
		}
	}

	public bool GetFileInfo(Uri item, ref DepotFileInfo info)
	{
		using (new ScopedReaderLock(m_depedencyRegistryLock))
		{
			return m_dependencyUpdater.GetFileInfo(m_dependencies, item, ref info);
		}
	}

	public string GetDepotRootedPath(string filePath)
	{
		string text = filePath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		string text2 = text.ToLower();
		for (int i = 0; i < m_pantryRoots.Count; i++)
		{
			string value = m_pantryRoots[i];
			if (text2.Contains(value))
			{
				text = text2.Replace(m_depotRoots[i], "");
				return filePath.Substring(filePath.Length - text.Length, text.Length).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			}
		}
		return GetWorkspaceRootedPath(text);
	}

	public string GetWorkspaceRootedPath(string filePath)
	{
		if (string.IsNullOrEmpty(m_workspaceRoot))
		{
			return filePath;
		}
		string text = filePath.ToLower().Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).TrimStart(Path.AltDirectorySeparatorChar);
		if (!text.Contains(m_workspaceRoot))
		{
			return filePath;
		}
		return text.Replace(m_workspaceRoot, "").TrimStart(Path.AltDirectorySeparatorChar);
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

	private void FillDependentTree(DependencyTree tree)
	{
		List<DependencyTree> list = new List<DependencyTree>();
		foreach (Uri item in (IEnumerable<Uri>)GetDependents(tree.Root).OrderBy((Uri u) => u.LocalPath, PathComparer.PathCompareIgnoreCase).ToArray())
		{
			bool flag = false;
			DependencyTree parent = tree.Parent;
			while (!flag && parent != null)
			{
				flag = PathCompareHelper.Equals(parent.Root.LocalPath, item.LocalPath, bIgnoreCase: true);
				parent = parent.Parent;
			}
			if (!flag)
			{
				DependencyTree dependentTree = GetDependentTree(item, tree);
				list.Add(dependentTree);
			}
		}
		tree.Dependents = list;
	}

	private DependencyTree GetDependentTree(Uri item, DependencyTree parent)
	{
		DependencyTree dependencyTree = new DependencyTree(item);
		dependencyTree.Parent = parent;
		FillDependentTree(dependencyTree);
		return dependencyTree;
	}

	private DateTime GetFileChangeTime(Uri fileUri)
	{
		DateTime result = DateTime.MinValue.ToUniversalTime();
		try
		{
			result = new FileInfo(fileUri.LocalPath).LastWriteTimeUtc;
		}
		catch
		{
		}
		return result;
	}

	private DateTime GetLastChangeTimeImpl(Uri fileUri, ISet<string> seenPaths)
	{
		DateTime dateTime = DateTime.MinValue.ToUniversalTime();
		string localPath = fileUri.LocalPath;
		if (seenPaths.Add(localPath))
		{
			dateTime = GetFileChangeTime(fileUri);
			foreach (Uri dependency in GetDependencies(fileUri))
			{
				DateTime lastChangeTimeImpl = GetLastChangeTimeImpl(dependency, seenPaths);
				if (lastChangeTimeImpl > dateTime)
				{
					dateTime = lastChangeTimeImpl;
				}
			}
		}
		return dateTime;
	}

	private IEnumerable<Uri> LocklessGetUris(Uri fileUri, IDependencyCatalog adjacencyDictionary)
	{
		IList<Uri> list = new List<Uri>();
		string localPath = fileUri.LocalPath;
		string depotRootedPath = GetDepotRootedPath(localPath);
		if (adjacencyDictionary.TryGetValue(depotRootedPath, out var children))
		{
			string workspaceRoot = m_projectEnvironment.VersionControl.WorkspaceRoot;
			BugSubmitter.SilentAssert(Path.IsPathRooted(workspaceRoot), "Depot path not rooted in project {0} while getting dependencies for \"{1}\" with a depot root of \"{2}\" @summary Depot path not rooted for dependency @assign bwhitman", TargetProject, fileUri.LocalPath, workspaceRoot);
			string[] array = children;
			foreach (string text in array)
			{
				if (!string.IsNullOrEmpty(text))
				{
					if (Uri.TryCreate(Path.Combine(workspaceRoot, text), UriKind.Absolute, out var result))
					{
						list.Add(result);
						continue;
					}
					BugSubmitter.SilentReport("Failed to create Uri for path \"" + text + "\" in project " + TargetProject + " while getting dependencies for \"" + fileUri.LocalPath + "\" with a depot root of \"" + workspaceRoot + "\" @summary Failed to create Uri for dependency @assign bwhitman");
				}
			}
		}
		return list;
	}

	private void InitializeDependencyInfo(string targetProject)
	{
		using (new ScopedWriterLock(m_depedencyRegistryLock))
		{
			using (new ScopedStopwatch(targetProject + ": Initializing dependency information took {0} seconds", delegate(string str)
			{
				Outputs.WriteLine(OutputMessageType.Info, str);
			}))
			{
				m_dependencies = new DatabaseDependencies();
				if (m_dependencyUpdater.UpdateDependencies(m_dependencies))
				{
					IList<DepotFileInfo> badEntities = new List<DepotFileInfo>();
					m_dependencies.Files.ForEachValue((DepotFileInfo predicate) => predicate.EntityType == 2 && predicate.Timestamp == 0, delegate(DepotFileInfo visitor)
					{
						badEntities.Add(visitor);
					});
					ReportIncorrectProjectFiles(badEntities);
					m_dependencies.GenerateDependants();
				}
				else
				{
					Outputs.WriteLine(OutputMessageType.Error, OutputMessageVerbosity.Verbose, "Failed to load dependency information");
				}
			}
		}
	}

	private void ReportIncorrectProjectFiles(IEnumerable<DepotFileInfo> badEntities)
	{
		if (badEntities.Any())
		{
			Outputs.WriteLine(OutputMessageType.Error, OutputMessageVerbosity.Verbose, "Incorrect project attribution:");
			badEntities.ForEach(delegate(DepotFileInfo file)
			{
				Outputs.WriteLine(OutputMessageType.Error, OutputMessageVerbosity.Verbose, "     {0}", file.Filename);
			});
		}
	}

	private void SetupPantryRoots()
	{
		foreach (ProjectEnvironment projectAndDependency in m_projectMapService.GetProjectAndDependencies(m_projectEnvironment))
		{
			m_depotRoots.Add(EnsureTrailingSlash(projectAndDependency.VersionControl.WorkspaceRoot).ToLower().Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			m_pantryRoots.Add(EnsureTrailingSlash(projectAndDependency.Paths.GamePantry).ToLower().Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		}
	}
}

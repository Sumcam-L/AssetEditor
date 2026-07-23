using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using DatabaseWrapper;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Properties;
using Firaxis.Threading;
using Firaxis.Utility;
using Microsoft.Win32;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public class WorkspaceDependencyRegistry : IWorkspaceDependencyRegistry, IWorkspaceDependencyWatcher, IDisposable, IWorkspaceMapper
{
	private enum ChangeType
	{
		Created,
		Destroyed,
		Changed,
		Renamed
	}

	private class FileWatchChangedOperation
	{
		public readonly Uri FilePath;

		public readonly ChangeType Type;

		public readonly FileSystemEventArgs EventArgs;

		public FileWatchChangedOperation(string filePath, ChangeType type, FileSystemEventArgs eventArgs)
		{
			FilePath = new Uri(filePath);
			Type = type;
			EventArgs = eventArgs;
		}
	}

	private class WorkspaceItemChangedQueueItem
	{
		public readonly WorkspaceItemChangedEvent ChangedEvent;

		public readonly ChangeType Type;

		public WorkspaceItemChangedQueueItem(WorkspaceItemChangedEvent changedEvent, ChangeType type)
		{
			ChangedEvent = changedEvent;
			Type = type;
		}
	}

	private IQuietTimeAction m_changeSaver;

	private IQuietTimeAction m_dependencyUpdateKicker;

	private ReaderWriterLockSlim m_depedencyRegistryLock = new ReaderWriterLockSlim();

	private IDatabaseDependencies m_dependencies = new DatabaseDependencies();

	private WorkspaceDependencyUpdater m_dependencyUpdater;

	private IList<string> m_depotRoots = new List<string>();

	private bool m_isRunning = true;

	private IList<string> m_pantryRoots = new List<string>();

	private ProjectEnvironment m_projectEnvironment;

	private IProjectMapService m_projectMapService;

	private IProjectConfigService m_projectConfigService;

	private IVersionControlService m_versionControlService;

	private bool m_requiresDependencyUpdate;

	private ICollection<FileSystemWatcher> m_watchers = new List<FileSystemWatcher>();

	private string m_workspaceRoot;

	private IMainForm m_mainWindow;

	private readonly ConcurrentQueue<FileWatchChangedOperation> m_pendingChanges = new ConcurrentQueue<FileWatchChangedOperation>();

	private readonly ConcurrentQueue<FileWatchChangedOperation> m_processingFileQueue = new ConcurrentQueue<FileWatchChangedOperation>();

	private readonly Queue<WorkspaceItemChangedQueueItem> m_queuedEvents = new Queue<WorkspaceItemChangedQueueItem>();

	private readonly Thread m_workspaceChangeHandlerThread;

	private readonly AutoResetEvent m_workspaceThreadSignal = new AutoResetEvent(initialState: false);

	private volatile bool m_disposed;

	private string TargetProject { get; set; } = "Uninitialized";

	public event EventHandler<WorkspaceItemChangedEvent> WorkspaceItemChanged;

	public event EventHandler<WorkspaceItemRenamedEvent> WorkspaceItemRenamed;

	public event EventHandler<WorkspaceItemChangedEvent> WorkspaceItemRemoved;

	public event EventHandler<WorkspaceItemChangedEvent> WorkspaceItemAdded;

	public WorkspaceDependencyRegistry(IMainForm mainWindow, IProjectMapService projMapSvc, IProjectConfigService projCfgSvc, IVersionControlService verCtlSvc)
	{
		m_mainWindow = mainWindow;
		m_projectMapService = projMapSvc;
		m_projectConfigService = projCfgSvc;
		m_versionControlService = verCtlSvc;
		m_workspaceChangeHandlerThread = new Thread(ProcessWorkspaceChanges);
		m_workspaceChangeHandlerThread.Name = "Workspace Watcher Thread";
		m_workspaceChangeHandlerThread.IsBackground = true;
		m_workspaceChangeHandlerThread.Start();
	}

	public void Initialize(string targetProject)
	{
		TargetProject = targetProject;
		m_projectEnvironment = m_projectMapService.AllProjectsMap[targetProject];
		m_workspaceRoot = EnsureTrailingSlash(m_projectEnvironment.VersionControl.WorkspaceRoot).ToLower().Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		InitializeDependencyInfo(targetProject);
		PaintTimingLog.Write("Startup: deps-init depinfo-done project={0}", targetProject);
		SetupPantryRoots();
		SetupFileWatchers();
		PaintTimingLog.Write("Startup: deps-init watchers-done project={0}", targetProject);
	}

	public virtual void DisableFileWatches()
	{
		foreach (FileSystemWatcher watcher in m_watchers)
		{
			watcher.EnableRaisingEvents = false;
		}
	}

	public virtual void EnableFileWatches()
	{
		foreach (FileSystemWatcher watcher in m_watchers)
		{
			watcher.EnableRaisingEvents = true;
		}
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

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_disposed)
		{
			return;
		}
		m_disposed = true;
		if (!disposing)
		{
			return;
		}
		m_isRunning = false;
		foreach (FileSystemWatcher watcher in m_watchers)
		{
			watcher.EnableRaisingEvents = false;
			watcher.Changed -= HandleWorkspaceItemChanged;
			watcher.Renamed -= HandleWorkspaceItemRenamed;
			watcher.Created -= HandleWorkspaceItemCreated;
			watcher.Deleted -= HandleWorkspaceItemDeleted;
			watcher.Dispose();
		}
		m_watchers.Clear();
		m_workspaceThreadSignal.Set();
		m_workspaceChangeHandlerThread.Join(1);
		m_workspaceThreadSignal.Dispose();
		m_changeSaver.Dispose();
		m_changeSaver = null;
		m_dependencyUpdateKicker.Dispose();
		m_dependencyUpdateKicker = null;
	}

	public virtual string GetDepotRootedPath(string filePath)
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

	public virtual string GetWorkspaceRootedPath(string filePath)
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

	protected virtual void OnWorkspaceItemAdded(WorkspaceItemChangedEvent addedEvent)
	{
		this.WorkspaceItemAdded?.Invoke(this, addedEvent);
	}

	protected virtual void OnWorkspaceItemChanged(WorkspaceItemChangedEvent changedEvent)
	{
		this.WorkspaceItemChanged?.Invoke(this, changedEvent);
	}

	protected virtual void OnWorkspaceItemRemoved(WorkspaceItemChangedEvent removedEvent)
	{
		this.WorkspaceItemRemoved?.Invoke(this, removedEvent);
	}

	protected virtual void OnWorkspaceItemRenamed(WorkspaceItemRenamedEvent renamedEvent)
	{
		this.WorkspaceItemRenamed?.Invoke(this, renamedEvent);
	}

	private WorkspaceItemChangedQueueItem CreateChangedEvent(FileWatchChangedOperation operation)
	{
		WorkspaceItemChangedEvent workspaceItemChangedEvent = null;
		WatcherChangeTypes changeType = operation.EventArgs.ChangeType;
		if (operation.Type == ChangeType.Renamed)
		{
			if (operation.EventArgs is RenamedEventArgs e)
			{
				workspaceItemChangedEvent = CreateWorkspaceItemRenamedEvent(new Uri(e.OldFullPath), operation.FilePath, changeType);
			}
		}
		else
		{
			workspaceItemChangedEvent = CreateWorkspaceItemChangedEvent(operation.FilePath, changeType);
		}
		if (workspaceItemChangedEvent == null)
		{
			return null;
		}
		return new WorkspaceItemChangedQueueItem(workspaceItemChangedEvent, operation.Type);
	}

	private WorkspaceItemChangedEvent CreateWorkspaceItemChangedEvent(Uri changedUri, WatcherChangeTypes changeType)
	{
		return new WorkspaceItemChangedEvent(changedUri, changeType);
	}

	private WorkspaceItemRenamedEvent CreateWorkspaceItemRenamedEvent(Uri oldID, Uri newID, WatcherChangeTypes changeType)
	{
		return new WorkspaceItemRenamedEvent(oldID, newID, changeType);
	}

	private void EnsureSettingsFolderCreated(string settingsFilePath)
	{
		string directoryName = Path.GetDirectoryName(settingsFilePath);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
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

	private string GetInstalledDependencyInfoPath(string targetProject)
	{
		string path = string.Empty;
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Firaxis\\Tools\\AssetCloudCiv6");
		if (registryKey != null)
		{
			object value = registryKey.GetValue("DependencyInfo");
			if (value is string)
			{
				path = (string)value;
			}
		}
		return Path.Combine(path, targetProject + "-asset-deps.json");
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

	private string GetUpdatedDepedencyInfoPath(string targetProject)
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		if (!Firaxis.CivTech.Properties.Resources.ModTools)
		{
			return folderPath + "\\AssetCloud\\" + targetProject + "-asset-deps.json";
		}
		return folderPath + "\\AssetCloud\\mod-" + targetProject + "-asset-deps.json";
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

	private void FireEvent(WorkspaceItemChangedQueueItem itemChangedEvent)
	{
		switch (itemChangedEvent.Type)
		{
		case ChangeType.Created:
			OnWorkspaceItemAdded(itemChangedEvent.ChangedEvent);
			break;
		case ChangeType.Destroyed:
			OnWorkspaceItemRemoved(itemChangedEvent.ChangedEvent);
			break;
		case ChangeType.Changed:
			OnWorkspaceItemChanged(itemChangedEvent.ChangedEvent);
			break;
		case ChangeType.Renamed:
			OnWorkspaceItemRenamed(itemChangedEvent.ChangedEvent as WorkspaceItemRenamedEvent);
			break;
		default:
			BugSubmitter.SilentReport("Update this switch statement to handle the new case. @assign bwhitman");
			break;
		}
	}

	private void ScheduleDependencyUpdate(FileWatchChangedOperation operation)
	{
		m_pendingChanges.Enqueue(operation);
		m_dependencyUpdateKicker?.UpdateLastChangeTime();
	}

	private void ProcessChangesIfNoSyncInFlight()
	{
		_ = m_dependencyUpdateKicker?.UpdatesSinceLastAction;
		int num = m_pendingChanges.Count;
		int num2 = 0;
		FileWatchChangedOperation result;
		while (num > 0 && m_pendingChanges.TryDequeue(out result))
		{
			m_processingFileQueue.Enqueue(result);
			num2++;
			num--;
		}
		BugSubmitter.SilentAssert(num == 0, "{0}: Failed to drain all pending file watch operations from pending queue, {1} items remaing! @summary Failed to drain all pending file watch operations from pending queue @assign bwhitman", TargetProject, num);
		Outputs.WriteLine(OutputMessageType.Info, "{0}: Local environment has changed, queueing dependency update for {1} environment changes", TargetProject, num2);
		m_workspaceThreadSignal.Set();
	}

	private void HandleWorkspaceItemDeleted(object sender, FileSystemEventArgs e)
	{
		if (!(Path.GetExtension(e.FullPath) == ".tmp"))
		{
			_ = (FileSystemWatcher)sender;
			FileWatchChangedOperation operation = new FileWatchChangedOperation(e.FullPath, ChangeType.Destroyed, e);
			ScheduleDependencyUpdate(operation);
		}
	}

	private void HandleWorkspaceItemCreated(object sender, FileSystemEventArgs e)
	{
		if (!(Path.GetExtension(e.FullPath) == ".tmp"))
		{
			_ = (FileSystemWatcher)sender;
			FileWatchChangedOperation operation = new FileWatchChangedOperation(e.FullPath, ChangeType.Created, e);
			ScheduleDependencyUpdate(operation);
		}
	}

	private void HandleWorkspaceItemRenamed(object sender, RenamedEventArgs e)
	{
		_ = (FileSystemWatcher)sender;
		FileWatchChangedOperation operation = new FileWatchChangedOperation(e.FullPath, ChangeType.Renamed, e);
		ScheduleDependencyUpdate(operation);
	}

	private void HandleWorkspaceItemChanged(object sender, FileSystemEventArgs e)
	{
		if (!(Path.GetExtension(e.FullPath) == ".tmp"))
		{
			_ = (FileSystemWatcher)sender;
			FileWatchChangedOperation operation = new FileWatchChangedOperation(e.FullPath, ChangeType.Changed, e);
			ScheduleDependencyUpdate(operation);
		}
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
				string timingInfoFmt = string.Empty;
				string installedDependencyInfoPath = GetInstalledDependencyInfoPath(targetProject);
				string updatedDepsFiles = GetUpdatedDepedencyInfoPath(targetProject);
				installedDependencyInfoPath = SelectNewestFile(installedDependencyInfoPath, updatedDepsFiles);
				var depTimer = Stopwatch.StartNew();
				Action<string> action = delegate(string message)
				{
					timingInfoFmt = message;
					m_dependencies = new DatabaseDependencies();
					new WorkspaceDependencyBuilder(m_projectMapService, m_projectConfigService, targetProject, "0", testFilesExist: false).UpdateDependencies(m_dependencies);
				};
				m_dependencyUpdater = new WorkspaceDependencyUpdater(m_projectMapService, m_projectConfigService, targetProject);
				bool flag = false;
				if (File.Exists(installedDependencyInfoPath))
				{
					using (new ScopedStopwatch(targetProject + ": Loading dependency information took {0} seconds", delegate(string str)
					{
						Outputs.WriteLine(OutputMessageType.Info, str);
					}))
					{
						flag = m_dependencies.Load(installedDependencyInfoPath);
					}
				}
				PaintTimingLog.Write("Startup: deps-load elapsed={0}ms", depTimer.ElapsedMilliseconds);
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				if (flag)
				{
					timingInfoFmt = targetProject + ": Updating dependency information took {0} seconds";
					try
					{
						m_dependencyUpdater.UpdateDependencies(m_dependencies);
					}
					catch (System.Exception ex)
					{
						Outputs.WriteLine(OutputMessageType.Error, "Updating dependency information from workspace failed\n\nError:\n{0}", ex.Message);
						BugSubmitter.SilentException(ex);
						action(targetProject + ": Update failed.  Fall back to building dependency information took {0} seconds");
					}
					IList<DepotFileInfo> badEntities = new List<DepotFileInfo>();
					m_dependencies.Files.ForEachValue((DepotFileInfo predicate) => predicate.EntityType == 2 && predicate.Timestamp == 0, delegate(DepotFileInfo visitor)
					{
						badEntities.Add(visitor);
					});
					ReportIncorrectProjectFiles(badEntities);
				}
				else
				{
					action(targetProject + ": Building dependency information took {0} seconds");
				}
				Outputs.WriteLine(OutputMessageType.Info, timingInfoFmt, (double)stopwatch.ElapsedMilliseconds / 1000.0);
				PaintTimingLog.Write("Startup: deps-update elapsed={0}ms", depTimer.ElapsedMilliseconds);
				using (new ScopedStopwatch(targetProject + ": Saving dependency information took {0} seconds", delegate(string str)
				{
					Outputs.WriteLine(OutputMessageType.Info, str);
				}))
				{
					EnsureSettingsFolderCreated(updatedDepsFiles);
					m_dependencies.Save(updatedDepsFiles);
				}
				PaintTimingLog.Write("Startup: deps-save elapsed={0}ms", depTimer.ElapsedMilliseconds);
				using (new ScopedStopwatch(targetProject + ": Generating upward dependency information took {0} seconds", delegate(string str)
				{
					Outputs.WriteLine(OutputMessageType.Info, str);
				}))
				{
					m_dependencies.GenerateDependants();
				}
				PaintTimingLog.Write("Startup: deps-dependants elapsed={0}ms", depTimer.ElapsedMilliseconds);
				m_changeSaver = new QuietTimeAction(QuietTimeWaitBehavior.ExponentialBackoff, delegate
				{
					using (new ScopedReaderLock(m_depedencyRegistryLock))
					{
						m_dependencies.Save(updatedDepsFiles);
					}
				}, targetProject + ": Saving updated dependency information to " + updatedDepsFiles + "...");
				m_dependencyUpdateKicker = new QuietTimeAction(QuietTimeWaitBehavior.Adaptive, delegate
				{
					ProcessChangesIfNoSyncInFlight();
				});
			}
		}
	}

	private void ProcessFileWatchOperation(FileWatchChangedOperation operation)
	{
		WorkspaceItemChangedQueueItem workspaceItemChangedQueueItem = CreateChangedEvent(operation);
		if (workspaceItemChangedQueueItem != null)
		{
			m_queuedEvents.Enqueue(workspaceItemChangedQueueItem);
		}
		switch (operation.Type)
		{
		case ChangeType.Created:
			m_dependencyUpdater.AddDependency(operation.FilePath, m_dependencies);
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.ExtremelyVerbose, "Added {0} dependency information", operation.FilePath.LocalPath);
			break;
		case ChangeType.Destroyed:
			m_dependencyUpdater.RemoveDependency(operation.FilePath, m_dependencies);
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.ExtremelyVerbose, "Removed {0} dependency information", operation.FilePath.LocalPath);
			break;
		case ChangeType.Changed:
			m_dependencyUpdater.RemoveDependency(operation.FilePath, m_dependencies);
			m_dependencyUpdater.AddDependency(operation.FilePath, m_dependencies);
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.ExtremelyVerbose, "Updated {0} dependency information", operation.FilePath.LocalPath);
			break;
		case ChangeType.Renamed:
		{
			Uri uri = new Uri((operation.EventArgs as RenamedEventArgs).OldFullPath);
			m_dependencyUpdater.RemoveDependency(uri, m_dependencies);
			m_dependencyUpdater.AddDependency(operation.FilePath, m_dependencies);
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.ExtremelyVerbose, "Updated {0} dependency information after begin renamed from {1}", uri.LocalPath, operation.FilePath.LocalPath);
			break;
		}
		default:
			BugSubmitter.SilentReport("Unknown workspace change event has occurred.  Implement me! @assign bwhitman");
			break;
		}
		m_requiresDependencyUpdate = true;
	}

	private void InvokeIfNeeded(IMainForm invoker, Action act)
	{
		if (invoker?.Invoker != null && invoker.Invoker.InvokeRequired && invoker.DialogOwner != null)
		{
			invoker.Invoker.Invoke(act, null);
		}
		else
		{
			act();
		}
	}

	private void ShowWaitDialog(string dialogTitle, Action<WaitDialog> act)
	{
		InvokeIfNeeded(m_mainWindow, delegate
		{
			WaitDialog waitDialog = new WaitDialog(dialogTitle, act);
			SkinService.ApplyActiveSkin(waitDialog);
			if (m_mainWindow?.DialogOwner != null)
			{
				waitDialog.ShowDialog(m_mainWindow.DialogOwner);
			}
			else
			{
				waitDialog.ShowDialog();
			}
		});
	}

	private void ProcessWorkspaceChanges(object context)
	{
		while (m_workspaceThreadSignal.WaitOne() && m_isRunning)
		{
			ShowWaitDialog("Processing workspace changes: " + TargetProject, delegate(WaitDialog dlg)
			{
				using (new ScopedWriterLock(m_depedencyRegistryLock))
				{
					int num = 0;
					while (true)
					{
						if (m_processingFileQueue.TryDequeue(out var result))
						{
							dlg.SetMessage(Path.GetFileName(result.FilePath.LocalPath));
							ProcessFileWatchOperation(result);
							m_changeSaver?.UpdateLastChangeTime();
							num++;
						}
						else
						{
							Thread.Yield();
							if (m_pendingChanges.Count <= 0)
							{
								break;
							}
						}
					}
					if (m_requiresDependencyUpdate)
					{
						m_dependencies.GenerateDependants();
						m_requiresDependencyUpdate = false;
						Outputs.WriteLine(OutputMessageType.Info, "{0}: Finished dependency update for {1} environment changes", TargetProject, num);
					}
				}
			});
			while (m_queuedEvents.Count > 0)
			{
				WorkspaceItemChangedQueueItem itemChangedEvent = m_queuedEvents.Dequeue();
				FireEvent(itemChangedEvent);
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

	private string SelectNewestFile(string installedFilePath, string userFilePath)
	{
		if (File.Exists(userFilePath) && File.Exists(installedFilePath))
		{
			FileInfo fileInfo = new FileInfo(userFilePath);
			FileInfo fileInfo2 = new FileInfo(installedFilePath);
			if (fileInfo.LastWriteTimeUtc > fileInfo2.LastWriteTimeUtc)
			{
				Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "{0}: Using newer local dependency information at \"{1}\"", TargetProject, userFilePath);
				return userFilePath;
			}
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "{0}: Using newer installed dependency information at \"{1}\"", TargetProject, installedFilePath);
			return installedFilePath;
		}
		if (!File.Exists(userFilePath) && File.Exists(installedFilePath))
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "{0}: Using existent installed dependency information at \"{1}\"", TargetProject, installedFilePath);
			return installedFilePath;
		}
		Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "{0}: Using existent local dependency information at \"{1}\"", TargetProject, userFilePath);
		return userFilePath;
	}

	private void SetupFileWatchers()
	{
		foreach (InstanceType value in Enum.GetValues(typeof(InstanceType)))
		{
			if (value != InstanceType.IT_COUNT && value != InstanceType.IT_INVALID)
			{
				string folder = StaticMethods.PantryRootForInstanceType(m_projectEnvironment.Paths.GamePantry, value);
				AddWatcher(folder);
			}
		}
		AddWatcher(m_projectEnvironment.Paths.XLPRoot);
		AddWatcher(m_projectEnvironment.Paths.ArtDefRoot);
	}

	private void AddWatcher(string folder)
	{
		FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
		fileSystemWatcher.EnableRaisingEvents = false;
		fileSystemWatcher.Path = folder;
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.Changed += HandleWorkspaceItemChanged;
		fileSystemWatcher.Renamed += HandleWorkspaceItemRenamed;
		fileSystemWatcher.Created += HandleWorkspaceItemCreated;
		fileSystemWatcher.Deleted += HandleWorkspaceItemDeleted;
		m_watchers.Add(fileSystemWatcher);
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

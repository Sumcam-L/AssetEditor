using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(EntityDocumentImportWatcherService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class EntityDocumentImportWatcherService : IDisposable, IInitializable
{
	private readonly IFileWatcherService m_fileWatcher;

	private readonly IDocumentRegistry m_documentRegistry;

	private readonly IDocumentRegistryMediator m_registryMediator;

	private readonly IImportService m_importService;

	private readonly AssetBrowserFileCommands m_fileCommands;

	private readonly BatchEntitySourceControlService m_sourceControl;

	private readonly ConcurrentQueue<string> m_changedFiles = new ConcurrentQueue<string>();

	private readonly Thread m_exportThread;

	private readonly AutoResetEvent m_threadSignal = new AutoResetEvent(initialState: false);

	private volatile bool m_exportThreadRunning = true;

	private readonly TimeSpan m_threadDisposeWaitTime = TimeSpan.FromSeconds(30.0);

	private readonly TaskScheduler m_uiTaskScheduler;

	private readonly ISplashScreenService m_splashScreenService;

	private readonly ICivTechService m_civTechService;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	private bool m_exportOnFileChanged = true;

	private bool disposedValue;

	[Import(AllowDefault = true)]
	private IAssetPreviewerService PreviewerService { get; set; }

	[Import(AllowDefault = true)]
	private IWorkspaceChangeMediator WorkspaceChangeMediator { get; set; }

	public bool ExportOnFileChanged
	{
		get
		{
			return m_exportOnFileChanged;
		}
		set
		{
			m_exportOnFileChanged = value;
		}
	}

	private HashSet<EntityID> ImportedEntities { get; } = new HashSet<EntityID>();

	[ImportingConstructor]
	public EntityDocumentImportWatcherService(IDocumentRegistry documentRegistry, IDocumentRegistryMediator mediator, IFileWatcherService fileWatcherService, IImportService importService, AssetBrowserFileCommands fileCommands, BatchEntitySourceControlService sourceControl, ICivTechService civTechSvc, ISplashScreenService splashScreenService)
	{
		m_documentRegistry = documentRegistry;
		m_fileWatcher = fileWatcherService;
		m_importService = importService;
		m_fileCommands = fileCommands;
		m_registryMediator = mediator;
		m_sourceControl = sourceControl;
		m_splashScreenService = splashScreenService;
		m_civTechService = civTechSvc;
		m_uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
		m_exportThread = new Thread(ExportChangedFiles);
		m_exportThread.Name = "Export Changed Source Files Thread";
		m_exportThread.IsBackground = false;
	}

	public void Initialize()
	{
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings("Application".Localize(), new BoundPropertyDescriptor(this, () => ExportOnFileChanged, "Enable Automatic Export".Localize(), "File Watching".Localize(), "If true, when a source file that you have open for edit changes, all entities that depend on it will be exported.".Localize()));
			m_settingsService.RegisterUserSettings("Application".Localize(), new BoundPropertyDescriptor(this, () => ExportOnFileChanged, "Enable Automatic Export".Localize(), "File Watching".Localize(), "If true, when a source file that you have open for edit changes, all entities that depend on it will be exported.".Localize()));
		}
		m_exportThread.Start();
		m_fileWatcher.FileChanged += HandleFileChanged;
	}

	private void HandleFileChanged(object sender, FileSystemEventArgs e)
	{
		if (ShouldExportFile(e.FullPath))
		{
			m_changedFiles.Enqueue(e.FullPath);
			m_threadSignal.Set();
		}
	}

	private bool ShouldExportFile(string filePath)
	{
		if (!ExportOnFileChanged)
		{
			return false;
		}
		if (!Uri.TryCreate(filePath, UriKind.Absolute, out var result))
		{
			return false;
		}
		ProjectEnvironment primaryProject = m_civTechService.PrimaryProject;
		if (primaryProject == null)
		{
			BugSubmitter.SilentAssert(primaryProject != null, "Called 'ShouldExportFile' when the CivTech service's primary project was null.  @assign bwhitman");
			return false;
		}
		IWorkspaceDependencyRegistry dependencyRegistry = primaryProject.DependencyRegistry;
		if (dependencyRegistry == null)
		{
			BugSubmitter.SilentAssert(dependencyRegistry != null, "Called 'ShouldExportFile' before the PrimaryProject had a Dependency Registry assigned.  @assign bwhitman");
			return false;
		}
		if (dependencyRegistry.GetFileType(result) != FileType.SourceFile)
		{
			return false;
		}
		IVersionControlService versionControl = primaryProject.VersionControl;
		if (versionControl == null)
		{
			BugSubmitter.SilentAssert(versionControl != null, "Called 'ShouldExportFile' when the primary project had no VCS service.  @assign bwhitman");
			return false;
		}
		return true;
	}

	private void ExportChangedFiles(object context)
	{
		while (m_exportThreadRunning && m_threadSignal.WaitOne())
		{
			IDocument activeDocument = m_documentRegistry.ActiveDocument;
			while (m_changedFiles.Count != 0)
			{
				IEnumerable<string> enumerable = DrainQueue();
				if (enumerable.Any())
				{
					ExportFiles(enumerable);
				}
			}
			if (ImportedEntities.Count != 0)
			{
				if (PreviewerService != null)
				{
					IEntityChangeList changeList = Context.EnsureCreated<CivTechContext>().CreateInstance<IEntityChangeList>();
					changeList.AddGenericEntityChangedEvents(ImportedEntities);
					PreviewerService.SendChanges(changeList);
				}
				if (WorkspaceChangeMediator != null)
				{
					IEnumerable<Uri> changedEntityUris = GetChangedEntityUris(ImportedEntities);
					WorkspaceChangeMediator.AddChangesToQueue(changedEntityUris);
				}
				ImportedEntities.Clear();
			}
			RestoreActiveDocument(activeDocument);
		}
	}

	private void ExportFiles(IEnumerable<string> filePaths)
	{
		Task.Factory.StartNew(delegate
		{
			m_splashScreenService.ShowSplashScreen(delegate
			{
				ImportAffectedDocuments(filePaths);
			}, "Exporting files...", string.Join("\n", filePaths));
		}, default(CancellationToken), TaskCreationOptions.LongRunning, m_uiTaskScheduler).Wait();
	}

	private void RestoreActiveDocument(IDocument activeDocument)
	{
		Task.Factory.StartNew(delegate
		{
			OpenDocumentSafe(activeDocument);
		}, default(CancellationToken), TaskCreationOptions.LongRunning, m_uiTaskScheduler).Wait();
	}

	private IEnumerable<string> DrainQueue()
	{
		ICollection<string> collection = new HashSet<string>();
		string result;
		while (m_changedFiles.TryDequeue(out result))
		{
			collection.Add(result);
		}
		return collection;
	}

	private void ImportAffectedDocuments(IEnumerable<string> paths)
	{
		IEnumerable<EntityID> affectedEntities = GetAffectedEntities(paths);
		new EntityImporter(m_civTechService, m_importService, m_fileCommands, m_registryMediator, m_sourceControl, affectedEntities, recurseIntoChildren: false).Import();
		ImportedEntities.UnionWith(affectedEntities);
	}

	private IEnumerable<EntityID> GetAffectedEntities(IEnumerable<string> changedSourceFilePaths)
	{
		ISet<EntityID> set = new HashSet<EntityID>();
		GetAffectedEntities(m_civTechService.PrimaryProject.DependencyRegistry, changedSourceFilePaths, set);
		foreach (string dependency in m_civTechService.PrimaryProject.Dependencies)
		{
			if (!m_civTechService.ActiveProjectMap.ContainsProject(dependency))
			{
				Outputs.WriteLine(OutputMessageType.Warning, "Ignoring inactive project \"{0}\" during export", dependency);
			}
			else
			{
				GetAffectedEntities(m_civTechService.ActiveProjectMap[dependency].DependencyRegistry, changedSourceFilePaths, set);
			}
		}
		return set;
	}

	private void GetAffectedEntities(IWorkspaceDependencyRegistry depReg, IEnumerable<string> changedSourceFilePaths, ISet<EntityID> entityIDSet)
	{
		foreach (string changedSourceFilePath in changedSourceFilePaths)
		{
			Uri item;
			try
			{
				item = new Uri(changedSourceFilePath);
			}
			catch (UriFormatException)
			{
				continue;
			}
			catch (ArgumentNullException)
			{
				continue;
			}
			IEnumerable<Uri> dependents = depReg.GetDependents(item);
			AddEntityIDsToSet(dependents, entityIDSet);
		}
	}

	private void AddEntityIDsToSet(IEnumerable<Uri> entityUris, ISet<EntityID> entityIDSet)
	{
		foreach (Uri entityUri in entityUris)
		{
			if (StaticMethods.GetInstanceNameAndType(m_civTechService.ProjectMapService, entityUri.LocalPath, out var instanceName, out var type))
			{
				EntityID item = new EntityID(instanceName, type);
				entityIDSet.Add(item);
			}
		}
	}

	private IEnumerable<Uri> GetChangedEntityUris(IEnumerable<EntityID> entityIDs)
	{
		foreach (EntityID entityID in entityIDs)
		{
			string entityPath = m_civTechService.GetEntityPath(entityID.Name, entityID.Type);
			if (Uri.TryCreate(entityPath, UriKind.Absolute, out var result))
			{
				yield return result;
			}
			else
			{
				BugSubmitter.SilentReport($"CivTechService.GetEntityPath({entityID.Name}, {entityID.Type}) returned {entityPath} @summary CivTechService.GetEntityPath returned an invalid Uri while attempting to build the hot load list in response to files changed.  @assign bwhitman  @summary Invalid Entity URI when Creating Hot Load Queue");
			}
			result = null;
		}
	}

	private void OpenDocumentSafe(IDocument document)
	{
		if (document != null)
		{
			IDocumentClient client = m_fileCommands.GetClient(document);
			m_fileCommands.OpenExistingDocument(client, document.Uri);
		}
	}

	void IDisposable.Dispose()
	{
		if (disposedValue)
		{
			return;
		}
		disposedValue = true;
		m_fileWatcher.FileChanged -= HandleFileChanged;
		m_exportThreadRunning = false;
		m_threadSignal.Set();
		Thread.Sleep(50);
		if (m_exportThread.IsAlive)
		{
			Action action = delegate
			{
				m_exportThread.Join();
			};
			string message = "Waiting for export thread to finish...";
			m_splashScreenService.ShowSplashScreen(action, "Source File Watcher Service", message);
		}
		m_threadSignal.Dispose();
	}
}

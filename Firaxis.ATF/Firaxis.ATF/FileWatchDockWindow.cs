using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(IFileWatchDockWindow))]
[Export(typeof(FileWatchDockWindow))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FileWatchDockWindow : IInitializable, IControlHostClient, IFileWatchDockWindow, IDisposable
{
	[Import(AllowDefault = false)]
	private IControlHostService m_controlHostService;

	private FileWatchTreeContext m_dataContext;

	private bool m_disposedValue;

	[Import(AllowDefault = false)]
	private IFileWatcherService m_fileWatcherService;

	private readonly IDocumentRegistry m_documentRegistry;

	private readonly FileWatchControl m_fileWatchControl;

	private readonly ICivTechService m_civTechService;

	private readonly ISet<IDocument> m_watchedDocuments = new HashSet<IDocument>();

	[ImportingConstructor]
	public FileWatchDockWindow(ICivTechService civTechSvc, IDocumentRegistry registry, ISkinService skinSvc)
	{
		m_civTechService = civTechSvc;
		m_documentRegistry = registry;
		m_fileWatchControl = new FileWatchControl(m_civTechService, skinSvc);
	}

	void IControlHostClient.Activate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		return true;
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void FinishProjectChange()
	{
		RegisterWatchedHandlers();
		RegisterDocumentHandlers();
		RegisterDependencyHandlers();
	}

	public void HandleProjectChange()
	{
	}

	public void StartProjectChange()
	{
		UnregisterDependencyHandlers();
		UnregisterDocumentHandlers();
		UnregisterWatchedHandlers();
	}

	void IInitializable.Initialize()
	{
		if (m_controlHostService != null && m_fileWatcherService != null && m_documentRegistry != null)
		{
			m_dataContext = new FileWatchTreeContext(m_fileWatcherService, m_civTechService);
			m_controlHostService.RegisterControl(m_fileWatchControl, "File Watches", "File watch management view", StandardControlGroup.Bottom, null, this);
			m_fileWatchControl.Bind(m_dataContext);
			RegisterDocumentHandlers();
			RegisterDependencyHandlers();
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!m_disposedValue)
		{
			if (disposing)
			{
				UnregisterWatchedHandlers();
				UnregisterDocumentHandlers();
				UnregisterDependencyHandlers();
				m_watchedDocuments.Clear();
				m_fileWatchControl.Dispose();
				m_dataContext.Dispose();
			}
			m_disposedValue = true;
		}
	}

	private void DocumentRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
	{
		Uri uri = e.Item.Uri;
		if (m_watchedDocuments.Add(e.Item))
		{
			RegisterChangeHandler(e.Item);
			e.Item.UriChanged += Item_UriChanged;
		}
		m_dataContext.AddRootNode(uri);
	}

	private void DocumentRegistry_DocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
	{
		IDocument item = e.Item;
		item.UriChanged -= Item_UriChanged;
		m_watchedDocuments.Remove(item);
		Uri uri = item.Uri;
		m_dataContext.RemoveRootNode(uri);
	}

	private void Item_UriChanged(object sender, UriChangedEventArgs e)
	{
		IResource resource = sender as IResource;
		m_dataContext.RemoveRootNode(e.OldUri);
		m_dataContext.AddRootNode(resource.Uri);
	}

	private void RegisterChangeHandler(IDocument doc)
	{
		UnregisterChangeHandler(doc);
		doc.UriChanged += Item_UriChanged;
	}

	private void RegisterDependencyHandlers()
	{
		IWorkspaceDependencyWatcher workspaceDependencyWatcher = m_civTechService.PrimaryProject.DependencyRegistry.As<IWorkspaceDependencyWatcher>();
		if (workspaceDependencyWatcher != null)
		{
			workspaceDependencyWatcher.WorkspaceItemChanged -= WorkspaceWatcher_WorkspaceItemChanged;
			workspaceDependencyWatcher.WorkspaceItemChanged += WorkspaceWatcher_WorkspaceItemChanged;
		}
	}

	private void RegisterDocumentHandlers()
	{
		UnregisterDocumentHandlers();
		m_documentRegistry.DocumentAdded += DocumentRegistry_DocumentAdded;
		m_documentRegistry.DocumentRemoved += DocumentRegistry_DocumentRemoved;
	}

	private void RegisterWatchedHandlers()
	{
		foreach (IDocument watchedDocument in m_watchedDocuments)
		{
			RegisterChangeHandler(watchedDocument);
		}
	}

	private void UnregisterChangeHandler(IDocument doc)
	{
		doc.UriChanged -= Item_UriChanged;
	}

	private void UnregisterDependencyHandlers()
	{
		foreach (ProjectEnvironment project in m_civTechService.AllProjectsMap.Projects)
		{
			if (project.DependencyRegistry != null)
			{
				IWorkspaceDependencyWatcher workspaceDependencyWatcher = project.DependencyRegistry.As<IWorkspaceDependencyWatcher>();
				if (workspaceDependencyWatcher != null)
				{
					workspaceDependencyWatcher.WorkspaceItemChanged -= WorkspaceWatcher_WorkspaceItemChanged;
				}
			}
		}
	}

	private void UnregisterDocumentHandlers()
	{
		m_documentRegistry.DocumentAdded -= DocumentRegistry_DocumentAdded;
		m_documentRegistry.DocumentRemoved -= DocumentRegistry_DocumentRemoved;
	}

	private void UnregisterWatchedHandlers()
	{
		foreach (IDocument watchedDocument in m_watchedDocuments)
		{
			UnregisterChangeHandler(watchedDocument);
		}
	}

	private void WorkspaceWatcher_WorkspaceItemChanged(object sender, WorkspaceItemChangedEvent e)
	{
		m_dataContext.UpdateChangedNodes(e.Uri);
	}
}

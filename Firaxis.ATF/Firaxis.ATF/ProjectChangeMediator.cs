using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Firaxis.CivTech;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(ProjectChangeMediator))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ProjectChangeMediator
{
	[ImportMany]
	private Lazy<IProjectChangeWatcher>[] ProjectChangeWatchers;

	private IProjectMapService ProjectMapService { get; set; }

	private IProjectSelectionService ProjectSelectionService { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IProjectSelectionCommands> ProjectSelectionCommands { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IAssetPreviewerService> AssetPreviewerService { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IPreviewerCacheService> PreviewerCacheService { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IPreviewerWidgetService> PreviewerWidgetService { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IPreviewerDocumentService> PreviewerDocumentService { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IPreviewerKnobService> PreviewerKnobService { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IPreviewerWindowService> PreviewerWindowService { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IPreviewerEntityLoadingService> PreviewerEntityLoadingService { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<WorkspaceWatcherService> WorkspaceWatcherService { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IFileWatchDockWindow> FileWatchDockWindow { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IEntityCacheService> EntityCacheService { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IArtDefRegistry> ArtDefRegistry { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IXLPRegistry> XLPRegistry { get; set; }

	[Import(AllowDefault = true)]
	private Lazy<IWorkspaceDependencyRegistryService> WorkspaceDependencyRegistryService { get; set; }

	protected ISynchronizeInvoke Invoker { get; set; }

	[ImportingConstructor]
	public ProjectChangeMediator(IProjectMapService pms, IProjectSelectionService pss)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			ProjectMapService = pms;
			ProjectSelectionService = pss;
			ProjectSelectionService.ProjectChanged += ProjectSelectionService_ProjectChanged;
		}
	}

	private bool IsServiceAvailable<T>(Lazy<T> service)
	{
		if (service != null)
		{
			return service.Value != null;
		}
		return false;
	}

	private void InvokeIfNeeded(Action func)
	{
		if (Invoker != null && Invoker.InvokeRequired)
		{
			Invoker.Invoke(func, null);
		}
		else
		{
			func();
		}
	}

	private void BeginInvokeIfNeeded(Action func)
	{
		if (Invoker != null && Invoker.InvokeRequired)
		{
			Invoker.BeginInvoke(func, null);
		}
		else
		{
			func();
		}
	}

	protected virtual void SetMessage(string message)
	{
		Outputs.WriteLine(OutputMessageType.Info, message);
	}

	protected virtual void DoProjectChange()
	{
		if (IsServiceAvailable(WorkspaceWatcherService))
		{
			WorkspaceWatcherService.Value.StartProjectChange(SetMessage);
		}
		if (IsServiceAvailable(FileWatchDockWindow))
		{
			FileWatchDockWindow.Value.StartProjectChange();
		}
		if (IsServiceAvailable(WorkspaceDependencyRegistryService))
		{
			WorkspaceDependencyRegistryService.Value.StartProjectChange();
		}
		SetMessage("Updating project map...");
		ProjectMapService.HandleProjectChange();
		if (IsServiceAvailable(ProjectSelectionCommands))
		{
			SetMessage("Updating project menu...");
			BeginInvokeIfNeeded(ProjectSelectionCommands.Value.HandleProjectChange);
		}
		if (IsServiceAvailable(WorkspaceDependencyRegistryService))
		{
			SetMessage("Updating workspace registry...");
			WorkspaceDependencyRegistryService.Value.HandleProjectChange();
		}
		if (IsServiceAvailable(FileWatchDockWindow))
		{
			FileWatchDockWindow.Value.HandleProjectChange();
		}
		if (IsServiceAvailable(EntityCacheService))
		{
			SetMessage("Updating entity cache...");
			EntityCacheService.Value.HandleProjectChange();
		}
		if (IsServiceAvailable(ArtDefRegistry))
		{
			SetMessage("Updating ArtDef registry...");
			ArtDefRegistry.Value.HandleProjectChange();
		}
		if (IsServiceAvailable(XLPRegistry))
		{
			SetMessage("Updating XLP registry...");
			XLPRegistry.Value.HandleProjectChange();
		}
		if (IsServiceAvailable(AssetPreviewerService))
		{
			SetMessage("Restarting previewer...");
			InvokeIfNeeded(delegate
			{
				PreviewerWidgetService?.Value?.StartProjectChange();
				PreviewerCacheService?.Value?.StartProjectChange();
				AssetPreviewerService?.Value?.HandleProjectChange();
				PreviewerCacheService?.Value?.FinishProjectChange();
				PreviewerWidgetService?.Value?.FinishProjectChange();
			});
		}
		else if (IsServiceAvailable(PreviewerWindowService))
		{
			SetMessage("Beginning previewer restart...");
			InvokeIfNeeded(delegate
			{
				PreviewerWidgetService?.Value?.StartProjectChange();
				PreviewerCacheService?.Value?.StartProjectChange();
				PreviewerKnobService?.Value?.StartProjectChange(SetMessage);
				PreviewerDocumentService?.Value?.StartProjectChange(SetMessage);
				PreviewerEntityLoadingService?.Value?.StartProjectChange(SetMessage);
				PreviewerWindowService?.Value?.HandleProjectChange(SetMessage);
				PreviewerEntityLoadingService?.Value?.FinishProjectChange(SetMessage);
				PreviewerDocumentService?.Value?.FinishProjectChange(SetMessage);
				PreviewerKnobService?.Value?.FinishProjectChange(SetMessage);
				PreviewerCacheService?.Value?.FinishProjectChange();
				PreviewerWidgetService?.Value?.FinishProjectChange();
			});
			SetMessage("Finished previewer restart");
		}
		if (IsServiceAvailable(WorkspaceDependencyRegistryService))
		{
			WorkspaceDependencyRegistryService?.Value?.FinishProjectChange();
		}
		if (IsServiceAvailable(FileWatchDockWindow))
		{
			FileWatchDockWindow?.Value?.FinishProjectChange();
		}
		if (IsServiceAvailable(WorkspaceWatcherService))
		{
			WorkspaceWatcherService.Value.FinishProjectChange(SetMessage);
		}
		if (ProjectChangeWatchers == null)
		{
			return;
		}
		Lazy<IProjectChangeWatcher>[] projectChangeWatchers = ProjectChangeWatchers;
		foreach (Lazy<IProjectChangeWatcher> lazy in projectChangeWatchers)
		{
			if (IsServiceAvailable(lazy))
			{
				lazy.Value.HandleProjectChange(SetMessage);
			}
		}
	}

	private void ProjectSelectionService_ProjectChanged(object sender, EventArgs e)
	{
		DoProjectChange();
	}
}

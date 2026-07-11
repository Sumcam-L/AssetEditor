using System;
using System.ComponentModel.Composition;
using Firaxis.CivTech;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public abstract class WorkspaceDependencyRegistryServiceBase : IWorkspaceDependencyRegistryService, IDisposable, IPartImportsSatisfiedNotification
{
	private bool m_disposedValue;

	protected IProjectConfigService ProjectConfigService { get; private set; }

	protected IProjectMapService ProjectMapService { get; private set; }

	protected IVersionControlSelectionService VersionControlSelectionService { get; private set; }

	[Import(AllowDefault = true)]
	protected IMainForm MainWindow { get; set; }

	public WorkspaceDependencyRegistryServiceBase(IProjectMapService projectMapService, IProjectConfigService projCfgSvc, IVersionControlSelectionService verCtlSelSvc)
	{
		ProjectMapService = projectMapService;
		ProjectConfigService = projCfgSvc;
		VersionControlSelectionService = verCtlSelSvc;
	}

	protected abstract IWorkspaceDependencyRegistry CreateWorkspaceDependencyRegistry(IMainWindow mainWindow, IProjectMapService projectMapService, IProjectConfigService projCfgSvc, string projName, ProjectInfo projInfo, IVersionControlService verCtlSvc);

	public void OnImportsSatisfied()
	{
		SetupDependencyRegistries();
		EnableFileWatches();
	}

	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_disposedValue)
		{
			return;
		}
		if (disposing)
		{
			foreach (ProjectEnvironment project in ProjectMapService.ActiveProjectMap.Projects)
			{
				if (project.DependencyRegistry != null)
				{
					project.DependencyRegistry.As<IWorkspaceDependencyWatcher>()?.Dispose();
					project.DependencyRegistry = null;
				}
			}
		}
		m_disposedValue = true;
	}

	public virtual void FinishProjectChange()
	{
		EnableFileWatches();
	}

	public virtual void HandleProjectChange()
	{
		SetupDependencyRegistries();
	}

	public virtual void StartProjectChange()
	{
		DisableFileWatches();
	}

	private void SetupDependencyRegistries()
	{
		using (new ScopedStopwatch("Setting up project dependencies took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			foreach (ProjectEnvironment project in ProjectMapService.ActiveProjectMap.Projects)
			{
				if (project.DependencyRegistry == null)
				{
					IWorkspaceDependencyRegistry workspaceDependencyRegistry = CreateWorkspaceDependencyRegistry(MainWindow, ProjectMapService, ProjectConfigService, project.Name, project.Info, VersionControlSelectionService[project.Name]);
					workspaceDependencyRegistry.Initialize(project.Name);
					project.DependencyRegistry = workspaceDependencyRegistry;
				}
			}
		}
	}

	protected virtual void EnableFileWatches()
	{
		ProjectMapService.ActiveProjectMap.Projects.ForEach(delegate(ProjectEnvironment projectEnvironment)
		{
			projectEnvironment.DependencyRegistry.As<IWorkspaceDependencyWatcher>()?.EnableFileWatches();
		});
	}

	protected virtual void DisableFileWatches()
	{
		ProjectMapService.ActiveProjectMap.Projects.ForEach(delegate(ProjectEnvironment projectEnvironment)
		{
			projectEnvironment.DependencyRegistry.As<IWorkspaceDependencyWatcher>()?.DisableFileWatches();
		});
	}
}

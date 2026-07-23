using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
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
		PaintTimingLog.Write("Startup: deps-registries-done");
		EnableFileWatches();
		PaintTimingLog.Write("Startup: deps-watches-enabled");
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
			// Phase 1: Create all registries (fast, sequential)
			var pendingInits = new List<ProjectEnvironment>();
			foreach (ProjectEnvironment project in ProjectMapService.ActiveProjectMap.Projects)
			{
				if (project.DependencyRegistry == null)
				{
					var projTimer = System.Diagnostics.Stopwatch.StartNew();
					IWorkspaceDependencyRegistry workspaceDependencyRegistry = CreateWorkspaceDependencyRegistry(MainWindow, ProjectMapService, ProjectConfigService, project.Name, project.Info, VersionControlSelectionService[project.Name]);
					PaintTimingLog.Write("Startup: deps-reg created project={0} type={1} elapsed={2}ms", project.Name, project.Info.ProjectType, projTimer.ElapsedMilliseconds);
					project.DependencyRegistry = workspaceDependencyRegistry;
					pendingInits.Add(project);
				}
			}

			// Phase 2: Initialize in parallel (JSON loading is CPU-bound, per-instance isolated)
			if (pendingInits.Count > 1)
			{
				var parallelTimer = System.Diagnostics.Stopwatch.StartNew();
				Parallel.ForEach(pendingInits, project =>
				{
					var projTimer = System.Diagnostics.Stopwatch.StartNew();
					project.DependencyRegistry.Initialize(project.Name);
					PaintTimingLog.Write("Startup: deps-reg initialized project={0} elapsed={1}ms", project.Name, projTimer.ElapsedMilliseconds);
				});
				PaintTimingLog.Write("Startup: deps-reg parallel-init total elapsed={0}ms", parallelTimer.ElapsedMilliseconds);
			}
			else if (pendingInits.Count == 1)
			{
				var project = pendingInits[0];
				var projTimer = System.Diagnostics.Stopwatch.StartNew();
				project.DependencyRegistry.Initialize(project.Name);
				PaintTimingLog.Write("Startup: deps-reg initialized project={0} elapsed={1}ms", project.Name, projTimer.ElapsedMilliseconds);
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

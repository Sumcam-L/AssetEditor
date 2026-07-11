using System.ComponentModel.Composition;
using System.IO;
using DatabaseWrapper;
using Firaxis.CivTech;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(IWorkspaceDependencyRegistryService))]
[Export(typeof(StaticDependencyRegistryService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StaticDependencyRegistryService : IWorkspaceDependencyRegistryService
{
	private IProjectConfigService ProjectConfigService { get; set; }

	private IProjectMapService ProjectMapService { get; set; }

	private IVersionControlSelectionService VersionControlSelectionService { get; set; }

	private string DependencyFileFolder { get; set; }

	[ImportingConstructor]
	public StaticDependencyRegistryService(IProjectMapService projectMapService, IProjectConfigService projCfgSvc, IVersionControlSelectionService verCtlSelSvc, IDependencyRootProvider depRootProv)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			ProjectMapService = projectMapService;
			ProjectConfigService = projCfgSvc;
			VersionControlSelectionService = verCtlSelSvc;
			DependencyFileFolder = depRootProv.DependencyRoot;
			if (!Directory.Exists(DependencyFileFolder))
			{
				Directory.CreateDirectory(DependencyFileFolder);
			}
			SetupDependencyRegistries();
		}
	}

	public virtual void FinishProjectChange()
	{
	}

	public virtual void HandleProjectChange()
	{
		foreach (ProjectEnvironment project in ProjectMapService.ActiveProjectMap.Projects)
		{
			if (project.DependencyRegistry == null)
			{
				WorkspaceDependencyBase depUpdater = new WorkspaceDependencyLoader(ProjectMapService, ProjectConfigService, project.Name, Path.Combine(DependencyFileFolder, project.Name + "-asset-deps.json"));
				IWorkspaceDependencyRegistry workspaceDependencyRegistry = new WorkspaceDependencyRegistryLite(ProjectMapService, ProjectConfigService, VersionControlSelectionService[project.Name], depUpdater);
				workspaceDependencyRegistry.Initialize(project.Name);
				project.DependencyRegistry = workspaceDependencyRegistry;
			}
		}
	}

	public virtual void StartProjectChange()
	{
	}

	private void SetupDependencyRegistries()
	{
		foreach (ProjectEnvironment project in ProjectMapService.ActiveProjectMap.Projects)
		{
			if (VersionControlSelectionService.VersionControlInfoMap.ContainsKey(project.Name))
			{
				WorkspaceDependencyBase depUpdater = new WorkspaceDependencyLoader(ProjectMapService, ProjectConfigService, project.Name, Path.Combine(DependencyFileFolder, project.Name + "-asset-deps.json"));
				IWorkspaceDependencyRegistry workspaceDependencyRegistry = (project.DependencyRegistry = new WorkspaceDependencyRegistryLite(ProjectMapService, ProjectConfigService, VersionControlSelectionService[project.Name], depUpdater));
				workspaceDependencyRegistry.Initialize(project.Name);
			}
		}
	}
}

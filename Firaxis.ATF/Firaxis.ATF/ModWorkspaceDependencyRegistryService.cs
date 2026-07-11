using System.ComponentModel.Composition;
using System.IO;
using DatabaseWrapper;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IWorkspaceDependencyRegistryService))]
[Export(typeof(ModWorkspaceDependencyRegistryService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ModWorkspaceDependencyRegistryService : WorkspaceDependencyRegistryServiceBase
{
	[ImportingConstructor]
	public ModWorkspaceDependencyRegistryService(IProjectMapService projectMapService, IProjectConfigService projCfgSvc, IVersionControlSelectionService verCtlSelSvc)
		: base(projectMapService, projCfgSvc, verCtlSelSvc)
	{
	}

	protected override IWorkspaceDependencyRegistry CreateWorkspaceDependencyRegistry(IMainWindow mainWindow, IProjectMapService projMapSvc, IProjectConfigService projCfgSvc, string projName, ProjectInfo projInfo, IVersionControlService verCtlSvc)
	{
		if (projInfo.ProjectType == ProjectType.eMod)
		{
			return new WorkspaceDependencyRegistry(base.MainWindow, projMapSvc, projCfgSvc, verCtlSvc);
		}
		string depFilePath = Path.Combine(verCtlSvc.WorkspaceRoot, projName + "-asset-deps.json");
		WorkspaceDependencyBase depUpdater = new WorkspaceDependencyLoader(projMapSvc, projCfgSvc, projName, depFilePath);
		return new WorkspaceDependencyRegistryLite(projMapSvc, projCfgSvc, verCtlSvc, depUpdater);
	}

	protected override void EnableFileWatches()
	{
		base.ProjectMapService.ActiveProjectMap.Projects.ForEach(delegate(ProjectEnvironment proj)
		{
			IWorkspaceDependencyWatcher workspaceDependencyWatcher = proj.DependencyRegistry.As<IWorkspaceDependencyWatcher>();
			if (workspaceDependencyWatcher != null)
			{
				if (proj.Info.ProjectType == ProjectType.eMod)
				{
					workspaceDependencyWatcher.EnableFileWatches();
				}
				else
				{
					workspaceDependencyWatcher.DisableFileWatches();
				}
			}
		});
	}

	protected override void DisableFileWatches()
	{
		base.ProjectMapService.ActiveProjectMap.Projects.ForEach(delegate(ProjectEnvironment proj)
		{
			proj.DependencyRegistry.As<IWorkspaceDependencyWatcher>()?.DisableFileWatches();
		});
	}
}

using System.ComponentModel.Composition;
using Firaxis.CivTech;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IWorkspaceDependencyRegistryService))]
[Export(typeof(WorkspaceDependencyRegistryService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class WorkspaceDependencyRegistryService : WorkspaceDependencyRegistryServiceBase
{
	[ImportingConstructor]
	public WorkspaceDependencyRegistryService(IProjectMapService projectMapService, IProjectConfigService projCfgSvc, IVersionControlSelectionService verCtlSelSvc)
		: base(projectMapService, projCfgSvc, verCtlSelSvc)
	{
	}

	protected override IWorkspaceDependencyRegistry CreateWorkspaceDependencyRegistry(IMainWindow mainWindow, IProjectMapService projectMapService, IProjectConfigService projCfgSvc, string projName, ProjectInfo projInfo, IVersionControlService verCtlSvc)
	{
		return new WorkspaceDependencyRegistry(base.MainWindow, projectMapService, projCfgSvc, verCtlSvc);
	}
}

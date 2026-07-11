using System.ComponentModel.Composition;
using Firaxis.CivTech;

namespace Firaxis.ATF;

[Export(typeof(IVersionControlSelectionService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class LocalVersionControlSelectionService : VersionControlSelectionServiceBase
{
	[ImportingConstructor]
	public LocalVersionControlSelectionService(IProjectSelectionService pss, IProjectRootProvider rootProviver)
	{
		foreach (ProjectInfo projectInfo in pss.Projects.ProjectInfos)
		{
			if (!VersionControlInfoMap.ContainsKey(projectInfo.Name))
			{
				VersionControlInfoMap[projectInfo.Name] = new VersionControlInfo(projectInfo.Name, rootProviver.WorkspaceRoot);
			}
		}
	}
}

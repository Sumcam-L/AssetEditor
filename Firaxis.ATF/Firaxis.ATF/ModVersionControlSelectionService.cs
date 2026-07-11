using System.ComponentModel.Composition;
using System.IO;
using Firaxis.CivTech;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(IVersionControlSelectionService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ModVersionControlSelectionService : VersionControlSelectionServiceBase
{
	[ImportingConstructor]
	public ModVersionControlSelectionService(IProjectSelectionService pss, IModWorkspaceRootProvider modRootProvider)
	{
		foreach (ProjectInfo projectInfo in pss.Projects.ProjectInfos)
		{
			if (!VersionControlInfoMap.ContainsKey(projectInfo.Name))
			{
				VersionControlInfoMap[projectInfo.Name] = new VersionControlInfo(projectInfo.Name, (projectInfo.ProjectType == ProjectType.eMod) ? modRootProvider.ModWorkspaceRoot : modRootProvider.BaseWorkspaceRoot);
				if (!Directory.Exists(projectInfo.Paths.Pantry))
				{
					Outputs.WriteLine(OutputMessageType.Error, "Failed to find pantry for project \"{0}\" at \"{1}\"", projectInfo.Name, projectInfo.Paths.Pantry);
				}
			}
		}
	}
}

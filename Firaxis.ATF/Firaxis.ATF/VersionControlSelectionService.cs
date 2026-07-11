using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.CivTech;
using Microsoft.Win32;

namespace Firaxis.ATF;

[Export(typeof(IVersionControlSelectionService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class VersionControlSelectionService : VersionControlSelectionServiceBase
{
	private IAssetCloudSettingService AssetCloudSettingService { get; set; }

	public static VersionControlInfo CreateFromRegistry(IAssetCloudSettingService acss)
	{
		RegistryKey toolsRegistryKey = acss.AssetCloudSettings.GetToolsRegistryKey("Civ6", "");
		if (toolsRegistryKey == null)
		{
			return null;
		}
		if (!(toolsRegistryKey.GetValue("PerforceUser") is string user))
		{
			return null;
		}
		if (!(toolsRegistryKey.GetValue("PerforceWorkspace") is string workspace))
		{
			return null;
		}
		return new VersionControlInfo("helix.internal.firaxis.com:1667", user, workspace);
	}

	public static VersionControlInfo CreateFromUserInput(string projName)
	{
		VersionControlSetupForm versionControlSetupForm = new VersionControlSetupForm(projName);
		if (versionControlSetupForm.ShowDialog() != DialogResult.OK)
		{
			return null;
		}
		if (string.IsNullOrEmpty(versionControlSetupForm.Server) || string.IsNullOrEmpty(versionControlSetupForm.User) || string.IsNullOrEmpty(versionControlSetupForm.Workspace))
		{
			return null;
		}
		return new VersionControlInfo(versionControlSetupForm.Server, versionControlSetupForm.User, versionControlSetupForm.Workspace);
	}

	[ImportingConstructor]
	public VersionControlSelectionService(IProjectSelectionService pss, IAssetCloudSettingService acss)
	{
		AssetCloudSettingService = acss;
		foreach (ProjectInfo projectInfo in pss.Projects.ProjectInfos)
		{
			if (VersionControlInfoMap.ContainsKey(projectInfo.Name))
			{
				continue;
			}
			VersionControlInfo versionControlInfo = VersionControlInfoHelpers.LoadVersionControlInfo(projectInfo.Name);
			if (versionControlInfo != null)
			{
				VersionControlInfoMap[projectInfo.Name] = versionControlInfo;
				continue;
			}
			if (projectInfo.ProjectType != ProjectType.eMod)
			{
				versionControlInfo = CreateFromRegistry(AssetCloudSettingService);
				if (versionControlInfo == null)
				{
					versionControlInfo = CreateFromUserInput(projectInfo.Name);
					if (versionControlInfo == null)
					{
						throw new ProjectConfigVersionControlException(projectInfo.Name);
					}
				}
			}
			else
			{
				FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
				{
					Description = "Select the root folder for the mod \"" + projectInfo.Name + "\""
				};
				if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
				{
					throw new ProjectConfigVersionControlException(projectInfo.Name);
				}
				versionControlInfo = new VersionControlInfo(projectInfo.Name, folderBrowserDialog.SelectedPath);
			}
			VersionControlInfoMap[projectInfo.Name] = versionControlInfo;
			VersionControlInfoHelpers.SaveVersionControlInfo(projectInfo.Name, versionControlInfo);
		}
	}
}

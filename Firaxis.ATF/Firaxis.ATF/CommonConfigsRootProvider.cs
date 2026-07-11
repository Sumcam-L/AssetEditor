using System.ComponentModel.Composition;
using System.IO;
using Firaxis.CivTech;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(ICommonConfigsRootProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CommonConfigsRootProvider : CommonConfigsRootProviderBase
{
	public override string WorkspaceRoot { get; protected set; }

	private void SetWorkspaceRootFromVersionControl(IAssetCloudSettingService acss)
	{
		VersionControlInfo versionControlInfo = VersionControlInfoHelpers.LoadVersionControlInfo("Civ6");
		if (versionControlInfo == null)
		{
			versionControlInfo = VersionControlSelectionService.CreateFromRegistry(acss);
			if (versionControlInfo == null)
			{
				versionControlInfo = VersionControlSelectionService.CreateFromUserInput("Civ6");
				if (versionControlInfo == null)
				{
					throw new ProjectConfigVersionControlException("Civ6");
				}
			}
			VersionControlInfoHelpers.SaveVersionControlInfo("Civ6", versionControlInfo);
		}
		VersionControlService versionControlService = new VersionControlService(versionControlInfo);
		WorkspaceRoot = versionControlService.WorkspaceRoot;
		base.ConfigPath = Path.Combine(WorkspaceRoot, "Civ6", "pantry", "Civ6.cfg");
		base.EnvironmentPath = Path.Combine(WorkspaceRoot, "Civ6", "AssetCloud.env");
	}

	[ImportingConstructor]
	public CommonConfigsRootProvider(IAssetCloudSettingService acss)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			if (!SetWorkspaceRootFromExecutablePath())
			{
				SetWorkspaceRootFromVersionControl(acss);
			}
		}
	}
}

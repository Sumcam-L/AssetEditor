using System.ComponentModel.Composition;
using System.IO;
using Firaxis.CivTech;

namespace Firaxis.ATF;

[Export(typeof(ICommonConfigsRootProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class LocalCommonConfigsRootProvider : CommonConfigsRootProviderBase
{
	public override string WorkspaceRoot { get; protected set; }

	public LocalCommonConfigsRootProvider(string root)
	{
		if (!SetWorkspaceRootFromExecutablePath())
		{
			WorkspaceRoot = root;
			base.ConfigPath = Path.Combine(WorkspaceRoot, "Civ6", "pantry", "Civ6.cfg");
			base.EnvironmentPath = Path.Combine(WorkspaceRoot, "Civ6", "AssetCloud.env");
		}
	}
}

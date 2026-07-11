using System.ComponentModel.Composition;

namespace Firaxis.ATF;

[Export(typeof(IModWorkspaceRootProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ModWorkspaceRootProvider : IModWorkspaceRootProvider
{
	public string ModWorkspaceRoot { get; private set; }

	public string BaseWorkspaceRoot { get; private set; }

	public ModWorkspaceRootProvider(string modRoot, string baseRoot)
	{
		ModWorkspaceRoot = modRoot;
		BaseWorkspaceRoot = baseRoot;
	}
}

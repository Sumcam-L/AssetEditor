using System.ComponentModel.Composition;
using Firaxis.CivTech;

namespace Firaxis.ATF;

[Export(typeof(IProjectRootProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class LocalWorkspaceRootProvider : IProjectRootProvider
{
	public string WorkspaceRoot { get; private set; }

	public LocalWorkspaceRootProvider(string root)
	{
		WorkspaceRoot = root;
	}
}

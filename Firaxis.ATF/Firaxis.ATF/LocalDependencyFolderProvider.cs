using System.ComponentModel.Composition;

namespace Firaxis.ATF;

[Export(typeof(IDependencyRootProvider))]
[Export(typeof(LocalDependencyFolderProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class LocalDependencyFolderProvider : IDependencyRootProvider
{
	private readonly string LocalDepFolder;

	public string DependencyRoot => LocalDepFolder;

	public LocalDependencyFolderProvider(string depFolder)
	{
		LocalDepFolder = depFolder;
	}
}

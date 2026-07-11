using System;
using System.Collections.Generic;

namespace Firaxis.CivTech;

public interface IWorkspaceDependencyRegistry
{
	void Initialize(string targetProject);

	DateTime GetLastChangeTime(Uri fileUri);

	DependencyTree GetDependentTree(Uri item);

	IEnumerable<Uri> GetDependencies(Uri item);

	IEnumerable<Uri> GetDependents(Uri item);

	bool DependsOn(Uri entityThatDependsOn, Uri entityThatIsDependedOn);

	bool HasFile(Uri item);

	IEnumerable<Uri> GetFiles();

	FileType GetFileType(Uri item);

	bool GetFileInfo(Uri item, ref DepotFileInfo info);
}

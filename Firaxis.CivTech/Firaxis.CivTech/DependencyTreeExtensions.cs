using System;
using System.Collections.Generic;

namespace Firaxis.CivTech;

public static class DependencyTreeExtensions
{
	public static IEnumerable<Uri> GetDependencyEntityURIs(this DependencyTree dependencyTree, IWorkspaceDependencyRegistry dependencyService)
	{
		ISet<Uri> set = new HashSet<Uri>();
		GetDependencyEntityURIs(dependencyTree, dependencyService, set);
		return set;
	}

	private static void GetDependencyEntityURIs(DependencyTree dependencyTree, IWorkspaceDependencyRegistry dependencyService, ICollection<Uri> uriCollection)
	{
		Uri root = dependencyTree.Root;
		if (dependencyService.GetFileType(root) == FileType.Entity)
		{
			uriCollection.Add(root);
		}
		foreach (DependencyTree dependency in dependencyTree.Dependencies)
		{
			GetDependencyEntityURIs(dependency, dependencyService, uriCollection);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Firaxis.CivTech;

public class DependencyTree
{
	public Uri Root { get; set; }

	public IEnumerable<DependencyTree> Dependencies { get; set; }

	public IEnumerable<DependencyTree> Dependents { get; set; }

	public DependencyTree Parent { get; set; }

	public DependencyTree(Uri root)
	{
		Root = root;
		Dependencies = Enumerable.Empty<DependencyTree>();
		Dependents = Enumerable.Empty<DependencyTree>();
		Parent = null;
	}
}

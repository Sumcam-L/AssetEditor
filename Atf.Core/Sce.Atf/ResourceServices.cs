using System;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf;

public static class ResourceServices
{
	public static int GetResourceCount(this IResourceService resourceService)
	{
		return resourceService.Resources.Count();
	}

	public static IEnumerable<Uri> GetUris(this IResourceService resourceService)
	{
		foreach (IResource resource in resourceService.Resources)
		{
			yield return resource.Uri;
		}
	}
}

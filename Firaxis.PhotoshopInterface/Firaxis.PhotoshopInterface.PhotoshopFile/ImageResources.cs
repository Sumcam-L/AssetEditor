using System;
using System.Collections.Generic;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class ImageResources : List<ImageResource>
{
	public ImageResource Get(ResourceID id)
	{
		return Find((ImageResource x) => x.ID == id);
	}

	public void Set(ImageResource resource)
	{
		Predicate<ImageResource> match = (ImageResource res) => res.ID == resource.ID;
		int num = FindIndex(match);
		int num2 = FindLastIndex(match);
		if (num == -1)
		{
			Add(resource);
		}
		else if (num != num2)
		{
			RemoveAll(match);
			Insert(num, resource);
		}
		else
		{
			base[num] = resource;
		}
	}
}

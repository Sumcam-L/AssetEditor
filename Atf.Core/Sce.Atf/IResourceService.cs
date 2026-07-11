using System;
using System.Collections.Generic;

namespace Sce.Atf;

public interface IResourceService
{
	IEnumerable<IResource> Resources { get; }

	event EventHandler<ItemChangedEventArgs<IResource>> ResourceLoaded;

	event EventHandler<ItemChangedEventArgs<IResource>> ResourceUnloaded;

	IResource Load(Uri uri);

	IResource GetResource(Uri uri);

	bool Unload(Uri uri);
}

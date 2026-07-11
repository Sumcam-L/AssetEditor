using System;
using System.Collections.Generic;

namespace Sce.Atf;

public interface IResourceFolder
{
	IList<IResourceFolder> Folders { get; }

	IList<Uri> ResourceUris { get; }

	IResourceFolder Parent { get; }

	bool ReadOnlyName { get; }

	string Name { get; set; }

	IResourceFolder CreateFolder();
}

using System;

namespace Sce.Atf;

public interface IResource
{
	string Type { get; }

	Uri Uri { get; set; }

	event EventHandler<UriChangedEventArgs> UriChanged;
}

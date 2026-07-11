using System;

namespace Sce.Atf;

public class UriChangedEventArgs : EventArgs
{
	public readonly Uri OldUri;

	public readonly Uri NewUri;

	public UriChangedEventArgs(Uri oldUri, Uri newUri)
	{
		OldUri = oldUri;
		NewUri = newUri;
	}
}

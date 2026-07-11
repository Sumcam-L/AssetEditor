using System;

namespace Sce.Atf.Wpf.Applications;

public class ThumbnailReadyEventArgs : EventArgs
{
	public readonly Uri ResourceUri;

	public readonly object Image;

	public ThumbnailReadyEventArgs(Uri resourceUri, object image)
	{
		ResourceUri = resourceUri;
		Image = image;
	}
}

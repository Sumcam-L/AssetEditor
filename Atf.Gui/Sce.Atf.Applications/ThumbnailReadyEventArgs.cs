using System;
using System.Drawing;

namespace Sce.Atf.Applications;

public class ThumbnailReadyEventArgs : EventArgs
{
	public readonly Uri ResourceUri;

	public readonly Image Image;

	public ThumbnailReadyEventArgs(Uri resourceUri, Image image)
	{
		ResourceUri = resourceUri;
		Image = image;
	}
}

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;

namespace Sce.Atf.Applications;

[Export(typeof(ImageThumbnailResolver))]
[Export(typeof(IThumbnailResolver))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ImageThumbnailResolver : IThumbnailResolver, IInitializable
{
	public void Initialize()
	{
	}

	public Image Resolve(Uri resourceUri)
	{
		string resourcePath = ThumbnailService.GetResourcePath(resourceUri);
		if (resourcePath == null)
		{
			return null;
		}
		Bitmap result = null;
		string extension = Path.GetExtension(resourcePath);
		if (extension.EndsWith("jpg", StringComparison.InvariantCultureIgnoreCase) || extension.EndsWith("bmp", StringComparison.InvariantCultureIgnoreCase) || extension.EndsWith("png", StringComparison.InvariantCultureIgnoreCase) || extension.EndsWith("tif", StringComparison.InvariantCultureIgnoreCase) || extension.EndsWith("gif", StringComparison.InvariantCultureIgnoreCase))
		{
			using FileStream fileStream = File.OpenRead(resourcePath);
			result = new Bitmap(fileStream);
			fileStream.Close();
		}
		return result;
	}
}

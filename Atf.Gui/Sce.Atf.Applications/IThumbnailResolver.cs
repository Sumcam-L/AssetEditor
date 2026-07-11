using System;
using System.Drawing;

namespace Sce.Atf.Applications;

public interface IThumbnailResolver
{
	Image Resolve(Uri resourceUri);
}

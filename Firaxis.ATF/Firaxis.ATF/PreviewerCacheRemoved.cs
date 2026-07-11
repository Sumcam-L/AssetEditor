using System;

namespace Firaxis.ATF;

public class PreviewerCacheRemoved : EventArgs
{
	public readonly Uri Removed;

	public PreviewerCacheRemoved(Uri uri)
	{
		Removed = uri;
	}
}

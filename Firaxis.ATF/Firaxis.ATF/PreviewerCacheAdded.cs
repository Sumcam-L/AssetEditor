using System;

namespace Firaxis.ATF;

public class PreviewerCacheAdded : EventArgs
{
	public readonly Uri Added;

	public PreviewerCacheAdded(Uri uri)
	{
		Added = uri;
	}
}

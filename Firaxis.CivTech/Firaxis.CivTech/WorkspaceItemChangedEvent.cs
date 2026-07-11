using System;
using System.IO;

namespace Firaxis.CivTech;

public class WorkspaceItemChangedEvent : EventArgs
{
	public readonly Uri Uri;

	public readonly WatcherChangeTypes ChangeType;

	public WorkspaceItemChangedEvent(Uri uri, WatcherChangeTypes changeType)
	{
		Uri = uri;
		ChangeType = changeType;
	}
}

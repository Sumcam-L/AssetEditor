using System;
using System.IO;

namespace Firaxis.CivTech;

public class WorkspaceItemRenamedEvent : WorkspaceItemChangedEvent
{
	public readonly Uri OldUri;

	public WorkspaceItemRenamedEvent(Uri oldUri, Uri newUri, WatcherChangeTypes changeType)
		: base(newUri, changeType)
	{
		OldUri = oldUri;
	}
}

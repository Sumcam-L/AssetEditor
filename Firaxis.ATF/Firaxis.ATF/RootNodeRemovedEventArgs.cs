using System;

namespace Firaxis.ATF;

public class RootNodeRemovedEventArgs : EventArgs
{
	public readonly IFileWatchNode Node;

	public RootNodeRemovedEventArgs(IFileWatchNode node)
	{
		Node = node;
	}
}

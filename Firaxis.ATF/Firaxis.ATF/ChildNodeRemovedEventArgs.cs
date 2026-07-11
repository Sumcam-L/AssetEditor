using System;

namespace Firaxis.ATF;

public class ChildNodeRemovedEventArgs : EventArgs
{
	public readonly IFileWatchNode Node;

	public ChildNodeRemovedEventArgs(IFileWatchNode node)
	{
		Node = node;
	}
}

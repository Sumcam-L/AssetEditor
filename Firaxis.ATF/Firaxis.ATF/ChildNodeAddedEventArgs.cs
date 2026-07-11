using System;

namespace Firaxis.ATF;

public class ChildNodeAddedEventArgs : EventArgs
{
	public readonly IFileWatchNode Node;

	public ChildNodeAddedEventArgs(IFileWatchNode node)
	{
		Node = node;
	}
}

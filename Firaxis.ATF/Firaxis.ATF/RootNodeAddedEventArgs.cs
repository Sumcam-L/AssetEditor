using System;

namespace Firaxis.ATF;

public class RootNodeAddedEventArgs : EventArgs
{
	public readonly IFileWatchNode Node;

	public RootNodeAddedEventArgs(IFileWatchNode node)
	{
		Node = node;
	}
}

using System;
using System.Collections.Generic;

namespace Firaxis.ATF;

public interface IFileWatchTreeContext
{
	IEnumerable<IFileWatchNode> RootNodes { get; }

	event EventHandler<RootNodeAddedEventArgs> RootNodeAdded;

	event EventHandler<RootNodeRemovedEventArgs> RootNodeRemoved;
}

using System;
using System.Collections.Generic;

namespace Firaxis.ATF;

public interface IFileWatchNode
{
	IEnumerable<IFileWatchNode> Children { get; }

	Uri FileUri { get; }

	bool IsChecked { get; set; }

	FileLocation Location { get; }

	IFileWatchNode Parent { get; }

	event EventHandler CheckedChanged;

	event EventHandler<ChildNodeAddedEventArgs> ChildNodeAdded;

	event EventHandler<ChildNodeRemovedEventArgs> ChildNodeRemoved;

	IFileWatchNode AddChild(Uri childUri);

	void RemoveChild(Uri childUri);
}

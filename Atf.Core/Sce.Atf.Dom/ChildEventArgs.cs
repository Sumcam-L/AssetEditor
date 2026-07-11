using System;

namespace Sce.Atf.Dom;

public class ChildEventArgs : EventArgs
{
	public readonly DomNode Parent;

	public readonly ChildInfo ChildInfo;

	public readonly DomNode Child;

	public readonly int Index;

	public ChildEventArgs(DomNode parent, ChildInfo childInfo, DomNode child, int index)
	{
		Parent = parent;
		ChildInfo = childInfo;
		Child = child;
		Index = index;
	}
}

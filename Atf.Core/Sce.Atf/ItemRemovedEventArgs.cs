using System;

namespace Sce.Atf;

public class ItemRemovedEventArgs<T> : EventArgs
{
	public readonly int Index;

	public readonly T Item;

	public readonly T Parent;

	public ItemRemovedEventArgs(int index, T item)
		: this(index, item, default(T))
	{
	}

	public ItemRemovedEventArgs(int index, T item, T parent)
	{
		Index = index;
		Item = item;
		Parent = parent;
	}
}

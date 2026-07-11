using System;

namespace Sce.Atf;

public class ItemInsertedEventArgs<T> : EventArgs
{
	public readonly int Index;

	public readonly T Item;

	public readonly T Parent;

	public ItemInsertedEventArgs(int index, T item, T parent)
	{
		Index = index;
		Item = item;
		Parent = parent;
	}

	public ItemInsertedEventArgs(int index, T item)
		: this(index, item, default(T))
	{
	}
}

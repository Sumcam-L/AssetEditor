using System;

namespace Sce.Atf;

public class ItemChangedEventArgs<T> : EventArgs
{
	public readonly T Item;

	public readonly bool Reloaded;

	public ItemChangedEventArgs(T item, bool reloaded = false)
	{
		Item = item;
		Reloaded = reloaded;
	}
}

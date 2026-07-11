using System;

namespace Sce.Atf;

public class ItemSelectedEventArgs<T> : EventArgs
{
	public readonly T Item;

	public bool Selected;

	public ItemSelectedEventArgs(T item, bool selected)
	{
		Item = item;
		Selected = selected;
	}
}

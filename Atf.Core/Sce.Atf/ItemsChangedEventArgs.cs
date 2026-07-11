using System;
using System.Collections.Generic;

namespace Sce.Atf;

public class ItemsChangedEventArgs<T> : EventArgs
{
	public readonly IEnumerable<T> AddedItems;

	public readonly IEnumerable<T> RemovedItems;

	public readonly IEnumerable<T> ChangedItems;

	public ItemsChangedEventArgs(IEnumerable<T> addedItems, IEnumerable<T> removedItems, IEnumerable<T> changedItems)
	{
		AddedItems = addedItems;
		RemovedItems = removedItems;
		ChangedItems = changedItems;
	}
}

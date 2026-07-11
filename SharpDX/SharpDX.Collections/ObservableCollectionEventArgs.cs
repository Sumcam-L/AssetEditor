using System;

namespace SharpDX.Collections;

public class ObservableCollectionEventArgs<T> : EventArgs
{
	public T Item { get; private set; }

	public ObservableCollectionEventArgs(T item)
	{
		Item = item;
	}
}

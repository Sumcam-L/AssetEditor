using System;
using System.Collections.ObjectModel;

namespace SharpDX.Collections;

public class ObservableCollection<T> : Collection<T>
{
	public event EventHandler<ObservableCollectionEventArgs<T>> ItemAdded;

	public event EventHandler<ObservableCollectionEventArgs<T>> ItemRemoved;

	protected override void ClearItems()
	{
		for (int i = 0; i < base.Count; i++)
		{
			OnComponentRemoved(new ObservableCollectionEventArgs<T>(base[i]));
		}
		base.ClearItems();
	}

	protected override void InsertItem(int index, T item)
	{
		if (Contains(item))
		{
			throw new ArgumentException("This item is already added");
		}
		base.InsertItem(index, item);
		if (item != null)
		{
			OnComponentAdded(new ObservableCollectionEventArgs<T>(item));
		}
	}

	protected override void RemoveItem(int index)
	{
		T val = base[index];
		base.RemoveItem(index);
		if (val != null)
		{
			OnComponentRemoved(new ObservableCollectionEventArgs<T>(val));
		}
	}

	protected override void SetItem(int index, T item)
	{
		throw new NotSupportedException("Cannot set item into this instance");
	}

	private void OnComponentAdded(ObservableCollectionEventArgs<T> e)
	{
		this.ItemAdded?.Invoke(this, e);
	}

	private void OnComponentRemoved(ObservableCollectionEventArgs<T> e)
	{
		this.ItemRemoved?.Invoke(this, e);
	}
}

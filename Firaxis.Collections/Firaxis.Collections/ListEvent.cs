using System;
using System.Collections.Generic;

namespace Firaxis.Collections;

public class ListEvent<T> : List<T>
{
	public class ListEventArgs : EventArgs
	{
		public T Item { get; private set; }

		public IEnumerable<T> Collection { get; private set; }

		public ListEventArgs()
		{
		}

		public ListEventArgs(T item)
		{
			Item = item;
		}

		public ListEventArgs(IEnumerable<T> collection)
		{
			Collection = collection;
		}
	}

	public delegate void ListEventHandler(object sender, ListEventArgs e);

	public object LockObject { get; private set; }

	private int SuspendedCount { get; set; }

	public event ListEventHandler AddedItem;

	public event ListEventHandler RemovedItem;

	public event ListEventHandler ClearedItems;

	public event ListEventHandler ItemCountChanging;

	public event ListEventHandler ItemCountChanged;

	private void OnAddedItem(IEnumerable<T> items)
	{
		this.AddedItem?.Invoke(this, new ListEventArgs(items));
	}

	private void OnAddedItem(T item)
	{
		this.AddedItem?.Invoke(this, new ListEventArgs(item));
	}

	private void OnRemovedItem(IEnumerable<T> items)
	{
		this.RemovedItem?.Invoke(this, new ListEventArgs(items));
	}

	private void OnRemovedItem(T item)
	{
		this.RemovedItem?.Invoke(this, new ListEventArgs(item));
	}

	private void OnClearedItems()
	{
		this.ClearedItems?.Invoke(this, new ListEventArgs());
	}

	private void OnItemCountChanging()
	{
		if (SuspendedCount <= 0)
		{
			this.ItemCountChanging?.Invoke(this, new ListEventArgs());
		}
	}

	private void OnItemCountChanged()
	{
		if (SuspendedCount <= 0)
		{
			this.ItemCountChanged?.Invoke(this, new ListEventArgs());
		}
	}

	public ListEvent()
	{
		LockObject = new object();
	}

	public ListEvent(IEnumerable<T> collection)
		: base(collection)
	{
		LockObject = new object();
	}

	public ListEvent(int capacity)
		: base(capacity)
	{
		LockObject = new object();
	}

	public void SuspendNotify()
	{
		int suspendedCount = SuspendedCount + 1;
		SuspendedCount = suspendedCount;
	}

	public void ResumeNotify()
	{
		int suspendedCount = SuspendedCount - 1;
		SuspendedCount = suspendedCount;
	}

	public new void InsertRange(int index, IEnumerable<T> collection)
	{
		lock (LockObject)
		{
			OnItemCountChanging();
			base.InsertRange(index, collection);
			OnAddedItem(collection);
			OnItemCountChanged();
		}
	}

	public new void AddRange(IEnumerable<T> collection)
	{
		lock (LockObject)
		{
			OnItemCountChanging();
			base.AddRange(collection);
			OnAddedItem(collection);
			OnItemCountChanged();
		}
	}

	public new void Insert(int index, T item)
	{
		lock (LockObject)
		{
			OnItemCountChanging();
			base.Insert(index, item);
			OnAddedItem(item);
			OnItemCountChanged();
		}
	}

	public new void Add(T item)
	{
		lock (LockObject)
		{
			OnItemCountChanging();
			base.Add(item);
			OnAddedItem(item);
			OnItemCountChanged();
		}
	}

	public new void Remove(T item)
	{
		lock (LockObject)
		{
			OnItemCountChanging();
			base.Remove(item);
			OnRemovedItem(item);
			OnItemCountChanged();
		}
	}

	public new void RemoveRange(int index, int count)
	{
		lock (LockObject)
		{
			OnItemCountChanging();
			IEnumerable<T> range = GetRange(index, count);
			base.RemoveRange(index, count);
			OnRemovedItem(range);
			OnItemCountChanged();
		}
	}

	public new void Clear()
	{
		lock (LockObject)
		{
			if (base.Count != 0)
			{
				OnItemCountChanging();
				base.Clear();
				OnClearedItems();
				OnItemCountChanged();
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;

namespace Sce.Atf;

public class ActiveCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable where T : class
{
	private readonly List<T> m_list = new List<T>();

	private int m_maximumCount;

	public int MaximumCount
	{
		get
		{
			return m_maximumCount;
		}
		set
		{
			while (m_list.Count > value)
			{
				Remove(m_list[0]);
			}
			m_maximumCount = value;
		}
	}

	public virtual T ActiveItem
	{
		get
		{
			return (m_list.Count > 0) ? m_list[m_list.Count - 1] : null;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			T activeItem = ActiveItem;
			if (value == activeItem)
			{
				return;
			}
			OnActiveItemChanging(EventArgs.Empty);
			int num = m_list.IndexOf(value);
			if (num >= 0)
			{
				m_list.RemoveAt(num);
				m_list.Add(value);
			}
			else
			{
				if (m_list.Count == m_maximumCount)
				{
					Remove(m_list[0]);
				}
				m_list.Add(value);
				OnItemAdded(new ItemInsertedEventArgs<T>(0, value));
			}
			OnActiveItemChanged(EventArgs.Empty);
		}
	}

	public IEnumerable<T> MostRecentOrder
	{
		get
		{
			for (int i = m_list.Count - 1; i >= 0; i--)
			{
				yield return m_list[i];
			}
		}
	}

	public int Count => m_list.Count;

	public bool IsReadOnly => false;

	protected internal IList<T> RawList => m_list;

	public event EventHandler ActiveItemChanging;

	public event EventHandler ActiveItemChanged;

	public event EventHandler<ItemInsertedEventArgs<T>> ItemAdded;

	public event EventHandler<ItemRemovedEventArgs<T>> ItemRemoved;

	public ActiveCollection()
		: this(int.MaxValue)
	{
	}

	public ActiveCollection(int maximumCount)
	{
		if (maximumCount <= 0)
		{
			throw new ArgumentException("maximumCount must be > 0");
		}
		m_maximumCount = maximumCount;
	}

	public T[] GetSnapshot()
	{
		return m_list.ToArray();
	}

	public U GetActiveItem<U>() where U : class
	{
		for (int num = m_list.Count - 1; num >= 0; num--)
		{
			U val = Convert<U>(m_list[num]);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	public IEnumerable<U> AsIEnumerable<U>() where U : class
	{
		using IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T item = enumerator.Current;
			U u = Convert<U>(item);
			if (u != null)
			{
				yield return u;
			}
		}
	}

	public U[] GetSnapshot<U>() where U : class
	{
		List<U> list = new List<U>();
		using (IEnumerator<T> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				U val = Convert<U>(current);
				if (val != null)
				{
					list.Add(val);
				}
			}
		}
		return list.ToArray();
	}

	protected virtual U Convert<U>(T item) where U : class
	{
		return item as U;
	}

	protected virtual void OnActiveItemChanging(EventArgs e)
	{
		this.ActiveItemChanging.Raise(this, e);
	}

	protected virtual void OnActiveItemChanged(EventArgs e)
	{
		this.ActiveItemChanged.Raise(this, e);
	}

	protected virtual void OnItemAdded(ItemInsertedEventArgs<T> e)
	{
		this.ItemAdded.Raise(this, e);
	}

	public void Set(IEnumerable<T> items)
	{
		object activeItem = ActiveItem;
		ClearInternal();
		int num = 0;
		foreach (T item in items)
		{
			m_list.Add(item);
			OnItemAdded(new ItemInsertedEventArgs<T>(num++, item));
		}
		if (activeItem != ActiveItem)
		{
			OnActiveItemChanged(EventArgs.Empty);
		}
	}

	public bool Remove(T item)
	{
		object activeItem = ActiveItem;
		int num = m_list.IndexOf(item);
		if (num >= 0)
		{
			m_list.RemoveAt(num);
			OnItemRemoved(new ItemRemovedEventArgs<T>(num, item));
			if (activeItem != ActiveItem)
			{
				OnActiveItemChanged(EventArgs.Empty);
			}
			return true;
		}
		return false;
	}

	protected virtual void OnItemRemoved(ItemRemovedEventArgs<T> e)
	{
		this.ItemRemoved.Raise(this, e);
	}

	public void Clear()
	{
		object activeItem = ActiveItem;
		ClearInternal();
		if (activeItem != ActiveItem)
		{
			OnActiveItemChanged(EventArgs.Empty);
		}
	}

	private void ClearInternal()
	{
		for (int num = m_list.Count - 1; num >= 0; num--)
		{
			T item = m_list[num];
			m_list.RemoveAt(num);
			OnItemRemoved(new ItemRemovedEventArgs<T>(num, item));
		}
	}

	public void Add(T item)
	{
		ActiveItem = item;
	}

	public bool Contains(T item)
	{
		return m_list.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		m_list.CopyTo(array, arrayIndex);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return m_list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

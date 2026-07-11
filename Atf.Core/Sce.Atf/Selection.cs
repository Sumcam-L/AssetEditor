using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf;

public class Selection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	private readonly List<T> m_list = new List<T>();

	private readonly HashSet<T> m_set = new HashSet<T>();

	private bool m_updating;

	public T LastSelected => (m_list.Count == 0) ? default(T) : m_list[m_list.Count - 1];

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

	public T this[int index]
	{
		get
		{
			return m_list[index];
		}
		set
		{
			T item = m_list[index];
			if (!item.Equals(value))
			{
				RaiseChanging();
				m_list[index] = value;
				m_set.Remove(item);
				if (m_set.Contains(value))
				{
					m_list.Remove(value);
				}
				else
				{
					m_set.Add(value);
				}
				RaiseChanged();
			}
		}
	}

	public int Count => m_list.Count;

	public bool IsReadOnly => false;

	public event EventHandler Changing;

	public event EventHandler Changed;

	public event EventHandler<ItemsChangedEventArgs<T>> ItemsChanged;

	public T[] GetSnapshot()
	{
		return m_list.ToArray();
	}

	public IEnumerable<U> AsIEnumerable<U>() where U : class
	{
		using IEnumerator<T> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			T t = enumerator.Current;
			U u = Convert<U>(t);
			if (u != null)
			{
				yield return u;
			}
		}
	}

	public U GetLastSelected<U>() where U : class
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			U val = Convert<U>(this[num]);
			if (val != null)
			{
				return val;
			}
		}
		return null;
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

	public void Set(T item)
	{
		SetRange(new T[1] { item });
	}

	public void SetRange(IEnumerable<T> items)
	{
		if (Equals(items))
		{
			return;
		}
		RaiseChanging();
		List<T> list = null;
		List<T> list2 = null;
		EventHandler<ItemsChangedEventArgs<T>> eventHandler = this.ItemsChanged;
		if (eventHandler != null)
		{
			list = new List<T>();
			list2 = new List<T>();
			HashSet<T> hashSet = new HashSet<T>(items);
			foreach (T item in m_list)
			{
				if (!hashSet.Contains(item))
				{
					list2.Add(item);
				}
			}
			foreach (T item2 in items)
			{
				if (!m_set.Contains(item2))
				{
					list.Add(item2);
				}
			}
		}
		m_list.Clear();
		m_set.Clear();
		foreach (T item3 in items)
		{
			m_list.Add(item3);
			m_set.Add(item3);
		}
		eventHandler?.Invoke(this, new ItemsChangedEventArgs<T>(list, list2, EmptyEnumerable<T>.Instance));
		RaiseChanged();
	}

	public void Add(T item)
	{
		AddRange(new T[1] { item });
	}

	public void AddRange(IEnumerable<T> items)
	{
		List<T> list = new List<T>();
		HashSet<T> hashSet = new HashSet<T>(items);
		foreach (T item in m_list)
		{
			if (!hashSet.Contains(item))
			{
				list.Add(item);
			}
		}
		list.AddRange(items);
		SetRange(list);
	}

	public bool Remove(T item)
	{
		return RemoveRange(new T[1] { item });
	}

	public bool RemoveRange(IEnumerable<T> items)
	{
		HashSet<T> hashSet = new HashSet<T>();
		foreach (T item in items)
		{
			hashSet.Add(item);
		}
		bool result = false;
		List<T> list = new List<T>();
		foreach (T item2 in m_list)
		{
			if (!hashSet.Contains(item2))
			{
				list.Add(item2);
			}
			else
			{
				result = true;
			}
		}
		SetRange(list);
		return result;
	}

	public void Toggle(T item)
	{
		ToggleRange(new T[1] { item });
	}

	public void ToggleRange(IEnumerable<T> items)
	{
		HashSet<T> hashSet = new HashSet<T>();
		foreach (T item in items)
		{
			hashSet.Add(item);
		}
		List<T> list = new List<T>();
		foreach (T item2 in m_list)
		{
			if (!hashSet.Contains(item2))
			{
				list.Add(item2);
			}
		}
		foreach (T item3 in items)
		{
			if (!Contains(item3))
			{
				list.Add(item3);
			}
		}
		SetRange(list);
	}

	public bool Equals(ICollection<T> collection)
	{
		if (this == collection)
		{
			return true;
		}
		if (m_list.Count != collection.Count)
		{
			return false;
		}
		return this.SequenceEqual(collection);
	}

	public override bool Equals(object obj)
	{
		if (obj is ICollection<T> collection)
		{
			return Equals(collection);
		}
		if (obj is IEnumerable<T> second)
		{
			return this.SequenceEqual(second);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 0;
		foreach (T item in m_list)
		{
			num ^= item.GetHashCode();
		}
		return num;
	}

	public void BeginUpdate()
	{
		if (m_updating)
		{
			throw new InvalidOperationException("Can't nest updates");
		}
		RaiseChanging();
		m_updating = true;
	}

	public void EndUpdate()
	{
		if (!m_updating)
		{
			throw new InvalidOperationException("Not updating");
		}
		m_updating = false;
		RaiseChanged();
	}

	public int IndexOf(T item)
	{
		if (!m_set.Contains(item))
		{
			return -1;
		}
		return m_list.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		RaiseChanging();
		if (m_set.Contains(item))
		{
			int num = m_list.IndexOf(item);
			m_list.RemoveAt(num);
			if (index > num)
			{
				index--;
			}
		}
		m_list.Insert(index, item);
		m_set.Add(item);
		RaiseChanged();
	}

	public void RemoveAt(int index)
	{
		T item = m_list[index];
		RaiseChanging();
		m_list.RemoveAt(index);
		m_set.Remove(item);
		RaiseChanged();
	}

	public bool Contains(T item)
	{
		return m_set.Contains(item);
	}

	public void Clear()
	{
		SetRange(EmptyEnumerable<T>.Instance);
	}

	public void CopyTo(T[] array, int index)
	{
		m_list.CopyTo(array, index);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return m_list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_list.GetEnumerator();
	}

	protected virtual void OnChanging(EventArgs e)
	{
		this.Changing.Raise(this, e);
	}

	private void RaiseChanging()
	{
		if (!m_updating)
		{
			OnChanging(EventArgs.Empty);
		}
	}

	protected virtual void OnChanged(EventArgs e)
	{
		this.Changed.Raise(this, e);
	}

	private void RaiseChanged()
	{
		if (!m_updating)
		{
			OnChanged(EventArgs.Empty);
		}
	}
}

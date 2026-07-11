using System;
using System.Collections;
using System.Collections.Generic;

namespace Sce.Atf;

public class CollectionAdapter<T, U> : ICollection<U>, IEnumerable<U>, IEnumerable where T : class where U : class
{
	private readonly ICollection<T> m_collection;

	public int Count => m_collection.Count;

	public bool IsReadOnly => m_collection.IsReadOnly;

	public CollectionAdapter(ICollection<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		m_collection = list;
	}

	public void Add(U item)
	{
		if (m_collection.IsReadOnly)
		{
			throw new InvalidOperationException("Collection is read only");
		}
		T item2 = Convert(item);
		m_collection.Add(item2);
	}

	public void Clear()
	{
		if (m_collection.IsReadOnly)
		{
			throw new InvalidOperationException("Collection is read only");
		}
		m_collection.Clear();
	}

	public bool Contains(U item)
	{
		T item2 = Convert(item);
		return m_collection.Contains(item2);
	}

	public void CopyTo(U[] array, int arrayIndex)
	{
		int num = 0;
		foreach (T item in m_collection)
		{
			array[arrayIndex + num] = Convert(item);
			num++;
		}
	}

	public bool Remove(U item)
	{
		if (m_collection.IsReadOnly)
		{
			throw new InvalidOperationException("Collection is read only");
		}
		T item2 = Convert(item);
		return m_collection.Remove(item2);
	}

	public IEnumerator<U> GetEnumerator()
	{
		foreach (T t in m_collection)
		{
			yield return Convert(t);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	protected virtual T Convert(U item)
	{
		T val = item as T;
		if (val == null && item != null)
		{
			throw new InvalidOperationException("Item of wrong type for adapted collection");
		}
		return val;
	}

	protected virtual U Convert(T item)
	{
		return item as U;
	}
}

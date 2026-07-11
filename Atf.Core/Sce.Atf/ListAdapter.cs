using System;
using System.Collections;
using System.Collections.Generic;

namespace Sce.Atf;

public class ListAdapter<T, U> : CollectionAdapter<T, U>, IList<U>, ICollection<U>, IEnumerable<U>, IEnumerable where T : class where U : class
{
	private readonly IList<T> m_list;

	public U this[int index]
	{
		get
		{
			return Convert(m_list[index]);
		}
		set
		{
			if (m_list.IsReadOnly)
			{
				throw new InvalidOperationException("Collection is read only");
			}
			T value2 = Convert(value);
			m_list[index] = value2;
		}
	}

	public ListAdapter(IList<T> list)
		: base((ICollection<T>)list)
	{
		m_list = list;
	}

	public int IndexOf(U item)
	{
		T item2 = Convert(item);
		return m_list.IndexOf(item2);
	}

	public void Insert(int index, U item)
	{
		if (m_list.IsReadOnly)
		{
			throw new InvalidOperationException("Collection is read only");
		}
		T item2 = Convert(item);
		m_list.Insert(index, item2);
	}

	public void RemoveAt(int index)
	{
		if (m_list.IsReadOnly)
		{
			throw new InvalidOperationException("Collection is read only");
		}
		m_list.RemoveAt(index);
	}
}

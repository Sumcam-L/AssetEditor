using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sce.Atf;

public class PropertyChangedEventArgsCollection : ICollection<PropertyChangedEventArgs>, IEnumerable<PropertyChangedEventArgs>, IEnumerable
{
	private class PropertyChangedEventArgsComparer : IEqualityComparer<PropertyChangedEventArgs>
	{
		public bool Equals(PropertyChangedEventArgs x, PropertyChangedEventArgs y)
		{
			return x.PropertyName.Equals(y.PropertyName);
		}

		public int GetHashCode(PropertyChangedEventArgs obj)
		{
			return obj.PropertyName.GetHashCode();
		}
	}

	private readonly List<PropertyChangedEventArgs> m_innerList = new List<PropertyChangedEventArgs>();

	private static readonly PropertyChangedEventArgsComparer s_comparer = new PropertyChangedEventArgsComparer();

	public int Count => m_innerList.Count;

	public bool IsReadOnly => false;

	public void Add(PropertyChangedEventArgs item)
	{
		if (!Contains(item))
		{
			m_innerList.Add(item);
		}
	}

	public void Clear()
	{
		m_innerList.Clear();
	}

	public bool Contains(PropertyChangedEventArgs item)
	{
		return m_innerList.Contains(item, s_comparer);
	}

	public void CopyTo(PropertyChangedEventArgs[] array, int arrayIndex)
	{
		m_innerList.CopyTo(array, arrayIndex);
	}

	public bool Remove(PropertyChangedEventArgs item)
	{
		return m_innerList.Remove(item);
	}

	public IEnumerator<PropertyChangedEventArgs> GetEnumerator()
	{
		return m_innerList.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_innerList.GetEnumerator();
	}
}

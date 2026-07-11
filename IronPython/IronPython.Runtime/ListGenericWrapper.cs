using System.Collections;
using System.Collections.Generic;

namespace IronPython.Runtime;

public class ListGenericWrapper<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	private IList<object> _value;

	public T this[int index]
	{
		get
		{
			return (T)_value[index];
		}
		set
		{
			_value[index] = value;
		}
	}

	public int Count => _value.Count;

	public bool IsReadOnly => _value.IsReadOnly;

	public ListGenericWrapper(IList<object> value)
	{
		_value = value;
	}

	public int IndexOf(T item)
	{
		return _value.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		_value.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		_value.RemoveAt(index);
	}

	public void Add(T item)
	{
		_value.Add(item);
	}

	public void Clear()
	{
		_value.Clear();
	}

	public bool Contains(T item)
	{
		return _value.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		for (int i = 0; i < _value.Count; i++)
		{
			array[arrayIndex + i] = (T)_value[i];
		}
	}

	public bool Remove(T item)
	{
		return _value.Remove(item);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return new IEnumeratorOfTWrapper<T>(_value.GetEnumerator());
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _value.GetEnumerator();
	}
}

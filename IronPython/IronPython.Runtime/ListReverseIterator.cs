using System;
using System.Collections;
using System.Collections.Generic;

namespace IronPython.Runtime;

[PythonType("listreverseiterator")]
public sealed class ListReverseIterator : IEnumerable<object>, IEnumerable, IEnumerator<object>, IDisposable, IEnumerator
{
	private int _index;

	private readonly List _list;

	private bool _iterating;

	public object Current => _list._data[_list._size - _index];

	public ListReverseIterator(List l)
	{
		_list = l;
		Reset();
	}

	public void Reset()
	{
		_index = 0;
		_iterating = true;
	}

	public bool MoveNext()
	{
		if (_iterating)
		{
			_index++;
			_iterating = _index <= _list._size;
		}
		return _iterating;
	}

	public IEnumerator GetEnumerator()
	{
		return this;
	}

	public void Dispose()
	{
	}

	IEnumerator<object> IEnumerable<object>.GetEnumerator()
	{
		return this;
	}
}

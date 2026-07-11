using System;
using System.Collections;
using System.Collections.Generic;

namespace IronPython.Runtime;

[PythonType("listiterator")]
public sealed class ListIterator : IEnumerable<object>, IEnumerable, IEnumerator<object>, IDisposable, IEnumerator
{
	private int _index;

	private readonly List _list;

	private bool _iterating;

	public object Current => _list._data[_index];

	public ListIterator(List l)
	{
		_list = l;
		Reset();
	}

	public void Reset()
	{
		_index = -1;
		_iterating = true;
	}

	public bool MoveNext()
	{
		if (_iterating)
		{
			_index++;
			_iterating = _index < _list._size;
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

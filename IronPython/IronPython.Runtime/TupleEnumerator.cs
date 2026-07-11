using System;
using System.Collections;
using System.Collections.Generic;

namespace IronPython.Runtime;

[PythonType("tupleiterator")]
public sealed class TupleEnumerator : IEnumerable, IEnumerator<object>, IDisposable, IEnumerator
{
	private int _curIndex;

	private PythonTuple _tuple;

	public object Current => _tuple._data[_curIndex];

	public TupleEnumerator(PythonTuple t)
	{
		_tuple = t;
		_curIndex = -1;
	}

	public bool MoveNext()
	{
		if (_curIndex + 1 >= _tuple.Count)
		{
			return false;
		}
		_curIndex++;
		return true;
	}

	public void Reset()
	{
		_curIndex = -1;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public IEnumerator GetEnumerator()
	{
		return this;
	}
}

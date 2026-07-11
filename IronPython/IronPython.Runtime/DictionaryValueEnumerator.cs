using System;
using System.Collections;
using System.Collections.Generic;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

[PythonType("dictionary-valueiterator")]
public sealed class DictionaryValueEnumerator : IEnumerator<object>, IDisposable, IEnumerator
{
	private readonly int _size;

	private DictionaryStorage _dict;

	private readonly object[] _values;

	private int _pos;

	public object Current => _values[_pos];

	internal DictionaryValueEnumerator(DictionaryStorage dict)
	{
		_dict = dict;
		_size = dict.Count;
		_values = new object[_size];
		int num = 0;
		foreach (KeyValuePair<object, object> item in dict.GetItems())
		{
			_values[num++] = item.Value;
		}
		_pos = -1;
	}

	public bool MoveNext()
	{
		if (_size != _dict.Count)
		{
			_pos = _size - 1;
			throw PythonOps.RuntimeError("dictionary changed size during iteration");
		}
		if (_pos + 1 < _size)
		{
			_pos++;
			return true;
		}
		return false;
	}

	public void Reset()
	{
		_pos = -1;
	}

	public void Dispose()
	{
	}

	public object __iter__()
	{
		return this;
	}

	public int __len__()
	{
		return _size - _pos - 1;
	}
}

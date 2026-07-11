using System;
using System.Collections;
using System.Collections.Generic;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

[PythonType("dictionary-itemiterator")]
public sealed class DictionaryItemEnumerator : IEnumerator<object>, IDisposable, IEnumerator
{
	private readonly int _size;

	private readonly DictionaryStorage _dict;

	private readonly List<object> _keys;

	private readonly List<object> _values;

	private int _pos;

	public object Current => PythonOps.MakeTuple(_keys[_pos], _values[_pos]);

	internal DictionaryItemEnumerator(DictionaryStorage dict)
	{
		_dict = dict;
		_keys = new List<object>(dict.Count);
		_values = new List<object>(dict.Count);
		foreach (KeyValuePair<object, object> item in dict.GetItems())
		{
			_keys.Add(item.Key);
			_values.Add(item.Value);
		}
		_size = _values.Count;
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

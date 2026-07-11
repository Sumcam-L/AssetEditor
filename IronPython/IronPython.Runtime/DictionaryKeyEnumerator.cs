using System;
using System.Collections;
using System.Collections.Generic;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

[PythonType("dictionary-keyiterator")]
public sealed class DictionaryKeyEnumerator : IEnumerator<object>, IDisposable, IEnumerator
{
	private readonly int _size;

	private readonly DictionaryStorage _dict;

	private readonly IEnumerator<object> _keys;

	private int _pos;

	object IEnumerator.Current => _keys.Current;

	object IEnumerator<object>.Current => _keys.Current;

	internal DictionaryKeyEnumerator(DictionaryStorage dict)
	{
		_dict = dict;
		_size = dict.Count;
		_keys = dict.GetKeys().GetEnumerator();
		_pos = -1;
	}

	bool IEnumerator.MoveNext()
	{
		if (_size != _dict.Count)
		{
			_pos = _size - 1;
			throw PythonOps.RuntimeError("dictionary changed size during iteration");
		}
		if (_keys.MoveNext())
		{
			_pos++;
			return true;
		}
		return false;
	}

	void IEnumerator.Reset()
	{
		_keys.Reset();
		_pos = -1;
	}

	void IDisposable.Dispose()
	{
	}

	public object __iter__()
	{
		return this;
	}

	public int __length_hint__()
	{
		return _size - _pos - 1;
	}
}

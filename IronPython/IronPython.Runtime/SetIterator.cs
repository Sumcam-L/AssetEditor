using System;
using System.Collections;
using System.Collections.Generic;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

[PythonType("setiterator")]
public sealed class SetIterator : IEnumerable<object>, IEnumerable, IEnumerator<object>, IDisposable, IEnumerator
{
	private readonly SetStorage _items;

	private readonly int _version;

	private readonly int _maxIndex;

	private int _index = -2;

	public object Current
	{
		[PythonHidden]
		get
		{
			if (_index < 0)
			{
				return null;
			}
			object item = _items._buckets[_index].Item;
			if (_items.Version != _version)
			{
				throw PythonOps.RuntimeError("set changed during iteration");
			}
			return item;
		}
	}

	internal SetIterator(SetStorage items, bool mutable)
	{
		_items = items;
		if (mutable)
		{
			lock (items)
			{
				_version = items.Version;
				_maxIndex = ((items._count > 0) ? items._buckets.Length : 0);
				return;
			}
		}
		_version = items.Version;
		_maxIndex = ((items._count > 0) ? items._buckets.Length : 0);
	}

	[PythonHidden]
	public void Dispose()
	{
	}

	[PythonHidden]
	public bool MoveNext()
	{
		if (_index == _maxIndex)
		{
			return false;
		}
		_index++;
		if (_index < 0)
		{
			if (_items._hasNull)
			{
				return true;
			}
			_index++;
		}
		if (_maxIndex > 0)
		{
			SetStorage.Bucket[] buckets = _items._buckets;
			while (_index < buckets.Length)
			{
				object item = buckets[_index].Item;
				if (item != null && item != SetStorage.Removed)
				{
					return true;
				}
				_index++;
			}
		}
		return false;
	}

	[PythonHidden]
	public void Reset()
	{
		_index = -2;
	}

	[PythonHidden]
	public IEnumerator GetEnumerator()
	{
		return this;
	}

	IEnumerator<object> IEnumerable<object>.GetEnumerator()
	{
		return this;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[DontMapIEnumerableToContains]
[PythonType("xrange")]
public sealed class XRange : IEnumerable<int>, ICodeFormattable, IList, ICollection, IEnumerable, IReversible
{
	private int _start;

	private int _stop;

	private int _step;

	private int _length;

	public int Start
	{
		[PythonHidden]
		get
		{
			return _start;
		}
	}

	public int Stop
	{
		[PythonHidden]
		get
		{
			return _stop;
		}
	}

	public int Step
	{
		[PythonHidden]
		get
		{
			return _step;
		}
	}

	public object this[int index]
	{
		get
		{
			if (index < 0)
			{
				index += _length;
			}
			if (index >= _length || index < 0)
			{
				throw PythonOps.IndexError("xrange object index out of range");
			}
			int value = index * _step + _start;
			return ScriptingRuntimeHelpers.Int32ToObject(value);
		}
	}

	public object this[object index] => this[Converter.ConvertToIndex(index)];

	public object this[Slice slice]
	{
		get
		{
			throw PythonOps.TypeError("sequence index must be integer");
		}
	}

	int ICollection.Count => _length;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => null;

	bool IList.IsFixedSize => true;

	bool IList.IsReadOnly => true;

	object IList.this[int index]
	{
		get
		{
			int num = 0;
			foreach (int item in (IEnumerable<int>)this)
			{
				object result = item;
				if (num == index)
				{
					return result;
				}
				num++;
			}
			throw new IndexOutOfRangeException();
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public XRange(int stop)
		: this(0, stop, 1)
	{
	}

	public XRange(int start, int stop)
		: this(start, stop, 1)
	{
	}

	public XRange(int start, int stop, int step)
	{
		Initialize(start, stop, step);
	}

	private void Initialize(int start, int stop, int step)
	{
		if (step == 0)
		{
			throw PythonOps.ValueError("step must not be zero");
		}
		if (step > 0)
		{
			if (start > stop)
			{
				stop = start;
			}
		}
		else if (start < stop)
		{
			stop = start;
		}
		_start = start;
		_stop = stop;
		_step = step;
		_length = GetLengthHelper();
		_stop = start + step * _length;
	}

	public int __len__()
	{
		return _length;
	}

	private int GetLengthHelper()
	{
		long num = ((_step <= 0) ? (((long)_stop - (long)_start + _step + 1) / _step) : (((long)_stop - (long)_start + _step - 1) / _step));
		if (num > int.MaxValue)
		{
			throw PythonOps.OverflowError("xrange() result has too many items");
		}
		return (int)num;
	}

	public IEnumerator __reversed__()
	{
		return new XRangeIterator(new XRange(_stop - _step, _start - _step, -_step));
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new XRangeIterator(this);
	}

	IEnumerator<int> IEnumerable<int>.GetEnumerator()
	{
		return new XRangeIterator(this);
	}

	public string __repr__(CodeContext context)
	{
		if (_step == 1)
		{
			if (_start == 0)
			{
				return $"xrange({_stop})";
			}
			return $"xrange({_start}, {_stop})";
		}
		return $"xrange({_start}, {_stop}, {_step})";
	}

	void ICollection.CopyTo(Array array, int index)
	{
		foreach (int item in (IEnumerable<int>)this)
		{
			object value = item;
			array.SetValue(value, index++);
		}
	}

	int IList.Add(object value)
	{
		throw new InvalidOperationException();
	}

	void IList.Clear()
	{
		throw new InvalidOperationException();
	}

	bool IList.Contains(object value)
	{
		return ((IList)this).IndexOf(value) != -1;
	}

	int IList.IndexOf(object value)
	{
		int num = 0;
		foreach (int item in (IEnumerable<int>)this)
		{
			object obj = item;
			if (obj == value)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	void IList.Insert(int index, object value)
	{
		throw new InvalidOperationException();
	}

	void IList.Remove(object value)
	{
		throw new InvalidOperationException();
	}

	void IList.RemoveAt(int index)
	{
		throw new InvalidOperationException();
	}
}

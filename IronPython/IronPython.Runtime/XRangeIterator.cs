using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("rangeiterator")]
public sealed class XRangeIterator : IEnumerable, IEnumerator<int>, IDisposable, IEnumerator
{
	private XRange _xrange;

	private int _value;

	private int _position;

	public object Current => ScriptingRuntimeHelpers.Int32ToObject(_value);

	int IEnumerator<int>.Current => _value;

	public XRangeIterator(XRange xrange)
	{
		_xrange = xrange;
		_value = xrange.Start - xrange.Step;
	}

	public bool MoveNext()
	{
		if (_position >= _xrange.__len__())
		{
			return false;
		}
		_position++;
		_value += _xrange.Step;
		return true;
	}

	public void Reset()
	{
		_value = _xrange.Start - _xrange.Step;
		_position = 0;
	}

	public void Dispose()
	{
	}

	public IEnumerator GetEnumerator()
	{
		return this;
	}
}

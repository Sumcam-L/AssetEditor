using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[Documentation("enumerate(iterable) -> iterator for index, value of iterable")]
[DontMapIEnumerableToContains]
[PythonType("enumerate")]
[DontMapIDisposableToContextManager]
public class Enumerate : IEnumerator<object>, IDisposable, IEnumerator
{
	private readonly IEnumerator _iter;

	private object _index;

	object IEnumerator.Current => PythonTuple.MakeTuple(_index, _iter.Current);

	object IEnumerator<object>.Current => ((IEnumerator)this).Current;

	public Enumerate(object iter)
	{
		_iter = PythonOps.GetEnumerator(iter);
		_index = ScriptingRuntimeHelpers.Int32ToObject(-1);
	}

	public Enumerate(CodeContext context, object iter, object start)
	{
		if (!Converter.TryConvertToIndex(start, out object index))
		{
			throw PythonOps.TypeErrorForUnIndexableObject(start);
		}
		_iter = PythonOps.GetEnumerator(iter);
		_index = context.LanguageContext.Operation(PythonOperationKind.Subtract, index, ScriptingRuntimeHelpers.Int32ToObject(1));
	}

	public object __iter__()
	{
		return this;
	}

	void IEnumerator.Reset()
	{
		throw new NotImplementedException();
	}

	bool IEnumerator.MoveNext()
	{
		if (_index is int)
		{
			int num = (int)_index;
			if (num != int.MaxValue)
			{
				_index = ScriptingRuntimeHelpers.Int32ToObject(num + 1);
			}
			else
			{
				_index = new BigInteger(int.MaxValue) + 1;
			}
		}
		else
		{
			_index = (BigInteger)_index + 1;
		}
		return _iter.MoveNext();
	}

	void IDisposable.Dispose()
	{
		Dispose(notFinalizing: true);
		GC.SuppressFinalize(this);
	}

	[PythonHidden]
	protected virtual void Dispose(bool notFinalizing)
	{
	}
}

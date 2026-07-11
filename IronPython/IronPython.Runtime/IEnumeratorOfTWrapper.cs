using System;
using System.Collections;
using System.Collections.Generic;

namespace IronPython.Runtime;

public class IEnumeratorOfTWrapper<T> : IEnumerator<T>, IDisposable, IEnumerator
{
	private IEnumerator enumerable;

	public T Current => (T)enumerable.Current;

	object IEnumerator.Current => enumerable.Current;

	public IEnumeratorOfTWrapper(IEnumerator enumerable)
	{
		this.enumerable = enumerable;
	}

	public void Dispose()
	{
	}

	public bool MoveNext()
	{
		return enumerable.MoveNext();
	}

	public void Reset()
	{
		enumerable.Reset();
	}
}

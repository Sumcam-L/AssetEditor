using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

[PythonType("SentinelIterator")]
public sealed class SentinelIterator : IEnumerator<object>, IDisposable, IEnumerator
{
	private readonly object _target;

	private readonly object _sentinel;

	private readonly CodeContext _context;

	private readonly CallSite<Func<CallSite, CodeContext, object, object>> _site;

	private object _current;

	private bool _sinkState;

	object IEnumerator.Current => _current;

	object IEnumerator<object>.Current => _current;

	public SentinelIterator(CodeContext context, object target, object sentinel)
	{
		_target = target;
		_sentinel = sentinel;
		_context = context;
		_site = CallSite<Func<CallSite, CodeContext, object, object>>.Create(_context.LanguageContext.InvokeOne);
	}

	public object __iter__()
	{
		return this;
	}

	public object next()
	{
		if (((IEnumerator)this).MoveNext())
		{
			return ((IEnumerator)this).Current;
		}
		throw PythonOps.StopIteration();
	}

	bool IEnumerator.MoveNext()
	{
		if (_sinkState)
		{
			return false;
		}
		_current = _site.Target(_site, _context, _target);
		bool flag = _sentinel == _current || PythonOps.EqualRetBool(_context, _sentinel, _current);
		if (flag)
		{
			_sinkState = true;
		}
		return !flag;
	}

	void IEnumerator.Reset()
	{
		throw new NotImplementedException();
	}

	void IDisposable.Dispose()
	{
	}
}

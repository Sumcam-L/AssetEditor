using System;
using System.Collections;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Exceptions;

namespace IronPython.Runtime;

[PythonType("iterator")]
public class ItemEnumerator : IEnumerator
{
	private readonly object _getItemMethod;

	private readonly CallSite<Func<CallSite, CodeContext, object, int, object>> _site;

	private object _current;

	private int _index;

	object IEnumerator.Current => _current;

	internal ItemEnumerator(object getItemMethod, CallSite<Func<CallSite, CodeContext, object, int, object>> site)
	{
		_getItemMethod = getItemMethod;
		_site = site;
	}

	bool IEnumerator.MoveNext()
	{
		if (_index < 0)
		{
			return false;
		}
		try
		{
			_current = _site.Target(_site, DefaultContext.Default, _getItemMethod, _index);
			_index++;
			return true;
		}
		catch (IndexOutOfRangeException)
		{
			_current = null;
			_index = -1;
			return false;
		}
		catch (StopIterationException)
		{
			_current = null;
			_index = -1;
			return false;
		}
	}

	void IEnumerator.Reset()
	{
		_index = 0;
		_current = null;
	}
}

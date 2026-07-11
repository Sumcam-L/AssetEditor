using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace IronPython.Runtime;

[PythonType("iterable")]
public class ItemEnumerable : IEnumerable
{
	private readonly object _getitem;

	private readonly CallSite<Func<CallSite, CodeContext, object, int, object>> _site;

	internal ItemEnumerable(object getitem, CallSite<Func<CallSite, CodeContext, object, int, object>> site)
	{
		_getitem = getitem;
		_site = site;
	}

	public IEnumerator __iter__()
	{
		return ((IEnumerable)this).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new ItemEnumerator(_getitem, _site);
	}
}

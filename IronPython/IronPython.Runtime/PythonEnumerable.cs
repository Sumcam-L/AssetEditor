using System.Collections;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

[PythonType("enumerable")]
public class PythonEnumerable : IEnumerable
{
	private object _iterator;

	public static bool TryCreate(object baseEnumerator, out IEnumerable enumerator)
	{
		if (PythonOps.TryGetBoundAttr(baseEnumerator, "__iter__", out var ret))
		{
			object obj = PythonCalls.Call(ret);
			if (obj is IEnumerable)
			{
				enumerator = (IEnumerable)obj;
			}
			else
			{
				enumerator = new PythonEnumerable(obj);
			}
			return true;
		}
		enumerator = null;
		return false;
	}

	public static IEnumerable Create(object baseObject)
	{
		if (!TryCreate(baseObject, out var enumerator))
		{
			throw PythonOps.TypeError("cannot convert {0} to IEnumerable", PythonTypeOps.GetName(baseObject));
		}
		return enumerator;
	}

	private PythonEnumerable(object iterator)
	{
		_iterator = iterator;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return (_iterator as IEnumerator) ?? new PythonEnumerator(_iterator);
	}
}

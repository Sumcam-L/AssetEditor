using System.Collections;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("reversed")]
public class ReversedEnumerator : IEnumerator
{
	private readonly object _getItemMethod;

	private readonly int _savedIndex;

	private object _current;

	private int _index;

	object IEnumerator.Current => _current;

	protected ReversedEnumerator(int length, object getitem)
	{
		_index = (_savedIndex = length);
		_getItemMethod = getitem;
	}

	public static object __new__(CodeContext context, PythonType type, [NotNull] IReversible o)
	{
		return o.__reversed__();
	}

	public static object __new__(CodeContext context, PythonType type, object o)
	{
		if (PythonOps.TryGetBoundAttr(context, o, "__reversed__", out var ret))
		{
			return PythonCalls.Call(context, ret);
		}
		PythonType pythonType = DynamicHelpers.GetPythonType(o);
		if (!pythonType.TryResolveSlot(context, "__getitem__", out var slot) || !slot.TryGetValue(context, o, pythonType, out var value) || o is PythonDictionary)
		{
			throw PythonOps.TypeError("argument to reversed() must be a sequence");
		}
		if (!DynamicHelpers.GetPythonType(o).TryGetLength(context, o, out var length))
		{
			throw PythonOps.TypeError("object of type '{0}' has no len()", DynamicHelpers.GetPythonType(o).Name);
		}
		if (type.UnderlyingSystemType == typeof(ReversedEnumerator))
		{
			return new ReversedEnumerator(length, value);
		}
		return type.CreateInstance(context, length, slot);
	}

	public int __length_hint__()
	{
		return _savedIndex;
	}

	public ReversedEnumerator __iter__()
	{
		return this;
	}

	bool IEnumerator.MoveNext()
	{
		if (_index > 0)
		{
			_index--;
			_current = PythonCalls.Call(_getItemMethod, _index);
			return true;
		}
		return false;
	}

	void IEnumerator.Reset()
	{
		_index = _savedIndex;
	}
}

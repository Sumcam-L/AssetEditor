using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime;

[PythonType]
public class staticmethod : PythonTypeSlot
{
	internal object _func;

	internal override bool GetAlwaysSucceeds => true;

	public object __func__ => _func;

	public staticmethod(object func)
	{
		_func = func;
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		value = __get__(instance, PythonOps.ToPythonType(owner));
		return true;
	}

	public object __get__(object instance)
	{
		return __get__(instance, null);
	}

	public object __get__(object instance, object owner)
	{
		return _func;
	}
}

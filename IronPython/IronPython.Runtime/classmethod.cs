using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime;

[PythonType]
public class classmethod : PythonTypeSlot
{
	internal object func;

	internal override bool GetAlwaysSucceeds => true;

	public object __func__ => func;

	public classmethod(CodeContext context, object func)
	{
		if (!PythonOps.IsCallable(context, func))
		{
			throw PythonOps.TypeError("{0} object is not callable", PythonTypeOps.GetName(func));
		}
		this.func = func;
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
		if (owner == null)
		{
			if (instance == null)
			{
				throw PythonOps.TypeError("__get__(None, None) is invalid");
			}
			owner = DynamicHelpers.GetPythonType(instance);
		}
		return new Method(func, owner, DynamicHelpers.GetPythonType(owner));
	}
}

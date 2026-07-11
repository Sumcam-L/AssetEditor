using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime;

internal sealed class InstanceFinalizer
{
	private object _instance;

	internal InstanceFinalizer(CodeContext context, object inst)
	{
		_instance = inst;
	}

	internal object CallDirect(CodeContext context)
	{
		object value;
		if (_instance is OldInstance oldInstance)
		{
			if (oldInstance.TryGetBoundCustomMember(context, "__del__", out value))
			{
				return PythonContext.GetContext(context).CallSplat(value);
			}
		}
		else
		{
			PythonTypeOps.TryInvokeUnaryOperator(context, _instance, "__del__", out value);
		}
		return null;
	}
}

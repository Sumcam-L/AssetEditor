using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;

namespace IronPython.Modules;

[PythonType("method-wrapper")]
public class GenericMethodWrapper
{
	private string name;

	private IProxyObject target;

	public GenericMethodWrapper(string methodName, IProxyObject proxyTarget)
	{
		name = methodName;
		target = proxyTarget;
	}

	[SpecialName]
	public object Call(CodeContext context, params object[] args)
	{
		return PythonOps.Invoke(context, target.Target, name, args);
	}

	[SpecialName]
	public object Call(CodeContext context, [ParamDictionary] IDictionary<object, object> dict, params object[] args)
	{
		if (!DynamicHelpers.GetPythonType(target.Target).TryGetBoundMember(context, target.Target, name, out var value))
		{
			throw PythonOps.AttributeError("type {0} has no attribute {1}", DynamicHelpers.GetPythonType(target.Target), name);
		}
		return PythonCalls.CallWithKeywordArgs(context, value, args, dict);
	}
}

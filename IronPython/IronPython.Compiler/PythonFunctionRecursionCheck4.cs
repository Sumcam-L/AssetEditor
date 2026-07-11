using System;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler;

internal class PythonFunctionRecursionCheck4
{
	private readonly Func<PythonFunction, object, object, object, object, object> _target;

	public PythonFunctionRecursionCheck4(Func<PythonFunction, object, object, object, object, object> target)
	{
		_target = target;
	}

	public object CallTarget(PythonFunction function, object arg0, object arg1, object arg2, object arg3)
	{
		PythonOps.FunctionPushFrame(function.Context.LanguageContext);
		try
		{
			return _target(function, arg0, arg1, arg2, arg3);
		}
		finally
		{
			PythonOps.FunctionPopFrame();
		}
	}
}

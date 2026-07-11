using System;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler;

internal class PythonFunctionRecursionCheck1
{
	private readonly Func<PythonFunction, object, object> _target;

	public PythonFunctionRecursionCheck1(Func<PythonFunction, object, object> target)
	{
		_target = target;
	}

	public object CallTarget(PythonFunction function, object arg0)
	{
		PythonOps.FunctionPushFrame(function.Context.LanguageContext);
		try
		{
			return _target(function, arg0);
		}
		finally
		{
			PythonOps.FunctionPopFrame();
		}
	}
}

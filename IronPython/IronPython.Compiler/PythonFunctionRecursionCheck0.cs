using System;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler;

internal class PythonFunctionRecursionCheck0
{
	private readonly Func<PythonFunction, object> _target;

	public PythonFunctionRecursionCheck0(Func<PythonFunction, object> target)
	{
		_target = target;
	}

	public object CallTarget(PythonFunction function)
	{
		PythonOps.FunctionPushFrame(function.Context.LanguageContext);
		try
		{
			return _target(function);
		}
		finally
		{
			PythonOps.FunctionPopFrame();
		}
	}
}

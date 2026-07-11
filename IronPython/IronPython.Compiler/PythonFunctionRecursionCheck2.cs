using System;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler;

internal class PythonFunctionRecursionCheck2
{
	private readonly Func<PythonFunction, object, object, object> _target;

	public PythonFunctionRecursionCheck2(Func<PythonFunction, object, object, object> target)
	{
		_target = target;
	}

	public object CallTarget(PythonFunction function, object arg0, object arg1)
	{
		PythonOps.FunctionPushFrame(function.Context.LanguageContext);
		try
		{
			return _target(function, arg0, arg1);
		}
		finally
		{
			PythonOps.FunctionPopFrame();
		}
	}
}

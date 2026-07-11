using System;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler;

internal class PythonFunctionRecursionCheckN
{
	private readonly Func<PythonFunction, object[], object> _target;

	public PythonFunctionRecursionCheckN(Func<PythonFunction, object[], object> target)
	{
		_target = target;
	}

	public object CallTarget(PythonFunction function, object[] args)
	{
		PythonOps.FunctionPushFrame(function.Context.LanguageContext);
		try
		{
			return _target(function, args);
		}
		finally
		{
			PythonOps.FunctionPopFrame();
		}
	}
}

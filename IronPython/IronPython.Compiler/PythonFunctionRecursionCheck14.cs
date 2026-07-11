using System;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler;

internal class PythonFunctionRecursionCheck14
{
	private readonly Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> _target;

	public PythonFunctionRecursionCheck14(Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> target)
	{
		_target = target;
	}

	public object CallTarget(PythonFunction function, object arg0, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
	{
		PythonOps.FunctionPushFrame(function.Context.LanguageContext);
		try
		{
			return _target(function, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
		}
		finally
		{
			PythonOps.FunctionPopFrame();
		}
	}
}

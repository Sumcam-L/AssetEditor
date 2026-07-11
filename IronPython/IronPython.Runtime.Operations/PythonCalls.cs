using System.Collections.Generic;

namespace IronPython.Runtime.Operations;

public static class PythonCalls
{
	public static object Call(object func, params object[] args)
	{
		return DefaultContext.DefaultPythonContext.CallSplat(func, args);
	}

	public static object Call(CodeContext context, object func)
	{
		return PythonContext.GetContext(context).Call(context, func);
	}

	public static object Call(CodeContext context, object func, object arg0)
	{
		return PythonContext.GetContext(context).Call(context, func, arg0);
	}

	public static object Call(CodeContext context, object func, object arg0, object arg1)
	{
		return PythonContext.GetContext(context).Call(context, func, arg0, arg1);
	}

	public static object Call(CodeContext context, object func, params object[] args)
	{
		return PythonContext.GetContext(context).CallSplat(func, args);
	}

	public static object CallWithKeywordArgs(CodeContext context, object func, object[] args, string[] names)
	{
		PythonDictionary pythonDictionary = new PythonDictionary();
		for (int i = 0; i < names.Length; i++)
		{
			pythonDictionary[names[i]] = args[args.Length - names.Length + i];
		}
		object[] array = new object[args.Length - names.Length];
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = args[j];
		}
		return CallWithKeywordArgs(context, func, array, pythonDictionary);
	}

	public static object CallWithKeywordArgs(CodeContext context, object func, object[] args, IDictionary<object, object> dict)
	{
		return PythonContext.GetContext(context).CallWithKeywords(func, args, dict);
	}
}

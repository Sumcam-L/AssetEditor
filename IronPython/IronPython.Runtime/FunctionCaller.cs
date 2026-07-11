using System;
using System.Runtime.CompilerServices;

namespace IronPython.Runtime;

public class FunctionCaller
{
	protected readonly int _compat;

	internal FunctionCaller(int compat)
	{
		_compat = compat;
	}

	public object Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object>)pythonFunction.func_code.Target)(pythonFunction);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default1Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default2Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default3Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default4Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default5Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default6Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default7Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default8Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default9Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default10Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8], pythonFunction.Defaults[num + 9]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default11Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8], pythonFunction.Defaults[num + 9], pythonFunction.Defaults[num + 10]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default12Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8], pythonFunction.Defaults[num + 9], pythonFunction.Defaults[num + 10], pythonFunction.Defaults[num + 11]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}

	public object Default13Call0(CallSite site, CodeContext context, object func)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8], pythonFunction.Defaults[num + 9], pythonFunction.Defaults[num + 10], pythonFunction.Defaults[num + 11], pythonFunction.Defaults[num + 12]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, object>>)site).Update(site, context, func);
	}
}
public sealed class FunctionCaller<T0> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}

	public object Default1Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 1;
			return ((Func<PythonFunction, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}

	public object Default2Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 1;
			return ((Func<PythonFunction, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}

	public object Default3Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 1;
			return ((Func<PythonFunction, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}

	public object Default4Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 1;
			return ((Func<PythonFunction, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}

	public object Default5Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 1;
			return ((Func<PythonFunction, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}

	public object Default6Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 1;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}

	public object Default7Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 1;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}

	public object Default8Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 1;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}

	public object Default9Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 1;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}

	public object Default10Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 1;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8], pythonFunction.Defaults[num + 9]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}

	public object Default11Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 1;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8], pythonFunction.Defaults[num + 9], pythonFunction.Defaults[num + 10]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}

	public object Default12Call1(CallSite site, CodeContext context, object func, T0 arg0)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 1;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8], pythonFunction.Defaults[num + 9], pythonFunction.Defaults[num + 10], pythonFunction.Defaults[num + 11]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, object>>)site).Update(site, context, func, arg0);
	}
}
public sealed class FunctionCaller<T0, T1> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call2(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, func, arg0, arg1);
	}

	public object Default1Call2(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 2;
			return ((Func<PythonFunction, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, func, arg0, arg1);
	}

	public object Default2Call2(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 2;
			return ((Func<PythonFunction, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, func, arg0, arg1);
	}

	public object Default3Call2(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 2;
			return ((Func<PythonFunction, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, func, arg0, arg1);
	}

	public object Default4Call2(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 2;
			return ((Func<PythonFunction, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, func, arg0, arg1);
	}

	public object Default5Call2(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 2;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, func, arg0, arg1);
	}

	public object Default6Call2(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 2;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, func, arg0, arg1);
	}

	public object Default7Call2(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 2;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, func, arg0, arg1);
	}

	public object Default8Call2(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 2;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, func, arg0, arg1);
	}

	public object Default9Call2(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 2;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, func, arg0, arg1);
	}

	public object Default10Call2(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 2;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8], pythonFunction.Defaults[num + 9]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, func, arg0, arg1);
	}

	public object Default11Call2(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 2;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8], pythonFunction.Defaults[num + 9], pythonFunction.Defaults[num + 10]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, object>>)site).Update(site, context, func, arg0, arg1);
	}
}
public sealed class FunctionCaller<T0, T1, T2> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call3(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, func, arg0, arg1, arg2);
	}

	public object Default1Call3(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 3;
			return ((Func<PythonFunction, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, func, arg0, arg1, arg2);
	}

	public object Default2Call3(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 3;
			return ((Func<PythonFunction, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, func, arg0, arg1, arg2);
	}

	public object Default3Call3(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 3;
			return ((Func<PythonFunction, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, func, arg0, arg1, arg2);
	}

	public object Default4Call3(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 3;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, func, arg0, arg1, arg2);
	}

	public object Default5Call3(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 3;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, func, arg0, arg1, arg2);
	}

	public object Default6Call3(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 3;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, func, arg0, arg1, arg2);
	}

	public object Default7Call3(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 3;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, func, arg0, arg1, arg2);
	}

	public object Default8Call3(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 3;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, func, arg0, arg1, arg2);
	}

	public object Default9Call3(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 3;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, func, arg0, arg1, arg2);
	}

	public object Default10Call3(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 3;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8], pythonFunction.Defaults[num + 9]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, object>>)site).Update(site, context, func, arg0, arg1, arg2);
	}
}
public sealed class FunctionCaller<T0, T1, T2, T3> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call4(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3);
	}

	public object Default1Call4(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 4;
			return ((Func<PythonFunction, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3);
	}

	public object Default2Call4(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 4;
			return ((Func<PythonFunction, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3);
	}

	public object Default3Call4(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 4;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3);
	}

	public object Default4Call4(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 4;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3);
	}

	public object Default5Call4(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 4;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3);
	}

	public object Default6Call4(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 4;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3);
	}

	public object Default7Call4(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 4;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3);
	}

	public object Default8Call4(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 4;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3);
	}

	public object Default9Call4(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 4;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7], pythonFunction.Defaults[num + 8]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3);
	}
}
public sealed class FunctionCaller<T0, T1, T2, T3, T4> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call5(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4);
	}

	public object Default1Call5(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 5;
			return ((Func<PythonFunction, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4);
	}

	public object Default2Call5(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 5;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4);
	}

	public object Default3Call5(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 5;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4);
	}

	public object Default4Call5(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 5;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4);
	}

	public object Default5Call5(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 5;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4);
	}

	public object Default6Call5(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 5;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4);
	}

	public object Default7Call5(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 5;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4);
	}

	public object Default8Call5(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 5;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6], pythonFunction.Defaults[num + 7]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4);
	}
}
public sealed class FunctionCaller<T0, T1, T2, T3, T4, T5> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call6(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5);
	}

	public object Default1Call6(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 6;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5);
	}

	public object Default2Call6(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 6;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5);
	}

	public object Default3Call6(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 6;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5);
	}

	public object Default4Call6(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 6;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5);
	}

	public object Default5Call6(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 6;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5);
	}

	public object Default6Call6(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 6;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5);
	}

	public object Default7Call6(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 6;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5], pythonFunction.Defaults[num + 6]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5);
	}
}
public sealed class FunctionCaller<T0, T1, T2, T3, T4, T5, T6> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call7(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public object Default1Call7(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 7;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public object Default2Call7(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 7;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public object Default3Call7(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 7;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public object Default4Call7(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 7;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public object Default5Call7(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 7;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public object Default6Call7(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 7;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4], pythonFunction.Defaults[num + 5]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
	}
}
public sealed class FunctionCaller<T0, T1, T2, T3, T4, T5, T6, T7> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call8(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}

	public object Default1Call8(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 8;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}

	public object Default2Call8(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 8;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}

	public object Default3Call8(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 8;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}

	public object Default4Call8(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 8;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}

	public object Default5Call8(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 8;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3], pythonFunction.Defaults[num + 4]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}
}
public sealed class FunctionCaller<T0, T1, T2, T3, T4, T5, T6, T7, T8> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call9(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
	}

	public object Default1Call9(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 9;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
	}

	public object Default2Call9(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 9;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
	}

	public object Default3Call9(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 9;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
	}

	public object Default4Call9(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 9;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2], pythonFunction.Defaults[num + 3]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
	}
}
public sealed class FunctionCaller<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call10(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
	}

	public object Default1Call10(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 10;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
	}

	public object Default2Call10(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 10;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
	}

	public object Default3Call10(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 10;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1], pythonFunction.Defaults[num + 2]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
	}
}
public sealed class FunctionCaller<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call11(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
	}

	public object Default1Call11(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 11;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
	}

	public object Default2Call11(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 11;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, pythonFunction.Defaults[num], pythonFunction.Defaults[num + 1]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
	}
}
public sealed class FunctionCaller<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call12(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
	}

	public object Default1Call12(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			int num = pythonFunction.Defaults.Length - pythonFunction.NormalArgumentCount + 12;
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, pythonFunction.Defaults[num]);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
	}
}
public sealed class FunctionCaller<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : FunctionCaller
{
	public FunctionCaller(int compat)
		: base(compat)
	{
	}

	public object Call13(CallSite site, CodeContext context, object func, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
	{
		if (func is PythonFunction pythonFunction && pythonFunction._compat == _compat)
		{
			return ((Func<PythonFunction, object, object, object, object, object, object, object, object, object, object, object, object, object, object>)pythonFunction.func_code.Target)(pythonFunction, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
		}
		return ((CallSite<Func<CallSite, CodeContext, object, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, object>>)site).Update(site, context, func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
	}
}

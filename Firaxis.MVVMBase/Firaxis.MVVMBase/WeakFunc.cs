using System;
using System.Reflection;

namespace Firaxis.MVVMBase;

public class WeakFunc<ReturnType> : IWeakFunc
{
	protected Func<ReturnType> _staticFunc;

	protected MethodInfo _methodInfo;

	protected WeakReference _funcTargetReference;

	public bool IsAlive
	{
		get
		{
			if (_staticFunc == null)
			{
				return _methodInfo != null && _funcTargetReference.IsAlive;
			}
			return _funcTargetReference == null || _funcTargetReference.IsAlive;
		}
	}

	public WeakFunc(Func<ReturnType> f)
	{
		if (f == null)
		{
			throw new ArgumentNullException("f");
		}
		if (f.Method.IsStatic)
		{
			_staticFunc = f;
		}
		else
		{
			_methodInfo = f.Method;
		}
		if (f.Target != null)
		{
			_funcTargetReference = new WeakReference(f.Target);
		}
	}

	public bool Execute(object parameter, out ReturnType result)
	{
		if ((_funcTargetReference != null && !_funcTargetReference.IsAlive) || (_funcTargetReference == null && _staticFunc == null))
		{
			result = default(ReturnType);
			return false;
		}
		if (_staticFunc != null)
		{
			result = _staticFunc();
			return true;
		}
		if (_methodInfo != null)
		{
			WeakReference funcTargetReference = _funcTargetReference;
			if (funcTargetReference == null)
			{
				result = default(ReturnType);
				return false;
			}
			object obj = _methodInfo.Invoke(funcTargetReference.Target, new object[0]);
			if (obj is ReturnType)
			{
				result = (ReturnType)obj;
			}
			else
			{
				result = default(ReturnType);
			}
			return true;
		}
		result = default(ReturnType);
		return true;
	}

	bool IWeakFunc.Execute(object parameter, out object result)
	{
		if (Execute(parameter, out var result2))
		{
			result = result2;
			return true;
		}
		result = null;
		return true;
	}

	public void ClearReferences()
	{
		_methodInfo = null;
		_funcTargetReference = null;
		_staticFunc = null;
	}

	public bool FuncEquals(Func<ReturnType> f)
	{
		if (f == null || (f.Method.IsStatic && _staticFunc != f) || _methodInfo != f.Method)
		{
			return false;
		}
		return f.Target == null || (_funcTargetReference != null && _funcTargetReference.IsAlive && _funcTargetReference.Target == f.Target);
	}

	public bool FuncEquals(Delegate other)
	{
		Func<ReturnType> f = other as Func<ReturnType>;
		return (object)other != null && FuncEquals(f);
	}
}
public class WeakFunc<InputType, ReturnType> : IWeakFunc
{
	protected Func<InputType, ReturnType> _staticFunc;

	protected MethodInfo _methodInfo;

	protected WeakReference _funcTargetReference;

	public bool IsAlive
	{
		get
		{
			if (_staticFunc == null)
			{
				return _methodInfo != null && _funcTargetReference.IsAlive;
			}
			return _funcTargetReference == null || _funcTargetReference.IsAlive;
		}
	}

	public WeakFunc(Func<InputType, ReturnType> f)
	{
		if (f == null)
		{
			throw new ArgumentNullException("f");
		}
		if (f.Method.IsStatic)
		{
			_staticFunc = f;
		}
		else
		{
			_methodInfo = f.Method;
		}
		if (f.Target != null)
		{
			_funcTargetReference = new WeakReference(f.Target);
		}
	}

	public bool Execute(InputType parameter, out ReturnType result)
	{
		if ((_funcTargetReference != null && !_funcTargetReference.IsAlive) || (_funcTargetReference == null && _staticFunc == null))
		{
			result = default(ReturnType);
			return false;
		}
		if (_staticFunc != null)
		{
			result = _staticFunc(parameter);
			return true;
		}
		if (_methodInfo != null)
		{
			WeakReference funcTargetReference = _funcTargetReference;
			if (funcTargetReference == null)
			{
				result = default(ReturnType);
				return false;
			}
			object obj = _methodInfo.Invoke(funcTargetReference.Target, new object[1] { parameter });
			if (obj is ReturnType)
			{
				result = (ReturnType)obj;
			}
			else
			{
				result = default(ReturnType);
			}
			return true;
		}
		result = default(ReturnType);
		return true;
	}

	bool IWeakFunc.Execute(object parameter, out object result)
	{
		if (parameter is InputType && Execute((InputType)parameter, out var result2))
		{
			result = result2;
			return true;
		}
		result = null;
		return true;
	}

	public void ClearReferences()
	{
		_methodInfo = null;
		_funcTargetReference = null;
		_staticFunc = null;
	}

	public bool FuncEquals(Func<InputType, ReturnType> f)
	{
		if (f == null || (f.Method.IsStatic && _staticFunc != f) || _methodInfo != f.Method)
		{
			return false;
		}
		return f.Target == null || (_funcTargetReference != null && _funcTargetReference.IsAlive && _funcTargetReference.Target == f.Target);
	}

	public bool FuncEquals(Delegate other)
	{
		Func<InputType, ReturnType> f = other as Func<InputType, ReturnType>;
		return (object)other != null && FuncEquals(f);
	}
}

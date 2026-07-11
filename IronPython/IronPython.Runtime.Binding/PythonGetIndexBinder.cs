using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class PythonGetIndexBinder : GetIndexBinder, IPythonSite, IExpressionSerializable
{
	private readonly PythonContext _context;

	public PythonContext Context => _context;

	public PythonGetIndexBinder(PythonContext context, int argCount)
		: base(new CallInfo(argCount))
	{
		_context = context;
	}

	public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
	{
		if (ComBinder.TryBindGetIndex(this, target, BindingHelpers.GetComArguments(indexes), out var result))
		{
			return result;
		}
		return PythonProtocol.Index(this, PythonIndexType.GetItem, ArrayUtils.Insert(target, indexes), errorSuggestion);
	}

	public override T BindDelegate<T>(CallSite<T> site, object[] args)
	{
		if (CompilerHelpers.GetType(args[1]) == typeof(int))
		{
			if (CompilerHelpers.GetType(args[0]) == typeof(List))
			{
				if (typeof(T) == typeof(Func<CallSite, object, object, object>))
				{
					return (T)(object)new Func<CallSite, object, object, object>(ListIndex);
				}
				if (typeof(T) == typeof(Func<CallSite, object, int, object>))
				{
					return (T)(object)new Func<CallSite, object, int, object>(ListIndex);
				}
				if (typeof(T) == typeof(Func<CallSite, List, object, object>))
				{
					return (T)(object)new Func<CallSite, List, object, object>(ListIndex);
				}
			}
			else if (CompilerHelpers.GetType(args[0]) == typeof(PythonTuple))
			{
				if (typeof(T) == typeof(Func<CallSite, object, object, object>))
				{
					return (T)(object)new Func<CallSite, object, object, object>(TupleIndex);
				}
				if (typeof(T) == typeof(Func<CallSite, object, int, object>))
				{
					return (T)(object)new Func<CallSite, object, int, object>(TupleIndex);
				}
				if (typeof(T) == typeof(Func<CallSite, PythonTuple, object, object>))
				{
					return (T)(object)new Func<CallSite, PythonTuple, object, object>(TupleIndex);
				}
			}
			else if (CompilerHelpers.GetType(args[0]) == typeof(string))
			{
				if (typeof(T) == typeof(Func<CallSite, object, object, object>))
				{
					return (T)(object)new Func<CallSite, object, object, object>(StringIndex);
				}
				if (typeof(T) == typeof(Func<CallSite, object, int, object>))
				{
					return (T)(object)new Func<CallSite, object, int, object>(StringIndex);
				}
				if (typeof(T) == typeof(Func<CallSite, string, object, object>))
				{
					return (T)(object)new Func<CallSite, string, object, object>(StringIndex);
				}
			}
		}
		return base.BindDelegate(site, args);
	}

	private object ListIndex(CallSite site, List target, object index)
	{
		if (target != null && index != null && index.GetType() == typeof(int))
		{
			return target[(int)index];
		}
		return ((CallSite<Func<CallSite, List, object, object>>)site).Update(site, target, index);
	}

	private object ListIndex(CallSite site, object target, object index)
	{
		if (target is List list && index != null && index.GetType() == typeof(int))
		{
			return list[(int)index];
		}
		return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, target, index);
	}

	private object ListIndex(CallSite site, object target, int index)
	{
		if (target is List list)
		{
			return list[index];
		}
		return ((CallSite<Func<CallSite, object, int, object>>)site).Update(site, target, index);
	}

	private object TupleIndex(CallSite site, PythonTuple target, object index)
	{
		if (target != null && index != null && index.GetType() == typeof(int))
		{
			return target[(int)index];
		}
		return ((CallSite<Func<CallSite, PythonTuple, object, object>>)site).Update(site, target, index);
	}

	private object TupleIndex(CallSite site, object target, object index)
	{
		if (target is PythonTuple pythonTuple && index != null && index.GetType() == typeof(int))
		{
			return pythonTuple[(int)index];
		}
		return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, target, index);
	}

	private object TupleIndex(CallSite site, object target, int index)
	{
		if (target is PythonTuple pythonTuple)
		{
			return pythonTuple[index];
		}
		return ((CallSite<Func<CallSite, object, int, object>>)site).Update(site, target, index);
	}

	private object StringIndex(CallSite site, string target, object index)
	{
		if (target != null && index != null && index.GetType() == typeof(int))
		{
			return StringOps.GetItem(target, (int)index);
		}
		return ((CallSite<Func<CallSite, string, object, object>>)site).Update(site, target, index);
	}

	private object StringIndex(CallSite site, object target, object index)
	{
		if (target is string s && index != null && index.GetType() == typeof(int))
		{
			return StringOps.GetItem(s, (int)index);
		}
		return ((CallSite<Func<CallSite, object, object, object>>)site).Update(site, target, index);
	}

	private object StringIndex(CallSite site, object target, int index)
	{
		if (target is string s)
		{
			return StringOps.GetItem(s, index);
		}
		return ((CallSite<Func<CallSite, object, int, object>>)site).Update(site, target, index);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ _context.Binder.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PythonGetIndexBinder pythonGetIndexBinder))
		{
			return false;
		}
		if (pythonGetIndexBinder._context.Binder == _context.Binder)
		{
			return base.Equals(obj);
		}
		return false;
	}

	public Expression CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeGetIndexAction"), BindingHelpers.CreateBinderStateExpression(), Utils.Constant(base.CallInfo.ArgumentCount));
	}
}

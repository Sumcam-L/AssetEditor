using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Binding;

internal class PythonSetIndexBinder : SetIndexBinder, IPythonSite, IExpressionSerializable
{
	private readonly PythonContext _context;

	public PythonContext Context => _context;

	public PythonSetIndexBinder(PythonContext context, int argCount)
		: base(new CallInfo(argCount))
	{
		_context = context;
	}

	public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
	{
		if (ComBinder.TryBindSetIndex(this, target, BindingHelpers.GetComArguments(indexes), BindingHelpers.GetComArgument(value), out var result))
		{
			return result;
		}
		DynamicMetaObject[] array = new DynamicMetaObject[indexes.Length + 2];
		array[0] = target;
		for (int i = 0; i < indexes.Length; i++)
		{
			array[i + 1] = indexes[i];
		}
		array[array.Length - 1] = value;
		return PythonProtocol.Index(this, PythonIndexType.SetItem, array, errorSuggestion);
	}

	public override T BindDelegate<T>(CallSite<T> site, object[] args)
	{
		if (args[0] != null && args[0].GetType() == typeof(PythonDictionary) && typeof(T) == typeof(Func<CallSite, object, object, object, object>))
		{
			return (T)(object)new Func<CallSite, object, object, object, object>(DictAssign);
		}
		return base.BindDelegate(site, args);
	}

	private object DictAssign(CallSite site, object dict, object key, object value)
	{
		if (dict != null && dict.GetType() == typeof(PythonDictionary))
		{
			((PythonDictionary)dict)[key] = value;
			return value;
		}
		return ((CallSite<Func<CallSite, object, object, object, object>>)site).Update(site, dict, key, value);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ _context.Binder.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PythonSetIndexBinder pythonSetIndexBinder))
		{
			return false;
		}
		if (pythonSetIndexBinder._context.Binder == _context.Binder)
		{
			return base.Equals(obj);
		}
		return false;
	}

	public Expression CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeSetIndexAction"), BindingHelpers.CreateBinderStateExpression(), Utils.Constant(base.CallInfo.ArgumentCount));
	}
}

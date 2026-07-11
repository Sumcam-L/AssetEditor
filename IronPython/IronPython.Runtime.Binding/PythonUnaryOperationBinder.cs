using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Binding;

internal class PythonUnaryOperationBinder : UnaryOperationBinder, IPythonSite, IExpressionSerializable
{
	private readonly PythonContext _context;

	public PythonContext Context => _context;

	public PythonUnaryOperationBinder(PythonContext context, ExpressionType operation)
		: base(operation)
	{
		_context = context;
	}

	public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
	{
		return PythonProtocol.Operation(this, target, errorSuggestion);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ _context.Binder.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PythonUnaryOperationBinder pythonUnaryOperationBinder))
		{
			return false;
		}
		if (pythonUnaryOperationBinder._context.Binder == _context.Binder)
		{
			return base.Equals(obj);
		}
		return false;
	}

	public override T BindDelegate<T>(CallSite<T> site, object[] args)
	{
		switch (base.Operation)
		{
		case ExpressionType.Negate:
			if (CompilerHelpers.GetType(args[0]) == typeof(int) && typeof(T) == typeof(Func<CallSite, object, object>))
			{
				return (T)(object)new Func<CallSite, object, object>(IntNegate);
			}
			break;
		case ExpressionType.IsFalse:
			if (args[0] == null)
			{
				if (typeof(T) == typeof(Func<CallSite, object, bool>))
				{
					return (T)(object)new Func<CallSite, object, bool>(NoneIsFalse);
				}
			}
			else if (args[0].GetType() == typeof(string))
			{
				if (typeof(T) == typeof(Func<CallSite, object, bool>))
				{
					return (T)(object)new Func<CallSite, object, bool>(StringIsFalse);
				}
			}
			else if (args[0].GetType() == typeof(bool))
			{
				if (typeof(T) == typeof(Func<CallSite, object, bool>))
				{
					return (T)(object)new Func<CallSite, object, bool>(BoolIsFalse);
				}
			}
			else if (args[0].GetType() == typeof(List))
			{
				if (typeof(T) == typeof(Func<CallSite, object, bool>))
				{
					return (T)(object)new Func<CallSite, object, bool>(ListIsFalse);
				}
			}
			else if (args[0].GetType() == typeof(PythonTuple))
			{
				if (typeof(T) == typeof(Func<CallSite, object, bool>))
				{
					return (T)(object)new Func<CallSite, object, bool>(TupleIsFalse);
				}
			}
			else if (args[0].GetType() == typeof(int) && typeof(T) == typeof(Func<CallSite, object, bool>))
			{
				return (T)(object)new Func<CallSite, object, bool>(IntIsFalse);
			}
			break;
		case ExpressionType.Not:
			if (args[0] == null)
			{
				if (typeof(T) == typeof(Func<CallSite, object, object>))
				{
					return (T)(object)new Func<CallSite, object, object>(NoneNot);
				}
			}
			else if (args[0].GetType() == typeof(string))
			{
				if (typeof(T) == typeof(Func<CallSite, object, object>))
				{
					return (T)(object)new Func<CallSite, object, object>(StringNot);
				}
			}
			else if (args[0].GetType() == typeof(bool))
			{
				if (typeof(T) == typeof(Func<CallSite, object, object>))
				{
					return (T)(object)new Func<CallSite, object, object>(BoolNot);
				}
			}
			else if (args[0].GetType() == typeof(List))
			{
				if (typeof(T) == typeof(Func<CallSite, object, object>))
				{
					return (T)(object)new Func<CallSite, object, object>(ListNot);
				}
			}
			else if (args[0].GetType() == typeof(PythonTuple))
			{
				if (typeof(T) == typeof(Func<CallSite, object, object>))
				{
					return (T)(object)new Func<CallSite, object, object>(TupleNot);
				}
			}
			else if (args[0].GetType() == typeof(int) && typeof(T) == typeof(Func<CallSite, object, object>))
			{
				return (T)(object)new Func<CallSite, object, object>(IntNot);
			}
			break;
		}
		return base.BindDelegate(site, args);
	}

	private object IntNegate(CallSite site, object value)
	{
		if (value is int)
		{
			return Int32Ops.Negate((int)value);
		}
		return ((CallSite<Func<CallSite, object, object>>)site).Update(site, value);
	}

	private bool StringIsFalse(CallSite site, object value)
	{
		if (value is string text)
		{
			return text.Length == 0;
		}
		if (value == null)
		{
			return true;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	private bool ListIsFalse(CallSite site, object value)
	{
		if (value != null && value.GetType() == typeof(List))
		{
			return ((List)value).Count == 0;
		}
		if (value == null)
		{
			return true;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	private bool NoneIsFalse(CallSite site, object value)
	{
		if (value == null)
		{
			return true;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	private bool IntIsFalse(CallSite site, object value)
	{
		if (value is int)
		{
			return (int)value == 0;
		}
		if (value == null)
		{
			return true;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	private bool TupleIsFalse(CallSite site, object value)
	{
		if (value != null && value.GetType() == typeof(PythonTuple))
		{
			return ((PythonTuple)value).Count == 0;
		}
		if (value == null)
		{
			return true;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	private bool BoolIsFalse(CallSite site, object value)
	{
		if (value is bool)
		{
			return !(bool)value;
		}
		if (value == null)
		{
			return true;
		}
		return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
	}

	private object StringNot(CallSite site, object value)
	{
		if (value is string text)
		{
			if (text.Length != 0)
			{
				return ScriptingRuntimeHelpers.False;
			}
			return ScriptingRuntimeHelpers.True;
		}
		if (value == null)
		{
			return true;
		}
		return ((CallSite<Func<CallSite, object, object>>)site).Update(site, value);
	}

	private object ListNot(CallSite site, object value)
	{
		if (value != null && value.GetType() == typeof(List))
		{
			if (((List)value).Count != 0)
			{
				return ScriptingRuntimeHelpers.False;
			}
			return ScriptingRuntimeHelpers.True;
		}
		if (value == null)
		{
			return true;
		}
		return ((CallSite<Func<CallSite, object, object>>)site).Update(site, value);
	}

	private object NoneNot(CallSite site, object value)
	{
		if (value == null)
		{
			return ScriptingRuntimeHelpers.True;
		}
		return ((CallSite<Func<CallSite, object, object>>)site).Update(site, value);
	}

	private object TupleNot(CallSite site, object value)
	{
		if (value != null && value.GetType() == typeof(PythonTuple))
		{
			if (((PythonTuple)value).Count != 0)
			{
				return ScriptingRuntimeHelpers.False;
			}
			return ScriptingRuntimeHelpers.True;
		}
		if (value == null)
		{
			return true;
		}
		return ((CallSite<Func<CallSite, object, object>>)site).Update(site, value);
	}

	private object BoolNot(CallSite site, object value)
	{
		if (value is bool)
		{
			if ((bool)value)
			{
				return ScriptingRuntimeHelpers.False;
			}
			return ScriptingRuntimeHelpers.True;
		}
		if (value == null)
		{
			return true;
		}
		return ((CallSite<Func<CallSite, object, object>>)site).Update(site, value);
	}

	private object IntNot(CallSite site, object value)
	{
		if (value is int)
		{
			if ((int)value != 0)
			{
				return ScriptingRuntimeHelpers.False;
			}
			return ScriptingRuntimeHelpers.True;
		}
		if (value == null)
		{
			return true;
		}
		return ((CallSite<Func<CallSite, object, object>>)site).Update(site, value);
	}

	public override string ToString()
	{
		return "PythonUnary " + base.Operation;
	}

	public Expression CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeUnaryOperationAction"), BindingHelpers.CreateBinderStateExpression(), Utils.Constant(base.Operation));
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class PythonOperationBinder : DynamicMetaObjectBinder, IPythonSite, IExpressionSerializable
{
	private readonly PythonContext _context;

	private readonly PythonOperationKind _operation;

	public PythonOperationKind Operation => _operation;

	public override Type ReturnType => (Operation & (PythonOperationKind)(-1073741825)) switch
	{
		PythonOperationKind.Compare => typeof(int), 
		PythonOperationKind.IsCallable => typeof(bool), 
		PythonOperationKind.Hash => typeof(int), 
		PythonOperationKind.Contains => typeof(bool), 
		PythonOperationKind.GetEnumeratorForIteration => typeof(KeyValuePair<IEnumerator, IDisposable>), 
		PythonOperationKind.CallSignatures => typeof(IList<string>), 
		PythonOperationKind.Documentation => typeof(string), 
		_ => typeof(object), 
	};

	public PythonContext Context => _context;

	public PythonOperationBinder(PythonContext context, PythonOperationKind operation)
	{
		_context = context;
		_operation = operation;
	}

	public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
	{
		if (target is IPythonOperable pythonOperable)
		{
			DynamicMetaObject dynamicMetaObject = pythonOperable.BindOperation(this, ArrayUtils.Insert(target, args));
			if (dynamicMetaObject != null)
			{
				return dynamicMetaObject;
			}
		}
		return PythonProtocol.Operation(this, ArrayUtils.Insert(target, args));
	}

	public override T BindDelegate<T>(CallSite<T> site, object[] args)
	{
		switch (_operation)
		{
		case PythonOperationKind.Hash:
			if (CompilerHelpers.GetType(args[0]) == typeof(PythonType))
			{
				if (typeof(T) == typeof(Func<CallSite, object, int>))
				{
					return (T)(object)new Func<CallSite, object, int>(HashPythonType);
				}
			}
			else if (args[0] is OldClass && typeof(T) == typeof(Func<CallSite, object, int>))
			{
				return (T)(object)new Func<CallSite, object, int>(HashOldClass);
			}
			break;
		case PythonOperationKind.Compare:
			if (CompilerHelpers.GetType(args[0]) == typeof(string) && CompilerHelpers.GetType(args[1]) == typeof(string) && typeof(T) == typeof(Func<CallSite, object, object, int>))
			{
				return (T)(object)new Func<CallSite, object, object, int>(CompareStrings);
			}
			break;
		case PythonOperationKind.GetEnumeratorForIteration:
			if (CompilerHelpers.GetType(args[0]) == typeof(List))
			{
				if (typeof(T) == typeof(Func<CallSite, List, KeyValuePair<IEnumerator, IDisposable>>))
				{
					return (T)(object)new Func<CallSite, List, KeyValuePair<IEnumerator, IDisposable>>(GetListEnumerator);
				}
				return (T)(object)new Func<CallSite, object, KeyValuePair<IEnumerator, IDisposable>>(GetListEnumerator);
			}
			if (CompilerHelpers.GetType(args[0]) == typeof(PythonTuple))
			{
				if (typeof(T) == typeof(Func<CallSite, PythonTuple, KeyValuePair<IEnumerator, IDisposable>>))
				{
					return (T)(object)new Func<CallSite, PythonTuple, KeyValuePair<IEnumerator, IDisposable>>(GetTupleEnumerator);
				}
				return (T)(object)new Func<CallSite, object, KeyValuePair<IEnumerator, IDisposable>>(GetTupleEnumerator);
			}
			break;
		case PythonOperationKind.Contains:
			if (CompilerHelpers.GetType(args[1]) == typeof(List))
			{
				Type typeFromHandle = typeof(T);
				if (typeFromHandle == typeof(Func<CallSite, object, object, bool>))
				{
					return (T)(object)new Func<CallSite, object, object, bool>(ListContains);
				}
				if (typeFromHandle == typeof(Func<CallSite, object, List, bool>))
				{
					return (T)(object)new Func<CallSite, object, List, bool>(ListContains);
				}
				if (typeFromHandle == typeof(Func<CallSite, int, object, bool>))
				{
					return (T)(object)new Func<CallSite, int, object, bool>(ListContains);
				}
				if (typeFromHandle == typeof(Func<CallSite, string, object, bool>))
				{
					return (T)(object)new Func<CallSite, string, object, bool>(ListContains);
				}
				if (typeFromHandle == typeof(Func<CallSite, double, object, bool>))
				{
					return (T)(object)new Func<CallSite, double, object, bool>(ListContains);
				}
				if (typeFromHandle == typeof(Func<CallSite, PythonTuple, object, bool>))
				{
					return (T)(object)new Func<CallSite, PythonTuple, object, bool>(ListContains);
				}
			}
			else if (CompilerHelpers.GetType(args[1]) == typeof(PythonTuple))
			{
				Type typeFromHandle2 = typeof(T);
				if (typeFromHandle2 == typeof(Func<CallSite, object, object, bool>))
				{
					return (T)(object)new Func<CallSite, object, object, bool>(TupleContains);
				}
				if (typeFromHandle2 == typeof(Func<CallSite, object, PythonTuple, bool>))
				{
					return (T)(object)new Func<CallSite, object, PythonTuple, bool>(TupleContains);
				}
				if (typeFromHandle2 == typeof(Func<CallSite, int, object, bool>))
				{
					return (T)(object)new Func<CallSite, int, object, bool>(TupleContains);
				}
				if (typeFromHandle2 == typeof(Func<CallSite, string, object, bool>))
				{
					return (T)(object)new Func<CallSite, string, object, bool>(TupleContains);
				}
				if (typeFromHandle2 == typeof(Func<CallSite, double, object, bool>))
				{
					return (T)(object)new Func<CallSite, double, object, bool>(TupleContains);
				}
				if (typeFromHandle2 == typeof(Func<CallSite, PythonTuple, object, bool>))
				{
					return (T)(object)new Func<CallSite, PythonTuple, object, bool>(TupleContains);
				}
			}
			else if (CompilerHelpers.GetType(args[0]) == typeof(string) && CompilerHelpers.GetType(args[1]) == typeof(string))
			{
				Type typeFromHandle3 = typeof(T);
				if (typeFromHandle3 == typeof(Func<CallSite, object, object, bool>))
				{
					return (T)(object)new Func<CallSite, object, object, bool>(StringContains);
				}
				if (typeFromHandle3 == typeof(Func<CallSite, string, object, bool>))
				{
					return (T)(object)new Func<CallSite, string, object, bool>(StringContains);
				}
				if (typeFromHandle3 == typeof(Func<CallSite, object, string, bool>))
				{
					return (T)(object)new Func<CallSite, object, string, bool>(StringContains);
				}
				if (typeFromHandle3 == typeof(Func<CallSite, string, string, bool>))
				{
					return (T)(object)new Func<CallSite, string, string, bool>(StringContains);
				}
			}
			else if (CompilerHelpers.GetType(args[1]) == typeof(SetCollection) && typeof(T) == typeof(Func<CallSite, object, object, bool>))
			{
				return (T)(object)new Func<CallSite, object, object, bool>(SetContains);
			}
			break;
		}
		return base.BindDelegate(site, args);
	}

	private KeyValuePair<IEnumerator, IDisposable> GetListEnumerator(CallSite site, List value)
	{
		return new KeyValuePair<IEnumerator, IDisposable>(new ListIterator(value), null);
	}

	private KeyValuePair<IEnumerator, IDisposable> GetListEnumerator(CallSite site, object value)
	{
		if (value != null && value.GetType() == typeof(List))
		{
			return new KeyValuePair<IEnumerator, IDisposable>(new ListIterator((List)value), null);
		}
		return ((CallSite<Func<CallSite, object, KeyValuePair<IEnumerator, IDisposable>>>)site).Update(site, value);
	}

	private KeyValuePair<IEnumerator, IDisposable> GetTupleEnumerator(CallSite site, PythonTuple value)
	{
		return new KeyValuePair<IEnumerator, IDisposable>(new TupleEnumerator(value), null);
	}

	private KeyValuePair<IEnumerator, IDisposable> GetTupleEnumerator(CallSite site, object value)
	{
		if (value != null && value.GetType() == typeof(PythonTuple))
		{
			return new KeyValuePair<IEnumerator, IDisposable>(new TupleEnumerator((PythonTuple)value), null);
		}
		return ((CallSite<Func<CallSite, object, KeyValuePair<IEnumerator, IDisposable>>>)site).Update(site, value);
	}

	private bool ListContains(CallSite site, object other, List value)
	{
		return value.ContainsWorker(other);
	}

	private bool ListContains<TOther>(CallSite site, TOther other, object value)
	{
		if (value != null && value.GetType() == typeof(List))
		{
			return ((List)value).ContainsWorker(other);
		}
		return ((CallSite<Func<CallSite, TOther, object, bool>>)site).Update(site, other, value);
	}

	private bool TupleContains(CallSite site, object other, PythonTuple value)
	{
		return value.Contains(other);
	}

	private bool TupleContains<TOther>(CallSite site, TOther other, object value)
	{
		if (value != null && value.GetType() == typeof(PythonTuple))
		{
			return ((PythonTuple)value).Contains(other);
		}
		return ((CallSite<Func<CallSite, TOther, object, bool>>)site).Update(site, other, value);
	}

	private bool StringContains(CallSite site, string other, string value)
	{
		if (other != null && value != null)
		{
			return StringOps.__contains__(value, other);
		}
		return ((CallSite<Func<CallSite, string, string, bool>>)site).Update(site, other, value);
	}

	private bool StringContains(CallSite site, object other, string value)
	{
		if (other is string && value != null)
		{
			return StringOps.__contains__(value, (string)other);
		}
		return ((CallSite<Func<CallSite, object, string, bool>>)site).Update(site, other, value);
	}

	private bool StringContains(CallSite site, string other, object value)
	{
		if (value is string && other != null)
		{
			return StringOps.__contains__((string)value, other);
		}
		return ((CallSite<Func<CallSite, string, object, bool>>)site).Update(site, other, value);
	}

	private bool StringContains(CallSite site, object other, object value)
	{
		if (value is string && other is string)
		{
			return StringOps.__contains__((string)value, (string)other);
		}
		return ((CallSite<Func<CallSite, object, object, bool>>)site).Update(site, other, value);
	}

	private bool SetContains(CallSite site, object other, object value)
	{
		if (value != null && value.GetType() == typeof(SetCollection))
		{
			return ((SetCollection)value).__contains__(other);
		}
		return ((CallSite<Func<CallSite, object, object, bool>>)site).Update(site, other, value);
	}

	private int HashPythonType(CallSite site, object value)
	{
		if (value != null && value.GetType() == typeof(PythonType))
		{
			return value.GetHashCode();
		}
		return ((CallSite<Func<CallSite, object, int>>)site).Update(site, value);
	}

	private int HashOldClass(CallSite site, object value)
	{
		if (value is OldClass)
		{
			return value.GetHashCode();
		}
		return ((CallSite<Func<CallSite, object, int>>)site).Update(site, value);
	}

	private int CompareStrings(CallSite site, object arg0, object arg1)
	{
		if (arg0 != null && arg0.GetType() == typeof(string) && arg1 != null && arg1.GetType() == typeof(string))
		{
			return StringOps.Compare((string)arg0, (string)arg1);
		}
		return ((CallSite<Func<CallSite, object, object, int>>)site).Update(site, arg0, arg1);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ _context.Binder.GetHashCode() ^ _operation.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PythonOperationBinder pythonOperationBinder))
		{
			return false;
		}
		if (pythonOperationBinder._context.Binder == _context.Binder)
		{
			return base.Equals(obj);
		}
		return false;
	}

	public override string ToString()
	{
		return "Python " + Operation;
	}

	public Expression CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeOperationAction"), BindingHelpers.CreateBinderStateExpression(), Utils.Constant((int)Operation));
	}
}

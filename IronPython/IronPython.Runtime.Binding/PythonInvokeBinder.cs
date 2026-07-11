using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class PythonInvokeBinder : DynamicMetaObjectBinder, IPythonSite, IExpressionSerializable, ILightExceptionBinder
{
	private class LightThrowBinder : PythonInvokeBinder
	{
		public override bool SupportsLightThrow => true;

		public LightThrowBinder(PythonContext context, CallSignature signature)
			: base(context, signature)
		{
		}

		public override CallSiteBinder GetLightExceptionBinder()
		{
			return this;
		}
	}

	private readonly PythonContext _context;

	private readonly CallSignature _signature;

	private LightThrowBinder _lightThrowBinder;

	public CallSignature Signature => _signature;

	public PythonContext Context => _context;

	public virtual bool SupportsLightThrow => false;

	public PythonInvokeBinder(PythonContext context, CallSignature signature)
	{
		_context = context;
		_signature = signature;
	}

	public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
	{
		DynamicMetaObject target2 = args[0];
		args = ArrayUtils.RemoveFirst(args);
		return BindWorker(target, target2, args);
	}

	private DynamicMetaObject BindWorker(DynamicMetaObject context, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		if (target is IPythonInvokable pythonInvokable)
		{
			return pythonInvokable.Invoke(this, context.Expression, target, args);
		}
		if (target.Value is IDynamicMetaObjectProvider)
		{
			return InvokeForeignObject(target, args);
		}
		if (ComBinder.CanComBind(target.Value))
		{
			return InvokeForeignObject(target, args);
		}
		return Fallback(context.Expression, target, args);
	}

	public override T BindDelegate<T>(CallSite<T> site, object[] args)
	{
		if (args[1] is IFastInvokable fastInvokable)
		{
			FastBindResult<T> fastBindResult = fastInvokable.MakeInvokeBinding(site, this, (CodeContext)args[0], ArrayUtils.ShiftLeft(args, 2));
			if (fastBindResult.Target != null)
			{
				if (fastBindResult.ShouldCache)
				{
					CacheTarget(fastBindResult.Target);
				}
				return fastBindResult.Target;
			}
		}
		_ = args[1] is PythonType;
		T val = this.LightBind<T>(args, Context.Options.CompilationThreshold);
		CacheTarget(val);
		return val;
	}

	internal DynamicMetaObject Fallback(Expression codeContext, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		if (target.NeedsDeferral())
		{
			return Defer(args);
		}
		return PythonProtocol.Call(this, target, args) ?? Context.Binder.Create(Signature, target, args, codeContext) ?? Context.Binder.Call(Signature, new PythonOverloadResolverFactory(Context.Binder, codeContext), target, args);
	}

	public override int GetHashCode()
	{
		return _signature.GetHashCode() ^ _context.Binder.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PythonInvokeBinder pythonInvokeBinder))
		{
			return false;
		}
		if (pythonInvokeBinder._context.Binder == _context.Binder)
		{
			return _signature == pythonInvokeBinder._signature;
		}
		return false;
	}

	public override string ToString()
	{
		return "Python Invoke " + Signature.ToString();
	}

	internal DynamicMetaObject InvokeForeignObject(DynamicMetaObject target, DynamicMetaObject[] args)
	{
		TranslateArguments(target, args, out var callInfo, out var metaArgs, out var test, out var restrictions);
		return BindingHelpers.AddDynamicTestAndDefer(this, new DynamicMetaObject(Expression.Dynamic(_context.CompatInvoke(callInfo), typeof(object), metaArgs.ToArray()), restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target.Expression, target.GetLimitType()))), args, new ValidationInfo(test));
	}

	private void TranslateArguments(DynamicMetaObject target, DynamicMetaObject[] args, out CallInfo callInfo, out List<Expression> metaArgs, out Expression test, out BindingRestrictions restrictions)
	{
		Argument[] argumentInfos = _signature.GetArgumentInfos();
		List<string> list = new List<string>();
		metaArgs = new List<Expression>();
		metaArgs.Add(target.Expression);
		Expression expression = null;
		Expression expression2 = null;
		restrictions = BindingRestrictions.Empty;
		for (int i = 0; i < argumentInfos.Length; i++)
		{
			Argument argument = argumentInfos[i];
			switch (argument.Kind)
			{
			case ArgumentType.Dictionary:
			{
				PythonDictionary pythonDictionary = (PythonDictionary)args[i].Value;
				List<string> list3 = new List<string>();
				foreach (KeyValuePair<object, object> item in pythonDictionary)
				{
					string text = (string)item.Key;
					list.Add(text);
					list3.Add(text);
					metaArgs.Add(Expression.Call(Utils.Convert(args[i].Expression, typeof(PythonDictionary)), typeof(PythonDictionary).GetMethod("get_Item", new Type[1] { typeof(object) }), Utils.Constant(text)));
				}
				restrictions = restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(args[i].Expression, args[i].GetLimitType()));
				expression2 = Expression.Call(typeof(PythonOps).GetMethod("CheckDictionaryMembers"), Utils.Convert(args[i].Expression, typeof(PythonDictionary)), Utils.Constant(list3.ToArray()));
				break;
			}
			case ArgumentType.List:
			{
				IList<object> list2 = (IList<object>)args[i].Value;
				expression = Expression.Equal(Expression.Property(Utils.Convert(args[i].Expression, args[i].GetLimitType()), typeof(ICollection<object>).GetProperty("Count")), Utils.Constant(list2.Count));
				for (int j = 0; j < list2.Count; j++)
				{
					metaArgs.Add(Expression.Call(Utils.Convert(args[i].Expression, typeof(IList<object>)), typeof(IList<object>).GetMethod("get_Item"), Utils.Constant(j)));
				}
				restrictions = restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(args[i].Expression, args[i].GetLimitType()));
				break;
			}
			case ArgumentType.Named:
				list.Add(argument.Name);
				metaArgs.Add(args[i].Expression);
				break;
			case ArgumentType.Simple:
				metaArgs.Add(args[i].Expression);
				break;
			default:
				throw new InvalidOperationException();
			}
		}
		callInfo = new CallInfo(metaArgs.Count - 1, list.ToArray());
		test = expression;
		if (expression2 != null)
		{
			if (test != null)
			{
				test = Expression.AndAlso(test, expression2);
			}
			else
			{
				test = expression2;
			}
		}
	}

	public virtual Expression CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeInvokeAction"), BindingHelpers.CreateBinderStateExpression(), Signature.CreateExpression());
	}

	public virtual CallSiteBinder GetLightExceptionBinder()
	{
		if (_lightThrowBinder == null)
		{
			_lightThrowBinder = new LightThrowBinder(_context, _signature);
		}
		return _lightThrowBinder;
	}

	public CallSiteBinder GetLightExceptionBinder(bool really)
	{
		if (really)
		{
			return GetLightExceptionBinder();
		}
		return this;
	}
}

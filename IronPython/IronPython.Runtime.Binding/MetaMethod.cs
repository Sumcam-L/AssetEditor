using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class MetaMethod : MetaPythonObject, IPythonInvokable, IPythonConvertible
{
	public new Method Value => (Method)base.Value;

	public MetaMethod(Expression expression, BindingRestrictions restrictions, Method value)
		: base(expression, BindingRestrictions.Empty, value)
	{
	}

	public DynamicMetaObject Invoke(PythonInvokeBinder pythonInvoke, Expression codeContext, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		return InvokeWorker(pythonInvoke, args);
	}

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder action, DynamicMetaObject[] args)
	{
		return BindingHelpers.GenericInvokeMember(action, null, this, args);
	}

	public override DynamicMetaObject BindInvoke(InvokeBinder callAction, DynamicMetaObject[] args)
	{
		return InvokeWorker(callAction, args);
	}

	public override DynamicMetaObject BindConvert(ConvertBinder conversion)
	{
		return ConvertWorker(conversion, conversion.Type, conversion.Explicit ? ConversionResultKind.ExplicitCast : ConversionResultKind.ImplicitCast);
	}

	public DynamicMetaObject BindConvert(PythonConversionBinder binder)
	{
		return ConvertWorker(binder, binder.Type, binder.ResultKind);
	}

	public DynamicMetaObject ConvertWorker(DynamicMetaObjectBinder binder, Type toType, ConversionResultKind kind)
	{
		if (toType.IsSubclassOf(typeof(Delegate)))
		{
			return MetaPythonObject.MakeDelegateTarget(binder, toType, Restrict(typeof(Method)));
		}
		return FallbackConvert(binder);
	}

	private DynamicMetaObject InvokeWorker(DynamicMetaObjectBinder callAction, DynamicMetaObject[] args)
	{
		CallSignature callSignature = BindingHelpers.GetCallSignature(callAction);
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(Method));
		BindingRestrictions restrictions = dynamicMetaObject.Restrictions;
		DynamicMetaObject metaFunction = GetMetaFunction(dynamicMetaObject);
		DynamicMetaObject dynamicMetaObject2;
		if (Value.im_self == null)
		{
			restrictions = restrictions.Merge(BindingRestrictions.GetExpressionRestriction(Expression.Equal(GetSelfExpression(dynamicMetaObject), Utils.Constant(null))));
			dynamicMetaObject2 = ((args.Length != 0) ? new DynamicMetaObject(Expression.Block(MakeCheckSelf(callAction, callSignature, args), Expression.Dynamic(PythonContext.GetPythonContext(callAction).Invoke(BindingHelpers.GetCallSignature(callAction)).GetLightExceptionBinder(callAction.SupportsLightThrow()), typeof(object), ArrayUtils.Insert(PythonContext.GetCodeContext(callAction), DynamicUtils.GetExpressions(ArrayUtils.Insert(metaFunction, args))))), BindingRestrictions.Empty) : new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("MethodCheckSelf"), PythonContext.GetCodeContext(callAction), dynamicMetaObject.Expression, Utils.Constant(null)), restrictions));
		}
		else
		{
			restrictions = restrictions.Merge(BindingRestrictions.GetExpressionRestriction(Expression.NotEqual(GetSelfExpression(dynamicMetaObject), Utils.Constant(null))));
			DynamicMetaObject metaSelf = GetMetaSelf(dynamicMetaObject);
			DynamicMetaObject[] objects = ArrayUtils.Insert(metaFunction, metaSelf, args);
			CallSignature signature = new CallSignature(ArrayUtils.Insert(new Argument(ArgumentType.Simple), callSignature.GetArgumentInfos()));
			dynamicMetaObject2 = new DynamicMetaObject(Expression.Dynamic(PythonContext.GetPythonContext(callAction).Invoke(signature).GetLightExceptionBinder(callAction.SupportsLightThrow()), typeof(object), ArrayUtils.Insert(PythonContext.GetCodeContext(callAction), DynamicUtils.GetExpressions(objects))), BindingRestrictions.Empty);
		}
		if (dynamicMetaObject2.HasValue)
		{
			return new DynamicMetaObject(dynamicMetaObject2.Expression, restrictions.Merge(dynamicMetaObject2.Restrictions), dynamicMetaObject2.Value);
		}
		return new DynamicMetaObject(dynamicMetaObject2.Expression, restrictions.Merge(dynamicMetaObject2.Restrictions));
	}

	private DynamicMetaObject GetMetaSelf(DynamicMetaObject self)
	{
		if (Value.im_self is IDynamicMetaObjectProvider dynamicMetaObjectProvider)
		{
			return dynamicMetaObjectProvider.GetMetaObject(GetSelfExpression(self));
		}
		if (Value.im_self == null)
		{
			return new DynamicMetaObject(GetSelfExpression(self), BindingRestrictions.Empty);
		}
		return new DynamicMetaObject(GetSelfExpression(self), BindingRestrictions.Empty, Value.im_self);
	}

	private DynamicMetaObject GetMetaFunction(DynamicMetaObject self)
	{
		if (Value.im_func is IDynamicMetaObjectProvider dynamicMetaObjectProvider)
		{
			return dynamicMetaObjectProvider.GetMetaObject(GetFunctionExpression(self));
		}
		return new DynamicMetaObject(GetFunctionExpression(self), BindingRestrictions.Empty);
	}

	private static MemberExpression GetFunctionExpression(DynamicMetaObject self)
	{
		return Expression.Property(self.Expression, typeof(Method).GetProperty("im_func"));
	}

	private static MemberExpression GetSelfExpression(DynamicMetaObject self)
	{
		return Expression.Property(self.Expression, typeof(Method).GetProperty("im_self"));
	}

	private Expression MakeCheckSelf(DynamicMetaObjectBinder binder, CallSignature signature, DynamicMetaObject[] args)
	{
		switch (signature.GetArgumentKind(0))
		{
		case ArgumentType.Simple:
		case ArgumentType.Instance:
			return CheckSelf(binder, Utils.Convert(base.Expression, typeof(Method)), args[0].Expression);
		default:
			return CheckSelf(binder, Utils.Convert(base.Expression, typeof(Method)), Utils.Constant(null));
		case ArgumentType.List:
			return CheckSelf(binder, Utils.Convert(base.Expression, typeof(Method)), Expression.Condition(Expression.AndAlso(Expression.TypeIs(args[0].Expression, typeof(IList<object>)), Expression.NotEqual(Expression.Property(Expression.Convert(args[0].Expression, typeof(ICollection)), typeof(ICollection).GetProperty("Count")), Utils.Constant(0))), Expression.Call(Expression.Convert(args[0].Expression, typeof(IList<object>)), typeof(IList<object>).GetMethod("get_Item"), Utils.Constant(0)), Utils.Constant(null)));
		}
	}

	private static Expression CheckSelf(DynamicMetaObjectBinder binder, Expression method, Expression inst)
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MethodCheckSelf"), PythonContext.GetCodeContext(binder), method, Utils.Convert(inst, typeof(object)));
	}
}

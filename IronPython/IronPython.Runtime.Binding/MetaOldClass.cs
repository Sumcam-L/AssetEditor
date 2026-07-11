using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class MetaOldClass : MetaPythonObject, IPythonInvokable, IPythonGetable, IPythonOperable, IPythonConvertible
{
	public new OldClass Value => (OldClass)base.Value;

	public MetaOldClass(Expression expression, BindingRestrictions restrictions, OldClass value)
		: base(expression, BindingRestrictions.Empty, value)
	{
	}

	public DynamicMetaObject Invoke(PythonInvokeBinder pythonInvoke, Expression codeContext, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		return MakeCallRule(pythonInvoke, codeContext, args);
	}

	public DynamicMetaObject GetMember(PythonGetMemberBinder member, DynamicMetaObject codeContext)
	{
		return MakeGetMember(member, codeContext);
	}

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder action, DynamicMetaObject[] args)
	{
		return BindingHelpers.GenericInvokeMember(action, null, this, args);
	}

	public override DynamicMetaObject BindInvoke(InvokeBinder call, DynamicMetaObject[] args)
	{
		return MakeCallRule(call, Utils.Constant(PythonContext.GetPythonContext(call).SharedContext), args);
	}

	public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder create, DynamicMetaObject[] args)
	{
		return MakeCallRule(create, Utils.Constant(PythonContext.GetPythonContext(create).SharedContext), args);
	}

	public override DynamicMetaObject BindGetMember(GetMemberBinder member)
	{
		return MakeGetMember(member, PythonContext.GetCodeContextMO(member));
	}

	public override DynamicMetaObject BindSetMember(SetMemberBinder member, DynamicMetaObject value)
	{
		return MakeSetMember(member.Name, value);
	}

	public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder member)
	{
		return MakeDeleteMember(member);
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
			return MetaPythonObject.MakeDelegateTarget(binder, toType, Restrict(typeof(OldClass)));
		}
		return FallbackConvert(binder);
	}

	public override IEnumerable<string> GetDynamicMemberNames()
	{
		foreach (object o in ((IPythonMembersList)Value).GetMemberNames(DefaultContext.Default))
		{
			if (o is string)
			{
				yield return (string)o;
			}
		}
	}

	private DynamicMetaObject MakeCallRule(DynamicMetaObjectBinder call, Expression codeContext, DynamicMetaObject[] args)
	{
		CallSignature callSignature = BindingHelpers.GetCallSignature(call);
		Expression[] array = new Expression[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			array[i] = args[i].Expression;
		}
		ParameterExpression parameterExpression = Expression.Variable(typeof(object), "init");
		ParameterExpression parameterExpression2 = Expression.Variable(typeof(object), "inst");
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldClass));
		return new DynamicMetaObject(Expression.Block(new ParameterExpression[2] { parameterExpression, parameterExpression2 }, Expression.Assign(parameterExpression2, Expression.New(typeof(OldInstance).GetConstructor(new Type[2]
		{
			typeof(CodeContext),
			typeof(OldClass)
		}), codeContext, dynamicMetaObject.Expression)), Expression.Condition(Expression.Not(Expression.TypeIs(Expression.Assign(parameterExpression, Expression.Call(typeof(PythonOps).GetMethod("OldClassTryLookupInit"), dynamicMetaObject.Expression, parameterExpression2)), typeof(OperationFailed))), Expression.Dynamic(PythonContext.GetPythonContext(call).Invoke(callSignature), typeof(object), ArrayUtils.Insert(codeContext, parameterExpression, array)), NoInitCheckNoArgs(callSignature, dynamicMetaObject, args)), parameterExpression2), dynamicMetaObject.Restrictions.Merge(BindingRestrictions.Combine(args)));
	}

	private static Expression NoInitCheckNoArgs(CallSignature signature, DynamicMetaObject self, DynamicMetaObject[] args)
	{
		int unusedCount = args.Length;
		Expression argumentExpression = GetArgumentExpression(signature, ArgumentType.Dictionary, ref unusedCount, args);
		Expression argumentExpression2 = GetArgumentExpression(signature, ArgumentType.List, ref unusedCount, args);
		if (signature.IsSimple || unusedCount > 0)
		{
			if (args.Length > 0)
			{
				return Expression.Call(typeof(PythonOps).GetMethod("OldClassMakeCallError"), self.Expression);
			}
			return Utils.Constant(null);
		}
		return Expression.Call(typeof(PythonOps).GetMethod("OldClassCheckCallError"), self.Expression, argumentExpression, argumentExpression2);
	}

	private static Expression GetArgumentExpression(CallSignature signature, ArgumentType kind, ref int unusedCount, DynamicMetaObject[] args)
	{
		int num = signature.IndexOf(kind);
		if (num != -1)
		{
			unusedCount--;
			return args[num].Expression;
		}
		return Utils.Constant(null);
	}

	public static object MakeCallError()
	{
		throw PythonOps.TypeError("this constructor takes no arguments");
	}

	private DynamicMetaObject MakeSetMember(string name, DynamicMetaObject value)
	{
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldClass));
		Expression expression = Utils.Convert(value.Expression, typeof(object));
		return new DynamicMetaObject(name switch
		{
			"__bases__" => Expression.Call(typeof(PythonOps).GetMethod("OldClassSetBases"), dynamicMetaObject.Expression, expression), 
			"__name__" => Expression.Call(typeof(PythonOps).GetMethod("OldClassSetName"), dynamicMetaObject.Expression, expression), 
			"__dict__" => Expression.Call(typeof(PythonOps).GetMethod("OldClassSetDictionary"), dynamicMetaObject.Expression, expression), 
			_ => Expression.Call(typeof(PythonOps).GetMethod("OldClassSetNameHelper"), dynamicMetaObject.Expression, Utils.Constant(name), expression), 
		}, dynamicMetaObject.Restrictions.Merge(value.Restrictions));
	}

	private DynamicMetaObject MakeDeleteMember(DeleteMemberBinder member)
	{
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldClass));
		return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("OldClassDeleteMember"), Utils.Constant(PythonContext.GetPythonContext(member).SharedContext), dynamicMetaObject.Expression, Utils.Constant(member.Name)), dynamicMetaObject.Restrictions);
	}

	private DynamicMetaObject MakeGetMember(DynamicMetaObjectBinder member, DynamicMetaObject codeContext)
	{
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldClass));
		string getMemberName = MetaPythonObject.GetGetMemberName(member);
		Expression expression;
		switch (getMemberName)
		{
		case "__dict__":
			expression = Expression.Block(Expression.Call(typeof(PythonOps).GetMethod("OldClassDictionaryIsPublic"), dynamicMetaObject.Expression), Expression.Call(typeof(PythonOps).GetMethod("OldClassGetDictionary"), dynamicMetaObject.Expression));
			break;
		case "__bases__":
			expression = Expression.Call(typeof(PythonOps).GetMethod("OldClassGetBaseClasses"), dynamicMetaObject.Expression);
			break;
		case "__name__":
			expression = Expression.Call(typeof(PythonOps).GetMethod("OldClassGetName"), dynamicMetaObject.Expression);
			break;
		default:
		{
			ParameterExpression parameterExpression = Expression.Variable(typeof(object), "lookupVal");
			return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.Not(Expression.TypeIs(Expression.Assign(parameterExpression, Expression.Call(typeof(PythonOps).GetMethod("OldClassTryLookupValue"), Utils.Constant(PythonContext.GetPythonContext(member).SharedContext), dynamicMetaObject.Expression, Utils.Constant(getMemberName))), typeof(OperationFailed))), parameterExpression, Utils.Convert(MetaPythonObject.GetMemberFallback(this, member, codeContext).Expression, typeof(object)))), dynamicMetaObject.Restrictions);
		}
		}
		return new DynamicMetaObject(expression, dynamicMetaObject.Restrictions);
	}

	DynamicMetaObject IPythonOperable.BindOperation(PythonOperationBinder action, DynamicMetaObject[] args)
	{
		if (action.Operation == PythonOperationKind.IsCallable)
		{
			return new DynamicMetaObject(Utils.Constant(true), base.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(base.Expression, typeof(OldClass))));
		}
		return null;
	}
}

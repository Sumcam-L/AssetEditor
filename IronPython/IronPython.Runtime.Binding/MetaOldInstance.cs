using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class MetaOldInstance : MetaPythonObject, IPythonInvokable, IPythonGetable, IPythonOperable, IPythonConvertible
{
	private enum MemberAccess
	{
		Get,
		Set,
		Delete,
		Invoke
	}

	public new OldInstance Value => (OldInstance)base.Value;

	public MetaOldInstance(Expression expression, BindingRestrictions restrictions, OldInstance value)
		: base(expression, BindingRestrictions.Empty, value)
	{
	}

	public DynamicMetaObject Invoke(PythonInvokeBinder pythonInvoke, Expression codeContext, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		return InvokeWorker(pythonInvoke, codeContext, args);
	}

	public DynamicMetaObject GetMember(PythonGetMemberBinder member, DynamicMetaObject codeContext)
	{
		return MakeMemberAccess(member, member.Name, MemberAccess.Get, this);
	}

	public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder action, DynamicMetaObject[] args)
	{
		return MakeMemberAccess(action, action.Name, MemberAccess.Invoke, args);
	}

	public override DynamicMetaObject BindGetMember(GetMemberBinder member)
	{
		return MakeMemberAccess(member, member.Name, MemberAccess.Get, this);
	}

	public override DynamicMetaObject BindSetMember(SetMemberBinder member, DynamicMetaObject value)
	{
		return MakeMemberAccess(member, member.Name, MemberAccess.Set, this, value);
	}

	public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder member)
	{
		return MakeMemberAccess(member, member.Name, MemberAccess.Delete, this);
	}

	public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
	{
		return PythonProtocol.Operation(binder, this, arg, null);
	}

	public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
	{
		return PythonProtocol.Operation(binder, this, null);
	}

	public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
	{
		return PythonProtocol.Index(binder, PythonIndexType.GetItem, ArrayUtils.Insert(this, indexes));
	}

	public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
	{
		return PythonProtocol.Index(binder, PythonIndexType.SetItem, ArrayUtils.Insert(this, ArrayUtils.Append(indexes, value)));
	}

	public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
	{
		return PythonProtocol.Index(binder, PythonIndexType.DeleteItem, ArrayUtils.Insert(this, indexes));
	}

	public override DynamicMetaObject BindConvert(ConvertBinder conversion)
	{
		return ConvertWorker(conversion, conversion.Type, conversion.Type, conversion.Explicit ? ConversionResultKind.ExplicitCast : ConversionResultKind.ImplicitCast);
	}

	public DynamicMetaObject BindConvert(PythonConversionBinder binder)
	{
		return ConvertWorker(binder, binder.Type, binder.ReturnType, binder.ResultKind);
	}

	public DynamicMetaObject ConvertWorker(DynamicMetaObjectBinder binder, Type type, Type retType, ConversionResultKind kind)
	{
		if (!type.IsEnum())
		{
			switch (type.GetTypeCode())
			{
			case TypeCode.Boolean:
				return MakeConvertToBool(binder);
			case TypeCode.Int32:
				return MakeConvertToCommon(binder, type, retType, "__int__");
			case TypeCode.Double:
				return MakeConvertToCommon(binder, type, retType, "__float__");
			case TypeCode.String:
				return MakeConvertToCommon(binder, type, retType, "__str__");
			case TypeCode.Object:
				if (type == typeof(BigInteger))
				{
					return MakeConvertToCommon(binder, type, retType, "__long__");
				}
				if (type == typeof(Complex))
				{
					return MakeConvertToCommon(binder, type, retType, "__complex__");
				}
				if (type == typeof(IEnumerable))
				{
					return MakeConvertToIEnumerable(binder);
				}
				if (type == typeof(IEnumerator))
				{
					return MakeConvertToIEnumerator(binder);
				}
				if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					return MakeConvertToIEnumerable(binder, type, type.GetGenericArguments()[0]);
				}
				if (type.IsSubclassOf(typeof(Delegate)))
				{
					return MetaPythonObject.MakeDelegateTarget(binder, type, Restrict(typeof(OldInstance)));
				}
				break;
			}
		}
		return FallbackConvert(binder);
	}

	public override DynamicMetaObject BindInvoke(InvokeBinder invoke, DynamicMetaObject[] args)
	{
		return InvokeWorker(invoke, PythonContext.GetCodeContext(invoke), args);
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

	private DynamicMetaObject InvokeWorker(DynamicMetaObjectBinder invoke, Expression codeContext, DynamicMetaObject[] args)
	{
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldInstance));
		Expression[] array = new Expression[args.Length + 1];
		for (int i = 0; i < args.Length; i++)
		{
			array[i + 1] = args[i].Expression;
		}
		ParameterExpression parameterExpression = (ParameterExpression)(array[0] = Expression.Variable(typeof(object), "callFunc"));
		return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.Not(Expression.TypeIs(Expression.Assign(parameterExpression, Expression.Call(typeof(PythonOps).GetMethod("OldInstanceTryGetBoundCustomMember"), codeContext, dynamicMetaObject.Expression, Utils.Constant("__call__"))), typeof(OperationFailed))), Expression.Block(Utils.Try(Expression.Call(typeof(PythonOps).GetMethod("FunctionPushFrameCodeContext"), codeContext), Expression.Assign(parameterExpression, Expression.Dynamic(PythonContext.GetPythonContext(invoke).Invoke(BindingHelpers.GetCallSignature(invoke)), typeof(object), ArrayUtils.Insert(codeContext, array)))).Finally(Expression.Call(typeof(PythonOps).GetMethod("FunctionPopFrame"))), parameterExpression), Utils.Convert(BindingHelpers.InvokeFallback(invoke, codeContext, this, args).Expression, typeof(object)))), dynamicMetaObject.Restrictions.Merge(BindingRestrictions.Combine(args)));
	}

	private DynamicMetaObject MakeConvertToIEnumerable(DynamicMetaObjectBinder conversion)
	{
		ParameterExpression parameterExpression = Expression.Variable(typeof(IEnumerable), "res");
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldInstance));
		return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.NotEqual(Expression.Assign(parameterExpression, Expression.Call(typeof(PythonOps).GetMethod("OldInstanceConvertToIEnumerableNonThrowing"), Utils.Constant(PythonContext.GetPythonContext(conversion).SharedContext), dynamicMetaObject.Expression)), Utils.Constant(null)), parameterExpression, Utils.Convert(Utils.Convert(FallbackConvert(conversion).Expression, typeof(object)), typeof(IEnumerable)))), dynamicMetaObject.Restrictions);
	}

	private DynamicMetaObject MakeConvertToIEnumerator(DynamicMetaObjectBinder conversion)
	{
		ParameterExpression parameterExpression = Expression.Variable(typeof(IEnumerator), "res");
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldInstance));
		return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.NotEqual(Expression.Assign(parameterExpression, Expression.Call(typeof(PythonOps).GetMethod("OldInstanceConvertToIEnumeratorNonThrowing"), Utils.Constant(PythonContext.GetPythonContext(conversion).SharedContext), dynamicMetaObject.Expression)), Utils.Constant(null)), parameterExpression, Utils.Convert(Utils.Convert(FallbackConvert(conversion).Expression, typeof(object)), typeof(IEnumerator)))), dynamicMetaObject.Restrictions);
	}

	private DynamicMetaObject MakeConvertToIEnumerable(DynamicMetaObjectBinder conversion, Type toType, Type genericType)
	{
		ParameterExpression parameterExpression = Expression.Variable(toType, "res");
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldInstance));
		return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.NotEqual(Expression.Assign(parameterExpression, Expression.Call(typeof(PythonOps).GetMethod("OldInstanceConvertToIEnumerableOfTNonThrowing").MakeGenericMethod(genericType), Utils.Constant(PythonContext.GetPythonContext(conversion).SharedContext), dynamicMetaObject.Expression)), Utils.Constant(null)), parameterExpression, Utils.Convert(Utils.Convert(FallbackConvert(conversion).Expression, typeof(object)), toType))), dynamicMetaObject.Restrictions);
	}

	private DynamicMetaObject MakeConvertToCommon(DynamicMetaObjectBinder conversion, Type toType, Type retType, string name)
	{
		ParameterExpression parameterExpression = Expression.Variable(typeof(object), "convertResult");
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldInstance));
		return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(MakeOneConvert(conversion, dynamicMetaObject, name, parameterExpression), Expression.Convert(parameterExpression, retType), FallbackConvert(conversion).Expression)), dynamicMetaObject.Restrictions);
	}

	private static BinaryExpression MakeOneConvert(DynamicMetaObjectBinder conversion, DynamicMetaObject self, string name, ParameterExpression tmp)
	{
		return Expression.NotEqual(Expression.Assign(tmp, Expression.Call(typeof(PythonOps).GetMethod("OldInstanceConvertNonThrowing"), Utils.Constant(PythonContext.GetPythonContext(conversion).SharedContext), self.Expression, Utils.Constant(name))), Utils.Constant(null));
	}

	private DynamicMetaObject MakeConvertToBool(DynamicMetaObjectBinder conversion)
	{
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldInstance));
		ParameterExpression parameterExpression = Expression.Variable(typeof(bool?), "tmp");
		DynamicMetaObject dynamicMetaObject2 = FallbackConvert(conversion);
		Type compatibleType = BindingHelpers.GetCompatibleType(typeof(bool), dynamicMetaObject2.Expression.Type);
		return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.NotEqual(Expression.Assign(parameterExpression, Expression.Call(typeof(PythonOps).GetMethod("OldInstanceConvertToBoolNonThrowing"), Utils.Constant(PythonContext.GetPythonContext(conversion).SharedContext), dynamicMetaObject.Expression)), Utils.Constant(null)), Utils.Convert(parameterExpression, compatibleType), Utils.Convert(dynamicMetaObject2.Expression, compatibleType))), dynamicMetaObject.Restrictions);
	}

	private DynamicMetaObject MakeMemberAccess(DynamicMetaObjectBinder member, string name, MemberAccess access, params DynamicMetaObject[] args)
	{
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldInstance));
		CustomInstanceDictionaryStorage dict;
		int customStorageSlot = GetCustomStorageSlot(name, out dict);
		if (customStorageSlot == -1)
		{
			return MakeDynamicMemberAccess(member, name, access, args);
		}
		ParameterExpression parameterExpression = Expression.Variable(typeof(object), "dict");
		ValidationInfo typeTest = new ValidationInfo(Expression.NotEqual(Expression.Assign(parameterExpression, Expression.Call(typeof(PythonOps).GetMethod("OldInstanceGetOptimizedDictionary"), dynamicMetaObject.Expression, Utils.Constant(dict.KeyVersion))), Utils.Constant(null)));
		Expression expression;
		switch (access)
		{
		case MemberAccess.Invoke:
		{
			ParameterExpression parameterExpression2 = Expression.Variable(typeof(object), "value");
			expression = Expression.Block(new ParameterExpression[1] { parameterExpression2 }, Expression.Condition(Expression.Call(typeof(PythonOps).GetMethod("TryOldInstanceDictionaryGetValueHelper"), parameterExpression, Expression.Constant(customStorageSlot), Utils.Convert(base.Expression, typeof(object)), parameterExpression2), Utils.Convert(((InvokeMemberBinder)member).FallbackInvoke(new DynamicMetaObject(parameterExpression2, BindingRestrictions.Empty), args, null).Expression, typeof(object)), Utils.Convert(((InvokeMemberBinder)member).FallbackInvokeMember(dynamicMetaObject, args).Expression, typeof(object))));
			break;
		}
		case MemberAccess.Get:
			expression = Expression.Call(typeof(PythonOps).GetMethod("OldInstanceDictionaryGetValueHelper"), parameterExpression, Utils.Constant(customStorageSlot), Utils.Convert(base.Expression, typeof(object)));
			break;
		case MemberAccess.Set:
			expression = Expression.Call(typeof(PythonOps).GetMethod("OldInstanceDictionarySetExtraValue"), parameterExpression, Utils.Constant(customStorageSlot), Utils.Convert(args[1].Expression, typeof(object)));
			break;
		case MemberAccess.Delete:
			expression = Expression.Call(typeof(PythonOps).GetMethod("OldInstanceDeleteCustomMember"), Utils.Constant(PythonContext.GetPythonContext(member).SharedContext), Utils.Convert(base.Expression, typeof(OldInstance)), Utils.Constant(name));
			break;
		default:
			throw new InvalidOperationException();
		}
		return BindingHelpers.AddDynamicTestAndDefer(member, new DynamicMetaObject(expression, BindingRestrictions.Combine(args).Merge(dynamicMetaObject.Restrictions)), args, typeTest, parameterExpression);
	}

	private int GetCustomStorageSlot(string name, out CustomInstanceDictionaryStorage dict)
	{
		dict = Value.Dictionary._storage as CustomInstanceDictionaryStorage;
		if (dict == null || Value._class.HasSetAttr)
		{
			return -1;
		}
		return dict.FindKey(name);
	}

	private DynamicMetaObject MakeDynamicMemberAccess(DynamicMetaObjectBinder member, string name, MemberAccess access, DynamicMetaObject[] args)
	{
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldInstance));
		ParameterExpression parameterExpression = Expression.Variable(typeof(object), "result");
		return new DynamicMetaObject(access switch
		{
			MemberAccess.Invoke => Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.Not(Expression.TypeIs(Expression.Assign(parameterExpression, Expression.Call(typeof(PythonOps).GetMethod("OldInstanceTryGetBoundCustomMember"), Utils.Constant(PythonContext.GetPythonContext(member).SharedContext), dynamicMetaObject.Expression, Utils.Constant(name))), typeof(OperationFailed))), ((InvokeMemberBinder)member).FallbackInvoke(new DynamicMetaObject(parameterExpression, BindingRestrictions.Empty), args, null).Expression, Utils.Convert(((InvokeMemberBinder)member).FallbackInvokeMember(this, args).Expression, typeof(object)))), 
			MemberAccess.Get => Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.Not(Expression.TypeIs(Expression.Assign(parameterExpression, Expression.Call(typeof(PythonOps).GetMethod("OldInstanceTryGetBoundCustomMember"), Utils.Constant(PythonContext.GetPythonContext(member).SharedContext), dynamicMetaObject.Expression, Utils.Constant(name))), typeof(OperationFailed))), parameterExpression, Utils.Convert(FallbackGet(member, args), typeof(object)))), 
			MemberAccess.Set => Expression.Call(typeof(PythonOps).GetMethod("OldInstanceSetCustomMember"), Utils.Constant(PythonContext.GetPythonContext(member).SharedContext), dynamicMetaObject.Expression, Utils.Constant(name), Utils.Convert(args[1].Expression, typeof(object))), 
			MemberAccess.Delete => Expression.Call(typeof(PythonOps).GetMethod("OldInstanceDeleteCustomMember"), Utils.Constant(PythonContext.GetPythonContext(member).SharedContext), dynamicMetaObject.Expression, Utils.Constant(name)), 
			_ => throw new InvalidOperationException(), 
		}, dynamicMetaObject.Restrictions.Merge(BindingRestrictions.Combine(args)));
	}

	private Expression FallbackGet(DynamicMetaObjectBinder member, DynamicMetaObject[] args)
	{
		if (member is GetMemberBinder getMemberBinder)
		{
			return getMemberBinder.FallbackGetMember(args[0]).Expression;
		}
		PythonGetMemberBinder pythonGetMemberBinder = member as PythonGetMemberBinder;
		if (pythonGetMemberBinder.IsNoThrow)
		{
			return Expression.Field(null, typeof(OperationFailed).GetField("Value"));
		}
		return member.Throw(Expression.Call(typeof(PythonOps).GetMethod("AttributeError"), Utils.Constant("{0} instance has no attribute '{1}'"), Expression.NewArrayInit(typeof(object), Utils.Constant(Value._class._name), Utils.Constant(pythonGetMemberBinder.Name))));
	}

	DynamicMetaObject IPythonOperable.BindOperation(PythonOperationBinder action, DynamicMetaObject[] args)
	{
		if (action.Operation == PythonOperationKind.IsCallable)
		{
			return MakeIsCallable(action);
		}
		return null;
	}

	private DynamicMetaObject MakeIsCallable(PythonOperationBinder operation)
	{
		DynamicMetaObject dynamicMetaObject = Restrict(typeof(OldInstance));
		return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("OldInstanceIsCallable"), Utils.Constant(PythonContext.GetPythonContext(operation).SharedContext), dynamicMetaObject.Expression), dynamicMetaObject.Restrictions);
	}
}

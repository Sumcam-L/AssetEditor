using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class MetaBuiltinFunction : MetaPythonObject, IPythonInvokable, IPythonOperable, IPythonConvertible
{
	public new BuiltinFunction Value => (BuiltinFunction)base.Value;

	public MetaBuiltinFunction(Expression expression, BindingRestrictions restrictions, BuiltinFunction value)
		: base(expression, BindingRestrictions.Empty, value)
	{
	}

	public override DynamicMetaObject BindInvoke(InvokeBinder call, DynamicMetaObject[] args)
	{
		return InvokeWorker(call, PythonContext.GetCodeContext(call), args);
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
			return MetaPythonObject.MakeDelegateTarget(binder, toType, Restrict(base.LimitType));
		}
		return FallbackConvert(binder);
	}

	DynamicMetaObject IPythonOperable.BindOperation(PythonOperationBinder action, DynamicMetaObject[] args)
	{
		PythonOperationKind operation = action.Operation;
		if (operation == PythonOperationKind.CallSignatures)
		{
			return PythonProtocol.MakeCallSignatureOperation(this, Value.Targets);
		}
		return null;
	}

	public DynamicMetaObject Invoke(PythonInvokeBinder pythonInvoke, Expression codeContext, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		return InvokeWorker(pythonInvoke, codeContext, args);
	}

	private DynamicMetaObject InvokeWorker(DynamicMetaObjectBinder call, Expression codeContext, DynamicMetaObject[] args)
	{
		if (this.NeedsDeferral())
		{
			return call.Defer(ArrayUtils.Insert(this, args));
		}
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].NeedsDeferral())
			{
				return call.Defer(ArrayUtils.Insert(this, args));
			}
		}
		if (Value.IsUnbound)
		{
			return MakeSelflessCall(call, codeContext, args);
		}
		return MakeSelfCall(call, codeContext, args);
	}

	private DynamicMetaObject MakeSelflessCall(DynamicMetaObjectBinder call, Expression codeContext, DynamicMetaObject[] args)
	{
		BindingRestrictions selfRestrict = BindingRestrictions.GetExpressionRestriction(Expression.Equal(base.Expression, Utils.Constant(Value))).Merge(base.Restrictions);
		return Value.MakeBuiltinFunctionCall(call, codeContext, this, args, hasSelf: false, selfRestrict, delegate(DynamicMetaObject[] newArgs)
		{
			PythonBinder binder = PythonContext.GetPythonContext(call).Binder;
			BindingTarget target;
			DynamicMetaObject res = binder.CallMethod(new PythonOverloadResolver(binder, newArgs, BindingHelpers.GetCallSignature(call), codeContext), Value.Targets, selfRestrict, Value.Name, NarrowingLevel.None, Value.IsBinaryOperator ? NarrowingLevel.Two : NarrowingLevel.All, out target);
			return BindingHelpers.CheckLightThrow(call, res, target);
		});
	}

	private DynamicMetaObject MakeSelfCall(DynamicMetaObjectBinder call, Expression codeContext, DynamicMetaObject[] args)
	{
		BindingRestrictions functionRestriction = base.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(base.Expression, base.LimitType)).Merge(BindingRestrictions.GetExpressionRestriction(Value.MakeBoundFunctionTest(Utils.Convert(base.Expression, typeof(BuiltinFunction)))));
		Expression instance = Expression.Call(typeof(PythonOps).GetMethod("GetBuiltinFunctionSelf"), Utils.Convert(base.Expression, typeof(BuiltinFunction)));
		DynamicMetaObject self = GetInstance(instance, CompilerHelpers.GetType(Value.BindingSelf));
		return Value.MakeBuiltinFunctionCall(call, codeContext, this, ArrayUtils.Insert(self, args), hasSelf: true, functionRestriction, delegate(DynamicMetaObject[] newArgs)
		{
			CallSignature callSignature = BindingHelpers.GetCallSignature(call);
			PythonContext pythonContext = PythonContext.GetPythonContext(call);
			PythonOverloadResolver resolver = ((!Value.IsReversedOperator) ? new PythonOverloadResolver(pythonContext.Binder, self, args, callSignature, codeContext) : new PythonOverloadResolver(pythonContext.Binder, newArgs, GetReversedSignature(callSignature), codeContext));
			BindingTarget target;
			DynamicMetaObject res = pythonContext.Binder.CallMethod(resolver, Value.Targets, self.Restrictions, Value.Name, NarrowingLevel.None, Value.IsBinaryOperator ? NarrowingLevel.Two : NarrowingLevel.All, out target);
			return BindingHelpers.CheckLightThrow(call, res, target);
		});
	}

	private DynamicMetaObject GetInstance(Expression instance, Type testType)
	{
		object obj = Value.BindingSelf;
		BindingRestrictions runtimeTypeRestriction = BindingRestrictionsHelpers.GetRuntimeTypeRestriction(instance, testType);
		if (CompilerHelpers.IsStrongBox(obj))
		{
			instance = ReadStrongBoxValue(instance);
			obj = ((IStrongBox)obj).Value;
		}
		else if (!testType.IsEnum())
		{
			Type type = CompilerHelpers.GetType(Value.BindingSelf);
			type = CompilerHelpers.GetVisibleType(type);
			if (type == typeof(object) && Value.DeclaringType.IsInterface())
			{
				type = Value.DeclaringType;
				Type type2 = null;
				if (Value.DeclaringType.IsGenericType() && Value.DeclaringType.FullName == null && Value.DeclaringType.ContainsGenericParameters() && !Value.DeclaringType.IsGenericTypeDefinition())
				{
					Type[] genericArguments = Value.DeclaringType.GetGenericArguments();
					bool flag = genericArguments.Length > 0;
					Type[] array = genericArguments;
					foreach (Type type3 in array)
					{
						if (!type3.IsGenericParameter)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						type2 = Value.DeclaringType.GetGenericTypeDefinition();
					}
				}
				else if (Value.DeclaringType.IsGenericTypeDefinition())
				{
					type2 = Value.DeclaringType;
				}
				if (type2 != null)
				{
					Type[] interfaces = CompilerHelpers.GetType(Value.BindingSelf).GetInterfaces();
					Type[] array2 = interfaces;
					foreach (Type type4 in array2)
					{
						if (type4.IsGenericType() && type4.GetGenericTypeDefinition() == type2)
						{
							type = type4;
							break;
						}
					}
				}
			}
			if (Value.DeclaringType.IsInterface() && type.IsValueType())
			{
				instance = Utils.Convert(instance, Value.DeclaringType);
			}
			else if (type.IsValueType())
			{
				instance = Expression.Unbox(instance, type);
			}
			else
			{
				Type type5 = ((type == typeof(MarshalByRefObject)) ? CompilerHelpers.GetVisibleType(Value.DeclaringType) : type);
				instance = Utils.Convert(instance, type5);
			}
		}
		else
		{
			instance = Utils.Convert(instance, typeof(Enum));
		}
		return new DynamicMetaObject(instance, runtimeTypeRestriction, obj);
	}

	private MemberExpression ReadStrongBoxValue(Expression instance)
	{
		return Expression.Field(Utils.Convert(instance, Value.BindingSelf.GetType()), Value.BindingSelf.GetType().GetField("Value"));
	}

	internal static CallSignature GetReversedSignature(CallSignature signature)
	{
		return new CallSignature(ArrayUtils.Append(signature.GetArgumentInfos(), new Argument(ArgumentType.Simple)));
	}
}

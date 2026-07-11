using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using IronPython.Compiler.Ast;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal static class BindingHelpers
{
	internal static bool TryGetStaticFunction(PythonContext state, string op, DynamicMetaObject mo, out BuiltinFunction function)
	{
		PythonType pythonType = MetaPythonObject.GetPythonType(mo);
		function = null;
		if (!string.IsNullOrEmpty(op) && pythonType.TryResolveSlot(state.SharedContext, op, out var slot) && slot.TryGetValue(state.SharedContext, null, pythonType, out var value))
		{
			function = TryConvertToBuiltinFunction(value);
			if (function == null)
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsNoThrow(DynamicMetaObjectBinder action)
	{
		if (action is PythonGetMemberBinder pythonGetMemberBinder)
		{
			return pythonGetMemberBinder.IsNoThrow;
		}
		return false;
	}

	internal static DynamicMetaObject FilterShowCls(DynamicMetaObject codeContext, DynamicMetaObjectBinder action, DynamicMetaObject res, System.Linq.Expressions.Expression failure)
	{
		if (action is IPythonSite)
		{
			return new DynamicMetaObject(System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.Call(typeof(PythonOps).GetMethod("IsClsVisible"), codeContext.Expression), Utils.Convert(res.Expression, typeof(object)), Utils.Convert(failure, typeof(object))), res.Restrictions);
		}
		return res;
	}

	internal static CallSignature GetCallSignature(DynamicMetaObjectBinder action)
	{
		if (action is PythonInvokeBinder pythonInvokeBinder)
		{
			return pythonInvokeBinder.Signature;
		}
		if (action is InvokeBinder invokeBinder)
		{
			return CallInfoToSignature(invokeBinder.CallInfo);
		}
		if (action is InvokeMemberBinder invokeMemberBinder)
		{
			return CallInfoToSignature(invokeMemberBinder.CallInfo);
		}
		CreateInstanceBinder createInstanceBinder = action as CreateInstanceBinder;
		return CallInfoToSignature(createInstanceBinder.CallInfo);
	}

	public static System.Linq.Expressions.Expression Invoke(System.Linq.Expressions.Expression codeContext, PythonContext binder, Type resultType, CallSignature signature, params System.Linq.Expressions.Expression[] args)
	{
		return System.Linq.Expressions.Expression.Dynamic(binder.Invoke(signature), resultType, ArrayUtils.Insert(codeContext, args));
	}

	internal static DynamicMetaObject GenericInvokeMember(InvokeMemberBinder action, ValidationInfo valInfo, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		if (target.NeedsDeferral())
		{
			return action.Defer(args);
		}
		return AddDynamicTestAndDefer(action, action.FallbackInvoke(new DynamicMetaObject(Binders.Get(PythonContext.GetCodeContext(action), PythonContext.GetPythonContext(action), typeof(object), action.Name, target.Expression), BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target)), args, null), args, valInfo);
	}

	internal static bool NeedsDeferral(DynamicMetaObject[] args)
	{
		foreach (DynamicMetaObject self in args)
		{
			if (self.NeedsDeferral())
			{
				return true;
			}
		}
		return false;
	}

	internal static CallSignature CallInfoToSignature(CallInfo callInfo)
	{
		Argument[] array = new Argument[callInfo.ArgumentCount];
		int num = callInfo.ArgumentCount - callInfo.ArgumentNames.Count;
		int i;
		for (i = 0; i < num; i++)
		{
			ref Argument reference = ref array[i];
			reference = new Argument(ArgumentType.Simple);
		}
		foreach (string argumentName in callInfo.ArgumentNames)
		{
			ref Argument reference2 = ref array[i++];
			reference2 = new Argument(ArgumentType.Named, argumentName);
		}
		return new CallSignature(array);
	}

	internal static Type GetCompatibleType(Type t, Type otherType)
	{
		if (t != otherType)
		{
			if (t.IsAssignableFrom(otherType))
			{
				t = otherType;
			}
			else if (!otherType.IsAssignableFrom(t))
			{
				t = typeof(object);
			}
		}
		return t;
	}

	internal static bool IsSubclassOf(DynamicMetaObject xType, DynamicMetaObject yType)
	{
		PythonType pythonType = MetaPythonObject.GetPythonType(xType);
		PythonType pythonType2 = MetaPythonObject.GetPythonType(yType);
		return pythonType.IsSubclassOf(pythonType2);
	}

	private static BuiltinFunction TryConvertToBuiltinFunction(object o)
	{
		if (o is BuiltinMethodDescriptor builtinMethodDescriptor)
		{
			return builtinMethodDescriptor.Template;
		}
		return o as BuiltinFunction;
	}

	internal static DynamicMetaObject AddDynamicTestAndDefer(DynamicMetaObjectBinder operation, DynamicMetaObject res, DynamicMetaObject[] args, ValidationInfo typeTest, params ParameterExpression[] temps)
	{
		return AddDynamicTestAndDefer(operation, res, args, typeTest, null, temps);
	}

	internal static DynamicMetaObject AddDynamicTestAndDefer(DynamicMetaObjectBinder operation, DynamicMetaObject res, DynamicMetaObject[] args, ValidationInfo typeTest, Type deferType, params ParameterExpression[] temps)
	{
		if (typeTest != null && typeTest.Test != null)
		{
			System.Linq.Expressions.Expression updateExpression = operation.GetUpdateExpression(deferType ?? typeof(object));
			Type compatibleType = GetCompatibleType(updateExpression.Type, res.Expression.Type);
			res = new DynamicMetaObject(System.Linq.Expressions.Expression.Condition(typeTest.Test, Utils.Convert(res.Expression, compatibleType), Utils.Convert(updateExpression, compatibleType)), res.Restrictions);
		}
		if (temps.Length > 0)
		{
			res = new DynamicMetaObject(System.Linq.Expressions.Expression.Block(temps, res.Expression), res.Restrictions, null);
		}
		return res;
	}

	internal static ValidationInfo GetValidationInfo(DynamicMetaObject tested, PythonType type)
	{
		return new ValidationInfo(System.Linq.Expressions.Expression.AndAlso(System.Linq.Expressions.Expression.TypeEqual(tested.Expression, type.UnderlyingSystemType), CheckTypeVersion(Utils.Convert(tested.Expression, type.UnderlyingSystemType), type.Version)));
	}

	internal static ValidationInfo GetValidationInfo(params DynamicMetaObject[] args)
	{
		System.Linq.Expressions.Expression expression = null;
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].HasValue && args[i].Value is IPythonObject pythonObject)
			{
				System.Linq.Expressions.Expression right = CheckTypeVersion(Utils.Convert(args[i].Expression, pythonObject.GetType()), pythonObject.PythonType.Version);
				right = System.Linq.Expressions.Expression.AndAlso(System.Linq.Expressions.Expression.TypeEqual(args[i].Expression, pythonObject.GetType()), right);
				expression = ((expression == null) ? right : System.Linq.Expressions.Expression.AndAlso(expression, right));
			}
		}
		return new ValidationInfo(expression);
	}

	internal static MethodCallExpression CheckTypeVersion(System.Linq.Expressions.Expression tested, int version)
	{
		FieldInfo field = tested.Type.GetField(".class");
		if (field == null)
		{
			return System.Linq.Expressions.Expression.Call(typeof(PythonOps).GetMethod("CheckTypeVersion"), Utils.Convert(tested, typeof(object)), Utils.Constant(version));
		}
		return System.Linq.Expressions.Expression.Call(typeof(PythonOps).GetMethod("CheckSpecificTypeVersion"), System.Linq.Expressions.Expression.Field(tested, field), Utils.Constant(version));
	}

	internal static System.Linq.Expressions.Expression AddRecursionCheck(PythonContext pyContext, System.Linq.Expressions.Expression expr)
	{
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Variable(expr.Type, "callres");
		expr = System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, Utils.Try(System.Linq.Expressions.Expression.Call(typeof(PythonOps).GetMethod("FunctionPushFrame"), System.Linq.Expressions.Expression.Constant(pyContext)), System.Linq.Expressions.Expression.Assign(parameterExpression, expr)).Finally(System.Linq.Expressions.Expression.Call(typeof(PythonOps).GetMethod("FunctionPopFrame"))), parameterExpression);
		return expr;
	}

	internal static System.Linq.Expressions.Expression CreateBinderStateExpression()
	{
		return PythonAst._globalContext;
	}

	internal static DynamicMetaObject InvokeFallback(DynamicMetaObjectBinder action, System.Linq.Expressions.Expression codeContext, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		if (action is InvokeBinder invokeBinder)
		{
			return invokeBinder.FallbackInvoke(target, args);
		}
		if (action is PythonInvokeBinder pythonInvokeBinder)
		{
			return pythonInvokeBinder.Fallback(codeContext, target, args);
		}
		throw new InvalidOperationException();
	}

	internal static System.Linq.Expressions.Expression TypeErrorForProtectedMember(Type type, string name)
	{
		return System.Linq.Expressions.Expression.Throw(System.Linq.Expressions.Expression.Call(typeof(PythonOps).GetMethod("TypeErrorForProtectedMember"), Utils.Constant(type), Utils.Constant(name)), typeof(object));
	}

	internal static DynamicMetaObject TypeErrorGenericMethod(Type type, string name, BindingRestrictions restrictions)
	{
		return new DynamicMetaObject(System.Linq.Expressions.Expression.Throw(System.Linq.Expressions.Expression.Call(typeof(PythonOps).GetMethod("TypeErrorForGenericMethod"), Utils.Constant(type), Utils.Constant(name)), typeof(object)), restrictions);
	}

	internal static bool IsDataMember(object p)
	{
		if (p is PythonFunction || p is BuiltinFunction || p is PythonType || p is BuiltinMethodDescriptor || p is OldClass || p is staticmethod || p is classmethod || p is Method || p is Delegate)
		{
			return false;
		}
		return true;
	}

	internal static DynamicMetaObject AddPythonBoxing(DynamicMetaObject res)
	{
		if (res.Expression.Type.IsValueType())
		{
			res = new DynamicMetaObject(AddPythonBoxing(res.Expression), res.Restrictions);
		}
		return res;
	}

	internal static System.Linq.Expressions.Expression AddPythonBoxing(System.Linq.Expressions.Expression res)
	{
		return Utils.Convert(res, typeof(object));
	}

	internal static DynamicMetaObject[] GetComArguments(DynamicMetaObject[] args)
	{
		DynamicMetaObject[] array = null;
		for (int i = 0; i < args.Length; i++)
		{
			DynamicMetaObject comArgument = GetComArgument(args[i]);
			if (!object.ReferenceEquals(comArgument, args[i]))
			{
				if (array == null)
				{
					array = new DynamicMetaObject[args.Length];
					for (int j = 0; j < i; j++)
					{
						array[j] = args[j];
					}
				}
				array[i] = comArgument;
			}
			else if (array != null)
			{
				array[i] = args[i];
			}
		}
		return array ?? args;
	}

	internal static DynamicMetaObject GetComArgument(DynamicMetaObject arg)
	{
		if (arg is IComConvertible comConvertible)
		{
			return comConvertible.GetComMetaObject();
		}
		if (arg.Value != null)
		{
			Type type = arg.Value.GetType();
			if (type == typeof(BigInteger))
			{
				return new DynamicMetaObject(System.Linq.Expressions.Expression.Convert(Utils.Convert(arg.Expression, typeof(BigInteger)), typeof(double)), BindingRestrictions.GetTypeRestriction(arg.Expression, type));
			}
		}
		return arg;
	}

	internal static BuiltinFunction.BindingResult CheckLightThrow(DynamicMetaObjectBinder call, DynamicMetaObject res, BindingTarget target)
	{
		return new BuiltinFunction.BindingResult(target, CheckLightThrowMO(call, res, target));
	}

	internal static DynamicMetaObject CheckLightThrowMO(DynamicMetaObjectBinder call, DynamicMetaObject res, BindingTarget target)
	{
		if (target.Success && target.Overload.ReflectionInfo.IsDefined(typeof(LightThrowingAttribute), inherit: false) && !call.SupportsLightThrow())
		{
			res = new DynamicMetaObject(LightExceptions.CheckAndThrow(res.Expression), res.Restrictions);
		}
		return res;
	}
}

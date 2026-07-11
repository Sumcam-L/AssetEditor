using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal sealed class SlotOrFunction
{
	private readonly BindingTarget _function;

	private readonly DynamicMetaObject _target;

	private readonly PythonTypeSlot _slot;

	public static readonly SlotOrFunction Empty = new SlotOrFunction(new DynamicMetaObject(Utils.Empty(), BindingRestrictions.Empty));

	public NarrowingLevel NarrowingLevel
	{
		get
		{
			if (_function != null)
			{
				return _function.NarrowingLevel;
			}
			return NarrowingLevel.None;
		}
	}

	public Type ReturnType => _target.GetLimitType();

	public bool MaybeNotImplemented
	{
		get
		{
			if (_function != null)
			{
				MethodInfo methodInfo = _function.Overload.ReflectionInfo as MethodInfo;
				if (methodInfo != null)
				{
					return methodInfo.ReturnTypeCustomAttributes.IsDefined(typeof(MaybeNotImplementedAttribute), inherit: false);
				}
				return false;
			}
			return true;
		}
	}

	public bool Success
	{
		get
		{
			if (_function != null)
			{
				return _function.Success;
			}
			return this != Empty;
		}
	}

	public bool IsNull
	{
		get
		{
			if (_slot is PythonTypeUserDescriptorSlot && ((PythonTypeUserDescriptorSlot)_slot).Value == null)
			{
				return true;
			}
			return false;
		}
	}

	public DynamicMetaObject Target => _target;

	private SlotOrFunction()
	{
	}

	public SlotOrFunction(BindingTarget function, DynamicMetaObject target)
	{
		_target = target;
		_function = function;
	}

	public SlotOrFunction(DynamicMetaObject target)
	{
		_target = target;
	}

	public SlotOrFunction(DynamicMetaObject target, PythonTypeSlot slot)
	{
		_target = target;
		_slot = slot;
	}

	public static bool GetCombinedTargets(SlotOrFunction fCand, SlotOrFunction rCand, out SlotOrFunction fTarget, out SlotOrFunction rTarget)
	{
		fTarget = (rTarget = Empty);
		if (fCand.Success)
		{
			if (rCand.Success)
			{
				if (fCand.NarrowingLevel <= rCand.NarrowingLevel)
				{
					fTarget = fCand;
					rTarget = rCand;
				}
				else
				{
					fTarget = Empty;
					rTarget = rCand;
				}
			}
			else
			{
				fTarget = fCand;
			}
		}
		else
		{
			if (!rCand.Success)
			{
				return false;
			}
			rTarget = rCand;
		}
		return true;
	}

	public bool ShouldWarn(PythonContext context, out WarningInfo info)
	{
		if (_function != null)
		{
			return BindingWarnings.ShouldWarn(context, _function.Overload, out info);
		}
		info = null;
		return false;
	}

	public static SlotOrFunction GetSlotOrFunction(PythonContext state, string op, params DynamicMetaObject[] types)
	{
		PythonTypeSlot slot;
		if (TryGetBinder(state, types, op, null, out var res))
		{
			if (res != Empty)
			{
				return res;
			}
		}
		else if (MetaPythonObject.GetPythonType(types[0]).TryResolveSlot(state.SharedContext, op, out slot))
		{
			ParameterExpression parameterExpression = Expression.Variable(typeof(object), "slotVal");
			Expression[] array = new Expression[types.Length - 1];
			for (int i = 1; i < types.Length; i++)
			{
				array[i - 1] = types[i].Expression;
			}
			return new SlotOrFunction(new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, MetaPythonObject.MakeTryGetTypeMember(state, slot, parameterExpression, types[0].Expression, Expression.Call(typeof(DynamicHelpers).GetMethod("GetPythonType"), types[0].Expression)), Expression.Dynamic(state.Invoke(new CallSignature(array.Length)), typeof(object), ArrayUtils.Insert(Utils.Constant(state.SharedContext), parameterExpression, array))), BindingRestrictions.Combine(types).Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(types[0].Expression, types[0].GetLimitType()))), slot);
		}
		return Empty;
	}

	internal static bool TryGetBinder(PythonContext state, DynamicMetaObject[] types, string op, string rop, out SlotOrFunction res)
	{
		PythonType declaringType;
		return TryGetBinder(state, types, op, rop, out res, out declaringType);
	}

	internal static bool TryGetBinder(PythonContext state, DynamicMetaObject[] types, string op, string rop, out SlotOrFunction res, out PythonType declaringType)
	{
		declaringType = null;
		DynamicMetaObject dynamicMetaObject = types[0];
		if (!BindingHelpers.TryGetStaticFunction(state, op, dynamicMetaObject, out var function))
		{
			res = Empty;
			return false;
		}
		function = CheckAlwaysNotImplemented(function);
		DynamicMetaObject dynamicMetaObject2 = null;
		BuiltinFunction function2 = null;
		if (types.Length > 1)
		{
			dynamicMetaObject2 = types[1];
			if (!BindingHelpers.IsSubclassOf(dynamicMetaObject, dynamicMetaObject2) && !BindingHelpers.TryGetStaticFunction(state, rop, dynamicMetaObject2, out function2))
			{
				res = Empty;
				return false;
			}
			function2 = CheckAlwaysNotImplemented(function2);
		}
		if (function2 == function)
		{
			function2 = null;
		}
		else if (function2 != null && BindingHelpers.IsSubclassOf(dynamicMetaObject2, dynamicMetaObject))
		{
			function = null;
		}
		PythonOverloadResolver resolver = new PythonOverloadResolver(state.Binder, types, new CallSignature(types.Length), Utils.Constant(state.SharedContext));
		DynamicMetaObject dynamicMetaObject3;
		BindingTarget target;
		if (function == null)
		{
			if (function2 == null)
			{
				dynamicMetaObject3 = null;
				target = null;
			}
			else
			{
				declaringType = DynamicHelpers.GetPythonTypeFromType(function2.DeclaringType);
				dynamicMetaObject3 = state.Binder.CallMethod(resolver, function2.Targets, BindingRestrictions.Empty, null, NarrowingLevel.None, NarrowingLevel.Two, out target);
			}
		}
		else if (function2 == null)
		{
			declaringType = DynamicHelpers.GetPythonTypeFromType(function.DeclaringType);
			dynamicMetaObject3 = state.Binder.CallMethod(resolver, function.Targets, BindingRestrictions.Empty, null, NarrowingLevel.None, NarrowingLevel.Two, out target);
		}
		else
		{
			List<MethodBase> list = new List<MethodBase>();
			list.AddRange(function.Targets);
			foreach (MethodBase target2 in function2.Targets)
			{
				if (!ContainsMethodSignature(list, target2))
				{
					list.Add(target2);
				}
			}
			dynamicMetaObject3 = state.Binder.CallMethod(resolver, list.ToArray(), BindingRestrictions.Empty, null, NarrowingLevel.None, NarrowingLevel.Two, out target);
			foreach (MethodBase target3 in function2.Targets)
			{
				if (target.Overload.ReflectionInfo == target3)
				{
					declaringType = DynamicHelpers.GetPythonTypeFromType(function2.DeclaringType);
					break;
				}
			}
			if (declaringType == null)
			{
				declaringType = DynamicHelpers.GetPythonTypeFromType(function.DeclaringType);
			}
		}
		if (dynamicMetaObject3 != null)
		{
			res = new SlotOrFunction(target, dynamicMetaObject3);
		}
		else
		{
			res = Empty;
		}
		return true;
	}

	private static BuiltinFunction CheckAlwaysNotImplemented(BuiltinFunction xBf)
	{
		if (xBf != null)
		{
			bool flag = false;
			foreach (MethodBase target in xBf.Targets)
			{
				if (target.GetReturnType() != typeof(NotImplementedType) || target.IsDefined(typeof(Python3WarningAttribute), inherit: true))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				xBf = null;
			}
		}
		return xBf;
	}

	private static bool ContainsMethodSignature(IList<MethodBase> existing, MethodBase check)
	{
		ParameterInfo[] parameters = check.GetParameters();
		foreach (MethodBase item in existing)
		{
			if (MatchesMethodSignature(parameters, item))
			{
				return true;
			}
		}
		return false;
	}

	private static bool MatchesMethodSignature(ParameterInfo[] pis, MethodBase mb)
	{
		ParameterInfo[] parameters = mb.GetParameters();
		if (pis.Length == parameters.Length)
		{
			for (int i = 0; i < pis.Length; i++)
			{
				if (pis[i].ParameterType != parameters[i].ParameterType)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}
}

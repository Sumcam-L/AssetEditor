using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal static class PythonProtocol
{
	private delegate void ComparisonHelper(ConditionalBuilder bodyBuilder, Expression retCondition, Expression retValue, bool isReverse, Type retType);

	private abstract class Callable
	{
		private readonly PythonContext _binder;

		private readonly PythonIndexType _op;

		protected PythonBinder Binder => _binder.Binder;

		protected PythonContext PythonContext => _binder;

		protected bool IsSetter
		{
			get
			{
				if (_op != PythonIndexType.SetItem)
				{
					return _op == PythonIndexType.SetSlice;
				}
				return true;
			}
		}

		protected Callable(PythonContext binder, PythonIndexType op)
		{
			_binder = binder;
			_op = op;
		}

		public static Callable MakeCallable(PythonContext binder, PythonIndexType op, BuiltinFunction itemFunc, PythonTypeSlot itemSlot)
		{
			if (itemFunc != null)
			{
				return new BuiltinCallable(binder, op, itemFunc);
			}
			if (itemSlot != null)
			{
				return new SlotCallable(binder, op, itemSlot);
			}
			return null;
		}

		public virtual DynamicMetaObject[] GetTupleArguments(DynamicMetaObject[] arguments)
		{
			if (IsSetter)
			{
				if (arguments.Length == 3)
				{
					return arguments;
				}
				Expression[] array = new Expression[arguments.Length - 2];
				BindingRestrictions bindingRestrictions = BindingRestrictions.Empty;
				for (int i = 1; i < arguments.Length - 1; i++)
				{
					array[i - 1] = Utils.Convert(arguments[i].Expression, typeof(object));
					bindingRestrictions = bindingRestrictions.Merge(arguments[i].Restrictions);
				}
				return new DynamicMetaObject[3]
				{
					arguments[0],
					new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("MakeTuple"), Expression.NewArrayInit(typeof(object), array)), bindingRestrictions),
					arguments[arguments.Length - 1]
				};
			}
			if (arguments.Length == 2)
			{
				return arguments;
			}
			Expression[] array2 = new Expression[arguments.Length - 1];
			for (int j = 1; j < arguments.Length; j++)
			{
				array2[j - 1] = Utils.Convert(arguments[j].Expression, typeof(object));
			}
			return new DynamicMetaObject[2]
			{
				arguments[0],
				new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("MakeTuple"), Expression.NewArrayInit(typeof(object), array2)), BindingRestrictions.Combine(ArrayUtils.RemoveFirst(arguments)))
			};
		}

		public abstract DynamicMetaObject CompleteRuleTarget(DynamicMetaObjectBinder metaBinder, DynamicMetaObject[] args, Func<DynamicMetaObject> customFailure);
	}

	private class BuiltinCallable : Callable
	{
		private readonly BuiltinFunction _bf;

		public BuiltinCallable(PythonContext binder, PythonIndexType op, BuiltinFunction func)
			: base(binder, op)
		{
			_bf = func;
		}

		public override DynamicMetaObject[] GetTupleArguments(DynamicMetaObject[] arguments)
		{
			if (arguments[0].GetLimitType() == typeof(OldInstance))
			{
				return base.GetTupleArguments(arguments);
			}
			return arguments;
		}

		public override DynamicMetaObject CompleteRuleTarget(DynamicMetaObjectBinder metaBinder, DynamicMetaObject[] args, Func<DynamicMetaObject> customFailure)
		{
			PythonOverloadResolver pythonOverloadResolver = new PythonOverloadResolver(base.Binder, args[0], ArrayUtils.RemoveFirst(args), new CallSignature(args.Length - 1), Utils.Constant(base.PythonContext.SharedContext));
			DynamicMetaObject res = base.Binder.CallMethod(pythonOverloadResolver, _bf.Targets, BindingRestrictions.Combine(args), _bf.Name, NarrowingLevel.None, NarrowingLevel.Three, out var target);
			res = BindingHelpers.CheckLightThrowMO(metaBinder, res, target);
			if (target.Success)
			{
				if (base.IsSetter)
				{
					res = new DynamicMetaObject(Expression.Block(res.Expression, args[args.Length - 1].Expression), res.Restrictions);
				}
				if (BindingWarnings.ShouldWarn(base.Binder.Context, target.Overload, out var info))
				{
					res = info.AddWarning(Expression.Constant(base.PythonContext.SharedContext), res);
				}
			}
			else if (customFailure == null || (res = customFailure()) == null)
			{
				res = DefaultBinder.MakeError(pythonOverloadResolver.MakeInvalidParametersError(target), BindingRestrictions.Combine(ConvertArgs(args)), typeof(object));
			}
			return res;
		}
	}

	private class SlotCallable : Callable
	{
		private PythonTypeSlot _slot;

		public SlotCallable(PythonContext binder, PythonIndexType op, PythonTypeSlot slot)
			: base(binder, op)
		{
			_slot = slot;
		}

		public override DynamicMetaObject CompleteRuleTarget(DynamicMetaObjectBinder metaBinder, DynamicMetaObject[] args, Func<DynamicMetaObject> customFailure)
		{
			ConditionalBuilder conditionalBuilder = new ConditionalBuilder();
			_slot.MakeGetExpression(base.Binder, Utils.Constant(base.PythonContext.SharedContext), args[0], new DynamicMetaObject(Expression.Call(typeof(DynamicHelpers).GetMethod("GetPythonType"), Utils.Convert(args[0].Expression, typeof(object))), BindingRestrictions.Empty, DynamicHelpers.GetPythonType(args[0].Value)), conditionalBuilder);
			if (!conditionalBuilder.IsFinal)
			{
				conditionalBuilder.FinishCondition(metaBinder.Throw(Expression.New(typeof(InvalidOperationException))));
			}
			Expression expression = conditionalBuilder.GetMetaObject().Expression;
			Expression[] array = new Expression[args.Length - 1];
			for (int i = 1; i < args.Length; i++)
			{
				array[i - 1] = args[i].Expression;
			}
			Expression expression2 = Expression.Dynamic(base.PythonContext.Invoke(new CallSignature(array.Length)), typeof(object), ArrayUtils.Insert(Utils.Constant(base.PythonContext.SharedContext), expression, array));
			if (base.IsSetter)
			{
				expression2 = Expression.Block(expression2, args[args.Length - 1].Expression);
			}
			return new DynamicMetaObject(expression2, BindingRestrictions.Combine(args));
		}
	}

	private abstract class IndexBuilder
	{
		private readonly Callable _callable;

		private readonly DynamicMetaObject[] _types;

		protected Callable Callable => _callable;

		public IndexBuilder(DynamicMetaObject[] types, Callable callable)
		{
			_callable = callable;
			_types = types;
		}

		public abstract DynamicMetaObject MakeRule(DynamicMetaObjectBinder metaBinder, PythonContext binder, DynamicMetaObject[] args);

		protected PythonType GetTypeAt(int index)
		{
			return MetaPythonObject.GetPythonType(_types[index]);
		}
	}

	private class SliceBuilder : IndexBuilder
	{
		private ParameterExpression _lengthVar;

		public SliceBuilder(DynamicMetaObject[] types, Callable callable)
			: base(types, callable)
		{
		}

		public override DynamicMetaObject MakeRule(DynamicMetaObjectBinder metaBinder, PythonContext binder, DynamicMetaObject[] args)
		{
			args = ArrayUtils.Copy(args);
			for (int i = 1; i < 3; i++)
			{
				args[i] = args[i].Restrict(args[i].GetLimitType());
				if (args[i].GetLimitType() == typeof(MissingParameter))
				{
					switch (i)
					{
					case 1:
						args[i] = new DynamicMetaObject(Utils.Constant(0), args[i].Restrictions);
						break;
					case 2:
						args[i] = new DynamicMetaObject(Utils.Constant(int.MaxValue), args[i].Restrictions);
						break;
					}
				}
				else if (args[i].GetLimitType() == typeof(int))
				{
					args[i] = MakeIntTest(args[0], args[i]);
				}
				else if (args[i].GetLimitType().IsSubclassOf(typeof(Extensible<int>)))
				{
					args[i] = MakeIntTest(args[0], new DynamicMetaObject(Expression.Property(args[i].Expression, args[i].GetLimitType().GetProperty("Value")), args[i].Restrictions));
				}
				else if (args[i].GetLimitType() == typeof(BigInteger))
				{
					args[i] = MakeBigIntTest(args[0], args[i]);
				}
				else if (args[i].GetLimitType().IsSubclassOf(typeof(Extensible<BigInteger>)))
				{
					args[i] = MakeBigIntTest(args[0], new DynamicMetaObject(Expression.Property(args[i].Expression, args[i].GetLimitType().GetProperty("Value")), args[i].Restrictions));
				}
				else if (args[i].GetLimitType() == typeof(bool))
				{
					args[i] = new DynamicMetaObject(Expression.Condition(args[i].Expression, Utils.Constant(1), Utils.Constant(0)), args[i].Restrictions);
				}
				else
				{
					args[i] = MakeIntTest(args[0], new DynamicMetaObject(Expression.Dynamic(binder.Convert(typeof(int), ConversionResultKind.ExplicitCast), typeof(int), Expression.Dynamic(binder.InvokeNone, typeof(object), Utils.Constant(binder.SharedContext), Binders.Get(Utils.Constant(binder.SharedContext), binder, typeof(object), "__index__", Expression.Convert(args[i].Expression, typeof(object))))), args[i].Restrictions));
				}
			}
			if (_lengthVar != null)
			{
				DynamicMetaObject dynamicMetaObject = base.Callable.CompleteRuleTarget(metaBinder, args, null);
				return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { _lengthVar }, Expression.Assign(_lengthVar, Utils.Constant(null, _lengthVar.Type)), dynamicMetaObject.Expression), dynamicMetaObject.Restrictions);
			}
			return base.Callable.CompleteRuleTarget(metaBinder, args, null);
		}

		private DynamicMetaObject MakeBigIntTest(DynamicMetaObject self, DynamicMetaObject bigInt)
		{
			EnsureLengthVariable();
			return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("NormalizeBigInteger"), self.Expression, bigInt.Expression, _lengthVar), self.Restrictions.Merge(bigInt.Restrictions));
		}

		private DynamicMetaObject MakeIntTest(DynamicMetaObject self, DynamicMetaObject intVal)
		{
			return new DynamicMetaObject(Expression.Condition(Expression.LessThan(intVal.Expression, Utils.Constant(0)), Expression.Add(intVal.Expression, MakeGetLength(self)), intVal.Expression), self.Restrictions.Merge(intVal.Restrictions));
		}

		private Expression MakeGetLength(DynamicMetaObject self)
		{
			EnsureLengthVariable();
			return Expression.Call(typeof(PythonOps).GetMethod("GetLengthOnce"), self.Expression, _lengthVar);
		}

		private void EnsureLengthVariable()
		{
			if (_lengthVar == null)
			{
				_lengthVar = Expression.Variable(typeof(int?), "objLength");
			}
		}
	}

	private class ItemBuilder : IndexBuilder
	{
		public ItemBuilder(DynamicMetaObject[] types, Callable callable)
			: base(types, callable)
		{
		}

		public override DynamicMetaObject MakeRule(DynamicMetaObjectBinder metaBinder, PythonContext binder, DynamicMetaObject[] args)
		{
			DynamicMetaObject[] tupleArgs = base.Callable.GetTupleArguments(args);
			return base.Callable.CompleteRuleTarget(metaBinder, tupleArgs, delegate
			{
				if (args[1].GetLimitType() != typeof(Slice) && GetTypeAt(1).TryResolveSlot(binder.SharedContext, "__index__", out var _))
				{
					args[1] = new DynamicMetaObject(Expression.Dynamic(binder.Convert(typeof(int), ConversionResultKind.ExplicitCast), typeof(int), Expression.Dynamic(binder.InvokeNone, typeof(object), Utils.Constant(binder.SharedContext), Binders.Get(Utils.Constant(binder.SharedContext), binder, typeof(object), "__index__", args[1].Expression))), BindingRestrictions.Empty);
					return base.Callable.CompleteRuleTarget(metaBinder, tupleArgs, null);
				}
				return (DynamicMetaObject)null;
			});
		}
	}

	private const string DisallowCoerce = "DisallowCoerce";

	internal static DynamicMetaObject ConvertToBool(DynamicMetaObjectBinder conversion, DynamicMetaObject self)
	{
		SlotOrFunction slotOrFunction = SlotOrFunction.GetSlotOrFunction(PythonContext.GetPythonContext(conversion), "__nonzero__", self);
		if (slotOrFunction.Success)
		{
			if (slotOrFunction.Target.Expression.Type != typeof(bool))
			{
				return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("ThrowingConvertToNonZero"), slotOrFunction.Target.Expression), slotOrFunction.Target.Restrictions);
			}
			return slotOrFunction.Target;
		}
		slotOrFunction = SlotOrFunction.GetSlotOrFunction(PythonContext.GetPythonContext(conversion), "__len__", self);
		if (slotOrFunction.Success)
		{
			return new DynamicMetaObject(GetConvertByLengthBody(PythonContext.GetPythonContext(conversion), slotOrFunction.Target.Expression), slotOrFunction.Target.Restrictions);
		}
		return null;
	}

	private static Expression GetConvertByLengthBody(PythonContext state, Expression call)
	{
		Expression left = call;
		if (call.Type != typeof(int))
		{
			left = Expression.Dynamic(state.Convert(typeof(int), ConversionResultKind.ExplicitCast), typeof(int), call);
		}
		return Expression.NotEqual(left, Utils.Constant(0));
	}

	internal static DynamicMetaObject Call(DynamicMetaObjectBinder call, DynamicMetaObject target, DynamicMetaObject[] args)
	{
		if (target.NeedsDeferral())
		{
			return call.Defer(ArrayUtils.Insert(target, args));
		}
		foreach (DynamicMetaObject self in args)
		{
			if (self.NeedsDeferral())
			{
				RestrictTypes(args);
				return call.Defer(ArrayUtils.Insert(target, args));
			}
		}
		DynamicMetaObject dynamicMetaObject = target.Restrict(target.GetLimitType());
		ValidationInfo validationInfo = BindingHelpers.GetValidationInfo(target);
		PythonType pythonType = DynamicHelpers.GetPythonType(target.Value);
		PythonContext pythonContext = PythonContext.GetPythonContext(call);
		if (!typeof(Delegate).IsAssignableFrom(target.GetLimitType()) && pythonType.TryResolveSlot(pythonContext.SharedContext, "__call__", out var slot))
		{
			ConditionalBuilder conditionalBuilder = new ConditionalBuilder(call);
			slot.MakeGetExpression(pythonContext.Binder, PythonContext.GetCodeContext(call), dynamicMetaObject, GetPythonType(dynamicMetaObject), conditionalBuilder);
			if (!conditionalBuilder.IsFinal)
			{
				conditionalBuilder.FinishCondition(GetCallError(call, dynamicMetaObject));
			}
			Expression[] arguments = ArrayUtils.Insert(PythonContext.GetCodeContext(call), conditionalBuilder.GetMetaObject().Expression, DynamicUtils.GetExpressions(args));
			Expression arg = Expression.Dynamic(PythonContext.GetPythonContext(call).Invoke(BindingHelpers.GetCallSignature(call)), typeof(object), arguments);
			arg = Expression.TryFinally(Expression.Block(Expression.Call(typeof(PythonOps).GetMethod("FunctionPushFrame"), Expression.Constant(pythonContext)), arg), Expression.Call(typeof(PythonOps).GetMethod("FunctionPopFrame")));
			return BindingHelpers.AddDynamicTestAndDefer(call, new DynamicMetaObject(arg, dynamicMetaObject.Restrictions.Merge(BindingRestrictions.Combine(args))), args, validationInfo);
		}
		return null;
	}

	private static DynamicMetaObject GetPythonType(DynamicMetaObject self)
	{
		PythonType pythonType = DynamicHelpers.GetPythonType(self.Value);
		if (pythonType.IsSystemType)
		{
			return new DynamicMetaObject(Utils.Constant(pythonType), BindingRestrictions.Empty, pythonType);
		}
		return new DynamicMetaObject(Expression.Property(Expression.Convert(self.Expression, typeof(IPythonObject)), IronPython.Runtime.Types.TypeInfo._IPythonObject.PythonType), BindingRestrictions.Empty, pythonType);
	}

	private static Expression GetCallError(DynamicMetaObjectBinder binder, DynamicMetaObject self)
	{
		return binder.Throw(Expression.Call(typeof(PythonOps).GetMethod("UncallableError"), Utils.Convert(self.Expression, typeof(object))));
	}

	public static DynamicMetaObject Operation(BinaryOperationBinder operation, DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
	{
		DynamicMetaObject[] args = new DynamicMetaObject[2] { target, arg };
		if (BindingHelpers.NeedsDeferral(args))
		{
			return operation.Defer(target, arg);
		}
		ValidationInfo validationInfo = BindingHelpers.GetValidationInfo(args);
		PythonOperationKind? pythonOperationKind = null;
		switch (operation.Operation)
		{
		case ExpressionType.Add:
			pythonOperationKind = PythonOperationKind.Add;
			break;
		case ExpressionType.And:
			pythonOperationKind = PythonOperationKind.BitwiseAnd;
			break;
		case ExpressionType.Divide:
			pythonOperationKind = PythonOperationKind.Divide;
			break;
		case ExpressionType.ExclusiveOr:
			pythonOperationKind = PythonOperationKind.ExclusiveOr;
			break;
		case ExpressionType.Modulo:
			pythonOperationKind = PythonOperationKind.Mod;
			break;
		case ExpressionType.Multiply:
			pythonOperationKind = PythonOperationKind.Multiply;
			break;
		case ExpressionType.Or:
			pythonOperationKind = PythonOperationKind.BitwiseOr;
			break;
		case ExpressionType.Power:
			pythonOperationKind = PythonOperationKind.Power;
			break;
		case ExpressionType.RightShift:
			pythonOperationKind = PythonOperationKind.RightShift;
			break;
		case ExpressionType.LeftShift:
			pythonOperationKind = PythonOperationKind.LeftShift;
			break;
		case ExpressionType.Subtract:
			pythonOperationKind = PythonOperationKind.Subtract;
			break;
		case ExpressionType.AddAssign:
			pythonOperationKind = PythonOperationKind.InPlaceAdd;
			break;
		case ExpressionType.AndAssign:
			pythonOperationKind = PythonOperationKind.InPlaceBitwiseAnd;
			break;
		case ExpressionType.DivideAssign:
			pythonOperationKind = PythonOperationKind.InPlaceDivide;
			break;
		case ExpressionType.ExclusiveOrAssign:
			pythonOperationKind = PythonOperationKind.InPlaceExclusiveOr;
			break;
		case ExpressionType.ModuloAssign:
			pythonOperationKind = PythonOperationKind.InPlaceMod;
			break;
		case ExpressionType.MultiplyAssign:
			pythonOperationKind = PythonOperationKind.InPlaceMultiply;
			break;
		case ExpressionType.OrAssign:
			pythonOperationKind = PythonOperationKind.InPlaceBitwiseOr;
			break;
		case ExpressionType.PowerAssign:
			pythonOperationKind = PythonOperationKind.InPlacePower;
			break;
		case ExpressionType.RightShiftAssign:
			pythonOperationKind = PythonOperationKind.InPlaceRightShift;
			break;
		case ExpressionType.LeftShiftAssign:
			pythonOperationKind = PythonOperationKind.InPlaceLeftShift;
			break;
		case ExpressionType.SubtractAssign:
			pythonOperationKind = PythonOperationKind.InPlaceSubtract;
			break;
		case ExpressionType.Equal:
			pythonOperationKind = PythonOperationKind.Equal;
			break;
		case ExpressionType.GreaterThan:
			pythonOperationKind = PythonOperationKind.GreaterThan;
			break;
		case ExpressionType.GreaterThanOrEqual:
			pythonOperationKind = PythonOperationKind.GreaterThanOrEqual;
			break;
		case ExpressionType.LessThan:
			pythonOperationKind = PythonOperationKind.LessThan;
			break;
		case ExpressionType.LessThanOrEqual:
			pythonOperationKind = PythonOperationKind.LessThanOrEqual;
			break;
		case ExpressionType.NotEqual:
			pythonOperationKind = PythonOperationKind.NotEqual;
			break;
		}
		DynamicMetaObject dynamicMetaObject = null;
		dynamicMetaObject = ((!pythonOperationKind.HasValue) ? operation.FallbackBinaryOperation(target, arg) : MakeBinaryOperation(operation, args, pythonOperationKind.Value, errorSuggestion));
		return BindingHelpers.AddDynamicTestAndDefer(operation, BindingHelpers.AddPythonBoxing(dynamicMetaObject), args, validationInfo);
	}

	public static DynamicMetaObject Operation(UnaryOperationBinder operation, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
	{
		DynamicMetaObject[] array = new DynamicMetaObject[1] { arg };
		if (arg.NeedsDeferral())
		{
			return operation.Defer(arg);
		}
		ValidationInfo validationInfo = BindingHelpers.GetValidationInfo(array);
		DynamicMetaObject dynamicMetaObject = null;
		Type typeFromHandle = typeof(object);
		switch (operation.Operation)
		{
		case ExpressionType.UnaryPlus:
			dynamicMetaObject = BindingHelpers.AddPythonBoxing(MakeUnaryOperation(operation, arg, "__pos__", errorSuggestion));
			break;
		case ExpressionType.Negate:
			dynamicMetaObject = BindingHelpers.AddPythonBoxing(MakeUnaryOperation(operation, arg, "__neg__", errorSuggestion));
			break;
		case ExpressionType.OnesComplement:
			dynamicMetaObject = BindingHelpers.AddPythonBoxing(MakeUnaryOperation(operation, arg, "__invert__", errorSuggestion));
			break;
		case ExpressionType.Not:
			dynamicMetaObject = MakeUnaryNotOperation(operation, arg, typeof(object), errorSuggestion);
			break;
		case ExpressionType.IsFalse:
			dynamicMetaObject = MakeUnaryNotOperation(operation, arg, typeof(bool), errorSuggestion);
			typeFromHandle = typeof(bool);
			break;
		case ExpressionType.IsTrue:
			dynamicMetaObject = ConvertToBool(operation, arg);
			typeFromHandle = typeof(bool);
			break;
		default:
			dynamicMetaObject = TypeError(operation, "unknown operation: " + operation.ToString(), array);
			break;
		}
		return BindingHelpers.AddDynamicTestAndDefer(operation, dynamicMetaObject, array, validationInfo, typeFromHandle);
	}

	public static DynamicMetaObject Index(DynamicMetaObjectBinder operation, PythonIndexType index, DynamicMetaObject[] args)
	{
		return Index(operation, index, args, null);
	}

	public static DynamicMetaObject Index(DynamicMetaObjectBinder operation, PythonIndexType index, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
	{
		_ = args.Length;
		_ = 3;
		if (BindingHelpers.NeedsDeferral(args))
		{
			return operation.Defer(args);
		}
		ValidationInfo validationInfo = BindingHelpers.GetValidationInfo(args[0]);
		DynamicMetaObject res = BindingHelpers.AddPythonBoxing(MakeIndexerOperation(operation, index, args, errorSuggestion));
		return BindingHelpers.AddDynamicTestAndDefer(operation, res, args, validationInfo);
	}

	public static DynamicMetaObject Operation(PythonOperationBinder operation, params DynamicMetaObject[] args)
	{
		_ = args.Length;
		_ = 1;
		if (BindingHelpers.NeedsDeferral(args))
		{
			return operation.Defer(args);
		}
		return MakeOperationRule(operation, args);
	}

	private static DynamicMetaObject MakeOperationRule(PythonOperationBinder operation, DynamicMetaObject[] args)
	{
		ValidationInfo validationInfo = BindingHelpers.GetValidationInfo(args);
		Type typeFromHandle = typeof(object);
		return BindingHelpers.AddDynamicTestAndDefer(operation, NormalizeOperator(operation.Operation) switch
		{
			PythonOperationKind.Documentation => BindingHelpers.AddPythonBoxing(MakeDocumentationOperation(operation, args)), 
			PythonOperationKind.CallSignatures => BindingHelpers.AddPythonBoxing(MakeCallSignatureOperation(args[0], CompilerHelpers.GetMethodTargets(args[0].Value))), 
			PythonOperationKind.IsCallable => MakeIscallableOperation(operation, args), 
			PythonOperationKind.Hash => MakeHashOperation(operation, args[0]), 
			PythonOperationKind.Contains => MakeContainsOperation(operation, args), 
			PythonOperationKind.AbsoluteValue => BindingHelpers.AddPythonBoxing(MakeUnaryOperation(operation, args[0], "__abs__", null)), 
			PythonOperationKind.Compare => MakeSortComparisonRule(args, operation, operation.Operation), 
			PythonOperationKind.GetEnumeratorForIteration => MakeEnumeratorOperation(operation, args[0]), 
			_ => BindingHelpers.AddPythonBoxing(MakeBinaryOperation(operation, args, operation.Operation, null)), 
		}, args, validationInfo, typeFromHandle);
	}

	private static DynamicMetaObject MakeBinaryOperation(DynamicMetaObjectBinder operation, DynamicMetaObject[] args, PythonOperationKind opStr, DynamicMetaObject errorSuggestion)
	{
		if (IsComparison(opStr))
		{
			return MakeComparisonOperation(args, operation, opStr, errorSuggestion);
		}
		return MakeSimpleOperation(args, operation, opStr, errorSuggestion);
	}

	private static DynamicMetaObject MakeContainsOperation(PythonOperationBinder operation, DynamicMetaObject[] types)
	{
		ArrayUtils.SwapLastTwo(types);
		PythonContext pythonContext = PythonContext.GetPythonContext(operation);
		SlotOrFunction slotOrFunction = SlotOrFunction.GetSlotOrFunction(pythonContext, "__contains__", types);
		DynamicMetaObject dynamicMetaObject;
		if (slotOrFunction.Success)
		{
			dynamicMetaObject = slotOrFunction.Target;
		}
		else
		{
			RestrictTypes(types);
			slotOrFunction = SlotOrFunction.GetSlotOrFunction(pythonContext, "__iter__", types[0]);
			if (slotOrFunction.Success)
			{
				dynamicMetaObject = new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("ContainsFromEnumerable"), Utils.Constant(pythonContext.SharedContext), slotOrFunction.Target.Expression, Utils.Convert(types[1].Expression, typeof(object))), BindingRestrictions.Combine(types));
			}
			else
			{
				ParameterExpression parameterExpression = Expression.Variable(typeof(int), "count");
				slotOrFunction = SlotOrFunction.GetSlotOrFunction(pythonContext, "__getitem__", types[0], new DynamicMetaObject(parameterExpression, BindingRestrictions.Empty));
				if (slotOrFunction.Success)
				{
					ParameterExpression parameterExpression2 = Expression.Variable(slotOrFunction.ReturnType, "getItemRes");
					ParameterExpression parameterExpression3 = Expression.Variable(typeof(bool), "containsRes");
					LabelTarget labelTarget = Expression.Label();
					dynamicMetaObject = new DynamicMetaObject(Expression.Block(new ParameterExpression[3] { parameterExpression, parameterExpression2, parameterExpression3 }, Utils.Loop(null, Expression.Assign(parameterExpression, Expression.Add(parameterExpression, Utils.Constant(1))), Expression.Block(Utils.Try(Expression.Block(Expression.Assign(parameterExpression2, slotOrFunction.Target.Expression), Expression.Empty())).Catch(typeof(IndexOutOfRangeException), Expression.Break(labelTarget)), Utils.If(Expression.Dynamic(pythonContext.BinaryOperationRetType(pythonContext.BinaryOperation(ExpressionType.Equal), pythonContext.Convert(typeof(bool), ConversionResultKind.ExplicitCast)), typeof(bool), types[1].Expression, parameterExpression2), Expression.Assign(parameterExpression3, Utils.Constant(true)), Expression.Break(labelTarget)), Utils.Empty()), null, labelTarget, null), parameterExpression3), BindingRestrictions.Combine(types));
				}
				else
				{
					dynamicMetaObject = new DynamicMetaObject(operation.Throw(Expression.Call(typeof(PythonOps).GetMethod("TypeErrorForNonIterableObject"), Utils.Convert(types[1].Expression, typeof(object))), typeof(bool)), BindingRestrictions.Combine(types));
				}
			}
		}
		if (dynamicMetaObject.GetLimitType() != typeof(bool) && dynamicMetaObject.GetLimitType() != typeof(void))
		{
			dynamicMetaObject = new DynamicMetaObject(Expression.Dynamic(pythonContext.Convert(typeof(bool), ConversionResultKind.ExplicitCast), typeof(bool), dynamicMetaObject.Expression), dynamicMetaObject.Restrictions);
		}
		return dynamicMetaObject;
	}

	private static void RestrictTypes(DynamicMetaObject[] types)
	{
		for (int i = 0; i < types.Length; i++)
		{
			types[i] = types[i].Restrict(types[i].GetLimitType());
		}
	}

	private static DynamicMetaObject MakeHashOperation(PythonOperationBinder operation, DynamicMetaObject self)
	{
		self = self.Restrict(self.GetLimitType());
		PythonContext pythonContext = PythonContext.GetPythonContext(operation);
		SlotOrFunction slotOrFunction = SlotOrFunction.GetSlotOrFunction(pythonContext, "__hash__", self);
		DynamicMetaObject dynamicMetaObject = slotOrFunction.Target;
		if (slotOrFunction.IsNull)
		{
			dynamicMetaObject = new DynamicMetaObject(operation.Throw(Expression.Call(typeof(PythonOps).GetMethod("TypeErrorForUnhashableObject"), self.Expression), typeof(int)), dynamicMetaObject.Restrictions);
		}
		else if (slotOrFunction.ReturnType != typeof(int))
		{
			if (slotOrFunction.ReturnType == typeof(BigInteger))
			{
				dynamicMetaObject = new DynamicMetaObject(HashBigInt(operation, dynamicMetaObject.Expression), dynamicMetaObject.Restrictions);
			}
			else if (slotOrFunction.ReturnType == typeof(object))
			{
				ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "hashTemp");
				dynamicMetaObject = new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Assign(parameterExpression, dynamicMetaObject.Expression), Expression.Condition(Expression.TypeIs(parameterExpression, typeof(int)), Expression.Convert(parameterExpression, typeof(int)), Expression.Condition(Expression.TypeIs(parameterExpression, typeof(BigInteger)), HashBigInt(operation, parameterExpression), HashConvertToInt(pythonContext, parameterExpression)))), dynamicMetaObject.Restrictions);
			}
			else
			{
				dynamicMetaObject = new DynamicMetaObject(HashConvertToInt(pythonContext, dynamicMetaObject.Expression), dynamicMetaObject.Restrictions);
			}
		}
		return dynamicMetaObject;
	}

	private static DynamicExpression HashBigInt(PythonOperationBinder operation, Expression expression)
	{
		return Expression.Dynamic(operation, typeof(int), expression);
	}

	private static DynamicExpression HashConvertToInt(PythonContext state, Expression expression)
	{
		return Expression.Dynamic(state.Convert(typeof(int), ConversionResultKind.ExplicitCast), typeof(int), expression);
	}

	private static DynamicMetaObject MakeUnaryOperation(DynamicMetaObjectBinder binder, DynamicMetaObject self, string symbol, DynamicMetaObject errorSuggestion)
	{
		self = self.Restrict(self.GetLimitType());
		SlotOrFunction slotOrFunction = SlotOrFunction.GetSlotOrFunction(PythonContext.GetPythonContext(binder), symbol, self);
		if (!slotOrFunction.Success)
		{
			return errorSuggestion ?? TypeError(binder, MakeUnaryOpErrorMessage(symbol, "{0}"), self);
		}
		return slotOrFunction.Target;
	}

	private static DynamicMetaObject MakeEnumeratorOperation(PythonOperationBinder operation, DynamicMetaObject self)
	{
		if (self.GetLimitType() == typeof(string))
		{
			self = self.Restrict(self.GetLimitType());
			return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("StringEnumerator"), self.Expression), self.Restrictions);
		}
		if (self.GetLimitType() == typeof(Bytes))
		{
			self = self.Restrict(self.GetLimitType());
			if (operation.Context.PythonOptions.Python30)
			{
				return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("BytesIntEnumerator"), self.Expression), self.Restrictions);
			}
			return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("BytesEnumerator"), self.Expression), self.Restrictions);
		}
		if ((self.Value is IEnumerable || typeof(IEnumerable).IsAssignableFrom(self.GetLimitType())) && !(self.Value is PythonGenerator))
		{
			self = self.Restrict(self.GetLimitType());
			return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("GetEnumeratorFromEnumerable"), Expression.Convert(self.Expression, typeof(IEnumerable))), self.Restrictions);
		}
		if (self.Value is IEnumerator || typeof(IEnumerator).IsAssignableFrom(self.GetLimitType()))
		{
			DynamicMetaObject dynamicMetaObject = new DynamicMetaObject(MakeEnumeratorResult(Expression.Convert(self.Expression, typeof(IEnumerator))), self.Restrict(self.GetLimitType()).Restrictions);
			if (ComBinder.IsComObject(self.Value))
			{
				dynamicMetaObject = new DynamicMetaObject(MakeEnumeratorResult(Expression.Convert(self.Expression, typeof(IEnumerator))), dynamicMetaObject.Restrictions.Merge(BindingRestrictions.GetExpressionRestriction(Expression.TypeIs(self.Expression, typeof(IEnumerator)))));
			}
			return dynamicMetaObject;
		}
		ParameterExpression parameterExpression = Expression.Parameter(typeof(IEnumerator), "enum");
		IPythonConvertible pythonConvertible = self as IPythonConvertible;
		PythonConversionBinder pythonConversionBinder = PythonContext.GetPythonContext(operation).Convert(typeof(IEnumerator), ConversionResultKind.ExplicitTry);
		DynamicMetaObject dynamicMetaObject2 = ((pythonConvertible == null) ? pythonConversionBinder.Bind(self, new DynamicMetaObject[0]) : pythonConvertible.BindConvert(pythonConversionBinder));
		return new DynamicMetaObject(Expression.Block(new ParameterExpression[1] { parameterExpression }, Expression.Condition(Expression.NotEqual(Expression.Assign(parameterExpression, dynamicMetaObject2.Expression), Utils.Constant(null)), MakeEnumeratorResult(parameterExpression), Expression.Call(typeof(PythonOps).GetMethod("ThrowTypeErrorForBadIteration"), PythonContext.GetCodeContext(operation), self.Expression))), dynamicMetaObject2.Restrictions);
	}

	private static NewExpression MakeEnumeratorResult(Expression tmp)
	{
		return Expression.New(typeof(KeyValuePair<IEnumerator, IDisposable>).GetConstructor(new Type[2]
		{
			typeof(IEnumerator),
			typeof(IDisposable)
		}), tmp, Expression.Constant(null, typeof(IDisposable)));
	}

	private static DynamicMetaObject MakeUnaryNotOperation(DynamicMetaObjectBinder operation, DynamicMetaObject self, Type retType, DynamicMetaObject errorSuggestion)
	{
		self = self.Restrict(self.GetLimitType());
		SlotOrFunction slotOrFunction = SlotOrFunction.GetSlotOrFunction(PythonContext.GetPythonContext(operation), "__nonzero__", self);
		SlotOrFunction slotOrFunction2 = SlotOrFunction.GetSlotOrFunction(PythonContext.GetPythonContext(operation), "__len__", self);
		Expression expression;
		if (!slotOrFunction.Success && !slotOrFunction2.Success)
		{
			expression = ((errorSuggestion != null) ? errorSuggestion.Expression : ((self.GetLimitType() == typeof(DynamicNull)) ? Utils.Constant(true) : Utils.Constant(false)));
		}
		else
		{
			SlotOrFunction slotOrFunction3 = (slotOrFunction.Success ? slotOrFunction : slotOrFunction2);
			expression = slotOrFunction3.Target.Expression;
			expression = (slotOrFunction.Success ? ((!(expression.Type == typeof(bool))) ? ((Expression)Expression.Call(typeof(PythonOps).GetMethod("Not"), Utils.Convert(expression, typeof(object)))) : ((Expression)Expression.Equal(expression, Utils.Constant(false)))) : ((!(expression.Type == typeof(int))) ? Expression.Equal(Expression.Dynamic(PythonContext.GetPythonContext(operation).Operation(PythonOperationKind.Compare), typeof(int), expression, Utils.Constant(0)), Utils.Constant(0)) : Expression.Equal(expression, Utils.Constant(0))));
		}
		if (retType == typeof(object) && expression.Type == typeof(bool))
		{
			expression = BindingHelpers.AddPythonBoxing(expression);
		}
		return new DynamicMetaObject(expression, self.Restrictions.Merge(slotOrFunction.Target.Restrictions.Merge(slotOrFunction2.Target.Restrictions)));
	}

	private static DynamicMetaObject MakeDocumentationOperation(PythonOperationBinder operation, DynamicMetaObject[] args)
	{
		PythonContext pythonContext = PythonContext.GetPythonContext(operation);
		return new DynamicMetaObject(Binders.Convert(PythonContext.GetCodeContext(operation), pythonContext, typeof(string), ConversionResultKind.ExplicitCast, Binders.Get(PythonContext.GetCodeContext(operation), pythonContext, typeof(object), "__doc__", args[0].Expression)), args[0].Restrictions);
	}

	internal static DynamicMetaObject MakeCallSignatureOperation(DynamicMetaObject self, IList<MethodBase> targets)
	{
		List<string> list = new List<string>();
		foreach (MethodBase target in targets)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string value = "";
			Type returnType = target.GetReturnType();
			if (returnType != typeof(void))
			{
				stringBuilder.Append(DynamicHelpers.GetPythonTypeFromType(returnType).Name);
				stringBuilder.Append(" ");
			}
			MethodInfo methodInfo = target as MethodInfo;
			if (methodInfo != null)
			{
				NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(target.DeclaringType), methodInfo, out var name);
				stringBuilder.Append(name);
			}
			else
			{
				stringBuilder.Append(DynamicHelpers.GetPythonTypeFromType(target.DeclaringType).Name);
			}
			stringBuilder.Append("(");
			if (!CompilerHelpers.IsStatic(target))
			{
				stringBuilder.Append("self");
				value = ", ";
			}
			ParameterInfo[] parameters = target.GetParameters();
			foreach (ParameterInfo parameterInfo in parameters)
			{
				if (!(parameterInfo.ParameterType == typeof(CodeContext)))
				{
					stringBuilder.Append(value);
					stringBuilder.Append(DynamicHelpers.GetPythonTypeFromType(parameterInfo.ParameterType).Name + " " + parameterInfo.Name);
					value = ", ";
				}
			}
			stringBuilder.Append(")");
			list.Add(stringBuilder.ToString());
		}
		return new DynamicMetaObject(Utils.Constant(list.ToArray()), self.Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(self.Expression, self.Value)));
	}

	private static DynamicMetaObject MakeIscallableOperation(PythonOperationBinder operation, DynamicMetaObject[] args)
	{
		DynamicMetaObject dynamicMetaObject = args[0];
		if (typeof(Delegate).IsAssignableFrom(dynamicMetaObject.GetLimitType()) || typeof(MethodGroup).IsAssignableFrom(dynamicMetaObject.GetLimitType()))
		{
			return new DynamicMetaObject(Utils.Constant(true), dynamicMetaObject.Restrict(dynamicMetaObject.GetLimitType()).Restrictions);
		}
		PythonContext pythonContext = PythonContext.GetPythonContext(operation);
		Expression expression = Expression.NotEqual(Binders.TryGet(PythonContext.GetCodeContext(operation), pythonContext, typeof(object), "__call__", dynamicMetaObject.Expression), Utils.Constant(OperationFailed.Value));
		return new DynamicMetaObject(expression, dynamicMetaObject.Restrict(dynamicMetaObject.GetLimitType()).Restrictions);
	}

	private static DynamicMetaObject MakeSimpleOperation(DynamicMetaObject[] types, DynamicMetaObjectBinder binder, PythonOperationKind operation, DynamicMetaObject errorSuggestion)
	{
		RestrictTypes(types);
		GetOperatorMethods(types, operation, PythonContext.GetPythonContext(binder), out var fbinder, out var rbinder, out var fSlot, out var rSlot);
		return MakeBinaryOperatorResult(types, binder, operation, fbinder, rbinder, fSlot, rSlot, errorSuggestion);
	}

	private static void GetOperatorMethods(DynamicMetaObject[] types, PythonOperationKind oper, PythonContext state, out SlotOrFunction fbinder, out SlotOrFunction rbinder, out PythonTypeSlot fSlot, out PythonTypeSlot rSlot)
	{
		oper = NormalizeOperator(oper);
		oper &= (PythonOperationKind)(-536870913);
		string text;
		string text2;
		if (!IsReverseOperator(oper))
		{
			text = Symbols.OperatorToSymbol(oper);
			text2 = Symbols.OperatorToReversedSymbol(oper);
		}
		else
		{
			text2 = Symbols.OperatorToSymbol(oper);
			text = Symbols.OperatorToReversedSymbol(oper);
		}
		fSlot = null;
		rSlot = null;
		if (oper == PythonOperationKind.Multiply && IsSequence(types[0]) && !PythonOps.IsNonExtensibleNumericType(types[1].GetLimitType()))
		{
			types = new DynamicMetaObject[2]
			{
				types[0],
				new DynamicMetaObject(Expression.New(typeof(Index).GetConstructor(new Type[1] { typeof(object) }), Utils.Convert(types[1].Expression, typeof(object))), BindingRestrictions.Empty)
			};
		}
		if (!SlotOrFunction.TryGetBinder(state, types, text, null, out fbinder, out var declaringType))
		{
			foreach (PythonType item in MetaPythonObject.GetPythonType(types[0]).ResolutionOrder)
			{
				if (item.TryLookupSlot(state.SharedContext, text, out fSlot))
				{
					declaringType = item;
					break;
				}
			}
		}
		if (!SlotOrFunction.TryGetBinder(state, types, null, text2, out rbinder, out var declaringType2))
		{
			foreach (PythonType item2 in MetaPythonObject.GetPythonType(types[1]).ResolutionOrder)
			{
				if (item2.TryLookupSlot(state.SharedContext, text2, out rSlot))
				{
					declaringType2 = item2;
					break;
				}
			}
		}
		if (declaringType != null && (rbinder.Success || rSlot != null) && declaringType2 != declaringType && declaringType2.IsSubclassOf(declaringType))
		{
			fbinder = SlotOrFunction.Empty;
			fSlot = null;
		}
		if (!fbinder.Success && !rbinder.Success && fSlot == null && rSlot == null && (text == "__truediv__" || text == "__rtruediv__"))
		{
			PythonOperationKind oper2 = ((text == "__truediv__") ? PythonOperationKind.Divide : PythonOperationKind.ReverseDivide);
			GetOperatorMethods(types, oper2, state, out fbinder, out rbinder, out fSlot, out rSlot);
		}
	}

	private static bool IsReverseOperator(PythonOperationKind oper)
	{
		return (oper & PythonOperationKind.Reversed) != 0;
	}

	private static bool IsSequence(DynamicMetaObject metaObject)
	{
		if (typeof(List).IsAssignableFrom(metaObject.GetLimitType()) || typeof(PythonTuple).IsAssignableFrom(metaObject.GetLimitType()) || typeof(string).IsAssignableFrom(metaObject.GetLimitType()))
		{
			return true;
		}
		return false;
	}

	private static DynamicMetaObject MakeBinaryOperatorResult(DynamicMetaObject[] types, DynamicMetaObjectBinder operation, PythonOperationKind op, SlotOrFunction fCand, SlotOrFunction rCand, PythonTypeSlot fSlot, PythonTypeSlot rSlot, DynamicMetaObject errorSuggestion)
	{
		PythonContext pythonContext = PythonContext.GetPythonContext(operation);
		ConditionalBuilder conditionalBuilder = new ConditionalBuilder(operation);
		if ((op & PythonOperationKind.InPlace) != PythonOperationKind.None)
		{
			SlotOrFunction slotOrFunction = SlotOrFunction.GetSlotOrFunction(PythonContext.GetPythonContext(operation), Symbols.OperatorToSymbol(op), types);
			if (!MakeOneCompareGeneric(slotOrFunction, reverse: false, types, MakeCompareReturn, conditionalBuilder, typeof(object)))
			{
				return conditionalBuilder.GetMetaObject(types);
			}
		}
		if (!SlotOrFunction.GetCombinedTargets(fCand, rCand, out var fTarget, out var rTarget) && fSlot == null && rSlot == null && !ShouldCoerce(pythonContext, op, types[0], types[1], isCompare: false) && !ShouldCoerce(pythonContext, op, types[1], types[0], isCompare: false) && conditionalBuilder.NoConditions)
		{
			return MakeRuleForNoMatch(operation, op, errorSuggestion, types);
		}
		if (ShouldCoerce(pythonContext, op, types[0], types[1], isCompare: false) && (op != PythonOperationKind.Mod || !MetaPythonObject.GetPythonType(types[0]).IsSubclassOf(TypeCache.String)))
		{
			DoCoerce(pythonContext, conditionalBuilder, op, types, reverse: false);
		}
		if (MakeOneTarget(PythonContext.GetPythonContext(operation), fTarget, fSlot, conditionalBuilder, reverse: false, types))
		{
			if (ShouldCoerce(pythonContext, op, types[1], types[0], isCompare: false))
			{
				DoCoerce(pythonContext, conditionalBuilder, op, new DynamicMetaObject[2]
				{
					types[1],
					types[0]
				}, reverse: true);
			}
			if (rSlot != null)
			{
				MakeSlotCall(PythonContext.GetPythonContext(operation), types, conditionalBuilder, rSlot, reverse: true);
				conditionalBuilder.FinishCondition(MakeBinaryThrow(operation, op, types).Expression, typeof(object));
			}
			else if (MakeOneTarget(PythonContext.GetPythonContext(operation), rTarget, rSlot, conditionalBuilder, reverse: false, types))
			{
				conditionalBuilder.FinishCondition(MakeBinaryThrow(operation, op, types).Expression, typeof(object));
			}
		}
		return conditionalBuilder.GetMetaObject(types);
	}

	private static void MakeCompareReturn(ConditionalBuilder bodyBuilder, Expression retCondition, Expression retValue, bool isReverse, Type retType)
	{
		if (retCondition != null)
		{
			bodyBuilder.AddCondition(retCondition, retValue);
		}
		else
		{
			bodyBuilder.FinishCondition(retValue, retType);
		}
	}

	private static bool MakeOneCompareGeneric(SlotOrFunction target, bool reverse, DynamicMetaObject[] types, ComparisonHelper returner, ConditionalBuilder bodyBuilder, Type retType)
	{
		if (target == SlotOrFunction.Empty || !target.Success)
		{
			return true;
		}
		ParameterExpression parameterExpression;
		if (target.ReturnType == typeof(bool))
		{
			parameterExpression = bodyBuilder.CompareRetBool;
		}
		else
		{
			parameterExpression = Expression.Variable(target.ReturnType, "compareRetValue");
			bodyBuilder.AddVariable(parameterExpression);
		}
		if (target.MaybeNotImplemented)
		{
			Expression expression = target.Target.Expression;
			Expression left = Expression.Assign(parameterExpression, expression);
			returner(bodyBuilder, Expression.NotEqual(left, Utils.Constant(PythonOps.NotImplemented)), parameterExpression, reverse, retType);
			return true;
		}
		returner(bodyBuilder, null, target.Target.Expression, reverse, retType);
		return false;
	}

	private static bool MakeOneTarget(PythonContext state, SlotOrFunction target, PythonTypeSlot slotTarget, ConditionalBuilder bodyBuilder, bool reverse, DynamicMetaObject[] types)
	{
		if (target == SlotOrFunction.Empty && slotTarget == null)
		{
			return true;
		}
		if (slotTarget != null)
		{
			MakeSlotCall(state, types, bodyBuilder, slotTarget, reverse);
			return true;
		}
		if (target.MaybeNotImplemented)
		{
			ParameterExpression parameterExpression = Expression.Variable(typeof(object), "slot");
			bodyBuilder.AddVariable(parameterExpression);
			bodyBuilder.AddCondition(Expression.NotEqual(Expression.Assign(parameterExpression, target.Target.Expression), Expression.Property(null, typeof(PythonOps).GetProperty("NotImplemented"))), parameterExpression);
			return true;
		}
		bodyBuilder.FinishCondition(target.Target.Expression, typeof(object));
		return false;
	}

	private static void MakeSlotCall(PythonContext state, DynamicMetaObject[] types, ConditionalBuilder bodyBuilder, PythonTypeSlot slotTarget, bool reverse)
	{
		Expression expression;
		Expression expression2;
		if (reverse)
		{
			expression = types[1].Expression;
			expression2 = types[0].Expression;
		}
		else
		{
			expression = types[0].Expression;
			expression2 = types[1].Expression;
		}
		MakeSlotCallWorker(state, slotTarget, expression, bodyBuilder, expression2);
	}

	private static void MakeSlotCallWorker(PythonContext state, PythonTypeSlot slotTarget, Expression self, ConditionalBuilder bodyBuilder, params Expression[] args)
	{
		ParameterExpression parameterExpression = Expression.Variable(typeof(object), "slot");
		ParameterExpression parameterExpression2 = Expression.Variable(typeof(object), "slot");
		bodyBuilder.AddCondition(Expression.AndAlso(Expression.Call(typeof(PythonOps).GetMethod("SlotTryGetValue"), Utils.Constant(state.SharedContext), Utils.Convert(Utils.WeakConstant(slotTarget), typeof(PythonTypeSlot)), Utils.Convert(self, typeof(object)), Expression.Call(typeof(DynamicHelpers).GetMethod("GetPythonType"), Utils.Convert(self, typeof(object))), parameterExpression), Expression.NotEqual(Expression.Assign(parameterExpression2, Expression.Dynamic(state.Invoke(new CallSignature(args.Length)), typeof(object), ArrayUtils.Insert(Utils.Constant(state.SharedContext), parameterExpression, args))), Expression.Property(null, typeof(PythonOps).GetProperty("NotImplemented")))), parameterExpression2);
		bodyBuilder.AddVariable(parameterExpression);
		bodyBuilder.AddVariable(parameterExpression2);
	}

	private static void DoCoerce(PythonContext state, ConditionalBuilder bodyBuilder, PythonOperationKind op, DynamicMetaObject[] types, bool reverse)
	{
		DoCoerce(state, bodyBuilder, op, types, reverse, (Expression e) => e);
	}

	private static void DoCoerce(PythonContext pyContext, ConditionalBuilder bodyBuilder, PythonOperationKind op, DynamicMetaObject[] types, bool reverse, Func<Expression, Expression> returnTransform)
	{
		ParameterExpression parameterExpression = Expression.Variable(typeof(object), "coerceResult");
		ParameterExpression parameterExpression2 = Expression.Variable(typeof(PythonTuple), "coerceTuple");
		SlotOrFunction slotOrFunction = SlotOrFunction.GetSlotOrFunction(pyContext, "__coerce__", types);
		if (slotOrFunction.Success)
		{
			bodyBuilder.AddCondition(Expression.AndAlso(Expression.Not(Expression.TypeIs(Expression.Assign(parameterExpression, slotOrFunction.Target.Expression), typeof(OldInstance))), Expression.NotEqual(Expression.Assign(parameterExpression2, Expression.Call(typeof(PythonOps).GetMethod("ValidateCoerceResult"), parameterExpression)), Utils.Constant(null))), BindingHelpers.AddRecursionCheck(pyContext, returnTransform(Expression.Dynamic(pyContext.Operation(op | PythonOperationKind.DisableCoerce), (op == PythonOperationKind.Compare) ? typeof(int) : typeof(object), reverse ? CoerceTwo(parameterExpression2) : CoerceOne(parameterExpression2), reverse ? CoerceOne(parameterExpression2) : CoerceTwo(parameterExpression2)))));
			bodyBuilder.AddVariable(parameterExpression);
			bodyBuilder.AddVariable(parameterExpression2);
		}
	}

	private static MethodCallExpression CoerceTwo(ParameterExpression coerceTuple)
	{
		return Expression.Call(typeof(PythonOps).GetMethod("GetCoerceResultTwo"), coerceTuple);
	}

	private static MethodCallExpression CoerceOne(ParameterExpression coerceTuple)
	{
		return Expression.Call(typeof(PythonOps).GetMethod("GetCoerceResultOne"), coerceTuple);
	}

	private static DynamicMetaObject MakeComparisonOperation(DynamicMetaObject[] types, DynamicMetaObjectBinder operation, PythonOperationKind opString, DynamicMetaObject errorSuggestion)
	{
		RestrictTypes(types);
		PythonOperationKind op = NormalizeOperator(opString);
		PythonContext pythonContext = PythonContext.GetPythonContext(operation);
		DynamicMetaObject dynamicMetaObject = types[0];
		DynamicMetaObject dynamicMetaObject2 = types[1];
		string op2 = Symbols.OperatorToSymbol(op);
		string op3 = Symbols.OperatorToReversedSymbol(op);
		DynamicMetaObject[] types2 = new DynamicMetaObject[2]
		{
			types[1],
			types[0]
		};
		SlotOrFunction fTarget = SlotOrFunction.GetSlotOrFunction(pythonContext, op2, types);
		SlotOrFunction rTarget = SlotOrFunction.GetSlotOrFunction(pythonContext, op3, types2);
		SlotOrFunction fTarget2 = SlotOrFunction.GetSlotOrFunction(pythonContext, "__cmp__", types);
		SlotOrFunction rTarget2 = SlotOrFunction.GetSlotOrFunction(pythonContext, "__cmp__", types2);
		ConditionalBuilder conditionalBuilder = new ConditionalBuilder(operation);
		SlotOrFunction.GetCombinedTargets(fTarget, rTarget, out fTarget, out rTarget);
		SlotOrFunction.GetCombinedTargets(fTarget2, rTarget2, out fTarget2, out rTarget2);
		bool flag = false;
		WarningInfo info = null;
		flag = fTarget.ShouldWarn(pythonContext, out info);
		if (MakeOneCompareGeneric(fTarget, reverse: false, types, MakeCompareReturn, conditionalBuilder, typeof(object)))
		{
			flag = flag || rTarget.ShouldWarn(pythonContext, out info);
			if (MakeOneCompareGeneric(rTarget, reverse: true, types, MakeCompareReturn, conditionalBuilder, typeof(object)))
			{
				flag = flag || fTarget2.ShouldWarn(pythonContext, out info);
				if (ShouldCoerce(pythonContext, opString, dynamicMetaObject, dynamicMetaObject2, isCompare: true))
				{
					DoCoerce(pythonContext, conditionalBuilder, PythonOperationKind.Compare, types, reverse: false, (Expression e) => GetCompareTest(op, e, reverse: false));
				}
				if (MakeOneCompareGeneric(fTarget2, reverse: false, types, delegate(ConditionalBuilder builder, Expression retCond, Expression expr, bool reverse, Type retType)
				{
					MakeCompareTest(op, builder, retCond, expr, reverse, retType);
				}, conditionalBuilder, typeof(object)))
				{
					flag = flag || rTarget2.ShouldWarn(pythonContext, out info);
					if (ShouldCoerce(pythonContext, opString, dynamicMetaObject2, dynamicMetaObject, isCompare: true))
					{
						DoCoerce(pythonContext, conditionalBuilder, PythonOperationKind.Compare, types2, reverse: true, (Expression e) => GetCompareTest(op, e, reverse: true));
					}
					if (MakeOneCompareGeneric(rTarget2, reverse: true, types, delegate(ConditionalBuilder builder, Expression retCond, Expression expr, bool reverse, Type retType)
					{
						MakeCompareTest(op, builder, retCond, expr, reverse, retType);
					}, conditionalBuilder, typeof(object)))
					{
						if (errorSuggestion != null)
						{
							conditionalBuilder.FinishCondition(errorSuggestion.Expression, typeof(object));
						}
						else
						{
							conditionalBuilder.FinishCondition(BindingHelpers.AddPythonBoxing(MakeFallbackCompare(operation, op, types)), typeof(object));
						}
					}
				}
			}
		}
		DynamicMetaObject metaObject = conditionalBuilder.GetMetaObject(types);
		if (!flag || metaObject == null)
		{
			return metaObject;
		}
		return info.AddWarning(Expression.Constant(pythonContext.SharedContext), metaObject);
	}

	private static DynamicMetaObject MakeSortComparisonRule(DynamicMetaObject[] types, DynamicMetaObjectBinder operation, PythonOperationKind op)
	{
		RestrictTypes(types);
		DynamicMetaObject dynamicMetaObject = FastPathCompare(types);
		if (dynamicMetaObject != null)
		{
			return dynamicMetaObject;
		}
		DynamicMetaObject[] types2 = new DynamicMetaObject[2]
		{
			types[1],
			types[0]
		};
		PythonContext pythonContext = PythonContext.GetPythonContext(operation);
		SlotOrFunction slotOrFunction = SlotOrFunction.GetSlotOrFunction(pythonContext, "__cmp__", types);
		SlotOrFunction slotOrFunction2 = SlotOrFunction.GetSlotOrFunction(pythonContext, "__cmp__", types2);
		SlotOrFunction slotOrFunction3 = SlotOrFunction.GetSlotOrFunction(pythonContext, "__eq__", types);
		SlotOrFunction slotOrFunction4 = SlotOrFunction.GetSlotOrFunction(pythonContext, "__eq__", types2);
		SlotOrFunction slotOrFunction5 = SlotOrFunction.GetSlotOrFunction(pythonContext, "__lt__", types);
		SlotOrFunction slotOrFunction6 = SlotOrFunction.GetSlotOrFunction(pythonContext, "__gt__", types);
		SlotOrFunction slotOrFunction7 = SlotOrFunction.GetSlotOrFunction(pythonContext, "__lt__", types2);
		SlotOrFunction slotOrFunction8 = SlotOrFunction.GetSlotOrFunction(pythonContext, "__gt__", types2);
		SlotOrFunction.GetCombinedTargets(slotOrFunction, slotOrFunction2, out var fTarget, out var rTarget);
		SlotOrFunction.GetCombinedTargets(slotOrFunction3, slotOrFunction4, out var fTarget2, out var rTarget2);
		SlotOrFunction.GetCombinedTargets(slotOrFunction5, slotOrFunction8, out var fTarget3, out var rTarget3);
		SlotOrFunction.GetCombinedTargets(slotOrFunction6, slotOrFunction7, out var fTarget4, out var rTarget4);
		PythonType pythonType = MetaPythonObject.GetPythonType(types[0]);
		PythonType pythonType2 = MetaPythonObject.GetPythonType(types[1]);
		if (pythonType.IsNull)
		{
			if (pythonType2.IsNull)
			{
				return new DynamicMetaObject(Utils.Constant(0), BindingRestrictions.Combine(types));
			}
			if (pythonType2.UnderlyingSystemType.IsPrimitive() || pythonType2.UnderlyingSystemType == typeof(BigInteger))
			{
				return new DynamicMetaObject(Utils.Constant(-1), BindingRestrictions.Combine(types));
			}
		}
		ConditionalBuilder conditionalBuilder = new ConditionalBuilder(operation);
		bool flag = true;
		bool flag2 = true;
		if (pythonType == pythonType2 && fTarget != SlotOrFunction.Empty)
		{
			if (ShouldCoerce(pythonContext, op, types[0], types[1], isCompare: true))
			{
				DoCoerce(pythonContext, conditionalBuilder, PythonOperationKind.Compare, types, reverse: false);
			}
			flag2 = flag2 && MakeOneCompareGeneric(fTarget, reverse: false, types, MakeCompareReverse, conditionalBuilder, typeof(int));
			if (pythonType != TypeCache.OldInstance)
			{
				flag2 = flag2 && MakeOneCompareGeneric(rTarget, reverse: true, types, MakeCompareReverse, conditionalBuilder, typeof(int));
				flag = false;
			}
		}
		if (flag && flag2)
		{
			MakeOneCompareGeneric(fTarget2, reverse: false, types, MakeCompareToZero, conditionalBuilder, typeof(int));
			MakeOneCompareGeneric(rTarget2, reverse: true, types, MakeCompareToZero, conditionalBuilder, typeof(int));
			MakeOneCompareGeneric(fTarget3, reverse: false, types, MakeCompareToNegativeOne, conditionalBuilder, typeof(int));
			MakeOneCompareGeneric(rTarget3, reverse: true, types, MakeCompareToNegativeOne, conditionalBuilder, typeof(int));
			MakeOneCompareGeneric(fTarget4, reverse: false, types, MakeCompareToOne, conditionalBuilder, typeof(int));
			MakeOneCompareGeneric(rTarget4, reverse: true, types, MakeCompareToOne, conditionalBuilder, typeof(int));
		}
		if (pythonType != pythonType2)
		{
			if (flag2 && ShouldCoerce(pythonContext, op, types[0], types[1], isCompare: true))
			{
				DoCoerce(pythonContext, conditionalBuilder, PythonOperationKind.Compare, types, reverse: false);
			}
			flag2 = flag2 && MakeOneCompareGeneric(fTarget, reverse: false, types, MakeCompareReverse, conditionalBuilder, typeof(int));
			if (flag2 && ShouldCoerce(pythonContext, op, types[1], types[0], isCompare: true))
			{
				DoCoerce(pythonContext, conditionalBuilder, PythonOperationKind.Compare, types2, reverse: true, (Expression e) => ReverseCompareValue(e));
			}
			flag2 = flag2 && MakeOneCompareGeneric(rTarget, reverse: true, types, MakeCompareReverse, conditionalBuilder, typeof(int));
		}
		if (flag2)
		{
			conditionalBuilder.FinishCondition(MakeFallbackCompare(operation, op, types), typeof(int));
		}
		return conditionalBuilder.GetMetaObject(types);
	}

	private static DynamicMetaObject FastPathCompare(DynamicMetaObject[] types)
	{
		if (types[0].GetLimitType() == types[1].GetLimitType())
		{
			if (types[0].GetLimitType() == typeof(List))
			{
				types[0] = types[0].Restrict(typeof(List));
				types[1] = types[1].Restrict(typeof(List));
				return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("CompareLists"), types[0].Expression, types[1].Expression), BindingRestrictions.Combine(types));
			}
			if (types[0].GetLimitType() == typeof(PythonTuple))
			{
				types[0] = types[0].Restrict(typeof(PythonTuple));
				types[1] = types[1].Restrict(typeof(PythonTuple));
				return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("CompareTuples"), types[0].Expression, types[1].Expression), BindingRestrictions.Combine(types));
			}
			if (types[0].GetLimitType() == typeof(double))
			{
				types[0] = types[0].Restrict(typeof(double));
				types[1] = types[1].Restrict(typeof(double));
				return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("CompareFloats"), types[0].Expression, types[1].Expression), BindingRestrictions.Combine(types));
			}
		}
		return null;
	}

	private static void MakeCompareToZero(ConditionalBuilder bodyBuilder, Expression retCondition, Expression expr, bool reverse, Type retType)
	{
		MakeValueCheck(0, expr, bodyBuilder, retCondition);
	}

	private static void MakeCompareToOne(ConditionalBuilder bodyBuilder, Expression retCondition, Expression expr, bool reverse, Type retType)
	{
		MakeValueCheck(1, expr, bodyBuilder, retCondition);
	}

	private static void MakeCompareToNegativeOne(ConditionalBuilder bodyBuilder, Expression retCondition, Expression expr, bool reverse, Type retType)
	{
		MakeValueCheck(-1, expr, bodyBuilder, retCondition);
	}

	private static void MakeValueCheck(int val, Expression retValue, ConditionalBuilder bodyBuilder, Expression retCondition)
	{
		if (retValue.Type != typeof(bool))
		{
			retValue = Expression.Dynamic(PythonContext.GetPythonContext(bodyBuilder.Action).Convert(typeof(bool), ConversionResultKind.ExplicitCast), typeof(bool), retValue);
		}
		if (retCondition != null)
		{
			retValue = Expression.AndAlso(retCondition, retValue);
		}
		bodyBuilder.AddCondition(retValue, Utils.Constant(val));
	}

	private static BinaryExpression ReverseCompareValue(Expression retVal)
	{
		return Expression.Multiply(Utils.Convert(retVal, typeof(int)), Utils.Constant(-1));
	}

	private static void MakeCompareReverse(ConditionalBuilder bodyBuilder, Expression retCondition, Expression expr, bool reverse, Type retType)
	{
		Expression retValue = expr;
		if (reverse)
		{
			retValue = ReverseCompareValue(expr);
		}
		MakeCompareReturn(bodyBuilder, retCondition, retValue, reverse, retType);
	}

	private static void MakeCompareTest(PythonOperationKind op, ConditionalBuilder bodyBuilder, Expression retCond, Expression expr, bool reverse, Type retType)
	{
		MakeCompareReturn(bodyBuilder, retCond, GetCompareTest(op, expr, reverse), reverse, retType);
	}

	private static Expression MakeFallbackCompare(DynamicMetaObjectBinder binder, PythonOperationKind op, DynamicMetaObject[] types)
	{
		return Expression.Call(GetComparisonFallbackMethod(op), PythonContext.GetCodeContext(binder), Utils.Convert(types[0].Expression, typeof(object)), Utils.Convert(types[1].Expression, typeof(object)));
	}

	private static Expression GetCompareTest(PythonOperationKind op, Expression expr, bool reverse)
	{
		if (expr.Type == typeof(int))
		{
			return GetCompareNode(op, reverse, expr);
		}
		return GetCompareExpression(op, reverse, Expression.Call(typeof(PythonOps).GetMethod("CompareToZero"), Utils.Convert(expr, typeof(object))));
	}

	private static DynamicMetaObject MakeIndexerOperation(DynamicMetaObjectBinder operation, PythonIndexType op, DynamicMetaObject[] types, DynamicMetaObject errorSuggestion)
	{
		DynamicMetaObject dynamicMetaObject = types[0].Restrict(types[0].GetLimitType());
		PythonContext pythonContext = PythonContext.GetPythonContext(operation);
		BuiltinFunction function = null;
		PythonTypeSlot slot = null;
		bool flag = false;
		GetIndexOperators(op, out var item, out var slice, out var mandatoryArgs);
		if (types.Length == mandatoryArgs + 1 && IsSlice(op) && HasOnlyNumericTypes(operation, types, op == PythonIndexType.SetSlice))
		{
			flag = BindingHelpers.TryGetStaticFunction(pythonContext, slice, dynamicMetaObject, out function);
			if (function == null || !flag)
			{
				flag = MetaPythonObject.GetPythonType(dynamicMetaObject).TryResolveSlot(pythonContext.SharedContext, slice, out slot);
			}
		}
		if (!flag && !BindingHelpers.TryGetStaticFunction(pythonContext, item, dynamicMetaObject, out function))
		{
			MetaPythonObject.GetPythonType(dynamicMetaObject).TryResolveSlot(pythonContext.SharedContext, item, out slot);
		}
		Callable callable = Callable.MakeCallable(pythonContext, op, function, slot);
		if (callable == null)
		{
			return errorSuggestion ?? MakeUnindexableError(operation, op, types, dynamicMetaObject, pythonContext);
		}
		IndexBuilder indexBuilder;
		DynamicMetaObject[] array;
		if (flag)
		{
			indexBuilder = new SliceBuilder(types, callable);
			array = ConvertArgs(types);
		}
		else
		{
			indexBuilder = new ItemBuilder(types, callable);
			if (IsSlice(op))
			{
				array = GetItemSliceArguments(pythonContext, op, types);
			}
			else
			{
				array = (DynamicMetaObject[])types.Clone();
				array[0] = types[0].Restrict(types[0].GetLimitType());
			}
		}
		return indexBuilder.MakeRule(operation, pythonContext, array);
	}

	private static DynamicMetaObject MakeUnindexableError(DynamicMetaObjectBinder operation, PythonIndexType op, DynamicMetaObject[] types, DynamicMetaObject indexedType, PythonContext state)
	{
		DynamicMetaObject[] array = (DynamicMetaObject[])types.Clone();
		array[0] = indexedType;
		if (op != PythonIndexType.GetItem && op != PythonIndexType.GetSlice && DynamicHelpers.GetPythonType(indexedType.Value).TryResolveSlot(state.SharedContext, "__getitem__", out var _))
		{
			if (op == PythonIndexType.SetItem || op == PythonIndexType.SetSlice)
			{
				return TypeError(operation, "'{0}' object does not support item assignment", array);
			}
			return TypeError(operation, "'{0}' object doesn't support item deletion", array);
		}
		return TypeError(operation, "'{0}' object is unsubscriptable", array);
	}

	private static DynamicMetaObject[] ConvertArgs(DynamicMetaObject[] types)
	{
		DynamicMetaObject[] array = new DynamicMetaObject[types.Length];
		for (int i = 0; i < types.Length; i++)
		{
			array[i] = types[i].Restrict(types[i].GetLimitType());
		}
		return array;
	}

	private static DynamicMetaObject[] GetItemSliceArguments(PythonContext state, PythonIndexType op, DynamicMetaObject[] types)
	{
		if (op == PythonIndexType.SetSlice)
		{
			return new DynamicMetaObject[3]
			{
				types[0].Restrict(types[0].GetLimitType()),
				GetSetSlice(state, types),
				types[types.Length - 1].Restrict(types[types.Length - 1].GetLimitType())
			};
		}
		return new DynamicMetaObject[2]
		{
			types[0].Restrict(types[0].GetLimitType()),
			GetGetOrDeleteSlice(state, types)
		};
	}

	private static bool HasOnlyNumericTypes(DynamicMetaObjectBinder action, DynamicMetaObject[] types, bool skipLast)
	{
		bool result = true;
		PythonContext pythonContext = PythonContext.GetPythonContext(action);
		for (int i = 1; i < (skipLast ? (types.Length - 1) : types.Length); i++)
		{
			DynamicMetaObject obj = types[i];
			if (!IsIndexType(pythonContext, obj))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private static bool IsIndexType(PythonContext state, DynamicMetaObject obj)
	{
		bool result = true;
		if (obj.GetLimitType() != typeof(MissingParameter) && !PythonOps.IsNumericType(obj.GetLimitType()))
		{
			PythonType pythonType = MetaPythonObject.GetPythonType(obj);
			if (!pythonType.TryResolveSlot(state.SharedContext, "__index__", out var _))
			{
				result = false;
			}
		}
		return result;
	}

	private static bool IsSlice(PythonIndexType op)
	{
		return op >= PythonIndexType.GetSlice;
	}

	private static void GetIndexOperators(PythonIndexType op, out string item, out string slice, out int mandatoryArgs)
	{
		switch (op)
		{
		case PythonIndexType.GetItem:
		case PythonIndexType.GetSlice:
			item = "__getitem__";
			slice = "__getslice__";
			mandatoryArgs = 2;
			break;
		case PythonIndexType.SetItem:
		case PythonIndexType.SetSlice:
			item = "__setitem__";
			slice = "__setslice__";
			mandatoryArgs = 3;
			break;
		case PythonIndexType.DeleteItem:
		case PythonIndexType.DeleteSlice:
			item = "__delitem__";
			slice = "__delslice__";
			mandatoryArgs = 2;
			break;
		default:
			throw new InvalidOperationException();
		}
	}

	private static DynamicMetaObject GetSetSlice(PythonContext state, DynamicMetaObject[] args)
	{
		DynamicMetaObject[] array = (DynamicMetaObject[])args.Clone();
		for (int i = 1; i < array.Length; i++)
		{
			if (!IsIndexType(state, array[i]))
			{
				array[i] = array[i].Restrict(array[i].GetLimitType());
			}
		}
		return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("MakeSlice"), Utils.Convert(GetSetParameter(array, 1), typeof(object)), Utils.Convert(GetSetParameter(array, 2), typeof(object)), Utils.Convert(GetSetParameter(array, 3), typeof(object))), BindingRestrictions.Combine(array));
	}

	private static DynamicMetaObject GetGetOrDeleteSlice(PythonContext state, DynamicMetaObject[] args)
	{
		DynamicMetaObject[] array = (DynamicMetaObject[])args.Clone();
		for (int i = 1; i < array.Length; i++)
		{
			if (!IsIndexType(state, array[i]))
			{
				array[i] = array[i].Restrict(array[i].GetLimitType());
			}
		}
		return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("MakeSlice"), Utils.Convert(GetGetOrDeleteParameter(array, 1), typeof(object)), Utils.Convert(GetGetOrDeleteParameter(array, 2), typeof(object)), Utils.Convert(GetGetOrDeleteParameter(array, 3), typeof(object))), BindingRestrictions.Combine(array));
	}

	private static Expression GetGetOrDeleteParameter(DynamicMetaObject[] args, int index)
	{
		if (args.Length > index)
		{
			return CheckMissing(args[index].Expression);
		}
		return Utils.Constant(null);
	}

	private static Expression GetSetParameter(DynamicMetaObject[] args, int index)
	{
		if (args.Length > index + 1)
		{
			return CheckMissing(args[index].Expression);
		}
		return Utils.Constant(null);
	}

	private static bool ShouldCoerce(PythonContext state, PythonOperationKind operation, DynamicMetaObject x, DynamicMetaObject y, bool isCompare)
	{
		if ((operation & PythonOperationKind.DisableCoerce) != PythonOperationKind.None)
		{
			return false;
		}
		PythonType pythonType = MetaPythonObject.GetPythonType(x);
		PythonType pythonType2 = MetaPythonObject.GetPythonType(y);
		if (pythonType == TypeCache.OldInstance)
		{
			return true;
		}
		if (isCompare && !pythonType.IsSystemType && pythonType2.IsSystemType && (pythonType2 == TypeCache.Int32 || pythonType2 == TypeCache.BigInteger || pythonType2 == TypeCache.Double || pythonType2 == TypeCache.Complex) && pythonType.TryResolveSlot(state.SharedContext, "__coerce__", out var slot))
		{
			if (!(slot is BuiltinMethodDescriptor builtinMethodDescriptor))
			{
				return true;
			}
			if (builtinMethodDescriptor.__name__ != "__coerce__" && builtinMethodDescriptor.DeclaringType != typeof(int) && builtinMethodDescriptor.DeclaringType != typeof(BigInteger) && builtinMethodDescriptor.DeclaringType != typeof(double) && builtinMethodDescriptor.DeclaringType != typeof(Complex))
			{
				return true;
			}
			foreach (PythonType item in pythonType.ResolutionOrder)
			{
				if (item.UnderlyingSystemType == builtinMethodDescriptor.DeclaringType)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public static PythonOperationKind DirectOperation(PythonOperationKind op)
	{
		if ((op & PythonOperationKind.InPlace) == 0)
		{
			throw new InvalidOperationException();
		}
		return op & (PythonOperationKind)(-536870913);
	}

	private static PythonOperationKind NormalizeOperator(PythonOperationKind op)
	{
		if ((op & PythonOperationKind.DisableCoerce) != PythonOperationKind.None)
		{
			op &= (PythonOperationKind)(-1073741825);
		}
		return op;
	}

	private static bool IsComparisonOperator(PythonOperationKind op)
	{
		return (op & PythonOperationKind.Comparison) != 0;
	}

	private static bool IsComparison(PythonOperationKind op)
	{
		return IsComparisonOperator(NormalizeOperator(op));
	}

	private static Expression GetCompareNode(PythonOperationKind op, bool reverse, Expression expr)
	{
		op = NormalizeOperator(op);
		return (reverse ? OperatorToReverseOperator(op) : op) switch
		{
			PythonOperationKind.Equal => Expression.Equal(expr, Utils.Constant(0)), 
			PythonOperationKind.NotEqual => Expression.NotEqual(expr, Utils.Constant(0)), 
			PythonOperationKind.GreaterThan => Expression.GreaterThan(expr, Utils.Constant(0)), 
			PythonOperationKind.GreaterThanOrEqual => Expression.GreaterThanOrEqual(expr, Utils.Constant(0)), 
			PythonOperationKind.LessThan => Expression.LessThan(expr, Utils.Constant(0)), 
			PythonOperationKind.LessThanOrEqual => Expression.LessThanOrEqual(expr, Utils.Constant(0)), 
			_ => throw new InvalidOperationException(), 
		};
	}

	public static PythonOperationKind OperatorToReverseOperator(PythonOperationKind op)
	{
		return op switch
		{
			PythonOperationKind.LessThan => PythonOperationKind.GreaterThan, 
			PythonOperationKind.LessThanOrEqual => PythonOperationKind.GreaterThanOrEqual, 
			PythonOperationKind.GreaterThan => PythonOperationKind.LessThan, 
			PythonOperationKind.GreaterThanOrEqual => PythonOperationKind.LessThanOrEqual, 
			PythonOperationKind.Equal => PythonOperationKind.Equal, 
			PythonOperationKind.NotEqual => PythonOperationKind.NotEqual, 
			PythonOperationKind.DivMod => PythonOperationKind.ReverseDivMod, 
			PythonOperationKind.ReverseDivMod => PythonOperationKind.DivMod, 
			_ => op & (PythonOperationKind)(-268435457), 
		};
	}

	private static Expression GetCompareExpression(PythonOperationKind op, bool reverse, Expression value)
	{
		op = NormalizeOperator(op);
		Expression right = Utils.Constant(0);
		return BindingHelpers.AddPythonBoxing((reverse ? OperatorToReverseOperator(op) : op) switch
		{
			PythonOperationKind.Equal => Expression.Equal(value, right), 
			PythonOperationKind.NotEqual => Expression.NotEqual(value, right), 
			PythonOperationKind.GreaterThan => Expression.GreaterThan(value, right), 
			PythonOperationKind.GreaterThanOrEqual => Expression.GreaterThanOrEqual(value, right), 
			PythonOperationKind.LessThan => Expression.LessThan(value, right), 
			PythonOperationKind.LessThanOrEqual => Expression.LessThanOrEqual(value, right), 
			_ => throw new InvalidOperationException(), 
		});
	}

	private static MethodInfo GetComparisonFallbackMethod(PythonOperationKind op)
	{
		op = NormalizeOperator(op);
		string name = op switch
		{
			PythonOperationKind.Equal => "CompareTypesEqual", 
			PythonOperationKind.NotEqual => "CompareTypesNotEqual", 
			PythonOperationKind.GreaterThan => "CompareTypesGreaterThan", 
			PythonOperationKind.LessThan => "CompareTypesLessThan", 
			PythonOperationKind.GreaterThanOrEqual => "CompareTypesGreaterThanOrEqual", 
			PythonOperationKind.LessThanOrEqual => "CompareTypesLessThanOrEqual", 
			PythonOperationKind.Compare => "CompareTypes", 
			_ => throw new InvalidOperationException(), 
		};
		return typeof(PythonOps).GetMethod(name);
	}

	internal static Expression CheckMissing(Expression toCheck)
	{
		if (toCheck.Type == typeof(MissingParameter))
		{
			return Utils.Constant(null);
		}
		if (toCheck.Type != typeof(object))
		{
			return toCheck;
		}
		return Expression.Condition(Expression.TypeIs(toCheck, typeof(MissingParameter)), Utils.Constant(null), toCheck);
	}

	private static DynamicMetaObject MakeRuleForNoMatch(DynamicMetaObjectBinder operation, PythonOperationKind op, DynamicMetaObject errorSuggestion, params DynamicMetaObject[] types)
	{
		return errorSuggestion ?? TypeError(operation, MakeBinaryOpErrorMessage(op, "{0}", "{1}"), types);
	}

	internal static string MakeUnaryOpErrorMessage(string op, string xType)
	{
		return op switch
		{
			"__invert__" => $"bad operand type for unary ~: '{xType}'", 
			"__abs__" => $"bad operand type for abs(): '{xType}'", 
			"__pos__" => $"bad operand type for unary +: '{xType}'", 
			"__neg__" => $"bad operand type for unary -: '{xType}'", 
			_ => throw new InvalidOperationException(), 
		};
	}

	internal static string MakeBinaryOpErrorMessage(PythonOperationKind op, string xType, string yType)
	{
		return string.Format("unsupported operand type(s) for {2}: '{0}' and '{1}'", xType, yType, GetOperatorDisplay(op));
	}

	private static string GetOperatorDisplay(PythonOperationKind op)
	{
		op = NormalizeOperator(op);
		return op switch
		{
			PythonOperationKind.Add => "+", 
			PythonOperationKind.Subtract => "-", 
			PythonOperationKind.Power => "**", 
			PythonOperationKind.Multiply => "*", 
			PythonOperationKind.FloorDivide => "//", 
			PythonOperationKind.Divide => "/", 
			PythonOperationKind.TrueDivide => "//", 
			PythonOperationKind.Mod => "%", 
			PythonOperationKind.LeftShift => "<<", 
			PythonOperationKind.RightShift => ">>", 
			PythonOperationKind.BitwiseAnd => "&", 
			PythonOperationKind.BitwiseOr => "|", 
			PythonOperationKind.ExclusiveOr => "^", 
			PythonOperationKind.LessThan => "<", 
			PythonOperationKind.GreaterThan => ">", 
			PythonOperationKind.LessThanOrEqual => "<=", 
			PythonOperationKind.GreaterThanOrEqual => ">=", 
			PythonOperationKind.Equal => "==", 
			PythonOperationKind.NotEqual => "!=", 
			PythonOperationKind.LessThanGreaterThan => "<>", 
			PythonOperationKind.InPlaceAdd => "+=", 
			PythonOperationKind.InPlaceSubtract => "-=", 
			PythonOperationKind.InPlacePower => "**=", 
			PythonOperationKind.InPlaceMultiply => "*=", 
			PythonOperationKind.InPlaceFloorDivide => "/=", 
			PythonOperationKind.InPlaceDivide => "/=", 
			PythonOperationKind.InPlaceTrueDivide => "//=", 
			PythonOperationKind.InPlaceMod => "%=", 
			PythonOperationKind.InPlaceLeftShift => "<<=", 
			PythonOperationKind.InPlaceRightShift => ">>=", 
			PythonOperationKind.InPlaceBitwiseAnd => "&=", 
			PythonOperationKind.InPlaceBitwiseOr => "|=", 
			PythonOperationKind.InPlaceExclusiveOr => "^=", 
			PythonOperationKind.ReverseAdd => "+", 
			PythonOperationKind.ReverseSubtract => "-", 
			PythonOperationKind.ReversePower => "**", 
			PythonOperationKind.ReverseMultiply => "*", 
			PythonOperationKind.ReverseFloorDivide => "/", 
			PythonOperationKind.ReverseDivide => "/", 
			PythonOperationKind.ReverseTrueDivide => "//", 
			PythonOperationKind.ReverseMod => "%", 
			PythonOperationKind.ReverseLeftShift => "<<", 
			PythonOperationKind.ReverseRightShift => ">>", 
			PythonOperationKind.ReverseBitwiseAnd => "&", 
			PythonOperationKind.ReverseBitwiseOr => "|", 
			PythonOperationKind.ReverseExclusiveOr => "^", 
			_ => op.ToString(), 
		};
	}

	private static DynamicMetaObject MakeBinaryThrow(DynamicMetaObjectBinder action, PythonOperationKind op, DynamicMetaObject[] args)
	{
		if (action is IPythonSite)
		{
			return new DynamicMetaObject(action.Throw(Expression.Call(typeof(PythonOps).GetMethod("TypeErrorForBinaryOp"), Utils.Constant(Symbols.OperatorToSymbol(NormalizeOperator(op))), Utils.Convert(args[0].Expression, typeof(object)), Utils.Convert(args[1].Expression, typeof(object))), typeof(object)), BindingRestrictions.Combine(args));
		}
		return GenericFallback(action, args);
	}

	public static DynamicMetaObject TypeError(DynamicMetaObjectBinder action, string message, params DynamicMetaObject[] types)
	{
		if (action is IPythonSite)
		{
			message = string.Format(message, ArrayUtils.ConvertAll(types, (DynamicMetaObject x) => MetaPythonObject.GetPythonType(x).Name));
			Expression expression = action.Throw(Expression.Call(typeof(PythonOps).GetMethod("SimpleTypeError"), Expression.Constant(message)), typeof(object));
			return new DynamicMetaObject(expression, BindingRestrictions.Combine(types));
		}
		return GenericFallback(action, types);
	}

	private static DynamicMetaObject GenericFallback(DynamicMetaObjectBinder action, DynamicMetaObject[] types)
	{
		if (action is GetIndexBinder)
		{
			return ((GetIndexBinder)action).FallbackGetIndex(types[0], ArrayUtils.RemoveFirst(types));
		}
		if (action is SetIndexBinder)
		{
			return ((SetIndexBinder)action).FallbackSetIndex(types[0], ArrayUtils.RemoveLast(ArrayUtils.RemoveFirst(types)), types[types.Length - 1]);
		}
		if (action is DeleteIndexBinder)
		{
			return ((DeleteIndexBinder)action).FallbackDeleteIndex(types[0], ArrayUtils.RemoveFirst(types));
		}
		if (action is UnaryOperationBinder)
		{
			return ((UnaryOperationBinder)action).FallbackUnaryOperation(types[0]);
		}
		if (action is BinaryOperationBinder)
		{
			return ((BinaryOperationBinder)action).FallbackBinaryOperation(types[0], types[1]);
		}
		throw new NotImplementedException();
	}
}

using System;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Actions;

namespace IronPython.Runtime.Binding;

internal static class Binders
{
	public static Expression Convert(Expression codeContext, PythonContext binder, Type type, ConversionResultKind resultKind, Expression target)
	{
		return Expression.Dynamic(binder.Convert(type, resultKind), type, target);
	}

	public static Expression Get(Expression codeContext, PythonContext binder, Type resultType, string name, Expression target)
	{
		return Expression.Dynamic(binder.GetMember(name), resultType, target, codeContext);
	}

	public static Expression TryGet(Expression codeContext, PythonContext binder, Type resultType, string name, Expression target)
	{
		return Expression.Dynamic(binder.GetMember(name, isNoThrow: true), resultType, target, codeContext);
	}

	public static DynamicMetaObjectBinder UnaryOperationBinder(PythonContext state, PythonOperationKind operatorName)
	{
		ExpressionType? expressionTypeFromUnaryOperator = GetExpressionTypeFromUnaryOperator(operatorName);
		if (!expressionTypeFromUnaryOperator.HasValue)
		{
			return state.Operation(operatorName);
		}
		return state.UnaryOperation(expressionTypeFromUnaryOperator.Value);
	}

	private static ExpressionType? GetExpressionTypeFromUnaryOperator(PythonOperationKind operatorName)
	{
		return operatorName switch
		{
			PythonOperationKind.Positive => ExpressionType.UnaryPlus, 
			PythonOperationKind.Negate => ExpressionType.Negate, 
			PythonOperationKind.OnesComplement => ExpressionType.OnesComplement, 
			PythonOperationKind.Not => ExpressionType.Not, 
			PythonOperationKind.IsFalse => ExpressionType.IsFalse, 
			_ => null, 
		};
	}

	public static DynamicMetaObjectBinder BinaryOperationBinder(PythonContext state, PythonOperationKind operatorName)
	{
		ExpressionType? expressionTypeFromBinaryOperator = GetExpressionTypeFromBinaryOperator(operatorName);
		if (!expressionTypeFromBinaryOperator.HasValue)
		{
			return state.Operation(operatorName);
		}
		return state.BinaryOperation(expressionTypeFromBinaryOperator.Value);
	}

	private static ExpressionType? GetExpressionTypeFromBinaryOperator(PythonOperationKind operatorName)
	{
		return operatorName switch
		{
			PythonOperationKind.Add => ExpressionType.Add, 
			PythonOperationKind.BitwiseAnd => ExpressionType.And, 
			PythonOperationKind.Divide => ExpressionType.Divide, 
			PythonOperationKind.ExclusiveOr => ExpressionType.ExclusiveOr, 
			PythonOperationKind.Mod => ExpressionType.Modulo, 
			PythonOperationKind.Multiply => ExpressionType.Multiply, 
			PythonOperationKind.BitwiseOr => ExpressionType.Or, 
			PythonOperationKind.Power => ExpressionType.Power, 
			PythonOperationKind.RightShift => ExpressionType.RightShift, 
			PythonOperationKind.LeftShift => ExpressionType.LeftShift, 
			PythonOperationKind.Subtract => ExpressionType.Subtract, 
			PythonOperationKind.InPlaceAdd => ExpressionType.AddAssign, 
			PythonOperationKind.InPlaceBitwiseAnd => ExpressionType.AndAssign, 
			PythonOperationKind.InPlaceDivide => ExpressionType.DivideAssign, 
			PythonOperationKind.InPlaceExclusiveOr => ExpressionType.ExclusiveOrAssign, 
			PythonOperationKind.InPlaceMod => ExpressionType.ModuloAssign, 
			PythonOperationKind.InPlaceMultiply => ExpressionType.MultiplyAssign, 
			PythonOperationKind.InPlaceBitwiseOr => ExpressionType.OrAssign, 
			PythonOperationKind.InPlacePower => ExpressionType.PowerAssign, 
			PythonOperationKind.InPlaceRightShift => ExpressionType.RightShiftAssign, 
			PythonOperationKind.InPlaceLeftShift => ExpressionType.LeftShiftAssign, 
			PythonOperationKind.InPlaceSubtract => ExpressionType.SubtractAssign, 
			PythonOperationKind.Equal => ExpressionType.Equal, 
			PythonOperationKind.GreaterThan => ExpressionType.GreaterThan, 
			PythonOperationKind.GreaterThanOrEqual => ExpressionType.GreaterThanOrEqual, 
			PythonOperationKind.LessThan => ExpressionType.LessThan, 
			PythonOperationKind.LessThanOrEqual => ExpressionType.LessThanOrEqual, 
			PythonOperationKind.NotEqual => ExpressionType.NotEqual, 
			_ => null, 
		};
	}

	public static PythonInvokeBinder InvokeSplat(PythonContext state)
	{
		return state.Invoke(new CallSignature(new Argument(ArgumentType.List)));
	}

	public static PythonInvokeBinder InvokeKeywords(PythonContext state)
	{
		return state.Invoke(new CallSignature(new Argument(ArgumentType.List), new Argument(ArgumentType.Dictionary)));
	}
}

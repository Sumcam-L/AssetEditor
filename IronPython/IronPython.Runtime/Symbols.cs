using System;
using IronPython.Runtime.Binding;

namespace IronPython.Runtime;

public static class Symbols
{
	internal static string OperatorToSymbol(PythonOperationKind op)
	{
		return op switch
		{
			PythonOperationKind.Add => "__add__", 
			PythonOperationKind.ReverseAdd => "__radd__", 
			PythonOperationKind.InPlaceAdd => "__iadd__", 
			PythonOperationKind.Subtract => "__sub__", 
			PythonOperationKind.ReverseSubtract => "__rsub__", 
			PythonOperationKind.InPlaceSubtract => "__isub__", 
			PythonOperationKind.Power => "__pow__", 
			PythonOperationKind.ReversePower => "__rpow__", 
			PythonOperationKind.InPlacePower => "__ipow__", 
			PythonOperationKind.Multiply => "__mul__", 
			PythonOperationKind.ReverseMultiply => "__rmul__", 
			PythonOperationKind.InPlaceMultiply => "__imul__", 
			PythonOperationKind.FloorDivide => "__floordiv__", 
			PythonOperationKind.ReverseFloorDivide => "__rfloordiv__", 
			PythonOperationKind.InPlaceFloorDivide => "__ifloordiv__", 
			PythonOperationKind.Divide => "__div__", 
			PythonOperationKind.ReverseDivide => "__rdiv__", 
			PythonOperationKind.InPlaceDivide => "__idiv__", 
			PythonOperationKind.TrueDivide => "__truediv__", 
			PythonOperationKind.ReverseTrueDivide => "__rtruediv__", 
			PythonOperationKind.InPlaceTrueDivide => "__itruediv__", 
			PythonOperationKind.Mod => "__mod__", 
			PythonOperationKind.ReverseMod => "__rmod__", 
			PythonOperationKind.InPlaceMod => "__imod__", 
			PythonOperationKind.LeftShift => "__lshift__", 
			PythonOperationKind.ReverseLeftShift => "__rlshift__", 
			PythonOperationKind.InPlaceLeftShift => "__ilshift__", 
			PythonOperationKind.RightShift => "__rshift__", 
			PythonOperationKind.ReverseRightShift => "__rrshift__", 
			PythonOperationKind.InPlaceRightShift => "__irshift__", 
			PythonOperationKind.BitwiseAnd => "__and__", 
			PythonOperationKind.ReverseBitwiseAnd => "__rand__", 
			PythonOperationKind.InPlaceBitwiseAnd => "__iand__", 
			PythonOperationKind.BitwiseOr => "__or__", 
			PythonOperationKind.ReverseBitwiseOr => "__ror__", 
			PythonOperationKind.InPlaceBitwiseOr => "__ior__", 
			PythonOperationKind.ExclusiveOr => "__xor__", 
			PythonOperationKind.ReverseExclusiveOr => "__rxor__", 
			PythonOperationKind.InPlaceExclusiveOr => "__ixor__", 
			PythonOperationKind.LessThan => "__lt__", 
			PythonOperationKind.GreaterThan => "__gt__", 
			PythonOperationKind.LessThanOrEqual => "__le__", 
			PythonOperationKind.GreaterThanOrEqual => "__ge__", 
			PythonOperationKind.Equal => "__eq__", 
			PythonOperationKind.NotEqual => "__ne__", 
			PythonOperationKind.LessThanGreaterThan => "__lg__", 
			PythonOperationKind.OnesComplement => "__invert__", 
			PythonOperationKind.Negate => "__neg__", 
			PythonOperationKind.Positive => "__pos__", 
			PythonOperationKind.AbsoluteValue => "__abs__", 
			PythonOperationKind.DivMod => "__divmod__", 
			PythonOperationKind.ReverseDivMod => "__rdivmod__", 
			PythonOperationKind.Compare => "__cmp__", 
			_ => throw new InvalidOperationException(op.ToString()), 
		};
	}

	internal static string OperatorToReversedSymbol(PythonOperationKind op)
	{
		switch (op)
		{
		case PythonOperationKind.LessThan:
			return "__gt__";
		case PythonOperationKind.LessThanOrEqual:
			return "__ge__";
		case PythonOperationKind.GreaterThan:
			return "__lt__";
		case PythonOperationKind.GreaterThanOrEqual:
			return "__le__";
		case PythonOperationKind.Equal:
			return "__eq__";
		case PythonOperationKind.NotEqual:
			return "__ne__";
		default:
			if ((op & PythonOperationKind.Reversed) != PythonOperationKind.None)
			{
				return OperatorToSymbol(op & (PythonOperationKind)(-268435457));
			}
			return OperatorToSymbol(op | PythonOperationKind.Reversed);
		}
	}

	internal static PythonOperationKind OperatorToReverseOperator(PythonOperationKind op)
	{
		switch (op)
		{
		case PythonOperationKind.LessThan:
			return PythonOperationKind.GreaterThan;
		case PythonOperationKind.LessThanOrEqual:
			return PythonOperationKind.GreaterThanOrEqual;
		case PythonOperationKind.GreaterThan:
			return PythonOperationKind.LessThan;
		case PythonOperationKind.GreaterThanOrEqual:
			return PythonOperationKind.LessThanOrEqual;
		case PythonOperationKind.Equal:
			return PythonOperationKind.Equal;
		case PythonOperationKind.NotEqual:
			return PythonOperationKind.NotEqual;
		case PythonOperationKind.DivMod:
			return PythonOperationKind.ReverseDivMod;
		default:
			if ((op & PythonOperationKind.Reversed) != PythonOperationKind.None)
			{
				return op & (PythonOperationKind)(-268435457);
			}
			return op | PythonOperationKind.Reversed;
		}
	}
}

using System.Linq.Expressions;
using IronPython.Runtime.Binding;

namespace IronPython.Compiler.Ast;

public class AugmentedAssignStatement : Statement
{
	private readonly PythonOperator _op;

	private readonly Expression _left;

	private readonly Expression _right;

	public PythonOperator Operator => _op;

	public Expression Left => _left;

	public Expression Right => _right;

	public AugmentedAssignStatement(PythonOperator op, Expression left, Expression right)
	{
		_op = op;
		_left = left;
		_right = right;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return _left.TransformSet(base.Span, _right, PythonOperatorToAction(_op));
	}

	private static PythonOperationKind PythonOperatorToAction(PythonOperator op)
	{
		return op switch
		{
			PythonOperator.Add => PythonOperationKind.InPlaceAdd, 
			PythonOperator.Subtract => PythonOperationKind.InPlaceSubtract, 
			PythonOperator.Multiply => PythonOperationKind.InPlaceMultiply, 
			PythonOperator.Divide => PythonOperationKind.InPlaceDivide, 
			PythonOperator.TrueDivide => PythonOperationKind.InPlaceTrueDivide, 
			PythonOperator.Mod => PythonOperationKind.InPlaceMod, 
			PythonOperator.BitwiseAnd => PythonOperationKind.InPlaceBitwiseAnd, 
			PythonOperator.BitwiseOr => PythonOperationKind.InPlaceBitwiseOr, 
			PythonOperator.Xor => PythonOperationKind.InPlaceExclusiveOr, 
			PythonOperator.LeftShift => PythonOperationKind.InPlaceLeftShift, 
			PythonOperator.RightShift => PythonOperationKind.InPlaceRightShift, 
			PythonOperator.Power => PythonOperationKind.InPlacePower, 
			PythonOperator.FloorDivide => PythonOperationKind.InPlaceFloorDivide, 
			_ => PythonOperationKind.None, 
		};
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_left != null)
			{
				_left.Walk(walker);
			}
			if (_right != null)
			{
				_right.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}

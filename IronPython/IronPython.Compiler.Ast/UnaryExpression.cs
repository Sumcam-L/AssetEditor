using System.Linq.Expressions;
using IronPython.Runtime.Binding;

namespace IronPython.Compiler.Ast;

public class UnaryExpression : Expression
{
	private readonly Expression _expression;

	private readonly PythonOperator _op;

	public Expression Expression => _expression;

	public PythonOperator Op => _op;

	public UnaryExpression(PythonOperator op, Expression expression)
	{
		_op = op;
		_expression = expression;
		base.EndIndex = expression.EndIndex;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return base.GlobalParent.Operation(typeof(object), PythonOperatorToOperatorString(_op), _expression);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && _expression != null)
		{
			_expression.Walk(walker);
		}
		walker.PostWalk(this);
	}

	private static PythonOperationKind PythonOperatorToOperatorString(PythonOperator op)
	{
		return op switch
		{
			PythonOperator.Not => PythonOperationKind.Not, 
			PythonOperator.Pos => PythonOperationKind.Positive, 
			PythonOperator.Invert => PythonOperationKind.OnesComplement, 
			PythonOperator.Negate => PythonOperationKind.Negate, 
			_ => PythonOperationKind.None, 
		};
	}
}

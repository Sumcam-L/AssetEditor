using System.Linq.Expressions;

namespace IronPython.Compiler.Ast;

public class ExpressionStatement : Statement
{
	private readonly Expression _expression;

	public Expression Expression => _expression;

	public override string Documentation
	{
		get
		{
			if (_expression is ConstantExpression constantExpression)
			{
				return constantExpression.Value as string;
			}
			return null;
		}
	}

	internal override bool CanThrow => _expression.CanThrow;

	public ExpressionStatement(Expression expression)
	{
		_expression = expression;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		System.Linq.Expressions.Expression expression = _expression;
		return ReduceWorker(expression);
	}

	private System.Linq.Expressions.Expression ReduceWorker(System.Linq.Expressions.Expression expression)
	{
		if (base.Parent.PrintExpressions)
		{
			expression = System.Linq.Expressions.Expression.Call(AstMethods.PrintExpressionValue, base.Parent.LocalContext, Node.ConvertIfNeeded(expression, typeof(object)));
		}
		return base.GlobalParent.AddDebugInfoAndVoid(expression, _expression.Span);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && _expression != null)
		{
			_expression.Walk(walker);
		}
		walker.PostWalk(this);
	}
}

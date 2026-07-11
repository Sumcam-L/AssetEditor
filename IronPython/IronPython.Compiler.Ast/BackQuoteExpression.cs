using System.Linq.Expressions;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class BackQuoteExpression : Expression
{
	private readonly Expression _expression;

	public Expression Expression => _expression;

	public BackQuoteExpression(Expression expression)
	{
		_expression = expression;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return System.Linq.Expressions.Expression.Call(AstMethods.Repr, base.Parent.LocalContext, Utils.Convert(_expression, typeof(object)));
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

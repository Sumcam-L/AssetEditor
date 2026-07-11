using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class PrintStatement : Statement
{
	private readonly Expression _dest;

	private readonly Expression[] _expressions;

	private readonly bool _trailingComma;

	public Expression Destination => _dest;

	public IList<Expression> Expressions => _expressions;

	public bool TrailingComma => _trailingComma;

	public PrintStatement(Expression destination, Expression[] expressions, bool trailingComma)
	{
		_dest = destination;
		_expressions = expressions;
		_trailingComma = trailingComma;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		System.Linq.Expressions.Expression expression = _dest;
		if (_expressions.Length == 0)
		{
			System.Linq.Expressions.Expression expression2 = ((expression == null) ? System.Linq.Expressions.Expression.Call(AstMethods.PrintNewline, base.Parent.LocalContext) : System.Linq.Expressions.Expression.Call(AstMethods.PrintNewlineWithDest, base.Parent.LocalContext, expression));
			return base.GlobalParent.AddDebugInfo(expression2, base.Span);
		}
		ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>();
		ParameterExpression parameterExpression = null;
		if (expression != null)
		{
			parameterExpression = System.Linq.Expressions.Expression.Variable(typeof(object), "destination");
			readOnlyCollectionBuilder.Add(Node.MakeAssignment(parameterExpression, expression));
			expression = parameterExpression;
		}
		for (int i = 0; i < _expressions.Length; i++)
		{
			bool flag = i < _expressions.Length - 1 || _trailingComma;
			Expression expression3 = _expressions[i];
			MethodCallExpression item = ((expression == null) ? System.Linq.Expressions.Expression.Call(flag ? AstMethods.PrintComma : AstMethods.Print, base.Parent.LocalContext, Utils.Convert(expression3, typeof(object))) : System.Linq.Expressions.Expression.Call(flag ? AstMethods.PrintCommaWithDest : AstMethods.PrintWithDest, base.Parent.LocalContext, expression, Utils.Convert(expression3, typeof(object))));
			readOnlyCollectionBuilder.Add(item);
		}
		readOnlyCollectionBuilder.Add(Utils.Empty());
		System.Linq.Expressions.Expression expression4 = ((parameterExpression == null) ? System.Linq.Expressions.Expression.Block(readOnlyCollectionBuilder.ToReadOnlyCollection()) : System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, readOnlyCollectionBuilder.ToReadOnlyCollection()));
		return base.GlobalParent.AddDebugInfo(expression4, base.Span);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_dest != null)
			{
				_dest.Walk(walker);
			}
			if (_expressions != null)
			{
				Expression[] expressions = _expressions;
				foreach (Expression expression in expressions)
				{
					expression.Walk(walker);
				}
			}
		}
		walker.PostWalk(this);
	}
}

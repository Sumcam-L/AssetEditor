using System.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class ConditionalExpression : Expression
{
	private readonly Expression _testExpr;

	private readonly Expression _trueExpr;

	private readonly Expression _falseExpr;

	public Expression FalseExpression => _falseExpr;

	public Expression Test => _testExpr;

	public Expression TrueExpression => _trueExpr;

	public ConditionalExpression(Expression testExpression, Expression trueExpression, Expression falseExpression)
	{
		_testExpr = testExpression;
		_trueExpr = trueExpression;
		_falseExpr = falseExpression;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		System.Linq.Expressions.Expression ifTrue = Utils.Convert(_trueExpr, typeof(object));
		System.Linq.Expressions.Expression ifFalse = Utils.Convert(_falseExpr, typeof(object));
		return System.Linq.Expressions.Expression.Condition(base.GlobalParent.Convert(typeof(bool), ConversionResultKind.ExplicitCast, _testExpr), ifTrue, ifFalse);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_testExpr != null)
			{
				_testExpr.Walk(walker);
			}
			if (_trueExpr != null)
			{
				_trueExpr.Walk(walker);
			}
			if (_falseExpr != null)
			{
				_falseExpr.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}

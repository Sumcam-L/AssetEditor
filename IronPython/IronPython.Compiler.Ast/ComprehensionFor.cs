using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IronPython.Compiler.Ast;

public class ComprehensionFor : ComprehensionIterator
{
	private readonly Expression _lhs;

	private readonly Expression _list;

	public Expression Left => _lhs;

	public Expression List => _list;

	public ComprehensionFor(Expression lhs, Expression list)
	{
		_lhs = lhs;
		_list = list;
	}

	internal override System.Linq.Expressions.Expression Transform(System.Linq.Expressions.Expression body)
	{
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(KeyValuePair<IEnumerator, IDisposable>), "list_comprehension_for");
		return System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, ForStatement.TransformFor(base.Parent, parameterExpression, _list, _lhs, body, null, base.Span, base.GlobalParent.IndexToLocation(_lhs.EndIndex), null, null, isStatement: false));
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_lhs != null)
			{
				_lhs.Walk(walker);
			}
			if (_list != null)
			{
				_list.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}

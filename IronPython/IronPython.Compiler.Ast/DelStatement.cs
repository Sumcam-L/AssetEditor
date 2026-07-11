using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class DelStatement : Statement
{
	private readonly Expression[] _expressions;

	public IList<Expression> Expressions => _expressions;

	internal override bool CanThrow
	{
		get
		{
			Expression[] expressions = _expressions;
			foreach (Expression expression in expressions)
			{
				if (expression.CanThrow)
				{
					return true;
				}
			}
			return false;
		}
	}

	public DelStatement(Expression[] expressions)
	{
		_expressions = expressions;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>(_expressions.Length + 1);
		for (int i = 0; i < _expressions.Length; i++)
		{
			readOnlyCollectionBuilder.Add(_expressions[i].TransformDelete());
		}
		readOnlyCollectionBuilder.Add(Utils.Empty());
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Block(readOnlyCollectionBuilder), base.Span);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && _expressions != null)
		{
			Expression[] expressions = _expressions;
			foreach (Expression expression in expressions)
			{
				expression.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}

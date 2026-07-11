using System;
using System.Linq.Expressions;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;

namespace IronPython.Compiler.Ast;

public class ParenthesisExpression : Expression
{
	private readonly Expression _expression;

	public Expression Expression => _expression;

	public override Type Type => _expression.Type;

	internal override bool CanThrow => _expression.CanThrow;

	public ParenthesisExpression(Expression expression)
	{
		_expression = expression;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return _expression;
	}

	internal override System.Linq.Expressions.Expression TransformSet(SourceSpan span, System.Linq.Expressions.Expression right, PythonOperationKind op)
	{
		return _expression.TransformSet(span, right, op);
	}

	internal override string CheckAssign()
	{
		return _expression.CheckAssign();
	}

	internal override string CheckDelete()
	{
		return _expression.CheckDelete();
	}

	internal override System.Linq.Expressions.Expression TransformDelete()
	{
		return _expression.TransformDelete();
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

using System;
using System.Linq.Expressions;

namespace IronPython.Compiler;

internal sealed class DelayedTupleAssign : Expression
{
	private readonly Expression _lhs;

	private readonly Expression _rhs;

	public sealed override ExpressionType NodeType => ExpressionType.Extension;

	public sealed override Type Type => _lhs.Type;

	public override bool CanReduce => true;

	public DelayedTupleAssign(Expression lhs, Expression rhs)
	{
		_lhs = lhs;
		_rhs = rhs;
	}

	public override Expression Reduce()
	{
		return Expression.Assign(_lhs.Reduce(), _rhs);
	}

	protected override Expression VisitChildren(ExpressionVisitor visitor)
	{
		Expression expression = visitor.Visit(_rhs);
		if (expression != _rhs)
		{
			return new DelayedTupleAssign(_lhs, expression);
		}
		return this;
	}
}

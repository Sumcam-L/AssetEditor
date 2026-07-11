using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;

namespace IronPython.Compiler;

internal sealed class DelayedTupleExpression : Expression
{
	public readonly int Index;

	private readonly StrongBox<Type> _tupleType;

	private readonly StrongBox<ParameterExpression> _tupleExpr;

	private readonly Type _type;

	public sealed override ExpressionType NodeType => ExpressionType.Extension;

	public sealed override Type Type => _type;

	public override bool CanReduce => true;

	public DelayedTupleExpression(int index, StrongBox<ParameterExpression> tupleExpr, StrongBox<Type> tupleType, Type type)
	{
		Index = index;
		_tupleType = tupleType;
		_tupleExpr = tupleExpr;
		_type = type;
	}

	public override Expression Reduce()
	{
		Expression expression = _tupleExpr.Value;
		foreach (PropertyInfo item in MutableTuple.GetAccessPath(_tupleType.Value, Index))
		{
			expression = Expression.Property(expression, item);
		}
		return expression;
	}

	protected override Expression VisitChildren(ExpressionVisitor visitor)
	{
		return this;
	}
}

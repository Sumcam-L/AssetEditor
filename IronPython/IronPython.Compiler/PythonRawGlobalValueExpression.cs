using System;
using System.Linq.Expressions;

namespace IronPython.Compiler;

internal class PythonRawGlobalValueExpression : Expression
{
	private readonly PythonGlobalVariableExpression _global;

	public sealed override ExpressionType NodeType => ExpressionType.Extension;

	public sealed override Type Type => typeof(object);

	public override bool CanReduce => true;

	public PythonGlobalVariableExpression Global => _global;

	public PythonRawGlobalValueExpression(PythonGlobalVariableExpression global)
	{
		_global = global;
	}

	public override Expression Reduce()
	{
		return Expression.Property(_global.Target, PythonGlobal.RawValueProperty);
	}

	protected override Expression VisitChildren(ExpressionVisitor visitor)
	{
		return this;
	}
}

using System;
using System.Linq.Expressions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast;

public class ReturnStatement : Statement
{
	private readonly Expression _expression;

	public Expression Expression => _expression;

	internal override bool CanThrow
	{
		get
		{
			if (_expression == null)
			{
				return false;
			}
			return _expression.CanThrow;
		}
	}

	public ReturnStatement(Expression expression)
	{
		_expression = expression;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		if (base.Parent.IsGeneratorMethod)
		{
			if (_expression != null)
			{
				return System.Linq.Expressions.Expression.Throw(System.Linq.Expressions.Expression.New(typeof(InvalidOperationException).GetConstructor(ReflectionUtils.EmptyTypes)));
			}
			return base.GlobalParent.AddDebugInfo(Utils.YieldBreak(Node.GeneratorLabel), base.Span);
		}
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Return(FunctionDefinition._returnLabel, Node.TransformOrConstantNull(_expression, typeof(object))), base.Span);
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

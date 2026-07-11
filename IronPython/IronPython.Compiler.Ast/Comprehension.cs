using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace IronPython.Compiler.Ast;

public abstract class Comprehension : Expression
{
	public abstract IList<ComprehensionIterator> Iterators { get; }

	public abstract override string NodeName { get; }

	protected abstract ParameterExpression MakeParameter();

	protected abstract MethodInfo Factory();

	protected abstract System.Linq.Expressions.Expression Body(ParameterExpression res);

	public abstract override void Walk(PythonWalker walker);

	public override System.Linq.Expressions.Expression Reduce()
	{
		ParameterExpression parameterExpression = MakeParameter();
		System.Linq.Expressions.Expression expression = System.Linq.Expressions.Expression.Assign(parameterExpression, System.Linq.Expressions.Expression.Call(Factory()));
		System.Linq.Expressions.Expression expression2 = Body(parameterExpression);
		for (int num = Iterators.Count - 1; num >= 0; num--)
		{
			ComprehensionIterator comprehensionIterator = Iterators[num];
			expression2 = comprehensionIterator.Transform(expression2);
		}
		return System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, expression, expression2, parameterExpression);
	}
}

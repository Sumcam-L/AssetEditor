using System.Linq.Expressions;

namespace IronPython.Compiler.Ast;

public class GeneratorExpression : Expression
{
	private readonly FunctionDefinition _function;

	private readonly Expression _iterable;

	public FunctionDefinition Function => _function;

	public Expression Iterable => _iterable;

	public GeneratorExpression(FunctionDefinition function, Expression iterable)
	{
		_function = function;
		_iterable = iterable;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return System.Linq.Expressions.Expression.Call(AstMethods.MakeGeneratorExpression, _function.MakeFunctionExpression(), _iterable);
	}

	internal override string CheckAssign()
	{
		return "can't assign to generator expression";
	}

	internal override string CheckAugmentedAssign()
	{
		return CheckAssign();
	}

	internal override string CheckDelete()
	{
		return "can't delete generator expression";
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			_function.Walk(walker);
			_iterable.Walk(walker);
		}
		walker.PostWalk(this);
	}
}

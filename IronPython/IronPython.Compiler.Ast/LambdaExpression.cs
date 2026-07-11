using System.Linq.Expressions;

namespace IronPython.Compiler.Ast;

public class LambdaExpression : Expression
{
	private readonly FunctionDefinition _function;

	public FunctionDefinition Function => _function;

	public LambdaExpression(FunctionDefinition function)
	{
		_function = function;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return _function.MakeFunctionExpression();
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && _function != null)
		{
			_function.Walk(walker);
		}
		walker.PostWalk(this);
	}
}

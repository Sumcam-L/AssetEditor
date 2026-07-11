using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler.Ast;

public class YieldExpression : Expression
{
	private readonly Expression _expression;

	public Expression Expression => _expression;

	public override string NodeName => "yield expression";

	public YieldExpression(Expression expression)
	{
		_expression = expression;
	}

	internal static System.Linq.Expressions.Expression CreateCheckThrowExpression(SourceSpan span)
	{
		System.Linq.Expressions.Expression generatorParam = GeneratorRewriter._generatorParam;
		return LightExceptions.CheckAndThrow(System.Linq.Expressions.Expression.Call(AstMethods.GeneratorCheckThrowableAndReturnSendValue, generatorParam));
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return System.Linq.Expressions.Expression.Block(Utils.YieldReturn(Node.GeneratorLabel, Utils.Convert(_expression, typeof(object))), CreateCheckThrowExpression(base.Span));
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && _expression != null)
		{
			_expression.Walk(walker);
		}
		walker.PostWalk(this);
	}

	internal override string CheckAssign()
	{
		return "can't assign to yield expression";
	}

	internal override string CheckAugmentedAssign()
	{
		return CheckAssign();
	}
}

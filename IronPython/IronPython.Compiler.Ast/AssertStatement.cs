using System.Linq.Expressions;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class AssertStatement : Statement
{
	private readonly Expression _test;

	private readonly Expression _message;

	public Expression Test => _test;

	public Expression Message => _message;

	public AssertStatement(Expression test, Expression message)
	{
		_test = test;
		_message = message;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		if (base.Optimize)
		{
			return Utils.Empty();
		}
		return base.GlobalParent.AddDebugInfoAndVoid(Utils.Unless(TransformAndDynamicConvert(_test, typeof(bool)), System.Linq.Expressions.Expression.Call(AstMethods.RaiseAssertionError, Node.TransformOrConstantNull(_message, typeof(object)))), base.Span);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_test != null)
			{
				_test.Walk(walker);
			}
			if (_message != null)
			{
				_message.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}

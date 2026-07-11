using System.Linq.Expressions;

namespace IronPython.Compiler.Ast;

public class ContinueStatement : Statement
{
	private ILoopStatement _loop;

	internal override bool CanThrow => false;

	internal ILoopStatement LoopStatement
	{
		get
		{
			return _loop;
		}
		set
		{
			_loop = value;
		}
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Continue(_loop.ContinueLabel), base.Span);
	}

	public override void Walk(PythonWalker walker)
	{
		walker.Walk(this);
		walker.PostWalk(this);
	}
}

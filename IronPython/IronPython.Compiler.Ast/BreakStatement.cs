using System.Linq.Expressions;

namespace IronPython.Compiler.Ast;

public class BreakStatement : Statement
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
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Break(_loop.BreakLabel), base.Span);
	}

	public override void Walk(PythonWalker walker)
	{
		walker.Walk(this);
		walker.PostWalk(this);
	}
}

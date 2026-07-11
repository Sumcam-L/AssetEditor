using System.Linq.Expressions;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class EmptyStatement : Statement
{
	internal static EmptyStatement PreCompiledInstance = new EmptyStatement();

	internal override bool CanThrow => false;

	public override System.Linq.Expressions.Expression Reduce()
	{
		return base.GlobalParent.AddDebugInfoAndVoid(Utils.Empty(), base.Span);
	}

	public override void Walk(PythonWalker walker)
	{
		walker.Walk(this);
		walker.PostWalk(this);
	}
}

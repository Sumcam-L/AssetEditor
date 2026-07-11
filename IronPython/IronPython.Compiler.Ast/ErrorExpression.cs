namespace IronPython.Compiler.Ast;

public class ErrorExpression : Expression
{
	public override void Walk(PythonWalker walker)
	{
		walker.Walk(this);
		walker.PostWalk(this);
	}
}

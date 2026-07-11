namespace IronPython.Compiler.Ast;

internal class FlowDeleter : PythonWalkerNonRecursive
{
	private readonly FlowChecker _fc;

	public FlowDeleter(FlowChecker fc)
	{
		_fc = fc;
	}

	public override bool Walk(NameExpression node)
	{
		_fc.Delete(node.Name);
		return false;
	}
}

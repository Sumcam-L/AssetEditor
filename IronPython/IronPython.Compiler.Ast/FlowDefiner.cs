namespace IronPython.Compiler.Ast;

internal class FlowDefiner : PythonWalkerNonRecursive
{
	private readonly FlowChecker _fc;

	public FlowDefiner(FlowChecker fc)
	{
		_fc = fc;
	}

	public override bool Walk(NameExpression node)
	{
		_fc.Define(node.Name);
		return false;
	}

	public override bool Walk(MemberExpression node)
	{
		node.Walk(_fc);
		return false;
	}

	public override bool Walk(IndexExpression node)
	{
		node.Walk(_fc);
		return false;
	}

	public override bool Walk(ParenthesisExpression node)
	{
		return true;
	}

	public override bool Walk(TupleExpression node)
	{
		return true;
	}

	public override bool Walk(ListExpression node)
	{
		return true;
	}

	public override bool Walk(Parameter node)
	{
		_fc.Define(node.Name);
		return true;
	}

	public override bool Walk(SublistParameter node)
	{
		return true;
	}
}

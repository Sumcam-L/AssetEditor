namespace IronPython.Compiler.Ast;

internal class DefineBinder : PythonWalkerNonRecursive
{
	private PythonNameBinder _binder;

	public DefineBinder(PythonNameBinder binder)
	{
		_binder = binder;
	}

	public override bool Walk(NameExpression node)
	{
		_binder.DefineName(node.Name);
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
}

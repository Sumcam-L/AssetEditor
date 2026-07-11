namespace IronPython.Compiler.Ast;

internal class DeleteBinder : PythonWalkerNonRecursive
{
	private PythonNameBinder _binder;

	public DeleteBinder(PythonNameBinder binder)
	{
		_binder = binder;
	}

	public override bool Walk(NameExpression node)
	{
		_binder.DefineDeleted(node.Name);
		return false;
	}
}

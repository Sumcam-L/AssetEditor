namespace IronPython.Compiler.Ast;

internal class ParameterBinder : PythonWalkerNonRecursive
{
	private PythonNameBinder _binder;

	public ParameterBinder(PythonNameBinder binder)
	{
		_binder = binder;
	}

	public override bool Walk(Parameter node)
	{
		node.Parent = _binder._currentScope;
		node.PythonVariable = _binder.DefineParameter(node.Name);
		return false;
	}

	public override bool Walk(SublistParameter node)
	{
		node.PythonVariable = _binder.DefineParameter(node.Name);
		node.Parent = _binder._currentScope;
		WalkTuple(node.Tuple);
		return false;
	}

	private void WalkTuple(TupleExpression tuple)
	{
		tuple.Parent = _binder._currentScope;
		foreach (Expression item in tuple.Items)
		{
			if (item is NameExpression nameExpression)
			{
				_binder.DefineName(nameExpression.Name);
				nameExpression.Parent = _binder._currentScope;
				nameExpression.Reference = _binder.Reference(nameExpression.Name);
			}
			else
			{
				WalkTuple((TupleExpression)item);
			}
		}
	}

	public override bool Walk(TupleExpression node)
	{
		node.Parent = _binder._currentScope;
		return true;
	}
}

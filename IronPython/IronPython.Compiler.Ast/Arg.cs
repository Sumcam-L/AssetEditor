using Microsoft.Scripting.Actions;

namespace IronPython.Compiler.Ast;

public class Arg : Node
{
	private readonly string _name;

	private readonly Expression _expression;

	public string Name => _name;

	public Expression Expression => _expression;

	public Arg(Expression expression)
		: this(null, expression)
	{
	}

	public Arg(string name, Expression expression)
	{
		_name = name;
		_expression = expression;
	}

	public override string ToString()
	{
		return base.ToString() + ":" + _name;
	}

	internal Argument GetArgumentInfo()
	{
		if (_name == null)
		{
			return Argument.Simple;
		}
		if (_name == "*")
		{
			return new Argument(ArgumentType.List);
		}
		if (_name == "**")
		{
			return new Argument(ArgumentType.Dictionary);
		}
		return new Argument(_name);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && _expression != null)
		{
			_expression.Walk(walker);
		}
		walker.PostWalk(this);
	}
}

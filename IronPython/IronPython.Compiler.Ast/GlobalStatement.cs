using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class GlobalStatement : Statement
{
	private readonly string[] _names;

	public IList<string> Names => _names;

	internal override bool CanThrow => false;

	public GlobalStatement(string[] names)
	{
		_names = names;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return Utils.Empty();
	}

	public override void Walk(PythonWalker walker)
	{
		walker.Walk(this);
		walker.PostWalk(this);
	}
}

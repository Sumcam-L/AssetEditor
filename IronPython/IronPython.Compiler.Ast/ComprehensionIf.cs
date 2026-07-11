using System.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class ComprehensionIf : ComprehensionIterator
{
	private readonly Expression _test;

	public Expression Test => _test;

	public ComprehensionIf(Expression test)
	{
		_test = test;
	}

	internal override System.Linq.Expressions.Expression Transform(System.Linq.Expressions.Expression body)
	{
		return base.GlobalParent.AddDebugInfoAndVoid(Utils.If(base.GlobalParent.Convert(typeof(bool), ConversionResultKind.ExplicitCast, _test), body), base.Span);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && _test != null)
		{
			_test.Walk(walker);
		}
		walker.PostWalk(this);
	}
}

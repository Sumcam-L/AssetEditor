using System.Linq.Expressions;

namespace IronPython.Compiler.Ast;

public abstract class ComprehensionIterator : Node
{
	internal abstract System.Linq.Expressions.Expression Transform(System.Linq.Expressions.Expression body);
}

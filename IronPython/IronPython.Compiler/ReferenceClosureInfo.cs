using System.Linq.Expressions;
using IronPython.Compiler.Ast;

namespace IronPython.Compiler;

internal class ReferenceClosureInfo
{
	public readonly PythonVariable Variable;

	public bool IsClosedOver;

	public PythonVariable PythonVariable => Variable;

	public ReferenceClosureInfo(PythonVariable variable, int index, System.Linq.Expressions.Expression tupleExpr, bool accessedInThisScope)
	{
		Variable = variable;
		IsClosedOver = accessedInThisScope;
	}
}

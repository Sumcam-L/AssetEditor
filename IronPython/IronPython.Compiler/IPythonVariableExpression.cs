using System.Linq.Expressions;

namespace IronPython.Compiler;

internal interface IPythonVariableExpression
{
	Expression Assign(Expression value);

	Expression Delete();

	Expression Create();
}

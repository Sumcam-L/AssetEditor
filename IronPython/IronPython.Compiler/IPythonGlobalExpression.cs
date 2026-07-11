using System.Linq.Expressions;

namespace IronPython.Compiler;

internal interface IPythonGlobalExpression : IPythonVariableExpression
{
	Expression RawValue();
}

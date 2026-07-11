using System.Dynamic;
using System.Linq.Expressions;

namespace IronPython.Runtime.Binding;

internal interface IPythonInvokable
{
	DynamicMetaObject Invoke(PythonInvokeBinder pythonInvoke, Expression codeContext, DynamicMetaObject target, DynamicMetaObject[] args);
}

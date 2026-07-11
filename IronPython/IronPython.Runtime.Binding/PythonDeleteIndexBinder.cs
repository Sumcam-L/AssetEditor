using System.Dynamic;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class PythonDeleteIndexBinder : DeleteIndexBinder, IPythonSite, IExpressionSerializable
{
	private readonly PythonContext _context;

	public PythonContext Context => _context;

	public PythonDeleteIndexBinder(PythonContext context, int argCount)
		: base(new CallInfo(argCount))
	{
		_context = context;
	}

	public override DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
	{
		return PythonProtocol.Index(this, PythonIndexType.DeleteItem, ArrayUtils.Insert(target, indexes), errorSuggestion);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ _context.Binder.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PythonDeleteIndexBinder pythonDeleteIndexBinder))
		{
			return false;
		}
		if (pythonDeleteIndexBinder._context.Binder == _context.Binder)
		{
			return base.Equals(obj);
		}
		return false;
	}

	public Expression CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeDeleteIndexAction"), BindingHelpers.CreateBinderStateExpression(), Utils.Constant(base.CallInfo.ArgumentCount));
	}
}

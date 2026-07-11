using System.Dynamic;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding;

internal class PythonSetSliceBinder : DynamicMetaObjectBinder, IPythonSite, IExpressionSerializable
{
	private readonly PythonContext _context;

	public PythonContext Context => _context;

	public PythonSetSliceBinder(PythonContext context)
	{
		_context = context;
	}

	public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
	{
		return PythonProtocol.Index(this, PythonIndexType.SetSlice, ArrayUtils.Insert(target, args));
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ _context.Binder.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PythonSetSliceBinder pythonSetSliceBinder))
		{
			return false;
		}
		if (pythonSetSliceBinder._context.Binder == _context.Binder)
		{
			return base.Equals(obj);
		}
		return false;
	}

	public Expression CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeSetSliceBinder"), BindingHelpers.CreateBinderStateExpression());
	}
}

using System.Dynamic;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Binding;

internal class PythonDeleteMemberBinder : DeleteMemberBinder, IPythonSite, IExpressionSerializable
{
	private readonly PythonContext _context;

	public PythonContext Context => _context;

	public PythonDeleteMemberBinder(PythonContext context, string name)
		: base(name, ignoreCase: false)
	{
		_context = context;
	}

	public PythonDeleteMemberBinder(PythonContext context, string name, bool ignoreCase)
		: base(name, ignoreCase)
	{
		_context = context;
	}

	public override DynamicMetaObject FallbackDeleteMember(DynamicMetaObject self, DynamicMetaObject errorSuggestion)
	{
		if (self.NeedsDeferral())
		{
			return Defer(self);
		}
		return Context.Binder.DeleteMember(base.Name, self, new PythonOverloadResolverFactory(_context.Binder, Utils.Constant(Context.SharedContext)), errorSuggestion);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ _context.Binder.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PythonDeleteMemberBinder pythonDeleteMemberBinder))
		{
			return false;
		}
		if (pythonDeleteMemberBinder._context.Binder == _context.Binder)
		{
			return base.Equals(obj);
		}
		return false;
	}

	public override string ToString()
	{
		return "Python DeleteMember " + base.Name;
	}

	public Expression CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeDeleteAction"), BindingHelpers.CreateBinderStateExpression(), Utils.Constant(base.Name));
	}
}

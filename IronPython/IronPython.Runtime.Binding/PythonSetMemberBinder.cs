using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Binding;

internal class PythonSetMemberBinder : SetMemberBinder, IPythonSite, IExpressionSerializable
{
	private readonly PythonContext _context;

	public PythonContext Context => _context;

	public PythonSetMemberBinder(PythonContext context, string name)
		: base(name, ignoreCase: false)
	{
		_context = context;
	}

	public PythonSetMemberBinder(PythonContext context, string name, bool ignoreCase)
		: base(name, ignoreCase)
	{
		_context = context;
	}

	public override DynamicMetaObject FallbackSetMember(DynamicMetaObject self, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
	{
		if (self.NeedsDeferral())
		{
			return Defer(self, value);
		}
		if (ComBinder.TryBindSetMember(this, self, BindingHelpers.GetComArgument(value), out var result))
		{
			return result;
		}
		return Context.Binder.SetMember(base.Name, self, value, errorSuggestion, new PythonOverloadResolverFactory(_context.Binder, Utils.Constant(Context.SharedContext)));
	}

	public override T BindDelegate<T>(CallSite<T> site, object[] args)
	{
		if (args[0] is IFastSettable fastSettable)
		{
			T val = fastSettable.MakeSetBinding(site, this);
			if (val != null)
			{
				return val;
			}
		}
		if (args[0] is IPythonObject pythonObject && !(pythonObject is IProxyObject))
		{
			FastBindResult<T> fastBindResult = UserTypeOps.MakeSetBinding(Context.SharedContext, site, pythonObject, args[1], this);
			if (fastBindResult.Target != null)
			{
				if (fastBindResult.ShouldCache)
				{
					CacheTarget(fastBindResult.Target);
				}
				return fastBindResult.Target;
			}
		}
		return base.BindDelegate(site, args);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ _context.Binder.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PythonSetMemberBinder pythonSetMemberBinder))
		{
			return false;
		}
		if (pythonSetMemberBinder._context.Binder == _context.Binder)
		{
			return base.Equals(obj);
		}
		return false;
	}

	public override string ToString()
	{
		return "Python SetMember " + base.Name;
	}

	public Expression CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeSetAction"), BindingHelpers.CreateBinderStateExpression(), Utils.Constant(base.Name));
	}
}

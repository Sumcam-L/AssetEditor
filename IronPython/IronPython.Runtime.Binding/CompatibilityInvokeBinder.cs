using System.Dynamic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.ComInterop;

namespace IronPython.Runtime.Binding;

internal class CompatibilityInvokeBinder : InvokeBinder, IPythonSite
{
	private readonly PythonContext _context;

	public PythonContext Context => _context;

	public CompatibilityInvokeBinder(PythonContext context, CallInfo callInfo)
		: base(callInfo)
	{
		_context = context;
	}

	public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
	{
		if (target.Value is IDynamicMetaObjectProvider && errorSuggestion == null)
		{
			return target.BindCreateInstance(_context.Create(this, base.CallInfo), args);
		}
		if (ComBinder.TryBindInvoke(this, target, BindingHelpers.GetComArguments(args), out var result))
		{
			return result;
		}
		return InvokeFallback(target, args, BindingHelpers.CallInfoToSignature(base.CallInfo), errorSuggestion);
	}

	internal DynamicMetaObject InvokeFallback(DynamicMetaObject target, DynamicMetaObject[] args, CallSignature sig, DynamicMetaObject errorSuggestion)
	{
		return PythonProtocol.Call(this, target, args) ?? Context.Binder.Create(sig, target, args, Utils.Constant(_context.SharedContext)) ?? Context.Binder.Call(sig, errorSuggestion, new PythonOverloadResolverFactory(Context.Binder, Utils.Constant(_context.SharedContext)), target, args);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ _context.Binder.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CompatibilityInvokeBinder compatibilityInvokeBinder))
		{
			return false;
		}
		if (compatibilityInvokeBinder._context.Binder == _context.Binder)
		{
			return base.Equals(obj);
		}
		return false;
	}
}

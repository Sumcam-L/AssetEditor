using System.Dynamic;

namespace IronPython.Runtime.Binding;

internal class CreateFallback : CreateInstanceBinder, IPythonSite
{
	private readonly CompatibilityInvokeBinder _fallback;

	public PythonContext Context => _fallback.Context;

	public CreateFallback(CompatibilityInvokeBinder realFallback, CallInfo callInfo)
		: base(callInfo)
	{
		_fallback = realFallback;
	}

	public override DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
	{
		return _fallback.InvokeFallback(target, args, BindingHelpers.GetCallSignature(this), errorSuggestion);
	}
}

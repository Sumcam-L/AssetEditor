using System;
using System.Dynamic;

namespace IronPython.Runtime.Binding;

internal class CompatConversionBinder : ConvertBinder
{
	private readonly PythonConversionBinder _binder;

	public CompatConversionBinder(PythonConversionBinder binder, Type toType, bool isExplicit)
		: base(toType, isExplicit)
	{
		_binder = binder;
	}

	public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
	{
		return _binder.FallbackConvert(ReturnType, target, errorSuggestion);
	}
}

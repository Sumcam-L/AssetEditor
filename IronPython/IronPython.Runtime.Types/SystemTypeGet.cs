using System;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;

namespace IronPython.Runtime.Types;

internal class SystemTypeGet : TypeGetBase
{
	private readonly PythonType _self;

	public SystemTypeGet(PythonGetMemberBinder binder, FastGetDelegate[] delegates, PythonType type, bool isMeta, bool optimizing)
		: base(binder, delegates)
	{
		_self = type;
		if (optimizing)
		{
			if (isMeta)
			{
				_func = MetaOnlyTargetOptimizing;
			}
			else
			{
				_func = TargetOptimizing;
			}
		}
		else if (isMeta)
		{
			_func = MetaOnlyTarget;
		}
		else
		{
			_func = Target;
		}
	}

	public object Target(CallSite site, object self, CodeContext context)
	{
		if (self == _self)
		{
			return RunDelegatesNoOptimize(self, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object MetaOnlyTarget(CallSite site, object self, CodeContext context)
	{
		if (self is PythonType)
		{
			return RunDelegatesNoOptimize(self, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object TargetOptimizing(CallSite site, object self, CodeContext context)
	{
		if (self == _self && base.ShouldUseNonOptimizedSite)
		{
			return RunDelegates(self, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object MetaOnlyTargetOptimizing(CallSite site, object self, CodeContext context)
	{
		if (self is PythonType && base.ShouldUseNonOptimizedSite)
		{
			return RunDelegates(self, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public override bool IsValid(PythonType type)
	{
		if (_func == new Func<CallSite, object, CodeContext, object>(MetaOnlyTarget) || _func == new Func<CallSite, object, CodeContext, object>(MetaOnlyTargetOptimizing))
		{
			return true;
		}
		return type == _self;
	}
}

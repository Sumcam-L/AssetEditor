using System;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types;

internal class TypeGet : TypeGetBase
{
	private int _version;

	public TypeGet(PythonGetMemberBinder binder, FastGetDelegate[] delegates, int version, bool isMeta, bool canOptimize)
		: base(binder, delegates)
	{
		_version = version;
		if (canOptimize)
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
		if (self is PythonType pythonType && pythonType.Version == _version)
		{
			return RunDelegatesNoOptimize(self, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object MetaOnlyTarget(CallSite site, object self, CodeContext context)
	{
		if (self is PythonType o && PythonOps.CheckTypeVersion(o, _version))
		{
			return RunDelegatesNoOptimize(self, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object TargetOptimizing(CallSite site, object self, CodeContext context)
	{
		if (self is PythonType pythonType && pythonType.Version == _version && base.ShouldUseNonOptimizedSite)
		{
			return RunDelegates(self, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object MetaOnlyTargetOptimizing(CallSite site, object self, CodeContext context)
	{
		if (self is PythonType o && PythonOps.CheckTypeVersion(o, _version) && base.ShouldUseNonOptimizedSite)
		{
			return RunDelegates(self, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public override bool IsValid(PythonType type)
	{
		if (_func == new Func<CallSite, object, CodeContext, object>(MetaOnlyTarget) || _func == new Func<CallSite, object, CodeContext, object>(MetaOnlyTargetOptimizing))
		{
			return PythonOps.CheckTypeVersion(type, _version);
		}
		return type.Version == _version;
	}
}

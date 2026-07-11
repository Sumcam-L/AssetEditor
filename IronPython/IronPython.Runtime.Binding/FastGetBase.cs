using System;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding;

internal abstract class FastGetBase
{
	internal Func<CallSite, object, CodeContext, object> _func;

	internal int _hitCount;

	internal virtual bool ShouldCache => true;

	internal bool ShouldUseNonOptimizedSite => _hitCount < 100;

	public abstract bool IsValid(PythonType type);

	protected static object Update(CallSite site, object self, CodeContext context)
	{
		return ((CallSite<Func<CallSite, object, CodeContext, object>>)site).Update(site, self, context);
	}
}

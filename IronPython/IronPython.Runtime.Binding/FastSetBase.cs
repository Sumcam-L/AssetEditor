using System;
using System.Runtime.CompilerServices;

namespace IronPython.Runtime.Binding;

internal class FastSetBase
{
	internal Delegate _func;

	internal int _version;

	internal int _hitCount;

	public bool ShouldUseNonOptimizedSite => _hitCount < 100;

	public FastSetBase(int version)
	{
		_version = version;
	}
}
internal class FastSetBase<TValue> : FastSetBase
{
	public FastSetBase(int version)
		: base(version)
	{
	}

	protected static object Update(CallSite site, object self, TValue value)
	{
		return ((CallSite<Func<CallSite, object, TValue, object>>)site).Update(site, self, value);
	}
}

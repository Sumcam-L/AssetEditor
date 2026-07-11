using System;

namespace IronPython.Runtime.Types;

internal sealed class CachedGetIdWeakRefExtensionMethod : CachedGetKey
{
	private readonly WeakReference _extMethodSet;

	public CachedGetIdWeakRefExtensionMethod(string name, WeakReference weakReference)
		: base(name)
	{
		_extMethodSet = weakReference;
	}

	public override bool Equals(CachedGetKey other)
	{
		if (!(other is CachedGetIdWeakRefExtensionMethod cachedGetIdWeakRefExtensionMethod))
		{
			return false;
		}
		if (cachedGetIdWeakRefExtensionMethod._extMethodSet.Target == _extMethodSet.Target)
		{
			return cachedGetIdWeakRefExtensionMethod.Name == Name;
		}
		return false;
	}
}

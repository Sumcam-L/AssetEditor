using System;

namespace IronPython.Runtime.Types;

internal abstract class CachedGetKey : IEquatable<CachedGetKey>
{
	public readonly string Name;

	public CachedGetKey(string name)
	{
		Name = name;
	}

	public static CachedGetKey Make(string name, ExtensionMethodSet set)
	{
		if (set.Id != int.MinValue)
		{
			return new CachedGetIdIntExtensionMethod(name, set.Id);
		}
		return new CachedGetIdWeakRefExtensionMethod(name, new WeakReference(set));
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CachedGetKey key))
		{
			return false;
		}
		return Equals(key);
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}

	public abstract bool Equals(CachedGetKey key);
}

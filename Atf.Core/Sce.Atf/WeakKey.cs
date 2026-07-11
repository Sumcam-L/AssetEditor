using System;

namespace Sce.Atf;

public class WeakKey<T> : WeakReference where T : class
{
	private readonly int m_hashCode;

	public WeakKey(T target)
		: base(target)
	{
		m_hashCode = target.GetHashCode();
	}

	public override int GetHashCode()
	{
		return m_hashCode;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is WeakKey<T> weakKey && weakKey.m_hashCode == m_hashCode)
		{
			T val = (T)Target;
			if (val != null)
			{
				return val.Equals(weakKey.Target);
			}
		}
		return false;
	}

	public static implicit operator T(WeakKey<T> reference)
	{
		return (T)reference.Target;
	}
}

using System;
using System.Collections.Generic;

namespace Sce.Atf.Adaptation;

public class AdaptablePath<T> : Path<T>, IAdaptable, IDecoratable
{
	public AdaptablePath(T last)
		: base(last)
	{
	}

	public AdaptablePath(IEnumerable<T> path)
		: base(path)
	{
	}

	public AdaptablePath(ICollection<T> path)
		: base(path)
	{
	}

	public object GetAdapter(Type type)
	{
		object obj = base.Last.As(type);
		if (obj != null)
		{
			return obj;
		}
		if (type.IsAssignableFrom(GetType()))
		{
			return this;
		}
		return null;
	}

	public IEnumerable<object> GetDecorators(Type type)
	{
		foreach (object item in Last.AsAll(type))
		{
			yield return item;
		}
	}

	public U As<U>() where U : class
	{
		return Adapters.As<U>(this);
	}

	public U Cast<U>() where U : class
	{
		return Adapters.Cast<U>(this);
	}

	public bool Is<U>() where U : class
	{
		return Adapters.Is<U>(this);
	}

	public IEnumerable<U> AsAll<U>() where U : class
	{
		return Adapters.AsAll<U>(this);
	}

	public static AdaptablePath<T> operator +(T lhs, AdaptablePath<T> rhs)
	{
		if (rhs == null)
		{
			return new AdaptablePath<T>(lhs);
		}
		T[] array = new T[1 + rhs.Count];
		array[0] = lhs;
		rhs.CopyTo(array, 1);
		return new AdaptablePath<T>(array);
	}

	public static AdaptablePath<T> operator +(AdaptablePath<T> lhs, T rhs)
	{
		if (lhs == null)
		{
			return new AdaptablePath<T>(rhs);
		}
		T[] array = new T[lhs.Count + 1];
		lhs.CopyTo(array, 0);
		array[lhs.Count] = rhs;
		return new AdaptablePath<T>(array);
	}

	public static AdaptablePath<T> operator +(AdaptablePath<T> lhs, AdaptablePath<T> rhs)
	{
		if (lhs == null)
		{
			return rhs;
		}
		if (rhs == null)
		{
			return lhs;
		}
		T[] array = new T[lhs.Count + rhs.Count];
		lhs.CopyTo(array, 0);
		rhs.CopyTo(array, lhs.Count);
		return new AdaptablePath<T>(array);
	}

	protected override U Convert<U>(T item)
	{
		return item.As<U>();
	}
}

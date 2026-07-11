using System;
using System.Collections;
using System.Collections.Generic;

namespace Sce.Atf.Adaptation;

public static class Adapters
{
	public static object As(this object reference, Type type)
	{
		if (reference == null)
		{
			return null;
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (type.IsAssignableFrom(reference.GetType()))
		{
			return reference;
		}
		if (reference is IAdaptable adaptable)
		{
			object adapter = adaptable.GetAdapter(type);
			if (adapter != null)
			{
				return adapter;
			}
		}
		return null;
	}

	public static T As<T>(this object reference) where T : class
	{
		if (reference == null)
		{
			return null;
		}
		T val = reference as T;
		if (val == null && reference is IAdaptable adaptable)
		{
			val = adaptable.GetAdapter(typeof(T)) as T;
		}
		return val;
	}

	public static T As<T>(this IAdaptable adaptable) where T : class
	{
		if (adaptable == null)
		{
			return null;
		}
		T val = adaptable as T;
		if (val == null)
		{
			val = adaptable.GetAdapter(typeof(T)) as T;
		}
		return val;
	}

	public static object Cast(this object reference, Type type)
	{
		object obj = reference.As(type);
		if (obj == null)
		{
			throw new AdaptationException(type.Name + " adapter required");
		}
		return obj;
	}

	public static T Cast<T>(this object reference) where T : class
	{
		T val = reference.As<T>();
		if (val == null)
		{
			throw new AdaptationException(typeof(T).Name + " adapter required");
		}
		return val;
	}

	public static T Cast<T>(this IAdaptable adaptable) where T : class
	{
		T val = adaptable.As<T>();
		if (val == null)
		{
			throw new AdaptationException(typeof(T).Name + " adapter required");
		}
		return val;
	}

	public static bool Is(this object reference, Type type)
	{
		return reference.As(type) != null;
	}

	public static bool Is<T>(this object reference) where T : class
	{
		return reference.As<T>() != null;
	}

	public static bool Is<T>(this IAdaptable adaptable) where T : class
	{
		return adaptable.As<T>() != null;
	}

	public static IEnumerable<object> AsAll(this object reference, Type type)
	{
		if (reference != null)
		{
			if (reference is IDecoratable decoratable)
			{
				return decoratable.GetDecorators(type);
			}
			if (type.IsAssignableFrom(reference.GetType()))
			{
				return new object[1] { reference };
			}
		}
		return EmptyEnumerable<object>.Instance;
	}

	public static IEnumerable<T> AsAll<T>(this object reference) where T : class
	{
		if (reference != null)
		{
			if (reference is IDecoratable decoratable)
			{
				return decoratable.AsAll<T>();
			}
			if (reference is T val)
			{
				return new T[1] { val };
			}
		}
		return EmptyEnumerable<T>.Instance;
	}

	public static IEnumerable<T> AsAll<T>(this IDecoratable decoratable) where T : class
	{
		if (decoratable == null)
		{
			yield break;
		}
		foreach (object decorator in decoratable.GetDecorators(typeof(T)))
		{
			yield return decorator as T;
		}
	}

	public static IEnumerable<object> AsIEnumerable(this IEnumerable enumerable, Type type)
	{
		if (enumerable == null)
		{
			yield break;
		}
		foreach (object item in enumerable)
		{
			object adapter = item.As(type);
			if (adapter != null)
			{
				yield return adapter;
			}
		}
	}

	public static IEnumerable<T> AsIEnumerable<T>(this IEnumerable enumerable) where T : class
	{
		if (enumerable == null)
		{
			yield break;
		}
		foreach (object item in enumerable)
		{
			T adapter = item.As<T>();
			if (adapter != null)
			{
				yield return adapter;
			}
		}
	}

	public static bool Any(this IEnumerable enumerable, Type type)
	{
		if (enumerable != null)
		{
			foreach (object item in enumerable)
			{
				object obj = item.As(type);
				if (obj != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool Any<T>(this IEnumerable enumerable) where T : class
	{
		return enumerable.Any(typeof(T));
	}

	public static bool All(this IEnumerable enumerable, Type type)
	{
		if (enumerable != null)
		{
			foreach (object item in enumerable)
			{
				object obj = item.As(type);
				if (obj == null)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool All<T>(this IEnumerable enumerable) where T : class
	{
		return enumerable.All(typeof(T));
	}
}

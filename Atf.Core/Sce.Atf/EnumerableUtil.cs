using System;
using System.Collections.Generic;

namespace Sce.Atf;

public static class EnumerableUtil
{
	public static void ForEachWithIndex<T>(this IEnumerable<T> that, Action<T, int> action)
	{
		int num = 0;
		foreach (T item in that)
		{
			action(item, num);
			num++;
		}
	}

	public static void ForEach<T>(this IEnumerable<T> that, Action<T> action)
	{
		foreach (T item in that)
		{
			action(item);
		}
	}

	public static void ForEachWhileTrue<T>(this IEnumerable<T> that, Func<T, bool> action)
	{
		foreach (T item in that)
		{
			if (!action(item))
			{
				break;
			}
		}
	}

	public static int IndexOf<T>(this IEnumerable<T> source, T value)
	{
		return source.IndexOf(value, null);
	}

	public static int IndexOf<T>(this IEnumerable<T> source, T value, IEqualityComparer<T> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (source is IList<T> list)
		{
			return list.IndexOf(value);
		}
		if (comparer == null)
		{
			comparer = EqualityComparer<T>.Default;
		}
		int num = 0;
		foreach (T item in source)
		{
			if (comparer.Equals(item, value))
			{
				return num;
			}
			num++;
		}
		return -1;
	}
}

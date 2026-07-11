using System.Collections.Generic;
using System.Linq;

namespace Firaxis.Collections;

public static class ICollectionExtensions
{
	public static void Add(this ICollection<string> list, string formatString, params object[] args)
	{
		list.Add(string.Format(formatString, args));
	}

	public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			list.Add(item);
		}
	}

	public static void Remove<T>(this ICollection<T> list, IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			list.Remove(item);
		}
	}

	public static IEnumerable<T> MoveThreadSafe<T>(this ICollection<T> lockedCollection)
	{
		return lockedCollection.MoveThreadSafe(lockedCollection);
	}

	public static IEnumerable<T> MoveThreadSafe<T>(this ICollection<T> lockedCollection, object locker)
	{
		IEnumerable<T> result = null;
		lock (locker)
		{
			result = lockedCollection.ToArray();
			lockedCollection.Clear();
		}
		return result;
	}
}

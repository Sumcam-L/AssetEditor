using System;
using System.Collections.Generic;
using System.Linq;

namespace Firaxis.Collections;

public static class IEnumerableExtensions
{
	public static bool RemoveIf<T>(this ICollection<T> collection, Func<T, bool> test)
	{
		T[] array = collection.Where(test).ToArray();
		if (array.Length == 0)
		{
			return false;
		}
		T[] array2 = array;
		foreach (T item in array2)
		{
			collection.Remove(item);
		}
		return true;
	}

	public static int UniqueCount<T>(this IEnumerable<T> list)
	{
		HashSet<T> hashSet = new HashSet<T>();
		list.All(hashSet.Add);
		return hashSet.Count;
	}

	public static bool AllUnique<T>(this IEnumerable<T> list)
	{
		HashSet<T> hashSet = new HashSet<T>();
		return list.All(hashSet.Add);
	}

	public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
	{
		foreach (T item in list)
		{
			action(item);
		}
	}

	public static T Find<T>(this IEnumerable<T> list, Predicate<T> match)
	{
		foreach (T item in list)
		{
			if (match(item))
			{
				return item;
			}
		}
		return default(T);
	}

	public static IEnumerable<U> GetElementsByType<T, U>(this IEnumerable<T> collection) where U : class
	{
		return from item in collection
			where item is U
			select item as U;
	}

	public static bool IsEquivalentTo<T>(this IEnumerable<T> collectionOne, IEnumerable<T> collectionTwo) where T : class, IEquatable<T>, IComparable<T>
	{
		int num = collectionOne.Count();
		int num2 = collectionTwo.Count();
		bool flag = num == num2;
		if (flag)
		{
			List<T> list = collectionOne.OrderBy((T x) => x).ToList();
			List<T> list2 = collectionTwo.OrderBy((T x) => x).ToList();
			int num3 = 0;
			while (flag && num3 < num)
			{
				flag = list2[num3].Equals(list[num3]);
				num3++;
			}
		}
		return flag;
	}

	public static bool IsEquivalentTo(this IEnumerable<Uri> collectionOne, IEnumerable<Uri> collectionTwo)
	{
		int num = collectionOne.Count();
		int num2 = collectionTwo.Count();
		bool flag = num == num2;
		if (flag)
		{
			List<Uri> list = collectionOne.OrderBy((Uri x) => x.LocalPath).ToList();
			List<Uri> list2 = collectionTwo.OrderBy((Uri x) => x.LocalPath).ToList();
			int num3 = 0;
			while (flag && num3 < num)
			{
				flag = list2[num3].Equals(list[num3]);
				num3++;
			}
		}
		return flag;
	}
}

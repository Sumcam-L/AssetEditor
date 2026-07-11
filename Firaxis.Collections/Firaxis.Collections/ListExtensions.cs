using System;
using System.Collections.Generic;
using System.Linq;

namespace Firaxis.Collections;

public static class ListExtensions
{
	public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
	{
		return listToClone.Select((T item) => (T)item.Clone()).ToList();
	}

	public static void MoveAt<T>(this IList<T> list, T item, int index)
	{
		if (index < 0 || index >= list.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		int num = list.IndexOf(item);
		if (index != num)
		{
			list.Remove(item);
			list.Insert(index, item);
		}
	}

	public static IEnumerable<T> ReverseIterator<T>(this IList<T> list)
	{
		int count = list.Count;
		for (int i = count - 1; i >= 0; i--)
		{
			yield return list[i];
		}
	}

	public static IEnumerable<T> RemoveWhere<T>(this List<T> list, Predicate<T> condition)
	{
		List<T> list2 = new List<T>();
		int num = list.Count - 1;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			T val = list[i];
			if (condition(val))
			{
				list2.Add(val);
				list[i] = list[num--];
				num2++;
				i--;
			}
		}
		list.RemoveRange(num, num2);
		return list2;
	}
}

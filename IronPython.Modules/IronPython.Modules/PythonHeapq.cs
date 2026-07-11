using System;
using System.Collections;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonHeapq
{
	public const string __doc__ = "implements a heapq or priority queue.";

	public const string __about__ = "Heaps are arrays for which a[k] <= a[2*k+1] and a[k] <= a[2*k+2] for all k.";

	[Documentation("Transform list into a heap, in-place, in O(len(heap)) time.")]
	public static void heapify(CodeContext context, List list)
	{
		lock (list)
		{
			DoHeapify(context, list);
		}
	}

	[Documentation("Pop the smallest item off the heap, maintaining the heap invariant.")]
	public static object heappop(CodeContext context, List list)
	{
		lock (list)
		{
			int num = list._size - 1;
			if (num < 0)
			{
				throw PythonOps.IndexError("index out of range");
			}
			list.FastSwap(0, num);
			list._size--;
			SiftDown(context, list, 0, num - 1);
			return list._data[list._size];
		}
	}

	[Documentation("Push item onto heap, maintaining the heap invariant.")]
	public static void heappush(CodeContext context, List list, object item)
	{
		lock (list)
		{
			list.AddNoLock(item);
			SiftUp(context, list, list._size - 1);
		}
	}

	[Documentation("Push item on the heap, then pop and return the smallest item\nfrom the heap. The combined action runs more efficiently than\nheappush() followed by a separate call to heappop().")]
	public static object heappushpop(CodeContext context, List list, object item)
	{
		lock (list)
		{
			return DoPushPop(context, list, item);
		}
	}

	[Documentation("Pop and return the current smallest value, and add the new item.\n\nThis is more efficient than heappop() followed by heappush(), and can be\nmore appropriate when using a fixed-size heap. Note that the value\nreturned may be larger than item!  That constrains reasonable uses of\nthis routine unless written as part of a conditional replacement:\n\n        if item > heap[0]:\n            item = heapreplace(heap, item)\n")]
	public static object heapreplace(CodeContext context, List list, object item)
	{
		lock (list)
		{
			object result = list._data[0];
			list._data[0] = item;
			SiftDown(context, list, 0, list._size - 1);
			return result;
		}
	}

	[Documentation("Find the n largest elements in a dataset.\n\nEquivalent to:  sorted(iterable, reverse=True)[:n]\n")]
	public static List nlargest(CodeContext context, int n, object iterable)
	{
		if (n <= 0)
		{
			return new List();
		}
		List list = new List(Math.Min(n, 4000));
		IEnumerator enumerator = PythonOps.GetEnumerator(iterable);
		for (int i = 0; i < n; i++)
		{
			if (!enumerator.MoveNext())
			{
				HeapSort(context, list, reverse: true);
				return list;
			}
			list.append(enumerator.Current);
		}
		DoHeapify(context, list);
		while (enumerator.MoveNext())
		{
			DoPushPop(context, list, enumerator.Current);
		}
		HeapSort(context, list, reverse: true);
		return list;
	}

	[Documentation("Find the n smallest elements in a dataset.\n\nEquivalent to:  sorted(iterable)[:n]\n")]
	public static List nsmallest(CodeContext context, int n, object iterable)
	{
		if (n <= 0)
		{
			return new List();
		}
		List list = new List(Math.Min(n, 4000));
		IEnumerator enumerator = PythonOps.GetEnumerator(iterable);
		for (int i = 0; i < n; i++)
		{
			if (!enumerator.MoveNext())
			{
				HeapSort(context, list);
				return list;
			}
			list.append(enumerator.Current);
		}
		DoHeapifyMax(context, list);
		while (enumerator.MoveNext())
		{
			DoPushPopMax(context, list, enumerator.Current);
		}
		HeapSort(context, list);
		return list;
	}

	private static bool IsLessThan(CodeContext context, object x, object y)
	{
		if (PythonTypeOps.TryInvokeBinaryOperator(context, x, y, "__lt__", out var value) && !object.ReferenceEquals(value, NotImplementedType.Value))
		{
			return Converter.ConvertToBoolean(value);
		}
		if (PythonTypeOps.TryInvokeBinaryOperator(context, y, x, "__le__", out value) && !object.ReferenceEquals(value, NotImplementedType.Value))
		{
			return !Converter.ConvertToBoolean(value);
		}
		return PythonContext.GetContext(context).LessThan(x, y);
	}

	private static void HeapSort(CodeContext context, List list)
	{
		HeapSort(context, list, reverse: false);
	}

	private static void HeapSort(CodeContext context, List list, bool reverse)
	{
		if (reverse)
		{
			DoHeapify(context, list);
		}
		else
		{
			DoHeapifyMax(context, list);
		}
		int num = list._size - 1;
		while (num > 0)
		{
			list.FastSwap(0, num);
			num--;
			if (reverse)
			{
				SiftDown(context, list, 0, num);
			}
			else
			{
				SiftDownMax(context, list, 0, num);
			}
		}
	}

	private static void DoHeapify(CodeContext context, List list)
	{
		int num = list._size - 1;
		for (int num2 = (num - 1) / 2; num2 >= 0; num2--)
		{
			SiftDown(context, list, num2, num);
		}
	}

	private static void DoHeapifyMax(CodeContext context, List list)
	{
		int num = list._size - 1;
		for (int num2 = (num - 1) / 2; num2 >= 0; num2--)
		{
			SiftDownMax(context, list, num2, num);
		}
	}

	private static object DoPushPop(CodeContext context, List heap, object item)
	{
		object result;
		if (heap._size == 0 || !IsLessThan(context, result = heap._data[0], item))
		{
			return item;
		}
		heap._data[0] = item;
		SiftDown(context, heap, 0, heap._size - 1);
		return result;
	}

	private static object DoPushPopMax(CodeContext context, List heap, object item)
	{
		object result;
		if (heap._size == 0 || !IsLessThan(context, item, result = heap._data[0]))
		{
			return item;
		}
		heap._data[0] = item;
		SiftDownMax(context, heap, 0, heap._size - 1);
		return result;
	}

	private static void SiftDown(CodeContext context, List heap, int start, int stop)
	{
		int num = start;
		int num2;
		while ((num2 = num * 2 + 1) <= stop)
		{
			if (num2 + 1 <= stop && IsLessThan(context, heap._data[num2 + 1], heap._data[num2]))
			{
				num2++;
			}
			if (IsLessThan(context, heap._data[num2], heap._data[num]))
			{
				heap.FastSwap(num, num2);
				num = num2;
				continue;
			}
			break;
		}
	}

	private static void SiftDownMax(CodeContext context, List heap, int start, int stop)
	{
		int num = start;
		int num2;
		while ((num2 = num * 2 + 1) <= stop)
		{
			if (num2 + 1 <= stop && IsLessThan(context, heap._data[num2], heap._data[num2 + 1]))
			{
				num2++;
			}
			if (IsLessThan(context, heap._data[num], heap._data[num2]))
			{
				heap.FastSwap(num, num2);
				num = num2;
				continue;
			}
			break;
		}
	}

	private static void SiftUp(CodeContext context, List heap, int index)
	{
		while (index > 0)
		{
			int num = (index - 1) / 2;
			if (IsLessThan(context, heap._data[index], heap._data[num]))
			{
				heap.FastSwap(num, index);
				index = num;
				continue;
			}
			break;
		}
	}

	private static void SiftUpMax(CodeContext context, List heap, int index)
	{
		while (index > 0)
		{
			int num = (index - 1) / 2;
			if (IsLessThan(context, heap._data[num], heap._data[index]))
			{
				heap.FastSwap(num, index);
				index = num;
				continue;
			}
			break;
		}
	}
}

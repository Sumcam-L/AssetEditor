using System;
using System.Collections;
using System.Runtime.InteropServices;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public class PythonBisectModule
{
	public const string __doc__ = "Bisection algorithms.\r\n\r\nThis module provides support for maintaining a list in sorted order without\r\nhaving to sort the list after each insertion. For long lists of items with\r\nexpensive comparison operations, this can be an improvement over the more\r\ncommon approach.\r\n";

	private static int InternalBisectLeft(CodeContext context, List list, object item, int lo, int hi)
	{
		if (lo < 0)
		{
			throw PythonOps.ValueError("lo must be non-negative");
		}
		if (hi == -1)
		{
			hi = list.Count;
		}
		IComparer comparer = PythonContext.GetContext(context).GetComparer(null, GetComparisonType(list));
		while (lo < hi)
		{
			int num = (lo + hi) / 2;
			object x = list[num];
			if (comparer.Compare(x, item) < 0)
			{
				lo = num + 1;
			}
			else
			{
				hi = num;
			}
		}
		return lo;
	}

	private static int InternalBisectLeft(CodeContext context, object list, object item, int lo, int hi)
	{
		if (lo < 0)
		{
			throw PythonOps.ValueError("lo must be non-negative");
		}
		if (hi == -1)
		{
			hi = PythonOps.Length(list);
		}
		IComparer comparer = PythonContext.GetContext(context).GetComparer(null, GetComparisonType(context, list));
		while (lo < hi)
		{
			int num = (lo + hi) / 2;
			object index = PythonOps.GetIndex(context, list, num);
			if (comparer.Compare(index, item) < 0)
			{
				lo = num + 1;
			}
			else
			{
				hi = num;
			}
		}
		return lo;
	}

	private static int InternalBisectRight(CodeContext context, List list, object item, int lo, int hi)
	{
		if (lo < 0)
		{
			throw PythonOps.ValueError("lo must be non-negative");
		}
		if (hi == -1)
		{
			hi = list.Count;
		}
		IComparer comparer = PythonContext.GetContext(context).GetComparer(null, GetComparisonType(list));
		while (lo < hi)
		{
			int num = (lo + hi) / 2;
			object y = list[num];
			if (comparer.Compare(item, y) < 0)
			{
				hi = num;
			}
			else
			{
				lo = num + 1;
			}
		}
		return lo;
	}

	private static int InternalBisectRight(CodeContext context, object list, object item, int lo, int hi)
	{
		if (lo < 0)
		{
			throw PythonOps.ValueError("lo must be non-negative");
		}
		if (hi == -1)
		{
			hi = PythonOps.Length(list);
		}
		IComparer comparer = PythonContext.GetContext(context).GetComparer(null, GetComparisonType(context, list));
		while (lo < hi)
		{
			int num = (lo + hi) / 2;
			object index = PythonOps.GetIndex(context, list, num);
			if (comparer.Compare(item, index) < 0)
			{
				hi = num;
			}
			else
			{
				lo = num + 1;
			}
		}
		return lo;
	}

	private static Type GetComparisonType(CodeContext context, object a)
	{
		if (PythonOps.Length(a) > 0)
		{
			return CompilerHelpers.GetType(PythonOps.GetIndex(context, a, 0));
		}
		return typeof(object);
	}

	private static Type GetComparisonType(List a)
	{
		if (a.Count > 0)
		{
			return CompilerHelpers.GetType(a[0]);
		}
		return typeof(object);
	}

	[Documentation("bisect_right(a, x[, lo[, hi]]) -> index\r\n\r\nReturn the index where to insert item x in list a, assuming a is sorted.\r\n\r\nThe return value i is such that all e in a[:i] have e <= x, and all e in\r\na[i:] have e > x.  So if x already appears in the list, i points just\r\nbeyond the rightmost x already there\r\n\r\nOptional args lo (default 0) and hi (default len(a)) bound the\r\nslice of a to be searched.\r\n")]
	public static object bisect_right(CodeContext context, List a, object x, [DefaultParameterValue(0)] int lo, [DefaultParameterValue(-1)] int hi)
	{
		return InternalBisectRight(context, a, x, lo, hi);
	}

	public static object bisect_right(CodeContext context, object a, object x, [DefaultParameterValue(0)] int lo, [DefaultParameterValue(-1)] int hi)
	{
		return InternalBisectRight(context, a, x, lo, hi);
	}

	[Documentation("insort_right(a, x[, lo[, hi]])\r\n\r\nInsert item x in list a, and keep it sorted assuming a is sorted.\r\n\r\nIf x is already in a, insert it to the right of the rightmost x.\r\n\r\nOptional args lo (default 0) and hi (default len(a)) bound the\r\nslice of a to be searched.\r\n")]
	public static void insort_right(CodeContext context, List a, object x, [DefaultParameterValue(0)] int lo, [DefaultParameterValue(-1)] int hi)
	{
		a.Insert(InternalBisectRight(context, a, x, lo, hi), x);
	}

	public static void insort_right(CodeContext context, object a, object x, [DefaultParameterValue(0)] int lo, [DefaultParameterValue(-1)] int hi)
	{
		PythonOps.Invoke(context, a, "insert", InternalBisectRight(context, a, x, lo, hi), x);
	}

	[Documentation("bisect_left(a, x[, lo[, hi]]) -> index\r\n\r\nReturn the index where to insert item x in list a, assuming a is sorted.\r\n\r\nThe return value i is such that all e in a[:i] have e < x, and all e in\r\na[i:] have e >= x.  So if x already appears in the list, i points just\r\nbefore the leftmost x already there.\r\n\r\nOptional args lo (default 0) and hi (default len(a)) bound the\r\nslice of a to be searched.\r\n")]
	public static object bisect_left(CodeContext context, List a, object x, [DefaultParameterValue(0)] int lo, [DefaultParameterValue(-1)] int hi)
	{
		return InternalBisectLeft(context, a, x, lo, hi);
	}

	public static object bisect_left(CodeContext context, object a, object x, [DefaultParameterValue(0)] int lo, [DefaultParameterValue(-1)] int hi)
	{
		return InternalBisectLeft(context, a, x, lo, hi);
	}

	[Documentation("insort_left(a, x[, lo[, hi]])\r\n\r\nInsert item x in list a, and keep it sorted assuming a is sorted.\r\n\r\nIf x is already in a, insert it to the left of the leftmost x.\r\n\r\nOptional args lo (default 0) and hi (default len(a)) bound the\r\nslice of a to be searched.\r\n")]
	public static void insort_left(CodeContext context, List a, object x, [DefaultParameterValue(0)] int lo, [DefaultParameterValue(-1)] int hi)
	{
		a.Insert(InternalBisectLeft(context, a, x, lo, hi), x);
	}

	public static void insort_left(CodeContext context, object a, object x, [DefaultParameterValue(0)] int lo, [DefaultParameterValue(-1)] int hi)
	{
		PythonOps.Invoke(context, a, "insert", InternalBisectLeft(context, a, x, lo, hi), x);
	}

	[Documentation("Alias for bisect_right().")]
	public static object bisect(CodeContext context, List a, object x, [DefaultParameterValue(0)] int lo, [DefaultParameterValue(-1)] int hi)
	{
		return bisect_right(context, a, x, lo, hi);
	}

	[Documentation("Alias for bisect_right().")]
	public static object bisect(CodeContext context, object a, object x, [DefaultParameterValue(0)] int lo, [DefaultParameterValue(-1)] int hi)
	{
		return bisect_right(context, a, x, lo, hi);
	}

	[Documentation("Alias for insort_right().")]
	public static void insort(CodeContext context, List a, object x, [DefaultParameterValue(0)] int lo, [DefaultParameterValue(-1)] int hi)
	{
		insort_right(context, a, x, lo, hi);
	}

	[Documentation("Alias for insort_right().")]
	public static void insort(CodeContext context, object a, object x, [DefaultParameterValue(0)] int lo, [DefaultParameterValue(-1)] int hi)
	{
		insort_right(context, a, x, lo, hi);
	}
}

using System;
using System.Collections;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("slice")]
public sealed class Slice : ICodeFormattable, IComparable, ISlice
{
	internal delegate void SliceAssign(int index, object value);

	private readonly object _start;

	private readonly object _stop;

	private readonly object _step;

	public object start => _start;

	public object stop => _stop;

	public object step => _step;

	object ISlice.Start => start;

	object ISlice.Stop => stop;

	object ISlice.Step => step;

	public Slice(object stop)
		: this(null, stop, null)
	{
	}

	public Slice(object start, object stop)
		: this(start, stop, null)
	{
	}

	public Slice(object start, object stop, object step)
	{
		_start = start;
		_stop = stop;
		_step = step;
	}

	public int __cmp__(Slice obj)
	{
		return PythonOps.CompareArrays(new object[3] { _start, _stop, _step }, 3, new object[3] { obj._start, obj._stop, obj._step }, 3);
	}

	public void indices(int len, out int ostart, out int ostop, out int ostep)
	{
		PythonOps.FixSlice(len, _start, _stop, _step, out ostart, out ostop, out ostep);
	}

	public void indices(object len, out int ostart, out int ostop, out int ostep)
	{
		PythonOps.FixSlice(Converter.ConvertToIndex(len), _start, _stop, _step, out ostart, out ostop, out ostep);
	}

	public PythonTuple __reduce__()
	{
		return PythonTuple.MakeTuple(DynamicHelpers.GetPythonTypeFromType(typeof(Slice)), PythonTuple.MakeTuple(_start, _stop, _step));
	}

	int IComparable.CompareTo(object obj)
	{
		if (!(obj is Slice obj2))
		{
			throw new ValueErrorException("expected slice");
		}
		return __cmp__(obj2);
	}

	public int __hash__()
	{
		throw PythonOps.TypeErrorForUnhashableType("slice");
	}

	public string __repr__(CodeContext context)
	{
		return $"slice({PythonOps.Repr(context, _start)}, {PythonOps.Repr(context, _stop)}, {PythonOps.Repr(context, _step)})";
	}

	internal static void FixSliceArguments(int size, ref int start, ref int stop)
	{
		start = ((start >= 0) ? ((start > size) ? size : start) : 0);
		stop = ((stop >= 0) ? ((stop > size) ? size : stop) : 0);
	}

	internal static void FixSliceArguments(long size, ref long start, ref long stop)
	{
		start = ((start < 0) ? 0 : ((start > size) ? size : start));
		stop = ((stop < 0) ? 0 : ((stop > size) ? size : stop));
	}

	internal void DeprecatedFixed(object self, out int newStart, out int newStop)
	{
		bool flag = false;
		int num = 0;
		if (_start != null)
		{
			newStart = Converter.ConvertToIndex(_start);
			if (newStart < 0)
			{
				flag = true;
				num = PythonOps.Length(self);
				newStart += num;
			}
		}
		else
		{
			newStart = 0;
		}
		if (_stop != null)
		{
			newStop = Converter.ConvertToIndex(_stop);
			if (newStop < 0)
			{
				if (!flag)
				{
					num = PythonOps.Length(self);
				}
				newStop += num;
			}
		}
		else
		{
			newStop = int.MaxValue;
		}
	}

	internal void DoSliceAssign(SliceAssign assign, int size, object value)
	{
		indices(size, out var ostart, out var ostop, out var ostep);
		if (_step == null)
		{
			throw PythonOps.ValueError("cannot do slice assignment w/ no step");
		}
		DoSliceAssign(assign, ostart, ostop, ostep, value);
	}

	private static void DoSliceAssign(SliceAssign assign, int start, int stop, int step, object value)
	{
		stop = ((step > 0) ? Math.Max(stop, start) : Math.Min(stop, start));
		int n = Math.Max(0, ((step > 0) ? (stop - start + step - 1) : (stop - start + step + 1)) / step);
		if (value is IList)
		{
			ListSliceAssign(assign, start, n, step, value as IList);
		}
		else
		{
			OtherSliceAssign(assign, start, stop, step, value);
		}
	}

	private static void ListSliceAssign(SliceAssign assign, int start, int n, int step, IList lst)
	{
		if (lst.Count < n)
		{
			throw PythonOps.ValueError("too few items in the enumerator. need {0} have {1}", n, lst.Count);
		}
		if (lst.Count != n)
		{
			throw PythonOps.ValueError("too many items in the enumerator need {0} have {1}", n, lst.Count);
		}
		int num = 0;
		int num2 = start;
		while (num < n)
		{
			assign(num2, lst[num]);
			num++;
			num2 += step;
		}
	}

	private static void OtherSliceAssign(SliceAssign assign, int start, int stop, int step, object value)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(value);
		List list = new List();
		while (enumerator.MoveNext())
		{
			list.AddNoLock(enumerator.Current);
		}
		DoSliceAssign(assign, start, stop, step, list);
	}
}

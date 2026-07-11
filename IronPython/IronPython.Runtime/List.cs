using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

[Serializable]
[DebuggerTypeProxy(typeof(ObjectCollectionDebugProxy))]
[PythonType("list")]
[DebuggerDisplay("list, {Count} items")]
public class List : IList, ICollection, ICodeFormattable, IList<object>, ICollection<object>, IEnumerable<object>, IEnumerable, IReversible, IStructuralEquatable, IStructuralComparable
{
	private const int INITIAL_SIZE = 20;

	public const object __hash__ = null;

	internal int _size;

	internal volatile object[] _data;

	private static readonly object _boxedOne = ScriptingRuntimeHelpers.Int32ToObject(1);

	public virtual object this[Slice slice]
	{
		get
		{
			if (slice == null)
			{
				throw PythonOps.TypeError("list indices must be integer or slice, not None");
			}
			slice.indices(_size, out var ostart, out var ostop, out var ostep);
			if ((ostep > 0 && ostart >= ostop) || (ostep < 0 && ostart <= ostop))
			{
				return new List();
			}
			if (ostep == 1)
			{
				object[] slice2;
				lock (this)
				{
					slice2 = ArrayOps.GetSlice(_data, ostart, ostop);
				}
				return new List(slice2);
			}
			int num = (int)((ostep > 0) ? (((long)ostop - (long)ostart + ostep - 1) / ostep) : (((long)ostop - (long)ostart + ostep + 1) / ostep));
			object[] array = new object[num];
			lock (this)
			{
				int num2 = 0;
				int num3 = 0;
				int num4 = ostart;
				while (num3 < num)
				{
					array[num2++] = _data[num4];
					num3++;
					num4 += ostep;
				}
			}
			return new List(array);
		}
		set
		{
			if (slice == null)
			{
				throw PythonOps.TypeError("list indices must be integer or slice, not None");
			}
			if (slice.step != null && (!(slice.step is int) || !slice.step.Equals(_boxedOne)))
			{
				if (this == value)
				{
					value = new List(value);
				}
				if (ValueRequiresNoLocks(value))
				{
					lock (this)
					{
						slice.DoSliceAssign(SliceAssignNoLock, _size, value);
						return;
					}
				}
				slice.DoSliceAssign(SliceAssign, _size, value);
			}
			else
			{
				slice.indices(_size, out var ostart, out var ostop, out var _);
				if (value is List other)
				{
					SliceNoStep(ostart, ostop, other);
				}
				else
				{
					SliceNoStep(ostart, ostop, value);
				}
			}
		}
	}

	bool IList.IsReadOnly => false;

	public virtual object this[int index]
	{
		get
		{
			object[] data = GetData();
			return data[PythonOps.FixIndex(index, _size)];
		}
		set
		{
			lock (this)
			{
				_data[PythonOps.FixIndex(index, _size)] = value;
			}
		}
	}

	public virtual object this[BigInteger index]
	{
		get
		{
			return this[(int)index];
		}
		set
		{
			this[(int)index] = value;
		}
	}

	public virtual object this[object index]
	{
		get
		{
			return this[Converter.ConvertToIndex(index)];
		}
		set
		{
			this[Converter.ConvertToIndex(index)] = value;
		}
	}

	bool IList.IsFixedSize => false;

	bool ICollection.IsSynchronized => false;

	public int Count
	{
		[PythonHidden]
		get
		{
			return _size;
		}
	}

	object ICollection.SyncRoot => this;

	bool ICollection<object>.IsReadOnly => ((IList)this).IsReadOnly;

	public void __init__()
	{
		_data = new object[8];
		_size = 0;
	}

	public void __init__([NotNull] IEnumerable enumerable)
	{
		__init__();
		foreach (object item in enumerable)
		{
			AddNoLock(item);
		}
	}

	public void __init__([NotNull] ICollection sequence)
	{
		_data = new object[sequence.Count];
		int size = 0;
		foreach (object item in sequence)
		{
			_data[size++] = item;
		}
		_size = size;
	}

	public void __init__([NotNull] SetCollection sequence)
	{
		List items = sequence._items.GetItems();
		_size = items._size;
		_data = items._data;
	}

	public void __init__([NotNull] FrozenSetCollection sequence)
	{
		List items = sequence._items.GetItems();
		_size = items._size;
		_data = items._data;
	}

	public void __init__([NotNull] List sequence)
	{
		if (this == sequence)
		{
			_size = 0;
			return;
		}
		_data = new object[sequence._size];
		object[] data = sequence._data;
		for (int i = 0; i < _data.Length; i++)
		{
			_data[i] = data[i];
		}
		_size = _data.Length;
	}

	public void __init__([NotNull] string sequence)
	{
		_data = new object[sequence.Length];
		_size = sequence.Length;
		for (int i = 0; i < sequence.Length; i++)
		{
			_data[i] = ScriptingRuntimeHelpers.CharToString(sequence[i]);
		}
	}

	public void __init__(CodeContext context, object sequence)
	{
		try
		{
			if (PythonTypeOps.TryInvokeUnaryOperator(context, sequence, "__len__", out var value))
			{
				int num = PythonContext.GetContext(context).ConvertToInt32(value);
				_data = new object[num];
				_size = 0;
				extend(sequence);
			}
			else
			{
				_data = new object[20];
				_size = 0;
				extend(sequence);
			}
		}
		catch (MissingMemberException)
		{
			_data = new object[20];
			_size = 0;
			extend(sequence);
		}
	}

	public static object __new__(CodeContext context, PythonType cls)
	{
		if (cls == TypeCache.List)
		{
			return new List();
		}
		return cls.CreateInstance(context);
	}

	public static object __new__(CodeContext context, PythonType cls, object arg)
	{
		return __new__(context, cls);
	}

	public static object __new__(CodeContext context, PythonType cls, params object[] argsø)
	{
		return __new__(context, cls);
	}

	public static object __new__(CodeContext context, PythonType cls, [ParamDictionary] IDictionary<object, object> kwArgsø, params object[] argsø)
	{
		return __new__(context, cls);
	}

	private List(IEnumerator e)
		: this(10)
	{
		while (e.MoveNext())
		{
			AddNoLock(e.Current);
		}
	}

	internal List(int capacity)
	{
		if (capacity == 0)
		{
			_data = ArrayUtils.EmptyObjects;
		}
		else
		{
			_data = new object[capacity];
		}
	}

	private List(params object[] items)
	{
		_data = items;
		_size = _data.Length;
	}

	public List()
		: this(0)
	{
	}

	internal List(object sequence)
	{
		object value;
		if (sequence is ICollection collection)
		{
			_data = new object[collection.Count];
			int size = 0;
			foreach (object item in collection)
			{
				_data[size++] = item;
			}
			_size = size;
		}
		else if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, sequence, "__len__", out value))
		{
			int num = Converter.ConvertToInt32(value);
			_data = new object[num];
			extend(sequence);
		}
		else
		{
			_data = new object[20];
			extend(sequence);
		}
	}

	internal List(ICollection items)
		: this(items.Count)
	{
		int size = 0;
		foreach (object item in items)
		{
			_data[size++] = item;
		}
		_size = size;
	}

	internal static List FromArrayNoCopy(params object[] data)
	{
		return new List(data);
	}

	internal object[] GetObjectArray()
	{
		lock (this)
		{
			return ArrayOps.CopyArray(_data, _size);
		}
	}

	public static List operator +([NotNull] List l1, [NotNull] List l2)
	{
		object[] array;
		int size;
		lock (l1)
		{
			array = ArrayOps.CopyArray(l1._data, GetAddSize(l1._size, l2._size));
			size = l1._size;
		}
		lock (l2)
		{
			if (l2._size + size > array.Length)
			{
				array = ArrayOps.CopyArray(array, GetAddSize(size, l2._size));
			}
			Array.Copy(l2._data, 0, array, size, l2._size);
			List list = new List(array);
			list._size = size + l2._size;
			return list;
		}
	}

	private static int GetAddSize(int s1, int s2)
	{
		int length = s1 + s2;
		return GetNewSize(length);
	}

	private static int GetNewSize(int length)
	{
		if (length > 256)
		{
			return (length + 127) & -128;
		}
		return (length + 15) & -16;
	}

	public static List operator *([NotNull] List l, int count)
	{
		return MultiplyWorker(l, count);
	}

	public static List operator *(int count, List l)
	{
		return MultiplyWorker(l, count);
	}

	public static object operator *([NotNull] List self, [NotNull] Index count)
	{
		return PythonOps.MultiplySequence(MultiplyWorker, self, count, isForward: true);
	}

	public static object operator *([NotNull] Index count, [NotNull] List self)
	{
		return PythonOps.MultiplySequence(MultiplyWorker, self, count, isForward: false);
	}

	public static object operator *([NotNull] List self, object count)
	{
		if (Converter.TryConvertToIndex(count, out int num))
		{
			return self * num;
		}
		throw PythonOps.TypeErrorForUnIndexableObject(count);
	}

	public static object operator *(object count, [NotNull] List self)
	{
		if (Converter.TryConvertToIndex(count, out int num))
		{
			return num * self;
		}
		throw PythonOps.TypeErrorForUnIndexableObject(count);
	}

	private static List MultiplyWorker(List self, int count)
	{
		if (count <= 0)
		{
			return PythonOps.MakeEmptyList(0);
		}
		int size;
		int num;
		object[] array;
		lock (self)
		{
			size = self._size;
			num = checked(size * count);
			array = ArrayOps.CopyArray(self._data, num);
		}
		int num2 = size;
		int num3 = size;
		while (num3 < num)
		{
			Array.Copy(array, 0, array, num3, Math.Min(num2, num - num3));
			num3 += num2;
			num2 *= 2;
		}
		return new List(array);
	}

	public virtual int __len__()
	{
		return _size;
	}

	public virtual IEnumerator __iter__()
	{
		return new ListIterator(this);
	}

	public virtual IEnumerator __reversed__()
	{
		return new ListReverseIterator(this);
	}

	public virtual bool __contains__(object value)
	{
		return ContainsWorker(value);
	}

	internal bool ContainsWorker(object value)
	{
		bool lockTaken = false;
		try
		{
			MonitorUtils.Enter(this, ref lockTaken);
			for (int i = 0; i < _size; i++)
			{
				object x = _data[i];
				MonitorUtils.Exit(this, ref lockTaken);
				try
				{
					if (PythonOps.EqualRetBool(x, value))
					{
						return true;
					}
				}
				finally
				{
					MonitorUtils.Enter(this, ref lockTaken);
				}
			}
		}
		finally
		{
			if (lockTaken)
			{
				Monitor.Exit(this);
			}
		}
		return false;
	}

	internal void AddRange<T>(ICollection<T> otherList)
	{
		foreach (T other in otherList)
		{
			object item = other;
			append(item);
		}
	}

	[SpecialName]
	public virtual object InPlaceAdd(object other)
	{
		if (!object.ReferenceEquals(this, other))
		{
			IEnumerator enumerator = PythonOps.GetEnumerator(other);
			while (enumerator.MoveNext())
			{
				append(enumerator.Current);
			}
		}
		else
		{
			InPlaceMultiply(2);
		}
		return this;
	}

	[SpecialName]
	public List InPlaceMultiply(int count)
	{
		lock (this)
		{
			int size = _size;
			int num = checked(size * count);
			EnsureSize(num);
			int num2 = size;
			int num3 = size;
			while (num3 < num)
			{
				Array.Copy(_data, 0, _data, num3, Math.Min(num2, num - num3));
				num3 += num2;
				num2 *= 2;
			}
			_size = num;
			return this;
		}
	}

	[SpecialName]
	public object InPlaceMultiply(Index count)
	{
		return PythonOps.MultiplySequence(InPlaceMultiplyWorker, this, count, isForward: true);
	}

	[SpecialName]
	public object InPlaceMultiply(object count)
	{
		if (Converter.TryConvertToIndex(count, out int num))
		{
			return InPlaceMultiply(num);
		}
		throw PythonOps.TypeErrorForUnIndexableObject(count);
	}

	private static List InPlaceMultiplyWorker(List self, int count)
	{
		return self.InPlaceMultiply(count);
	}

	public virtual object __getslice__(int start, int stop)
	{
		lock (this)
		{
			Slice.FixSliceArguments(_size, ref start, ref stop);
			object[] slice = ArrayOps.GetSlice(_data, start, stop);
			return new List(slice);
		}
	}

	internal object[] GetSliceAsArray(int start, int stop)
	{
		if (start < 0)
		{
			start = 0;
		}
		if (stop > Count)
		{
			stop = Count;
		}
		lock (this)
		{
			return ArrayOps.GetSlice(_data, start, stop);
		}
	}

	public virtual void __setslice__(int start, int stop, object value)
	{
		Slice.FixSliceArguments(_size, ref start, ref stop);
		if (value is List)
		{
			SliceNoStep(start, stop, (List)value);
		}
		else
		{
			SliceNoStep(start, stop, value);
		}
	}

	public virtual void __delslice__(int start, int stop)
	{
		lock (this)
		{
			Slice.FixSliceArguments(_size, ref start, ref stop);
			if (start <= stop)
			{
				int num = start;
				int num2 = stop;
				while (num2 < _size)
				{
					_data[num] = _data[num2];
					num2++;
					num++;
				}
				_size -= stop - start;
			}
		}
	}

	private static bool ValueRequiresNoLocks(object value)
	{
		if (!(value is PythonTuple) && !(value is Array))
		{
			return value is FrozenSetCollection;
		}
		return true;
	}

	private void SliceNoStep(int start, int stop, List other)
	{
		int size = other._size;
		object[] data = other._data;
		lock (this)
		{
			if (stop - start == size)
			{
				for (int i = 0; i < size; i++)
				{
					_data[i + start] = data[i];
				}
				return;
			}
			stop = Math.Max(stop, start);
			int num = _size - (stop - start) + size;
			object[] array = new object[GetNewSize(num)];
			for (int j = 0; j < start; j++)
			{
				array[j] = _data[j];
			}
			for (int k = 0; k < size; k++)
			{
				array[k + start] = data[k];
			}
			int num2 = size - (stop - start);
			for (int l = stop; l < _size; l++)
			{
				array[l + num2] = _data[l];
			}
			_size = num;
			_data = array;
		}
	}

	private void SliceNoStep(int start, int stop, object value)
	{
		IList<object> list = (value as IList<object>) ?? new List(PythonOps.GetEnumerator(value));
		lock (this)
		{
			if (stop - start == list.Count)
			{
				for (int i = 0; i < list.Count; i++)
				{
					_data[i + start] = list[i];
				}
				return;
			}
			stop = Math.Max(stop, start);
			int num = _size - (stop - start) + list.Count;
			object[] array = new object[GetNewSize(num)];
			for (int j = 0; j < start; j++)
			{
				array[j] = _data[j];
			}
			for (int k = 0; k < list.Count; k++)
			{
				array[k + start] = list[k];
			}
			int num2 = list.Count - (stop - start);
			for (int l = stop; l < _size; l++)
			{
				array[l + num2] = _data[l];
			}
			_size = num;
			_data = array;
		}
	}

	private void SliceAssign(int index, object value)
	{
		this[index] = value;
	}

	private void SliceAssignNoLock(int index, object value)
	{
		_data[index] = value;
	}

	public virtual void __delitem__(int index)
	{
		lock (this)
		{
			RawDelete(PythonOps.FixIndex(index, _size));
		}
	}

	public virtual void __delitem__(object index)
	{
		__delitem__(Converter.ConvertToIndex(index));
	}

	public void __delitem__(Slice slice)
	{
		if (slice == null)
		{
			throw PythonOps.TypeError("list indices must be integers or slices");
		}
		lock (this)
		{
			slice.indices(_size, out var ostart, out var ostop, out var ostep);
			if ((ostep > 0 && ostart >= ostop) || (ostep < 0 && ostart <= ostop))
			{
				return;
			}
			if (ostep == 1)
			{
				int num = ostart;
				int num2 = ostop;
				while (num2 < _size)
				{
					_data[num] = _data[num2];
					num2++;
					num++;
				}
				_size -= ostop - ostart;
				return;
			}
			if (ostep == -1)
			{
				int num3 = ostop + 1;
				int num4 = ostart + 1;
				while (num4 < _size)
				{
					_data[num3] = _data[num4];
					num4++;
					num3++;
				}
				_size -= ostart - ostop;
				return;
			}
			if (ostep < 0)
			{
				int i;
				for (i = ostart; i > ostop; i += ostep)
				{
				}
				i -= ostep;
				ostop = ostart + 1;
				ostart = i;
				ostep = -ostep;
			}
			int num6;
			int num7;
			int num5 = (num6 = (num7 = ostart));
			while (num5 < ostop && num7 < ostop)
			{
				if (num7 != num6)
				{
					_data[num5++] = _data[num7];
				}
				else
				{
					num6 += ostep;
				}
				num7++;
			}
			while (ostop < _size)
			{
				_data[num5++] = _data[ostop++];
			}
			_size = num5;
		}
	}

	private void RawDelete(int index)
	{
		int num = --_size;
		object[] data = _data;
		for (int i = index; i < num; i++)
		{
			data[i] = data[i + 1];
		}
		data[num] = null;
	}

	internal void EnsureSize(int needed)
	{
		if (_data.Length >= needed)
		{
			return;
		}
		if (_data.Length == 0)
		{
			_data = new object[4];
			return;
		}
		int num;
		for (num = Math.Max(_size * 3, 10); num < needed; num *= 2)
		{
		}
		_data = ArrayOps.CopyArray(_data, num);
	}

	public void append(object item)
	{
		lock (this)
		{
			AddNoLock(item);
		}
	}

	internal void AddNoLock(object item)
	{
		EnsureSize(_size + 1);
		_data[_size] = item;
		_size++;
	}

	internal void AddNoLockNoDups(object item)
	{
		for (int i = 0; i < _size; i++)
		{
			if (PythonOps.EqualRetBool(_data[i], item))
			{
				return;
			}
		}
		AddNoLock(item);
	}

	internal void AppendListNoLockNoDups(List list)
	{
		if (list == null)
		{
			return;
		}
		foreach (object item in list)
		{
			AddNoLockNoDups(item);
		}
	}

	public int count(object item)
	{
		bool lockTaken = false;
		try
		{
			MonitorUtils.Enter(this, ref lockTaken);
			int num = 0;
			int i = 0;
			for (int size = _size; i < size; i++)
			{
				object x = _data[i];
				MonitorUtils.Exit(this, ref lockTaken);
				try
				{
					if (PythonOps.EqualRetBool(x, item))
					{
						num++;
					}
				}
				finally
				{
					MonitorUtils.Enter(this, ref lockTaken);
				}
			}
			return num;
		}
		finally
		{
			if (lockTaken)
			{
				Monitor.Exit(this);
			}
		}
	}

	public void extend([NotNull] List seq)
	{
		using (new OrderedLocker(this, seq))
		{
			int num = seq.Count;
			EnsureSize(Count + num);
			for (int i = 0; i < num; i++)
			{
				AddNoLock(seq[i]);
			}
		}
	}

	public void extend([NotNull] PythonTuple seq)
	{
		lock (this)
		{
			EnsureSize(Count + seq.Count);
			for (int i = 0; i < seq.Count; i++)
			{
				AddNoLock(seq[i]);
			}
		}
	}

	public void extend(object seq)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(seq);
		if (seq == this)
		{
			List list = new List(enumerator);
			enumerator = ((IEnumerable)list).GetEnumerator();
		}
		while (enumerator.MoveNext())
		{
			append(enumerator.Current);
		}
	}

	public int index(object item)
	{
		return index(item, 0, _size);
	}

	public int index(object item, int start)
	{
		return index(item, start, _size);
	}

	public int index(object item, int start, int stop)
	{
		object[] data;
		int size;
		lock (this)
		{
			data = _data;
			size = _size;
		}
		start = PythonOps.FixSliceIndex(start, size);
		stop = PythonOps.FixSliceIndex(stop, size);
		for (int i = start; i < Math.Min(stop, Math.Min(size, _size)); i++)
		{
			if (PythonOps.EqualRetBool(data[i], item))
			{
				return i;
			}
		}
		throw PythonOps.ValueError("list.index(item): item not in list");
	}

	public int index(object item, object start)
	{
		return index(item, Converter.ConvertToIndex(start), _size);
	}

	public int index(object item, object start, object stop)
	{
		return index(item, Converter.ConvertToIndex(start), Converter.ConvertToIndex(stop));
	}

	public void insert(int index, object value)
	{
		if (index >= _size)
		{
			append(value);
			return;
		}
		lock (this)
		{
			index = PythonOps.FixSliceIndex(index, _size);
			EnsureSize(_size + 1);
			_size++;
			for (int num = _size - 1; num > index; num--)
			{
				_data[num] = _data[num - 1];
			}
			_data[index] = value;
		}
	}

	[PythonHidden]
	public void Insert(int index, object value)
	{
		insert(index, value);
	}

	public object pop()
	{
		if (_size == 0)
		{
			throw PythonOps.IndexError("pop off of empty list");
		}
		lock (this)
		{
			_size--;
			return _data[_size];
		}
	}

	public object pop(int index)
	{
		lock (this)
		{
			index = PythonOps.FixIndex(index, _size);
			if (_size == 0)
			{
				throw PythonOps.IndexError("pop off of empty list");
			}
			object result = _data[index];
			_size--;
			for (int i = index; i < _size; i++)
			{
				_data[i] = _data[i + 1];
			}
			return result;
		}
	}

	public void remove(object value)
	{
		lock (this)
		{
			RawDelete(index(value));
		}
	}

	void IList.Remove(object value)
	{
		remove(value);
	}

	public void reverse()
	{
		lock (this)
		{
			Array.Reverse(_data, 0, _size);
		}
	}

	internal void reverse(int index, int count)
	{
		lock (this)
		{
			Array.Reverse(_data, index, count);
		}
	}

	public void sort(CodeContext context)
	{
		sort(context, null, null, reverse: false);
	}

	public void sort(CodeContext context, object cmp)
	{
		sort(context, cmp, null, reverse: false);
	}

	public void sort(CodeContext context, object cmp, object key)
	{
		sort(context, cmp, key, reverse: false);
	}

	public void sort(CodeContext context, [DefaultParameterValue(null)] object cmp, [DefaultParameterValue(null)] object key, [DefaultParameterValue(false)] bool reverse)
	{
		if (_size != 0)
		{
			IComparer comparer = PythonContext.GetContext(context).GetComparer(cmp, GetComparisonType());
			DoSort(context, comparer, key, reverse, 0, _size);
		}
	}

	private Type GetComparisonType()
	{
		if (_size >= 4000)
		{
			return null;
		}
		if (_data.Length > 0)
		{
			return CompilerHelpers.GetType(_data[0]);
		}
		return typeof(object);
	}

	internal void DoSort(CodeContext context, IComparer cmp, object key, bool reverse, int index, int count)
	{
		lock (this)
		{
			object[] array = _data;
			int size = _size;
			try
			{
				_data = ArrayUtils.EmptyObjects;
				_size = 0;
				if (key != null)
				{
					object[] array2 = new object[size];
					for (int i = 0; i < size; i++)
					{
						array2[i] = PythonCalls.Call(context, key, array[i]);
						if (_data.Length != 0)
						{
							throw PythonOps.ValueError("list mutated while determing keys");
						}
					}
					array = ListMergeSort(array, array2, cmp, index, count, reverse);
				}
				else
				{
					array = ListMergeSort(array, cmp, index, count, reverse);
				}
			}
			finally
			{
				_data = array;
				_size = size;
			}
		}
	}

	internal object[] ListMergeSort(object[] sortData, IComparer cmp, int index, int count, bool reverse)
	{
		return ListMergeSort(sortData, null, cmp, index, count, reverse);
	}

	internal object[] ListMergeSort(object[] sortData, object[] keys, IComparer cmp, int index, int count, bool reverse)
	{
		if (count - index < 2)
		{
			return sortData;
		}
		if (keys == null)
		{
			keys = sortData;
		}
		int num = count - index;
		int[] array = new int[num + 2];
		array[0] = 1;
		array[num + 1] = 2;
		for (int i = 1; i <= num - 2; i++)
		{
			array[i] = -(i + 2);
		}
		array[num - 1] = (array[num] = 0);
		while (true)
		{
			int num2 = 0;
			int num3 = num + 1;
			int num4 = array[num2];
			int num5 = array[num3];
			if (num5 == 0)
			{
				break;
			}
			do
			{
				IL_0065:
				if (num4 < 1 || (num5 <= num && DoCompare(keys, cmp, num4 + index - 1, num5 + index - 1, reverse)))
				{
					if (array[num2] < 0)
					{
						array[num2] = Math.Abs(num4) * -1;
					}
					else
					{
						array[num2] = Math.Abs(num4);
					}
					num2 = num4;
					num4 = array[num4];
					if (num4 > 0)
					{
						goto IL_0065;
					}
					array[num2] = num5;
					num2 = num3;
					do
					{
						num3 = num5;
						num5 = array[num5];
					}
					while (num5 > 0);
				}
				else
				{
					if (array[num2] < 0)
					{
						array[num2] = Math.Abs(num5) * -1;
					}
					else
					{
						array[num2] = Math.Abs(num5);
					}
					num2 = num5;
					num5 = array[num5];
					if (num5 > 0)
					{
						goto IL_0065;
					}
					array[num2] = num4;
					num2 = num3;
					do
					{
						num3 = num4;
						num4 = array[num4];
					}
					while (num4 > 0);
				}
				num4 *= -1;
				num5 *= -1;
			}
			while (num5 != 0);
			if (array[num2] < 0)
			{
				array[num2] = Math.Abs(num4) * -1;
			}
			else
			{
				array[num2] = Math.Abs(num4);
			}
			array[num3] = 0;
		}
		object[] array2 = new object[num];
		int num6 = array[0];
		int num7 = 0;
		while (num6 != 0)
		{
			array2[num7++] = sortData[num6 + index - 1];
			num6 = array[num6];
		}
		if (sortData.Length != count || index != 0)
		{
			for (int j = 0; j < count; j++)
			{
				sortData[j + index] = array2[j];
			}
		}
		else
		{
			sortData = array2;
		}
		return sortData;
	}

	private bool DoCompare(object[] keys, IComparer cmp, int p, int q, bool reverse)
	{
		int num = cmp.Compare(keys[p], keys[q]);
		bool result = (reverse ? (num >= 0) : (num <= 0));
		if (_data.Length != 0)
		{
			throw PythonOps.ValueError("list mutated during sort");
		}
		return result;
	}

	internal int BinarySearch(int index, int count, object value, IComparer comparer)
	{
		lock (this)
		{
			return Array.BinarySearch(_data, index, count, value, comparer);
		}
	}

	internal bool EqualsWorker(List l, IEqualityComparer comparer)
	{
		using (new OrderedLocker(this, l))
		{
			if (comparer == null)
			{
				return PythonOps.ArraysEqual(_data, _size, l._data, l._size);
			}
			return PythonOps.ArraysEqual(_data, _size, l._data, l._size, comparer);
		}
	}

	internal int CompareToWorker(List l)
	{
		return CompareToWorker(l, null);
	}

	internal int CompareToWorker(List l, IComparer comparer)
	{
		using (new OrderedLocker(this, l))
		{
			if (comparer == null)
			{
				return PythonOps.CompareArrays(_data, _size, l._data, l._size);
			}
			return PythonOps.CompareArrays(_data, _size, l._data, l._size, comparer);
		}
	}

	internal bool FastSwap(int i, int j)
	{
		if (i > j)
		{
			int num = i;
			i = j;
			j = num;
		}
		if (i < 0 || j >= _size)
		{
			return false;
		}
		if (i == j)
		{
			return true;
		}
		object obj = _data[i];
		_data[i] = _data[j];
		_data[j] = obj;
		return true;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private object[] GetData()
	{
		return _data;
	}

	[PythonHidden]
	public void RemoveAt(int index)
	{
		lock (this)
		{
			RawDelete(index);
		}
	}

	[PythonHidden]
	public bool Contains(object value)
	{
		return __contains__(value);
	}

	[PythonHidden]
	public void Clear()
	{
		lock (this)
		{
			_size = 0;
		}
	}

	[PythonHidden]
	public int IndexOf(object value)
	{
		object[] data;
		int size;
		lock (this)
		{
			data = _data;
			size = _size;
		}
		for (int i = 0; i < Math.Min(size, _size); i++)
		{
			if (PythonOps.EqualRetBool(data[i], value))
			{
				return i;
			}
		}
		return -1;
	}

	[PythonHidden]
	public int Add(object value)
	{
		lock (this)
		{
			AddNoLock(value);
			return _size - 1;
		}
	}

	[PythonHidden]
	public void CopyTo(Array array, int index)
	{
		Array.Copy(_data, 0, array, index, _size);
	}

	internal void CopyTo(Array array, int index, int arrayIndex, int count)
	{
		Array.Copy(_data, index, array, arrayIndex, count);
	}

	[PythonHidden]
	public IEnumerator GetEnumerator()
	{
		return __iter__();
	}

	public virtual string __repr__(CodeContext context)
	{
		List<object> andCheckInfinite = PythonOps.GetAndCheckInfinite(this);
		if (andCheckInfinite == null)
		{
			return "[...]";
		}
		int num = andCheckInfinite.Count;
		andCheckInfinite.Add(this);
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[");
			for (int i = 0; i < _size; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(PythonOps.Repr(context, _data[i]));
			}
			stringBuilder.Append("]");
			return stringBuilder.ToString();
		}
		finally
		{
			andCheckInfinite.RemoveAt(num);
		}
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		if (CompareUtil.Check(this))
		{
			return 0;
		}
		CompareUtil.Push(this);
		try
		{
			return ((IStructuralEquatable)new PythonTuple(this)).GetHashCode(comparer);
		}
		finally
		{
			CompareUtil.Pop(this);
		}
	}

	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (object.ReferenceEquals(this, other))
		{
			return true;
		}
		if (!(other is List list) || list.Count != Count)
		{
			return false;
		}
		return Equals(list, comparer);
	}

	void ICollection<object>.Add(object item)
	{
		append(item);
	}

	public void CopyTo(object[] array, int arrayIndex)
	{
		for (int i = 0; i < Count; i++)
		{
			array[arrayIndex + i] = this[i];
		}
	}

	[PythonHidden]
	public bool Remove(object item)
	{
		if (__contains__(item))
		{
			remove(item);
			return true;
		}
		return false;
	}

	IEnumerator<object> IEnumerable<object>.GetEnumerator()
	{
		return new IEnumeratorOfTWrapper<object>(((IEnumerable)this).GetEnumerator());
	}

	private bool Equals(List other)
	{
		return Equals(other, null);
	}

	private bool Equals(List other, IEqualityComparer comparer)
	{
		CompareUtil.Push(this, other);
		try
		{
			return EqualsWorker(other, comparer);
		}
		finally
		{
			CompareUtil.Pop(this, other);
		}
	}

	internal int CompareTo(List other)
	{
		return CompareTo(other, null);
	}

	internal int CompareTo(List other, IComparer comparer)
	{
		CompareUtil.Push(this, other);
		try
		{
			return CompareToWorker(other, comparer);
		}
		finally
		{
			CompareUtil.Pop(this, other);
		}
	}

	[return: MaybeNotImplemented]
	public static object operator >(List self, object other)
	{
		if (!(other is List other2))
		{
			return NotImplementedType.Value;
		}
		if (self.CompareTo(other2) <= 0)
		{
			return ScriptingRuntimeHelpers.False;
		}
		return ScriptingRuntimeHelpers.True;
	}

	[return: MaybeNotImplemented]
	public static object operator <(List self, object other)
	{
		if (!(other is List other2))
		{
			return NotImplementedType.Value;
		}
		if (self.CompareTo(other2) >= 0)
		{
			return ScriptingRuntimeHelpers.False;
		}
		return ScriptingRuntimeHelpers.True;
	}

	[return: MaybeNotImplemented]
	public static object operator >=(List self, object other)
	{
		if (!(other is List other2))
		{
			return NotImplementedType.Value;
		}
		if (self.CompareTo(other2) < 0)
		{
			return ScriptingRuntimeHelpers.False;
		}
		return ScriptingRuntimeHelpers.True;
	}

	[return: MaybeNotImplemented]
	public static object operator <=(List self, object other)
	{
		if (!(other is List other2))
		{
			return NotImplementedType.Value;
		}
		if (self.CompareTo(other2) > 0)
		{
			return ScriptingRuntimeHelpers.False;
		}
		return ScriptingRuntimeHelpers.True;
	}

	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (!(other is List other2))
		{
			throw new ValueErrorException("expected List");
		}
		return CompareTo(other2, comparer);
	}
}

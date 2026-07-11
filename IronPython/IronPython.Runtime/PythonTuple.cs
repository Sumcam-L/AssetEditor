using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Compiler.Ast;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

[Serializable]
[PythonType("tuple")]
[DebuggerTypeProxy(typeof(CollectionDebugProxy))]
[DebuggerDisplay("tuple, {Count} items")]
public class PythonTuple : IList, ICollection, IList<object>, ICollection<object>, IEnumerable<object>, IEnumerable, ICodeFormattable, IExpressionSerializable, IStructuralEquatable, IStructuralComparable
{
	internal readonly object[] _data;

	internal static readonly PythonTuple EMPTY = new PythonTuple();

	public virtual object this[int index] => _data[PythonOps.FixIndex(index, _data.Length)];

	public virtual object this[object index] => this[Converter.ConvertToIndex(index)];

	public virtual object this[BigInteger index] => this[(int)index];

	public virtual object this[Slice slice]
	{
		get
		{
			slice.indices(_data.Length, out var ostart, out var ostop, out var ostep);
			if (ostart == 0 && ostop == _data.Length && ostep == 1 && GetType() == typeof(PythonTuple))
			{
				return this;
			}
			return MakeTuple(ArrayOps.GetSlice(_data, ostart, ostop, ostep));
		}
	}

	bool ICollection.IsSynchronized => false;

	public int Count
	{
		[PythonHidden]
		get
		{
			return _data.Length;
		}
	}

	object ICollection.SyncRoot => this;

	object IList<object>.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			throw new InvalidOperationException("Tuple is readonly");
		}
	}

	bool ICollection<object>.IsReadOnly => true;

	bool IList.IsFixedSize => true;

	bool IList.IsReadOnly => true;

	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			throw new InvalidOperationException("Tuple is readonly");
		}
	}

	public PythonTuple(object o)
	{
		_data = MakeItems(o);
	}

	protected PythonTuple(object[] items)
	{
		_data = items;
	}

	public PythonTuple()
	{
		_data = ArrayUtils.EmptyObjects;
	}

	internal PythonTuple(PythonTuple other, object o)
	{
		_data = other.Expand(o);
	}

	public static PythonTuple __new__(CodeContext context, PythonType cls)
	{
		if (cls == TypeCache.PythonTuple)
		{
			return EMPTY;
		}
		if (!(cls.CreateInstance(context) is PythonTuple result))
		{
			throw PythonOps.TypeError("{0} is not a subclass of tuple", cls);
		}
		return result;
	}

	public static PythonTuple __new__(CodeContext context, PythonType cls, object sequence)
	{
		if (sequence == null)
		{
			throw PythonOps.TypeError("iteration over a non-sequence");
		}
		if (cls == TypeCache.PythonTuple)
		{
			if (sequence.GetType() == typeof(PythonTuple))
			{
				return (PythonTuple)sequence;
			}
			return new PythonTuple(MakeItems(sequence));
		}
		if (!(cls.CreateInstance(context, sequence) is PythonTuple result))
		{
			throw PythonOps.TypeError("{0} is not a subclass of tuple", cls);
		}
		return result;
	}

	public int index(object obj, object start)
	{
		return index(obj, Converter.ConvertToIndex(start), _data.Length);
	}

	public int index(object obj, [DefaultParameterValue(0)] int start)
	{
		return index(obj, start, _data.Length);
	}

	public int index(object obj, object start, object end)
	{
		return index(obj, Converter.ConvertToIndex(start), Converter.ConvertToIndex(end));
	}

	public int index(object obj, int start, int end)
	{
		start = PythonOps.FixSliceIndex(start, _data.Length);
		end = PythonOps.FixSliceIndex(end, _data.Length);
		for (int i = start; i < end; i++)
		{
			if (PythonOps.EqualRetBool(obj, _data[i]))
			{
				return i;
			}
		}
		throw PythonOps.ValueError("tuple.index(x): x not in list");
	}

	public int count(object obj)
	{
		int num = 0;
		object[] data = _data;
		foreach (object y in data)
		{
			if (PythonOps.EqualRetBool(obj, y))
			{
				num++;
			}
		}
		return num;
	}

	internal static PythonTuple Make(object o)
	{
		if (o is PythonTuple)
		{
			return (PythonTuple)o;
		}
		return new PythonTuple(MakeItems(o));
	}

	internal static PythonTuple MakeTuple(params object[] items)
	{
		if (items.Length == 0)
		{
			return EMPTY;
		}
		return new PythonTuple(items);
	}

	private static object[] MakeItems(object o)
	{
		if (o is PythonTuple)
		{
			return ((PythonTuple)o)._data;
		}
		if (o is string)
		{
			string text = (string)o;
			object[] array = new object[text.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ScriptingRuntimeHelpers.CharToString(text[i]);
			}
			return array;
		}
		if (o is List)
		{
			return ((List)o).GetObjectArray();
		}
		if (o is object[] array2)
		{
			return ArrayOps.CopyArray(array2, array2.Length);
		}
		List<object> list = new List<object>();
		IEnumerator enumerator = PythonOps.GetEnumerator(o);
		while (enumerator.MoveNext())
		{
			list.Add(enumerator.Current);
		}
		return list.ToArray();
	}

	internal object[] ToArray()
	{
		return ArrayOps.CopyArray(_data, _data.Length);
	}

	public virtual int __len__()
	{
		return _data.Length;
	}

	public virtual object __getslice__(int start, int stop)
	{
		Slice.FixSliceArguments(_data.Length, ref start, ref stop);
		if (start == 0 && stop == _data.Length && GetType() == typeof(PythonTuple))
		{
			return this;
		}
		return MakeTuple(ArrayOps.GetSlice(_data, start, stop));
	}

	public static PythonTuple operator +([NotNull] PythonTuple x, [NotNull] PythonTuple y)
	{
		return MakeTuple(ArrayOps.Add(x._data, x._data.Length, y._data, y._data.Length));
	}

	private static PythonTuple MultiplyWorker(PythonTuple self, int count)
	{
		if (count <= 0)
		{
			return EMPTY;
		}
		if (count == 1 && self.GetType() == typeof(PythonTuple))
		{
			return self;
		}
		return MakeTuple(ArrayOps.Multiply(self._data, self._data.Length, count));
	}

	public static PythonTuple operator *(PythonTuple x, int n)
	{
		return MultiplyWorker(x, n);
	}

	public static PythonTuple operator *(int n, PythonTuple x)
	{
		return MultiplyWorker(x, n);
	}

	public static object operator *([NotNull] PythonTuple self, [NotNull] Index count)
	{
		return PythonOps.MultiplySequence(MultiplyWorker, self, count, isForward: true);
	}

	public static object operator *([NotNull] Index count, [NotNull] PythonTuple self)
	{
		return PythonOps.MultiplySequence(MultiplyWorker, self, count, isForward: false);
	}

	public static object operator *([NotNull] PythonTuple self, object count)
	{
		if (Converter.TryConvertToIndex(count, out int num))
		{
			return self * num;
		}
		throw PythonOps.TypeErrorForUnIndexableObject(count);
	}

	public static object operator *(object count, [NotNull] PythonTuple self)
	{
		if (Converter.TryConvertToIndex(count, out int num))
		{
			return num * self;
		}
		throw PythonOps.TypeErrorForUnIndexableObject(count);
	}

	[PythonHidden]
	public void CopyTo(Array array, int index)
	{
		Array.Copy(_data, 0, array, index, _data.Length);
	}

	public virtual IEnumerator __iter__()
	{
		return new TupleEnumerator(this);
	}

	[PythonHidden]
	public IEnumerator GetEnumerator()
	{
		return __iter__();
	}

	private object[] Expand(object value)
	{
		int num = _data.Length;
		object[] array = ((value != null) ? new object[num + 1] : new object[num]);
		for (int i = 0; i < num; i++)
		{
			array[i] = _data[i];
		}
		if (value != null)
		{
			array[num] = value;
		}
		return array;
	}

	public object __getnewargs__()
	{
		return MakeTuple(new PythonTuple(this));
	}

	IEnumerator<object> IEnumerable<object>.GetEnumerator()
	{
		return new TupleEnumerator(this);
	}

	[PythonHidden]
	public int IndexOf(object item)
	{
		for (int i = 0; i < Count; i++)
		{
			if (PythonOps.EqualRetBool(this[i], item))
			{
				return i;
			}
		}
		return -1;
	}

	void IList<object>.Insert(int index, object item)
	{
		throw new InvalidOperationException("Tuple is readonly");
	}

	void IList<object>.RemoveAt(int index)
	{
		throw new InvalidOperationException("Tuple is readonly");
	}

	void ICollection<object>.Add(object item)
	{
		throw new InvalidOperationException("Tuple is readonly");
	}

	void ICollection<object>.Clear()
	{
		throw new InvalidOperationException("Tuple is readonly");
	}

	[PythonHidden]
	public bool Contains(object item)
	{
		for (int i = 0; i < _data.Length; i++)
		{
			if (PythonOps.EqualRetBool(_data[i], item))
			{
				return true;
			}
		}
		return false;
	}

	[PythonHidden]
	public void CopyTo(object[] array, int arrayIndex)
	{
		for (int i = 0; i < Count; i++)
		{
			array[arrayIndex + i] = this[i];
		}
	}

	bool ICollection<object>.Remove(object item)
	{
		throw new InvalidOperationException("Tuple is readonly");
	}

	internal int CompareTo(PythonTuple other)
	{
		return PythonOps.CompareArrays(_data, _data.Length, other._data, other._data.Length);
	}

	public static bool operator >([NotNull] PythonTuple self, [NotNull] PythonTuple other)
	{
		return self.CompareTo(other) > 0;
	}

	public static bool operator <([NotNull] PythonTuple self, [NotNull] PythonTuple other)
	{
		return self.CompareTo(other) < 0;
	}

	public static bool operator >=([NotNull] PythonTuple self, [NotNull] PythonTuple other)
	{
		return self.CompareTo(other) >= 0;
	}

	public static bool operator <=([NotNull] PythonTuple self, [NotNull] PythonTuple other)
	{
		return self.CompareTo(other) <= 0;
	}

	int IStructuralComparable.CompareTo(object obj, IComparer comparer)
	{
		if (!(obj is PythonTuple pythonTuple))
		{
			throw new ValueErrorException("expected tuple");
		}
		return PythonOps.CompareArrays(_data, _data.Length, pythonTuple._data, pythonTuple._data.Length, comparer);
	}

	public override bool Equals(object obj)
	{
		if (!object.ReferenceEquals(this, obj))
		{
			if (!(obj is PythonTuple pythonTuple) || _data.Length != pythonTuple._data.Length)
			{
				return false;
			}
			for (int i = 0; i < _data.Length; i++)
			{
				object obj2 = this[i];
				object obj3 = pythonTuple[i];
				if (!object.ReferenceEquals(obj2, obj3))
				{
					if (obj2 == null)
					{
						return false;
					}
					if (!obj2.Equals(obj3))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = 6551;
		int num2 = num;
		for (int i = 0; i < _data.Length; i += 2)
		{
			num = ((num << 27) + (num2 + 1 << 1) + (num >> 5)) ^ _data[i].GetHashCode();
			if (i == _data.Length - 1)
			{
				break;
			}
			num2 = ((num2 << 5) + (num - 1 >> 1) + (num2 >> 27)) ^ _data[i + 1].GetHashCode();
		}
		return num + num2 * 1566083941;
	}

	private int GetHashCode(HashDelegate dlg)
	{
		int num = 6551;
		int num2 = num;
		for (int i = 0; i < _data.Length; i += 2)
		{
			num = ((num << 27) + (num2 + 1 << 1) + (num >> 5)) ^ dlg(_data[i], ref dlg);
			if (i == _data.Length - 1)
			{
				break;
			}
			num2 = ((num2 << 5) + (num - 1 >> 1) + (num2 >> 27)) ^ dlg(_data[i + 1], ref dlg);
		}
		return num + num2 * 1566083941;
	}

	private int GetHashCode(IEqualityComparer comparer)
	{
		int num = 6551;
		int num2 = num;
		for (int i = 0; i < _data.Length; i += 2)
		{
			num = ((num << 27) + (num2 + 1 << 1) + (num >> 5)) ^ comparer.GetHashCode(_data[i]);
			if (i == _data.Length - 1)
			{
				break;
			}
			num2 = ((num2 << 5) + (num - 1 >> 1) + (num2 >> 27)) ^ comparer.GetHashCode(_data[i + 1]);
		}
		return num + num2 * 1566083941;
	}

	public override string ToString()
	{
		return __repr__(DefaultContext.Default);
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		if (comparer is PythonContext.PythonEqualityComparer pythonEqualityComparer)
		{
			return GetHashCode(pythonEqualityComparer.Context.InitialHasher);
		}
		return GetHashCode(comparer);
	}

	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (!object.ReferenceEquals(other, this))
		{
			if (!(other is PythonTuple pythonTuple) || _data.Length != pythonTuple._data.Length)
			{
				return false;
			}
			for (int i = 0; i < _data.Length; i++)
			{
				object obj = _data[i];
				object obj2 = pythonTuple._data[i];
				if (!object.ReferenceEquals(obj, obj2) && !comparer.Equals(obj, obj2))
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual string __repr__(CodeContext context)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		for (int i = 0; i < _data.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(PythonOps.Repr(context, _data[i]));
		}
		if (_data.Length == 1)
		{
			stringBuilder.Append(",");
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	int IList.Add(object value)
	{
		throw new InvalidOperationException("Tuple is readonly");
	}

	void IList.Clear()
	{
		throw new InvalidOperationException("Tuple is readonly");
	}

	void IList.Insert(int index, object value)
	{
		throw new InvalidOperationException("Tuple is readonly");
	}

	void IList.Remove(object value)
	{
		throw new InvalidOperationException("Tuple is readonly");
	}

	void IList.RemoveAt(int index)
	{
		throw new InvalidOperationException("Tuple is readonly");
	}

	public System.Linq.Expressions.Expression CreateExpression()
	{
		System.Linq.Expressions.Expression[] array = new System.Linq.Expressions.Expression[Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Utils.Constant(this[i]);
		}
		return System.Linq.Expressions.Expression.Call(AstMethods.MakeTuple, System.Linq.Expressions.Expression.NewArrayInit(typeof(object), array));
	}
}

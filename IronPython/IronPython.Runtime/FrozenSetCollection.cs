using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("frozenset")]
[DebuggerDisplay("frozenset, {Count} items", TargetTypeName = "frozenset")]
[DebuggerTypeProxy(typeof(CollectionDebugProxy))]
public class FrozenSetCollection : IEnumerable<object>, ICollection, IEnumerable, IStructuralEquatable, ICodeFormattable
{
	private sealed class HashCache
	{
		internal readonly int HashCode;

		internal readonly IEqualityComparer Comparer;

		internal HashCache(int hashCode, IEqualityComparer comparer)
		{
			HashCode = hashCode;
			Comparer = comparer;
		}
	}

	internal SetStorage _items;

	private HashCache _hashCache;

	private static readonly FrozenSetCollection _empty = new FrozenSetCollection();

	private FrozenSetCollection Empty
	{
		get
		{
			if (GetType() == typeof(FrozenSetCollection))
			{
				return _empty;
			}
			return Make(DynamicHelpers.GetPythonType(this), new SetStorage());
		}
	}

	public int Count
	{
		[PythonHidden]
		get
		{
			return _items.Count;
		}
	}

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	public void __init__(params object[] o)
	{
	}

	public static FrozenSetCollection __new__(CodeContext context, object cls)
	{
		if (cls == TypeCache.FrozenSet)
		{
			return _empty;
		}
		object obj = ((PythonType)cls).CreateInstance(context);
		if (!(obj is FrozenSetCollection result))
		{
			throw PythonOps.TypeError("{0} is not a subclass of frozenset", obj);
		}
		return result;
	}

	public static FrozenSetCollection __new__(CodeContext context, object cls, object set)
	{
		if (cls == TypeCache.FrozenSet)
		{
			return Make(TypeCache.FrozenSet, set);
		}
		object obj = ((PythonType)cls).CreateInstance(context, set);
		if (!(obj is FrozenSetCollection result))
		{
			throw PythonOps.TypeError("{0} is not a subclass of frozenset", obj);
		}
		return result;
	}

	public FrozenSetCollection()
		: this(new SetStorage())
	{
	}

	internal FrozenSetCollection(SetStorage set)
	{
		_items = set;
	}

	protected internal FrozenSetCollection(object set)
		: this(SetStorage.GetFrozenItems(set))
	{
	}

	private FrozenSetCollection Make(SetStorage items)
	{
		if (items.Count == 0)
		{
			return Empty;
		}
		if (GetType() == typeof(FrozenSetCollection))
		{
			return new FrozenSetCollection(items);
		}
		return Make(DynamicHelpers.GetPythonType(this), items);
	}

	private static FrozenSetCollection Make(PythonType cls, SetStorage items)
	{
		if (cls == TypeCache.FrozenSet)
		{
			if (items.Count == 0)
			{
				return _empty;
			}
			return new FrozenSetCollection(items);
		}
		FrozenSetCollection frozenSetCollection = PythonCalls.Call(cls) as FrozenSetCollection;
		if (items.Count > 0)
		{
			frozenSetCollection._items = items;
		}
		return frozenSetCollection;
	}

	internal static FrozenSetCollection Make(PythonType cls, object set)
	{
		if (set is FrozenSetCollection result && cls == TypeCache.FrozenSet)
		{
			return result;
		}
		return Make(cls, SetStorage.GetFrozenItems(set));
	}

	public FrozenSetCollection copy()
	{
		if (GetType() == typeof(FrozenSetCollection))
		{
			return this;
		}
		return Make(DynamicHelpers.GetPythonType(this), _items);
	}

	public int __len__()
	{
		return Count;
	}

	public bool __contains__(object item)
	{
		if (!SetStorage.GetHashableSetIfSet(ref item))
		{
			return _items.ContainsAlwaysHash(item);
		}
		return _items.Contains(item);
	}

	public PythonTuple __reduce__()
	{
		return SetStorage.Reduce(_items, TypeCache.FrozenSet);
	}

	private int CalculateHashCode(IEqualityComparer comparer)
	{
		HashCache hashCache = _hashCache;
		if (hashCache != null && object.ReferenceEquals(comparer, hashCache.Comparer))
		{
			return hashCache.HashCode;
		}
		int hashCode = SetStorage.GetHashCode(_items, comparer);
		_hashCache = new HashCache(hashCode, comparer);
		return hashCode;
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return CalculateHashCode(comparer);
	}

	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (SetStorage.GetItemsIfSet(other, out var items))
		{
			return SetStorage.Equals(_items, items, comparer);
		}
		return false;
	}

	public bool __eq__(object other)
	{
		if (SetStorage.GetItemsIfSet(other, out var items) && _items.Count == items.Count)
		{
			return _items.IsSubset(items);
		}
		return false;
	}

	public bool __ne__(object other)
	{
		if (SetStorage.GetItemsIfSet(other, out var items) && _items.Count == items.Count)
		{
			return !_items.IsSubset(items);
		}
		return true;
	}

	public bool isdisjoint(FrozenSetCollection set)
	{
		return _items.IsDisjoint(set._items);
	}

	public bool isdisjoint(SetCollection set)
	{
		return _items.IsDisjoint(set._items);
	}

	public bool isdisjoint(object set)
	{
		return _items.IsDisjoint(SetStorage.GetItems(set));
	}

	public bool issubset(FrozenSetCollection set)
	{
		return _items.IsSubset(set._items);
	}

	public bool issubset(SetCollection set)
	{
		return _items.IsSubset(set._items);
	}

	public bool issubset(object set)
	{
		return _items.IsSubset(SetStorage.GetItems(set));
	}

	public bool issuperset(FrozenSetCollection set)
	{
		return set._items.IsSubset(_items);
	}

	public bool issuperset(SetCollection set)
	{
		return set._items.IsSubset(_items);
	}

	public bool issuperset(object set)
	{
		return SetStorage.GetItems(set).IsSubset(_items);
	}

	public FrozenSetCollection union()
	{
		return Make(_items);
	}

	public FrozenSetCollection union(FrozenSetCollection set)
	{
		return Make(SetStorage.Union(_items, set._items));
	}

	public FrozenSetCollection union(SetCollection set)
	{
		return Make(SetStorage.Union(_items, set._items));
	}

	public FrozenSetCollection union(object set)
	{
		if (SetStorage.GetItems(set, out var items))
		{
			items = SetStorage.Union(_items, items);
		}
		else
		{
			items.UnionUpdate(_items);
		}
		return Make(items);
	}

	public FrozenSetCollection union([NotNull] params object[] sets)
	{
		SetStorage setStorage = _items.Clone();
		foreach (object set in sets)
		{
			setStorage.UnionUpdate(SetStorage.GetItems(set));
		}
		return Make(setStorage);
	}

	public FrozenSetCollection intersection()
	{
		return Make(_items);
	}

	public FrozenSetCollection intersection(FrozenSetCollection set)
	{
		return Make(SetStorage.Intersection(_items, set._items));
	}

	public FrozenSetCollection intersection(SetCollection set)
	{
		return Make(SetStorage.Intersection(_items, set._items));
	}

	public FrozenSetCollection intersection(object set)
	{
		if (SetStorage.GetItems(set, out var items))
		{
			items = SetStorage.Intersection(_items, items);
		}
		else
		{
			items.IntersectionUpdate(_items);
		}
		return Make(items);
	}

	public FrozenSetCollection intersection([NotNull] params object[] sets)
	{
		if (sets.Length == 0)
		{
			return Make(_items);
		}
		SetStorage setStorage = _items;
		foreach (object set in sets)
		{
			SetStorage x = setStorage;
			SetStorage y;
			if (SetStorage.GetItems(set, out var items))
			{
				y = items;
				SetStorage.SortBySize(ref x, ref y);
				if (object.ReferenceEquals(x, items) || object.ReferenceEquals(x, _items))
				{
					x = x.Clone();
				}
			}
			else
			{
				y = items;
				SetStorage.SortBySize(ref x, ref y);
				if (object.ReferenceEquals(x, _items))
				{
					x = x.Clone();
				}
			}
			x.IntersectionUpdate(y);
			setStorage = x;
		}
		return Make(setStorage);
	}

	public FrozenSetCollection difference()
	{
		return Make(_items);
	}

	public FrozenSetCollection difference(FrozenSetCollection set)
	{
		if (object.ReferenceEquals(set, this))
		{
			return Empty;
		}
		return Make(SetStorage.Difference(_items, set._items));
	}

	public FrozenSetCollection difference(SetCollection set)
	{
		return Make(SetStorage.Difference(_items, set._items));
	}

	public FrozenSetCollection difference(object set)
	{
		return Make(SetStorage.Difference(_items, SetStorage.GetItems(set)));
	}

	public FrozenSetCollection difference([NotNull] params object[] sets)
	{
		if (sets.Length == 0)
		{
			return Make(_items);
		}
		SetStorage setStorage = _items;
		foreach (object obj in sets)
		{
			if (object.ReferenceEquals(obj, this))
			{
				return Empty;
			}
			SetStorage items = SetStorage.GetItems(obj);
			if (object.ReferenceEquals(setStorage, _items))
			{
				setStorage = SetStorage.Difference(_items, items);
			}
			else
			{
				setStorage.DifferenceUpdate(items);
			}
		}
		return Make(setStorage);
	}

	public FrozenSetCollection symmetric_difference(FrozenSetCollection set)
	{
		if (object.ReferenceEquals(set, this))
		{
			return Empty;
		}
		return Make(SetStorage.SymmetricDifference(_items, set._items));
	}

	public FrozenSetCollection symmetric_difference(SetCollection set)
	{
		return Make(SetStorage.SymmetricDifference(_items, set._items));
	}

	public FrozenSetCollection symmetric_difference(object set)
	{
		if (SetStorage.GetItems(set, out var items))
		{
			items = SetStorage.SymmetricDifference(_items, items);
		}
		else
		{
			items.SymmetricDifferenceUpdate(_items);
		}
		return Make(items);
	}

	public static FrozenSetCollection operator |(FrozenSetCollection x, FrozenSetCollection y)
	{
		return x.union(y);
	}

	public static FrozenSetCollection operator &(FrozenSetCollection x, FrozenSetCollection y)
	{
		return x.intersection(y);
	}

	public static FrozenSetCollection operator ^(FrozenSetCollection x, FrozenSetCollection y)
	{
		return x.symmetric_difference(y);
	}

	public static FrozenSetCollection operator -(FrozenSetCollection x, FrozenSetCollection y)
	{
		return x.difference(y);
	}

	public static FrozenSetCollection operator |(FrozenSetCollection x, SetCollection y)
	{
		return x.union(y);
	}

	public static FrozenSetCollection operator &(FrozenSetCollection x, SetCollection y)
	{
		return x.intersection(y);
	}

	public static FrozenSetCollection operator ^(FrozenSetCollection x, SetCollection y)
	{
		return x.symmetric_difference(y);
	}

	public static FrozenSetCollection operator -(FrozenSetCollection x, SetCollection y)
	{
		return x.difference(y);
	}

	public static bool operator >(FrozenSetCollection self, object other)
	{
		if (SetStorage.GetItemsIfSet(other, out var items))
		{
			return items.IsStrictSubset(self._items);
		}
		throw PythonOps.TypeError("can only compare to a set");
	}

	public static bool operator <(FrozenSetCollection self, object other)
	{
		if (SetStorage.GetItemsIfSet(other, out var items))
		{
			return self._items.IsStrictSubset(items);
		}
		throw PythonOps.TypeError("can only compare to a set");
	}

	public static bool operator >=(FrozenSetCollection self, object other)
	{
		if (SetStorage.GetItemsIfSet(other, out var items))
		{
			return items.IsSubset(self._items);
		}
		throw PythonOps.TypeError("can only compare to a set");
	}

	public static bool operator <=(FrozenSetCollection self, object other)
	{
		if (SetStorage.GetItemsIfSet(other, out var items))
		{
			return self._items.IsSubset(items);
		}
		throw PythonOps.TypeError("can only compare to a set");
	}

	[SpecialName]
	public int Compare(object o)
	{
		throw PythonOps.TypeError("cannot compare sets using cmp()");
	}

	public int __cmp__(object o)
	{
		throw PythonOps.TypeError("cannot compare sets using cmp()");
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new SetIterator(_items, mutable: false);
	}

	IEnumerator<object> IEnumerable<object>.GetEnumerator()
	{
		return new SetIterator(_items, mutable: false);
	}

	public virtual string __repr__(CodeContext context)
	{
		return SetStorage.SetToString(context, this, _items);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		int num = 0;
		foreach (object item in (IEnumerable<object>)this)
		{
			array.SetValue(item, index + num++);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[DebuggerDisplay("set, {Count} items", TargetTypeName = "set")]
[PythonType("set")]
[DebuggerTypeProxy(typeof(CollectionDebugProxy))]
public class SetCollection : IEnumerable<object>, ICollection, IEnumerable, IStructuralEquatable, ICodeFormattable
{
	public const object __hash__ = null;

	internal SetStorage _items;

	private SetCollection Empty
	{
		get
		{
			if (GetType() == typeof(SetCollection))
			{
				return new SetCollection();
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

	public void __init__()
	{
		clear();
	}

	public void __init__([NotNull] SetCollection set)
	{
		_items = set._items.Clone();
	}

	public void __init__([NotNull] FrozenSetCollection set)
	{
		_items = set._items.Clone();
	}

	public void __init__(object set)
	{
		if (SetStorage.GetItems(set, out var items))
		{
			_items = items.Clone();
		}
		else
		{
			_items = items;
		}
	}

	public static object __new__(CodeContext context, PythonType cls)
	{
		if (cls == TypeCache.Set)
		{
			return new SetCollection();
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

	public static object __new__(CodeContext context, PythonType cls, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] argsø)
	{
		return __new__(context, cls);
	}

	public SetCollection()
	{
		_items = new SetStorage();
	}

	internal SetCollection(SetStorage items)
	{
		_items = items;
	}

	internal SetCollection(object[] items)
	{
		_items = new SetStorage(items.Length);
		foreach (object item in items)
		{
			_items.AddNoLock(item);
		}
	}

	private SetCollection Make(SetStorage items)
	{
		if (GetType() == typeof(SetCollection))
		{
			return new SetCollection(items);
		}
		return Make(DynamicHelpers.GetPythonType(this), items);
	}

	private static SetCollection Make(PythonType cls, SetStorage items)
	{
		if (cls == TypeCache.Set)
		{
			return new SetCollection(items);
		}
		SetCollection setCollection = PythonCalls.Call(cls) as SetCollection;
		if (items.Count > 0)
		{
			setCollection._items = items;
		}
		return setCollection;
	}

	internal static SetCollection Make(PythonType cls, object set)
	{
		if (SetStorage.GetItems(set, out var items))
		{
			items = items.Clone();
		}
		return Make(cls, items);
	}

	public SetCollection copy()
	{
		return Make(_items.Clone());
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
		return SetStorage.Reduce(_items, TypeCache.Set);
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
			return ((IStructuralEquatable)new FrozenSetCollection(_items)).GetHashCode(comparer);
		}
		finally
		{
			CompareUtil.Pop(this);
		}
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

	public void add(object item)
	{
		_items.Add(item);
	}

	public void clear()
	{
		_items.Clear();
	}

	public void discard(object item)
	{
		SetStorage.GetHashableSetIfSet(ref item);
		_items.Remove(item);
	}

	public object pop()
	{
		if (_items.Pop(out var item))
		{
			return item;
		}
		throw PythonOps.KeyError("pop from an empty set");
	}

	public void remove(object item)
	{
		object o = item;
		if (!((!SetStorage.GetHashableSetIfSet(ref o)) ? _items.RemoveAlwaysHash(o) : _items.Remove(o)))
		{
			throw PythonOps.KeyError(item);
		}
	}

	public void update(SetCollection set)
	{
		if (object.ReferenceEquals(set, this))
		{
			return;
		}
		lock (_items)
		{
			_items.UnionUpdate(set._items);
		}
	}

	public void update(FrozenSetCollection set)
	{
		lock (_items)
		{
			_items.UnionUpdate(set._items);
		}
	}

	public void update(object set)
	{
		if (object.ReferenceEquals(set, this))
		{
			return;
		}
		SetStorage items = SetStorage.GetItems(set);
		lock (_items)
		{
			_items.UnionUpdate(items);
		}
	}

	public void update([NotNull] params object[] sets)
	{
		if (sets.Length == 0)
		{
			return;
		}
		lock (_items)
		{
			foreach (object obj in sets)
			{
				if (!object.ReferenceEquals(obj, this))
				{
					_items.UnionUpdate(SetStorage.GetItems(obj));
				}
			}
		}
	}

	public void intersection_update(SetCollection set)
	{
		if (object.ReferenceEquals(set, this))
		{
			return;
		}
		lock (_items)
		{
			_items.IntersectionUpdate(set._items);
		}
	}

	public void intersection_update(FrozenSetCollection set)
	{
		lock (_items)
		{
			_items.IntersectionUpdate(set._items);
		}
	}

	public void intersection_update(object set)
	{
		if (object.ReferenceEquals(set, this))
		{
			return;
		}
		SetStorage items = SetStorage.GetItems(set);
		lock (_items)
		{
			_items.IntersectionUpdate(items);
		}
	}

	public void intersection_update([NotNull] params object[] sets)
	{
		if (sets.Length == 0)
		{
			return;
		}
		lock (_items)
		{
			foreach (object obj in sets)
			{
				if (!object.ReferenceEquals(obj, this))
				{
					_items.IntersectionUpdate(SetStorage.GetItems(obj));
				}
			}
		}
	}

	public void difference_update(SetCollection set)
	{
		if (object.ReferenceEquals(set, this))
		{
			_items.Clear();
			return;
		}
		lock (_items)
		{
			_items.DifferenceUpdate(set._items);
		}
	}

	public void difference_update(FrozenSetCollection set)
	{
		lock (_items)
		{
			_items.DifferenceUpdate(set._items);
		}
	}

	public void difference_update(object set)
	{
		if (object.ReferenceEquals(set, this))
		{
			_items.Clear();
			return;
		}
		SetStorage items = SetStorage.GetItems(set);
		lock (_items)
		{
			_items.DifferenceUpdate(items);
		}
	}

	public void difference_update([NotNull] params object[] sets)
	{
		if (sets.Length == 0)
		{
			return;
		}
		lock (_items)
		{
			foreach (object obj in sets)
			{
				if (object.ReferenceEquals(obj, this))
				{
					_items.ClearNoLock();
					break;
				}
				_items.DifferenceUpdate(SetStorage.GetItems(obj));
			}
		}
	}

	public void symmetric_difference_update(SetCollection set)
	{
		if (object.ReferenceEquals(set, this))
		{
			_items.Clear();
			return;
		}
		lock (_items)
		{
			_items.SymmetricDifferenceUpdate(set._items);
		}
	}

	public void symmetric_difference_update(FrozenSetCollection set)
	{
		lock (_items)
		{
			_items.SymmetricDifferenceUpdate(set._items);
		}
	}

	public void symmetric_difference_update(object set)
	{
		if (object.ReferenceEquals(set, this))
		{
			_items.Clear();
			return;
		}
		SetStorage items = SetStorage.GetItems(set);
		lock (_items)
		{
			_items.SymmetricDifferenceUpdate(items);
		}
	}

	public bool isdisjoint(SetCollection set)
	{
		return _items.IsDisjoint(set._items);
	}

	public bool isdisjoint(FrozenSetCollection set)
	{
		return _items.IsDisjoint(set._items);
	}

	public bool isdisjoint(object set)
	{
		return _items.IsDisjoint(SetStorage.GetItems(set));
	}

	public bool issubset(SetCollection set)
	{
		return _items.IsSubset(set._items);
	}

	public bool issubset(FrozenSetCollection set)
	{
		return _items.IsSubset(set._items);
	}

	public bool issubset(object set)
	{
		return _items.IsSubset(SetStorage.GetItems(set));
	}

	public bool issuperset(SetCollection set)
	{
		return set._items.IsSubset(_items);
	}

	public bool issuperset(FrozenSetCollection set)
	{
		return set._items.IsSubset(_items);
	}

	public bool issuperset(object set)
	{
		return SetStorage.GetItems(set).IsSubset(_items);
	}

	public SetCollection union()
	{
		return copy();
	}

	public SetCollection union(SetCollection set)
	{
		return Make(SetStorage.Union(_items, set._items));
	}

	public SetCollection union(FrozenSetCollection set)
	{
		return Make(SetStorage.Union(_items, set._items));
	}

	public SetCollection union(object set)
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

	public SetCollection union([NotNull] params object[] sets)
	{
		SetStorage setStorage = _items.Clone();
		foreach (object set in sets)
		{
			setStorage.UnionUpdate(SetStorage.GetItems(set));
		}
		return Make(setStorage);
	}

	public SetCollection intersection()
	{
		return copy();
	}

	public SetCollection intersection(SetCollection set)
	{
		return Make(SetStorage.Intersection(_items, set._items));
	}

	public SetCollection intersection(FrozenSetCollection set)
	{
		return Make(SetStorage.Intersection(_items, set._items));
	}

	public SetCollection intersection(object set)
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

	public SetCollection intersection([NotNull] params object[] sets)
	{
		if (sets.Length == 0)
		{
			return copy();
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

	public SetCollection difference()
	{
		return copy();
	}

	public SetCollection difference(SetCollection set)
	{
		if (object.ReferenceEquals(set, this))
		{
			return Empty;
		}
		return Make(SetStorage.Difference(_items, set._items));
	}

	public SetCollection difference(FrozenSetCollection set)
	{
		return Make(SetStorage.Difference(_items, set._items));
	}

	public SetCollection difference(object set)
	{
		return Make(SetStorage.Difference(_items, SetStorage.GetItems(set)));
	}

	public SetCollection difference([NotNull] params object[] sets)
	{
		if (sets.Length == 0)
		{
			return copy();
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

	public SetCollection symmetric_difference(SetCollection set)
	{
		if (object.ReferenceEquals(set, this))
		{
			return Empty;
		}
		return Make(SetStorage.SymmetricDifference(_items, set._items));
	}

	public SetCollection symmetric_difference(FrozenSetCollection set)
	{
		return Make(SetStorage.SymmetricDifference(_items, set._items));
	}

	public SetCollection symmetric_difference(object set)
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

	[SpecialName]
	public SetCollection InPlaceBitwiseOr(SetCollection set)
	{
		update(set);
		return this;
	}

	[SpecialName]
	public SetCollection InPlaceBitwiseOr(FrozenSetCollection set)
	{
		update(set);
		return this;
	}

	[SpecialName]
	public SetCollection InPlaceBitwiseOr(object set)
	{
		if (set is FrozenSetCollection || set is SetCollection)
		{
			update(set);
			return this;
		}
		throw PythonOps.TypeError("unsupported operand type(s) for |=: '{0}' and '{1}'", PythonTypeOps.GetName(this), PythonTypeOps.GetName(set));
	}

	[SpecialName]
	public SetCollection InPlaceBitwiseAnd(SetCollection set)
	{
		intersection_update(set);
		return this;
	}

	[SpecialName]
	public SetCollection InPlaceBitwiseAnd(FrozenSetCollection set)
	{
		intersection_update(set);
		return this;
	}

	[SpecialName]
	public SetCollection InPlaceBitwiseAnd(object set)
	{
		if (set is FrozenSetCollection || set is SetCollection)
		{
			intersection_update(set);
			return this;
		}
		throw PythonOps.TypeError("unsupported operand type(s) for &=: '{0}' and '{1}'", PythonTypeOps.GetName(this), PythonTypeOps.GetName(set));
	}

	[SpecialName]
	public SetCollection InPlaceExclusiveOr(SetCollection set)
	{
		symmetric_difference_update(set);
		return this;
	}

	[SpecialName]
	public SetCollection InPlaceExclusiveOr(FrozenSetCollection set)
	{
		symmetric_difference_update(set);
		return this;
	}

	[SpecialName]
	public SetCollection InPlaceExclusiveOr(object set)
	{
		if (set is FrozenSetCollection || set is SetCollection)
		{
			symmetric_difference_update(set);
			return this;
		}
		throw PythonOps.TypeError("unsupported operand type(s) for ^=: '{0}' and '{1}'", PythonTypeOps.GetName(this), PythonTypeOps.GetName(set));
	}

	[SpecialName]
	public SetCollection InPlaceSubtract(SetCollection set)
	{
		difference_update(set);
		return this;
	}

	[SpecialName]
	public SetCollection InPlaceSubtract(FrozenSetCollection set)
	{
		difference_update(set);
		return this;
	}

	[SpecialName]
	public SetCollection InPlaceSubtract(object set)
	{
		if (set is FrozenSetCollection || set is SetCollection)
		{
			difference_update(set);
			return this;
		}
		throw PythonOps.TypeError("unsupported operand type(s) for -=: '{0}' and '{1}'", PythonTypeOps.GetName(this), PythonTypeOps.GetName(set));
	}

	public static SetCollection operator |(SetCollection x, SetCollection y)
	{
		return x.union(y);
	}

	public static SetCollection operator &(SetCollection x, SetCollection y)
	{
		return x.intersection(y);
	}

	public static SetCollection operator ^(SetCollection x, SetCollection y)
	{
		return x.symmetric_difference(y);
	}

	public static SetCollection operator -(SetCollection x, SetCollection y)
	{
		return x.difference(y);
	}

	public static SetCollection operator |(SetCollection x, FrozenSetCollection y)
	{
		return x.union(y);
	}

	public static SetCollection operator &(SetCollection x, FrozenSetCollection y)
	{
		return x.intersection(y);
	}

	public static SetCollection operator ^(SetCollection x, FrozenSetCollection y)
	{
		return x.symmetric_difference(y);
	}

	public static SetCollection operator -(SetCollection x, FrozenSetCollection y)
	{
		return x.difference(y);
	}

	public static bool operator >(SetCollection self, object other)
	{
		if (SetStorage.GetItemsIfSet(other, out var items))
		{
			return items.IsStrictSubset(self._items);
		}
		throw PythonOps.TypeError("can only compare to a set");
	}

	public static bool operator <(SetCollection self, object other)
	{
		if (SetStorage.GetItemsIfSet(other, out var items))
		{
			return self._items.IsStrictSubset(items);
		}
		throw PythonOps.TypeError("can only compare to a set");
	}

	public static bool operator >=(SetCollection self, object other)
	{
		if (SetStorage.GetItemsIfSet(other, out var items))
		{
			return items.IsSubset(self._items);
		}
		throw PythonOps.TypeError("can only compare to a set");
	}

	public static bool operator <=(SetCollection self, object other)
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
		return new SetIterator(_items, mutable: true);
	}

	IEnumerator<object> IEnumerable<object>.GetEnumerator()
	{
		return new SetIterator(_items, mutable: true);
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

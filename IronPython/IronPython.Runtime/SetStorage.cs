using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime;

[Serializable]
internal sealed class SetStorage : IEnumerable<object>, IEnumerable, ISerializable, IDeserializationCallback
{
	internal struct Bucket
	{
		public object Item;

		public int HashCode;

		public Bucket(int hashCode, object item)
		{
			HashCode = hashCode;
			Item = item;
		}
	}

	private const int InitialBuckets = 8;

	private const double Load = 0.7;

	private const double MinLoad = 0.5;

	internal Bucket[] _buckets;

	internal int _count;

	private int _version;

	internal bool _hasNull;

	private Func<object, int> _hashFunc;

	private Func<object, object, bool> _eqFunc;

	private Type _itemType;

	private int _maxCount;

	private static readonly Type HeterogeneousType = typeof(SetStorage);

	internal static readonly object Removed = new object();

	private static readonly Func<object, int> _primitiveHash = PrimitiveHash;

	private static readonly Func<object, int> _intHash = IntHash;

	private static readonly Func<object, int> _doubleHash = DoubleHash;

	private static readonly Func<object, int> _tupleHash = TupleHash;

	private static readonly Func<object, int> _genericHash = GenericHash;

	private static readonly Func<object, object, bool> _stringEquals = StringEquals;

	private static readonly Func<object, object, bool> _intEquals = IntEquals;

	private static readonly Func<object, object, bool> _doubleEquals = DoubleEquals;

	private static readonly Func<object, object, bool> _tupleEquals = TupleEquals;

	private static readonly Func<object, object, bool> _genericEquals = GenericEquals;

	private static readonly Func<object, object, bool> _objectEquals = object.ReferenceEquals;

	public int Count
	{
		get
		{
			int num = _count;
			if (_hasNull)
			{
				num++;
			}
			return num;
		}
	}

	public int Version => _version;

	public SetStorage()
	{
	}

	public SetStorage(int count)
	{
		Initialize(count);
	}

	private SetStorage(SerializationInfo info, StreamingContext context)
	{
		_buckets = new Bucket[1]
		{
			new Bucket(0, info)
		};
	}

	private void Initialize()
	{
		_maxCount = 5;
		_buckets = new Bucket[8];
	}

	private void Initialize(int count)
	{
		int x = Math.Max((int)((double)count / 0.7) + 1, 8);
		x = 1 << CeilLog2(x);
		_maxCount = (int)((double)x * 0.7);
		_buckets = new Bucket[x];
	}

	public void Add(object item)
	{
		lock (this)
		{
			AddNoLock(item);
		}
	}

	public void AddNoLock(object item)
	{
		if (item != null)
		{
			if (_buckets == null)
			{
				Initialize();
			}
			if (item.GetType() != _itemType && _itemType != HeterogeneousType)
			{
				UpdateHelperFunctions(item.GetType(), item);
			}
			AddWorker(item, Hash(item));
		}
		else
		{
			_hasNull = true;
		}
	}

	private void AddWorker(object item, int hashCode)
	{
		if (AddWorker(_buckets, item, hashCode, _eqFunc, ref _version))
		{
			_count++;
			if (_count > _maxCount)
			{
				Grow();
			}
		}
	}

	private static bool AddWorker(Bucket[] buckets, object item, int hashCode, Func<object, object, bool> eqFunc, ref int version)
	{
		int index = hashCode & (buckets.Length - 1);
		while (true)
		{
			Bucket bucket = buckets[index];
			if (bucket.Item == null || bucket.Item == Removed)
			{
				version++;
				buckets[index].HashCode = hashCode;
				buckets[index].Item = item;
				return true;
			}
			if (bucket.HashCode == hashCode && eqFunc(item, bucket.Item))
			{
				break;
			}
			ProbeNext(buckets, ref index);
		}
		return false;
	}

	private void AddOrRemoveWorker(object item, int hashCode)
	{
		int index = hashCode & (_buckets.Length - 1);
		while (true)
		{
			Bucket bucket = _buckets[index];
			if (bucket.Item == null)
			{
				_version++;
				_buckets[index].HashCode = hashCode;
				_buckets[index].Item = item;
				_count++;
				if (_count > _maxCount)
				{
					Grow();
				}
				return;
			}
			if (bucket.Item != Removed && bucket.HashCode == hashCode && _eqFunc(item, bucket.Item))
			{
				break;
			}
			ProbeNext(_buckets, ref index);
		}
		_version++;
		_buckets[index].Item = Removed;
		_count--;
	}

	public void Clear()
	{
		lock (this)
		{
			ClearNoLock();
		}
	}

	public void ClearNoLock()
	{
		if (_buckets != null)
		{
			_version++;
			Initialize();
			_count = 0;
		}
		_hasNull = false;
	}

	public SetStorage Clone()
	{
		SetStorage setStorage = new SetStorage();
		setStorage._hasNull = _hasNull;
		if (_count == 0)
		{
			return setStorage;
		}
		Bucket[] buckets = _buckets;
		setStorage._hashFunc = _hashFunc;
		setStorage._eqFunc = _eqFunc;
		setStorage._itemType = _itemType;
		if ((double)_count < (double)_buckets.Length * 0.5)
		{
			setStorage.Initialize(_count);
			for (int i = 0; i < buckets.Length; i++)
			{
				Bucket bucket = buckets[i];
				if (bucket.Item != null && bucket.Item != Removed)
				{
					setStorage.AddWorker(bucket.Item, bucket.HashCode);
				}
			}
		}
		else
		{
			setStorage._maxCount = (int)((double)buckets.Length * 0.7);
			setStorage._buckets = new Bucket[buckets.Length];
			for (int j = 0; j < buckets.Length; j++)
			{
				Bucket bucket2 = buckets[j];
				if (bucket2.Item != null)
				{
					setStorage._buckets[j].Item = bucket2.Item;
					setStorage._buckets[j].HashCode = bucket2.HashCode;
					setStorage._count++;
				}
			}
		}
		return setStorage;
	}

	public bool Contains(object item)
	{
		if (item == null)
		{
			return _hasNull;
		}
		if (_count == 0)
		{
			return false;
		}
		int hashCode;
		Func<object, object, bool> eqFunc;
		if (item.GetType() == _itemType || _itemType == HeterogeneousType)
		{
			hashCode = _hashFunc(item);
			eqFunc = _eqFunc;
		}
		else
		{
			hashCode = _genericHash(item);
			eqFunc = _genericEquals;
		}
		return ContainsWorker(_buckets, item, hashCode, eqFunc);
	}

	public bool ContainsAlwaysHash(object item)
	{
		if (item == null)
		{
			return _hasNull;
		}
		int hashCode;
		Func<object, object, bool> eqFunc;
		if (item.GetType() == _itemType || _itemType == HeterogeneousType)
		{
			hashCode = _hashFunc(item);
			eqFunc = _eqFunc;
		}
		else
		{
			hashCode = _genericHash(item);
			eqFunc = _genericEquals;
		}
		if (_count > 0)
		{
			return ContainsWorker(_buckets, item, hashCode, eqFunc);
		}
		return false;
	}

	private static bool ContainsWorker(Bucket[] buckets, object item, int hashCode, Func<object, object, bool> eqFunc)
	{
		int index = hashCode & (buckets.Length - 1);
		int num = index;
		do
		{
			Bucket bucket = buckets[index];
			if (bucket.Item == null)
			{
				break;
			}
			if (bucket.Item != Removed && bucket.HashCode == hashCode && eqFunc(item, bucket.Item))
			{
				return true;
			}
			ProbeNext(buckets, ref index);
		}
		while (num != index);
		return false;
	}

	public void CopyTo(SetStorage into)
	{
		lock (into)
		{
			into.UnionUpdate(this);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<object> GetEnumerator()
	{
		if (_hasNull)
		{
			yield return null;
		}
		if (_count == 0)
		{
			yield break;
		}
		Bucket[] buckets = _buckets;
		for (int i = 0; i < buckets.Length; i++)
		{
			object item = buckets[i].Item;
			if (item != null && item != Removed)
			{
				yield return item;
			}
		}
	}

	public List GetItems()
	{
		List list = new List(Count);
		if (_hasNull)
		{
			list.AddNoLock(null);
		}
		if (_count > 0)
		{
			Bucket[] buckets = _buckets;
			for (int i = 0; i < buckets.Length; i++)
			{
				object item = buckets[i].Item;
				if (item != null && item != Removed)
				{
					list.AddNoLock(item);
				}
			}
		}
		return list;
	}

	public bool Pop(out object item)
	{
		item = null;
		if (_hasNull)
		{
			_hasNull = false;
			return true;
		}
		if (_count == 0)
		{
			return false;
		}
		lock (this)
		{
			for (int i = 0; i < _buckets.Length; i++)
			{
				if (_buckets[i].Item != null && _buckets[i].Item != Removed)
				{
					item = _buckets[i].Item;
					_version++;
					_buckets[i].Item = Removed;
					_count--;
					return true;
				}
			}
			item = null;
			return false;
		}
	}

	public bool Remove(object item)
	{
		lock (this)
		{
			return RemoveNoLock(item);
		}
	}

	public bool RemoveNoLock(object item)
	{
		if (item == null)
		{
			return RemoveNull();
		}
		if (_count == 0)
		{
			return false;
		}
		return RemoveItem(item);
	}

	internal bool RemoveAlwaysHash(object item)
	{
		lock (this)
		{
			if (item == null)
			{
				return RemoveNull();
			}
			return RemoveItem(item);
		}
	}

	private bool RemoveNull()
	{
		if (_hasNull)
		{
			_hasNull = false;
			return true;
		}
		return false;
	}

	private bool RemoveItem(object item)
	{
		int hashCode;
		Func<object, object, bool> eqFunc;
		if (item.GetType() == _itemType || _itemType == HeterogeneousType)
		{
			hashCode = _hashFunc(item);
			eqFunc = _eqFunc;
		}
		else
		{
			hashCode = _genericHash(item);
			eqFunc = _genericEquals;
		}
		return RemoveWorker(item, hashCode, eqFunc);
	}

	private bool RemoveWorker(object item, int hashCode, Func<object, object, bool> eqFunc)
	{
		if (_count == 0)
		{
			return false;
		}
		int index = hashCode & (_buckets.Length - 1);
		int num = index;
		do
		{
			Bucket bucket = _buckets[index];
			if (bucket.Item == null)
			{
				break;
			}
			if (bucket.Item != Removed && bucket.HashCode == hashCode && eqFunc(item, bucket.Item))
			{
				_version++;
				_buckets[index].Item = Removed;
				_count--;
				return true;
			}
			ProbeNext(_buckets, ref index);
		}
		while (index != num);
		return false;
	}

	public bool IsDisjoint(SetStorage other)
	{
		return IsDisjoint(this, other);
	}

	public static bool IsDisjoint(SetStorage self, SetStorage other)
	{
		SortBySize(ref self, ref other);
		if (self._hasNull && other._hasNull)
		{
			return false;
		}
		if (self._count == 0 || other._count == 0)
		{
			return true;
		}
		Bucket[] buckets = self._buckets;
		Bucket[] buckets2 = other._buckets;
		Func<object, object, bool> eqFunc = GetEqFunc(self, other);
		for (int i = 0; i < buckets.Length; i++)
		{
			Bucket bucket = buckets[i];
			if (bucket.Item != null && bucket.Item != Removed && ContainsWorker(buckets2, bucket.Item, bucket.HashCode, eqFunc))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsSubset(SetStorage other)
	{
		if (_count > other._count || (_hasNull && !other._hasNull))
		{
			return false;
		}
		return IsSubsetWorker(other);
	}

	public bool IsStrictSubset(SetStorage other)
	{
		if (_count > other._count || (_hasNull && !other._hasNull) || Count == other.Count)
		{
			return false;
		}
		return IsSubsetWorker(other);
	}

	private bool IsSubsetWorker(SetStorage other)
	{
		if (_count == 0)
		{
			return true;
		}
		if (other._count == 0)
		{
			return false;
		}
		Bucket[] buckets = _buckets;
		Bucket[] buckets2 = other._buckets;
		Func<object, object, bool> eqFunc = GetEqFunc(this, other);
		for (int i = 0; i < buckets.Length; i++)
		{
			Bucket bucket = buckets[i];
			if (bucket.Item != null && bucket.Item != Removed && !ContainsWorker(buckets2, bucket.Item, bucket.HashCode, eqFunc))
			{
				return false;
			}
		}
		return true;
	}

	public void UnionUpdate(SetStorage other)
	{
		_hasNull |= other._hasNull;
		if (other._count == 0)
		{
			return;
		}
		if (_buckets == null)
		{
			Initialize(other._count);
		}
		Bucket[] buckets = other._buckets;
		UpdateHelperFunctions(other);
		for (int i = 0; i < buckets.Length; i++)
		{
			Bucket bucket = buckets[i];
			if (bucket.Item != null && bucket.Item != Removed)
			{
				AddWorker(bucket.Item, bucket.HashCode);
			}
		}
	}

	public void IntersectionUpdate(SetStorage other)
	{
		if (other._count == 0)
		{
			ClearNoLock();
			_hasNull &= other._hasNull;
			return;
		}
		_hasNull &= other._hasNull;
		if (_count == 0)
		{
			return;
		}
		Bucket[] buckets = _buckets;
		Bucket[] buckets2 = other._buckets;
		Func<object, object, bool> eqFunc = GetEqFunc(this, other);
		for (int i = 0; i < buckets.Length; i++)
		{
			Bucket bucket = buckets[i];
			if (bucket.Item != null && bucket.Item != Removed && !ContainsWorker(buckets2, bucket.Item, bucket.HashCode, eqFunc))
			{
				_version++;
				buckets[i].Item = Removed;
				_count--;
			}
		}
	}

	public void SymmetricDifferenceUpdate(SetStorage other)
	{
		_hasNull ^= other._hasNull;
		if (other._count == 0)
		{
			return;
		}
		if (_buckets == null)
		{
			Initialize();
		}
		Bucket[] buckets = other._buckets;
		UpdateHelperFunctions(other);
		for (int i = 0; i < buckets.Length; i++)
		{
			Bucket bucket = buckets[i];
			if (bucket.Item != null && bucket.Item != Removed)
			{
				AddOrRemoveWorker(bucket.Item, bucket.HashCode);
			}
		}
	}

	public void DifferenceUpdate(SetStorage other)
	{
		_hasNull &= !other._hasNull;
		if (_count == 0 || other._count == 0)
		{
			return;
		}
		Bucket[] buckets = _buckets;
		Bucket[] buckets2 = other._buckets;
		Func<object, object, bool> eqFunc = GetEqFunc(this, other);
		if (buckets.Length < buckets2.Length)
		{
			for (int i = 0; i < buckets.Length; i++)
			{
				Bucket bucket = buckets[i];
				if (bucket.Item != null && bucket.Item != Removed && ContainsWorker(buckets2, bucket.Item, bucket.HashCode, eqFunc))
				{
					RemoveWorker(bucket.Item, bucket.HashCode, eqFunc);
				}
			}
			return;
		}
		for (int j = 0; j < buckets2.Length; j++)
		{
			Bucket bucket2 = buckets2[j];
			if (bucket2.Item != null && bucket2.Item != Removed)
			{
				RemoveWorker(bucket2.Item, bucket2.HashCode, eqFunc);
			}
		}
	}

	public static SetStorage Union(SetStorage self, SetStorage other)
	{
		SetStorage setStorage;
		if (self._count < other._count)
		{
			setStorage = other.Clone();
			setStorage.UnionUpdate(self);
		}
		else
		{
			setStorage = self.Clone();
			setStorage.UnionUpdate(other);
		}
		return setStorage;
	}

	public static SetStorage Intersection(SetStorage self, SetStorage other)
	{
		SetStorage setStorage = new SetStorage(Math.Min(self._count, other._count));
		setStorage._hasNull = self._hasNull && other._hasNull;
		if (self._count == 0 || other._count == 0)
		{
			return setStorage;
		}
		SortBySize(ref self, ref other);
		Bucket[] buckets = self._buckets;
		Bucket[] buckets2 = other._buckets;
		Func<object, object, bool> eqFunc = GetEqFunc(self, other);
		if (other._itemType != HeterogeneousType)
		{
			setStorage.UpdateHelperFunctions(other);
		}
		else
		{
			setStorage.UpdateHelperFunctions(self);
		}
		for (int i = 0; i < buckets.Length; i++)
		{
			Bucket bucket = buckets[i];
			if (bucket.Item != null && bucket.Item != Removed && ContainsWorker(buckets2, bucket.Item, bucket.HashCode, eqFunc))
			{
				setStorage.AddWorker(bucket.Item, bucket.HashCode);
			}
		}
		return setStorage;
	}

	public static SetStorage SymmetricDifference(SetStorage self, SetStorage other)
	{
		SortBySize(ref self, ref other);
		SetStorage setStorage = other.Clone();
		setStorage.SymmetricDifferenceUpdate(self);
		return setStorage;
	}

	public static SetStorage Difference(SetStorage self, SetStorage other)
	{
		SetStorage setStorage;
		if (self._count == 0 || other._count == 0)
		{
			setStorage = self.Clone();
			setStorage._hasNull &= !other._hasNull;
			return setStorage;
		}
		if (self._buckets.Length <= other._buckets.Length)
		{
			setStorage = new SetStorage(self._count);
			setStorage._hasNull &= !other._hasNull;
			Bucket[] buckets = self._buckets;
			Bucket[] buckets2 = other._buckets;
			Func<object, object, bool> eqFunc = GetEqFunc(self, other);
			setStorage.UpdateHelperFunctions(self);
			for (int i = 0; i < buckets.Length; i++)
			{
				Bucket bucket = buckets[i];
				if (bucket.Item != null && bucket.Item != Removed && !ContainsWorker(buckets2, bucket.Item, bucket.HashCode, eqFunc))
				{
					setStorage.AddWorker(bucket.Item, bucket.HashCode);
				}
			}
		}
		else
		{
			setStorage = self.Clone();
			setStorage.DifferenceUpdate(other);
		}
		return setStorage;
	}

	public static bool Equals(SetStorage x, SetStorage y, IEqualityComparer comparer)
	{
		if (object.ReferenceEquals(x, y))
		{
			return true;
		}
		if (x._count != y._count || (x._hasNull ^ y._hasNull))
		{
			return false;
		}
		if (x._count == 0)
		{
			return true;
		}
		SortBySize(ref x, ref y);
		if (comparer is PythonContext.PythonEqualityComparer)
		{
			Bucket[] buckets = x._buckets;
			Bucket[] buckets2 = y._buckets;
			Func<object, object, bool> eqFunc = GetEqFunc(x, y);
			for (int i = 0; i < buckets.Length; i++)
			{
				Bucket bucket = buckets[i];
				if (bucket.Item != null && bucket.Item != Removed && !ContainsWorker(buckets2, bucket.Item, bucket.HashCode, eqFunc))
				{
					return false;
				}
			}
			return true;
		}
		SetStorage setStorage = new SetStorage();
		setStorage._itemType = HeterogeneousType;
		setStorage._eqFunc = comparer.Equals;
		setStorage._hashFunc = comparer.GetHashCode;
		foreach (object item in y)
		{
			setStorage.AddNoLock(item);
		}
		foreach (object item2 in x)
		{
			if (!setStorage.RemoveNoLock(item2))
			{
				return false;
			}
		}
		return setStorage._count == 0;
	}

	public static int GetHashCode(SetStorage set, IEqualityComparer comparer)
	{
		int num = 1420601183;
		int num2 = 674132117;
		int num3 = 393601577;
		if (set._count > 0)
		{
			num ^= set._count * 8803;
			num = (num << 10) ^ (num >> 22);
			num2 += set._count * 5179;
			num2 = (num2 << 10) ^ (num2 >> 22);
			num3 = num3 * set._count + 784251623;
			num3 = (num3 << 10) ^ (num3 >> 22);
		}
		if (comparer is PythonContext.PythonEqualityComparer)
		{
			if (set._hasNull)
			{
				num = (num << 7) ^ (num >> 25) ^ 0x1E1A2E40;
				num2 = ((num2 << 7) ^ (num2 >> 25)) + 505032256;
				num3 = ((num3 << 7) ^ (num3 >> 25)) * 505032256;
			}
			if (set._count > 0)
			{
				Bucket[] buckets = set._buckets;
				for (int i = 0; i < buckets.Length; i++)
				{
					object item = buckets[i].Item;
					if (item != null && item != Removed)
					{
						int hashCode = buckets[i].HashCode;
						num ^= hashCode;
						num2 += hashCode;
						num3 *= hashCode;
					}
				}
			}
		}
		else
		{
			if (set._hasNull)
			{
				int hashCode2 = comparer.GetHashCode(null);
				num = (num + ((num << 7) ^ (num >> 25))) ^ hashCode2;
				num2 = ((num2 << 7) ^ (num2 >> 25)) + hashCode2;
				num3 = ((num3 << 7) ^ (num3 >> 25)) * hashCode2;
			}
			if (set._count > 0)
			{
				Bucket[] buckets2 = set._buckets;
				for (int j = 0; j < buckets2.Length; j++)
				{
					object item2 = buckets2[j].Item;
					if (item2 != null && item2 != Removed)
					{
						int hashCode3 = comparer.GetHashCode(item2);
						num ^= hashCode3;
						num2 += hashCode3;
						num3 *= hashCode3;
					}
				}
			}
		}
		num = (num << 11) ^ (num >> 21) ^ num2;
		num = (num << 27) ^ (num >> 5) ^ num3;
		return (num << 9) ^ (num >> 23) ^ 0x774614B1;
	}

	private static int PrimitiveHash(object o)
	{
		return o.GetHashCode();
	}

	private static int IntHash(object o)
	{
		return (int)o;
	}

	private static int DoubleHash(object o)
	{
		return DoubleOps.__hash__((double)o);
	}

	private static int TupleHash(object o)
	{
		return ((IStructuralEquatable)o).GetHashCode(DefaultContext.DefaultPythonContext.EqualityComparerNonGeneric);
	}

	private static int GenericHash(object o)
	{
		return PythonOps.Hash(DefaultContext.Default, o);
	}

	private static bool StringEquals(object o1, object o2)
	{
		return (string)o1 == (string)o2;
	}

	private static bool IntEquals(object o1, object o2)
	{
		return (int)o1 == (int)o2;
	}

	private static bool DoubleEquals(object o1, object o2)
	{
		return (double)o1 == (double)o2;
	}

	private static bool TupleEquals(object o1, object o2)
	{
		return ((IStructuralEquatable)o1).Equals(o2, DefaultContext.DefaultPythonContext.EqualityComparerNonGeneric);
	}

	private static bool GenericEquals(object o1, object o2)
	{
		if (!object.ReferenceEquals(o1, o2))
		{
			return PythonOps.EqualRetBool(o1, o2);
		}
		return true;
	}

	private void UpdateHelperFunctions(SetStorage other)
	{
		if (!(_itemType == HeterogeneousType) && !(_itemType == other._itemType))
		{
			if (other._itemType == HeterogeneousType)
			{
				SetHeterogeneousSites();
			}
			else if (_itemType == null)
			{
				_hashFunc = other._hashFunc;
				_eqFunc = other._eqFunc;
				_itemType = other._itemType;
			}
			else
			{
				SetHeterogeneousSites();
			}
		}
	}

	private void UpdateHelperFunctions(Type t, object item)
	{
		if (_itemType == null)
		{
			if (t == typeof(int))
			{
				_hashFunc = _intHash;
				_eqFunc = _intEquals;
			}
			else if (t == typeof(string))
			{
				_hashFunc = _primitiveHash;
				_eqFunc = _stringEquals;
			}
			else if (t == typeof(double))
			{
				_hashFunc = _doubleHash;
				_eqFunc = _doubleEquals;
			}
			else if (t == typeof(PythonTuple))
			{
				_hashFunc = _tupleHash;
				_eqFunc = _tupleEquals;
			}
			else if (t == typeof(Type).GetType())
			{
				_hashFunc = _primitiveHash;
				_eqFunc = _objectEquals;
			}
			else
			{
				PythonType pythonType = DynamicHelpers.GetPythonType(item);
				AssignSiteDelegates(PythonContext.GetHashSite(pythonType), DefaultContext.DefaultPythonContext.GetEqualSite(pythonType));
			}
			_itemType = t;
		}
		else if (_itemType != HeterogeneousType)
		{
			SetHeterogeneousSites();
		}
	}

	private void SetHeterogeneousSites()
	{
		_buckets = (Bucket[])_buckets.Clone();
		AssignSiteDelegates(DefaultContext.DefaultPythonContext.MakeHashSite(), DefaultContext.DefaultPythonContext.MakeEqualSite());
		_itemType = HeterogeneousType;
	}

	private void AssignSiteDelegates(CallSite<Func<CallSite, object, int>> hashSite, CallSite<Func<CallSite, object, object, bool>> equalSite)
	{
		_hashFunc = (object o) => hashSite.Target(hashSite, o);
		_eqFunc = (object o0, object o1) => equalSite.Target(equalSite, o0, o1);
	}

	private int Hash(object item)
	{
		if (item is string)
		{
			return item.GetHashCode();
		}
		return _hashFunc(item);
	}

	private static Func<object, object, bool> GetEqFunc(SetStorage self, SetStorage other)
	{
		if (self._itemType == other._itemType || self._itemType == HeterogeneousType)
		{
			return self._eqFunc;
		}
		if (other._itemType == HeterogeneousType)
		{
			return other._eqFunc;
		}
		return _genericEquals;
	}

	internal static void SortBySize(ref SetStorage x, ref SetStorage y)
	{
		if (x._count > 0 && ((y._count > 0 && x._buckets.Length > y._buckets.Length) || y._count == 0))
		{
			SetStorage setStorage = x;
			x = y;
			y = setStorage;
		}
	}

	internal static SetStorage GetItems(object set)
	{
		if (GetItemsIfSet(set, out var items))
		{
			return items;
		}
		return GetItemsWorker(set);
	}

	internal static bool GetItems(object set, out SetStorage items)
	{
		if (GetItemsIfSet(set, out items))
		{
			return true;
		}
		items = GetItemsWorker(set);
		return false;
	}

	internal static SetStorage GetFrozenItems(object o)
	{
		if (o is FrozenSetCollection frozenSetCollection)
		{
			return frozenSetCollection._items;
		}
		if (o is SetCollection setCollection)
		{
			return setCollection._items.Clone();
		}
		return GetItemsWorker(o);
	}

	internal static SetStorage GetItemsWorker(object set)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(set);
		return GetItemsWorker(enumerator);
	}

	internal static SetStorage GetItemsWorker(IEnumerator en)
	{
		SetStorage setStorage = new SetStorage();
		while (en.MoveNext())
		{
			setStorage.AddNoLock(en.Current);
		}
		return setStorage;
	}

	public static bool GetItemsIfSet(object o, out SetStorage items)
	{
		if (o is FrozenSetCollection frozenSetCollection)
		{
			items = frozenSetCollection._items;
			return true;
		}
		if (o is SetCollection setCollection)
		{
			items = setCollection._items;
			return true;
		}
		items = null;
		return false;
	}

	internal static bool GetHashableSetIfSet(ref object o)
	{
		if (o is SetCollection setCollection)
		{
			if (IsHashable(setCollection))
			{
				return true;
			}
			o = new FrozenSetCollection(setCollection._items.Clone());
			return true;
		}
		return o is FrozenSetCollection;
	}

	private static bool IsHashable(SetCollection set)
	{
		if (set.GetType() == typeof(SetCollection))
		{
			return false;
		}
		PythonType pythonType = DynamicHelpers.GetPythonType(set);
		if (pythonType.TryResolveSlot(DefaultContext.Default, "__hash__", out var slot) && slot.TryGetValue(DefaultContext.Default, set, pythonType, out var value))
		{
			return value != null;
		}
		return false;
	}

	internal static PythonTuple Reduce(SetStorage items, PythonType type)
	{
		PythonTuple pythonTuple = PythonTuple.MakeTuple(items.GetItems());
		return PythonTuple.MakeTuple(type, pythonTuple, null);
	}

	internal static string SetToString(CodeContext context, object set, SetStorage items)
	{
		Type type = set.GetType();
		string value = ((type == typeof(SetCollection)) ? "set" : ((!(type == typeof(FrozenSetCollection))) ? PythonTypeOps.GetName(set) : "frozenset"));
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(value);
		stringBuilder.Append("([");
		string value2 = "";
		if (items._hasNull)
		{
			stringBuilder.Append(value2);
			stringBuilder.Append(PythonOps.Repr(context, null));
			value2 = ", ";
		}
		if (items._count > 0)
		{
			Bucket[] buckets = items._buckets;
			for (int i = 0; i < buckets.Length; i++)
			{
				Bucket bucket = buckets[i];
				if (bucket.Item != null && bucket.Item != Removed)
				{
					stringBuilder.Append(value2);
					stringBuilder.Append(PythonOps.Repr(context, bucket.Item));
					value2 = ", ";
				}
			}
		}
		stringBuilder.Append("])");
		return stringBuilder.ToString();
	}

	private void Grow()
	{
		if (_buckets.Length >= 1073741824)
		{
			throw PythonOps.MemoryError("set has reached its maximum size");
		}
		Bucket[] buckets = new Bucket[_buckets.Length << 1];
		for (int i = 0; i < _buckets.Length; i++)
		{
			Bucket bucket = _buckets[i];
			if (bucket.Item != null && bucket.Item != Removed)
			{
				AddWorker(buckets, bucket.Item, bucket.HashCode, _eqFunc, ref _version);
			}
		}
		_buckets = buckets;
		_maxCount = (int)((double)_buckets.Length * 0.7);
	}

	private static void ProbeNext(Bucket[] buckets, ref int index)
	{
		index++;
		if (index == buckets.Length)
		{
			index = 0;
		}
	}

	private static int CeilLog2(int x)
	{
		int num = x;
		int num2 = 1;
		if (x >= 65536)
		{
			x >>= 16;
			num2 += 16;
		}
		if (x >= 256)
		{
			x >>= 8;
			num2 += 8;
		}
		if (x >= 16)
		{
			x >>= 4;
			num2 += 4;
		}
		if (x >= 4)
		{
			x >>= 2;
			num2 += 2;
		}
		if (x >= 2)
		{
			num2++;
		}
		if (1 << num2 != num)
		{
			return num2;
		}
		return num2 + 1;
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("buckets", GetItems());
		info.AddValue("hasnull", _hasNull);
	}

	void IDeserializationCallback.OnDeserialization(object sender)
	{
		if (_buckets == null || !(_buckets[0].Item is SerializationInfo serializationInfo))
		{
			return;
		}
		_buckets = null;
		List list = (List)serializationInfo.GetValue("buckets", typeof(List));
		foreach (object item in list)
		{
			AddNoLock(item);
		}
		_hasNull = (bool)serializationInfo.GetValue("hasnull", typeof(bool));
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime;

[Serializable]
internal class CommonDictionaryStorage : DictionaryStorage, ISerializable, IDeserializationCallback
{
	protected struct Bucket
	{
		public object Key;

		public object Value;

		public int HashCode;

		public Bucket(int hashCode, object key, object value)
		{
			HashCode = hashCode;
			Key = key;
			Value = value;
		}
	}

	[Serializable]
	private class NullValue
	{
		public object Value;

		public NullValue(object value)
		{
			Value = value;
		}
	}

	private class DeserializationNullValue : NullValue
	{
		public SerializationInfo SerializationInfo => (SerializationInfo)Value;

		public DeserializationNullValue(SerializationInfo info)
			: base(info)
		{
		}
	}

	private const int InitialBucketSize = 7;

	private const int ResizeMultiplier = 3;

	private const double Load = 0.7;

	protected Bucket[] _buckets;

	private int _count;

	private int _version;

	private NullValue _nullValue;

	private Func<object, int> _hashFunc;

	private Func<object, object, bool> _eqFunc;

	private Type _keyType;

	private static readonly Func<object, int> _primitiveHash = PrimitiveHash;

	private static readonly Func<object, int> _doubleHash = DoubleHash;

	private static readonly Func<object, int> _intHash = IntHash;

	private static readonly Func<object, int> _tupleHash = TupleHash;

	private static readonly Func<object, int> _genericHash = GenericHash;

	private static readonly Func<object, object, bool> _intEquals = IntEquals;

	private static readonly Func<object, object, bool> _doubleEquals = DoubleEquals;

	private static readonly Func<object, object, bool> _stringEquals = StringEquals;

	private static readonly Func<object, object, bool> _tupleEquals = TupleEquals;

	private static readonly Func<object, object, bool> _genericEquals = GenericEquals;

	private static readonly Func<object, object, bool> _objectEq = object.ReferenceEquals;

	private static readonly Type HeterogeneousType = typeof(CommonDictionaryStorage);

	private static readonly object _removed = new object();

	public int Version => _version;

	public override int Count
	{
		get
		{
			int num = _count;
			if (_nullValue != null)
			{
				num++;
			}
			return num;
		}
	}

	public CommonDictionaryStorage()
	{
	}

	public CommonDictionaryStorage(int count)
	{
		_buckets = new Bucket[(int)((double)count / 0.7 + 2.0)];
	}

	public CommonDictionaryStorage(object[] items, bool isHomogeneous)
		: this(Math.Max(items.Length / 2, 7))
	{
		PythonType pythonType = DynamicHelpers.GetPythonType(items[1]);
		if (!isHomogeneous)
		{
			for (int i = 1; i < items.Length / 2; i++)
			{
				if (DynamicHelpers.GetPythonType(items[i * 2 + 1]) != pythonType)
				{
					SetHeterogeneousSites();
					pythonType = null;
					break;
				}
			}
		}
		if (pythonType != null)
		{
			UpdateHelperFunctions(pythonType, items[1]);
		}
		for (int j = 0; j < items.Length / 2; j++)
		{
			object obj = items[j * 2 + 1];
			if (obj != null)
			{
				AddOne(obj, items[j * 2]);
			}
			else
			{
				AddNull(items[j * 2]);
			}
		}
	}

	private void AddItems(object[] items)
	{
		for (int i = 0; i < items.Length / 2; i++)
		{
			AddNoLock(items[i * 2 + 1], items[i * 2]);
		}
	}

	private CommonDictionaryStorage(Bucket[] buckets, int count, Type keyType, Func<object, int> hashFunc, Func<object, object, bool> eqFunc, NullValue nullValue)
	{
		_buckets = buckets;
		_count = count;
		_keyType = keyType;
		_hashFunc = hashFunc;
		_eqFunc = eqFunc;
		_nullValue = nullValue;
	}

	private CommonDictionaryStorage(SerializationInfo info, StreamingContext context)
	{
		_nullValue = new DeserializationNullValue(info);
	}

	public override void Add(ref DictionaryStorage storage, object key, object value)
	{
		Add(key, value);
	}

	public void Add(object key, object value)
	{
		lock (this)
		{
			AddNoLock(key, value);
		}
	}

	private void AddNull(object value)
	{
		if (_nullValue != null)
		{
			_nullValue.Value = value;
		}
		else
		{
			_nullValue = new NullValue(value);
		}
	}

	public override void AddNoLock(ref DictionaryStorage storage, object key, object value)
	{
		AddNoLock(key, value);
	}

	public void AddNoLock(object key, object value)
	{
		if (key != null)
		{
			if (_buckets == null)
			{
				Initialize();
			}
			if (key.GetType() != _keyType && _keyType != HeterogeneousType)
			{
				UpdateHelperFunctions(key.GetType(), key);
			}
			AddOne(key, value);
		}
		else
		{
			AddNull(value);
		}
	}

	private void AddOne(object key, object value)
	{
		if (Add(_buckets, key, value))
		{
			_count++;
			if ((double)_count >= (double)_buckets.Length * 0.7)
			{
				EnsureSize((int)((double)_buckets.Length / 0.7) * 3);
			}
		}
	}

	private void UpdateHelperFunctions(Type t, object key)
	{
		if (_keyType == null)
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
				_eqFunc = _objectEq;
			}
			else
			{
				PythonType pythonType = DynamicHelpers.GetPythonType(key);
				CallSite<Func<CallSite, object, int>> hashSite = PythonContext.GetHashSite(pythonType);
				CallSite<Func<CallSite, object, object, bool>> equalSite = DefaultContext.DefaultPythonContext.GetEqualSite(pythonType);
				AssignSiteDelegates(hashSite, equalSite);
			}
			_keyType = t;
		}
		else if (_keyType != HeterogeneousType)
		{
			SetHeterogeneousSites();
			_buckets = (Bucket[])_buckets.Clone();
		}
	}

	private void SetHeterogeneousSites()
	{
		CallSite<Func<CallSite, object, int>> hashSite = DefaultContext.DefaultPythonContext.MakeHashSite();
		CallSite<Func<CallSite, object, object, bool>> equalSite = DefaultContext.DefaultPythonContext.MakeEqualSite();
		AssignSiteDelegates(hashSite, equalSite);
		_keyType = HeterogeneousType;
	}

	private void AssignSiteDelegates(CallSite<Func<CallSite, object, int>> hashSite, CallSite<Func<CallSite, object, object, bool>> equalSite)
	{
		_hashFunc = (object o) => hashSite.Target(hashSite, o);
		_eqFunc = (object o1, object o2) => equalSite.Target(equalSite, o1, o2);
	}

	private void EnsureSize(int newSize)
	{
		if (_buckets.Length >= newSize)
		{
			return;
		}
		Bucket[] buckets = _buckets;
		Bucket[] buckets2 = new Bucket[newSize];
		for (int i = 0; i < buckets.Length; i++)
		{
			Bucket bucket = buckets[i];
			if (bucket.Key != null && bucket.Key != _removed)
			{
				AddWorker(buckets2, bucket.Key, bucket.Value, bucket.HashCode);
			}
		}
		_buckets = buckets2;
	}

	public override void EnsureCapacityNoLock(int size)
	{
		if (_buckets == null)
		{
			_buckets = new Bucket[(int)((double)size / 0.7) + 1];
		}
		else
		{
			EnsureSize((int)((double)size / 0.7));
		}
	}

	private void Initialize()
	{
		_buckets = new Bucket[7];
	}

	private bool Add(Bucket[] buckets, object key, object value)
	{
		int hc = Hash(key);
		return AddWorker(buckets, key, value, hc);
	}

	protected bool AddWorker(Bucket[] buckets, object key, object value, int hc)
	{
		int num = hc % buckets.Length;
		int num2 = num;
		int num3 = -1;
		do
		{
			Bucket bucket = buckets[num2];
			if (bucket.Key == null)
			{
				if (num3 == -1)
				{
					num3 = num2;
				}
				break;
			}
			if (bucket.Key == _removed)
			{
				if (num3 == -1)
				{
					num3 = num2;
				}
			}
			else if (object.ReferenceEquals(key, bucket.Key) || (bucket.HashCode == hc && _eqFunc(key, bucket.Key)))
			{
				_version++;
				buckets[num2].Value = value;
				return false;
			}
			num2 = ProbeNext(buckets, num2);
		}
		while (num2 != num);
		_version++;
		buckets[num3].HashCode = hc;
		buckets[num3].Value = value;
		buckets[num3].Key = key;
		return true;
	}

	private static int ProbeNext(Bucket[] buckets, int index)
	{
		index++;
		if (index == buckets.Length)
		{
			index = 0;
		}
		return index;
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		return Remove(key);
	}

	public bool Remove(object key)
	{
		object value;
		return TryRemoveValue(key, out value);
	}

	internal bool RemoveAlwaysHash(object key)
	{
		lock (this)
		{
			object value;
			if (key == null)
			{
				return TryRemoveNull(out value);
			}
			return TryRemoveNoLock(key, out value);
		}
	}

	public override bool TryRemoveValue(ref DictionaryStorage storage, object key, out object value)
	{
		return TryRemoveValue(key, out value);
	}

	public bool TryRemoveValue(object key, out object value)
	{
		lock (this)
		{
			if (key == null)
			{
				return TryRemoveNull(out value);
			}
			if (_count == 0)
			{
				value = null;
				return false;
			}
			return TryRemoveNoLock(key, out value);
		}
	}

	private bool TryRemoveNull(out object value)
	{
		if (_nullValue != null)
		{
			value = _nullValue.Value;
			_nullValue = null;
			return true;
		}
		value = null;
		return false;
	}

	private bool TryRemoveNoLock(object key, out object value)
	{
		Func<object, int> func;
		Func<object, object, bool> eqFunc;
		if (key.GetType() == _keyType || _keyType == HeterogeneousType)
		{
			func = _hashFunc;
			eqFunc = _eqFunc;
		}
		else
		{
			func = _genericHash;
			eqFunc = _genericEquals;
		}
		int hc = func(key) & 0x7FFFFFFF;
		return TryRemoveNoLock(key, eqFunc, hc, out value);
	}

	protected bool TryRemoveNoLock(object key, Func<object, object, bool> eqFunc, int hc, out object value)
	{
		if (_buckets == null)
		{
			value = null;
			return false;
		}
		int num = hc % _buckets.Length;
		int num2 = num;
		do
		{
			Bucket bucket = _buckets[num];
			if (bucket.Key == null)
			{
				break;
			}
			if (object.ReferenceEquals(key, bucket.Key) || (bucket.Key != _removed && bucket.HashCode == hc && eqFunc(key, bucket.Key)))
			{
				value = bucket.Value;
				_version++;
				_buckets[num].Key = _removed;
				Thread.MemoryBarrier();
				_buckets[num].Value = null;
				_count--;
				return true;
			}
			num = ProbeNext(_buckets, num);
		}
		while (num != num2);
		value = null;
		return false;
	}

	public override bool Contains(object key)
	{
		object value;
		return TryGetValue(key, out value);
	}

	public override bool TryGetValue(object key, out object value)
	{
		if (key != null)
		{
			return TryGetValue(_buckets, key, out value);
		}
		NullValue nullValue = _nullValue;
		if (nullValue != null)
		{
			value = nullValue.Value;
			return true;
		}
		value = null;
		return false;
	}

	private bool TryGetValue(Bucket[] buckets, object key, out object value)
	{
		if (_count > 0 && buckets != null)
		{
			int hc;
			Func<object, object, bool> eqFunc;
			if (key.GetType() == _keyType || _keyType == HeterogeneousType)
			{
				hc = _hashFunc(key) & 0x7FFFFFFF;
				eqFunc = _eqFunc;
			}
			else
			{
				hc = _genericHash(key) & 0x7FFFFFFF;
				eqFunc = _genericEquals;
			}
			return TryGetValue(buckets, key, hc, eqFunc, out value);
		}
		value = null;
		return false;
	}

	protected static bool TryGetValue(Bucket[] buckets, object key, int hc, Func<object, object, bool> eqFunc, out object value)
	{
		int num = hc % buckets.Length;
		int num2 = num;
		do
		{
			Bucket bucket = buckets[num];
			if (bucket.Key == null)
			{
				break;
			}
			if (object.ReferenceEquals(key, bucket.Key) || (bucket.Key != _removed && bucket.HashCode == hc && eqFunc(key, bucket.Key)))
			{
				value = bucket.Value;
				return true;
			}
			num = ProbeNext(buckets, num);
		}
		while (num2 != num);
		value = null;
		return false;
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		Clear();
	}

	public void Clear()
	{
		lock (this)
		{
			if (_buckets != null)
			{
				_version++;
				_buckets = new Bucket[8];
				_count = 0;
			}
			_nullValue = null;
		}
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		lock (this)
		{
			List<KeyValuePair<object, object>> list = new List<KeyValuePair<object, object>>(_count + ((_nullValue != null) ? 1 : 0));
			if (_count > 0)
			{
				for (int i = 0; i < _buckets.Length; i++)
				{
					Bucket bucket = _buckets[i];
					if (bucket.Key != null && bucket.Key != _removed)
					{
						list.Add(new KeyValuePair<object, object>(bucket.Key, bucket.Value));
					}
				}
			}
			if (_nullValue != null)
			{
				list.Add(new KeyValuePair<object, object>(null, _nullValue.Value));
			}
			return list;
		}
	}

	public override IEnumerator<KeyValuePair<object, object>> GetEnumerator()
	{
		lock (this)
		{
			if (_count > 0)
			{
				for (int i = 0; i < _buckets.Length; i++)
				{
					Bucket curBucket = _buckets[i];
					if (curBucket.Key != null && curBucket.Key != _removed)
					{
						yield return new KeyValuePair<object, object>(curBucket.Key, curBucket.Value);
					}
				}
			}
			if (_nullValue != null)
			{
				yield return new KeyValuePair<object, object>(null, _nullValue.Value);
			}
		}
	}

	public override IEnumerable<object> GetKeys()
	{
		Bucket[] buckets = _buckets;
		lock (this)
		{
			object[] array = new object[Count];
			int num = 0;
			if (buckets != null)
			{
				for (int i = 0; i < buckets.Length; i++)
				{
					Bucket bucket = buckets[i];
					if (bucket.Key != null && bucket.Key != _removed)
					{
						array[num++] = bucket.Key;
					}
				}
			}
			if (_nullValue != null)
			{
				array[num++] = null;
			}
			return array;
		}
	}

	public override bool HasNonStringAttributes()
	{
		lock (this)
		{
			NullValue nullValue = _nullValue;
			if (nullValue != null && !(nullValue.Value is string))
			{
				return true;
			}
			if (_keyType != typeof(string) && _keyType != null && _count > 0)
			{
				for (int i = 0; i < _buckets.Length; i++)
				{
					Bucket bucket = _buckets[i];
					if (bucket.Key != null && !(bucket.Key is string))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public override DictionaryStorage Clone()
	{
		lock (this)
		{
			if (_buckets == null)
			{
				if (_nullValue != null)
				{
					return new CommonDictionaryStorage(null, 1, _keyType, _hashFunc, _eqFunc, new NullValue(_nullValue.Value));
				}
				return new CommonDictionaryStorage();
			}
			Bucket[] array = new Bucket[_buckets.Length];
			for (int i = 0; i < _buckets.Length; i++)
			{
				if (_buckets[i].Key != null)
				{
					ref Bucket reference = ref array[i];
					reference = _buckets[i];
				}
			}
			NullValue nullValue = null;
			if (_nullValue != null)
			{
				nullValue = new NullValue(_nullValue.Value);
			}
			return new CommonDictionaryStorage(array, _count, _keyType, _hashFunc, _eqFunc, nullValue);
		}
	}

	public override void CopyTo(ref DictionaryStorage into)
	{
		into = CopyTo(into);
	}

	public DictionaryStorage CopyTo(DictionaryStorage into)
	{
		if (_buckets != null)
		{
			using (new OrderedLocker(this, into))
			{
				if (into is CommonDictionaryStorage commonDictionaryStorage)
				{
					CommonCopyTo(commonDictionaryStorage);
				}
				else
				{
					UncommonCopyTo(ref into);
				}
			}
		}
		NullValue nullValue = _nullValue;
		if (nullValue != null)
		{
			into.Add(ref into, null, nullValue.Value);
		}
		return into;
	}

	private void CommonCopyTo(CommonDictionaryStorage into)
	{
		if (into._buckets == null)
		{
			into._buckets = new Bucket[_buckets.Length];
		}
		else
		{
			int num = into._buckets.Length;
			int num2 = (int)((double)(_count + into._count) / 0.7) + 2;
			while (num < num2)
			{
				num *= 3;
			}
			into.EnsureSize(num);
		}
		if (into._keyType == null)
		{
			into._keyType = _keyType;
			into._hashFunc = _hashFunc;
			into._eqFunc = _eqFunc;
		}
		else if (into._keyType != _keyType)
		{
			into.SetHeterogeneousSites();
		}
		for (int i = 0; i < _buckets.Length; i++)
		{
			Bucket bucket = _buckets[i];
			if (bucket.Key != null && bucket.Key != _removed && into.AddWorker(into._buckets, bucket.Key, bucket.Value, bucket.HashCode))
			{
				into._count++;
			}
		}
	}

	private void UncommonCopyTo(ref DictionaryStorage into)
	{
		for (int i = 0; i < _buckets.Length; i++)
		{
			Bucket bucket = _buckets[i];
			if (bucket.Key != null && bucket.Key != _removed)
			{
				into.AddNoLock(ref into, bucket.Key, bucket.Value);
			}
		}
	}

	private int Hash(object key)
	{
		if (key is string)
		{
			return key.GetHashCode() & 0x7FFFFFFF;
		}
		return _hashFunc(key) & 0x7FFFFFFF;
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

	private static int GenericHash(object o)
	{
		return PythonOps.Hash(DefaultContext.Default, o);
	}

	private static int TupleHash(object o)
	{
		return ((IStructuralEquatable)o).GetHashCode(DefaultContext.DefaultPythonContext.EqualityComparerNonGeneric);
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
		return PythonOps.EqualRetBool(o1, o2);
	}

	private DeserializationNullValue GetDeserializationBucket()
	{
		return _nullValue as DeserializationNullValue;
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("buckets", GetItems());
		info.AddValue("nullvalue", _nullValue);
	}

	void IDeserializationCallback.OnDeserialization(object sender)
	{
		DeserializationNullValue deserializationBucket = GetDeserializationBucket();
		if (deserializationBucket == null)
		{
			return;
		}
		SerializationInfo serializationInfo = deserializationBucket.SerializationInfo;
		_buckets = null;
		_nullValue = null;
		List<KeyValuePair<object, object>> list = (List<KeyValuePair<object, object>>)serializationInfo.GetValue("buckets", typeof(List<KeyValuePair<object, object>>));
		foreach (KeyValuePair<object, object> item in list)
		{
			Add(item.Key, item.Value);
		}
		NullValue nullValue = (NullValue)serializationInfo.GetValue("nullvalue", typeof(NullValue));
		if (nullValue != null)
		{
			_nullValue = new NullValue(nullValue);
		}
	}
}

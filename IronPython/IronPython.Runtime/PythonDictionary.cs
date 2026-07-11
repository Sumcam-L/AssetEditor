using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[Serializable]
[PythonType("dict")]
[DebuggerTypeProxy(typeof(DebugProxy))]
[DebuggerDisplay("dict, {Count} items")]
public class PythonDictionary : IDictionary<object, object>, ICollection<KeyValuePair<object, object>>, IEnumerable<KeyValuePair<object, object>>, IDictionary, ICollection, IEnumerable, ICodeFormattable, IStructuralEquatable
{
	internal class DictEnumerator : IDictionaryEnumerator, IEnumerator
	{
		private IEnumerator<KeyValuePair<object, object>> _enumerator;

		private bool _moved;

		public DictionaryEntry Entry
		{
			get
			{
				if (!_moved)
				{
					throw new InvalidOperationException();
				}
				return new DictionaryEntry(_enumerator.Current.Key, _enumerator.Current.Value);
			}
		}

		public object Key => Entry.Key;

		public object Value => Entry.Value;

		public object Current => Entry;

		public DictEnumerator(IEnumerator<KeyValuePair<object, object>> enumerator)
		{
			_enumerator = enumerator;
		}

		public bool MoveNext()
		{
			if (_enumerator.MoveNext())
			{
				_moved = true;
				return true;
			}
			_moved = false;
			return false;
		}

		public void Reset()
		{
			_enumerator.Reset();
			_moved = false;
		}
	}

	internal class DebugProxy
	{
		private readonly PythonDictionary _dict;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public List<KeyValueDebugView> Members
		{
			get
			{
				List<KeyValueDebugView> list = new List<KeyValueDebugView>();
				foreach (KeyValuePair<object, object> item in _dict)
				{
					list.Add(new KeyValueDebugView(item.Key, item.Value));
				}
				return list;
			}
		}

		public DebugProxy(PythonDictionary dict)
		{
			_dict = dict;
		}
	}

	[DebuggerDisplay("{Value}", Name = "{Key,nq}", Type = "{TypeInfo,nq}")]
	internal class KeyValueDebugView
	{
		public readonly object Key;

		public readonly object Value;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public string TypeInfo => "Key: " + PythonTypeOps.GetName(Key) + ", Value: " + PythonTypeOps.GetName(Value);

		public KeyValueDebugView(object key, object value)
		{
			Key = key;
			Value = value;
		}
	}

	public const object __hash__ = null;

	internal DictionaryStorage _storage;

	public ICollection<object> Keys
	{
		[PythonHidden]
		get
		{
			return keys();
		}
	}

	public ICollection<object> Values
	{
		[PythonHidden]
		get
		{
			return values();
		}
	}

	public int Count
	{
		[PythonHidden]
		get
		{
			return _storage.Count;
		}
	}

	bool ICollection<KeyValuePair<object, object>>.IsReadOnly => false;

	public virtual object this[params object[] key]
	{
		get
		{
			if (key == null)
			{
				return GetItem(null);
			}
			if (key.Length == 0)
			{
				throw PythonOps.TypeError("__getitem__() takes exactly one argument (0 given)");
			}
			return this[PythonTuple.MakeTuple(key)];
		}
		set
		{
			if (key == null)
			{
				SetItem(null, value);
				return;
			}
			if (key.Length == 0)
			{
				throw PythonOps.TypeError("__setitem__() takes exactly two argument (1 given)");
			}
			this[PythonTuple.MakeTuple(key)] = value;
		}
	}

	public virtual object this[object key]
	{
		get
		{
			return GetItem(key);
		}
		set
		{
			SetItem(key, value);
		}
	}

	bool IDictionary.IsFixedSize => false;

	bool IDictionary.IsReadOnly => false;

	ICollection IDictionary.Keys => keys();

	ICollection IDictionary.Values => values();

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => null;

	internal static object MakeDict(CodeContext context, PythonType cls)
	{
		if (cls == TypeCache.Dict)
		{
			return new PythonDictionary();
		}
		return PythonCalls.Call(context, cls);
	}

	public PythonDictionary()
	{
		_storage = EmptyDictionaryStorage.Instance;
	}

	internal PythonDictionary(DictionaryStorage storage)
	{
		_storage = storage;
	}

	internal PythonDictionary(IDictionary dict)
	{
		CommonDictionaryStorage commonDictionaryStorage = new CommonDictionaryStorage();
		foreach (DictionaryEntry item in dict)
		{
			commonDictionaryStorage.AddNoLock(item.Key, item.Value);
		}
		_storage = commonDictionaryStorage;
	}

	internal PythonDictionary(PythonDictionary dict)
	{
		_storage = dict._storage.Clone();
	}

	internal PythonDictionary(CodeContext context, object o)
		: this()
	{
		update(context, o);
	}

	internal PythonDictionary(int size)
	{
		_storage = new CommonDictionaryStorage(size);
	}

	internal static PythonDictionary FromIAC(CodeContext context, PythonDictionary iac)
	{
		if (!(iac.GetType() == typeof(PythonDictionary)))
		{
			return MakeDictFromIAC(context, iac);
		}
		return iac;
	}

	private static PythonDictionary MakeDictFromIAC(CodeContext context, PythonDictionary iac)
	{
		return new PythonDictionary(new ObjectAttributesAdapter(context, iac));
	}

	internal static PythonDictionary MakeSymbolDictionary()
	{
		return new PythonDictionary(new StringDictionaryStorage());
	}

	internal static PythonDictionary MakeSymbolDictionary(int count)
	{
		return new PythonDictionary(new StringDictionaryStorage(count));
	}

	public void __init__(CodeContext context, object oø, [ParamDictionary] IDictionary<object, object> kwArgs)
	{
		update(context, oø);
		update(context, kwArgs);
	}

	public void __init__(CodeContext context, [ParamDictionary] IDictionary<object, object> kwArgs)
	{
		update(context, kwArgs);
	}

	public void __init__(CodeContext context, object oø)
	{
		update(context, oø);
	}

	public void __init__()
	{
	}

	[PythonHidden]
	public void Add(object key, object value)
	{
		_storage.Add(ref _storage, key, value);
	}

	[PythonHidden]
	public bool ContainsKey(object key)
	{
		return _storage.Contains(key);
	}

	[PythonHidden]
	public bool Remove(object key)
	{
		try
		{
			__delitem__(key);
			return true;
		}
		catch (KeyNotFoundException)
		{
			return false;
		}
	}

	[PythonHidden]
	public bool TryGetValue(object key, out object value)
	{
		if (_storage.TryGetValue(key, out value))
		{
			return true;
		}
		if (GetType() != typeof(PythonDictionary) && PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default, this, key, "__missing__", out value))
		{
			return true;
		}
		return false;
	}

	internal bool TryGetValueNoMissing(object key, out object value)
	{
		return _storage.TryGetValue(key, out value);
	}

	[PythonHidden]
	public void Add(KeyValuePair<object, object> item)
	{
		_storage.Add(ref _storage, item.Key, item.Value);
	}

	[PythonHidden]
	public void Clear()
	{
		_storage.Clear(ref _storage);
	}

	[PythonHidden]
	public bool Contains(KeyValuePair<object, object> item)
	{
		return _storage.Contains(item.Key);
	}

	[PythonHidden]
	public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
	{
		_storage.GetItems().CopyTo(array, arrayIndex);
	}

	[PythonHidden]
	public bool Remove(KeyValuePair<object, object> item)
	{
		return _storage.Remove(ref _storage, item.Key);
	}

	[PythonHidden]
	public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
	{
		foreach (KeyValuePair<object, object> item in _storage.GetItems())
		{
			yield return item;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Converter.ConvertToIEnumerator(__iter__());
	}

	public virtual object __iter__()
	{
		return new DictionaryKeyEnumerator(_storage);
	}

	public object get(object key)
	{
		return DictionaryOps.get(this, key);
	}

	public object get(object key, object defaultValue)
	{
		return DictionaryOps.get(this, key, defaultValue);
	}

	private void SetItem(object key, object value)
	{
		_storage.Add(ref _storage, key, value);
	}

	private object GetItem(object key)
	{
		if (TryGetValue(key, out var value))
		{
			return value;
		}
		throw PythonOps.KeyError(key);
	}

	public virtual void __delitem__(object key)
	{
		if (!_storage.Remove(ref _storage, key))
		{
			throw PythonOps.KeyError(key);
		}
	}

	public virtual void __delitem__(params object[] key)
	{
		if (key == null)
		{
			__delitem__((object)null);
			return;
		}
		if (key.Length > 0)
		{
			__delitem__(PythonTuple.MakeTuple(key));
			return;
		}
		throw PythonOps.TypeError("__delitem__() takes exactly one argument (0 given)");
	}

	public virtual int __len__()
	{
		return Count;
	}

	public void clear()
	{
		_storage.Clear(ref _storage);
	}

	public bool has_key(object key)
	{
		return DictionaryOps.has_key(this, key);
	}

	public object pop(object key)
	{
		return DictionaryOps.pop(this, key);
	}

	public object pop(object key, object defaultValue)
	{
		return DictionaryOps.pop(this, key, defaultValue);
	}

	public PythonTuple popitem()
	{
		return DictionaryOps.popitem(this);
	}

	public object setdefault(object key)
	{
		return DictionaryOps.setdefault(this, key);
	}

	public object setdefault(object key, object defaultValue)
	{
		return DictionaryOps.setdefault(this, key, defaultValue);
	}

	public virtual List keys()
	{
		List list = new List();
		foreach (KeyValuePair<object, object> item in _storage.GetItems())
		{
			list.append(item.Key);
		}
		return list;
	}

	public virtual List values()
	{
		List list = new List();
		foreach (KeyValuePair<object, object> item in _storage.GetItems())
		{
			list.append(item.Value);
		}
		return list;
	}

	public virtual List items()
	{
		List list = new List();
		foreach (KeyValuePair<object, object> item in _storage.GetItems())
		{
			list.append(PythonTuple.MakeTuple(item.Key, item.Value));
		}
		return list;
	}

	public IEnumerator iteritems()
	{
		return new DictionaryItemEnumerator(_storage);
	}

	public IEnumerator iterkeys()
	{
		return new DictionaryKeyEnumerator(_storage);
	}

	public IEnumerator itervalues()
	{
		return new DictionaryValueEnumerator(_storage);
	}

	public IEnumerable viewitems()
	{
		return new DictionaryItemView(this);
	}

	public IEnumerable viewkeys()
	{
		return new DictionaryKeyView(this);
	}

	public IEnumerable viewvalues()
	{
		return new DictionaryValueView(this);
	}

	public void update()
	{
	}

	public void update(CodeContext context, [ParamDictionary] IDictionary<object, object> b)
	{
		DictionaryOps.update(context, this, b);
	}

	public void update(CodeContext context, object b)
	{
		DictionaryOps.update(context, this, b);
	}

	public void update(CodeContext context, object b, [ParamDictionary] IDictionary<object, object> f)
	{
		DictionaryOps.update(context, this, b);
		DictionaryOps.update(context, this, f);
	}

	private static object fromkeysAny(CodeContext context, PythonType cls, object o, object value)
	{
		if (cls == TypeCache.Dict)
		{
			PythonDictionary pythonDictionary = ((!(o is ICollection collection)) ? ((!(o is string text)) ? new PythonDictionary() : new PythonDictionary(text.Length)) : new PythonDictionary(new CommonDictionaryStorage(collection.Count)));
			IEnumerator enumerator = PythonOps.GetEnumerator(o);
			while (enumerator.MoveNext())
			{
				pythonDictionary._storage.AddNoLock(ref pythonDictionary._storage, enumerator.Current, value);
			}
			return pythonDictionary;
		}
		object obj = MakeDict(context, cls);
		if (obj is PythonDictionary pythonDictionary2)
		{
			IEnumerator enumerator2 = PythonOps.GetEnumerator(o);
			while (enumerator2.MoveNext())
			{
				pythonDictionary2[enumerator2.Current] = value;
			}
		}
		else
		{
			PythonContext context2 = PythonContext.GetContext(context);
			IEnumerator enumerator3 = PythonOps.GetEnumerator(o);
			while (enumerator3.MoveNext())
			{
				context2.SetIndex(obj, enumerator3.Current, value);
			}
		}
		return obj;
	}

	[ClassMethod]
	public static object fromkeys(CodeContext context, PythonType cls, object seq)
	{
		return fromkeys(context, cls, seq, null);
	}

	[ClassMethod]
	public static object fromkeys(CodeContext context, PythonType cls, object seq, object value)
	{
		if (seq is XRange xRange)
		{
			int num = xRange.__len__();
			object obj = PythonContext.GetContext(context).CallSplat(cls);
			if (obj.GetType() == typeof(PythonDictionary))
			{
				PythonDictionary pythonDictionary = obj as PythonDictionary;
				for (int i = 0; i < num; i++)
				{
					pythonDictionary[xRange[i]] = value;
				}
			}
			else
			{
				PythonContext context2 = PythonContext.GetContext(context);
				for (int j = 0; j < num; j++)
				{
					context2.SetIndex(obj, xRange[j], value);
				}
			}
			return obj;
		}
		return fromkeysAny(context, cls, seq, value);
	}

	public virtual PythonDictionary copy(CodeContext context)
	{
		return new PythonDictionary(_storage.Clone());
	}

	public virtual bool __contains__(object key)
	{
		return _storage.Contains(key);
	}

	[return: MaybeNotImplemented]
	public object __eq__(CodeContext context, object other)
	{
		if (!(other is PythonDictionary) && !(other is IDictionary<object, object>))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(((IStructuralEquatable)this).Equals(other, PythonContext.GetContext(context).EqualityComparerNonGeneric));
	}

	[return: MaybeNotImplemented]
	public object __ne__(CodeContext context, object other)
	{
		if (!(other is PythonDictionary) && !(other is IDictionary<object, object>))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(!((IStructuralEquatable)this).Equals(other, PythonContext.GetContext(context).EqualityComparerNonGeneric));
	}

	[return: MaybeNotImplemented]
	public object __cmp__(CodeContext context, object other)
	{
		if (!(other is IDictionary<object, object> dictionary))
		{
			if (!PythonOps.TryGetBoundAttr(context, other, "__len__", out var ret) || !PythonOps.TryGetBoundAttr(context, other, "iteritems", out var ret2))
			{
				return NotImplementedType.Value;
			}
			int count = Count;
			int num = PythonContext.GetContext(context).ConvertToInt32(PythonOps.CallWithContext(context, ret));
			if (count != num)
			{
				return (count > num) ? 1 : (-1);
			}
			return DictionaryOps.CompareToWorker(context, this, new List(PythonOps.CallWithContext(context, ret2)));
		}
		CompareUtil.Push(this, dictionary);
		try
		{
			return DictionaryOps.CompareTo(context, this, dictionary);
		}
		finally
		{
			CompareUtil.Pop(this, dictionary);
		}
	}

	public int __cmp__(CodeContext context, [NotNull] PythonDictionary other)
	{
		CompareUtil.Push(this, other);
		try
		{
			return DictionaryOps.CompareTo(context, this, other);
		}
		finally
		{
			CompareUtil.Pop(this, other);
		}
	}

	[Python3Warning("dict inequality comparisons not supported in 3.x")]
	[return: MaybeNotImplemented]
	public static NotImplementedType operator >(PythonDictionary self, PythonDictionary other)
	{
		return PythonOps.NotImplemented;
	}

	[Python3Warning("dict inequality comparisons not supported in 3.x")]
	[return: MaybeNotImplemented]
	public static NotImplementedType operator <(PythonDictionary self, PythonDictionary other)
	{
		return PythonOps.NotImplemented;
	}

	[Python3Warning("dict inequality comparisons not supported in 3.x")]
	[return: MaybeNotImplemented]
	public static NotImplementedType operator >=(PythonDictionary self, PythonDictionary other)
	{
		return PythonOps.NotImplemented;
	}

	[Python3Warning("dict inequality comparisons not supported in 3.x")]
	[return: MaybeNotImplemented]
	public static NotImplementedType operator <=(PythonDictionary self, PythonDictionary other)
	{
		return PythonOps.NotImplemented;
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		if (CompareUtil.Check(this))
		{
			return 0;
		}
		SetStorage setStorage = new SetStorage();
		foreach (KeyValuePair<object, object> item in _storage.GetItems())
		{
			setStorage.AddNoLock(PythonTuple.MakeTuple(item.Key, item.Value));
		}
		CompareUtil.Push(this);
		try
		{
			IStructuralEquatable structuralEquatable = FrozenSetCollection.Make(TypeCache.FrozenSet, setStorage);
			return structuralEquatable.GetHashCode(comparer);
		}
		finally
		{
			CompareUtil.Pop(this);
		}
	}

	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		return EqualsWorker(other, comparer);
	}

	private bool EqualsWorker(object other, IEqualityComparer comparer)
	{
		if (object.ReferenceEquals(this, other))
		{
			return true;
		}
		if (!(other is IDictionary<object, object> dictionary))
		{
			return false;
		}
		if (dictionary.Count != Count)
		{
			return false;
		}
		if (other is PythonDictionary pd)
		{
			return ValueEqualsPythonDict(pd, comparer);
		}
		List list = keys();
		foreach (object item in list)
		{
			if (!dictionary.TryGetValue(item, out var value))
			{
				return false;
			}
			CompareUtil.Push(value);
			try
			{
				if (comparer == null)
				{
					if (!PythonOps.EqualRetBool(value, this[item]))
					{
						return false;
					}
				}
				else if (!comparer.Equals(value, this[item]))
				{
					return false;
				}
			}
			finally
			{
				CompareUtil.Pop(value);
			}
		}
		return true;
	}

	private bool ValueEqualsPythonDict(PythonDictionary pd, IEqualityComparer comparer)
	{
		List list = keys();
		foreach (object item in list)
		{
			if (!pd.TryGetValueNoMissing(item, out var value))
			{
				return false;
			}
			CompareUtil.Push(value);
			try
			{
				if (comparer == null)
				{
					if (!PythonOps.EqualRetBool(value, this[item]))
					{
						return false;
					}
				}
				else if (!comparer.Equals(value, this[item]))
				{
					return false;
				}
			}
			finally
			{
				CompareUtil.Pop(value);
			}
		}
		return true;
	}

	[PythonHidden]
	public bool Contains(object key)
	{
		return __contains__(key);
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new DictEnumerator(_storage.GetItems().GetEnumerator());
	}

	void IDictionary.Remove(object key)
	{
		((IDictionary<object, object>)this).Remove(key);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public virtual string __repr__(CodeContext context)
	{
		return DictionaryOps.__repr__(context, this);
	}

	internal bool TryRemoveValue(object key, out object value)
	{
		return _storage.TryRemoveValue(ref _storage, key, out value);
	}
}

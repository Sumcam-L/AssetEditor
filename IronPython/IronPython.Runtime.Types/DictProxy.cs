using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types;

[PythonType("dictproxy")]
public class DictProxy : IDictionary, ICollection, IDictionary<object, object>, ICollection<KeyValuePair<object, object>>, IEnumerable<KeyValuePair<object, object>>, IEnumerable
{
	private readonly PythonType _dt;

	public object this[object key]
	{
		get
		{
			return GetIndex(DefaultContext.Default, key);
		}
		[PythonHidden]
		set
		{
			throw PythonOps.TypeError("cannot assign to dictproxy");
		}
	}

	bool IDictionary.IsFixedSize => true;

	bool IDictionary.IsReadOnly => true;

	ICollection IDictionary.Keys
	{
		get
		{
			ICollection<object> collection = _dt.GetMemberDictionary(DefaultContext.Default, excludeDict: false).Keys;
			if (collection is ICollection result)
			{
				return result;
			}
			return new List<object>(collection);
		}
	}

	ICollection IDictionary.Values
	{
		get
		{
			List<object> list = new List<object>();
			foreach (KeyValuePair<object, object> item in _dt.GetMemberDictionary(DefaultContext.Default, excludeDict: false))
			{
				list.Add(item.Value);
			}
			return list;
		}
	}

	int ICollection.Count => __len__(DefaultContext.Default);

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	ICollection<object> IDictionary<object, object>.Keys => _dt.GetMemberDictionary(DefaultContext.Default, excludeDict: false).Keys;

	ICollection<object> IDictionary<object, object>.Values => _dt.GetMemberDictionary(DefaultContext.Default, excludeDict: false).Values;

	int ICollection<KeyValuePair<object, object>>.Count => __len__(DefaultContext.Default);

	bool ICollection<KeyValuePair<object, object>>.IsReadOnly => true;

	internal PythonType Type => _dt;

	public DictProxy(PythonType dt)
	{
		_dt = dt;
	}

	public int __len__(CodeContext context)
	{
		return _dt.GetMemberDictionary(context, excludeDict: false).Count;
	}

	public bool __contains__(CodeContext context, object value)
	{
		return has_key(context, value);
	}

	public string __str__(CodeContext context)
	{
		return DictionaryOps.__repr__(context, this);
	}

	public bool has_key(CodeContext context, object key)
	{
		object value;
		return TryGetValue(context, key, out value);
	}

	public object get(CodeContext context, [NotNull] object k, [DefaultParameterValue(null)] object d)
	{
		if (!TryGetValue(context, k, out var value))
		{
			return d;
		}
		return value;
	}

	public object keys(CodeContext context)
	{
		return new List(_dt.GetMemberDictionary(context, excludeDict: false).Keys);
	}

	public object values(CodeContext context)
	{
		List list = new List();
		foreach (KeyValuePair<object, object> item in _dt.GetMemberDictionary(context, excludeDict: false))
		{
			if (item.Value is PythonTypeUserDescriptorSlot pythonTypeUserDescriptorSlot)
			{
				list.AddNoLock(pythonTypeUserDescriptorSlot.Value);
			}
			else
			{
				list.AddNoLock(item.Value);
			}
		}
		return list;
	}

	public List items(CodeContext context)
	{
		List list = new List();
		foreach (KeyValuePair<object, object> item in _dt.GetMemberDictionary(context, excludeDict: false))
		{
			object obj = ((!(item.Value is PythonTypeUserDescriptorSlot pythonTypeUserDescriptorSlot)) ? item.Value : pythonTypeUserDescriptorSlot.Value);
			list.append(PythonTuple.MakeTuple(item.Key, obj));
		}
		return list;
	}

	public PythonDictionary copy(CodeContext context)
	{
		return new PythonDictionary(context, this);
	}

	public IEnumerator iteritems(CodeContext context)
	{
		return new DictionaryItemEnumerator(_dt.GetMemberDictionary(context, excludeDict: false)._storage);
	}

	public IEnumerator iterkeys(CodeContext context)
	{
		return new DictionaryKeyEnumerator(_dt.GetMemberDictionary(context, excludeDict: false)._storage);
	}

	public IEnumerator itervalues(CodeContext context)
	{
		return new DictionaryValueEnumerator(_dt.GetMemberDictionary(context, excludeDict: false)._storage);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is DictProxy dictProxy))
		{
			return false;
		}
		return dictProxy._dt == _dt;
	}

	public override int GetHashCode()
	{
		return ~_dt.GetHashCode();
	}

	bool IDictionary.Contains(object key)
	{
		return has_key(DefaultContext.Default, key);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return DictionaryOps.iterkeys(_dt.GetMemberDictionary(DefaultContext.Default, excludeDict: false));
	}

	[PythonHidden]
	public void Add(object key, object value)
	{
		this[key] = value;
	}

	[PythonHidden]
	public void Clear()
	{
		throw new InvalidOperationException("dictproxy is read-only");
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new PythonDictionary.DictEnumerator(_dt.GetMemberDictionary(DefaultContext.Default, excludeDict: false).GetEnumerator());
	}

	void IDictionary.Remove(object key)
	{
		throw new InvalidOperationException("dictproxy is read-only");
	}

	void ICollection.CopyTo(Array array, int index)
	{
		foreach (DictionaryEntry item in (IDictionary)this)
		{
			array.SetValue(item, index++);
		}
	}

	bool IDictionary<object, object>.ContainsKey(object key)
	{
		return has_key(DefaultContext.Default, key);
	}

	bool IDictionary<object, object>.Remove(object key)
	{
		throw new InvalidOperationException("dictproxy is read-only");
	}

	bool IDictionary<object, object>.TryGetValue(object key, out object value)
	{
		return TryGetValue(DefaultContext.Default, key, out value);
	}

	void ICollection<KeyValuePair<object, object>>.Add(KeyValuePair<object, object> item)
	{
		this[item.Key] = item.Value;
	}

	bool ICollection<KeyValuePair<object, object>>.Contains(KeyValuePair<object, object> item)
	{
		return has_key(DefaultContext.Default, item.Key);
	}

	void ICollection<KeyValuePair<object, object>>.CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
	{
		foreach (KeyValuePair<object, object> item in (IEnumerable<KeyValuePair<object, object>>)this)
		{
			array.SetValue(item, arrayIndex++);
		}
	}

	bool ICollection<KeyValuePair<object, object>>.Remove(KeyValuePair<object, object> item)
	{
		return ((IDictionary<object, object>)this).Remove(item.Key);
	}

	IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator()
	{
		return _dt.GetMemberDictionary(DefaultContext.Default, excludeDict: false).GetEnumerator();
	}

	private object GetIndex(CodeContext context, object index)
	{
		if (index is string name && _dt.TryLookupSlot(context, name, out var slot))
		{
			if (slot is PythonTypeUserDescriptorSlot pythonTypeUserDescriptorSlot)
			{
				return pythonTypeUserDescriptorSlot.Value;
			}
			return slot;
		}
		throw PythonOps.KeyError(index.ToString());
	}

	private bool TryGetValue(CodeContext context, object key, out object value)
	{
		if (key is string name && _dt.TryLookupSlot(context, name, out var slot))
		{
			if (slot is PythonTypeUserDescriptorSlot pythonTypeUserDescriptorSlot)
			{
				value = pythonTypeUserDescriptorSlot.Value;
				return true;
			}
			value = slot;
			return true;
		}
		value = null;
		return false;
	}
}

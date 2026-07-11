using System;
using System.Collections.Generic;

namespace IronPython.Runtime;

[Serializable]
internal class DebuggerDictionaryStorage : DictionaryStorage
{
	private IDictionary<object, object> _data;

	private readonly CommonDictionaryStorage _hidden;

	public override int Count => _data.Count - _hidden.Count;

	public DebuggerDictionaryStorage(IDictionary<object, object> data)
	{
		_hidden = new CommonDictionaryStorage();
		foreach (object key in data.Keys)
		{
			if (key is string { Length: >0 } text && text[0] == '$')
			{
				_hidden.Add(text, null);
			}
		}
		_data = data;
	}

	public override void Add(ref DictionaryStorage storage, object key, object value)
	{
		AddNoLock(ref storage, key, value);
	}

	public override void AddNoLock(ref DictionaryStorage storage, object key, object value)
	{
		_hidden.Remove(key);
		_data[key] = value;
	}

	public override bool Contains(object key)
	{
		if (_hidden.Contains(key))
		{
			return false;
		}
		return _data.ContainsKey(key);
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		if (_hidden.Contains(key))
		{
			return false;
		}
		return _data.Remove(key);
	}

	public override bool TryGetValue(object key, out object value)
	{
		if (_hidden.Contains(key))
		{
			value = null;
			return false;
		}
		return _data.TryGetValue(key, out value);
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		_data = new Dictionary<object, object>();
		_hidden.Clear();
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		List<KeyValuePair<object, object>> list = new List<KeyValuePair<object, object>>(Count);
		foreach (KeyValuePair<object, object> datum in _data)
		{
			if (!_hidden.Contains(datum.Key))
			{
				list.Add(datum);
			}
		}
		return list;
	}

	public override bool HasNonStringAttributes()
	{
		return true;
	}
}

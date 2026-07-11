using System;
using System.Collections.Generic;

namespace IronPython.Runtime;

[Serializable]
internal class StringDictionaryStorage : DictionaryStorage
{
	private Dictionary<string, object> _data;

	public override int Count
	{
		get
		{
			if (_data == null)
			{
				return 0;
			}
			lock (this)
			{
				if (_data == null)
				{
					return 0;
				}
				int num = _data.Count;
				Dictionary<object, object> dictionary = TryGetObjectDictionary();
				if (dictionary != null)
				{
					num += dictionary.Count - 1;
				}
				return num;
			}
		}
	}

	public StringDictionaryStorage()
	{
	}

	public StringDictionaryStorage(int count)
	{
		_data = new Dictionary<string, object>(count, StringComparer.Ordinal);
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

	public override void AddNoLock(ref DictionaryStorage storage, object key, object value)
	{
		AddNoLock(key, value);
	}

	public void AddNoLock(object key, object value)
	{
		EnsureData();
		if (key is string key2)
		{
			_data[key2] = value;
		}
		else
		{
			GetObjectDictionary()[key] = value;
		}
	}

	public override bool Contains(object key)
	{
		if (_data == null)
		{
			return false;
		}
		lock (this)
		{
			if (key is string key2)
			{
				return _data.ContainsKey(key2);
			}
			return TryGetObjectDictionary()?.ContainsKey(key) ?? false;
		}
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		return Remove(key);
	}

	public bool Remove(object key)
	{
		if (_data == null)
		{
			return false;
		}
		lock (this)
		{
			if (key is string key2)
			{
				return _data.Remove(key2);
			}
			return TryGetObjectDictionary()?.Remove(key) ?? false;
		}
	}

	public override bool TryGetValue(object key, out object value)
	{
		if (_data != null)
		{
			lock (this)
			{
				if (key is string key2)
				{
					return _data.TryGetValue(key2, out value);
				}
				Dictionary<object, object> dictionary = TryGetObjectDictionary();
				if (dictionary != null)
				{
					return dictionary.TryGetValue(key, out value);
				}
			}
		}
		value = null;
		return false;
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		_data = null;
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		List<KeyValuePair<object, object>> list = new List<KeyValuePair<object, object>>();
		if (_data != null)
		{
			lock (this)
			{
				foreach (KeyValuePair<string, object> datum in _data)
				{
					if (!string.IsNullOrEmpty(datum.Key))
					{
						list.Add(new KeyValuePair<object, object>(datum.Key, datum.Value));
					}
				}
				Dictionary<object, object> dictionary = TryGetObjectDictionary();
				if (dictionary != null)
				{
					foreach (KeyValuePair<object, object> item in GetObjectDictionary())
					{
						list.Add(item);
					}
				}
			}
		}
		return list;
	}

	public override bool HasNonStringAttributes()
	{
		if (_data != null)
		{
			lock (this)
			{
				if (TryGetObjectDictionary() != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	private Dictionary<object, object> TryGetObjectDictionary()
	{
		if (_data != null && _data.TryGetValue(string.Empty, out var value))
		{
			return (Dictionary<object, object>)value;
		}
		return null;
	}

	private Dictionary<object, object> GetObjectDictionary()
	{
		lock (this)
		{
			EnsureData();
			if (_data.TryGetValue(string.Empty, out var value))
			{
				return (Dictionary<object, object>)value;
			}
			Dictionary<object, object> dictionary = new Dictionary<object, object>();
			_data[string.Empty] = dictionary;
			return dictionary;
		}
	}

	private void EnsureData()
	{
		if (_data == null)
		{
			_data = new Dictionary<string, object>();
		}
	}
}

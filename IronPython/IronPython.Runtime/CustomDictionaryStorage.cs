using System.Collections.Generic;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal abstract class CustomDictionaryStorage : DictionaryStorage
{
	private readonly CommonDictionaryStorage _storage = new CommonDictionaryStorage();

	public override int Count => GetItems().Count;

	public override void Add(ref DictionaryStorage storage, object key, object value)
	{
		Add(key, value);
	}

	public override void AddNoLock(ref DictionaryStorage storage, object key, object value)
	{
		if (!(key is string) || !TrySetExtraValue((string)key, value))
		{
			_storage.AddNoLock(ref storage, key, value);
		}
	}

	public void Add(object key, object value)
	{
		if (!(key is string) || !TrySetExtraValue((string)key, value))
		{
			_storage.Add(key, value);
		}
	}

	public override bool Contains(object key)
	{
		if (key is string && TryGetExtraValue((string)key, out var value))
		{
			return value != Uninitialized.Instance;
		}
		return _storage.Contains(key);
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		return Remove(key);
	}

	public bool Remove(object key)
	{
		if (key is string)
		{
			return TryRemoveExtraValue((string)key) ?? _storage.Remove(key);
		}
		return _storage.Remove(key);
	}

	public override bool TryGetValue(object key, out object value)
	{
		if (key is string && TryGetExtraValue((string)key, out value))
		{
			return value != Uninitialized.Instance;
		}
		return _storage.TryGetValue(key, out value);
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		_storage.Clear(ref storage);
		foreach (KeyValuePair<string, object> extraItem in GetExtraItems())
		{
			TryRemoveExtraValue(extraItem.Key);
		}
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		List<KeyValuePair<object, object>> items = _storage.GetItems();
		foreach (KeyValuePair<string, object> extraItem in GetExtraItems())
		{
			items.Add(new KeyValuePair<object, object>(extraItem.Key, extraItem.Value));
		}
		return items;
	}

	protected abstract IEnumerable<KeyValuePair<string, object>> GetExtraItems();

	protected abstract bool TrySetExtraValue(string key, object value);

	protected abstract bool TryGetExtraValue(string key, out object value);

	protected abstract bool? TryRemoveExtraValue(string key);
}

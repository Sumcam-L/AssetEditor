using System;
using System.Collections.Generic;

namespace IronPython.Runtime;

[Serializable]
internal class EmptyDictionaryStorage : DictionaryStorage
{
	public static EmptyDictionaryStorage Instance = new EmptyDictionaryStorage();

	public override int Count => 0;

	private EmptyDictionaryStorage()
	{
	}

	public override void Add(ref DictionaryStorage storage, object key, object value)
	{
		lock (this)
		{
			if (storage == this)
			{
				CommonDictionaryStorage commonDictionaryStorage = new CommonDictionaryStorage();
				commonDictionaryStorage.AddNoLock(key, value);
				storage = commonDictionaryStorage;
				return;
			}
		}
		storage.Add(ref storage, key, value);
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		return false;
	}

	public override void Clear(ref DictionaryStorage storage)
	{
	}

	public override bool Contains(object key)
	{
		return false;
	}

	public override bool TryGetValue(object key, out object value)
	{
		value = null;
		return false;
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		return new List<KeyValuePair<object, object>>();
	}

	public override DictionaryStorage Clone()
	{
		return this;
	}

	public override bool HasNonStringAttributes()
	{
		return false;
	}
}

using System;
using System.Collections.Generic;

namespace IronPython.Runtime;

[Serializable]
internal abstract class DictionaryStorage
{
	public abstract int Count { get; }

	public abstract void Add(ref DictionaryStorage storage, object key, object value);

	public virtual void AddNoLock(ref DictionaryStorage storage, object key, object value)
	{
		Add(ref storage, key, value);
	}

	public abstract bool Remove(ref DictionaryStorage storage, object key);

	public virtual bool TryRemoveValue(ref DictionaryStorage storage, object key, out object value)
	{
		if (TryGetValue(key, out value))
		{
			return Remove(ref storage, key);
		}
		return false;
	}

	public abstract void Clear(ref DictionaryStorage storage);

	public virtual void CopyTo(ref DictionaryStorage into)
	{
		foreach (KeyValuePair<object, object> item in GetItems())
		{
			into.Add(ref into, item.Key, item.Value);
		}
	}

	public abstract bool Contains(object key);

	public abstract bool TryGetValue(object key, out object value);

	public virtual bool HasNonStringAttributes()
	{
		foreach (KeyValuePair<object, object> item in GetItems())
		{
			if (!(item.Key is string))
			{
				return true;
			}
		}
		return false;
	}

	public abstract List<KeyValuePair<object, object>> GetItems();

	public virtual IEnumerable<object> GetKeys()
	{
		foreach (KeyValuePair<object, object> o in GetItems())
		{
			KeyValuePair<object, object> keyValuePair = o;
			yield return keyValuePair.Key;
		}
	}

	public virtual DictionaryStorage Clone()
	{
		CommonDictionaryStorage commonDictionaryStorage = new CommonDictionaryStorage();
		foreach (KeyValuePair<object, object> item in GetItems())
		{
			commonDictionaryStorage.Add(item.Key, item.Value);
		}
		return commonDictionaryStorage;
	}

	public virtual void EnsureCapacityNoLock(int size)
	{
	}

	public virtual IEnumerator<KeyValuePair<object, object>> GetEnumerator()
	{
		return GetItems().GetEnumerator();
	}

	public virtual bool TryGetPath(out object value)
	{
		return TryGetValue("__path__", out value);
	}

	public virtual bool TryGetPackage(out object value)
	{
		return TryGetValue("__package__", out value);
	}

	public virtual bool TryGetBuiltins(out object value)
	{
		return TryGetValue("__builtins__", out value);
	}

	public virtual bool TryGetName(out object value)
	{
		return TryGetValue("__name__", out value);
	}

	public virtual bool TryGetImport(out object value)
	{
		return TryGetValue("__import__", out value);
	}
}

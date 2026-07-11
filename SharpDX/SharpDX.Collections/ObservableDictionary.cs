using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpDX.Collections;

public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	private readonly Dictionary<TKey, TValue> dictionary;

	public ICollection<TKey> Keys => dictionary.Keys;

	public ICollection<TValue> Values => dictionary.Values;

	public int Count => dictionary.Count;

	public TValue this[TKey key]
	{
		get
		{
			return dictionary[key];
		}
		set
		{
			TValue value2;
			bool flag = dictionary.TryGetValue(key, out value2);
			dictionary[key] = value;
			if (flag)
			{
				OnItemRemoved(new ObservableDictionaryEventArgs<TKey, TValue>(key, value2));
			}
			OnItemAdded(new ObservableDictionaryEventArgs<TKey, TValue>(key, value));
		}
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).IsReadOnly;

	public event EventHandler<ObservableDictionaryEventArgs<TKey, TValue>> ItemAdded;

	public event EventHandler<ObservableDictionaryEventArgs<TKey, TValue>> ItemRemoved;

	public ObservableDictionary()
		: this(0, (IEqualityComparer<TKey>)null)
	{
	}

	public ObservableDictionary(int capacity)
		: this(capacity, (IEqualityComparer<TKey>)null)
	{
	}

	public ObservableDictionary(IEqualityComparer<TKey> comparer)
		: this(0, comparer)
	{
	}

	public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
		: this(dictionary, (IEqualityComparer<TKey>)null)
	{
	}

	public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
		: this(dictionary?.Count ?? 0, comparer)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			Add(item.Key, item.Value);
		}
	}

	public ObservableDictionary(int capacity, IEqualityComparer<TKey> comparer)
	{
		dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return dictionary.GetEnumerator();
	}

	public void Clear()
	{
		List<ObservableDictionaryEventArgs<TKey, TValue>> list = new List<ObservableDictionaryEventArgs<TKey, TValue>>();
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			list.Add(new ObservableDictionaryEventArgs<TKey, TValue>(item));
		}
		dictionary.Clear();
		foreach (ObservableDictionaryEventArgs<TKey, TValue> item2 in list)
		{
			OnItemRemoved(item2);
		}
	}

	public void Add(TKey key, TValue value)
	{
		dictionary.Add(key, value);
		OnItemAdded(new ObservableDictionaryEventArgs<TKey, TValue>(key, value));
	}

	public bool ContainsKey(TKey key)
	{
		return dictionary.ContainsKey(key);
	}

	public bool Remove(TKey key)
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			return false;
		}
		dictionary.Remove(key);
		OnItemRemoved(new ObservableDictionaryEventArgs<TKey, TValue>(key, value));
		return true;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return dictionary.TryGetValue(key, out value);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
	{
		((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Add(item);
		OnItemAdded(new ObservableDictionaryEventArgs<TKey, TValue>(item));
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
	{
		bool flag = ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Remove(item);
		if (flag)
		{
			OnItemRemoved(new ObservableDictionaryEventArgs<TKey, TValue>(item));
		}
		return flag;
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
	{
		return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
	}

	protected virtual void OnItemAdded(ObservableDictionaryEventArgs<TKey, TValue> args)
	{
		this.ItemAdded?.Invoke(this, args);
	}

	protected virtual void OnItemRemoved(ObservableDictionaryEventArgs<TKey, TValue> args)
	{
		this.ItemRemoved?.Invoke(this, args);
	}
}

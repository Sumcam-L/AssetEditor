using System.Collections;
using System.Collections.Generic;

namespace IronPython.Runtime;

public class DictionaryGenericWrapper<K, V> : IDictionary<K, V>, ICollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable
{
	private IDictionary<object, object> self;

	public ICollection<K> Keys
	{
		get
		{
			List<K> list = new List<K>();
			foreach (object key in self.Keys)
			{
				list.Add((K)key);
			}
			return list;
		}
	}

	public ICollection<V> Values
	{
		get
		{
			List<V> list = new List<V>();
			foreach (object value in self.Values)
			{
				list.Add((V)value);
			}
			return list;
		}
	}

	public V this[K key]
	{
		get
		{
			return (V)self[key];
		}
		set
		{
			self[key] = value;
		}
	}

	public int Count => self.Count;

	public bool IsReadOnly => self.IsReadOnly;

	public DictionaryGenericWrapper(IDictionary<object, object> self)
	{
		this.self = self;
	}

	public void Add(K key, V value)
	{
		self.Add(key, value);
	}

	public bool ContainsKey(K key)
	{
		return self.ContainsKey(key);
	}

	public bool Remove(K key)
	{
		return self.Remove(key);
	}

	public bool TryGetValue(K key, out V value)
	{
		if (self.TryGetValue(key, out var value2))
		{
			value = (V)value2;
			return true;
		}
		value = default(V);
		return false;
	}

	public void Add(KeyValuePair<K, V> item)
	{
		self.Add(new KeyValuePair<object, object>(item.Key, item.Value));
	}

	public void Clear()
	{
		self.Clear();
	}

	public bool Contains(KeyValuePair<K, V> item)
	{
		return self.Contains(new KeyValuePair<object, object>(item.Key, item.Value));
	}

	public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
	{
		using IEnumerator<KeyValuePair<K, V>> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<K, V> current = enumerator.Current;
			array[arrayIndex++] = current;
		}
	}

	public bool Remove(KeyValuePair<K, V> item)
	{
		return self.Remove(new KeyValuePair<object, object>(item.Key, item.Value));
	}

	public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
	{
		foreach (KeyValuePair<object, object> kv in self)
		{
			KeyValuePair<object, object> keyValuePair = kv;
			K key = (K)keyValuePair.Key;
			KeyValuePair<object, object> keyValuePair2 = kv;
			yield return new KeyValuePair<K, V>(key, (V)keyValuePair2.Value);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return self.GetEnumerator();
	}
}

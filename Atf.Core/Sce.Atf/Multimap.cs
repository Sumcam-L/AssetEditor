using System;
using System.Collections.Generic;

namespace Sce.Atf;

[Serializable]
public class Multimap<Key, Value>
{
	private readonly Dictionary<Key, List<Value>> m_keyValues;

	public IEnumerable<Key> Keys => m_keyValues.Keys;

	public IEnumerable<Value> this[Key key] => Find(key);

	public Multimap()
		: this((IEqualityComparer<Key>)null)
	{
	}

	public Multimap(IEqualityComparer<Key> comparer)
	{
		m_keyValues = new Dictionary<Key, List<Value>>(comparer);
	}

	public bool ContainsKey(Key key)
	{
		return m_keyValues.ContainsKey(key);
	}

	public bool ContainsKeyValue(Key key, Value value)
	{
		if (!m_keyValues.TryGetValue(key, out var value2))
		{
			return false;
		}
		return value2.Contains(value);
	}

	public IEnumerable<Value> Find(Key key)
	{
		if (!m_keyValues.TryGetValue(key, out var value))
		{
			return EmptyEnumerable<Value>.Instance;
		}
		return value;
	}

	public Value FindFirst(Key key)
	{
		if (!m_keyValues.TryGetValue(key, out var value))
		{
			return default(Value);
		}
		return value[0];
	}

	public Value FindLast(Key key)
	{
		if (!m_keyValues.TryGetValue(key, out var value))
		{
			return default(Value);
		}
		return value[value.Count - 1];
	}

	public bool TryGetFirst(Key key, out Value result)
	{
		if (!m_keyValues.TryGetValue(key, out var value))
		{
			result = default(Value);
			return false;
		}
		result = value[0];
		return true;
	}

	public bool TryGetLast(Key key, out Value result)
	{
		if (!m_keyValues.TryGetValue(key, out var value))
		{
			result = default(Value);
			return false;
		}
		result = value[value.Count - 1];
		return true;
	}

	public void Add(Key key, Value value)
	{
		if (!m_keyValues.TryGetValue(key, out var value2))
		{
			List<Value> list = (m_keyValues[key] = new List<Value>());
			value2 = list;
		}
		else
		{
			value2.Remove(value);
		}
		value2.Add(value);
	}

	public void AddFirst(Key key, Value value)
	{
		if (!m_keyValues.TryGetValue(key, out var value2))
		{
			List<Value> list = (m_keyValues[key] = new List<Value>());
			value2 = list;
		}
		else
		{
			value2.Remove(value);
		}
		value2.Insert(0, value);
	}

	public bool Remove(Key key)
	{
		return m_keyValues.Remove(key);
	}

	public bool Remove(Key key, Value value)
	{
		if (m_keyValues.TryGetValue(key, out var value2) && value2.Remove(value))
		{
			if (value2.Count == 0)
			{
				m_keyValues.Remove(key);
			}
			return true;
		}
		return false;
	}

	public void Clear()
	{
		m_keyValues.Clear();
	}
}

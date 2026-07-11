using System;
using System.Collections.Generic;

namespace Firaxis.Collections;

public class UniqueMultiDictionary<TKey, TValue> : MultiDictionary<TKey, TValue>
{
	private HashSet<TValue> m_CollectedValues = new HashSet<TValue>();

	public override void Add(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (!m_CollectedValues.Add(value))
		{
			Clear();
			m_CollectedValues.Clear();
			throw new ArgumentException(string.Concat("Cannot add ", value, " because it is already present in the dictionary."));
		}
		base.Add(key, value);
	}

	public override void AddRange(TKey key, HashSet<TValue> newValues)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		foreach (TValue newValue in newValues)
		{
			if (m_CollectedValues.Contains(newValue))
			{
				Clear();
				m_CollectedValues.Clear();
				throw new ArgumentException(string.Concat("Cannot add ", newValue, " because it is already present in the dictionary."));
			}
			Add(key, newValue);
		}
	}

	public new void Clear()
	{
		m_CollectedValues.Clear();
		base.Clear();
	}

	public override bool Contains(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		bool flag = m_CollectedValues.Contains(value);
		bool flag2 = base.Contains(key, value);
		if (flag != flag2)
		{
			Clear();
			m_CollectedValues.Clear();
			throw new ArgumentException("The collection of values and the dictionary are out of sync.");
		}
		return flag2;
	}

	public override bool ContainsValue(TValue value)
	{
		bool flag = m_CollectedValues.Contains(value);
		bool flag2 = base.ContainsValue(value);
		if (flag != flag2)
		{
			Clear();
			m_CollectedValues.Clear();
			throw new ArgumentException("The collection of values and the dictionary are out of sync.");
		}
		return flag2;
	}

	public override void Remove(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		m_CollectedValues.Remove(value);
		base.Remove(value);
	}

	public override void Remove(TValue value)
	{
		m_CollectedValues.Remove(value);
		base.Remove(value);
	}

	public new bool Remove(TKey key)
	{
		if (TryGetValue(key, out var value))
		{
			foreach (TValue item in value)
			{
				m_CollectedValues.Remove(item);
			}
		}
		return base.Remove(key);
	}

	public override IEnumerable<TValue> GetValues()
	{
		HashSet<TValue> hashSet = new HashSet<TValue>();
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				foreach (TValue item in enumerator.Current.Value)
				{
					if (!hashSet.Add(item))
					{
						Clear();
						m_CollectedValues.Clear();
						throw new ArgumentException(string.Concat("A duplicate of ", item, " was contained in one or more keys."));
					}
				}
			}
		}
		return hashSet;
	}

	public override string ToString()
	{
		string text = "";
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, HashSet<TValue>> current = enumerator.Current;
				text = text + "Key: " + current.Key.ToString() + " Values: ";
				foreach (TValue item in current.Value)
				{
					text = text + item.ToString() + ", ";
				}
				text += "\n";
			}
		}
		return text;
	}
}
